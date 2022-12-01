using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class FTMS_DeviceSelector : MonoBehaviour
{
    public IndoorBike_FTMS_Connector ftms_connector;
    public Button scanButton;
    public Button connectButton;
    public Text connectButtonText;
    public Button unConnectButton;
    public Transform deviceListParent;
    public GameObject deviceElementProto;
    [HideInInspector]
    public List<GameObject> deviceList = new List<GameObject>();
    [HideInInspector]
    public string selectDeviceId = "";
    public InputField serviceId;
    public InputField characteristicId;
    [HideInInspector]
    public GameObject selectDevice;
    public Text scanButtonText;

    private void Start()
    {
        deviceElementProto.transform.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        if (ftms_connector.IsScanning())
        {
            scanButton.interactable = false;
            scanButtonText.text = "���y��...";
        }
        else
        {
            scanButton.interactable = true;
            scanButtonText.text = "�}�l���y";
        }
        if (selectDeviceId == "" || serviceId.text == "" || characteristicId.text == "")
        {
            connectButtonText.text = "�}�l�s�u";
            connectButton.interactable = false;
        }
        else if (ftms_connector.IsConnecting())
        {
            connectButtonText.text = "�s�u��";
            connectButton.interactable = false;
        }
        else
        {
            connectButtonText.text = "�}�l�s�u";
            connectButton.interactable = true;
        }

        if (ftms_connector.IsConnected())
        {
            unConnectButton.interactable = true;
        }
        else
        {
            unConnectButton.interactable = false;
        }
    }

    public void StartScan()
    {
        if (ftms_connector.IsScanning()) return;
        ftms_connector.StartScan();
        ftms_connector.scanFinishedCallBack += this.FinishScan;
    }

    public void FinishScan(object sender,EventArgs eventArgs)
    {
        Debug.Log("FinishScan");
        for (int i = 0; i < deviceList.Count; i++)
        {
            Destroy(deviceList[i]);
        }
        deviceList.Clear();
        Dictionary<string, Dictionary<string, string>> deviceInfo = ftms_connector.GetScanDevicesInfo();
        foreach (KeyValuePair<string, Dictionary<string, string>> device in deviceInfo)
        {
            if (device.Value["name"] == string.Empty) continue;//�L�o���L�W�˸m
            GameObject gobj = Instantiate(deviceElementProto, deviceListParent);
            gobj.transform.GetChild(0).GetComponent<Text>().text = device.Value["name"];
            gobj.transform.GetChild(1).GetComponent<Text>().text = device.Key;
            deviceList.Add(gobj);
        }
        ftms_connector.scanFinishedCallBack -= this.FinishScan;
    }

    public void SelectDevice(GameObject _selectDevice)
    {
        Debug.Log("SelectDevice");
        for (int i = 0; i < deviceList.Count; i++)
        {
            if (_selectDevice == deviceList[i])
            {
                deviceList[i].GetComponent<Image>().color = new Color(1, 0.5f, 0.6f);
            }
            else
            {
                deviceList[i].GetComponent<Image>().color = deviceElementProto.GetComponent<Image>().color;
            }
        }
        selectDeviceId = _selectDevice.transform.GetChild(1).GetComponent<Text>().text;
    }

    public void StartConnect()
    {
        if (selectDeviceId == "" || serviceId.text == "" || characteristicId.text == "")
        {
            Debug.LogError("�ʤֳs�u��T");
            return;
        }
        Debug.Log("StartConnect " + selectDeviceId + " " + serviceId.text  + " " + characteristicId.text);
        ftms_connector.StartConnect(selectDeviceId, serviceId.text, characteristicId.text);
    }
}
