using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SceneManager : MonoBehaviour
{
    //data
    public string file_path = null;
    private int reading_pointer = 0;
    public List<List<Vector3>> all_pos = new List<List<Vector3>>() { };
    private int current_segment = 0;
    private int max_segment = 8;
    public int road_buffer = 0;

    //obj
    public GameObject system = null;
    private Dictionary<int, int> branches = new Dictionary<int, int>();
    private Dictionary<int, int> bridges = new Dictionary<int, int>();
    private RoadArchitect.Road combine = null;

    //config
    private bool loop = true;

    // Start is called before the first frame update
    void Start()
    {
        StreamReader reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_path);
        string data = reader.ReadToEnd();
        string[] data_lines = data.Split('\n');

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
            if (str_arr.Length > 3)
            {
                if (int.Parse(str_arr[3]) > 0) branches.Add(id, int.Parse(str_arr[3]));
                else bridges.Add(id, int.Parse(str_arr[3]));
            }
        }

        system.GetComponent<RoadArchitect.RoadSystem>().AddRoad();
        system.GetComponentInChildren<RoadArchitect.Road>().roadDefinition = 0.01f;
        createNewNode();

        system.GetComponent<RoadArchitect.RoadSystem>().UpdateAllRoads();

        road_buffer = max_segment;
    }

    // Update is called once per frame
    void Update()
    {
        if (system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes.Count >= max_segment)
        {
            if (branches.ContainsKey(current_segment - max_segment))
            {
                GameObject.Destroy(GameObject.Find("Inter1"));
                GameObject.Destroy(GameObject.Find("Road" + (branches[current_segment - max_segment] + 1).ToString()));

                system.GetComponent<RoadArchitect.RoadSystem>().UpdateAllRoads();
            }
            if (system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes[system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes.Count - 1].isBridgeEnd)
            {
                RoadArchitect.SplineN start = system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes[system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes.Count - 2];
                start.RemoveAllSplinatedObjects();
                start.RemoveAllEdgeObjects();
            }
            GameObject.Destroy(system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes[system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes.Count - 1]);
        }
        if (road_buffer > 0 && !(current_segment == all_pos[0].Count && !loop))
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

    private void addBridges()
    {
        Debug.Log(0);
        RoadArchitect.SplineN start = system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes[1];
        RoadArchitect.SplineN end = system.GetComponentInChildren<RoadArchitect.Road>().spline.nodes[2];
        //Bridge start:
        if (!start.isEndPoint)
        {
            if (!start.isBridgeEnd && start.CanBridgeStart())
            {
                start.isBridgeStart = true;
                start.BridgeToggleStart();
            }
        }
        //Bridge end:
        if (!end.isEndPoint)
        {
            if (!end.isBridgeStart && end.CanBridgeEnd())
            {
                end.isBridgeEnd = true;
                end.BridgeToggleEnd();
            }
        }
        start.LoadWizardObjectsFromLibrary("Causeway4-2L", true, true);
    }

    private void createNewNode()
    {
        //GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //a.transform.position = pos;
        GameObject obj = new GameObject();
        obj.transform.position = all_pos[0][current_segment % all_pos[0].Count];
        obj.AddComponent<RoadArchitect.SplineN>();
        obj.transform.parent = system.GetComponentInChildren<RoadArchitect.Road>().spline.transform;
        obj.GetComponent<RoadArchitect.SplineN>().spline = system.GetComponentInChildren<RoadArchitect.Road>().spline;
        if (branches.ContainsKey(current_segment - 1)) addBranches(branches[current_segment - 1]);
        if (bridges.ContainsKey(current_segment - 1)) addBridges();
        current_segment++;
    }

    public void nextSegment()
    {
        createNewNode();
        system.GetComponentInChildren<RoadArchitect.Road>().spline.Setup();
        system.GetComponent<RoadArchitect.RoadSystem>().UpdateAllRoads();

        if (combine != null)
        {
            RoadArchitect.Roads.RoadAutomation.CreateIntersectionsProgrammaticallyForRoad(combine, RoadArchitect.RoadIntersection.iStopTypeEnum.TrafficLight1);
            combine = null;
        }
        system.GetComponentInChildren<RoadArchitect.Road>().EditorUpdate();
    }
}
