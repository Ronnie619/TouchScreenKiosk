using UnityEngine;
using System.Collections;
using UnityEngine.UI; 
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Text_Setting : MonoBehaviour {
	public bool IsCalled = false;
	public Text currText;
	public Dropdown SizeDropdown;
	public Dropdown FontDropdown;
	public Dropdown ColorDropdown;
	public Font Times;
	public Font Arial;
	public Font Verdana;
	public InputField inputField;
	public string temp;
    public RectTransform rect;
    public float width;
    public Slider widthSlider;

    public void setWidth(float input) { this.width = input; }

    public GameObject TextSettingMenu;

	public void myDropdownValueChangedHandler(Dropdown target){
		textSize(target.value);
	}
	public void myFontDropdownValueChangedHandler(Dropdown target){
		setFont(target.value);
	}

	void Start(){
		SizeDropdown.value = 7;

		SizeDropdown.onValueChanged.AddListener(delegate { myDropdownValueChangedHandler(SizeDropdown);});
		FontDropdown.onValueChanged.AddListener(delegate { myFontDropdownValueChangedHandler(FontDropdown);});
		ColorDropdown.onValueChanged.AddListener(delegate { colorInput(ColorDropdown);});

	}
	public void colorInput(Dropdown target){
		colorSetting(target.value);
	}

	public void colorSetting(int i){
		switch (i){
		case 0:
			currText.color = Color.white;
			break;
		case 1:
			currText.color = Color.black;
			break;
		case 2:
			currText.color = Color.red;
			break;
		case 3:
			currText.color = Color.green;
			break;
		case 4:
			currText.color = Color.blue;
			break;				
		}
	}


	public void inputUpdate(string input){
		temp = input;
        currText.text = temp;
	}

	public void inputText(){
		currText.text = temp;
	}

	public void exit(){
		IsCalled = false;
	}
	public void setFont(int i){
		switch (i){
		case 0:
			currText.font = Arial;
			break;
		case 1:
			currText.font = Times;
			break;
		case 2:
			currText.font = Verdana;
			break;

		}
	}

	public void globalSetFont(GameObject text, string fontName){
		switch (fontName){
		case "ARIAL":
			text.GetComponent<Text>().font = Arial;
			break;
		case "TIMES":
			text.GetComponent<Text>().font = Times;
			break;
		case "AVERDANA":
			text.GetComponent<Text>().font = Verdana;
			break;
		}
	}


	public void textSize(int i){
		switch (i) {
		case 0: currText.fontSize = 5; break;
		case 1: currText.fontSize = 6; break;
		case 2: currText.fontSize = 7; break;
		case 3: currText.fontSize = 8; break;
		case 4: currText.fontSize = 9; break;
		case 5: currText.fontSize = 10; break;
		case 6: currText.fontSize = 11; break;
		case 7: currText.fontSize = 12; break;
		case 8: currText.fontSize = 14; break;
		case 9: currText.fontSize = 16; break;
		case 10: currText.fontSize = 18; break;
		case 11: currText.fontSize = 20; break;
		case 12: currText.fontSize = 22; break;
		case 13: currText.fontSize = 24; break;
		case 14: currText.fontSize = 26; break;
		case 15: currText.fontSize = 28; break;
		case 16: currText.fontSize = 36; break;
		case 17: currText.fontSize = 48; break;
		}
	}


	public void Enable(GameObject selected){
		currText = selected.GetComponent<Text>();
        rect = selected.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 160);
        currText.color = Color.white;
        IsCalled = true;
        inputField.text = currText.text;
        widthSlider.value = rect.sizeDelta.x;

    }


    void Update(){
		if(IsCalled){
			TextSettingMenu.SetActive(true);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + (widthSlider.value - rect.sizeDelta.x)/2, rect.anchoredPosition.y);
            rect.sizeDelta = new Vector2(widthSlider.value, rect.sizeDelta.y);
        }
        else{
			TextSettingMenu.SetActive(false);
		}

	}






}
