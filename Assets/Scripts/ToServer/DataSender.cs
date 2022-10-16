using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System;
using UnityEditor.PackageManager.Requests;
using System.IO;

public class DataSender : MonoBehaviour
{
    string token = "";
    // Start is called before the first frame update
    void Start()
    {
        string user_info = "{\n\"account\":\"Demo2\",\n\"password\":\"1111\"\n}\n";
        string auth_url = "http://140.121.197.90:8081/auth/";
        token = "Bearer " + sendTransmit(auth_url, user_info);
        string bicycle_data = $"{{\n\"power\":{20},\n\"rotateSpeed\":{25},\n\"speed\":{15}\n}}";
        Debug.Log("bicycle_data: " + bicycle_data);
        string cycle_data_url = "http://140.121.197.90:8081/cycleInfo/createCycleData/";
        Debug.Log(sendTransmit(cycle_data_url, bicycle_data, token));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private string sendTransmit(string url, string json_text, string token = "")
    {
        string content = "";
        try
        {
            byte[] data = Encoding.Default.GetBytes(json_text);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json";
            request.MediaType = "application/json";
            request.Accept = "application/json";
            request.Method = "POST";
            if (token != "")
            {
                request.Headers.Add("Authorization", token);
            }
            System.IO.Stream sm = request.GetRequestStream();
            sm.Write(data, 0, data.Length);
            sm.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            System.IO.Stream streamResponse = response.GetResponseStream();
            // get Token
            System.IO.StreamReader streamRead = new System.IO.StreamReader(streamResponse, Encoding.UTF8);
            char[] readBuff = new char[256];
            int count = streamRead.Read(readBuff, 0, 256);
            while (count > 0)
            {
                string outputData = new string(readBuff, 0, count);
                content += outputData;
                count = streamRead.Read(readBuff, 0, 256);
            }
        }
        catch (WebException ex)
        {
            string message = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            Debug.Log("WebException: " + message);
        }
        catch (System.Exception ex)
        {
            Debug.Log("Exception: " + ex.ToString());
        }
        return content;
    }
}