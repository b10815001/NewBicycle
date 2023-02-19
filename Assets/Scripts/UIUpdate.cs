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
    Texture2D slope_image_green;
    [SerializeField]
    Texture2D slope_image_yellow;
    [SerializeField]
    Texture2D slope_image_blue;
    [SerializeField]
    Text slope_text;
    [SerializeField]
    PathFollower path_follower;
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
    [SerializeField]
    RawImage path_map_gui;
    [SerializeField]
    RawImage arrow;
    [SerializeField]
    GameObject bluetooth_gui;

    Texture2D slope_map;
    Texture2D slope_map_current;
    bool map_initial = false;
    int margin;
    int height_data_length;
    bool bluetooth_connected = false;

    // Start is called before the first frame update
    void Start()
    {
        slope_map = new Texture2D(1280, 320, TextureFormat.ARGB32, false);
        slope_map.DrawRect(new RectInt(0, 0, slope_map.width, slope_map.height), new Color(0, 0, 0, 0));
        slope_map_current = new Texture2D(1280, 320, TextureFormat.ARGB32, false);
        slope_map_current.wrapMode = TextureWrapMode.Clamp;

        margin = path_follower.getMargin();
        height_data_length = path_follower.getHeightDataLength();
    }

    // Update is called once per frame
    void Update()
    {
        if (!map_initial)
        {
            map_initial = true;
            if (!ftms_connector.IsConnected())
            {
                bluetooth_gui.SetActive(true);
            }

            for (int d = 0; d < height_data_length; d++)
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
                slope_map.DrawCircle(new Vector2Int(d + margin, height_normized), 5, c);
            }
            slope_map.Apply();
            Graphics.CopyTexture(slope_map, slope_map_current);
            slope_map_gui.texture = (Texture)slope_map_current;

            path_map_gui.texture = (Texture)path_follower.path_map_current;
        }

        float slope = path_follower.getOutputSlope() * 100;
        slope_image.transform.rotation = Quaternion.Euler(0, 0, slope);
        if (slope < 0.0f)
        {
            slope_image.texture = (Texture)slope_image_blue;
        }
        else if (slope < 5.0f)
        {
            slope_image.texture = (Texture)slope_image_green;
        }
        else
        {
            slope_image.texture = (Texture)slope_image_yellow;
        }
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

        distance_text.text = $"<color=white>{path_follower.remain_distance:f1}m\nremaining</color>";

        drawSlopeMap();
        path_follower.drawCurrentPosMap();

        if (!bluetooth_connected && ftms_connector.IsConnected())
        {
            bluetooth_connected = true;
            bluetooth_gui.SetActive(false);
        }
    }

    void drawSlopeMap()
    {
        //Graphics.CopyTexture(slope_map, slope_map_current); // refresh
        if (path_follower.remain_distance < 0)
            path_follower.remain_distance = 0;
        int d = Mathf.RoundToInt((path_follower.total_distance - path_follower.remain_distance) * height_data_length / path_follower.total_distance);
        //slope_map_current.DrawFilledCircle(new Vector2Int(margin + d, path_follower.getHeightNorm(d)), 10, Color.red);
        //slope_map_current.Apply();

        float slope = path_follower.getOutputSlope() * 100;
        arrow.transform.rotation = Quaternion.Euler(0, 0, slope * 2);
        arrow.transform.position = slope_map_gui.transform.position - new Vector3(slope_map_gui.rectTransform.rect.width / 2, slope_map_gui.rectTransform.rect.height / 2, 0) + new Vector3(margin + d, path_follower.getHeightNorm(d), 0) / 5.0f;
    }
}