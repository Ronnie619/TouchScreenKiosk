using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

	public Transform mainPanel;											//Main canvas panel for the UI
	public GameObject partitionPrefab;									//Prefab for the user partitions
	public int maxPartitions = 5;										//Max allowed users/partitions (needs to be maxed out to prevent too many users/too small space)

	private Vector3[] canvasCorners = new Vector3[4];					//Corners of the canvas
	private float canvasWidth, canvasHeight;							//width and height of the primary canvas
	private List<Transform> currentPartitions = new List<Transform>();	//List of all the existing partitions
	private bool isDirty;												//dirty flag for layout updates

	void Start() {
		mainPanel.GetComponent<RectTransform>().GetWorldCorners(canvasCorners);
		canvasWidth = mainPanel.GetComponent<RectTransform>().rect.width;
		canvasHeight = mainPanel.GetComponent<RectTransform>().rect.height;

		IncreaseUsers(); //Initial partition
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			IncreaseUsers();
		} else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			DecreaseUsers();
		}

		if (isDirty) RefreshPanels();
	}

	/// <summary>
	/// Adds a partition
	/// </summary>
	void IncreaseUsers() {
		if (currentPartitions.Count >= maxPartitions)
			return;

		GameObject newPartition = Instantiate(partitionPrefab) as GameObject;
		newPartition.transform.SetParent(mainPanel, false);
		currentPartitions.Add(newPartition.transform);
		isDirty = true;
	}

	/// <summary>
	/// Removes a partition
	/// </summary>
	void DecreaseUsers() {
		if (currentPartitions.Count == 0)
			return;

		Transform partition = currentPartitions[currentPartitions.Count-1];
		currentPartitions.Remove(partition);	
		Destroy(partition.gameObject);

		isDirty = true;
	}

	/// <summary>
	/// Refreshs the panels whenever there is a layout change, any unautomated changes must be done here
	/// </summary>
	void RefreshPanels() {
		int numPanels = currentPartitions.Count;


		isDirty = false;
	}







}
