//UIManager.cs
//Author: Morgan Holbart
//UIManager is the overarching manager of all UI related actions
//This includes Content Creation, Content Saving and Loading, and Content Viewing

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class UIManager : MonoBehaviour {

	public static UIManager _instance;
	public CC_UISelectionManager elementEditor;
	public GameObject canvasPrefab;
	public GameObject cameraPrefab;

	public GameObject buttonPrefab;
	public GameObject imagePrefab;
	public GameObject textPrefab;

	//public List<int> windowIDPath = new List<int>();

	public List<int> canvas0Path = new List<int>();
	public List<int> canvas1Path = new List<int>();
	public List<int> canvas2Path = new List<int>();
	

	public List<Camera> cameras = new List<Camera>();
	
	public List<GameObject> canvases = new List<GameObject>();
	public List<UIWindow> canvasWindow = new List<UIWindow>();
	public List<int> canvasWindowID = new List<int>();

	void Awake() {
		if (_instance != null) {
			Destroy (_instance);
			_instance = this;
		} else _instance = this;
	}


	void Start() {
		CreateCameras();

		CC_Manager._instance.enabled = false; //Start the CC manager off
		if (elementEditor == null)
			elementEditor = GetComponent<CC_UISelectionManager>();
		canvas0Path.Add(-1);
		canvas1Path.Add(-1);
		canvas2Path.Add(-1);

		AddCanvas();
	}

	void Update() {
		if (Input.GetButtonDown("EditMode") || Input.GetKeyDown(KeyCode.F1)) { //Set desired input here or change it in InputManager
			RemoveAllCanvases();
			CC_Manager._instance.enabled = !CC_Manager._instance.enabled;
			CC_UISelectionManager._instance.enabled = CC_Manager._instance.enabled;
	//		CC_Selectable.CancelSelect();
		}

		if (!CC_Manager._instance.enabled && Input.GetKeyDown(KeyCode.Q)) {
			CC_Manager._instance.enabled = false;
			CC_UISelectionManager._instance.enabled = CC_Manager._instance.enabled;
			TransitionBackOneWindow(0);
		}

		if (Input.GetKeyDown(KeyCode.Alpha7)) {
			CC_Manager._instance.enabled = false;
			CC_UISelectionManager._instance.enabled = CC_Manager._instance.enabled;
			AddCanvas();
		} else if (Input.GetKeyDown(KeyCode.Alpha8)) {
			CC_Manager._instance.enabled = false;
			CC_UISelectionManager._instance.enabled = CC_Manager._instance.enabled;
			RemoveLastCanvas();
		}
	}

	/// <summary>
	/// Adds a new canvas up to a max of 3, for a new menu user
	/// </summary>
	void AddCanvas() {
		if (canvases.Count < 3) {
			GameObject canvas = Instantiate(canvasPrefab) as GameObject;
			canvases.Add(canvas);
			UIWindow window = new UIWindow();
			canvasWindow.Add(window);
			canvasWindowID.Add(-1);
			PopulateWindow(canvases.Count-1);
			//TransitionToWindow(-1, canvases.Count-1);
		}
		UpdateCameras();
	}

	/// <summary>
	/// Removes all canvases except the main one, so 2 and or 3rd one
	/// </summary>
	void RemoveAllCanvases() {
		while (canvases.Count > 1)
			RemoveLastCanvas();
	}

	/// <summary>
	/// Removes the last canvas which is canvas 2 or 3
	/// </summary>
	void RemoveLastCanvas() {
		RemoveCanvasAt(canvases.Count-1);
	}

	/// <summary>
	/// Removes the canvas at index i but will not remove the base canvas
	/// </summary>
	void RemoveCanvasAt(int i) {
		if (canvases.Count <= 1)
			return;

		GameObject canvas = canvases[i];
		canvases.Remove(canvas);
		Destroy (canvas);
		canvasWindow.RemoveAt(canvasWindow.Count-1);
		canvasWindowID[i] = -1;
		if (i == 0) {
			canvas0Path.Clear();
			canvas0Path.Add(-1);
		}
		else if (i == 1) {
			canvas1Path.Clear();
			canvas1Path.Add(-1);
		}
		else if (i == 2) {
			canvas2Path.Clear();
			canvas2Path.Add(-1);
		}
		UpdateCameras();
	}

	//TODO: Canvas scaling
	/// <summary>
	/// Updates the cameras for the number of canvases we are using
	/// </summary>
	void UpdateCameras() {
		for (int i = 0; i < 3; i++) {
			if (canvases.Count-1 >= i) {
				cameras[i].gameObject.SetActive(true);
			} else cameras[i].gameObject.SetActive(false);
		}
		float camWidth = 1;
		if (canvases.Count == 1) {
			camWidth = 1;
			cameras[0].rect = new Rect(0, 0, camWidth, 1);
			canvases[0].GetComponent<Canvas>().worldCamera = cameras[0];
		} else if (canvases.Count == 2) {
			camWidth = 0.5f;
			cameras[0].rect = new Rect(0, 0, camWidth, 1);
			canvases[0].GetComponent<Canvas>().worldCamera = cameras[0];
			cameras[1].rect = new Rect(camWidth, 0, camWidth, 1);
			canvases[1].GetComponent<Canvas>().worldCamera = cameras[1];			
		} else if (canvases.Count == 3) {
			camWidth = 0.33f;
			cameras[0].rect = new Rect(0, 0, camWidth, 1);
			canvases[0].GetComponent<Canvas>().worldCamera = cameras[0];
			cameras[1].rect = new Rect(camWidth, 0, camWidth, 1);
			canvases[1].GetComponent<Canvas>().worldCamera = cameras[1];
			cameras[2].rect = new Rect(camWidth + camWidth, 0, camWidth, 1);
			canvases[2].GetComponent<Canvas>().worldCamera = cameras[2];
		}
	}

	/// <summary>
	/// Creates the cameras, we already have our main camera so add 2 more for 2 more canvases
	/// </summary>
	void CreateCameras() {
		cameras.Add(Camera.main);

		GameObject cam = Instantiate(cameraPrefab) as GameObject;
		cameras.Add(cam.GetComponent<Camera>());
		cam.SetActive(false);

		cam = Instantiate(cameraPrefab) as GameObject;
		cameras.Add(cam.GetComponent<Camera>());
		cam.SetActive(false);
	}

	/// <summary>
	/// Populates the window by creating all the UIElements from the UIWindowData
	/// </summary>
	void PopulateWindow(int index) {
		UIWindow.WindowData data = canvasWindow[index].LoadWindow(canvasWindowID[index]);
		CC_Manager._instance.LoadWindow(data, index);
		//UIWindow.WindowData data = canvasWindowData[index];

		for (int i = 0; i < data.images.Count; i++) {
			GameObject image = Instantiate(imagePrefab) as GameObject;
			image.GetComponent<ImageData>().Initialize(data.images[i], index);
			CC_Manager._instance.allUIElements.Add(image);
			image.GetComponent<CC_Selectable>().myCanvas = index;
		}

		for (int i = 0; i < data.buttons.Count; i++) {
			GameObject button = Instantiate(buttonPrefab) as GameObject;
			ButtonData buttonData = button.GetComponent<ButtonData>();
			buttonData.Initialize(data.buttons[i], index);
			CC_Manager._instance.allUIElements.Add(button);
		}

		for (int i = 0; i < data.texts.Count; i++) {
			GameObject text = Instantiate(textPrefab) as GameObject;
			text.GetComponent<TextData>().Initialize(data.texts[i], index);
			CC_Manager._instance.allUIElements.Add(text);
			text.GetComponent<CC_Selectable>().myCanvas = index;
		}

		Background_Handler.SetCurrColor(data.BackgroundColor, cameras[index]);
	}

	/// <summary>
	/// Creates the window manager menu.
	/// </summary>
	public void CreateWindowManagerMenu() {
		CC_Manager.CCState[] renderStates = { CC_Manager.CCState.Default, CC_Manager.CCState.Selecting };
		CC_Menu menu = CC_Manager._instance.CreateNewCC_Menu(renderStates, "WindowManagerMenu");
		menu.AddButton(() => SaveWindow(), "Save Window");
		menu.AddButton(() => LoadWindow(), "Load Window");
	}

	/// <summary>
	/// Saves the window.
	/// </summary>
	void SaveWindow() {
		//currWindow.SaveWindow(currWindowID);
		canvasWindow[0].SaveWindow(canvasWindowID[0]);
	}

	/// <summary>
	/// Transitions the canvas designated by index back 1 window
	/// </summary>
	void TransitionBackOneWindow(int index) {
		if (canvasWindowID[index] == -1) //Cant go back from start
			return;

		if (index == 0) {
			int currWindow = canvas0Path[canvas0Path.Count-1];
			int nextWindow = canvas0Path[canvas0Path.Count-2];
			TransitionToWindow(nextWindow, index);
			canvas0Path.RemoveAt(canvas0Path.Count-1);
			canvas0Path.RemoveAt(canvas0Path.Count-1);
		} else 	if (index == 1) {
			int currWindow = canvas1Path[canvas1Path.Count-1];
			int nextWindow = canvas1Path[canvas1Path.Count-2];
			TransitionToWindow(nextWindow, index);
			canvas1Path.RemoveAt(canvas1Path.Count-1);
			canvas1Path.RemoveAt(canvas1Path.Count-1);
		} else 	if (index == 2) {
			int currWindow = canvas2Path[canvas2Path.Count-1];
			int nextWindow = canvas2Path[canvas2Path.Count-2];
			TransitionToWindow(nextWindow, index);
			canvas2Path.RemoveAt(canvas2Path.Count-1);
			canvas2Path.RemoveAt(canvas2Path.Count-1);
		}
	}

	/// <summary>
	/// Loads the window.
	/// </summary>
	void LoadWindow() {
		UIWindow.WindowData data = canvasWindow[0].LoadWindow(canvasWindowID[0]);
		if (data == null) 
			return;
		PopulateWindow(0);
	}

	/// <summary>
	/// Gets a new WindowID
	/// </summary>
	public int CreateNewWindowID() {
		return UIWindow.CreateNewWindowID();
	}

	/// <summary>
	/// Transitions the canvas designated by index to the window designated by id
	/// </summary>
	public void TransitionToWindow(int id, int index) {
		if (id == -2) {
			TransitionBackOneWindow(index);
			return;
		}

		UIWindow w = new UIWindow();
		//canvasWindowID[index] = w.LoadWindow(id).windowID;
		canvasWindowID[index] = id;
		if (index == 0)
			canvas0Path.Add(id);
		else if (index == 1)
			canvas1Path.Add(id);
		else canvas2Path.Add(id);
		PopulateWindow(index);
	}
	
	/// <summary>
	/// Gets a canvas based on user index
	/// </summary>
	public Transform GetMyCanvas(int i) {
		Transform t = canvases[i].transform;
		if (t == null) {
			Debug.LogError("Trying to use Canvas that doesnt exist at index: " + i);
			return null;
		}
		return canvases[i].transform;
	}
}
