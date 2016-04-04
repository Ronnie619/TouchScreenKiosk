//Vector3Helper.cs
//Author: Morgan Holbart
//Used for XML serialization with Vector3 and Vector4 so we can get to and from the serialized string
//Since we cannot deserialize them from a string without help

using UnityEngine;
using System.Collections;

public static class Vector3Helper
{
	
	public static Vector3 FromString(this Vector3 vector, string value){
		string[] temp = value.Replace(" ", "").Split(',');
		vector.x = float.Parse(temp[0]);
		vector.y = float.Parse(temp[1]);
		vector.z = float.Parse(temp[2]);
		
		return vector;
	}

	public static Vector4 FromString(this Vector4 vector, string value) {
		string[] temp = value.Replace(" ", "").Split(',');
		vector.x = float.Parse(temp[0]);
		vector.y = float.Parse(temp[1]);
		vector.z = float.Parse(temp[2]);
		vector.w = float.Parse(temp[3]);
		
		return vector;
	}
}
