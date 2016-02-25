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

public class CC_UISelectionManager : MonoBehaviour {

	public static CC_UISelectionManager _instance;	//Singleton reference

	public GameObject selected;						//Object being selected atm
	RectTransform selectedRT;						//RectTransform of that object

	//Enum for what kind of selection we are currently using
	public enum SelectionType {
		Move = 0,
		Rotate = 1,
		Scale = 2
	}
	SelectionType editMode;

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

	/// <summary>
	/// When we enable, we want to set creation mode for all selectables
	/// </summary>
	void OnEnable() {
		foreach (GameObject g in CC_Selectable.selectables) {
			g.GetComponent<CC_Selectable>().SetCreationMode(true);
		}
	}

	/// <summary>
	/// When we disable, we want to set creation mode for all selectables
	/// </summary>
	void OnDisable() {
		foreach (GameObject g in CC_Selectable.selectables) {
			g.GetComponent<CC_Selectable>().SetCreationMode(false);
		}
	}

	public void Initialize() {
		CreateSelectionMenu();
	}

	void Start() {
		SetEditMode(SelectionType.Move);
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
				startDrag = Input.mousePosition;
				lastMousePos = Input.mousePosition;
				isDragging = true;
			}
		}

		if (isDragging && Input.GetMouseButtonUp(0)) {
			isDragging = false;
			if (selectedRT.transform.position.x > Screen.width || selectedRT.transform.position.x < 0
			    || selectedRT.transform.position.y > Screen.height || selectedRT.transform.position.y < 0) {
				selectedRT.transform.position = startDrag;
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
		Vector2 mouseDelta = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - lastMousePos;
		lastMousePos = Input.mousePosition;

		switch(editMode) {
		case SelectionType.Move: 
			selected.transform.position += new Vector3(mouseDelta.x, mouseDelta.y, selected.transform.position.z);
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
	}

	/// <summary>
	/// Checks if we are currently selecting inside the rect transform of our current selectable
	/// </summary>
	bool isMouseover() {
		return RectTransformUtility.RectangleContainsScreenPoint(selectedRT, Input.mousePosition, null);
	}

	/// <summary>
	/// Creates the selection menu for the CC_Manager
	/// </summary>
	void CreateSelectionMenu() {
		CC_Manager.CCState[] renderStates = { CC_Manager.CCState.Selecting };
		CC_Menu menu = CC_Manager._instance.CreateNewCC_Menu(renderStates, "SelectionMenu");
		menu.AddButton(() => SetMoveMode(), "Move");
		menu.AddButton(() => SetRotateMode(), "Rotate");
		menu.AddButton(() => SetScaleMode(), "Scale");
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
	/// Gets a selected selectable and if valid, will set the selectable and change to selection state
	/// </summary>
	public void Select(GameObject g) {
		if (selected == g)
			return;

		selected = g;
		if (g != null) {
			selectedRT = selected.GetComponent<RectTransform>();
			CC_Manager._instance.SelectObject(g);
		} else CC_Manager._instance.DeselectObject();
	}

	/// <summary>
	/// Sets the edit mode in case we want to do anything here, if we add keybinds to the 
	/// mode changes we might want to cancel any in progress drags or something
	/// </summary>
	public void SetEditMode(SelectionType type) {
		editMode = type;
	}
}
