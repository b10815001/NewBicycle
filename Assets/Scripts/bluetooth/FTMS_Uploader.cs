using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FTMS_Uploader : MonoBehaviour
{
    public bool uploadSwitch = true;
    float update_duration = 0.0f;
    public float update_interval = 3.0f;
    public Text update_interval_text;
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
        if(update_duration > update_interval && uploadSwitch)
        {
            BicycleDataSender.sendBicycleData(ftms_connector.GetPower(), ftms_connector.GetRPM(), ftms_connector.GetSpeed(), cyclist.ended);
            BicycleDataSender.sendSceneData("Dadu", cyclist.total_distance, cyclist.gameObject.transform.position.y - cyclist.camera_height.y, cyclist.resistance, cyclist.ended);
            if (cyclist.ended)
            {
                cyclist.ended = false;
            }
            update_duration = 0.0f;
        }
        update_duration += Time.deltaTime;
    }

    public void ChangeUploadInterval(float _update_interval)
    {
        update_interval = _update_interval;
        update_interval_text.text = "쨁또픚쾤: 쭯" + update_interval.ToString("0.00") + "ы쨁또�@┯";
    }

    public void SetUploadSwitch(bool _uploadSwitch)
    {
        uploadSwitch = _uploadSwitch;
    }
}