using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SceneManager : MonoBehaviour
{
    public GameObject system = null;
    public List<List<Vector3>> all_pos = new List<List<Vector3>>() { };
    private int reading_pointer = 0;
    public List<Vector3> pos_list = new List<Vector3>() { };
    public GameObject new_obj = null;
    public GameObject cam;

    private int current_segment = 0;
    private int max_segment = 4;
    public int road_buffer = 0;

    private int editor_updating = 3;
    private int editor_updates = 3;

    public string file_path = null;
    private string[] data_lines = new string[] { };

    private Dictionary<int, int> branches = new Dictionary<int, int>();

    private bool loop = true;

    private RoadArchitect.Road combine = null;

    // Start is called before the first frame update
    void Start()
    {
        road_buffer = max_segment;
        StreamReader reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_path);
        string data = reader.ReadToEnd();
        data_lines = data.Split('\n');

        all_pos.Add(new List<Vector3>() { });
        for (int id = 0; id < data_lines.Length; id++)
        {
            if (data_lines[id][0] == '#')
            {
                id++;
                reading_pointer++;
                all_pos.Add(new List<Vector3>() { });
            }
            string[] str_arr = data_lines[id].Split(' ');

            all_pos[reading_pointer].Add(new Vector3(
                float.Parse(str_arr[0]),
                float.Parse(str_arr[1]),
                float.Parse(str_arr[2])));
            if (str_arr.Length > 3) branches.Add(id, int.Parse(str_arr[3]));
        }
        pos_list = all_pos[0];

        system.GetComponent<RoadArchitect.RoadSystem>().AddRoad();
        createNewNode();

        system.GetComponent<RoadArchitect.RoadSystem>().UpdateAllRoads();
    }

    // Update is called once per frame
    void Update()
    {
        if (system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes.Count >= max_segment && editor_updating == 0)
        {
            if (branches.ContainsKey(current_segment - max_segment))
            {
                GameObject.Destroy(GameObject.Find("Inter1"));
                GameObject.Destroy(GameObject.Find("Road" + (branches[current_segment - max_segment] + 1).ToString()));

                system.GetComponent<RoadArchitect.RoadSystem>().UpdateAllRoads();
            }
            GameObject.Destroy(system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes[system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes.Count - 1]);
        }
        if (editor_updating > 0)
        {
            system.GetComponentInChildren<RoadArchitect.Road>().EditorUpdate();
            editor_updating--;
        }
        else if (road_buffer > 0 && !(current_segment == pos_list.Count && !loop))
        {
            nextSegment();
            road_buffer--;
        }
    }

    private void addBranches(int id)
    {
        GameObject branch = system.GetComponent<RoadArchitect.RoadSystem>().AddRoad();
        for (int index = 0; index < all_pos[id].Count; index++)
        {
            GameObject obj = new GameObject();
            obj.transform.position = all_pos[id][index];
            obj.AddComponent<RoadArchitect.SplineN>();
            obj.transform.parent = branch.GetComponent<RoadArchitect.Road>().spline.transform;
            obj.GetComponent<RoadArchitect.SplineN>().spline = branch.GetComponent<RoadArchitect.Road>().spline;
        }
        combine = branch.GetComponent<RoadArchitect.Road>();
    }

    private void createNewNode()
    {
        //GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //a.transform.position = pos;
        GameObject obj = new GameObject();
        obj.transform.position = pos_list[current_segment % pos_list.Count];
        obj.AddComponent<RoadArchitect.SplineN>();
        obj.transform.parent = system.GetComponentInChildren<RoadArchitect.Road>().spline.transform;
        obj.GetComponent<RoadArchitect.SplineN>().spline = system.GetComponentInChildren<RoadArchitect.Road>().spline;
        if (branches.ContainsKey(current_segment - 1)) addBranches(branches[current_segment - 1]);
        current_segment++;
    }

    public void nextSegment()
    {
        createNewNode();
        system.GetComponentInChildren<RoadArchitect.Road>().spline.Setup();
        system.GetComponent<RoadArchitect.RoadSystem>().UpdateAllRoads();
        editor_updating = editor_updates;

        if (combine != null)
        {
            RoadArchitect.Roads.RoadAutomation.CreateIntersectionsProgrammaticallyForRoad(combine, RoadArchitect.RoadIntersection.iStopTypeEnum.TrafficLight1);
            combine = null;
        }
    }
}
