using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UIWindow))]
public class UIWindowEditor : Editor {

	UIWindow window;
	Object src;
	Button back;

	public override void OnInspectorGUI()
	{
		window = (UIWindow)target;

		EditorGUILayout.Separator();
		if (GUILayout.Button("Reset Buttons")) {
			window.buttons.Clear();
			window.backButton = null;
		}

		GUILayout.Label("List of Transitions");
		EditorGUI.indentLevel++;
		for (int i = 0; i < window.linkedWindows.Count; i++) {
			UIWindow w = window.linkedWindows [i];
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (w.name, GUILayout.MaxWidth (100));
			EditorGUILayout.LabelField ("Drag Button Here", GUILayout.MaxWidth (120));

			//Get src from the window, either the existing button or null if it hasnt been initalized
			if (window.buttons.Count > i) {
				src = (Object)window.buttons[i];
			} else src = null;
			src = EditorGUILayout.ObjectField (src, typeof(Button), true);

			//If button count isnt i+1 we need to add instead of just setting, will set to null for us
			if (window.buttons.Count == i) {
				window.buttons.Add((Button)src);
			} else if (window.buttons.Count > i) { //If its already been initialized just set
				window.buttons[i] = (Button)src;
			}
			EditorGUILayout.EndHorizontal ();
		}
		EditorGUI.indentLevel--;
		EditorGUILayout.Separator();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Back Button", GUILayout.MaxWidth(120));
		window.backButton = (Button)EditorGUILayout.ObjectField(window.backButton, typeof(Button), true);
		
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Separator();
		
		EditorUtility.SetDirty(window);
		serializedObject.Update();
	}
}

