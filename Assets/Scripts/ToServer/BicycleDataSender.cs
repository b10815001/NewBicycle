using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class BicycleDataSender
{
    static string auth_url = "http://140.121.197.90:8081/auth/";
    static string user_info = "{\n\"account\":\"Demo5\",\n\"password\":\"1111\"\n}\n";
    static string bicycle_data_initial_url = "http://140.121.197.90:8081/cycleInfo/createCycleData/";
    static string bicycle_data_consequence_url = "http://140.121.197.90:8081/cycleInfo/addCycleData/";
    static string bicycle_data_end_url = "http://140.121.197.90:8081/cycleInfo/stopCreateCycleData/";
    static string scene_data_initial_url = "http://140.121.197.90:8081/sceneInfo/createSceneData/";
    static string scene_data_consequence_url = "http://140.121.197.90:8081/sceneInfo/addSceneData/";
    static string scene_data_end_url = "http://140.121.197.90:8081/sceneInfo/stopCreateSceneData/";
    static bool is_bicycle_initial;
    static bool is_scene_initial;

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
        is_bicycle_initial = false;
        is_scene_initial = false;
        max_power = 0.0f;
        total_power = 0.0f;
        max_rotate_speed = 0.0f;
        total_rotate_speed = 0.0f;
        max_speed = 0.0f;
        total_speed = 0.0f;
        times = 0;
    }

    static public void sendBicycleData(float power, float rotate_speed, float speed, bool is_end = false)
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
        string get_data = data_sender.sendTransmit(is_bicycle_initial ? bicycle_data_consequence_url : bicycle_data_initial_url, bicycle_data);
        if (debug)
        {
            Debug.Log($"{get_data}\nbicycle_data: {bicycle_data}");
        }

        if (is_end)
        {
            string end_bicycle_data = $"{{\n\"maxPower\":{max_power},\n\"averagePower\":{average_power},\n\"maxRotateSpeed\":{max_rotate_speed},\n\"averageRotateSpeed\":{average_rotate_speed},\n\"maxSpeed\":{max_speed},\n\"averageSpeed\":{average_speed}\n}}";
            get_data = data_sender.sendTransmit(bicycle_data_end_url, end_bicycle_data);
            if (debug)
            {
                Debug.Log($"{get_data}\nend_bicycle_data: {end_bicycle_data}");
            }
        }

        is_bicycle_initial = true;
    }

    static public void sendSceneData(string scene, float distance, float climb, float resistance, bool is_end = false)
    {
        string scene_data = $"{{\n\"scene\":\"{scene}\",\n\"distance\":{distance},\n\"climb\":{climb},\n\"resistance\":{resistance}\n}}";
        string get_data = data_sender.sendTransmit(is_scene_initial ? scene_data_consequence_url : scene_data_initial_url, scene_data);
        if (debug)
        {
            Debug.Log($"{get_data}\nscene_data: {scene_data}");
        }

        if (is_end)
        {
            get_data = data_sender.sendTransmit(scene_data_end_url, "");
            if (debug)
            {
                Debug.Log(get_data);
            }
        }

        is_scene_initial = true;
    }

    static public void test()
    {
        initial();

        sendBicycleData(20, 25, 15);
        sendBicycleData(30, 27, 10, true);

        sendSceneData("Dadu", 13, 150, 0);
        sendSceneData("Dadu", 14, 152, 2);
        sendSceneData("Dadu", 15, 153, 1, true);
    }
}