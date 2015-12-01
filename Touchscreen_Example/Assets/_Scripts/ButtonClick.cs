using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour {
    public Transform labelBck;
    public Transform labelTxt;
    public Transform labelIcn;
    public Transform labelMsg;
    public Transform backImg;
    public Transform frntImg;

    public float fadeSpd;
    public float fadeWait;
    private float sign;
    private float timeStamp;

    private Color bckColor;
    private Color txtColor;
    private CanvasGroup frntBg;
    private CanvasGroup backBg;

    private Button button;

	// Use this for initialization
	void Start () {
        button = this.GetComponent<Button>();
        /*
        float buttW = button.GetComponent<RectTransform>().rect.width;
        float buttH = button.GetComponent<RectTransform>().rect.height;
        float frntW = frntImg.GetComponent<RectTransform>().rect.width;
        float frntH = frntImg.GetComponent<RectTransform>().rect.height;
        float backW = backImg.GetComponent<RectTransform>().rect.width;
        float backH = backImg.GetComponent<RectTransform>().rect.height;

        frntImg.localPosition = new Vector3(frntW - buttW, frntH - buttH, 0);
        */
        
        frntBg = frntImg.GetComponent<CanvasGroup>();
        backBg = backImg.GetComponent<CanvasGroup>();

        sign = (frntBg.alpha - backBg.alpha) / Mathf.Abs(frntBg.alpha - backBg.alpha);
        timeStamp = Time.time;

        bckColor = labelBck.GetComponent<Image>().color;
        txtColor = labelTxt.GetComponent<Text>().color;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        { // Change banner color on mouse down event
            labelBck.GetComponent<Image>().color = txtColor;
            labelTxt.GetComponent<Text>().color = bckColor;
            labelIcn.GetComponent<Image>().color = bckColor;
            labelMsg.GetComponent<Text>().color = bckColor;
        }
        
        if (Input.GetMouseButtonUp(0))
        { // Change banner color back on mouse up event
            labelBck.GetComponent<Image>().color = bckColor;
            labelTxt.GetComponent<Text>().color = txtColor;
            labelIcn.GetComponent<Image>().color = txtColor;
            labelMsg.GetComponent<Text>().color = txtColor;
        }

        if (Time.time - timeStamp > fadeWait) {
            frntBg.alpha -= sign * fadeSpd;
            backBg.alpha += sign * fadeSpd;

            if (frntImg.GetComponent<CanvasGroup>().alpha <= 0 || backImg.GetComponent<CanvasGroup>().alpha <= 0)
            {
                sign *= -1;
                timeStamp = Time.time;
            }
        }
	}
}
