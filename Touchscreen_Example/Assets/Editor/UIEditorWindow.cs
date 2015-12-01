//todo: 
//		scaling/zooming Doesn't work very well, looked into it, hopefully not wanted
//
//		debugging	some issue with being unable to drag lines from boxes (not yet replicated)
//
//		if we need more than 1 copy of a window for some reason, will need to move to instantiating
//			new instances of each window or use a scriptable object to store the data

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class UIEditorWindow : EditorWindow {
	List<UIWindow> UIWindows = new List<UIWindow>(); //= new List<UIWindow>();
	List<Rect> windows = new List<Rect>(); //= new List<Rect>();
	Object src;
	int lineMode;
	string[] lineModes = {"Bezier Curve", "Straight Line" };
	UIEditorWindowData data;
	List<Vector2> lineTransforms = new List<Vector2>();

//	Dictionary<UIWindow, Rect> windows = new Dictionary<UIWindow, Rect>();

	[MenuItem("UI/UI")]
	static void Initialize() {
		UIEditorWindow window  = (UIEditorWindow)EditorWindow.GetWindow(typeof(UIEditorWindow), true, "UI");
		UIEditorWindowData myInstance = (UIEditorWindowData )Resources.Load("editorWindowData") as UIEditorWindowData;
		if (myInstance == null) {
			Debug.Log ("Creating new data file");
			myInstance = CreateInstance<UIEditorWindowData>();
			AssetDatabase.CreateAsset(myInstance , "Assets/Resources/editorWindowData.asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		window.data = myInstance;
		window.Init();

	}

	/// <summary>
	/// Initializes the reset, have to reset lines beforehand since the data is held on the UIWindow.
	/// other option might be to flag for reset, and reset on the next run
	/// </summary>
	[MenuItem("UI/Reset UI")]
	static void InitializeReset() {
		UIEditorWindow window  = (UIEditorWindow)EditorWindow.GetWindow(typeof(UIEditorWindow), true, "UI");
		UIEditorWindowData myInstance = (UIEditorWindowData )Resources.Load("editorWindowData") as UIEditorWindowData;
		window.data = myInstance;
		window.Init(); //Init the data
		window.ResetLines(); //Reset lines
		window.SaveData(); //Save the UIWindows
		window.Close(); //Close

		Debug.Log ("Creating new data file");
		myInstance = CreateInstance<UIEditorWindowData>();
		AssetDatabase.CreateAsset(myInstance , "Assets/Resources/editorWindowData.asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();


		Initialize();
	}

	//Save changes on disable
	void OnDisable() {
		if (data != null)
			SaveData ();
	}

	/// <summary>
	/// Saves the data to the serialized object
	/// </summary>
	void SaveData ()
	{
		data.windows.Clear ();
		data.UIWindows.Clear ();
		foreach (Rect r in windows) {
			data.windows.Add (r);
		}
		foreach (UIWindow w in UIWindows) {
			data.UIWindows.Add (w);
			w.linkedWindows.Clear();
			foreach (UIWindow t in w.tempLinkedWindows) {
				w.linkedWindows.Add (t);
			}
		}
		data.lineMode = lineMode;
	}

	/// <summary>
	/// Update/set data from data file
	/// </summary>
	private void Init() {
		windows.Clear();
		UIWindows.Clear();
		foreach (Rect r in data.windows) {
			windows.Add(r);
		}
		foreach (UIWindow w in data.UIWindows) {
			UIWindows.Add(w);
			w.tempLinkedWindows.Clear();
			foreach(UIWindow t in w.linkedWindows) {
				w.tempLinkedWindows.Add(t);
			}
		}
		lineMode = data.lineMode;

		lineTransforms.Add(new Vector3(0,50)); //X line
		lineTransforms.Add(new Vector3(0,50)); //Y line
	}

	/// <summary>
	/// Reset this instance.
	/// </summary>
	void ResetWindows ()
	{
		foreach (UIWindow w in data.UIWindows) {
			w.tempLinkedWindows.Clear ();
		}
		UIWindows.Clear ();
		windows.Clear ();
	}

	void ResetLines() {
		foreach (UIWindow w in data.UIWindows) {
			w.tempLinkedWindows.Clear();
		}
		foreach (UIWindow w in UIWindows) {
			w.tempLinkedWindows.Clear();
		}
	}

	private void OnGUI()
	{
		if (data == null) {
			Debug.LogError("No data found or created for UIEditorWindow, check that the Resources folder exists");
			return; //No reason to allow changes if its not going to be saved, fix it first
		}

		//
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Drag a new UIWindow prefab here", GUILayout.MinWidth(220), GUILayout.MaxWidth(230));
		src = EditorGUILayout.ObjectField(src, typeof(GameObject), true);
		if (src != null) {
			if (!UIWindows.Contains(((GameObject)src).GetComponent<UIWindow>())) {
				UIWindows.Add(((GameObject)src).GetComponent<UIWindow>());
				windows.Add(new Rect(100, 100, 100, 100));
			}
			src = null;
		}
		if (GUILayout.Button("Save")) {
			SaveData();
		}
		if (GUILayout.Button("Revert to Save")) {
			Init();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		lineMode = GUILayout.SelectionGrid(lineMode, lineModes, 3, GUILayout.MaxWidth(350));
		if (GUILayout.Button("Reset Lines")) {
			ResetLines();
		}
		if (GUILayout.Button("Reset All")) {
			ResetLines();
			ResetWindows ();
		}
		EditorGUILayout.EndHorizontal();
		
		BeginWindows();
		for (int i = 0; i < windows.Count; i++) {
			windows[i] = GUI.Window (i, windows[i], WindowFunction, UIWindows [i].name);
		}
		EndWindows();

		HandleMouseEvents();

		Handles.BeginGUI();
		Handles.color = Color.red;
		
//		Handles.DrawLine(lineTransforms[0], lineTransforms[0] + new Vector2(25000, 0));
//		Handles.DrawLine(lineTransforms[1], lineTransforms[1] + new Vector2(0, 25000));

		foreach (UIWindow w in UIWindows) {
			foreach(UIWindow l in w.tempLinkedWindows) {
				int wIndex = UIWindows.IndexOf(w);
				int lIndex = UIWindows.IndexOf(l);
		
				if (lIndex == -1) //Null check
					break;

				Rect rect1 = windows[wIndex];
				Rect rect2 = windows[lIndex];
				Vector2 rect1Pos = new Vector2(rect1.xMax, (rect1.yMin + rect1.yMax)/2);
				Vector2 rect2Pos = new Vector2(rect2.x, (rect2.yMin + rect2.yMax)/2);

				if (lineMode == 0)
					Handles.DrawBezier(rect1Pos, rect2Pos, new Vector2(rect1.xMax + 50f, rect1.center.y), 
					                   new Vector2(rect2.xMin - 50f, rect2.center.y), Color.red, null, 5f);
				else if (lineMode == 1) {
					Handles.DrawSolidDisc(rect2Pos, new Vector3(0, 0, 1), 4);
					Handles.DrawLine(rect1Pos, rect2Pos);
				}
			}
		}

//		Handles.DrawBezier(windowRect.center, windowRect2.center, new Vector2(windowRect.xMax + 50f,windowRect.center.y), 
//		                   new Vector2(windowRect2.xMin - 50f,windowRect2.center.y),Color.red,null,5f);
//		Handles.DrawLine(windowRect.center);
		Handles.EndGUI();		
	}

	/// <summary>
	/// Handles the mouse events in the UIEditorWindow, but not the actual windows
	/// </summary>
	void HandleMouseEvents() {
		Event e = Event.current;

		if (e.type == EventType.MouseUp) { //Incase you let mouse up when not over a window
			startID = -1;
		}

		//Mouse drag to drag all the stuff around
		if (e.type == EventType.MouseDrag && e.button == 0) {
//			Debug.Log ("Drag " + e.delta);`

			bool stopDrag = false;
			for (int i = 0; i < windows.Count; i++) {
				Rect r = windows [i];
				if ((r.position + e.delta).x < 0) {
					r.position = new Vector2 (0, r.position.y);
					stopDrag = true;
				}
				if ((r.position + e.delta).y < 50) {
					r.position = new Vector2 (r.position.x, 50);
					stopDrag = true;
				}
				windows[i] = r;
				Repaint();
			}

			if (!stopDrag) {
				for (int i = 0; i < windows.Count; i++) {
					Rect r = windows[i];

					r.position += e.delta; //Update position based on mouse delta

					//Check for positions less than 0 on x and y to prevent losing things easily
	//				if (r.position.x < 0)
	//					r.position = new Vector2(0, r.position.y);
	//				if (r.position.y < 50)
	//					r.position = new Vector2(r.position.x, 50);

					windows[i] = r;
					Repaint(); //Force repaint or it won't repaint it until mouse drag finishes
				}
			}

//			for (int i = 0; i<2; i++) {
//				lineTransforms[i] += e.delta;
//
//				if (lineTransforms[i].x < 0)
//					lineTransforms[i] = new Vector2(0, lineTransforms[i].y);
//				if (lineTransforms[i].y < 50) 
//					lineTransforms[i] = new Vector2(lineTransforms[i].x, 50);
//
//				Repaint();
//			}
		}

//		if (e.type == EventType.scrollWheel) {
//			zoomScale -= e.delta.y/100;
//			Repaint();
//		}
	}

	int startID = -1; //Start for dragging to connect windows
	void WindowFunction (int windowID) 
	{
		Event e = Event.current;

		if (e.type == EventType.MouseUp && startID > -1 && startID != windowID) {
//			Debug.Log("connected " + startID + " and " + windowID);
			if (!UIWindows[startID].tempLinkedWindows.Contains(UIWindows[windowID]))
				UIWindows[startID].tempLinkedWindows.Add(UIWindows[windowID]);
			startID = -1;
			Repaint();
		}

		if (Event.current.control && e.type == EventType.mouseUp && e.button == 1) {
			UIWindows[windowID].tempLinkedWindows.Clear();
			Repaint();
		}

		if (e.type == EventType.MouseDown && e.button == 1) {
			startID = windowID;
		}

		if (e.button == 0)
			GUI.DragWindow();
	}
}
