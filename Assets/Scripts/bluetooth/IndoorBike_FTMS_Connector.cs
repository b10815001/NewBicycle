using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class IndoorBike_FTMS_Connector : MonoBehaviour
{
    // Start is called before the first frame update
    public bool scanOnStart = false;
    public IndoorBike_FTMS connectorAPI;
    private bool preScanningStatus = false;
    public event EventHandler scanFinishedCallBack;
    void Start()
    {
        connectorAPI = new IndoorBike_FTMS(this);
        if(scanOnStart) StartScan();
    }

    void Update()
    {
        connectorAPI.Update();
        bool nowScanStatus = IsScanning();
        bool nowConnectingStatus = IsConnecting();
        if (preScanningStatus == true && nowScanStatus == false)
        {
            if (scanFinishedCallBack != null) scanFinishedCallBack(this, EventArgs.Empty);
        }
        preScanningStatus = nowScanStatus;
    }
    private void OnApplicationQuit()
    {
        connectorAPI.quit();
    }

    public void Quit()
    {
        connectorAPI.quit();
    }

    public void StartScan()
    {
        StartCoroutine(connectorAPI.scan_device());
    }

    public void StartConnect(string _deviceId, string _serviceId, string _characteristicId)
    {
        StartCoroutine(connectorAPI.connect(_deviceId, _serviceId, _characteristicId));
    }

    public bool IsConnected()
    {
        return connectorAPI.connected;
    }

    public bool IsScanning()
    {
        return connectorAPI.isScanning;
    }
    public bool IsConnecting()
    {
        return connectorAPI.isConnecting;
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

    public Dictionary<string, Dictionary<string, string>> GetScanDevicesInfo()
    {
        return connectorAPI.devices;
    }
}
