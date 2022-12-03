using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Search;
#endif
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

[ExecuteInEditMode]
public class InterpolateMatrix : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    GameObject road_architect;
    [SerializeField]
    bool get_road_constraint = false;
    [SerializeField]
    bool do_constraint = true;
    [SerializeField]
    int u_devide = 10;
    [SerializeField]
    int v_devide = 10;
    [SerializeField]
    Terrain terrain;
    Road road;
    Vector3[] road_polygon;
    Vector2[] road_polygon2d;
    Vector3[] road_rights;
    float extend_width = 3;
    // Update is called once per frame
    void Update()
    {
        if (get_road_constraint)
        {
            get_road_constraint = false;
            if (road_architect.transform.GetChild(2).gameObject.TryGetComponent<Road>(out road))
            {
                float step = 1.0f / (v_devide - 1);
                road_polygon = new Vector3[v_devide * 2];
                road_rights = new Vector3[v_devide];
                GameObject cube_manager = new GameObject("Cube Manager");
                for (int segment_v = 0; segment_v < v_devide; segment_v++)
                {
                    Vector3 road_point_pos, road_tangent;
                    road.getPosAndTangent(16, step * segment_v, out road_point_pos, out road_tangent);
                    road_rights[segment_v] = new Vector3((road.laneWidth + road.shoulderWidth) * road_tangent.normalized.z, 0, (road.laneWidth + road.shoulderWidth) * -road_tangent.normalized.x);

                    // right
                    road_polygon[segment_v] = road_point_pos + road_rights[segment_v];

                    // right right
                    road_polygon[2 * v_devide - segment_v - 1] = road_point_pos + (1 + extend_width) * road_rights[segment_v];

                    //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //cube.transform.position = road_point_pos;
                    //cube.transform.parent = cube_manager.transform;

                    GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube2.transform.position = road_polygon[segment_v];
                    var material2 = new Material(cube2.GetComponent<Renderer>().sharedMaterial);
                    material2.color = Color.blue;
                    cube2.GetComponent<Renderer>().sharedMaterial = material2;
                    cube2.name = segment_v.ToString();
                    cube2.transform.parent = cube_manager.transform;

                    GameObject cube3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube3.transform.position = road_polygon[2 * v_devide - segment_v - 1];
                    var material3 = new Material(cube2.GetComponent<Renderer>().sharedMaterial);
                    material3.color = Color.green;
                    cube3.GetComponent<Renderer>().sharedMaterial = material3;
                    cube3.name = (2 * v_devide - segment_v - 1).ToString();
                    cube3.transform.parent = cube_manager.transform;
                }
            }
            else
            {
                Debug.LogError("Road.cs not be loaded");
            }
        }

        if (do_constraint)
        {
            do_constraint = false;

            //Matrix<double> A = DenseMatrix.OfArray(new double[,] {
            //    {1,1,1,1},
            //    {1,2,3,4},
            //    {4,3,2,1}});
            //Vector<double>[] nullspace = A.Kernel();

            //// verify: the following should be approximately (0,0,0)
            //Debug.Log((A * (2 * nullspace[0] - 3 * nullspace[1])).ToString());

            int x_base = int.MaxValue;
            int y_base = int.MaxValue;
            int x_boundary = int.MinValue;
            int y_boundary = int.MinValue;
            float step = 1.0f / (u_devide - 1);
            GameObject cube_manager = new GameObject("Cube inner Manager");
            Vector2[] inner_points = new Vector2[u_devide * v_devide];
            float[] inner_heights = new float[u_devide * v_devide];
            int inner_points_index = 0;
            for (int segment_v = 0; segment_v < v_devide; segment_v++)
            {
                for (int segment_u = 0; segment_u < u_devide; segment_u++)
                {
                    Vector3 road_extend = road_polygon[segment_v] + road_rights[segment_v] * extend_width * step * segment_u;

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = road_extend;
                    var material = new Material(cube.GetComponent<Renderer>().sharedMaterial);
                    cube.name = $"{segment_v} {segment_u}";
                    material.color = Color.gray;
                    cube.GetComponent<Renderer>().sharedMaterial = material;
                    cube.transform.parent = cube_manager.transform;

                    float terrain_height = terrain.SampleHeight(road_extend) + terrain.transform.position.y;
                    var terrain_data_pos = Utils.getTerrainDataPos(terrain, road_extend);
                    if (terrain_data_pos.x < x_base)
                    {
                        x_base = terrain_data_pos.x;
                    }
                    if (terrain_data_pos.x > x_boundary)
                    {
                        x_boundary = terrain_data_pos.x;
                    }
                    if (terrain_data_pos.y < y_base)
                    {
                        y_base = terrain_data_pos.y;
                    }
                    if (terrain_data_pos.y > y_boundary)
                    {
                        y_boundary = terrain_data_pos.y;
                    }
                    float blend_height = Utils.getSFunction(road_extend.y, terrain_height, step * segment_u);
                    inner_points[inner_points_index] = new Vector2(road_extend.x, road_extend.z);
                    inner_heights[inner_points_index] = blend_height;
                    cube.transform.position = new Vector3(inner_points[inner_points_index].x, inner_heights[inner_points_index], inner_points[inner_points_index].y);
                    inner_heights[inner_points_index] -= terrain.transform.position.y; // terrainData exclude position.y
                    inner_points_index++;
                }
            }

            int height_x_size = x_boundary - x_base + 1;
            int height_y_size = y_boundary - y_base + 1;
            float[,] constraint_kernel = terrain.terrainData.GetHeights(x_base, y_base, height_x_size, height_y_size);
            road_polygon2d = Utils.toVec2(road_polygon);
            for (int i = 0; i < height_x_size; i++)
            {
                for (int j = 0; j < height_y_size; j++)
                {
                    Vector2 pos2d = Utils.getWorldPos(terrain, x_base + i, y_base + j);
                    if (Utils.pointInPolygon(road_polygon2d, pos2d))
                    {
                        var nearest_point = Utils.getNearest(inner_points, pos2d);
                        constraint_kernel[j, i] = inner_heights[nearest_point.index] / terrain.terrainData.size.y;
                    }
                }
            }
            terrain.terrainData.SetHeights(x_base, y_base, constraint_kernel);

            //Vector2 coord = Utils.getWorldPos(terrain, x_base, y_base);
            //float h = terrain.SampleHeight(new Vector3(coord.x, 0, coord.y));
            //Vector3 pos = new Vector3(coord.x, h + terrain.transform.position.y, coord.y);
            //GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //c.transform.position = pos;
            //var material_c = new Material(c.GetComponent<Renderer>().sharedMaterial);
            //material_c.color = Color.cyan;
            //c.GetComponent<Renderer>().sharedMaterial = material_c;
            //c.transform.parent = cube_manager.transform;
        }
    }
#endif
}