using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class RoadConstraint : MonoBehaviour
{
    [SerializeField]
    GameObject road_architect;
    Road[] roads = new Road[1024];
    int roads_index;
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
            for (int child_index = 0; child_index < road_architect.transform.childCount; child_index++)
            {
                if (road_architect.transform.GetChild(child_index).gameObject.TryGetComponent<Road>(out roads[roads_index]))
                {
                    roads_index++;
                }
            }
            Debug.Log($"{roads_index} of Road.cs are loaded");

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

            doRoadConstaint(roads[1].spline.nodes.ToArray());
        }
    }

    void doRoadConstaint(SplineN[] points)
    {
        int height_set_size = 1;
        float[,] constraint_kernel = new float[height_set_size, height_set_size];

        for (int control_i = 0; control_i < points.Length - 1; control_i++)
        {
            for (int segment_i = 0; segment_i < 40; segment_i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Vector3 road_point_pos = roads[1].getPos(control_i, 0.025f * segment_i);
                //roads[1].laneWidth
                var terrain_data_pos = Utils.getTerrainDataPos(terrain, road_point_pos);
                float terrain_data_origin_y = terrain.SampleHeight(road_point_pos);
                //cube.transform.position = new Vector3(terrain_data_pos.x_base, road_point_pos.y, terrain_data_pos.y_base);
                cube.name = $"cube_{control_i}_{segment_i}_{terrain.terrainData.GetHeights(terrain_data_pos.x, terrain_data_pos.y, 1, 1)[0, 0]}_{road_point_pos.y}";
                if (terrain_data_origin_y > road_point_pos.y)
                {
                    road_point_pos.y -= 1.0f;
                    for (int i = 0; i < height_set_size; i++)
                    {
                        for (int j = 0; j < height_set_size; j++)
                        {
                            constraint_kernel[i, j] = road_point_pos.y / 294.9983f;
                        }
                    }
                    //constraint_kernel = terrain.terrainData.GetHeights(terrain_data_pos.y_base, terrain_data_pos.x_base, 1, 1);
                    //terrain.terrainData.SetHeights(terrain_data_pos.x, terrain_data_pos.y, constraint_kernel);
                }
                cube.transform.position = road_point_pos;
            }
        }
    }
}