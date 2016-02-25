//UIManager.cs
//Author: Morgan Holbart
//UIManager is the overarching manager of all UI related actions
//This includes Content Creation, Content Saving and Loading, and Content Viewing
//Currently very baseline, has a bit of work to be done as we expand
//No state machine currently in place, or any sort of functionality, currently only runs
//the CC_Manager 

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

	public static UIManager _instance;
	public GameObject mainCanvas;
	public CC_UISelectionManager elementEditor;
	
	void Awake() {
		if (_instance != null) {
			Destroy (_instance);
			_instance = this;
		} else _instance = this;
	}


	void Start() {
		CC_Manager._instance.enabled = false; //Start the CC manager off
		if (elementEditor == null)
			elementEditor = GetComponent<CC_UISelectionManager>();

	}

	void Update() {
		if (Input.GetButtonDown("EditMode")) { //Set desired input here or change it in InputManager
			CC_Manager._instance.enabled = !CC_Manager._instance.enabled;
			CC_UISelectionManager._instance.enabled = CC_Manager._instance.enabled;
		}
	}

	/// <summary>
	/// Gets a canvas based on user index, currently returns the main canvas
	/// TODO: return canvas based on index once we have multiple canvas running
	/// for multiple users
	/// </summary>
	public Transform GetMyCanvas(int i) {
		return mainCanvas.transform;
	}
}
