using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public Vector3 IniPosition;

	void IBeginDragHandler.OnBeginDrag (PointerEventData eventData)
	{
		IniPosition = new Vector3(Input.mousePosition.x - transform.position.x, Input.mousePosition.y - transform.position.y, Input.mousePosition.z - transform.position.z);
	}

	void IDragHandler.OnDrag (PointerEventData eventData)
	{
		transform.position = Input.mousePosition - IniPosition;
	}

	public void OnEndDrag (PointerEventData eventData)
	{
	
	}

}

