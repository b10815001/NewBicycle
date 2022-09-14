using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public GameObject road = null;
    private int current_node = 0;
    private float covered_distance = 0;

    private float speed = 0.1f;
    private Vector3 camera_height = Vector3.up * 2;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = camera_height + road.GetComponent<RoadArchitect.Road>().spline.nodes[current_node].transform.position;
        transform.LookAt(camera_height + road.GetComponent<RoadArchitect.Road>().spline.nodes[current_node + 1].transform.position, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) arclength();
    }

    private void arclength()
    {
        float distance = speed;
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
                transform.position = Vector3.up * 2 + nodes[current_node].transform.position + (nodes[next_node].transform.position - nodes[current_node].transform.position).normalized * covered_distance;
                Quaternion last_rotation = transform.rotation;
                transform.LookAt(Vector3.up * 2 + nodes[next_node].transform.position, Vector3.up);
                transform.rotation = Quaternion.Lerp(last_rotation, transform.rotation, 0.01f);
            }
        }
    }
}
