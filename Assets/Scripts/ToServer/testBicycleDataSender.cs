using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class testBicycleDataSender : MonoBehaviour
{
    // You can put this script to any gameObject and press test to test API
    [SerializeField]
    bool test;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (test)
        {
            test= false;
            BicycleDataSender.test();
        }
    }
}