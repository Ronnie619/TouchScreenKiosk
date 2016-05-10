//CC_UISelectionManager.cs
//Author: Morgan Holbart
//This script handles selection functionality through use of CC_Selectable
//CC_Selectable will let us know what currently selected UI element we have
//And if we select an object, will go into the Selection state in the CC_Manager
//Add functionality for object selection here
//Currently only mouse 0 supported, so left mouse dragging functionality
//We will most likely need to implement some sort of right click functionality or add another selection menu
//for setting properties of the objects such as transitioning screens/setting images/setting text/etc

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class CC_UISelectionManager : MonoBehaviour {

	public static string imgPath;
	public static CC_UISelectionManager _instance;	//Singleton reference
	public GameObject selected;						//Object being selected atm
	RectTransform selectedRT;						//RectTransform of that object
	Button mybutton;
	public Img_Handler currImgHandler;
	public Url_handler currURLHandler;
	public Text_Setting currTextSettings;
	public CC_Menu menu;
	Canvas myCanvas;

	public static string url;
	public static string urltext;

	//Enum for what kind of selection we are currently using
	public enum SelectionType {
		Move = 0,
		Rotate = 1,
		Scale = 2,
	}
	public SelectionType editMode;

	//Variables used for left mouse dragging
	Vector2 lastMousePos;							//Last frames mouse position used for checking mouseDelta
	bool isDragging = false; 						//Are we currently initiating a mouse drag, TODO may need more checks for disabling isDragging such as alt tabbing and stuff
	Vector2 startDrag; 								//Position we started dragging from in case we want to revert to that position

	void Awake() {
		this.enabled = false;

		if (_instance != null) {
			Destroy(_instance);
			_instance = this;
		} else _instance = this;
	}

	void Start() {
		myCanvas = UIManager._instance.GetMyCanvas(0).GetComponent<Canvas>();

		SetEditMode(SelectionType.Move);
		
		if (currImgHandler == null) { //Should be set with public var but heres a backup
			GameObject g = (GameObject)FindObjectOfType(typeof(Img_Handler));
			currImgHandler = g.GetComponent<Img_Handler>();
		}
	}

	/// <summary>
	/// When we enable, we want to set creation mode for all selectables
	/// </summary>
	void OnEnable() {
		CC_Selectable.creationMode = true;
		foreach (GameObject g in CC_Selectable.selectables) {
			if (g == null)
				continue;
			g.GetComponent<CC_Selectable>().SetCreationMode(true);
		}
	}

	/// <summary>
	/// When we disable, we want to set creation mode for all selectables
	/// </summary>
	void OnDisable() {
		CC_Selectable.creationMode = false;
		foreach (GameObject g in CC_Selectable.selectables) {
			if (g == null)
				continue;
			g.GetComponent<CC_Selectable>().SetCreationMode(false);
		}
	}

	public void Initialize() {
		CreateSelectionMenu();
	}

	/// <summary>
	/// Update function too long, need to refactor into some smaller methods if/when we expand this script
	/// </summary>
	void Update() {
		UpdateSelected();

		if (selected == null)
			return;

		if (Input.GetKeyDown(KeyCode.Escape)) {
			CC_Selectable.CancelSelect();
		}

		if (isMouseover()) {
			if (Input.GetMouseButtonDown(0) && !isDragging) { //Begin drag
				RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out lastMousePos);
				startDrag = lastMousePos;
				isDragging = true;
			}
		}

		if (isDragging && Input.GetMouseButtonUp(0)) {
			isDragging = false;

			Vector3 pos = Camera.main.WorldToScreenPoint(selected.transform.position);
			if (pos.x > Screen.width || pos.x < 0
			    || pos.y > Screen.height || pos.y < 0) {
				selected.transform.position = myCanvas.transform.TransformPoint(startDrag);
			}
		}

		if (isDragging) {
			ProcessMovement();
		}
	}

	/// <summary>
	/// Processes the movement of a mouse 0 drag based on our edit mode
	/// </summary>
	void ProcessMovement() {
		Vector2 currMousePos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out currMousePos);
		Vector2 mouseDelta = currMousePos - lastMousePos;

		switch(editMode) {
		case SelectionType.Move: 
			selected.transform.position = myCanvas.transform.TransformPoint
				(new Vector3(selected.transform.position.x, selected.transform.position.y, 0) 
				 + new Vector3(currMousePos.x, currMousePos.y, 0));
			
			break;
		case SelectionType.Scale:
			selected.transform.localScale += new Vector3(mouseDelta.x, mouseDelta.y, 0)/100;
			selected.transform.localScale = new Vector3(Mathf.Clamp(selected.transform.localScale.x, .1f, 5), Mathf.Clamp(selected.transform.localScale.y, .1f, 5), 1);
			break;
		case SelectionType.Rotate: 
			float negRot = mouseDelta.x; //todo proper rot
			float posRot = mouseDelta.y;
			selected.transform.Rotate(Vector3.forward, negRot + posRot);
			break;			
		}

		lastMousePos = currMousePos;
	}

	/// <summary>
	/// Checks if we are currently selecting inside the rect transform of our current selectable
	/// </summary>
	bool isMouseover() {
		Vector2 currMousePos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out currMousePos);
		return RectTransformUtility.RectangleContainsScreenPoint(selectedRT, myCanvas.transform.TransformPoint(currMousePos), null);
	}

	/// <summary>
	/// Creates the selection menu for the CC_Manager
	/// </summary>
	void CreateSelectionMenu() {
		CC_Manager.CCState[] renderStates = { CC_Manager.CCState.Selecting };
		//CC_Menu menu = CC_Manager._instance.CreateNewCC_Menu(renderStates, "SelectionMenu");
		menu = CC_Manager._instance.CreateNewCC_Menu(renderStates, "SelectionMenu");
		
		menu.AddButton(() => SetMoveMode(), "Move");
		menu.AddButton(() => SetRotateMode(), "Rotate");
		menu.AddButton(() => SetScaleMode(), "Scale");
		menu.AddSpace(25);
		menu.AddButton(() => DeleteObject(), "Delete");
		menu.AddSpace(25);
		menu.AddButton(() => CallImgHandler(), "Image Handler");
		//menu.AddButton(() => CallingURLHandler(), "Url Handler");
		menu.AddButton(() => CallTransitionHandler(), "Button Settings");
		menu.AddButton(() => CallTextSettings(), "Text Settings");
	}

	void CallTextSettings() {
		currTextSettings.Enable(selected);
	}

	void UpdateSelectionMenu() {
		CC_ButtonSettings._instance.Disable();

		menu.ShowAllButtons();
		if (selected.GetComponent<ButtonData>()) {
			menu.HideButton(6);
		} else if (selected.GetComponent<ImageData>()) {
			menu.HideButton(5);
			menu.HideButton(6);
		} else if (selected.GetComponent<TextData>()) {
			menu.HideButton(4);
			menu.HideButton(5);
		}
	}

	public void LoadWindow() {
		if (CC_ButtonSettings._instance != null)
			CC_ButtonSettings._instance.Disable();
		currImgHandler.Disable();
		currURLHandler.Disable();
	}

	public void ChangeCCState(CC_Manager.CCState state) {
		if (CC_ButtonSettings._instance != null)
			CC_ButtonSettings._instance.Disable();
		currImgHandler.Disable();
		currURLHandler.Disable();
	}

	public void SetIMGPath(string temp){
		imgPath = temp;
		Debug.Log("Loading current image: " + imgPath + ".jpg");
	}
	
	void DeleteObject(){
		CC_Selectable.selectables.Remove(selected);
		Destroy(selected);
		CC_Selectable.CancelSelect();
	}
	
	public void CallingURLHandler(){
		currURLHandler.callurlhandler();
	}

	void CallTransitionHandler() {
		CC_ButtonSettings._instance.Toggle(selected.GetComponent<ButtonData>());
	}

	public void AssignURL(){
		mybutton = selected.GetComponent<Button>();
		mybutton.onClick.AddListener(OpenURL);
 		Text aButton = selected.GetComponentInChildren<Text>();
 		aButton.text = url;
	}

	/*
	public void SetURL(string temp){
		url = temp;
	} */
	
	void OpenURL(){ Application.OpenURL(url);}

	void CallImgHandler(){
		currImgHandler.Enable(selected);
	}

	/// <summary>
	/// Sets the move mode.
	/// </summary>
	void SetMoveMode() {
		SetEditMode(SelectionType.Move);
	}

	/// <summary>
	/// Sets the rotate mode.
	/// </summary>
	void SetRotateMode() {
		SetEditMode(SelectionType.Rotate);
	}

	/// <summary>
	/// Sets the scale mode.
	/// </summary>
	void SetScaleMode() {
		SetEditMode(SelectionType.Scale);
	}

	/// <summary>
	/// Updates the currently selected selectable
	/// </summary>
	void UpdateSelected() {
		Select(CC_Selectable.selected);
	}

	/// <summary>
	/// Called when we stop selecting an object
	/// </summary>
	void StopSelecting() {
		currImgHandler.Disable();
	}

	/// <summary>
	/// Gets a selected selectable and if valid, will set the selectable and change to selection state
	/// </summary>
	public void Select(GameObject g) {
		if (selected == g)
			return;

		selected = g;
		if (g != null) {
			selectedRT = selected.GetComponent<RectTransform>();
			CC_Manager._instance.SelectObject(g);
			UpdateSelectionMenu();
		} else {
			StopSelecting();
			CC_Manager._instance.DeselectObject();
		}
	}

	/// <summary>
	/// Sets the edit mode in case we want to do anything here, if we add keybinds to the 
	/// mode changes we might want to cancel any in progress drags or something
	/// </summary>
	public void SetEditMode(SelectionType type) {
		editMode = type;
	}
}
