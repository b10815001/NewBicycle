using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEditor.ShaderData;

public static class SplitTerrain
{
    static Material terrian_mat;
    static TerrainLayer grass;
    
    public static void setup(Material _terrian_mat, TerrainLayer _grass)
    {
        terrian_mat = _terrian_mat;
        grass = _grass;
    }

    public static GameObject fetchTerrain(Terrain terrain, float x, float z, float size_x, float size_z, int resolution)
    {
        int minimum_resolution = 33;
        GameObject terrain_object = new GameObject("Terrain");
        Terrain new_terrain = terrain_object.AddComponent<Terrain>();
        new_terrain.transform.position = new Vector3(x, terrain.transform.position.y, z);
        new_terrain.terrainData = new TerrainData();
        new_terrain.terrainData.heightmapResolution = resolution;
        new_terrain.terrainData.size = new Vector3(size_x, terrain.terrainData.size.y, size_z);
        new_terrain.terrainData.terrainLayers = new TerrainLayer[1] { grass };
        new_terrain.materialTemplate = terrian_mat;
        TerrainCollider terrain_collider = terrain_object.AddComponent<TerrainCollider>();
        terrain_collider.terrainData = new_terrain.terrainData;

        float[,] heights = new float[resolution, resolution];
        float step_x = size_x / (resolution - 1);
        float step_z = size_z / (resolution - 1);
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                float v = x + step_x * i;
                float u = z + step_z * j;
                if (i == resolution - 1)
                    v = x + size_x;
                if (j == resolution - 1)
                    u = z + size_z;
                Vector3 pos = new Vector3(v, 0, u);
                heights[j, i] = terrain.SampleHeight(pos) / terrain.terrainData.size.y;
            }
        }
        new_terrain.terrainData.SetHeights(0, 0, heights);
        //var terrain_data_pos = Utils.getTerrainDataPos(terrain, x, z);
        //float[,] heights = terrain.terrainData.GetHeights(terrain_data_pos.x, terrain_data_pos.y, minimum_resolution, minimum_resolution);
        //new_terrain.terrainData.SetHeights(0, 0, heights);
        //new_terrain.terrainData.heightmapResolution = resolution;

        return terrain_object;
    }
}