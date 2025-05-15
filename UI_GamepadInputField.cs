using System;
using TMPro;
using UnityEngine.EventSystems;

public class UI_GamepadInputField : UI_GamepadFocusable, IGamepadFocusable, IGamepadFocusListener, IPointerClickHandler, IEventSystemHandler
{
	private TMP_InputField _inputField;

	private void Awake()
	{
		_inputField = GetComponent<TMP_InputField>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (focusOnEnable && _inputField.IsInteractable())
		{
			ManagerBase<GlobalUIManager>.instance.SetFocus(this);
		}
	}

	public override bool CanBeFocused()
	{
		if (base.CanBeFocused() && _inputField.isActiveAndEnabled && _inputField.IsInteractable())
		{
			return !_inputField.readOnly;
		}
		return false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && DewInput.currentMode == InputMode.Gamepad && CanBeFocused())
		{
			SingletonBehaviour<UI_GamepadTextInput>.instance.StartInput(_inputField);
		}
	}

	public void StartInput(Action onConfirm = null, Action onCancel = null)
	{
		SingletonBehaviour<UI_GamepadTextInput>.instance.StartInput(_inputField, onConfirm, onCancel);
	}
}
