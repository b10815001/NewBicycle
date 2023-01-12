using ProceduralToolkit;
using RoadArchitect;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PathFollower2 : MonoBehaviour
{
    public List<Road> road_list = new();
    public List<RoadIntersection> intersection_list = new();
    public List<GameObject> roads = new();
    public List<int> start = new();
    public List<int> end = new();
    public List<bool> straight = new();
    public float view_size = 100;
    public float view_buffer = 100;
    public GameObject view = null;

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

    float current_height;
    const int margin = 10;
    const int height_data_length = 1260; // 1280 - 2 * margin
    public float[] height_data;
    public float[] slope_data;
    public float height_data_max = 0.0f;
    public float map_x_max = float.NegativeInfinity;
    public float map_z_max = float.NegativeInfinity;
    public float map_x_min = float.PositiveInfinity;
    public float map_z_min = float.PositiveInfinity;
    bool refresh_map = false;
    bool find_boundary = false;
    Texture2D path_map;
    public Texture2D path_map_current;
    Vector3 current_pos, current_tangent;

    // Start is called before the first frame update
    void Start()
    {
        view.transform.localScale = new Vector3(view_size, 1, view_size) * 2;
        float step_const = 0.25f;
        while (!ended)
        {
            arclength(step_const);
            total_distance += step_const;
        }
        remain_distance = total_distance;
        ended = false;
        find_boundary = true;

        height_data = new float[height_data_length];
        slope_data = new float[height_data_length];
        float map_step = total_distance / height_data_length;
        path_map = new Texture2D(320, 320, TextureFormat.ARGB32, false);
        path_map.wrapMode = TextureWrapMode.Clamp;
        path_map.DrawRect(new RectInt(0, 0, path_map.width, path_map.height), new Color(0, 0, 0, 0));
        for (int d = 0; d < height_data_length; d++)
        {
            arclength(map_step);
            height_data[d] = current_height;
            slope_data[d] = getOutputSlope();
            height_data_max = Mathf.Max(height_data_max, height_data[d]);
            initPathMap();
        }
        path_map.Apply();
        path_map_current = new Texture2D(320, 320, TextureFormat.ARGB32, false);
        path_map_current.wrapMode = TextureWrapMode.Clamp;
        path_map_current.DrawRect(new RectInt(0, 0, path_map_current.width, path_map_current.height), new Color(0, 0, 0, 0));
        Graphics.CopyTexture(path_map, path_map_current); // refresh
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
            float next_step = speedTerm * speedFactor * connector.GetSpeed();
            remain_distance -= next_step;
            arclength(next_step);

            renderView();
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            float next_step = speedTerm * speedFactor * 2000;
            remain_distance -= next_step;
            arclength(next_step, 2000);

            renderView();
        }
    }

    void renderView()
    {
        for (int id = 0; id < road_list.Count; id++)
        {
            bool state = inRange(road_list[id].spline.GetSplineValue(road_list[id].spline.GetClosestParam(transform.position, false, false)));
            road_list[id].gameObject.SetActive(state);
        }
        for (int id = 0; id < intersection_list.Count; id++)
        {
            bool state = inRange(intersection_list[id].gameObject.transform.position);
            intersection_list[id].gameObject.SetActive(state);
        }
    }

    bool inRange(Vector3 compare)
    {
        if ((transform.position - compare).magnitude < view_size) return true;
        else if ((transform.position - compare).magnitude > view_size + view_buffer) return false;
        return true;
    }

    private void arclength(float distance, float speed = 0)
    {
        if (speed == 0) speed = connector.GetSpeed();
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
                            if (current_road == 0)
                            {
                                setEnd();
                            }
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
                        roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetSplineValueBoth(roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetClosestParam(linear_pos, false, false), out current_pos, out current_tangent);
                        transform.position = camera_height + current_pos;
                        updateBoundary();
                        current_height = current_pos.y;
                        current_tangent = current_tangent.normalized;
                        slope = current_tangent.y;
                        Quaternion last_rotation = transform.rotation;

                        int next_next_node = (next_node + 1 < nodes.Count) ? (next_node + 1) : 0;
                        Vector3 look_at = nodes[next_node].pos + (nodes[next_next_node].pos - nodes[next_node].pos).normalized * covered_distance;
                        transform.LookAt(camera_height + roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetSplineValue(roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetClosestParam(look_at, false, false)), Vector3.up);
                        transform.rotation = Quaternion.Lerp(last_rotation, transform.rotation, 0.1f * speedFactor * speed);
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
                            if (current_road == 0)
                            {
                                setEnd();
                            }
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
                        roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetSplineValueBoth(roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetClosestParam(linear_pos, false, false), out current_pos, out current_tangent);
                        transform.position = camera_height + current_pos;
                        updateBoundary();
                        current_height = current_pos.y;
                        current_tangent = current_tangent.normalized;
                        slope = -current_tangent.y; // not straight
                        Quaternion last_rotation = transform.rotation;

                        int next_next_node = (next_node - 1 > 0) ? (next_node - 1) : 0;
                        Vector3 look_at = nodes[next_node].pos + (nodes[next_next_node].pos - nodes[next_node].pos).normalized * covered_distance;
                        transform.LookAt(camera_height + roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetSplineValue(roads[current_road].GetComponent<RoadArchitect.Road>().spline.GetClosestParam(look_at, false, false)), Vector3.up);
                        transform.rotation = Quaternion.Lerp(last_rotation, transform.rotation, 0.1f * speedFactor * speed);
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

    public int getHeightNorm(int index)
    {
        index = Mathf.Min(index, height_data.Length - 1);
        return Mathf.RoundToInt(height_data[index] * 300 / height_data_max) + 10;
    }

    public void setEnd()
    {
        ended = true;
        refresh_map = true;
        remain_distance = total_distance;
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

    public int getMargin()
    {
        return margin;
    }

    public int getHeightDataLength()
    {
        return height_data_length;
    }    

    void updateBoundary()
    {
        if (find_boundary)
            return;

        map_x_max = Mathf.Max(map_x_max, current_pos.x);
        map_z_max = Mathf.Max(map_z_max, current_pos.z);
        map_x_min = Mathf.Min(map_x_min, current_pos.x);
        map_z_min = Mathf.Min(map_z_min, current_pos.z);
    }

    public (float x, float z) toPathMapCoord(Vector3 pos)
    {
        float offset_x = 0.0f, offset_z = 0.0f, norm_size;
        if (map_x_max - map_x_min > map_z_max - map_z_min)
        {
            norm_size = map_x_max - map_x_min;
            //f = map_x_max - map_x_min : map_z_max - map_z_min = 1 : x
            offset_z = ((path_map.height - 2 * margin) - (map_z_max - map_z_min) / norm_size * (path_map.height - 2 * margin)) / 2;
        }
        else
        {
            norm_size = map_z_max - map_z_min;
            offset_x = ((path_map.width - 2 * margin) - (map_x_max - map_x_min) / norm_size * (path_map.width - 2 * margin)) / 2;
        }
        float x = (pos.x - map_x_min) * (path_map.width - 2 * margin) / norm_size + margin + offset_x;
        float z = (pos.z - map_z_min) * (path_map.height - 2 * margin) / norm_size + margin + offset_z;

        return (x, z);
    }

    void initPathMap()
    {
        var coord = toPathMapCoord(current_pos);
        path_map.DrawFilledCircle(new Vector2Int(Mathf.FloorToInt(coord.x), Mathf.FloorToInt(coord.z)), 5, Color.grey);
    }

    public void drawCurrentPosMap()
    {
        if (refresh_map)
        {
            refresh_map = false;
            Graphics.CopyTexture(path_map, path_map_current); // refresh
        }
        var coord = toPathMapCoord(current_pos);
        path_map_current.DrawFilledCircle(new Vector2Int(Mathf.FloorToInt(coord.x), Mathf.FloorToInt(coord.z)), 6, Color.red);
        path_map_current.Apply();
    }
}