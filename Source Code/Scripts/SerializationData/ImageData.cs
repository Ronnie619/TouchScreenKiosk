//ImageData.cs
//Author: Morgan Holbart
//Class given to a image for saving and loading the image data with our serializable image data

using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class ImageData : MonoBehaviour {

	ImageDataSerializable data;
	public string path = "";

	public ImageDataSerializable GetDataSerializable() {
		ImageDataSerializable sd = new ImageDataSerializable();
		sd.path = path;
		sd.posData = transform.position;
		sd.rotData = transform.rotation.eulerAngles;
		sd.scaleData = transform.localScale;

		return sd;
	}

	//Initialize the image given the serialized data
	public void Initialize(ImageDataSerializable data, int index) {
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
		path = data.path;
		Img_Handler.SetImage(path, this.gameObject);
	}

	[XmlRoot("ImageData")]
	public class ImageDataSerializable : SerializationData {

		[XmlAttribute("imgpath")]
		public string path;

	}
}
