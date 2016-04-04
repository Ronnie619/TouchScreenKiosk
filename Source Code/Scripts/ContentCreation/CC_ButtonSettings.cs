//TODO check on creation that transition file still exists, so if a file is deleted between runs
//The buttons transition will be reset to 0

//CC_ButtonSettings.cs
//Author: Morgan Holbart
//ButtonSettings menu for giving buttons functionality
//Any additional button functionality needed should be done so in here

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CC_ButtonSettings : MonoBehaviour {

	public static CC_ButtonSettings _instance;
	public GameObject ButtonSettingsCanvas;
	public GameObject TransitionButton;
	public GameObject ExitButton;
	public Text TransitionButtonText;
	public Text TransitionID;
	public InputField IDInput;
	public GameObject manualIDSetButton;
	bool running;

	ButtonData currButtonData;

	void Awake() {
		if (_instance != null) {
			Destroy(_instance);
			Debug.LogError("Multiple instances of CC_ButtonSettings created");
		} 
		_instance = this;
	}

	/// <summary>
	/// Disable the ButtonSettings
	/// </summary>
	public void Disable() {
		running = false;
		ButtonSettingsCanvas.SetActive(false);
		currButtonData = null;
	}

	/// <summary>
	/// Called by clicking the TransitionButton from the menu
	/// </summary>
	public void OnClick_TransitionButton() {
		int id = currButtonData.GetTransitionWindowID();
		if (id == 0 || !UIWindow.WindowExists(currButtonData.GetTransitionWindowID())) {
			currButtonData.SetTransitionWindowID(UIManager._instance.CreateNewWindowID());
			id = currButtonData.GetTransitionWindowID();
			TransitionID.text = "Curr Transition ID: " + id;
			TransitionButton.GetComponentInChildren<Text>().text = "GoTo Window " + id;
		} else {
			//Todo: ask to save first
			UIManager._instance.TransitionToWindow(id, 0);
		}
	}

	/// <summary>
	/// Called by clicking the SetID button from the menu
	/// </summary>
	public void OnClick_ManualIDSetButton() {
		if (IDInput.text.Length == 0) {
			return;
		} else {
			currButtonData.SetTransitionWindowID(int.Parse(IDInput.text));
			TransitionID.text = "Curr Transition ID: " + currButtonData.GetTransitionWindowID();
			if (UIWindow.WindowExists(currButtonData.GetTransitionWindowID())) {
				TransitionButton.GetComponentInChildren<Text>().text = "GoTo Window " + currButtonData.GetTransitionWindowID();
			} else TransitionButton.GetComponentInChildren<Text>().text = "Create Window";
		}
	}

	/// <summary>
	/// Called by clicking the SetBack button from the menu
	/// </summary>
	public void OnClick_SetBackButton() {
		currButtonData.SetTransitionWindowID(-2);
		TransitionID.text = "Curr Transition ID: Back";
		TransitionButton.GetComponentInChildren<Text>().text = "Go Back";
	}

	/// <summary>
	/// Called from the Exit button to leave the menu
	/// </summary>
	public void OnClick_ExitButton() {
		Disable();
	}

	/// <summary>
	/// Toggles whether this menu is on or off
	/// </summary>
	public void Toggle(ButtonData data) {
		currButtonData = data;
		int id = currButtonData.GetTransitionWindowID();
		if (id == 0) 
			TransitionID.text = "Curr Transition ID: None";
		else TransitionID.text = "Curr Transition ID: " + id;

		if (id == 0) {
			TransitionButtonText.text = "Create Window";
		} else {
			TransitionButtonText.text = "GoTo Window " + id;
		}
		IDInput.text = string.Empty;

		running = !running;
		ButtonSettingsCanvas.SetActive(running);
	}
}
