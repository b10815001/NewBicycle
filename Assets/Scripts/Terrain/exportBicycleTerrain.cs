using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class exportBicycleTerrain : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    string log_name;
    [SerializeField]
    string raw_name;
    [SerializeField]
    bool export_log;
    [SerializeField]
    GameObject terrain_gameobject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (export_log)
        {
            export_log = false;

            Terrain terrain;
            if (terrain_gameobject.TryGetComponent<Terrain>(out terrain))
            {
                var path = EditorUtility.SaveFilePanel(
                    "Save log",
                    "",
                    log_name,
                    "log");

                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine($"raw_name: {raw_name}");
                    sw.WriteLine($"pos: {terrain_gameobject.transform.position.x} {terrain_gameobject.transform.position.y} {terrain_gameobject.transform.position.z}");
                    sw.WriteLine($"size: {terrain.terrainData.size.x} {terrain.terrainData.size.y} {terrain.terrainData.size.z}");
                    sw.WriteLine($"heightmapResolution: {terrain.terrainData.heightmapResolution}");

                    sw.WriteLine($"treeInstanceCount: {terrain.terrainData.treeInstanceCount}");
                    for (int tree_index = 0; tree_index < terrain.terrainData.treeInstanceCount; tree_index++)
                    {
                        sw.WriteLine($"index: {terrain.terrainData.treeInstances[tree_index].prototypeIndex} pos: {terrain.terrainData.treeInstances[tree_index].position.x} {terrain.terrainData.treeInstances[tree_index].position.y} {terrain.terrainData.treeInstances[tree_index].position.z} scale: {terrain.terrainData.treeInstances[tree_index].widthScale} {terrain.terrainData.treeInstances[tree_index].heightScale} rotation: {terrain.terrainData.treeInstances[tree_index].rotation}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Warning! terrain_gameobject not be loaded!");
            }
        }
    }
#endif
}