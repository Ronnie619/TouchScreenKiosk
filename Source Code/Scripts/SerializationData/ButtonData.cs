//ButtonData.cs
//Author: Morgan Holbart
//Class given to a button for saving and loading the button data with our serializable button data

using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class ButtonData : MonoBehaviour {

	ButtonDataSerializable data;
	int TransitionWindowID = 0;
	public string imagePath;

	public int GetTransitionWindowID() {
		return TransitionWindowID;
	}

	public void SetTransitionWindowID(int id) {
		TransitionWindowID = id;
	}

	public void OnClick_Button() {
		if (TransitionWindowID != 0) {
			UIManager._instance.TransitionToWindow(TransitionWindowID, GetComponent<CC_Selectable>().myCanvas);
		}
	}

	public ButtonDataSerializable GetDataSerializable() {
		ButtonDataSerializable bd = new ButtonDataSerializable();
		bd.posData = transform.position;
		bd.rotData = transform.rotation.eulerAngles;
		bd.scaleData = transform.localScale;
		bd.TransitionWindowID = TransitionWindowID;
		bd.imagePath = imagePath;
		
		return bd;
	}

	//Initialize the image given the serialized data
	public void Initialize(ButtonDataSerializable data, int index) {
		CC_Selectable.CreateUIElement(gameObject);
		GetComponent<CC_Selectable>().myCanvas = index;
		this.data = data;
		StartCoroutine("Init");		
	}

	IEnumerator Init() {
		yield return null;

		transform.SetParent(UIManager._instance.GetMyCanvas(GetComponent<CC_Selectable>().myCanvas).transform, false);
		transform.position = data.posData;
		transform.rotation = Quaternion.Euler(data.rotData);
		transform.localScale = data.scaleData;
		TransitionWindowID = data.TransitionWindowID;
		imagePath = data.imagePath;
	}

	[XmlRoot("ButtonData")]
	public class ButtonDataSerializable : SerializationData {

		[XmlAttribute("imagepath")]
		public string imagePath;

		[XmlAttribute("TransitionWindowID")]
		public int TransitionWindowID;
	}
}
