using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorBike_FTMS_Sender: MonoBehaviour
{
    [SerializeField]
    IndoorBike_FTMS_Connector indoorBike_FTMS_connector;
    [SerializeField]
    PathFollower2 path_follower;
    [SerializeField]
    float resistant_factor = 7000f;
    float send_resistance_duration = 0.0f;
    public float send_resistance_interval = 1.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (indoorBike_FTMS_connector.IsConnected())
        {
            if (send_resistance_duration > send_resistance_interval)
            {
                send_resistance_duration = 0.0f;

                float rest = resistant_factor * path_follower.getOutputSlope();
                if (rest < 0.0f)
                    rest = 0.0f;
                if (rest == Mathf.Infinity)
                {
                    Debug.Log("Infinity");
                    rest = 0.0f;
                }
                if (rest == Mathf.NegativeInfinity)
                {
                    Debug.Log("NegativeInfinity");
                    rest = 0.0f;
                }
                if (float.IsNaN(rest))
                {
                    Debug.Log("nan");
                    rest = 0.0f;
                }
                Debug.Log("resistance: " + rest);
                indoorBike_FTMS_connector.SetResistance(rest);
            }
            else 
            {
                send_resistance_duration += Time.deltaTime;
            }

        }
    }
}