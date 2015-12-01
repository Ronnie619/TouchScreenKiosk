using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIWindow : MonoBehaviour {

	//Need a temp linked windows for proper data saving to allow reverting changes, or we need to switch to storing a serialized object holding the data
	public List<UIWindow> linkedWindows = new List<UIWindow>();
	public List<UIWindow> tempLinkedWindows = new List<UIWindow>();
	public List<Button> buttons = new List<Button>();
	public Button backButton;

	void Start () {
		bool throwError = false;
		int count = 0;
		foreach (Button b in buttons) {
			if (b != null) count++;
		}
		Button[] buttonComponents = transform.GetComponentsInChildren<Button>();
		int buttonCount = buttonComponents.Length;

		if (buttonCount != linkedWindows.Count+1) { //First error if there cant be the right amount of buttons
			Debug.LogFormat("The number of buttons on " + name + ": ({0}) does not match the number of transitions needed: ({1})", buttonCount, linkedWindows.Count+1);
			throwError = true;
		} else {
			//Then check errors for having too many (shouldnt happen) or too few buttons assigned
			if (count != linkedWindows.Count) {
				Debug.LogFormat("Too {0} buttons on " + name + " assigned to transitions", count < linkedWindows.Count ? "few" : "many");
				throwError = true;
			}
			if (backButton == null) {
				Debug.Log("No back button has been assigned to " + name);
				throwError = true;
			}
		}

		if (throwError) //Don't proceed until previous errors have been fixed
			return;

		for (int i = 0; i < buttons.Count; i++) {
			Button b = buttons [i];
			for (int j = 0; j < buttons.Count; j++) {
				Button c = buttons [j];
				if (b == backButton && j == 0) //J == 0 to only check once
					Debug.LogFormat ("Button {0} has been assigned to {1} and the back button", b.name, linkedWindows[i]);
				if (i==j || i > j) continue; //If they are duplicates, or if we already did that match (i > j)
				if (b == c) 
					Debug.LogFormat ("Button {0} has been assigned to multiple transitions {1} and {2}", b.name, linkedWindows[i], linkedWindows[j]);

			}
		}
	}

	void Update () {
		
	}
}
