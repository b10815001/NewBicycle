using ProceduralToolkit;
using System.Collections;
using System.Collections.Generic;
using TriLibCore.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdate : MonoBehaviour
{
    [SerializeField]
    RawImage slope_image;
    [SerializeField]
    Text slope_text;
    [SerializeField]
    PathFollower2 path_follower;
    [SerializeField]
    RawImage bluetooth_image;
    [SerializeField]
    RawImage transmit_image;
    [SerializeField]
    IndoorBike_FTMS_Connector ftms_connector;
    [SerializeField]
    Text distance_text;
    [SerializeField]
    RawImage slope_map_gui;

    Texture2D slope_map;
    Texture2D slope_map_current;
    bool map_initial = false;

    // Start is called before the first frame update
    void Start()
    {
        slope_map = new Texture2D(1280, 320, TextureFormat.ARGB32, false);
        slope_map_current = new Texture2D(1280, 320, TextureFormat.ARGB32, false);
        slope_map_current.wrapMode = TextureWrapMode.Clamp;
    }

    // Update is called once per frame
    void Update()
    {
        if (!map_initial)
        {
            map_initial = true;
            for (int d = 0; d < path_follower.height_data_length; d++)
            {
                int height_normized = path_follower.getHeightNorm(d);
                Color c;
                if (path_follower.slope_data[d] < 0.0f)
                {
                    c = new Color(0, 0, 255);
                }
                else if (path_follower.slope_data[d] < 0.05f)
                {
                    c = new Color(0, 255, 0);
                }
                else
                {
                    c = new Color(255, 255, 0);
                }
                slope_map.DrawCircle(new Vector2Int(d, height_normized), 5, c);
            }
            slope_map.Apply();
            Graphics.CopyTexture(slope_map, slope_map_current);
            slope_map_gui.texture = (Texture)slope_map_current;
        }

        float slope = path_follower.getOutputSlope() * 100;
        slope_image.transform.rotation = Quaternion.Euler(0, 0, slope);
        slope_text.text = $"<color=white>{slope:f1}%</color>";

        if (ftms_connector.connectorAPI.isSubscribed)
        {
            bluetooth_image.color = Color.green;
        }
        else
        {
            bluetooth_image.color = Color.gray;
        }

        
        if (BicycleDataSender.transmit_status == TRANSMITSTATUS.success)
        {
            transmit_image.color = Color.green;
        }
        else if (BicycleDataSender.transmit_status == TRANSMITSTATUS.fail)
        {
            transmit_image.color = Color.red;
        }
        else if (BicycleDataSender.transmit_status == TRANSMITSTATUS.closed)
        {
            transmit_image.color = Color.gray;
        }

        distance_text.text = $"<color=white>{path_follower.remain_distance:f1}m remaining</color>";

        drawSlopeMap();
    }

    void drawSlopeMap()
    {
        //Graphics.CopyTexture(slope_map, slope_map_current); // refresh
        int d = Mathf.RoundToInt((path_follower.total_distance - path_follower.remain_distance) * path_follower.height_data_length / path_follower.total_distance);
        slope_map_current.DrawCircle(new Vector2Int(d, path_follower.getHeightNorm(d)), 10, Color.red);
        slope_map_current.Apply();
    }
}