using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Anchor)), CanEditMultipleObjects]
public class AnchorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var anchor = (Anchor)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Display Anchors"))
        {
            anchor.displayAnchors();
        }
    }
}