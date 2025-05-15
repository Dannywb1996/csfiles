using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UI_GamepadFocusable : MonoBehaviour, IGamepadFocusable, IGamepadFocusListener
{
	public SelectionDisplayType display = SelectionDisplayType.Box;

	public bool canHoldConfirmToRepeat;

	public bool focusOnEnable;

	public bool ignoreSelectableInteractivity;

	private bool _isClickable;

	private int _isClickableFrameCount;

	private Selectable _selectable;

	public bool isFocused { get; internal set; }

	protected virtual void OnEnable()
	{
		_selectable = GetComponent<Button>();
		if (ManagerBase<GlobalUIManager>.instance == null)
		{
			return;
		}
		ManagerBase<GlobalUIManager>.instance.AddGamepadFocusable(this);
		if (focusOnEnable)
		{
			ManagerBase<GlobalUIManager>.instance.SetFocus(this);
			Dew.CallDelayed(delegate
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(this);
			});
		}
	}

	public SelectionDisplayType GetSelectionDisplayType()
	{
		return display;
	}

	protected virtual void OnDisable()
	{
		if (ManagerBase<GlobalUIManager>.instance != null)
		{
			ManagerBase<GlobalUIManager>.instance.RemoveGamepadFocusable(this);
		}
	}

	public virtual bool CanBeFocused()
	{
		if (base.gameObject.activeInHierarchy && (ignoreSelectableInteractivity || _selectable == null || _selectable.IsInteractable()))
		{
			return IsClickable();
		}
		return false;
	}

	public virtual void OnFocusStateChanged(bool state)
	{
		isFocused = state;
	}

	public bool IsClickable()
	{
		if (_isClickableFrameCount != Time.frameCount)
		{
			_isClickable = ManagerBase<GlobalUIManager>.instance.IsUIElementClickable((RectTransform)base.transform);
			_isClickableFrameCount = Time.frameCount;
		}
		return _isClickable;
	}

	public bool CanHoldConfirmToRepeat()
	{
		return canHoldConfirmToRepeat;
	}
}
