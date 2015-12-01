using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIEditorWindowData : ScriptableObject {

	public int lineMode = 0;
	public List<UIWindow> UIWindows = new List<UIWindow>(); 
	public List<Rect> windows = new List<Rect>(); 

}
