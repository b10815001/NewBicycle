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
