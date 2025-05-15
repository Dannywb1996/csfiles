using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class InputManager : ManagerBase<InputManager>, ISettingsChangedCallback
{
	private enum BindType
	{
		PC,
		Gamepad
	}

	private struct InputTriggerCheckItem
	{
		public DewInputTrigger trigger;

		public BindType bindType;

		public int bindIndex;

		public int priority;
	}

	private readonly List<DewInputTrigger> _inputTriggers = new List<DewInputTrigger>();

	private readonly List<InputTriggerCheckItem> _checkedItems = new List<InputTriggerCheckItem>();

	private readonly List<(object, InputTriggerCheckItem)> _consumedInputs = new List<(object, InputTriggerCheckItem)>();

	private int _lastUpdateFrameCount;

	internal bool _LUDown;

	internal bool _LU;

	internal bool _LUUp;

	internal bool _LDDown;

	internal bool _LD;

	internal bool _LDUp;

	internal bool _LLDown;

	internal bool _LL;

	internal bool _LLUp;

	internal bool _LRDown;

	internal bool _LR;

	internal bool _LRUp;

	internal bool _RUDown;

	internal bool _RU;

	internal bool _RUUp;

	internal bool _RDDown;

	internal bool _RD;

	internal bool _RDUp;

	internal bool _RLDown;

	internal bool _RL;

	internal bool _RLUp;

	internal bool _RRDown;

	internal bool _RR;

	internal bool _RRUp;

	private const uint SPI_GETMOUSESPEED = 112u;

	private const uint SPI_SETMOUSESPEED = 113u;

	private const uint SPIF_UPDATEINIFILE = 1u;

	private const uint SPIF_SENDCHANGE = 2u;

	private int prevSensitivity = -1;

	private List<ShakeInstance> _shakes = new List<ShakeInstance>();

	private void Start()
	{
		SteamManager.instance.onGameOverlayShownChanged += new Action<bool>(OnGameOverlayShownChanged);
	}

	private void OnGameOverlayShownChanged(bool obj)
	{
		ResetInputDevices();
	}

	public void ResetInputDevices()
	{
		foreach (InputDevice device in InputSystem.devices)
		{
			InputSystem.ResetDevice(device);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		DewInput.UpdateScrollAxis();
		if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f)
		{
			DewInput.SetInputMode(InputMode.KeyboardAndMouse);
		}
		if (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude > DewSave.profile.controls.joystick0Dead || DewInput.GetButtonDownAnyGamepad())
		{
			DewInput.SetInputMode(InputMode.Gamepad);
		}
		UpdateJoystickStates();
		DoRumbleFrameUpdate();
		PrepareInputs();
		if (DewInput._currentAnyButtonListenerAction != null && Mathf.Abs(Mouse.current.scroll.value.y) > 0f)
		{
			DewInput._currentAnyButtonListenerAction?.Invoke(Mouse.current.scroll);
		}
	}

	internal void AddTrigger(DewInputTrigger t)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return null;
			if (DewInputTrigger.MockTrigger != t)
			{
				_inputTriggers.Add(t);
				t._binding = t.binding();
				if (t._binding.canAssignGamepad)
				{
					for (int i = 0; i < t._binding.gamepadBinds.Count; i++)
					{
						_checkedItems.Add(new InputTriggerCheckItem
						{
							trigger = t,
							priority = t.GetCheckPriorityOfGamepadBind(i),
							bindIndex = i,
							bindType = BindType.Gamepad
						});
					}
				}
				if (t._binding.canAssignKeyboard || t._binding.canAssignMouse)
				{
					for (int j = 0; j < t._binding.pcBinds.Count; j++)
					{
						_checkedItems.Add(new InputTriggerCheckItem
						{
							trigger = t,
							priority = t.GetCheckPriorityOfPCBind(j),
							bindIndex = j,
							bindType = BindType.PC
						});
					}
				}
				_checkedItems.Sort((InputTriggerCheckItem a, InputTriggerCheckItem b) => a.priority.CompareTo(b.priority));
			}
		}
	}

	internal void PrepareInputs()
	{
		if (Time.frameCount == _lastUpdateFrameCount)
		{
			return;
		}
		_lastUpdateFrameCount = Time.frameCount;
		for (int i = _consumedInputs.Count - 1; i >= 0; i--)
		{
			if (_consumedInputs[i].Item1 is Key key)
			{
				if (Keyboard.current == null || !Keyboard.current[key].isPressed)
				{
					_consumedInputs.RemoveAt(i);
				}
			}
			else if (_consumedInputs[i].Item1 is MouseButton mouse)
			{
				if (Mouse.current == null || !DewInput.GetMouseButton_Imp(mouse))
				{
					_consumedInputs.RemoveAt(i);
				}
			}
			else if (_consumedInputs[i].Item1 is GamepadButtonEx gamepad)
			{
				if (IsStickDirection(gamepad))
				{
					if (!GetStickDirection(gamepad))
					{
						_consumedInputs.RemoveAt(i);
					}
				}
				else if (Gamepad.current == null || !Gamepad.current[(GamepadButton)gamepad].isPressed)
				{
					_consumedInputs.RemoveAt(i);
				}
			}
			else
			{
				_consumedInputs.RemoveAt(i);
			}
		}
		for (int i2 = _inputTriggers.Count - 1; i2 >= 0; i2--)
		{
			if (_inputTriggers[i2].owner == null)
			{
				_inputTriggers.RemoveAt(i2);
			}
		}
		for (int i3 = _checkedItems.Count - 1; i3 >= 0; i3--)
		{
			if (_checkedItems[i3].trigger.owner == null)
			{
				_checkedItems.RemoveAt(i3);
			}
		}
		foreach (DewInputTrigger trg in _inputTriggers)
		{
			try
			{
				trg._isValid = trg.isValidCheck == null || trg.isValidCheck();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		foreach (DewInputTrigger inputTrigger in _inputTriggers)
		{
			inputTrigger._flag = false;
		}
		foreach (var c in _consumedInputs)
		{
			c.Item2.trigger._flag = true;
			if (!c.Item2.trigger._isValid)
			{
				c.Item2.trigger._isSuppressed = true;
			}
		}
		for (int j = 0; j < _checkedItems.Count; j++)
		{
			try
			{
				InputTriggerCheckItem item = _checkedItems[j];
				DewInputTrigger trg2 = item.trigger;
				if (trg2._flag)
				{
					continue;
				}
				DewBinding b = item.trigger._binding;
				if (!trg2._isValid)
				{
					continue;
				}
				if (item.bindType == BindType.PC)
				{
					PCBind pbind = item.trigger._binding.pcBinds[item.bindIndex];
					if (pbind.key != Key.None && Keyboard.current != null)
					{
						KeyControl key2 = Keyboard.current[pbind.key];
						bool isConsumed = IsConsumed(pbind.key);
						if ((!isConsumed || trg2.ignoreConsumeCheck) && key2.isPressed && DewInput.currentMode == InputMode.KeyboardAndMouse)
						{
							bool passedModifierCheck = true;
							foreach (Key k in pbind.modifiers)
							{
								if (!Keyboard.current[k].isPressed)
								{
									passedModifierCheck = false;
									break;
								}
							}
							if (passedModifierCheck)
							{
								if (trg2.canConsume && !isConsumed)
								{
									_consumedInputs.Add((pbind.key, item));
								}
								trg2._flag = true;
							}
						}
					}
					if (pbind.mouse != MouseButton.None && Mouse.current != null)
					{
						bool isConsumed2 = IsConsumed(pbind.mouse);
						if ((!isConsumed2 || trg2.ignoreConsumeCheck) && DewInput.GetMouseButton_Imp(pbind.mouse) && DewInput.currentMode == InputMode.KeyboardAndMouse)
						{
							if (trg2.checkGameAreaForMouse && !DewInput.IsGameRelatedMouseInputValid(pbind.mouse))
							{
								trg2._isSuppressed = true;
							}
							bool passedModifierCheck2 = true;
							foreach (Key k2 in pbind.modifiers)
							{
								if (!Keyboard.current[k2].isPressed)
								{
									passedModifierCheck2 = false;
									break;
								}
							}
							if (passedModifierCheck2)
							{
								if (trg2.canConsume && !isConsumed2)
								{
									_consumedInputs.Add((pbind.mouse, item));
								}
								trg2._flag = true;
							}
						}
					}
				}
				if (item.bindType != BindType.Gamepad || Gamepad.current == null)
				{
					continue;
				}
				GamepadButtonEx gbind = b.gamepadBinds[item.bindIndex];
				bool isPressed = ((!IsStickDirection(gbind)) ? Gamepad.current[(GamepadButton)gbind].isPressed : GetStickDirection(gbind));
				bool isConsumed3 = IsConsumed(gbind);
				if ((!isConsumed3 || trg2.ignoreConsumeCheck) && isPressed && DewInput.currentMode == InputMode.Gamepad)
				{
					if (trg2.canConsume && !isConsumed3)
					{
						_consumedInputs.Add((gbind, item));
					}
					trg2._flag = true;
				}
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		foreach (DewInputTrigger inputTrigger2 in _inputTriggers)
		{
			inputTrigger2.Update();
		}
	}

	private bool IsConsumed(object input)
	{
		for (int i = 0; i < _consumedInputs.Count; i++)
		{
			if (_consumedInputs[i].Item1.Equals(input))
			{
				return true;
			}
		}
		return false;
	}

	public void OnSettingsChanged()
	{
		try
		{
			OnSettingsChanged_MouseSensitivity();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		_lastUpdateFrameCount = -1;
		_checkedItems.Clear();
		foreach (DewInputTrigger t in _inputTriggers)
		{
			try
			{
				t._binding = t.binding();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			t.Reset();
			if (t._binding.canAssignGamepad)
			{
				for (int i = 0; i < t._binding.gamepadBinds.Count; i++)
				{
					_checkedItems.Add(new InputTriggerCheckItem
					{
						trigger = t,
						priority = t.GetCheckPriorityOfGamepadBind(i),
						bindIndex = i,
						bindType = BindType.Gamepad
					});
				}
			}
			if (t._binding.canAssignKeyboard || t._binding.canAssignMouse)
			{
				for (int j = 0; j < t._binding.pcBinds.Count; j++)
				{
					_checkedItems.Add(new InputTriggerCheckItem
					{
						trigger = t,
						priority = t.GetCheckPriorityOfPCBind(j),
						bindIndex = j,
						bindType = BindType.PC
					});
				}
			}
		}
		_checkedItems.Sort((InputTriggerCheckItem a, InputTriggerCheckItem b) => a.priority.CompareTo(b.priority));
	}

	public bool IsStickDirection(GamepadButtonEx g)
	{
		if (g >= GamepadButtonEx.LeftStickUp)
		{
			return g <= GamepadButtonEx.RightStickRight;
		}
		return false;
	}

	public bool GetStickDirection(GamepadButtonEx g)
	{
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		return g switch
		{
			GamepadButtonEx.LeftStickUp => _LU, 
			GamepadButtonEx.LeftStickDown => _LD, 
			GamepadButtonEx.LeftStickLeft => _LL, 
			GamepadButtonEx.LeftStickRight => _LR, 
			GamepadButtonEx.RightStickUp => _RU, 
			GamepadButtonEx.RightStickDown => _RD, 
			GamepadButtonEx.RightStickLeft => _RL, 
			GamepadButtonEx.RightStickRight => _RR, 
			_ => false, 
		};
	}

	public bool GetStickDirectionDown(GamepadButtonEx g)
	{
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		return g switch
		{
			GamepadButtonEx.LeftStickUp => _LUDown, 
			GamepadButtonEx.LeftStickDown => _LDDown, 
			GamepadButtonEx.LeftStickLeft => _LLDown, 
			GamepadButtonEx.LeftStickRight => _LRDown, 
			GamepadButtonEx.RightStickUp => _RUDown, 
			GamepadButtonEx.RightStickDown => _RDDown, 
			GamepadButtonEx.RightStickLeft => _RLDown, 
			GamepadButtonEx.RightStickRight => _RRDown, 
			_ => false, 
		};
	}

	public bool GetStickDirectionUp(GamepadButtonEx g)
	{
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		return g switch
		{
			GamepadButtonEx.LeftStickUp => _LUUp, 
			GamepadButtonEx.LeftStickDown => _LDUp, 
			GamepadButtonEx.LeftStickLeft => _LLUp, 
			GamepadButtonEx.LeftStickRight => _LRUp, 
			GamepadButtonEx.RightStickUp => _RUUp, 
			GamepadButtonEx.RightStickDown => _RDUp, 
			GamepadButtonEx.RightStickLeft => _RLUp, 
			GamepadButtonEx.RightStickRight => _RRUp, 
			_ => false, 
		};
	}

	private void UpdateJoystickStates()
	{
		Vector2 ls = DewInput.GetLeftJoystick();
		Vector2 rs = DewInput.GetLeftJoystick();
		float threshold = 0.8f;
		bool num = ls.magnitude > threshold;
		bool rsIsOverThreshold = rs.magnitude > threshold;
		DoJoystickDirection(num && Vector2.Angle(Vector2.left, ls) < 67.5f, out _LLDown, ref _LL, out _LLUp);
		DoJoystickDirection(num && Vector2.Angle(Vector2.right, ls) < 67.5f, out _LRDown, ref _LR, out _LRUp);
		DoJoystickDirection(num && Vector2.Angle(Vector2.down, ls) < 67.5f, out _LDDown, ref _LD, out _LDUp);
		DoJoystickDirection(num && Vector2.Angle(Vector2.up, ls) < 67.5f, out _LUDown, ref _LU, out _LUUp);
		DoJoystickDirection(rsIsOverThreshold && Vector2.Angle(Vector2.left, rs) < 67.5f, out _RLDown, ref _RL, out _RLUp);
		DoJoystickDirection(rsIsOverThreshold && Vector2.Angle(Vector2.right, rs) < 67.5f, out _RRDown, ref _RR, out _RRUp);
		DoJoystickDirection(rsIsOverThreshold && Vector2.Angle(Vector2.down, rs) < 67.5f, out _RDDown, ref _RD, out _RDUp);
		DoJoystickDirection(rsIsOverThreshold && Vector2.Angle(Vector2.up, rs) < 67.5f, out _RUDown, ref _RU, out _RUUp);
		static void DoJoystickDirection(bool condition, out bool down, ref bool held, out bool up)
		{
			if (condition)
			{
				down = !held;
				held = true;
				up = false;
			}
			else
			{
				up = held;
				held = false;
				down = false;
			}
		}
	}

	[DllImport("user32.dll")]
	private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref int pvParam, uint fWinIni);

	[DllImport("user32.dll")]
	private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, int pvParam, uint fWinIni);

	public static int GetMouseSensitivity()
	{
		int sensitivity = 0;
		SystemParametersInfo(112u, 0u, ref sensitivity, 0u);
		return sensitivity;
	}

	public static void SetMouseSensitivity(int sensitivity)
	{
		sensitivity = Math.Clamp(sensitivity, 1, 20);
		SystemParametersInfo(113u, 0u, sensitivity, 1u);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			if (prevSensitivity == -1)
			{
				prevSensitivity = GetMouseSensitivity();
			}
			SetMouseSensitivity(DewSave.profile.controls.mouseSensitivity);
		}
		else if (prevSensitivity != -1)
		{
			SetMouseSensitivity(prevSensitivity);
			prevSensitivity = -1;
		}
		ResetInputDevices();
	}

	private void OnApplicationQuit()
	{
		if (prevSensitivity != -1)
		{
			SetMouseSensitivity(prevSensitivity);
			prevSensitivity = -1;
		}
	}

	private void OnSettingsChanged_MouseSensitivity()
	{
		if (prevSensitivity == -1)
		{
			prevSensitivity = GetMouseSensitivity();
		}
		SetMouseSensitivity(DewSave.profile.controls.mouseSensitivity);
	}

	public void AddShakeInstance(ShakeInstance si)
	{
		si.attackTime *= 0.7f;
		si.sustainTime *= 0.9f;
		si.decayTime *= 0.3f;
		_shakes.Add(si);
	}

	private void DoRumbleFrameUpdate()
	{
		float lowFreq = 0f;
		float highFreq = 0f;
		float globalStrength = 2.5f * DewSave.profile.gameplay.gamepadVibrationStrength;
		for (int i = _shakes.Count - 1; i >= 0; i--)
		{
			ShakeInstance s = _shakes[i];
			float elapsed = Time.time - s.startTime;
			if (elapsed > s.attackTime + s.sustainTime + s.decayTime)
			{
				_shakes.RemoveAt(i);
			}
			else if (!(Time.timeScale < 0.001f))
			{
				float strength = ((elapsed < s.attackTime) ? 1f : ((!(elapsed < s.attackTime + s.sustainTime)) ? (1f - (elapsed - s.attackTime - s.sustainTime) / s.decayTime) : 1f));
				lowFreq += s.amplitude * 0.1f * strength * globalStrength;
				highFreq += s.amplitude * 0f * strength * globalStrength;
			}
		}
		if (DewInput.currentMode != InputMode.Gamepad)
		{
			lowFreq = 0f;
			highFreq = 0f;
		}
		if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
		}
	}

	private void OnDestroy()
	{
		if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(0f, 0f);
		}
	}
}
