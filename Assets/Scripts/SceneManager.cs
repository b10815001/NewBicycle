using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SceneManager : MonoBehaviour
{
    public GameObject system = null;
    public List<Vector3> pos_list = new List<Vector3>() { };
    public GameObject new_obj = null;
    public GameObject cam;

    private int current_segment = 0;
    private int max_segment = 20;

    private int editor_updating = 3;
    private int editor_updates = 3;

    public string file_path = null;
    private string[] data_lines = new string[] { };

    // Start is called before the first frame update
    void Start()
    {
        StreamReader reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_path);
        string data = reader.ReadToEnd();
        data_lines = data.Split('\n');
        for (int id = 0; id < data_lines.Length; id++)
        {
            string[] str_arr = data_lines[id].Split(' ');

            pos_list.Add(new Vector3(
                float.Parse(str_arr[0]),
                float.Parse(str_arr[1]),
                float.Parse(str_arr[2])));
        }

        system.GetComponent<RoadArchitect.RoadSystem>().AddRoad();
        createNewNode();
        system.GetComponent<RoadArchitect.RoadSystem>().UpdateAllRoads();
    }

    // Update is called once per frame
    void Update()
    {
        if (system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes.Count >= max_segment) GameObject.Destroy(system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes[system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes.Count - 1]);
        if (editor_updating > 0)
        {
            system.GetComponentInChildren<RoadArchitect.Road>().EditorUpdate();
            editor_updating--;
        }
        else if (system.GetComponentInChildren<RoadArchitect.Road>().spline.GetComponentsInChildren<RoadArchitect.SplineN>().Length < max_segment) nextSegment();
    }
    
    private void createNewNode()
    {
        //GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //a.transform.position = pos;
        GameObject obj = new GameObject();
        obj.transform.position = pos_list[(current_segment++) % pos_list.Count];
        obj.AddComponent<RoadArchitect.SplineN>();
        obj.transform.parent = system.GetComponentInChildren<RoadArchitect.Road>().spline.transform;
        obj.GetComponent<RoadArchitect.SplineN>().spline = system.GetComponentInChildren<RoadArchitect.Road>().spline;
    }

    private void nextSegment()
    {
        createNewNode();
        system.GetComponentInChildren<RoadArchitect.Road>().spline.Setup();
        system.GetComponent<RoadArchitect.RoadSystem>().UpdateAllRoads();
        editor_updating = editor_updates;
    }
}
