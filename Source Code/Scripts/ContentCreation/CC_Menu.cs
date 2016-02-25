/// Author: Morgan Holbart
/// Date: February 23, 2016
/// Holds information for a menu to be used with the content creation
/// Unless you need specific functionality added here, this shouldn't need modification
/// CC_Menus are created in your own script see the CC_UISelectionManager for an example how
/// Missing functionality right now: adding different button types (toggles/toggle groups)
/// 	for selecting from a group of buttons (needed for CC_UISelectionManager) and toggles for
/// 	our CC_Managers CreationMenu being enabled or not
///	TODO: theres a lot of calling of updating positions/menu statuses/etc in CC_Manager which could
/// 		be made more automatic, will look at it later and look into an isDirty flag for UIManager
/// 		which will update menus, instead of having to force an update or force a State update/change

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CC_Menu : ScriptableObject {
	
	//Either need to get the prefab from CC_Manager, or add a Resources folder to load our prefab from 
	//or make a prefab scriptable object to instantiate but that's not that great
	public GameObject buttonPrefab {
		get {
			return CC_Manager._instance.menuButtonPrefab;
		}
	}
	public List<CC_Manager.CCState> renderStates = new List<CC_Manager.CCState>();	//States that this menu is rendered in
	List<Button> Buttons = new List<Button>();										//Buttons tied to this menu
	Transform myPanel;																//Panel this menu is attached to
	bool menuEnabled = false;														//If this menu is enabled, if not enabled, will not render even in its states
	float menuSize = 50f;															//Pixel size of the menu bar

	//Default rect transform values for setting positions of the RT
	//Will need to be modified if we want to change sizes of menus 
	float minX, maxX;
	float minY, maxY;

	void Start() {
		RectTransform rt = myPanel.GetComponent<RectTransform>();
		minX = rt.offsetMin.x;
		maxX = rt.offsetMax.x;
		minY = rt.offsetMin.y;
		maxY = rt.offsetMax.y;
	}

	/// <summary>
	/// Flags this menu as enabled for rendering in CC_Manager
	/// </summary>
	public void SetEnabled(bool menuEnabled) {
		this.menuEnabled = menuEnabled;
	}

	/// <summary>
	/// Starts the render, should never be called except by CC_Manager
	/// Is called during the UpdateMenus() function
	/// </summary>
	public void StartRender() {
		myPanel.gameObject.SetActive(true);
	}

	/// <summary>
	/// Toggles whether or not this menu is enabled for rendering in CC_Manager
	/// </summary>
	public void ToggleEnabled() {
		menuEnabled = !myPanel.gameObject.activeSelf;
	}

	/// <summary>
	/// Stops the render, should never be called except by CC_Manager
	/// Is called during the UpdateMenus() function
	/// </summary>
	public void StopRender() {
		myPanel.gameObject.SetActive(false);
	}

	public bool isEnabled() {
		return menuEnabled;
	}

	public string GetName() {
		return myPanel.name;
	}

	/// <summary>
	/// Sets the position of the menu based on an index, so menus don't overlap or skip space
	/// </summary>
	public void SetPosition(int index) {
		RectTransform rt = myPanel.GetComponent<RectTransform> ();
		rt.offsetMin = new Vector2 (minX, Screen.height - (menuSize*(index+1)));
		rt.offsetMax = new Vector2 (minX, maxY - (menuSize*index));
	}

	/// <summary>
	/// Initialize the CC_Menu, pass a panelPrefab that the CC_Menu will use as a parent to the buttons
	/// pass a list of CC_Manager.CCStates that will determine when this menu can be rendered
	/// and a menu name that can be referenced in CC_Manager.GetMenuByName()
	/// </summary>
	public void Initialize(GameObject panelPrefab, CC_Manager.CCState[] states, string menuName) {
		GameObject obj = Instantiate(panelPrefab) as GameObject;
		myPanel = obj.transform;		
		myPanel.transform.SetParent(UIManager._instance.GetMyCanvas(0), false);
		myPanel.name = menuName;
		foreach (CC_Manager.CCState s in states)
			renderStates.Add(s);
	}

	/// <summary>
	/// Adds a button to this menu with a function callback and a button text
	/// </summary>
	public void AddButton(GameObject buttonPrefab, UnityAction callback, string buttonText) {
		GameObject obj = Instantiate(buttonPrefab) as GameObject;
		//obj.AddComponent<LayoutElement>();
		Button b = obj.GetComponent<Button>();
		b.name = buttonText;
		b.GetComponentInChildren<Text>().text = buttonText;
		obj.transform.SetParent(myPanel, false);
		b.GetComponent<LayoutElement>().minWidth = 10*buttonText.Length;
		b.GetComponent<LayoutElement>().minHeight = menuSize - 10;
		b.onClick.AddListener(callback);
	}

	public void AddButton(UnityAction callback, string buttonText) {
		AddButton(buttonPrefab, callback, buttonText);
	}
}
