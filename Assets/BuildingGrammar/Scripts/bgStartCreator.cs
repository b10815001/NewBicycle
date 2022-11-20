using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class bgStartCreator : MonoBehaviour
{
    public List<Transform> buildingTransformList = new List<Transform>();
    public List<string> buildingComponentNameList = new List<string>();
    public List<bgPolygonAsset> buildingPolygonList = new List<bgPolygonAsset>();
    public bool createUnderSourceTransform = false;
    public GameObject commonParent = null;
    public bgBuilder builder;
    public string[] grammarFilePathArray;
    public float polygonCoordScale = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        builder = new bgBuilder();
        builder.compile_code(grammarFilePathArray);
        for(int i=0;i< buildingTransformList.Count;i++)
        {
            GameObject building = BuildHouse(buildingComponentNameList[i], buildingTransformList[i].gameObject, buildingPolygonList[i]);
            if (building == null) continue;
            if (createUnderSourceTransform == false)
            { 
                if (buildingTransformList[i] != null)
                {
                    building.transform.position = buildingTransformList[i].position;
                    building.transform.rotation = buildingTransformList[i].rotation;
                    building.transform.localScale = buildingTransformList[i].localScale;
                }
                if (commonParent != null)
                {
                    building.transform.parent = commonParent.transform;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public GameObject BuildHouse(string _componentName,GameObject _targetGameObject,bgPolygonAsset _polygonAsset)
    {
        GameObject building = null;
        if (_componentName == "runtime_base")
        {
            List<Vector3> pointList = new List<Vector3>();
            for (int i = 0; i < _polygonAsset.points.Length; i++)
            {
                //Vector3 point = _targetGameObject.transform.position + (_polygonAsset.points[i] * polygonCoordScale);
                Vector3 point = (_polygonAsset.points[i] * polygonCoordScale);
                pointList.Add(point);
            }
            building = build_polygon_house(pointList, 4.0f);
            building.transform.parent = _targetGameObject.transform;
            building.transform.localPosition = Vector3.zero;
            building.transform.localRotation = Quaternion.identity;
            building.transform.localScale = Vector3.one;
            return building;
        }
        if (createUnderSourceTransform)
        {
            building = builder.build(_componentName,_targetGameObject);
        }
        else
        { 
            building = builder.build(_componentName);
        }
        return building;
    }


    public void AddBuilding()
    {
        buildingTransformList.Add(null);
        buildingComponentNameList.Add("");
        buildingPolygonList.Add(null);
    }

    public void RemoveLastBuilding()
    {
        buildingTransformList.RemoveAt(buildingTransformList.Count - 1);
        buildingComponentNameList.RemoveAt(buildingComponentNameList.Count - 1);
        buildingPolygonList.RemoveAt(buildingPolygonList.Count - 1);
    }

    public void reCompile()
    {
        float start = Time.realtimeSinceStartup;
        builder.clear();
        builder.compile_code(grammarFilePathArray);
        float end = Time.realtimeSinceStartup;
        Debug.Log("compile time:" + (end - start).ToString());
    }

#if UNITY_EDITOR
    public void DrawBuildingElementList()
    {
        int size = buildingTransformList.Count;
        while (size > buildingComponentNameList.Count) buildingComponentNameList.Add("");
        while (size > buildingPolygonList.Count) buildingPolygonList.Add(null);
        for (int i = 0; i < size; i++)
        {
            EditorGUILayout.BeginHorizontal();
            buildingTransformList[i] = (Transform)EditorGUILayout.ObjectField(buildingTransformList[i], typeof(Transform), true);
            buildingComponentNameList[i] = EditorGUILayout.TextField(buildingComponentNameList[i]);
            buildingPolygonList[i] = (bgPolygonAsset)EditorGUILayout.ObjectField(buildingPolygonList[i], typeof(bgPolygonAsset), true);
            EditorGUILayout.EndVertical();
        }
    }
#endif



    #region random polygon house

    static string[] componentNameArray = new string[] {
        "house1","floors_base","hello_house","polygon_house1"
    };

    static string[] facadeNameArray = new string[] {
        "hello_facade2","hello_facade","hello_facade3","hello_facade4","hello_facade5"
    };

    public GameObject build_polygon_house(List<Vector3> vertexs, float width_per_facade, GameObject targetGameObject = null)
    {
        if (!determine_clock_wise(vertexs))
        {
            vertexs.Reverse();
        }
        List<List<string>> component_names = new List<List<string>>();
        for (int i = 0; i < vertexs.Count; i++)
        {
            component_names.Add(new List<string>());
            //int facade_count = Random.Range(1,3);
            float dis;
            if (i != vertexs.Count - 1)
            {
                dis = Vector3.Distance(vertexs[i], vertexs[i + 1]);
            }
            else
            {
                dis = Vector3.Distance(vertexs[i], vertexs[0]);
            }
            int facade_count = (int)Mathf.Floor(dis / width_per_facade);
            if (facade_count == 0)
            {
                component_names[i].Add("nothing_facade");
                continue;
            }
            for (int j = 0; j < facade_count; j++)
            {
                component_names[i].Add(facadeNameArray[Random.Range(0, facadeNameArray.Length)]);
            }
            //component_names[i].Add("nothing_facade");
        }
        builder.load_base_coords("runtime_base", vertexs);
        builder.load_base_facades("runtime_base", component_names);
        return builder.build("runtime_base", targetGameObject);
    }

    bool determine_clock_wise(List<Vector3> points)
    { //§PÂ_¶¶®É°wOR°f®É°w
        //https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
        int coords_size = points.Count;
        float result = 0.0f;
        for (int index = 0; index < coords_size; index++)
        {
            Vector3 current = points[index];
            Vector3 next = points[(index + 1) % coords_size];
            result += (next.x - current.x) * (next.z + current.z);
        }
        return result >= 0.0f;
    }

    #endregion
}


#if UNITY_EDITOR

[CustomEditor(typeof(bgStartCreator))]
public class bgStartCreatorEditor : Editor
{
    bool foldOut = true;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        bgStartCreator creator = (bgStartCreator)target;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("grammarFilePathArray"), new GUIContent("Grammar File Path Array"));
        creator.createUnderSourceTransform = EditorGUILayout.ToggleLeft("Create Under Source Transform", creator.createUnderSourceTransform);
        if (creator.createUnderSourceTransform == false)
        {
            creator.commonParent = (GameObject)EditorGUILayout.ObjectField("Common Parent GameObject", creator.commonParent, typeof(GameObject), true);
        }
        foldOut = EditorGUILayout.Foldout(foldOut, "Building List");
        if (foldOut)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                creator.AddBuilding();
            }
            if (GUILayout.Button("-"))
            {
                creator.RemoveLastBuilding();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Transform");
            EditorGUILayout.LabelField("Component Name");
            EditorGUILayout.LabelField("Polygon Asset");
            EditorGUILayout.EndHorizontal();
            creator.DrawBuildingElementList();
            EditorGUILayout.EndVertical();
        }
        creator.polygonCoordScale = EditorGUILayout.Slider("Polygon Coord Scale:",creator.polygonCoordScale, 0.001f, 100.0f);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
