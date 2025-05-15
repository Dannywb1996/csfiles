using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_GamepadBindButton : LogicBehaviour
{
	public enum ButtonType
	{
		Apply,
		Secondary
	}

	public ButtonType buttonType;

	public GameObject buttonDisplayObject;

	public bool ignoreInteractableForObject = true;

	private DewInputTrigger it_trigger;

	private Button _button;

	private void Start()
	{
		_button = GetComponent<Button>();
		it_trigger = new DewInputTrigger
		{
			binding = () => buttonType switch
			{
				ButtonType.Apply => DewSave.profile.controls.gamepadApply, 
				ButtonType.Secondary => DewSave.profile.controls.gamepadSecondary, 
				_ => throw new ArgumentOutOfRangeException(), 
			},
			priority = -10,
			owner = this,
			canConsume = true,
			isValidCheck = () => _button != null && _button.isActiveAndEnabled && _button.IsInteractable()
		};
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (buttonDisplayObject != null)
		{
			buttonDisplayObject.SetActive(DewInput.currentMode == InputMode.Gamepad && (ignoreInteractableForObject || _button.IsInteractable()));
		}
		if (it_trigger.down)
		{
			ManagerBase<GlobalUIManager>.instance.SimulateClickOnUIElement(_button);
		}
	}
}
