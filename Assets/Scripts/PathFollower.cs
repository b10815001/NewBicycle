using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public GameObject road = null;
    private int current_node = 2;
    private float covered_distance = 0;
    public float slope = 0;

    private float speed = 0.5f;
    private Vector3 camera_height = Vector3.up * 3;

    public bool procedural = false;

    private int current_num = 0;
    private int current_control = 0;
    private float current_seg = 0;
    private List<int> road_num = new List<int>() { 1, 4, 2, 3, 1 };
    private List<Vector2> road_data = new List<Vector2>() { new Vector2(0, 2), new Vector2(3, 1), new Vector2(1, 28), new Vector2(1, 4), new Vector2(48, 0) };
    private int road_seg = 0;
    private int control_seg = 0;
    private int seg = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (road == null)
        {
            road = GameObject.Find("Road1");
        }
        //transform.position = camera_height + road.GetComponent<RoadArchitect.Road>().spline.nodes[current_node].transform.position;
        //transform.LookAt(camera_height + road.GetComponent<RoadArchitect.Road>().spline.nodes[current_node + 1].transform.position, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) arclength();
        //transform.position = movement();
    }

    private Vector3 movement()
    {
        int total_control = 0;
        int id = 0;
        for (; id < road_num.Count; id++)
        {
            total_control += (int)Mathf.Abs(road_data[id][1] - road_data[id][0]);
            if (current_control < total_control)
            {
                road = GameObject.Find("Road" + (road_num[id]).ToString());
                break;
            }
        }

        int grade = (int)((road_data[id][0] - road_data[id][1]) / Mathf.Abs(road_data[id][0] - road_data[id][1]));
        int this_node = (int)road_data[id][1] + grade * (total_control - current_control);
        RoadArchitect.SplineN node = road.GetComponent<RoadArchitect.Road>().spline.nodes[this_node];
        int that_node = (int)road_data[id][1] + grade * (total_control - current_control - 1);
        RoadArchitect.SplineN node2 = road.GetComponent<RoadArchitect.Road>().spline.nodes[that_node];

        return Vector3.zero;
    }

    private void arclength()
    {
        float distance = speed;
        if (procedural)
        {
            List<Vector3> nodes = GameObject.Find("EventSystem").GetComponent<SceneManager>().all_pos[0];
            while (distance > 0)
            {
                int next_node = (current_node + 1 < nodes.Count) ? (current_node + 1) : 0;
                Vector3 direction = nodes[next_node] - nodes[current_node];
                float length = direction.magnitude;

                if (covered_distance + distance >= length)
                {
                    distance -= length - covered_distance;
                    current_node = next_node;
                    next_node = (current_node + 1 < nodes.Count) ? (current_node + 1) : 0;
                    covered_distance = 0;
                    GameObject.Find("EventSystem").GetComponent<SceneManager>().road_buffer++;
                }
                else
                {
                    covered_distance += distance;
                    distance = 0;
                }

                if (distance == 0)
                {
                    Vector3 linear_pos = nodes[current_node] + (nodes[next_node] - nodes[current_node]).normalized * covered_distance;
                    transform.position = Vector3.up * 2 + road.GetComponent<RoadArchitect.Road>().spline.GetSplineValue(road.GetComponent<RoadArchitect.Road>().spline.GetClosestParam(linear_pos, false, false));
                    Quaternion last_rotation = transform.rotation;

                    //transform.LookAt(Vector3.up * 2 + nodes[next_node], Vector3.up);
                    int next_next_node = (next_node + 1 < nodes.Count) ? (next_node + 1) : 0;
                    Vector3 look_at = nodes[next_node] + (nodes[next_next_node] - nodes[next_node]).normalized * covered_distance;
                    transform.LookAt(Vector3.up * 2 + road.GetComponent<RoadArchitect.Road>().spline.GetSplineValue(road.GetComponent<RoadArchitect.Road>().spline.GetClosestParam(look_at, false, false)), Vector3.up);
                    transform.rotation = Quaternion.Lerp(last_rotation, transform.rotation, 0.01f);
                }

                //slope
                Vector3 here = nodes[current_node];
                Vector3 there = nodes[next_node];
                slope = (there.y - here.y) / (Mathf.Sqrt(Mathf.Pow(there.x - here.x, 2) + Mathf.Pow(there.z - here.z, 2)));
            }
        }
        else
        {
            List<RoadArchitect.SplineN> nodes = road.GetComponent<RoadArchitect.Road>().spline.nodes;
            while (distance > 0)
            {
                int next_node = (current_node + 1 < nodes.Count) ? (current_node + 1) : 0;
                Vector3 direction = nodes[next_node].transform.position - nodes[current_node].transform.position;
                float length = direction.magnitude;

                if (covered_distance + distance >= length)
                {
                    distance -= length - covered_distance;
                    current_node = next_node;
                    covered_distance = 0;
                }
                else
                {
                    covered_distance += distance;
                    distance = 0;
                }

                if (distance == 0)
                {
                    Vector3 linear_pos = nodes[current_node].pos + (nodes[next_node].pos - nodes[current_node].pos).normalized * covered_distance;
                    transform.position = camera_height + road.GetComponent<RoadArchitect.Road>().spline.GetSplineValue(road.GetComponent<RoadArchitect.Road>().spline.GetClosestParam(linear_pos, false, false));
                    Quaternion last_rotation = transform.rotation;

                    int next_next_node = (next_node + 1 < nodes.Count) ? (next_node + 1) : 0;
                    Vector3 look_at = nodes[next_node].pos + (nodes[next_next_node].pos - nodes[next_node].pos).normalized * covered_distance;
                    transform.LookAt(camera_height + road.GetComponent<RoadArchitect.Road>().spline.GetSplineValue(road.GetComponent<RoadArchitect.Road>().spline.GetClosestParam(look_at, false, false)), Vector3.up);
                    transform.rotation = Quaternion.Lerp(last_rotation, transform.rotation, 0.01f);
                }

                //slope
                Vector3 here = nodes[current_node].transform.position;
                Vector3 there = nodes[next_node].transform.position;
                slope = (there.y - here.y) / (Mathf.Sqrt(Mathf.Pow(there.x - here.x, 2) + Mathf.Pow(there.z - here.z, 2)));
            }
        }
    }

    public float getOutputSlope()
    {
        if (slope > 0.2f) slope = 0.2f; //cap
        if (slope < -0.2f) return -0.2f;
        if (slope < 0) return (slope + 0.2f) * 5 * 200;
        else if (slope == 0) return 200;
        return (slope * 800 * 5) + 200;
    }
}
