using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FTMS_Show : MonoBehaviour
{
    // Start is called before the first frame update
    public IndoorBike_FTMS_Connector connector;
    Text text;
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (connector != null)
        {
            text.text = connector.GetOutput();
        }
    }
}
