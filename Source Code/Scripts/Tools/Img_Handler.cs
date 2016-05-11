using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class Img_Handler : MonoBehaviour {
	public string url;
	public string imgname;
	public string path;
	public Texture2D ImgDisplay;
	public bool display;
	public Texture img;
	public string info;
	GUIStyle style;
	public string temp;
	public static bool IsCalled;
	public GameObject ImgMenu;
	public GameObject DisplayImage;
	public Text tempurl;
	public Text tempfile;
	public Text temppath;
	public Text tempsystem;
	public Image myImageComponent;
	public Text urlText;
	public Text nameText;
	public Image currImageElement;
	public InputField urlInput;
	public InputField nameInput;

	public void Download_Button(){
		if(String.IsNullOrEmpty(url) || String.IsNullOrEmpty(imgname)){
			info = "Enter a valid Url and File Name";
		}else{
			StartCoroutine(Img_Downloader());
			path = (Application.persistentDataPath + "/" + imgname + ".jpg");
		}
	}

	public IEnumerator Img_Downloader(){
		if(File.Exists(Application.persistentDataPath + "/" + imgname + ".jpg")){
			info = "File already exist.";
		}else{
			info = "Downloading...";
			WWW www = new WWW(url);
			yield return www;
			Texture2D img = www.texture;
			byte[] bytes = img.EncodeToJPG();
			File.WriteAllBytes(Application.persistentDataPath + "/" + imgname + ".jpg", bytes);
			if(File.Exists(Application.persistentDataPath + "/" + imgname + ".jpg")){
				info = "Download successful!";
		 	}
		}
	}

	public void Img_Display() {
		if (SetImg(myImageComponent)) {
			info = "Displaying Image";
		} 
    }

	public void Img_Set() {
		if (SetImg(currImageElement)) {
			info = "Image Set Succesfully";
			if (currImageElement.GetComponent<ImageData>())
				currImageElement.GetComponent<ImageData>().path = Application.persistentDataPath + "/" + imgname + ".jpg";
			else if (currImageElement.GetComponent<ButtonData>())
				currImageElement.GetComponent<ButtonData>().imagePath = Application.persistentDataPath + "/" + imgname + ".jpg";
		} 
	}

	bool SetImg(Image i) {
		if((File.Exists(Application.persistentDataPath + "/" + imgname + ".jpg")) == false){
			info = "Enter existing image name";
		} else {
			info = "Displaying image.";
			display = true;
			DisplayImage.SetActive(true);
			myImageComponent = GameObject.Find("Display_Image").GetComponent<Image>();
			
			byte[] bytearray = File.ReadAllBytes(Application.persistentDataPath + "/" + imgname + ".jpg");
			ImgDisplay = new Texture2D(8, 8);
			ImgDisplay.LoadImage(bytearray);
			//            this.GetComponent<Renderer>().material.mainTexture = ImgDisplay;
			
			float width = ImgDisplay.width;
			float height = ImgDisplay.height;
			float whratio = width/height;
			float hwratio = height/width;
			//print(ImgDisplay.width);
			//print(ImgDisplay.height);
			//print(whratio);
			
			Sprite imgsprite = 	Sprite.Create(ImgDisplay,new Rect(0, 0, ImgDisplay.width, ImgDisplay.height), new Vector2(0.5f, 0.5f) );

			i.sprite = imgsprite;
			if (ImgDisplay.width > ImgDisplay.height) {
				i.rectTransform.sizeDelta = new Vector2(200, hwratio*200);
			} else {
				i.rectTransform.sizeDelta = new Vector2(whratio*200, 200);
			}

			if (currImageElement.GetComponent<ImageData>()){
				currImageElement.GetComponent<ImageData>().x = i.rectTransform.sizeDelta.x;
				currImageElement.GetComponent<ImageData>().y = i.rectTransform.sizeDelta.y;
			}
			


			return true;
		}
		return false;
	}

	public static void SetImage(string path, GameObject imageObj, float x, float y) {
		if(string.IsNullOrEmpty(path)) {
			return;
		} else if (File.Exists(path)) {
			Image i = imageObj.GetComponent<Image>();
			Texture2D img = new Texture2D(8,8);
			byte[] bytearray = File.ReadAllBytes(path);
			img.LoadImage(bytearray);
			Sprite sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), new Vector2(0.5f, 0.5f));
			i.sprite = sprite;
			i.rectTransform.sizeDelta = new Vector2(x, y);
		} else {
			Debug.LogError("SERIALIZATION ERROR: No image found at path " + path);
		}
	}

	void Update(){
		if(IsCalled) {
			url = urlInput.text;
			imgname = nameText.text;
			if (!ImgMenu.activeSelf)
				ImgMenu.SetActive(true);
			tempurl.text = "Url: ";
			tempfile.text = "File name: ";
			temppath.text = "File path: ";
			tempsystem.text = "System info: ";
		} else {
			if (ImgMenu.activeSelf)
				ImgMenu.SetActive(false);
		}

		if(display) {
			if (!DisplayImage.activeSelf)
				DisplayImage.SetActive(true);
		} else {
			if (DisplayImage.activeSelf)
				DisplayImage.SetActive(false);
		}

		if (url != "") { tempurl.text = ("Url: " + url);}
		if (imgname != "") { tempfile.text = ("File name: " + imgname + ".jpg");}
		if (path != "") { temppath.text = ("File path: " + path);}
		if (info != "") { tempsystem.text = ("System info: " + info);}
	}

	public void BackButton(){
		IsCalled = false;
		display = false;

		CC_UISelectionManager temp1 = new CC_UISelectionManager();
		temp1.SetIMGPath(imgname);
	}

	public void Enable(GameObject selected) {
		currImageElement = selected.GetComponent<Image>();
		if (currImageElement == null) {
			Debug.LogError("Trying to run ImageHandler on a none image UI Element, Stopping");
			return;
		}
		IsCalled = true;
	}

	/// <summary>
	/// Disable the image handler
	/// </summary>
	public void Disable() {
		urlInput.text = "";
		nameInput.text = "";
		//currImageElement = null;
		IsCalled = false;
	}
/*
	void OnGUI(){
	if(IsCalled == true){		style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        if(imgname != ""){
        	temp = (imgname + ".jpg");
        }
        GUILayout.BeginArea(new Rect(40, 250, 100, 100), "Url: " +url, style);
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(40, 268, 100, 100), "File name: " + temp, style);
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(40, 286, 100, 100), "File path: " + path, style);
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(40, 304, 100, 100), "System info: " + info, style);
        GUILayout.EndArea();

		if(display == true){
        GUILayout.BeginArea(new Rect(600, 150, 600, 300), ImgDisplay);
        GUILayout.EndArea();
//      display = !display;
    	}
	}
	}
*/
/*
	public IEnumerator Img_Downloader(){
		if (File.Exists(Application.persistentDataPath + name))
        {
            print("Load from device");
            byte[] bytearray = File.ReadAllBytes(Application.persistentDataPath + name);
            Texture2D imgD = new Texture2D(8, 8);
            imgD.LoadImage(bytearray);
            this.GetComponent<Renderer>().material.mainTexture = imgD;
        }
        else {
            print("DOwnload from website");
            WWW www = new WWW(url);
            yield return www;
            Texture2D img = www.texture;
            this.GetComponent<Renderer>().material.mainTexture = img;
            byte[] bytes = img.EncodeToJPG();
            File.WriteAllBytes(Application.persistentDataPath + name +".jpg", bytes);
        }
	}
*/
}
