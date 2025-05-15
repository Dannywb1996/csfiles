using System;
using UnityEngine;

public class PerControlVisibility : MonoBehaviour
{
	public bool keyboardAndMouse;

	public bool gamepad;

	private void Start()
	{
		DewInput.onCurrentModeChanged = (Action<InputMode, InputMode>)Delegate.Combine(DewInput.onCurrentModeChanged, new Action<InputMode, InputMode>(OnCurrentModeChanged));
		OnCurrentModeChanged(InputMode.Gamepad, DewInput.currentMode);
	}

	private void OnDestroy()
	{
		DewInput.onCurrentModeChanged = (Action<InputMode, InputMode>)Delegate.Remove(DewInput.onCurrentModeChanged, new Action<InputMode, InputMode>(OnCurrentModeChanged));
	}

	private void OnCurrentModeChanged(InputMode arg1, InputMode arg2)
	{
		switch (arg2)
		{
		case InputMode.KeyboardAndMouse:
			base.gameObject.SetActive(keyboardAndMouse);
			break;
		case InputMode.Gamepad:
			base.gameObject.SetActive(gamepad);
			break;
		default:
			throw new ArgumentOutOfRangeException("arg2", arg2, null);
		}
	}
}
