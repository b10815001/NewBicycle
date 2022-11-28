using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BicycleDataSender
{
    static string auth_url = "http://140.121.197.90:8081/auth/";
    static string user_info = "{\n\"account\":\"Demo2\",\n\"password\":\"1111\"\n}\n";
    static string cycle_data_url = "http://140.121.197.90:8081/cycleInfo/createCycleData/";

    static DataSender data_sender;
    static float max_power;
    static float average_power;
    static float total_power;
    static float max_rotate_speed;
    static float average_rotate_speed;
    static float total_rotate_speed;
    static float max_speed;
    static float average_speed;
    static float total_speed;

    static int times;
    static bool debug = true;

    static public void initial()
    {
        data_sender = new DataSender();
        data_sender.authorized(auth_url, user_info);
        max_power = 0.0f;
        total_power = 0.0f;
        max_rotate_speed = 0.0f;
        total_rotate_speed = 0.0f;
        max_speed = 0.0f;
        total_speed = 0.0f;
        times = 0;
    }

    static public void sendData(float power, float rotate_speed, float speed, bool is_end = false)
    {
        times++;

        max_power = Mathf.Max(power, max_speed);
        total_power += power;
        average_power = total_power / times;

        max_rotate_speed = Mathf.Max(rotate_speed, max_rotate_speed);
        total_rotate_speed += rotate_speed;
        average_rotate_speed = total_rotate_speed / times;

        max_speed = Mathf.Max(speed, max_speed);
        total_speed += speed;
        average_speed = total_speed / times;

        string bicycle_data = $"{{\n\"power\":{power},\n\"rotateSpeed\":{rotate_speed},\n\"speed\":{speed}\n}}";
        string get_data = data_sender.sendTransmit(cycle_data_url, bicycle_data);
        if (debug)
        {
            Debug.Log("bicycle_data: " + bicycle_data);
            Debug.Log(get_data);
        }

        if (is_end)
        {
            string end_bicycle_data = $"{{\n\"maxPower\":{max_power},\n\"averagePower\":{average_power},\n\"maxRotateSpeed\":{max_rotate_speed},\n\"averageRotateSpeed\":{average_rotate_speed},\n\"maxSpeed\":{max_speed},\n\"averageSpeed\":{average_speed}\n}}";
            get_data = data_sender.sendTransmit(cycle_data_url, end_bicycle_data);
            if (debug)
            {
                Debug.Log("end_bicycle_data: " + end_bicycle_data);
                Debug.Log(get_data);
            }
        }
    }

    static public void test()
    {
        initial();

        sendData(20, 25, 15);
        sendData(30, 27, 10, true);
    }
}