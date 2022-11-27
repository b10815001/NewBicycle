using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[ExecuteInEditMode]
public class exportBicycleTerrain : MonoBehaviour
{
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
                }
            }
            else
            {
                Debug.LogWarning("Warning! terrain_gameobject not be loaded!");
            }
        }
    }
}