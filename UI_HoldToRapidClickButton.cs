using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HoldToRapidClickButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public UnityEvent onClick;

	private bool _isHeld;

	private int _clickCount;

	private float _lastClickUnscaledTime;

	private void Update()
	{
		if (!_isHeld)
		{
			return;
		}
		Selectable btn = GetComponent<Selectable>();
		if (btn != null && !btn.IsInteractable())
		{
			OnPointerUp(new PointerEventData(EventSystem.current)
			{
				button = PointerEventData.InputButton.Left
			});
			return;
		}
		float elapsed = Time.unscaledTime - _lastClickUnscaledTime;
		if (_clickCount < 4)
		{
			if (elapsed > 0.3f)
			{
				Click();
			}
		}
		else if (_clickCount < 8)
		{
			if (elapsed > 0.2f)
			{
				Click();
			}
		}
		else if (_clickCount < 20)
		{
			if (elapsed > 0.1f)
			{
				Click();
			}
		}
		else if (_clickCount < 30)
		{
			if (elapsed > 0.05f)
			{
				Click();
			}
		}
		else if (_clickCount < 50)
		{
			if (elapsed > 0.025f)
			{
				Click();
			}
		}
		else if (elapsed > 0.02f)
		{
			Click();
		}
	}

	private void OnDisable()
	{
		OnPointerUp(new PointerEventData(EventSystem.current)
		{
			button = PointerEventData.InputButton.Left
		});
	}

	private void Click()
	{
		_clickCount++;
		_lastClickUnscaledTime = Time.unscaledTime;
		PointerEventData e = new PointerEventData(EventSystem.current)
		{
			button = PointerEventData.InputButton.Left
		};
		IPointerDownHandler[] components = GetComponents<IPointerDownHandler>();
		foreach (IPointerDownHandler h in components)
		{
			if (h != this)
			{
				h.OnPointerDown(e);
			}
		}
		IPointerUpHandler[] components2 = GetComponents<IPointerUpHandler>();
		foreach (IPointerUpHandler h2 in components2)
		{
			if (h2 != this)
			{
				h2.OnPointerUp(e);
			}
		}
		IPointerClickHandler[] components3 = GetComponents<IPointerClickHandler>();
		foreach (IPointerClickHandler h3 in components3)
		{
			if (h3 != this)
			{
				h3.OnPointerClick(e);
			}
		}
		try
		{
			onClick?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			Selectable btn = GetComponent<Selectable>();
			if (!(btn != null) || btn.IsInteractable())
			{
				_clickCount = 0;
				_isHeld = true;
				_lastClickUnscaledTime = float.NegativeInfinity;
				Update();
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && _isHeld)
		{
			_clickCount = 0;
			_isHeld = false;
			_lastClickUnscaledTime = float.NegativeInfinity;
		}
	}
}
