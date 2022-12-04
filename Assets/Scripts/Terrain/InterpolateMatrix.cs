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
    bool do_segment_constraint = true;
    [SerializeField]
    bool do_constraint = false;
    [SerializeField]
    int u_devide = 10;
    [SerializeField]
    int v_devide = 10;
    [SerializeField]
    float extend_width = 10.0f;
    [SerializeField]
    bool debug = false;
    [SerializeField]
    Terrain terrain;
    [SerializeField]
    Terrain[] terrains;
    Road road;
    Vector3[] road_polygon;
    Vector3[] road_right_polygon;
    Vector3[] road_left_polygon;
    Vector2[] road_right_polygon2d;
    Vector2[] road_polygon2d;
    Vector2[] road_left_polygon2d;
    Vector2[] road_spline2d;
    Vector2[] road_right_spline2d;
    Vector2[] road_left_spline2d;
    Vector3[] road_rights;
    Vector3[] road_spline_points;
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
                road_right_polygon = new Vector3[v_devide * 2];
                road_left_polygon = new Vector3[v_devide * 2];
                road_rights = new Vector3[v_devide];
                road_spline2d = new Vector2[v_devide];
                road_right_spline2d = new Vector2[v_devide];
                road_left_spline2d = new Vector2[v_devide];
                GameObject cube_manager = new GameObject("Cube Manager");
                for (int segment_v = 0; segment_v < v_devide; segment_v++)
                {
                    Vector3 road_point_pos, road_tangent;
                    road.getPosAndTangent(16, step * segment_v, out road_point_pos, out road_tangent);
                    road_rights[segment_v] = new Vector3(road_tangent.normalized.z, 0, -road_tangent.normalized.x);

                    // right
                    road_right_polygon[segment_v] = road_point_pos + road_rights[segment_v] * (road.laneWidth + road.shoulderWidth);

                    // right right
                    road_right_polygon[2 * v_devide - segment_v - 1] = road_point_pos + (road.laneWidth + road.shoulderWidth + extend_width) * road_rights[segment_v];

                    // left
                    road_left_polygon[segment_v] = road_point_pos - road_rights[segment_v] * (road.laneWidth + road.shoulderWidth);

                    // left left
                    road_left_polygon[2 * v_devide - segment_v - 1] = road_point_pos - (road.laneWidth + road.shoulderWidth + extend_width) * road_rights[segment_v];

                    // road
                    road_polygon[segment_v] = road_right_polygon[segment_v];
                    road_polygon[2 * v_devide - segment_v - 1] = road_left_polygon[segment_v];

                    if (debug)
                    {
                        GameObject cube_right = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube_right.transform.position = road_right_polygon[segment_v];
                        var material_right = new Material(cube_right.GetComponent<Renderer>().sharedMaterial);
                        material_right.color = Color.blue;
                        cube_right.GetComponent<Renderer>().sharedMaterial = material_right;
                        cube_right.name = segment_v.ToString();
                        cube_right.transform.parent = cube_manager.transform;

                        GameObject cube_right_right = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube_right_right.transform.position = road_right_polygon[2 * v_devide - segment_v - 1];
                        var material_right_right = new Material(cube_right.GetComponent<Renderer>().sharedMaterial);
                        material_right_right.color = Color.green;
                        cube_right_right.GetComponent<Renderer>().sharedMaterial = material_right_right;
                        cube_right_right.name = (2 * v_devide - segment_v - 1).ToString();
                        cube_right_right.transform.parent = cube_manager.transform;

                        GameObject cube_left = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube_left.transform.position = road_left_polygon[segment_v];
                        var material_left = new Material(cube_left.GetComponent<Renderer>().sharedMaterial);
                        material_left.color = Color.blue;
                        cube_left.GetComponent<Renderer>().sharedMaterial = material_left;
                        cube_left.name = segment_v.ToString();
                        cube_left.transform.parent = cube_manager.transform;

                        GameObject cube_left_left = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube_left_left.transform.position = road_left_polygon[2 * v_devide - segment_v - 1];
                        var material_left_left = new Material(cube_left.GetComponent<Renderer>().sharedMaterial);
                        material_left_left.color = Color.green;
                        cube_left_left.GetComponent<Renderer>().sharedMaterial = material_left_left;
                        cube_left_left.name = (2 * v_devide - segment_v - 1).ToString();
                        cube_left_left.transform.parent = cube_manager.transform;
                    }
                }
                if (!debug)
                    DestroyImmediate(cube_manager);
            }
            else
            {
                Debug.LogError("Road.cs not be loaded");
            }
        }

        if (do_segment_constraint)
        {
            do_segment_constraint = false;

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
            //GameObject cube_manager = new GameObject("Cube inner Manager");
            Vector2[] inner_points = new Vector2[u_devide * v_devide];
            float[] inner_heights = new float[u_devide * v_devide];
            int inner_points_index = 0;
            for (int segment_v = 0; segment_v < v_devide; segment_v++)
            {
                for (int segment_u = 0; segment_u < u_devide; segment_u++)
                {
                    Vector3 road_right_extend = road_right_polygon[segment_v] + road_rights[segment_v] * extend_width * step * segment_u;
                    //float terrain_height = terrain.SampleHeight(road_right_extend) + terrain.transform.position.y;
                    var terrain_data_pos = Utils.getTerrainDataPos(terrain, road_right_extend);
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
                    //float blend_height = Utils.getSFunction(road_right_extend.y, terrain_height, step * segment_u);
                    //inner_points[inner_points_index] = new Vector2(road_right_extend.x, road_right_extend.z);
                    //inner_heights[inner_points_index] = blend_height;
                    //inner_heights[inner_points_index] -= terrain.transform.position.y; // terrainData exclude position.y
                    //inner_points_index++;

                    Vector3 road_left_extend = road_left_polygon[segment_v] - road_rights[segment_v] * extend_width * step * segment_u;
                    //terrain_height = terrain.SampleHeight(road_left_extend) + terrain.transform.position.y;
                    terrain_data_pos = Utils.getTerrainDataPos(terrain, road_left_extend);
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

                    if (debug)
                    {
                        //GameObject cube_right = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //cube_right.transform.position = new Vector3(inner_points[inner_points_index].x, inner_heights[inner_points_index], inner_points[inner_points_index].y);
                        //var material = new Material(cube_right.GetComponent<Renderer>().sharedMaterial);
                        //cube_right.name = $"{segment_v} {segment_u}";
                        //material.color = Color.gray;
                        //cube_right.GetComponent<Renderer>().sharedMaterial = material;
                        //cube_right.transform.parent = cube_manager.transform;

                        //GameObject cube_left = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //cube_left.transform.position = new Vector3(inner_points[inner_points_index].x, inner_heights[inner_points_index], inner_points[inner_points_index].y);
                        //material = new Material(cube_left.GetComponent<Renderer>().sharedMaterial);
                        //cube_left.name = $"{segment_v} {segment_u}";
                        //material.color = Color.gray;
                        //cube_left.GetComponent<Renderer>().sharedMaterial = material;
                        //cube_left.transform.parent = cube_manager.transform;
                    }
                }
            }
            //if (!debug)
            //    DestroyImmediate(cube_manager);

            int height_x_size = x_boundary - x_base + 1;
            int height_y_size = y_boundary - y_base + 1;
            float[,] constraint_kernel = terrain.terrainData.GetHeights(x_base, y_base, height_x_size, height_y_size);
            road_polygon2d = Utils.toVec2(road_polygon);
            road_right_polygon2d = Utils.toVec2(road_right_polygon);
            road_left_polygon2d = Utils.toVec2(road_left_polygon);
            road_spline2d = Utils.toVec2(road_polygon, v_devide);
            road_right_spline2d = Utils.toVec2(road_right_polygon, v_devide);
            road_left_spline2d = Utils.toVec2(road_left_polygon, v_devide);
            for (int i = 0; i < height_x_size; i++)
            {
                for (int j = 0; j < height_y_size; j++)
                {
                    Vector2 pos2d = Utils.getWorldPos(terrain, x_base + i, y_base + j);
                    float terrain_height = terrain.SampleHeight(new Vector3(pos2d.x, 0, pos2d.y)) + terrain.transform.position.y;

                    // right extend
                    if (Utils.pointInPolygon(road_right_polygon2d, pos2d))
                    {
                        var nearest_point = Utils.getNearest(road_right_spline2d, pos2d);
                        float u = Vector2.Distance(road_right_spline2d[nearest_point.index], pos2d) / extend_width;
                        constraint_kernel[j, i] = (Utils.getSFunction(road_right_polygon[nearest_point.index].y, terrain_height, u) - terrain.transform.position.y) / terrain.terrainData.size.y;
                        //constraint_kernel[j, i] = inner_heights[nearest_point.index] / terrain.terrainData.size.y;
                    }

                    // left extend
                    if (Utils.pointInPolygon(road_left_polygon2d, pos2d))
                    {
                        var nearest_point = Utils.getNearest(road_left_spline2d, pos2d);
                        float u = Vector2.Distance(road_left_spline2d[nearest_point.index], pos2d) / extend_width;
                        constraint_kernel[j, i] = (Utils.getSFunction(road_left_polygon[nearest_point.index].y, terrain_height, u) - terrain.transform.position.y) / terrain.terrainData.size.y;
                        //constraint_kernel[j, i] = inner_heights[nearest_point.index] / terrain.terrainData.size.y;
                    }

                    // road constraint
                    if (Utils.pointInPolygon(road_polygon2d, pos2d))
                    {
                        var nearest_point = Utils.getNearest(road_spline2d, pos2d);
                        constraint_kernel[j, i] = (road_polygon[nearest_point.index].y - terrain.transform.position.y - 0.5f) / terrain.terrainData.size.y;
                        //constraint_kernel[j, i] = inner_heights[nearest_point.index] / terrain.terrainData.size.y;
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

        if (do_constraint)
        {
            do_constraint = false;

            doRoadConstraint(2, ref terrains);
        }
    }

    void doRoadConstraint(int road_index, ref Terrain[] terrains)
    {
        if (road_architect.transform.GetChild(2).gameObject.TryGetComponent<Road>(out road))
        {
            for (int terrain_index = 0; terrain_index < terrains.Length; terrain_index++)
            {
                terrain = terrains[terrain_index];

                float[,] constraint_kernel = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);

                Vector3 road_point_pos, road_tangent;
                int node_count = road.spline.nodes.Count - 1;
                float step = 1.0f / v_devide;
                road_spline_points = new Vector3[node_count * v_devide + 1];
                for (int segment_index = 0; segment_index < node_count; segment_index++)
                {
                    for (int segment_v = 0; segment_v < v_devide; segment_v++)
                    {
                        road.getPosAndTangent(segment_index, step * segment_v, out road_point_pos, out road_tangent);
                        road_spline_points[segment_index * v_devide + segment_v] = road_point_pos;
                    }
                }
                road.getPosAndTangent(node_count - 1, 1, out road_point_pos, out road_tangent);
                road_spline_points[node_count * v_devide] = road_point_pos;

                road_spline2d = Utils.toVec2(road_spline_points);
                for (int i = 0; i < terrain.terrainData.heightmapResolution; i++)
                {
                    for (int j = 0; j < terrain.terrainData.heightmapResolution; j++)
                    {
                        Vector2 pos2d = Utils.getWorldPos(terrain, i, j);
                        float terrain_height = terrain.SampleHeight(new Vector3(pos2d.x, 0, pos2d.y)) + terrain.transform.position.y;
                        var nearest_point = Utils.getNearest(road_spline2d, pos2d);
                        if (nearest_point.distance < road.laneWidth + road.shoulderWidth) // in road
                        {
                            constraint_kernel[j, i] = (road_spline_points[nearest_point.index].y - terrain.transform.position.y - 0.5f) / terrain.terrainData.size.y;
                        }
                        else if (nearest_point.distance < road.laneWidth + road.shoulderWidth + extend_width) // in extend
                        {
                            float u = (nearest_point.distance - road.laneWidth - road.shoulderWidth) / extend_width;
                            constraint_kernel[j, i] = (Utils.getSFunction(road_spline_points[nearest_point.index].y, terrain_height, u) - terrain.transform.position.y) / terrain.terrainData.size.y;
                        }
                    }
                }
                terrain.terrainData.SetHeights(0, 0, constraint_kernel);
            }
        }
        else
        {
            Debug.LogError("Road.cs not be loaded");
        }
    }
#endif
}