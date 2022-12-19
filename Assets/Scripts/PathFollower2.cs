using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathFollower2 : MonoBehaviour
{
    public List<GameObject> roads = new();
    public List<int> start = new();
    public List<int> end = new();
    public List<bool> straight = new();

    private int current_road = 0;
    private int current_node = 0;
    private float covered_distance = 0;
    public float slope = 0;

    float speedFactor = 0.0056f; //1000/3600 * 0.02
    float speedTerm = 1.0f;
    public Vector3 camera_height = Vector3.up * 3;

    public bool procedural = false;
    private bool pause = false;
    public bool ended = false;

    public IndoorBike_FTMS_Connector connector;
    public float total_distance = 0;
    public float remain_distance = 0;
    public float resistance = 0;
    public bool use_resistance = false;

    public Text speedTermText;

    // Start is called before the first frame update
    void Start()
    {
        float step_const = 0.25f;
        while (!ended)
        {
            arclength(step_const);
            total_distance += step_const;
        }
        remain_distance = total_distance;
        ended = false;

        transform.position = camera_height + roads[current_road].GetComponent<RoadArchitect.Road>().spline.nodes[current_node].transform.position;
        transform.LookAt(camera_height + roads[current_road].GetComponent<RoadArchitect.Road>().spline.nodes[current_node + 1].transform.position, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            pause = !pause;
    }

    void FixedUpdate()
    {
        if (!pause)
        {
            float next_step = speedTerm *speedFactor * connector.GetSpeed();
            remain_distance -= next_step;
            arclength(next_step);
        }
    }

    private void arclength(float distance)
    {
        List<RoadArchitect.SplineN> nodes = roads[current_road].GetComponent<RoadArchitect.Road>().spline.nodes;
        int next_node = 0;
       
        if (procedural)
        {
            /*
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
            */
        }
        else
        {
            while (distance > 0)
            {
                if (straight[current_road])
                {
                    next_node = (current_node + 1 < nodes.Count) ? (current_node + 1) : 0;
                    Vector3 direction = nodes[next_node].transform.position - nodes[current_node].transform.position;
                    float length = direction.magnitude;

                    if (covered_distance + distance >= length)
                    {
                        distance -= length - covered_distance;
                        current_node = next_node;
                        if (next_node == end[current_road])
                        {
                            current_road = (current_road + 1) % roads.Count;
                            if (current_road == 0) ended = true;
                            current_node = start[current_road];
                            nodes = roads[current_road].GetComponent<RoadArchitect.Road>().spline.nodes;
                        }
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
                        Vector3 current_pos, current_tangent;
                        roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetSplineValueBoth(roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetClosestParam(linear_pos, false, false), out current_pos, out current_tangent);
                        transform.position = camera_height + current_pos;
                        current_tangent = current_tangent.normalized;
                        slope = current_tangent.y;
                        Quaternion last_rotation = transform.rotation;

                        int next_next_node = (next_node + 1 < nodes.Count) ? (next_node + 1) : 0;
                        Vector3 look_at = nodes[next_node].pos + (nodes[next_next_node].pos - nodes[next_node].pos).normalized * covered_distance;
                        transform.LookAt(camera_height + roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetSplineValue(roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetClosestParam(look_at, false, false)), Vector3.up);
                        transform.rotation = Quaternion.Lerp(last_rotation, transform.rotation, 0.1f * speedFactor * connector.GetSpeed());
                    }
                }
                else
                {
                    next_node = (current_node - 1 > 0) ? (current_node - 1) : 0;
                    Vector3 direction = nodes[next_node].transform.position - nodes[current_node].transform.position;
                    float length = direction.magnitude;

                    if (covered_distance + distance >= length)
                    {
                        distance -= length - covered_distance;
                        current_node = next_node;
                        if (next_node == end[current_road])
                        {
                            current_road = (current_road + 1) % roads.Count;
                            if (current_road == 0) ended = true;
                            current_node = start[current_road];
                            nodes = roads[current_road].GetComponent<RoadArchitect.Road>().spline.nodes;
                        }
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
                        Vector3 current_pos, current_tangent;
                        roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetSplineValueBoth(roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetClosestParam(linear_pos, false, false), out current_pos, out current_tangent);
                        transform.position = camera_height + current_pos;
                        current_tangent = current_tangent.normalized;
                        slope = current_tangent.y;
                        Quaternion last_rotation = transform.rotation;

                        int next_next_node = (next_node - 1 > 0) ? (next_node - 1) : 0;
                        Vector3 look_at = nodes[next_node].pos + (nodes[next_next_node].pos - nodes[next_node].pos).normalized * covered_distance;
                        transform.LookAt(camera_height + roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetSplineValue(roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetClosestParam(look_at, false, false)), Vector3.up);
                        transform.rotation = Quaternion.Lerp(last_rotation, transform.rotation, 0.1f * speedFactor * connector.GetSpeed());
                    }
                }
            }
        }

        //slope
        //Vector3 here = nodes[current_node].transform.position;
        //Vector3 there = nodes[next_node].transform.position;
        //slope = (there.y - here.y) / (Mathf.Sqrt(Mathf.Pow(there.x - here.x, 2) + Mathf.Pow(there.z - here.z, 2)));
        if (use_resistance)
            resistance = Mathf.FloorToInt(getOutputSlope());
    }

    public float getOutputSlope()
    {
        slope = Mathf.Min(slope, 0.2f);
        slope = Mathf.Max(-0.2f, slope);
        return slope;
        //if (slope < 0) return (slope + 0.2f) * 5 * 200;
        //else if (slope == 0) return 200;
        //return (slope * 800 * 5) + 200;
    }

    public void setEnd()
    {
        ended = true;
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void SetSpeedTerm(float _speedTerm)
    {
        speedTerm = _speedTerm;
        speedTermText.text = "Speed Multiplier: " + speedTerm.ToString("0.00");
    }
}
