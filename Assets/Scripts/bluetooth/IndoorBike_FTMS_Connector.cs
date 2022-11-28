using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorBike_FTMS_Connector : MonoBehaviour
{
    // Start is called before the first frame update
    public bool connect = true;
    public IndoorBike_FTMS connectorAPI;
    void Start()
    {
        connectorAPI = new IndoorBike_FTMS(this);
        if (connect)
        {
            StartCoroutine(connectorAPI.connect());
        }
    }

    void Update()
    {
        connectorAPI.Update();
    }
    private void OnApplicationQuit()
    {
        connectorAPI.quit();
    }

    public void ReConnect()
    {
        if (connect)
        {
            StartCoroutine(connectorAPI.connect());
        }
    }

    public bool IsConnected()
    {
        return connectorAPI.connected;
    }

    public void SetResistance(float val)
    {
        connectorAPI.write_resistance(val);
    }

    public string GetOutput()
    {
        return connectorAPI.output;
    }

    public float GetSpeed()
    {
        return connectorAPI.speed;
    }
    public float GetRPM()
    {
        return connectorAPI.rpm;
    }

    public float GetPower()
    {
        return connectorAPI.power;
    }
}
