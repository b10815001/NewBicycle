using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTMS_Uploader : MonoBehaviour
{
    float update_duration = 0.0f;
    public float update_interval = 3.0f;
    public IndoorBike_FTMS_Connector ftms_connector;
    public PathFollower2 cyclist = null;
    // Start is called before the first frame update
    void Start()
    {
        BicycleDataSender.initial();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!ftms_connector.IsConnected()) return;
        if(update_duration > update_interval)
        {
            BicycleDataSender.sendData(ftms_connector.GetPower(), ftms_connector.GetRPM(), ftms_connector.GetSpeed(), cyclist.ended);
            if (cyclist.ended)
            {
                cyclist.ended = false;
            }
            update_duration = 0.0f;
        }
        update_duration += Time.deltaTime;
    }
}
