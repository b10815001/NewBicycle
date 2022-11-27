using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingConstraint : MonoBehaviour
{
    [SerializeField]
    GameObject bg_start_creator_gameobject;
    bgStartCreator bg_start_creator;
    GameObject building;
    bool is_initial = false;
    [SerializeField]
    bool do_constraint = false;
    bool is_finished = false;
    Terrain terrain;
    // Start is called before the first frame update
    void Start()
    {
        if (do_constraint && bg_start_creator_gameobject == null)
        {
            Debug.LogWarning("Warning! bg_start_creator not be loaded!");
        }
        else
        {
            if (bg_start_creator_gameobject.TryGetComponent<bgStartCreator>(out bg_start_creator))
            {
                Debug.Log($"bgStartCreator.cs are loaded");
            }

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

            doBuildingConstaint(bg_start_creator.buildingPolygonList[11].points, new Vector3(602.4f, 38.8f, 104.3f));
        }
    }

    void doBuildingConstaint(Vector3[] points, Vector3 bias, bool debug = false)
    {
        int x_base = int.MaxValue;
        int y_base = int.MaxValue;
        int x_boundary = int.MinValue;
        int y_boundary = int.MinValue;
        Vector2[] building_polygon = new Vector2[points.Length];
        for (int point_index = 0; point_index < points.Length; point_index++)
        {
            Vector3 polygon_pos = new Vector3(points[point_index].x * bg_start_creator.polygonCoordScale, 0, points[point_index].z * bg_start_creator.polygonCoordScale) + bias;
            var terrain_data_pos = Utils.getTerrainDataPos(terrain, polygon_pos);
            building_polygon[point_index] = new Vector2(terrain_data_pos.x, terrain_data_pos.y);
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
                cube.name = $"{terrain_data_pos.x} {terrain_data_pos.y}";
                cube.transform.position = polygon_pos;
            }
        }

        int height_x_size = x_boundary - x_base + 1;
        int height_y_size = y_boundary - y_base + 1;
        float[,] constraint_kernel = terrain.terrainData.GetHeights(x_base, y_base, height_x_size, height_y_size);

        // display points of the polygon
        if (debug)
        {
            for (int point_index = 0; point_index < building_polygon.Length - 1; point_index++)
            {
                constraint_kernel[Mathf.RoundToInt(building_polygon[point_index].y - y_base), Mathf.RoundToInt(building_polygon[point_index].x - x_base)] = bias.y / 294.9983f;
            }
        }

        for (int i = 0; i < height_x_size; i++)
        {
            for (int j = 0; j < height_y_size; j++)
            {
                if (Utils.isPointInPolygon(building_polygon, new Vector2(x_base + i, y_base + j)))
                {
                    constraint_kernel[j, i] = bias.y / 294.9983f;
                }
            }
        }
        terrain.terrainData.SetHeights(x_base, y_base, constraint_kernel);
    }
}