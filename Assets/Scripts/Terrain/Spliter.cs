using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Spliter : MonoBehaviour
{
    [SerializeField]
    Material terrian_mat;
    [SerializeField]
    TerrainLayer grass;
    [SerializeField]
    Terrain terrain;
    [SerializeField]
    GameObject locator;
    [SerializeField]
    bool fetch_terrain;
    [SerializeField]
    int resolution;
    [SerializeField]
    Terrain t1;
    [SerializeField]
    Terrain t2;
    [SerializeField]
    bool neighbor;
    // Update is called once per frame
    void Update()
    {
        if (fetch_terrain)
        {
            fetch_terrain = false;

            //float step_x = terrain.terrainData.size.x / 80;
            //float step_z = terrain.terrainData.size.z / 86;
            //GameObject new_locator = new GameObject("destination");
            //new_locator.transform.position = new Vector3(Mathf.FloorToInt(locator.transform.position.x / step_x) * step_x, locator.transform.position.y, Mathf.FloorToInt(locator.transform.position.z / step_z) * step_z);

            SplitTerrain.setup(terrian_mat, grass);
            GameObject new_terrain = SplitTerrain.fetchTerrain(terrain, locator.transform.position.x, locator.transform.position.z, 100, 100, resolution);
            GameObject new_terrain2 = SplitTerrain.fetchTerrain(terrain, locator.transform.position.x - 100, locator.transform.position.z, 100, 100, resolution);
            GameObject new_terrain3 = SplitTerrain.fetchTerrain(terrain, locator.transform.position.x, locator.transform.position.z + 100, 100, 100, resolution);
        }

        if (neighbor)
        {
            neighbor = false;
            t1.SetNeighbors(t2, null, null, null);
            t2.SetNeighbors(null, null, t1, null);
            Debug.Log(t1.leftNeighbor);
        }
    }
}