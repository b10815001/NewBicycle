using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Anchor : MonoBehaviour
{
    [SerializeField]
    private string data_dir;
    [SerializeField]
    private string file_name;
    [SerializeField]
    private string file_info_name;

    public void displayAnchors()
    {
        GameObject anchor_manager = new GameObject("Anchor Manager");
        GameObject anchor_manager_mainroad = new GameObject("Anchor Manager Mainroad");
        GameObject anchor_manager_branch = new GameObject("Anchor Manager Branch");
        GameObject anchor_manager_building = new GameObject("Anchor Manager Building");
        GameObject anchor_manager_other = new GameObject("Anchor Manager Other");
        anchor_manager_mainroad.transform.parent = anchor_manager.transform;
        anchor_manager_branch.transform.parent = anchor_manager.transform;
        anchor_manager_building.transform.parent = anchor_manager.transform;
        anchor_manager_other.transform.parent = anchor_manager.transform;

        using (StreamReader sr = new StreamReader($"Assets/{data_dir}/{file_name}"), sr_info = new StreamReader($"Assets/{data_dir}/{file_info_name}"))
        {
            string[] lines = sr.ReadToEnd().Split('\n');
            string[] infos = sr_info.ReadToEnd().Split('\n');
            int main_index = 0;
            int building_index = 0;
            int info_i = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] inputs = lines[i].Split(' ');
                if (inputs.Length < 3)
                    continue;
                
                if (inputs[0] == "Mainroad")
                {
                    GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var material = new Material(ball.GetComponent<Renderer>().sharedMaterial);
                    ball.name = $"{inputs[0]} {main_index++} {infos[info_i++]}";
                    ball.transform.position = new Vector3(float.Parse(inputs[1]), float.Parse(inputs[2]), float.Parse(inputs[3]));
                    material.color = Color.red;
                    ball.GetComponent<Renderer>().sharedMaterial = material;
                    ball.transform.parent = anchor_manager_mainroad.transform;
                }
                else if (inputs[0] == "Branch")
                {
                    GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var material = new Material(ball.GetComponent<Renderer>().sharedMaterial);
                    ball.name = inputs[0];
                    ball.transform.position = new Vector3(float.Parse(inputs[1]), float.Parse(inputs[2]), float.Parse(inputs[3]));
                    material.color = Color.blue;
                    ball.GetComponent<Renderer>().sharedMaterial = material;
                    ball.transform.parent = anchor_manager_branch.transform;
                }
                else if (inputs[0] == "Building")
                {
                    int building_point_count = int.Parse(inputs[1]);
                    GameObject building_polygon = new GameObject($"Building {building_index++}");
                    building_polygon.transform.parent = anchor_manager_building.transform;
                    for (int building_point_index = 0; building_point_index < building_point_count; building_point_index++)
                    {
                        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        var material = new Material(ball.GetComponent<Renderer>().sharedMaterial);
                        ball.name = inputs[0];
                        ball.transform.position = new Vector3(float.Parse(inputs[2 + building_point_index * 3]), float.Parse(inputs[2 + building_point_index * 3 + 1]), float.Parse(inputs[2 + building_point_index * 3 + 2]));
                        material.color = Color.yellow;
                        ball.GetComponent<Renderer>().sharedMaterial = material;
                        ball.transform.parent = building_polygon.transform;
                    }
                }
                else if (inputs[0] == "Other")
                {
                    GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var material = new Material(ball.GetComponent<Renderer>().sharedMaterial);
                    ball.name = inputs[0];
                    ball.transform.position = new Vector3(float.Parse(inputs[1]), float.Parse(inputs[2]), float.Parse(inputs[3]));
                    material.color = Color.blue;
                    ball.GetComponent<Renderer>().sharedMaterial = material;
                    ball.transform.parent = anchor_manager_other.transform;
                }
            }
        }

        anchor_manager_branch.SetActive(false);
        anchor_manager_building.SetActive(false);
        anchor_manager_other.SetActive(false);
    }
}