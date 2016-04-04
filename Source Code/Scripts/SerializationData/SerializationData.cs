//SerializationData.cs
//Author: Morgan Holbart
//Base class for any info that is shared between UI elements
//Extend this class for any new UI element with new serialized data

using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

[XmlRoot("UIElementData")]
public abstract class SerializationData  {


	public Vector3 posData;
	[XmlAttribute("position")]
	string posSerializable {
		get { return posData.ToString(); }
		set { 
			posData = new Vector3().FromString(value);
		}
	}
	
	public Vector3 scaleData;
	[XmlAttribute("scale")]
	string scaleSerializable {
		get { return scaleData.ToString(); }
		set { 
			scaleData = new Vector3().FromString(value);
		}
	}
	
	public Vector3 rotData;
	[XmlAttribute("rotation")]
	string rotSerializable {
		get { return rotData.ToString(); }
		set { 
			rotData = new Vector3().FromString(value);
		}
	}

}
