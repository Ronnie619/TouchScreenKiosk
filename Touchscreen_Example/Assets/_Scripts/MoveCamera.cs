using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour {
    public Transform targetCanvas;
    public Transform parentCanvas;
    public Transform UI;
    public Camera camera;

    private float interval = 50;
    private float posX, posY, posZ;
    private float intX, intY, intZ;
    private float distance, fade;

	// Use this for initialization
	void Start () {
        posX = targetCanvas.position.x;
        posY = targetCanvas.position.y;
        posZ = targetCanvas.position.z - 690;
        
        intX = (posX - camera.transform.position.x) / interval;
        intY = (posY - camera.transform.position.y) / interval;
        intZ = (posZ - camera.transform.position.z) / interval;

        distance = Mathf.Abs(posZ - camera.transform.position.z);
        foreach (GameObject g in GameObject.FindGameObjectsWithTag(targetCanvas.tag)) {
            g.GetComponent<CanvasGroup>().alpha = 0;
        }
     //   targetCanvas.GetComponent<CanvasGroup>().alpha = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (Mathf.Abs(camera.transform.position.z - posZ) <= Mathf.Abs(intZ))
        {
            camera.transform.position = new Vector3(posX, posY, posZ);
            this.enabled = false;
        }
        else
        {
            camera.transform.position += new Vector3(intX, intY, intZ);
            float dist = Mathf.Abs(posZ - camera.transform.position.z);
            fade = 1 - (dist / distance);

            int t = int.Parse(targetCanvas.tag) - 1;
            string str = t.ToString();
            foreach (GameObject g in GameObject.FindGameObjectsWithTag(targetCanvas.tag))
            {
                g.GetComponent<CanvasGroup>().alpha = fade;
            }   
            foreach (GameObject g in GameObject.FindGameObjectsWithTag(str))
            {
                g.GetComponent<CanvasGroup>().alpha = 1 - fade;
            }
       //     targetCanvas.GetComponent<CanvasGroup>().alpha = fade;
       //     parentCanvas.GetComponent<CanvasGroup>().alpha = 1 - fade;
        }
    }
}
