using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeController : MonoBehaviour, IDragHandler
{

	public event System.Action<PointerEventData, Rect> OnDragEvent;

	private RectTransform _trs;

	void Awake()
	{
		_trs = GetComponent<RectTransform>();
	}

	public void OnDrag(PointerEventData data)
	{
		if (OnDragEvent != null && _trs != null)
		{
			OnDragEvent(data, _trs.rect);
			data.Use();
		}
	}
}
