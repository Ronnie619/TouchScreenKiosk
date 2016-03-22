using UnityEngine;
using UnityEditor;
using System.Collections;
using Rotorz.ReorderableList;

[CustomPropertyDrawer(typeof(AnimationAction))]
public class AnimationActionPropertyDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty name = property.FindPropertyRelative("name");
        SerializedProperty actionEvent = property.FindPropertyRelative("actionEvent");

        float height = EditorGUI.GetPropertyHeight(name) + 4;
        height += EditorGUI.GetPropertyHeight(actionEvent) + 2;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = EditorGUIUtility.singleLineHeight;
        SerializedProperty name = property.FindPropertyRelative("name");
        SerializedProperty actionEvent = property.FindPropertyRelative("actionEvent");

        EditorGUI.PropertyField(position, name);
        position.y += EditorGUI.GetPropertyHeight(name) + 2;

        EditorGUI.PropertyField(position, actionEvent);
        position.y += EditorGUI.GetPropertyHeight(actionEvent) + 2;
    }
}
