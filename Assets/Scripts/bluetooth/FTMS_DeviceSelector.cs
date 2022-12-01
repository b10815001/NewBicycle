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
    public Button endButton;
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
    float print_interval = 2.0f;
    float print_duration = 0.0f;
    bool scan_start = false;
    HashSet<string> deviceInfoDisplay = new HashSet<string>();

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
            scanButtonText.text = "掃描中...";
        }
        else
        {
            scanButton.interactable = true;
            scanButtonText.text = "開始掃描";
            scan_start = true;
        }

        if (scan_start)
        {
            print_duration += Time.deltaTime;
            if (print_duration > print_interval)
            {
                print_duration = 0.0f;
                UpdateScan();
            }
        }

        if (selectDeviceId == "" || serviceId.text == "" || characteristicId.text == "")
        {
            connectButtonText.text = "開始連線";
            connectButton.interactable = false;
        }
        else if (ftms_connector.IsConnecting())
        {
            connectButtonText.text = "連線中";
            connectButton.interactable = false;
        }
        else
        {
            connectButtonText.text = "開始連線";
            connectButton.interactable = true;
        }

        if (ftms_connector.IsConnected())
        {
            unConnectButton.interactable = true;
            endButton.interactable = true;
        }
        else
        {
            unConnectButton.interactable = false;
            endButton.interactable = false;
        }
    }

    public void StartScan()
    {
        if (ftms_connector.IsScanning()) return;
        for (int i = 0; i < deviceList.Count; i++)
        {
            Destroy(deviceList[i]);
        }
        deviceList.Clear();
        ftms_connector.StartScan();
        ftms_connector.scanFinishedCallBack += this.FinishScan;
    }

    public void UpdateScan()
    {
        Dictionary<string, Dictionary<string, string>> deviceInfo = ftms_connector.GetScanDevicesInfo();
        foreach (KeyValuePair<string, Dictionary<string, string>> device in deviceInfo)
        {
            if (device.Value["name"] == string.Empty || deviceInfoDisplay.Contains(device.Key)) continue;//過濾掉無名裝置
            GameObject gobj = Instantiate(deviceElementProto, deviceListParent);
            gobj.transform.GetChild(0).GetComponent<Text>().text = device.Value["name"];
            gobj.transform.GetChild(1).GetComponent<Text>().text = device.Key;
            deviceList.Add(gobj);
            deviceInfoDisplay.Add(device.Key);
        }
    }

    public void FinishScan(object sender,EventArgs eventArgs)
    {
        Debug.Log("FinishScan");
        Dictionary<string, Dictionary<string, string>> deviceInfo = ftms_connector.GetScanDevicesInfo();
        foreach (KeyValuePair<string, Dictionary<string, string>> device in deviceInfo)
        {
            if (device.Value["name"] == string.Empty) continue;//過濾掉無名裝置
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
            Debug.LogError("缺少連線資訊");
            return;
        }
        Debug.Log("StartConnect " + selectDeviceId + " " + serviceId.text  + " " + characteristicId.text);
        ftms_connector.StartConnect(selectDeviceId, serviceId.text, characteristicId.text);
    }
}
