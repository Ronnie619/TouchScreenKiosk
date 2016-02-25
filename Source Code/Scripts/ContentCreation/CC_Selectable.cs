//CC_Selectable.cs
//Author: Morgan Holbart
//CC_Selectable is a custom selection script added to every UI element we instantiate to be viewed
//This is required because we cannot use selectable itself as it is needed for default button and UI functionality
//Instead we will disable any existing selectables during creation mode, and use CC_Selectable instead
//CC_Selectable has a static list of all selectable objects (which happens to be all UI objects on the screen) and
//The currently selected CC_Selectable

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CC_Selectable : MonoBehaviour {

	public static GameObject selected;										//Currently selected gameobject
	public static List<GameObject> selectables = new List<GameObject>();	//List of all selectable objects

	//Colors used for transitioning between selected and not selected
	Color startColor;			//Start color of a selectable
	public Color endColor;		//End color of a selectable

	/// <summary>
	/// Start will get our image and update what our default color is, and add ourselves to the list of objects
	/// </summary>
	void Start() {
		Image i = GetComponent<Image>();
		if (i != null)
			startColor = i.color;

		if (!selectables.Contains(gameObject))
			selectables.Add(gameObject);
	}

	/// <summary>
	/// Sets whether we are in creation mode or not, which will handle if the UIElement already uses Selectable
	/// and also enable or disable this script, this is called when Creation Mode is enabled or disabled in CC_Manager
	/// </summary>
	public void SetCreationMode(bool b) {
		Button button = GetComponent<Button>();
		if (button != null) {
			button.interactable = !b;
		}

		enabled = b;
	}

	/// <summary>
	/// CC_Selectable uses an EventTrigger and OnMouseDown will call this function
	/// </summary>
	public void OnMouseDown() {

		if (!enabled)
			return;

		if (selected == gameObject) //No reason to reselect the same object
			return;

		if (selected != null) //If we had an object, deselect it
			selected.GetComponent<CC_Selectable>().Deselect();

		Select(); //Select new object, because this is only called OnMouseDown with a CC_Selectable we can assume we havent selected anything invalid
	}

	/// <summary>
	/// Static function allows us to cancel selection, in case of a state change or escape keybind
	/// </summary>
	public static void CancelSelect() {
		if (selected == null)
			return;

		selected.GetComponent<CC_Selectable>().Deselect();
	}

	/// <summary>
	/// Deselect this CC_Selectable and begin deselection animation
	/// </summary>
	public void Deselect() {
		StopCoroutine(SelectAnimation());
		StartCoroutine("DeselectAnimation");
		selected = null;
	}

	/// <summary>
	/// Select this instance and begin the selection animation
	/// </summary>
	public void Select() {
		StartCoroutine("SelectAnimation");
		selected = gameObject;
	}

	/// <summary>
	/// Starts the deselection animation, currently just does it instantly 
	/// </summary>
	IEnumerator DeselectAnimation() {
		Image i = GetComponent<Image>();

		if (i == null)
			yield break;

		i.color = startColor;

		yield return null;
	}

	/// <summary>
	/// Starts the selection animation which will lerp the color between the startColor and endColor
	/// </summary>
	IEnumerator SelectAnimation() {
		Image i = GetComponent<Image>();

		if (i == null)
			yield break;

		Color temp = startColor;
		float selectTime = .08f;

		float t = 0;
		while (t < selectTime) {
			temp = Color.Lerp(startColor, endColor, t/selectTime);
			i.color = temp;
			yield return null;
			t+=Time.deltaTime;
		}
	}
}
