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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void displayAnchors()
    {
        GameObject anchor_manager = new GameObject("Anchor Manager");
        GameObject anchor_manager_mainroad = new GameObject("Anchor Manager Mainroad");
        GameObject anchor_manager_branch = new GameObject("Anchor Manager Branch");
        GameObject anchor_manager_house = new GameObject("Anchor Manager House");
        GameObject anchor_manager_other = new GameObject("Anchor Manager Other");
        anchor_manager_mainroad.transform.parent = anchor_manager.transform;
        anchor_manager_branch.transform.parent = anchor_manager.transform;
        anchor_manager_house.transform.parent = anchor_manager.transform;
        anchor_manager_other.transform.parent = anchor_manager.transform;

        using (StreamReader sr = new StreamReader($"Assets/{data_dir}/{file_name}"))
        {
            string[] lines = sr.ReadToEnd().Split('\n');
            int main_index = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] inputs = lines[i].Split(' ');
                if (inputs.Length < 3)
                    continue;
                GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.transform.position = new Vector3(float.Parse(inputs[0]), float.Parse(inputs[1]), float.Parse(inputs[2]));
                var material = new Material(ball.GetComponent<Renderer>().sharedMaterial);
                switch (inputs[3])
                {
                    case "Mainroad":
                        ball.name = $"{inputs[3]} {main_index++}";
                        material.color = Color.red;
                        ball.GetComponent<Renderer>().sharedMaterial = material;
                        ball.transform.parent = anchor_manager_mainroad.transform;
                        break;
                    case "Branch":
                        ball.name = inputs[3];
                        material.color = Color.blue;
                        ball.GetComponent<Renderer>().sharedMaterial = material;
                        ball.transform.parent = anchor_manager_branch.transform;
                        break;
                    case "House":
                        ball.name = inputs[3];
                        material.color = Color.yellow;
                        ball.GetComponent<Renderer>().sharedMaterial = material;
                        ball.transform.parent = anchor_manager_house.transform;
                        break;
                    case "Other":
                        ball.name = inputs[3];
                        material.color = Color.grey;
                        ball.GetComponent<Renderer>().sharedMaterial = material;
                        ball.transform.parent = anchor_manager_other.transform;
                        break;
                }
            }
        }

        anchor_manager_branch.SetActive(false);
        anchor_manager_house.SetActive(false);
        anchor_manager_other.SetActive(false);
    }
}