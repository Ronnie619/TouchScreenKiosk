//TextData.cs
//Author: Morgan Holbart
//Class given to a text for saving and loading the text data with our serializable text data

using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class TextData : MonoBehaviour {

	TextDataSerializable data;

	public TextDataSerializable GetDataSerializable() {
		TextDataSerializable bd = new TextDataSerializable();
		bd.posData = transform.position;
		bd.rotData = transform.rotation.eulerAngles;
		bd.scaleData = transform.localScale;
		
		return bd;
	}
	
	//Initialize the image given the serialized data
	public void Initialize(TextDataSerializable data, int index) {
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
	}

	[XmlRoot("TextData")]
	public class TextDataSerializable : SerializationData {
		
		
	}
}
