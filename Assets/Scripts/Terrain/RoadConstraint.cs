using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TriangleNet.Geometry;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class RoadConstraint : MonoBehaviour
{
    [SerializeField]
    GameObject road_architect;
    Road[] roads = new Road[1024];
    int roads_count;
    bool is_initial = false;
    [SerializeField]
    bool do_constraint = true;
    bool is_finished = false;
    Terrain terrain;
    // Start is called before the first frame update
    void Start()
    {
        if (do_constraint && road_architect == null)
        {
            Debug.LogWarning("Warning! road_architect not be loaded!");
        }
        else
        {
            roads_count = 0;
            for (int child_index = 0; child_index < road_architect.transform.childCount; child_index++)
            {
                if (road_architect.transform.GetChild(child_index).gameObject.TryGetComponent<Road>(out roads[roads_count]))
                {
                    roads_count++;
                }
            }
            Debug.Log($"{roads_count} of Road.cs are loaded");

            is_initial = true;
        }

        terrain = GetComponent<Terrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (is_initial && do_constraint && !is_finished)
        {
            is_finished = true;

            for (int road_index = 0; road_index < roads_count; road_index++)
            {
                doRoadConstaint(roads[road_index], road_index == 1 ? true : false);
            }
        }
    }

    void doRoadConstaint(Road road, bool debug = false)
    {
        bool is_bridge = false;
        for (int control_id = 0; control_id < road.spline.nodes.Count - 1; control_id++)
        {
            int x_base = int.MaxValue;
            int y_base = int.MaxValue;
            int x_boundary = int.MinValue;
            int y_boundary = int.MinValue;
            int segment_max = 10;
            float step = 1.0f / (segment_max - 1);
            Vector3[] road_polygon = new Vector3[segment_max * 2];
            Vector2[] road_polygon2d = new Vector2[segment_max * 2];
            if (road.spline.nodes[control_id].isBridgeStart || (is_bridge && !road.spline.nodes[control_id].isBridgeEnd))
            {
                is_bridge = true;
                continue;
            }
            else
            {
                is_bridge = false;
            }

            for (int segment_i = 0; segment_i < segment_max; segment_i++)
            {
                Vector3 road_point_pos, road_tangent;
                road.getPosAndTangent(control_id, step * segment_i, out road_point_pos, out road_tangent);

                // right
                road_polygon[segment_i] = road_point_pos + new Vector3(road.laneWidth * road_tangent.normalized.z, 0, road.laneWidth * -road_tangent.normalized.x);
                var terrain_data_pos = Utils.getTerrainDataPos(terrain, road_polygon[segment_i]);
                road_polygon2d[segment_i] = new Vector2(terrain_data_pos.x, terrain_data_pos.y);
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

                // left
                road_polygon[2 * segment_max - segment_i - 1] = road_point_pos - new Vector3(road.laneWidth * road_tangent.normalized.z, 0, road.laneWidth * -road_tangent.normalized.x);
                terrain_data_pos = Utils.getTerrainDataPos(terrain, road_polygon[2 * segment_max - segment_i - 1]);
                road_polygon2d[2 * segment_max - segment_i - 1] = new Vector2(terrain_data_pos.x, terrain_data_pos.y);
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
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = road_point_pos;

                    GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube2.transform.position = road_polygon[segment_i];
                    cube2.GetComponent<MeshRenderer>().material.color = UnityEngine.Color.blue;
                    cube2.name = segment_i.ToString();

                    GameObject cube3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube3.transform.position = road_polygon[2 * segment_max - segment_i - 1];
                    cube3.GetComponent<MeshRenderer>().material.color = UnityEngine.Color.blue;
                    cube3.name = (2 * segment_max - segment_i - 1).ToString();
                }
            }

            int height_x_size = x_boundary - x_base + 1;
            int height_y_size = y_boundary - y_base + 1;
            float[,] constraint_kernel = terrain.terrainData.GetHeights(x_base, y_base, height_x_size, height_y_size);

            // display points of the polygon
            if (debug)
            {
                for (int point_index = 0; point_index < road_polygon2d.Length - 1; point_index++)
                {
                    constraint_kernel[Mathf.RoundToInt(road_polygon2d[point_index].y - y_base), Mathf.RoundToInt(road_polygon2d[point_index].x - x_base)] = 0;
                }
            }

            for (int i = 0; i < height_x_size; i++)
            {
                for (int j = 0; j < height_y_size; j++)
                {
                    if (Utils.pointInPolygon(road_polygon2d, new Vector2(x_base + i, y_base + j)))
                    {
                        constraint_kernel[j, i] = 0.0f;
                    }
                }
            }
            terrain.terrainData.SetHeights(x_base, y_base, constraint_kernel);
        }
    }
}