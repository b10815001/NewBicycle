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
    public bool createUnderSourceTransform = false;
    public GameObject commonParent = null;
    public bgBuilder builder;
    public string[] grammarFilePathArray;
    // Start is called before the first frame update
    void Start()
    {
        builder = new bgBuilder();
        builder.compile_code(grammarFilePathArray);
        for(int i=0;i< buildingTransformList.Count;i++)
        {
            GameObject building = builder.build(buildingComponentNameList[i]);
            if (building == null) continue;
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

    // Update is called once per frame
    void Update()
    {
        
    }


    public void AddBuilding()
    {
        buildingTransformList.Add(null);
        buildingComponentNameList.Add("");
    }

    public void RemoveLastBuilding()
    {
        buildingTransformList.RemoveAt(buildingTransformList.Count - 1);
        buildingComponentNameList.RemoveAt(buildingComponentNameList.Count - 1);
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
        for (int i = 0; i < size; i++)
        {
            EditorGUILayout.BeginHorizontal();
            buildingTransformList[i] = (Transform)EditorGUILayout.ObjectField(buildingTransformList[i], typeof(Transform), true);
            buildingComponentNameList[i] = EditorGUILayout.TextField(buildingComponentNameList[i]);
            EditorGUILayout.EndVertical();
        }
    }
#endif
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
        //creator.createUnderSourceTransform = EditorGUILayout.Toggle("Create Under Source Transform",creator.createUnderSourceTransform);
        creator.commonParent = (GameObject)EditorGUILayout.ObjectField(creator.commonParent, typeof(GameObject), true);
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
            EditorGUILayout.EndHorizontal();
            creator.DrawBuildingElementList();
            EditorGUILayout.EndVertical();
        }
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(creator);
    }
}
#endif
