using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTMS_Uploader : MonoBehaviour
{
    float update_duration = 0.0f;
    public float update_interval = 3.0f;
    public IndoorBike_FTMS_Connector ftms_connector;
    // Start is called before the first frame update
    void Start()
    {
        BicycleDataSender.initial();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(update_duration > update_interval)
        {
            BicycleDataSender.sendData(ftms_connector.GetPower(), ftms_connector.GetRPM(), ftms_connector.GetSpeed());
            update_duration = 0.0f;
        }
        update_duration += Time.deltaTime;
    }
}
