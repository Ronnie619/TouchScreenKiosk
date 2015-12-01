using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIWindow : MonoBehaviour {

	public List<UIWindow> linkedWindows = new List<UIWindow>();
	private List<Button> buttons = new List<Button>();

	void Start () {
		foreach (Button b in transform.GetComponentsInChildren<Button>()) {
			buttons.Add(b);
		}
		if (buttons.Count > linkedWindows.Count) {
			Debug.Log("Too many buttons on UIWindow for number of transitions");
		} else if (buttons.Count < linkedWindows.Count) {
			Debug.Log("Too few buttons on UIWindow for number of transitions");
		}
	}

	void Update () {
		
	}
}
