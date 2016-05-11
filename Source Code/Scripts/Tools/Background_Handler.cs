using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Background_Handler : MonoBehaviour {
    public float red = 0;
    public float green = 0;
    public float blue = 0;
    public float alpha = 0;
	public List<Slider> sliders = new List<Slider>();
    public GameObject BackgroundMenu;

    public static bool IsCalled;


    public static Color color;
    
	public static Background_Handler _instance;

	void Awake() {
		if (_instance != null) {
			Destroy(_instance);
			Debug.LogWarning("Trying to create multiple instances of Background_Handler");
		} 
		_instance = this;
	}

    void Start() {
    	Camera.main.clearFlags = CameraClearFlags.SolidColor;

    }
    
	public static Vector4 GetCurrColor() {
		return new Vector4(color.r, color.g, color.b, color.a);
	}

	public static void SetCurrColor(Vector4 c, Camera cam) {
		color = c;
        cam.backgroundColor = color;
		_instance.Disable();
	}

    public void ChangeColor(){
    }

	//Slider variables
	public void Red_int(float i){ red = i;}
    public void Green_int(float i){ green = i;}
    public void Blue_int(float i){ blue = i;}
    public void Alpha_int(float i){ alpha = i;}

    public void set_button(){
        IsCalled = false;
    }

    public void reset_button(){
        red = 0;
        green = 0;
        blue = 0;
        alpha = 0;
		for (int i = 0; i < sliders.Count; i++) {
			Slider s = sliders [i];
			s.value = 0;
		}
    }

	public void UpdateColor() {
		red = color.r;
		blue = color.b;
		green = color.g;
		alpha = color.a;
		sliders[0].value = red;
		sliders[1].value = blue;
		sliders[2].value = green;
		sliders[3].value = alpha;
	}

    public void Enable() {
		UpdateColor();
        IsCalled = true;
    }

	public void Disable() {
		UpdateColor();
		IsCalled = false;
	}

	public void Toggle() {
		UpdateColor();
		IsCalled = !IsCalled;
	}

    void Update(){
        if(IsCalled){
            BackgroundMenu.SetActive(true);
			color = new Color(red, green, blue, alpha);
			Camera.main.backgroundColor = color;
        }else{
            BackgroundMenu.SetActive(false);
        }
/*
        if(Input.GetKeyDown(KeyCode.F3)){
            IsCalled = !IsCalled;
        }
*/

//        print(red+green+blue+alpha);
    }


}

