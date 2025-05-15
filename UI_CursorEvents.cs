using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CursorEvents : MonoBehaviour, IDragHandler, IEventSystemHandler, IDropHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, IPointerMoveHandler
{
	public Action<PointerEventData> onDrag;

	public Action<PointerEventData> onDrop;

	public Action<PointerEventData> onPointerClick;

	public Action<PointerEventData> onPointerDown;

	public Action<PointerEventData> onPointerUp;

	public Action<PointerEventData> onPointerExit;

	public Action<PointerEventData> onPointerEnter;

	public Action<PointerEventData> onPointerMove;

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		onDrag?.Invoke(eventData);
	}

	void IDropHandler.OnDrop(PointerEventData eventData)
	{
		onDrop?.Invoke(eventData);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		onPointerClick?.Invoke(eventData);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		onPointerDown?.Invoke(eventData);
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		onPointerUp?.Invoke(eventData);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		onPointerExit?.Invoke(eventData);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		onPointerEnter?.Invoke(eventData);
	}

	void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
	{
		onPointerMove?.Invoke(eventData);
	}
}
