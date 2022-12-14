using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdate : MonoBehaviour
{
    [SerializeField]
    RawImage slope_image;
    [SerializeField]
    Text slope_test;
    [SerializeField]
    PathFollower2 path_follower;

    // Update is called once per frame
    void Update()
    {
        float slope = path_follower.getOutputSlope() * 100;
        slope_image.transform.rotation = Quaternion.Euler(0, 0, slope);
        slope_test.text = $"<color=white>{slope:f1}%</color>";
    }
}