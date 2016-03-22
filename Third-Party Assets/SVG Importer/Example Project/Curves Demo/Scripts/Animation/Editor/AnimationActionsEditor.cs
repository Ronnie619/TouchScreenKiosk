using UnityEngine;
using UnityEditor;
using System.Collections;

using Rotorz.ReorderableList;

[CustomEditor(typeof(AnimationActions))]
public class AnimationActionsEditor : Editor {

    SerializedProperty events;

    
    void OnEnable()
    {
        events = serializedObject.FindProperty("events");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        ReorderableListGUI.Title("Events");
        ReorderableListGUI.ListField(events);

        if(EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
