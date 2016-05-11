using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using UnityEngine.Events;
using System.Collections.Generic;

public class Url_handler : MonoBehaviour {
	public string get_url;

    public static bool IsCalled;
    public GameObject UrlSettingMenu;
	public Button currButton;
	public Text	currButtonText;
	public GameObject temp;
//	public float X, Y, Width, Height;
	public static UWKWebView setting;
	public static Web_GUI webGUI;
	public WebData webData;
	public Slider x_slider, y_slider, width_slider, height_slider;
	public Text X, Y, Width, Height;
	public InputField url, youtube;
	bool isclicked = false;	
	public GameObject buttonPrefab;
	public GameObject webPrefab;
	public CC_Manager CC_Manager;
	public GameObject ccmanager;
	public List<GameObject> allUIElements;


	public void url_input(string input){ get_url = input;}
	public void youtube_input(string input){ get_url = "http://www.youtube.com/embed/" + input;}
	public void X_input(float i){ webGUI.X = i;}
	public void Y_input(float i){ webGUI.Y = i;}
	public void Width_input(float i){ setting.CurrentWidth = (int)i;}	
	public void Height_input(float i){ setting.CurrentHeight = (int)i;}


	public void set_real_url(){
		if(get_url != "" && get_url != "http://www.youtube.com/embed/"){
			print (get_url);
			if(currButton == null){
				//ccmanager = GameObject.Find("Managers");
				//CC_Manager = ccmanager.GetComponent<CC_Manager>();
				allUIElements = CC_Manager.allUIElements;
				GameObject button = Instantiate(buttonPrefab) as GameObject;
				button.transform.SetParent(UIManager._instance.GetMyCanvas(0), false);
				button.GetComponent<Button>().interactable = false;
				allUIElements.Add(button);

				currButton = button.GetComponent<Button>();
				currButtonText = button.GetComponentInChildren<Text>();
			}
			currButton.onClick.AddListener(() => OpenURL(get_url));
			AddListener(currButton,get_url);
			currButtonText.text = get_url;
		}
	}
	
	void OpenURL(string url){ 
		temp = new GameObject ();
		temp.AddComponent<Web_GUI> ();
		temp.AddComponent<UWKWebView> ();
		setting = temp.GetComponent<UWKWebView> ();
		setting.MaxWidth = 1920;
		setting.MaxHeight = 1920;
		setting.URL = url;
	}
	public void update_url(){ get_url = url.text;}
	public void update_youtube(){ get_url = "http://www.youtube.com/embed/" + youtube.text;}


	public void set_In_Current_Page(){
		if(get_url != "" && get_url != "http://www.youtube.com/embed/"){
			isclicked = true;

				allUIElements = CC_Manager.allUIElements;
				GameObject web = Instantiate(webPrefab) as GameObject;
				web.transform.SetParent(UIManager._instance.GetMyCanvas(0), false);
				webGUI = web.AddComponent<Web_GUI> ();
				setting = web.AddComponent<UWKWebView> ();
				webData = web.AddComponent<WebData>();
				setting.URL = get_url;
                setting.MaxWidth = 1920;
                allUIElements.Add(web);

				x_slider.value = webGUI.X;
				y_slider.value = webGUI.Y;
				width_slider.value = setting.CurrentWidth;
				height_slider.value = setting.CurrentHeight;
				webData.url = get_url;
		}
	}


	public static void setWeb(GameObject temp, string url, int x, int y, int width, int height){
		webGUI = temp.AddComponent<Web_GUI> ();
		setting = temp.AddComponent<UWKWebView> ();
		temp.AddComponent<WebData>();
		temp.AddComponent<CC_Selectable>();

        setting.MaxWidth = 1920;
        setting.URL = url;
		setting.CurrentWidth = width;
		setting.CurrentHeight = height;

		webGUI.X = x;
		webGUI.Y = y;
	}



	public void setWeb1(GameObject temp,string url, int x, int y, int width, int height){
		get_url = url;

		set_In_Current_Page();
	}

	public void remove_Website(){
		Destroy(temp);
		if(currButton != null){
			currButton.onClick.RemoveAllListeners();
			currButtonText.text = "Button";
		}
	}


	void AddListener(Button b, string value) {
		b.onClick.AddListener(delegate{ OpenURL(value);});
	}




    public void callurlhandler(){ IsCalled = true;}

	public void Disable(){
		IsCalled = false;
		isclicked = false;
	}


/*
	public void AssignURL(){
		mybutton = selected.GetComponent<Button>();
		mybutton.onClick.AddListener(OpenURL);
		Text aButton = selected.GetComponentInChildren<Text>();
		aButton.text = url;
	}
*/

	public void Enable(GameObject selected){
		currButton = selected.GetComponent<Button>();
		currButtonText = selected.GetComponentInChildren<Text>();
		IsCalled = true;

	}






    void Update(){
        if(IsCalled){
            UrlSettingMenu.SetActive(true);
            if(isclicked){ 
            	X.text = webGUI.X.ToString();
				Y.text = webGUI.Y.ToString();
				webData.x = (int)webGUI.X;
				webData.y = (int)webGUI.Y;

            	Width.text = setting.CurrentWidth.ToString();
            	Height.text = setting.CurrentHeight.ToString();
            	webData.width = (int)setting.CurrentWidth;
            	webData.height = (int)setting.CurrentHeight;
            }
        }else{
            UrlSettingMenu.SetActive(false);
        }
//		if(X != null){ X.text = setting.CurrentWidth.ToString();}
/*
        if(Input.GetKeyDown(KeyCode.F4)){
            IsCalled = !IsCalled;
        }
*/
    }

}
