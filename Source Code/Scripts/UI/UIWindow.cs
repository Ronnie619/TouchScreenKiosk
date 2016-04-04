//UIWindow.cs
//Author: Morgan Holbart
//Holds the functionality to load and save a window using XML serialization
//To use, create a new UIWindow and UIWindow.LoadWindow(windowID)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;

public class UIWindow {

	WindowData myData; //The WindowData of this window, can only retrieve by calling LoadWindow to ensure up to date info

	/// <summary>
	/// Gets all images in the scene and add to myData
	/// </summary>
	void GetAllImages() {
		Transform parent = UIManager._instance.GetMyCanvas(0);

		for (int i = 0; i < parent.childCount; i++) {
			Transform child = parent.GetChild(i);
			if (child.GetComponent<ImageData>()) {
				myData.images.Add(child.GetComponent<ImageData>().GetDataSerializable());
			}
		}
	}

	/// <summary>
	/// Gets all buttons in the scene and add to myData
	/// </summary>
	void GetAllButtons() {
		Transform parent = UIManager._instance.GetMyCanvas(0);
		
		for (int i = 0; i < parent.childCount; i++) {
			Transform child = parent.GetChild(i);
			if (child.GetComponent<ButtonData>()) {
				myData.buttons.Add(child.GetComponent<ButtonData>().GetDataSerializable());
			}
		}
	}

	/// <summary>
	/// Gets all texts in the scene and add to myData
	/// </summary>
	void GetAllTexts() {
		Transform parent = UIManager._instance.GetMyCanvas(0);
		
		for (int i = 0; i < parent.childCount; i++) {
			Transform child = parent.GetChild(i);
			if (child.GetComponent<TextData>()) {
				myData.texts.Add(child.GetComponent<TextData>().GetDataSerializable());
			}
		}
	}
	
	void UpdateAllWindowObjects() {
		GetAllImages();
		GetAllButtons();
		GetAllTexts();
		myData.BackgroundColor = Background_Handler.GetCurrColor();
	}

	/// <summary>
	/// Creates a new window ID that is not in use
	/// </summary>
	public static int CreateNewWindowID() {
		int windowCount = 1;
		while (windowCount < 100000) {
			if (!File.Exists(Application.persistentDataPath + "/Windows/Window" + windowCount + ".txt")) {
				break;
			}
			else windowCount++;
		}

		UIWindow w = new UIWindow();
		w.CreateNewWindow(windowCount);
		return windowCount;
	}

	/// <summary>
	/// Check if a window exists with the given id
	/// </summary>
	public static bool WindowExists(int id) {
		if (!File.Exists(Application.persistentDataPath + "/Windows/Window" + id + ".txt")) {
			return false;
		}
		else return true;
	}

	/// <summary>
	/// Creates a new window with the given window ID and the current window UI
	/// </summary>
	public void CreateNewWindow(int windowID) {
		if (!Directory.Exists(Application.persistentDataPath + "/Windows")) {
			Directory.CreateDirectory(Application.persistentDataPath + "/Windows/");
		}
		
		string path = Application.persistentDataPath;
		if (windowID == -1) 
			path += "/Windows/BaseWindow.txt";	
		else
			path += "/Windows/Window" + windowID.ToString() + ".txt";
		try {
			myData = new WindowData();
			myData.windowID = windowID;
			
			using (FileStream fs = new FileStream(path, FileMode.Create)) {
				XmlSerializer xmls = new XmlSerializer(typeof(WindowData));
				xmls.Serialize(fs, myData);
				fs.Close();
			}
			
			Debug.Log ("Succesfully wrote UIWindow to " + path);
		} catch (InvalidOperationException e) {
			Debug.LogError ("Failed to write UIWindow to " + path + "- Error: " + e.Message);
		}
	}

	/// <summary>
	/// Saves the window with the passed window ID
	/// </summary>
	public void SaveWindow(int windowID) {
		if (!Directory.Exists(Application.persistentDataPath + "/Windows")) {
			Directory.CreateDirectory(Application.persistentDataPath + "/Windows/");
		}

		string path = Application.persistentDataPath;
		if (windowID == -1) 
			path += "/Windows/BaseWindow.txt";	
		else
			path += "/Windows/Window" + windowID.ToString() + ".txt";
		try {
			myData = new WindowData();
			UpdateAllWindowObjects();
			myData.windowID = windowID;

			using (FileStream fs = new FileStream(path, FileMode.Create)) {
				XmlSerializer xmls = new XmlSerializer(typeof(WindowData));
				xmls.Serialize(fs, myData);
				fs.Close();
			}

			Debug.Log ("Succesfully wrote UIWindow to " + path);
		} catch (InvalidOperationException e) {
			Debug.LogError ("Failed to write UIWindow to " + path + "- Error: " + e.Message);
		}
	}

	/// <summary>
	/// Saves the window, default to -1 (basewindow)
	/// </summary>
	public void SaveWindow() {
		SaveWindow(-1);
	}

	/// <summary>
	/// Loads the window from the windowID.
	/// Sets the WindowData to UIWindow and returns it
	/// </summary>
	public WindowData LoadWindow(int windowID) {
		if (!Directory.Exists(Application.persistentDataPath + "/Windows")) {
			Directory.CreateDirectory(Application.persistentDataPath + "/Windows/");
		}
		string path = Application.persistentDataPath;

		if (!File.Exists(path + "/Windows/BaseWindow.txt")) {
			SaveWindow(-1);
		}

		try {

			if (windowID == -1) 
				path += "/Windows/BaseWindow.txt";
			else
				path += "/Windows/Window" + windowID + ".txt";
			
			myData = new WindowData();
			XmlSerializer xmls = new XmlSerializer(typeof(WindowData));

			using (FileStream fs = new FileStream(path, FileMode.Open)) {
				myData = xmls.Deserialize(fs) as WindowData;
				fs.Close();
			}
			Debug.Log("Succesfully Loaded window from path: " + path);
			
			return myData;
		} catch (InvalidOperationException e) {
			Debug.LogError ("Failed to load UIWindow from " + path + " - Error: " + e.Message);
			return null;
		}
	}

	/// <summary>
	/// Loads the window id at default -1 (basewindow)
	/// </summary>
	public WindowData LoadWindow() {
		return LoadWindow(-1);
	}

	[XmlRoot("WindowData")]
	public class WindowData {
		[XmlElement("ID")]
		//[XmlAttribute("ID")]
		public int windowID;

		public Vector4 BackgroundColor;
		[XmlAttribute("BGColor")]
		string posSerializable {
			get { return BackgroundColor.ToString(); }
			set { 
				BackgroundColor = new Vector4().FromString(value);
			}
		}

		[XmlArray("images"),XmlArrayItem("image")]
		public List<ImageData.ImageDataSerializable> images = new List<ImageData.ImageDataSerializable>();

		[XmlArray("buttons"),XmlArrayItem("button")]
		public List<ButtonData.ButtonDataSerializable> buttons = new List<ButtonData.ButtonDataSerializable>();

		[XmlArray("texts"),XmlArrayItem("text")]
		public List<TextData.TextDataSerializable> texts = new List<TextData.TextDataSerializable>();
	}
}
