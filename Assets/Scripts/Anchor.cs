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
        using (StreamReader sr = new StreamReader($"Assets/{data_dir}/{file_name}"))
        {
            string[] lines = sr.ReadToEnd().Split('\n');
            int main_index = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] inputs = lines[i].Split(' ');
                GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.transform.position = new Vector3(float.Parse(inputs[0]), float.Parse(inputs[1]), float.Parse(inputs[2]));
                var material = new Material(ball.GetComponent<Renderer>().sharedMaterial);
                switch (inputs[3])
                {
                    case "Mainroad":
                        ball.name = $"{inputs[3]} {main_index++}";
                        material.color = Color.red;
                        break;
                    case "Branch":
                        ball.name = inputs[3];
                        material.color = Color.blue;
                        break;
                    case "House":
                        ball.name = inputs[3];
                        material.color = Color.yellow;
                        break;
                    case "Other":
                        ball.name = inputs[3];
                        material.color = Color.grey;
                        break;
                }
                ball.GetComponent<Renderer>().sharedMaterial = material;
                ball.transform.parent = anchor_manager.transform;
            }
        }
    }
}