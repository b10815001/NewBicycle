using System.Collections;
using System.Collections.Generic;
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
    
    // Update is called once per frame
    void Update()
    {
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
    }
}