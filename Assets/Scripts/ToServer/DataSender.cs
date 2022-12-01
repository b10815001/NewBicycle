using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System;
using System.IO;

public class DataSender
{
    string token = "";

    public void authorized(string auth_url, string user_info)
    {
        token = "Bearer " + sendTransmit(auth_url, user_info);
    }

    public string sendTransmit(string url, string json_text)
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
            using (System.IO.StreamReader streamRead = new System.IO.StreamReader(streamResponse, Encoding.UTF8))
            {
                content = streamRead.ReadToEnd();
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