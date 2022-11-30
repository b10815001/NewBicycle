using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

[ExecuteInEditMode]
public class importTerrain : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    bool import_terrain;
    GameObject terrain_object;
    Terrain terrain;
    TerrainCollider terrain_collider;
    [SerializeField]
    Material terrian_mat;
    [SerializeField]
    TerrainLayer grass;
    [SerializeField]
    GameObject broadleaf_prefab;
    [SerializeField]
    GameObject conifer_prefab;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (import_terrain)
        {
            import_terrain = false;


            var log_path = EditorUtility.OpenFilePanel(
                "Open log",
                "",
                "log");

            using (StreamReader sr = new StreamReader(log_path))
            {
                string raw_name = sr.ReadLine().Split(' ')[1];
                string[] pos_string = sr.ReadLine().Split(' ');
                string[] size_string = sr.ReadLine().Split(' ');
                int resolution = int.Parse(sr.ReadLine().Split(' ')[1]);
                int tree_instance_count = int.Parse(sr.ReadLine().Split(' ')[1]);
                int[] tree_instance_index = new int[tree_instance_count];
                Vector3[] tree_instance_pos = new Vector3[tree_instance_count];
                float[] tree_instance_width = new float[tree_instance_count];
                float[] tree_instance_height = new float[tree_instance_count];
                float[] tree_instance_rotation = new float[tree_instance_count];
                for (int tree_index = 0; tree_index < tree_instance_count; tree_index++)
                {
                    string[] tree_string = sr.ReadLine().Split(' ');
                    tree_instance_pos[tree_index] = new Vector3(float.Parse(tree_string[3]), float.Parse(tree_string[4]), float.Parse(tree_string[5]));
                    tree_instance_index[tree_index] = int.Parse(tree_string[1]);
                    tree_instance_width[tree_index] = float.Parse(tree_string[7]);
                    tree_instance_height[tree_index] = float.Parse(tree_string[8]);
                    tree_instance_rotation[tree_index] = float.Parse(tree_string[10]);
                }

                terrain_object = new GameObject("terrain");
                terrain_object.transform.position = new Vector3(float.Parse(pos_string[1]), float.Parse(pos_string[2]), float.Parse(pos_string[3]));
                terrain = terrain_object.AddComponent<Terrain>();
                terrain.name = raw_name.Split('.')[0];
                terrain.transform.position = new Vector3(float.Parse(pos_string[1]), float.Parse(pos_string[2]), float.Parse(pos_string[3]));
                terrain.terrainData = new TerrainData();
                terrain.terrainData.heightmapResolution = resolution;
                terrain.terrainData.size = new Vector3(float.Parse(size_string[1]), float.Parse(size_string[2]), float.Parse(size_string[3]));
                loadTerrainRaw(Path.GetDirectoryName(log_path) + "//" + raw_name, terrain.terrainData);
                terrain.materialTemplate = terrian_mat;
                terrain_collider = terrain_object.AddComponent<TerrainCollider>();
                terrain_collider.terrainData = terrain.terrainData;

                terrain.terrainData.terrainLayers = new TerrainLayer[1] { grass };

                TreePrototype broadleaf = new TreePrototype();
                broadleaf.prefab = broadleaf_prefab;
                TreePrototype conifer = new TreePrototype();
                conifer.prefab = conifer_prefab;
                terrain.terrainData.treePrototypes = new TreePrototype[2] { broadleaf, conifer };
                loadTerrainTreeInstances(terrain.terrainData, tree_instance_index, tree_instance_pos, tree_instance_width, tree_instance_height, tree_instance_rotation);
            }

            var asset_path = EditorUtility.SaveFilePanel(
                "Save terrain data asset",
                "",
                $"{terrain.name}.asset",
                "asset");
            var relative_path = Path.GetRelativePath(Application.dataPath,
                asset_path
                );
            AssetDatabase.CreateAsset(terrain.terrainData, $"Assets/{relative_path}");
            AssetDatabase.SaveAssets();
        }
    }

    void loadTerrainRaw(string file_path, TerrainData terrain_data)
    {
        float[,] data = new float[terrain_data.heightmapResolution, terrain_data.heightmapResolution];
        using (var file = System.IO.File.OpenRead(file_path))
        using (var reader = new System.IO.BinaryReader(file))
        {
            for (int y = 0; y < terrain_data.heightmapResolution; y++)
            {
                for (int x = 0; x < terrain_data.heightmapResolution; x++)
                {
                    float v = (float)reader.ReadUInt16() / 0xFFFF;
                    data[y, x] = v;
                }
            }
        }
        terrain_data.SetHeights(0, 0, data);
    }

    void loadTerrainTreeInstances(TerrainData terrain_data, int[] index, Vector3[] pos, float[] width, float[] height, float[] rotation)
    {
        TreeInstance[] tree_instances = new TreeInstance[pos.Length];
        for (int tree_index = 0; tree_index < tree_instances.Length; tree_index++)
        {
            tree_instances[tree_index] = new TreeInstance();
            tree_instances[tree_index].position = pos[tree_index];
            tree_instances[tree_index].prototypeIndex = index[tree_index];
            tree_instances[tree_index].widthScale = width[tree_index];
            tree_instances[tree_index].heightScale = height[tree_index];
            tree_instances[tree_index].rotation = rotation[tree_index];
            terrain_data.SetTreeInstances(tree_instances, false);
        }
    }
#endif
}