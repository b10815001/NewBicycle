using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class bgPolygonAsset : ScriptableObject
{
    public Vector3[] points;
}


#if UNITY_EDITOR
[CustomEditor(typeof(bgPolygonAsset))]
public class bgPolygonAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("points"), new GUIContent("Polygon Points"));
    }
}
#endif