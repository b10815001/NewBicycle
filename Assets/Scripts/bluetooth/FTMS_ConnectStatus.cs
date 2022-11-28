using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FTMS_ConnectStatus : MonoBehaviour
{
    public IndoorBike_FTMS_Connector ftms_connector;
    Text uiText; 
    // Start is called before the first frame update
    void Start()
    {
        uiText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        uiText.text = ftms_connector.IsConnected() ? "已連接" : "未連接";
    }
}
