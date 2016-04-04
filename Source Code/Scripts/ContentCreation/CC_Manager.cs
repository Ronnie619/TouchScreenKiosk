// CC_Manager.cs
// Author: Morgan Holbart
// CC_Manager manages "Content Creation" mananging anything having to do with runtime editting of the menus
// Any script added for functionality with editting needs to be added as a state here, and if it uses a menu
//	the menu needs to be added here
//

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class CC_Manager : MonoBehaviour {
	
	public static CC_Manager _instance; 								//Singleton instance reference
	List<CC_Menu> menus = new List<CC_Menu>(); 							//List of all currently registered menus

	public GameObject menuButtonPrefab;									//Prefab for menu buttons
	public GameObject menuSpacingPrefab;								//Prefab for a menu spacing element
	public GameObject menuPanelPrefab;									//Prefab for menu panel

	//Prefabs for instantiating UI elements
	public GameObject buttonPrefab;
	public GameObject imagePrefab;
	public GameObject textPrefab;
	
	public List<GameObject> allUIElements = new List<GameObject>();		//All UI elements on screen 

	public Background_Handler backgroundHandler;
	public Img_Handler imageHandler;

	//States for the CC editor
	//If you add a state here, you will need to register your menu to be rendered during that state
	//You will also need to register a callback function that will run SetCCState(yourstate)
	public enum CCState {
		Off,
		Default,
		Selecting
	}
	CCState currState = CCState.Off;

	/// <summary>
	/// Awake function, sets singleton reference
	/// </summary>
	void Awake() {
		if (_instance != null) {
			Destroy (_instance);
			_instance = this;
		} else _instance = this;
	}

	/// <summary>
	/// Start function, create all menus
	/// Any script you intend to use should be initialized here to ensure that CC_Managers initialization
	/// is done first, or you will need to modify script execution order in unity
	/// </summary>
	void Start() {
		CreateMenus();
		CC_UISelectionManager._instance.Initialize();
		UIManager._instance.CreateWindowManagerMenu();

		SetCCState(CCState.Default);
	}

	void OnEnable() {
		SetCCState(CCState.Default);
	}

	void OnDisable() {
		SetCCState(CCState.Off);
	}

	/// <summary>
	/// Update function, nothing should be ran here, and remain event driven, but 
	/// can be used for calling Debug.Log
	/// </summary>
	void Update() {

	}

	/// <summary>
	/// When a window is loaded this is called
	/// </summary>
	public void LoadWindow (UIWindow.WindowData data, int index) {
		for (int i = 0; i < allUIElements.Count; i++) {
			GameObject g = allUIElements[i];
			if (g == null) {
				allUIElements.Remove(g);
				i--;
			}
			else if (CC_Selectable.selectables.Contains(g) && g.GetComponent<CC_Selectable>().myCanvas == index) {
				CC_Selectable.selectables.Remove(g);
				Destroy(g);
			}
		}
		CC_UISelectionManager._instance.LoadWindow();
		SetCCState(CCState.Off);
	}

	/// <summary>
	/// Sets the current CCState and renders all displayed menus for this state
	/// </summary>
	void SetCCState(CCState newState, bool updateMenus) {
		//Begin state enable stuff

		//Begin state disable stuff
		if (newState != CCState.Selecting)
			CC_Selectable.CancelSelect();

		CC_Menu CreationMenu = GetMenuByName("CreationMenu");
		if (CreationMenu != null)
			CreationMenu.SetEnabled(false);

		CC_Menu windowManagerMenu = GetMenuByName("WindowManagerMenu");
		if (windowManagerMenu != null)
			windowManagerMenu.SetEnabled(false);

		CC_UISelectionManager._instance.ChangeCCState(newState);

		backgroundHandler.Disable();

		//Menu rendering
		if (updateMenus)
			UpdateMenus(newState);
		currState = newState;
	}

	void SetCCState(CCState newState) {
		SetCCState(newState, true);
	}

	/// <summary>
	/// Updates the menus positions given a new state
	/// </summary>
	public void UpdateMenus (CCState newState)
	{
		int renderCount = 0;
		for (int i = 0; i < menus.Count; i++) {
			CC_Menu m = menus [i];
			if (m.isEnabled () && m.renderStates.Contains (newState)) {
				m.StartRender ();
				m.SetPosition (renderCount);
				renderCount++;
			} else m.StopRender ();
		}
	}

	/// <summary>
	/// Updates the menus for a change on the current state
	/// </summary>
	public void UpdateMenus() {
		UpdateMenus(currState);
	}

	/// <summary>
	/// Gets reference to a registed menu by name
	/// </summary>
	CC_Menu GetMenuByName(string menuName) {
		foreach (CC_Menu m in menus) {
			if (m.GetName() == menuName) 
				return m;
		}
		return null;
	}

	/// <summary>
	/// Called by CC_UISelectionManager when we select an object
	/// </summary>
	public void SelectObject(GameObject obj) {
		GetMenuByName("SelectionMenu").SetEnabled(true);
		SetCCState(CCState.Selecting);
	}

	/// <summary>
	/// Called by CC_UISelectionManager when we deselect
	/// </summary>
	public void DeselectObject() {
		GetMenuByName("SelectionMenu").SetEnabled(false);
		SetCCState(CCState.Default);
	}
	
	/////////////Begin menu creation
	
	/// <summary>
	/// Creates the menus. Add new menus to be created here
	/// Add new menu creation functions here
	/// </summary>
	void CreateMenus() {
		CreateCCMenu();
		CreateCreationMenu();
	}

	/// <summary>
	/// Creates the CC default menu
	/// </summary>
	void CreateCCMenu() {
		CCState[] renderStates = { CCState.Default, CCState.Selecting };
		CC_Menu menu = CreateNewCC_Menu (menuPanelPrefab, renderStates, "CCMenu");
		menu.AddButton(menuButtonPrefab, () => StopEditMode(), "Stop Editting");
		menu.AddButton(menuButtonPrefab, () => OpenUIElementCreationMenu(), "Creation Menu");
		menu.AddButton(menuButtonPrefab, () => CallBackground(), "Background Color Menu");
		menu.AddButton(menuButtonPrefab, () => ToggleWindowManager(), "Save/Load");
		menu.SetEnabled(true);
	}

	/// <summary>
	/// Creates the creation menu menu.
	/// </summary>
	void CreateCreationMenu() {
		CCState[] renderStates = { CCState.Default };
		CC_Menu menu = CreateNewCC_Menu(menuPanelPrefab, renderStates, "CreationMenu");
		menu.AddButton(menuButtonPrefab, () => CreateButton(), "Create Button");
		menu.AddButton(menuButtonPrefab, () => CreateImage(), "Create Image");
		menu.AddButton(menuButtonPrefab, () => CreateText(), "Create Text");
	}

	public void CallBackground(){
			//Background_Handler temp = new Background_Handler();
			backgroundHandler.Toggle();
	}

	/// <summary>
	/// Creates a base CC_Menu initializes it and adds it to the menu list
	/// </summary>
	public CC_Menu CreateNewCC_Menu(GameObject panelPrefab, CCState[] renderStates, string menuName) {
		//GameObject g = new GameObject();
		//CC_Menu menu = g.AddComponent<CC_Menu>();
		CC_Menu menu = ScriptableObject.CreateInstance<CC_Menu>();
		menu.Initialize(menuPanelPrefab, renderStates, menuName);
		menus.Add(menu);
		return menu;
	}

	//Overload using default menu panel prefab
	public CC_Menu CreateNewCC_Menu(CCState[] renderStates, string menuName) {
		return CreateNewCC_Menu(menuPanelPrefab, renderStates, menuName);
	}

	/////////////End menu creation
	/////////////Begin menu button callback functions

	/// <summary>
	/// Creates a button UI element
	/// </summary>
	void CreateButton() {
		GameObject button = Instantiate(buttonPrefab) as GameObject;
		button.transform.SetParent(UIManager._instance.GetMyCanvas(0), false);
		button.GetComponent<Button>().interactable = false;
		allUIElements.Add(button);
	}

	/// <summary>
	/// Creates a image UI element
	/// </summary>
	void CreateImage() {
		GameObject image = Instantiate(imagePrefab) as GameObject;
		image.transform.SetParent(UIManager._instance.GetMyCanvas(0), false);
		allUIElements.Add(image);
	}

	/// <summary>
	/// Creates a text UI element
	/// </summary>
	void CreateText() {
		GameObject text = Instantiate(textPrefab) as GameObject;
		text.transform.SetParent(UIManager._instance.GetMyCanvas(0), false);
		allUIElements.Add(text);
	}
	
	/// <summary>
	/// Turns off this script
	/// </summary>
	void StopEditMode() {
		this.enabled = false;
	}
	
	/// <summary>
	/// Opens the Creation Menu
	/// </summary>
	void OpenUIElementCreationMenu() {
		SetCCState(CCState.Default, false);
		GetMenuByName("CreationMenu").ToggleEnabled();
		UpdateMenus();
	}

	void ToggleWindowManager() {
		SetCCState(CCState.Default, false);
		GetMenuByName("WindowManagerMenu").ToggleEnabled();
		UpdateMenus();
	}

	/////////////End menu button callback functions
}
