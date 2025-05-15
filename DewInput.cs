using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

public static class DewInput
{
	private enum ValidStatus
	{
		No,
		Yes,
		YesExceptLeft
	}

	private static UnityEngine.Object _currentAnyButtonListener;

	internal static Action<InputControl> _currentAnyButtonListenerAction;

	private static IDisposable _currentAnyButtonListenerDisposable;

	public static Action<InputMode, InputMode> onCurrentModeChanged;

	private static bool _shouldCheckInputModeConflict = true;

	private static List<float> _inputModeChangeTimes = new List<float>();

	internal static readonly IReadOnlyDictionary<Key, string> KeyReadableTextBindings = new Dictionary<Key, string>
	{
		{
			Key.None,
			"!None"
		},
		{
			Key.Escape,
			"Esc"
		},
		{
			Key.Enter,
			"Enter"
		},
		{
			Key.Digit0,
			"0"
		},
		{
			Key.Digit1,
			"1"
		},
		{
			Key.Digit2,
			"2"
		},
		{
			Key.Digit3,
			"3"
		},
		{
			Key.Digit4,
			"4"
		},
		{
			Key.Digit5,
			"5"
		},
		{
			Key.Digit6,
			"6"
		},
		{
			Key.Digit7,
			"7"
		},
		{
			Key.Digit8,
			"8"
		},
		{
			Key.Digit9,
			"9"
		},
		{
			Key.Period,
			"."
		},
		{
			Key.Backquote,
			"`"
		},
		{
			Key.Quote,
			"'"
		},
		{
			Key.NumpadMultiply,
			"Num*"
		},
		{
			Key.NumpadPlus,
			"Num+"
		},
		{
			Key.NumpadMinus,
			"Num-"
		},
		{
			Key.NumpadPeriod,
			"Num."
		},
		{
			Key.NumpadEnter,
			"NumEnter"
		},
		{
			Key.Minus,
			"-"
		},
		{
			Key.Slash,
			"/"
		},
		{
			Key.Backslash,
			"\\"
		},
		{
			Key.Semicolon,
			":"
		},
		{
			Key.Comma,
			","
		},
		{
			Key.Equals,
			"="
		},
		{
			Key.LeftBracket,
			"["
		},
		{
			Key.RightBracket,
			"]"
		},
		{
			Key.LeftArrow,
			"Left"
		},
		{
			Key.RightArrow,
			"Right"
		},
		{
			Key.UpArrow,
			"Up"
		},
		{
			Key.DownArrow,
			"Down"
		},
		{
			Key.LeftShift,
			"LShift"
		},
		{
			Key.RightShift,
			"RShift"
		},
		{
			Key.LeftAlt,
			"LAlt"
		},
		{
			Key.RightAlt,
			"RAlt"
		},
		{
			Key.LeftCtrl,
			"LCtrl"
		},
		{
			Key.RightCtrl,
			"RCtrl"
		}
	};

	internal static readonly IReadOnlyDictionary<GamepadButtonEx, string> GamepadButtonReadableTextBindings = new Dictionary<GamepadButtonEx, string>
	{
		{
			GamepadButtonEx.LeftShoulder,
			"LB"
		},
		{
			GamepadButtonEx.RightShoulder,
			"RB"
		},
		{
			GamepadButtonEx.LeftStick,
			"LS"
		},
		{
			GamepadButtonEx.RightStick,
			"RS"
		},
		{
			GamepadButtonEx.LeftTrigger,
			"LT"
		},
		{
			GamepadButtonEx.RightTrigger,
			"RT"
		},
		{
			GamepadButtonEx.A,
			"A"
		},
		{
			GamepadButtonEx.B,
			"B"
		},
		{
			GamepadButtonEx.Square,
			"X"
		},
		{
			GamepadButtonEx.North,
			"Y"
		}
	};

	internal static readonly IReadOnlyDictionary<MouseButton, string> MouseButtonReadableTextBindings = new Dictionary<MouseButton, string>
	{
		{
			MouseButton.None,
			"!None"
		},
		{
			MouseButton.Left,
			"LMB"
		},
		{
			MouseButton.Right,
			"RMB"
		},
		{
			MouseButton.Middle,
			"MMB"
		},
		{
			MouseButton.Forward,
			"MB5"
		},
		{
			MouseButton.Back,
			"MB4"
		},
		{
			MouseButton.ScrollDown,
			"Scroll-"
		},
		{
			MouseButton.ScrollUp,
			"Scroll+"
		}
	};

	public static readonly IReadOnlyDictionary<string, MouseButton> NameToMouseButton = new Dictionary<string, MouseButton>
	{
		{
			"leftButton",
			MouseButton.Left
		},
		{
			"rightButton",
			MouseButton.Right
		},
		{
			"middleButton",
			MouseButton.Middle
		},
		{
			"forwardButton",
			MouseButton.Forward
		},
		{
			"backButton",
			MouseButton.Back
		}
	};

	public static readonly List<Key> ModifierKeys = new List<Key>
	{
		Key.LeftShift,
		Key.LeftAlt,
		Key.LeftMeta,
		Key.LeftCtrl,
		Key.LeftMeta,
		Key.LeftMeta,
		Key.LeftMeta,
		Key.RightShift,
		Key.RightAlt,
		Key.RightMeta,
		Key.RightCtrl,
		Key.RightMeta,
		Key.RightMeta,
		Key.RightMeta
	};

	private static int _mouseScrollAxisFrameCount;

	private static float _currentFrameMouseScrollAxis;

	private static float _previousFrameMouseScrollAxis;

	private static ValidStatus _isMouseClickValid;

	private static int _isMouseClickValidFrame;

	private static List<RaycastResult> _isMouseClickValidHits = new List<RaycastResult>();

	public static InputMode currentMode { get; private set; }

	private static bool isInputDisabled => _currentAnyButtonListener != null;

	public static void StopListenAnyButton()
	{
		if (_currentAnyButtonListenerDisposable != null)
		{
			_currentAnyButtonListenerDisposable.Dispose();
			_currentAnyButtonListenerDisposable = null;
			_currentAnyButtonListener = null;
			_currentAnyButtonListenerAction = null;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Initialize()
	{
		_currentAnyButtonListenerDisposable = null;
		_currentAnyButtonListener = null;
		_currentAnyButtonListenerAction = null;
		onCurrentModeChanged = null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void InitializeLate()
	{
		_shouldCheckInputModeConflict = !DewLaunchOptions.forceGamepad && !DewLaunchOptions.forceKeyboardAndMouse;
		_inputModeChangeTimes.Clear();
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			SetInputMode(InputMode.KeyboardAndMouse);
		}
		if (DewLaunchOptions.forceGamepad)
		{
			SetInputMode(InputMode.Gamepad);
		}
	}

	public static void ListenAnyButtonRaw(UnityEngine.Object caller, Func<InputControl, bool> callback)
	{
		StopListenAnyButton();
		_currentAnyButtonListener = caller;
		_currentAnyButtonListenerAction = delegate(InputControl ctrl)
		{
			if (_currentAnyButtonListener == null)
			{
				StopListenAnyButton();
				return;
			}
			try
			{
				if (callback(ctrl))
				{
					StopListenAnyButton();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				StopListenAnyButton();
			}
		};
		_currentAnyButtonListenerDisposable = InputSystem.onAnyButtonPress.Call(_currentAnyButtonListenerAction);
	}

	public static string GetReadableTextOfGamepad(DewBinding b, out BindingType type)
	{
		if (b.HasGamepadAssigned())
		{
			type = BindingType.Gamepad;
			if (GamepadButtonReadableTextBindings.TryGetValue(b.gamepadBinds[0], out var val))
			{
				return val;
			}
			return b.gamepadBinds[0].ToString();
		}
		type = BindingType.None;
		return DewLocalization.GetUIValue("Key_None");
	}

	public static string GetReadableTextOfGamepad(DewBinding b)
	{
		BindingType type;
		return GetReadableTextOfGamepad(b, out type);
	}

	public static string GetReadableTextOfPC(DewBinding b, out BindingType type, int i = 0)
	{
		string result = "";
		if (b.HasPCAssigned())
		{
			if (b.pcBinds[i].mouse != MouseButton.None)
			{
				type = BindingType.Mouse;
				result = b.pcBinds[i].mouse.GetReadableText();
			}
			else
			{
				if (b.pcBinds[i].key == Key.None)
				{
					type = BindingType.None;
					return DewLocalization.GetUIValue("Key_None");
				}
				type = BindingType.Keyboard;
				result = b.pcBinds[i].key.GetReadableText();
			}
			if (b.pcBinds[i].modifiers.Count > 0)
			{
				foreach (Key k in b.pcBinds[0].modifiers)
				{
					result = ((!KeyReadableTextBindings.TryGetValue(k, out var val)) ? $"{k} + {result}" : (val + " + " + result));
				}
			}
			return result;
		}
		type = BindingType.None;
		return DewLocalization.GetUIValue("Key_None");
	}

	public static string GetReadableTextOfPC(DewBinding b, int i = 0)
	{
		BindingType type;
		return GetReadableTextOfPC(b, out type, i);
	}

	public static string GetReadableTextForCurrentMode(DewBinding b)
	{
		BindingType type;
		return GetReadableTextForCurrentMode(b, out type);
	}

	public static string GetReadableTextForCurrentMode(DewBinding b, out BindingType type)
	{
		if (currentMode == InputMode.Gamepad)
		{
			return GetReadableTextOfGamepad(b, out type);
		}
		return GetReadableTextOfPC(b, out type);
	}

	public static bool GetButtonDownAnyKey()
	{
		if (Keyboard.current != null)
		{
			return Keyboard.current.anyKey.wasPressedThisFrame;
		}
		return false;
	}

	public static bool GetButtonDownAnyGamepad()
	{
		if (Gamepad.current == null)
		{
			return false;
		}
		for (int i = 0; i < Gamepad.current.allControls.Count; i++)
		{
			if (Gamepad.current.allControls[i] is ButtonControl { wasPressedThisFrame: not false })
			{
				return true;
			}
		}
		return false;
	}

	public static bool GetButtonDownAnyMouse()
	{
		if (Mouse.current == null)
		{
			return false;
		}
		for (int i = 0; i < Mouse.current.allControls.Count; i++)
		{
			if (Mouse.current.allControls[i] is ButtonControl { wasPressedThisFrame: not false })
			{
				return true;
			}
		}
		return false;
	}

	public static void SetInputMode(InputMode mode)
	{
		if (DewLaunchOptions.forceGamepad)
		{
			mode = InputMode.Gamepad;
		}
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			mode = InputMode.KeyboardAndMouse;
		}
		if (currentMode == mode)
		{
			return;
		}
		if (_shouldCheckInputModeConflict)
		{
			_inputModeChangeTimes.Add(Time.unscaledTime);
			if (_inputModeChangeTimes.Count > 10)
			{
				_inputModeChangeTimes.RemoveAt(0);
				if (_inputModeChangeTimes.All((float t) => Time.unscaledTime - t < 3f))
				{
					_shouldCheckInputModeConflict = false;
					DewLaunchOptions.forceKeyboardAndMouse = true;
					mode = InputMode.KeyboardAndMouse;
					ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
					{
						rawContent = DewLocalization.GetUIValue("Message_ControlConflictConfirmation"),
						buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
						defaultButton = DewMessageSettings.ButtonType.No,
						onClose = delegate(DewMessageSettings.ButtonType b)
						{
							if (b == DewMessageSettings.ButtonType.Yes)
							{
								DewLaunchOptions.forceKeyboardAndMouse = true;
								ManagerBase<MessageManager>.instance.ShowMessageLocalized("Message_ControlConflictConfirmation_Confirmed");
							}
							else
							{
								DewLaunchOptions.forceKeyboardAndMouse = false;
							}
						}
					});
				}
			}
		}
		switch (mode)
		{
		case InputMode.KeyboardAndMouse:
			Mouse.current.WarpCursorPosition(new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f));
			Cursor.visible = true;
			break;
		case InputMode.Gamepad:
			Mouse.current.WarpCursorPosition(new Vector2((float)Screen.width * 0.02f, (float)Screen.height * 0.02f));
			Cursor.visible = false;
			break;
		}
		InputMode last = currentMode;
		currentMode = mode;
		try
		{
			onCurrentModeChanged?.Invoke(last, mode);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private static ButtonControl GetMouseButtonControl_Imp(MouseButton m)
	{
		return m switch
		{
			MouseButton.None => null, 
			MouseButton.Left => Mouse.current.leftButton, 
			MouseButton.Right => Mouse.current.rightButton, 
			MouseButton.Middle => Mouse.current.middleButton, 
			MouseButton.Forward => Mouse.current.forwardButton, 
			MouseButton.Back => Mouse.current.backButton, 
			_ => throw new ArgumentOutOfRangeException("m", m, null), 
		};
	}

	internal static void UpdateScrollAxis()
	{
		if (_mouseScrollAxisFrameCount != Time.frameCount)
		{
			_mouseScrollAxisFrameCount = Time.frameCount;
			_previousFrameMouseScrollAxis = _currentFrameMouseScrollAxis;
			_currentFrameMouseScrollAxis = Input.GetAxis("Mouse ScrollWheel");
		}
	}

	internal static bool GetMouseButtonDown_Imp(MouseButton m)
	{
		UpdateScrollAxis();
		switch (m)
		{
		case MouseButton.Left:
		case MouseButton.Right:
		case MouseButton.Middle:
		case MouseButton.Forward:
		case MouseButton.Back:
			return GetMouseButtonControl_Imp(m).wasPressedThisFrame;
		case MouseButton.ScrollUp:
			return _currentFrameMouseScrollAxis > 0f;
		case MouseButton.ScrollDown:
			return _currentFrameMouseScrollAxis < 0f;
		default:
			return false;
		}
	}

	internal static bool GetMouseButton_Imp(MouseButton m)
	{
		UpdateScrollAxis();
		switch (m)
		{
		case MouseButton.Left:
		case MouseButton.Right:
		case MouseButton.Middle:
		case MouseButton.Forward:
		case MouseButton.Back:
			return GetMouseButtonControl_Imp(m).isPressed;
		case MouseButton.ScrollUp:
			return _currentFrameMouseScrollAxis > 0f;
		case MouseButton.ScrollDown:
			return _currentFrameMouseScrollAxis < 0f;
		default:
			return false;
		}
	}

	internal static bool GetMouseButtonUp_Imp(MouseButton m)
	{
		UpdateScrollAxis();
		switch (m)
		{
		case MouseButton.Left:
		case MouseButton.Right:
		case MouseButton.Middle:
		case MouseButton.Forward:
		case MouseButton.Back:
			return GetMouseButtonControl_Imp(m).wasReleasedThisFrame;
		case MouseButton.ScrollUp:
			if (_previousFrameMouseScrollAxis > 0f)
			{
				return _currentFrameMouseScrollAxis == 0f;
			}
			return false;
		case MouseButton.ScrollDown:
			if (_previousFrameMouseScrollAxis < 0f)
			{
				return _currentFrameMouseScrollAxis == 0f;
			}
			return false;
		default:
			return false;
		}
	}

	public static Vector2 GetLeftJoystick()
	{
		return GetJoystick(isLeft: true, DewSave.profile.controls.joystick0Dead, DewSave.profile.controls.joystick0Max);
	}

	public static Vector2 GetRightJoystick()
	{
		return GetJoystick(isLeft: false, DewSave.profile.controls.joystick1Dead, DewSave.profile.controls.joystick1Max);
	}

	private static Vector2 GetJoystick(bool isLeft, float dead, float max)
	{
		if (Gamepad.current == null || DewLaunchOptions.forceKeyboardAndMouse)
		{
			return Vector2.zero;
		}
		Vector2 axisVec = (isLeft ? Gamepad.current.leftStick.value : Gamepad.current.rightStick.value);
		if (axisVec.magnitude < dead)
		{
			return Vector2.zero;
		}
		Vector2 res = axisVec.normalized * Mathf.Clamp01((axisVec.magnitude - dead) / (max - dead));
		if (ControlManager.AreControlsInverted())
		{
			res *= -1f;
		}
		return res;
	}

	public static bool GetButtonDown(Key k)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Keyboard.current == null || k == Key.None)
		{
			return false;
		}
		if (Keyboard.current[k].wasPressedThisFrame)
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButtonDown(MouseButton m, bool checkGameArea)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Mouse.current == null || m == MouseButton.None)
		{
			return false;
		}
		if (checkGameArea && (m == MouseButton.Left || m == MouseButton.Right) && !IsGameRelatedMouseInputValid(m))
		{
			return false;
		}
		if (GetMouseButtonDown_Imp(m))
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButtonDown(GamepadButtonEx? g)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		if (Gamepad.current == null || !g.HasValue)
		{
			return false;
		}
		if (ManagerBase<InputManager>.instance.IsStickDirection(g.Value))
		{
			if (ManagerBase<InputManager>.instance.GetStickDirectionDown(g.Value))
			{
				SetInputMode(InputMode.Gamepad);
				return true;
			}
		}
		else if (Gamepad.current[(GamepadButton)g.Value].wasPressedThisFrame)
		{
			SetInputMode(InputMode.Gamepad);
			return true;
		}
		return false;
	}

	public static bool GetButtonDown(DewBinding b, bool checkGameAreaForMouse)
	{
		if (b.HasPCAssigned())
		{
			foreach (PCBind item in b.pcBinds)
			{
				if (item.mouse != MouseButton.None && GetButtonDown(item.mouse, checkGameAreaForMouse))
				{
					return true;
				}
				if (item.key != Key.None && GetButtonDown(item.key))
				{
					return true;
				}
			}
		}
		if (b.HasGamepadAssigned())
		{
			using List<GamepadButtonEx>.Enumerator enumerator2 = b.gamepadBinds.GetEnumerator();
			if (enumerator2.MoveNext())
			{
				return GetButtonDown(enumerator2.Current);
			}
		}
		return false;
	}

	public static bool GetButton(Key k)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Keyboard.current == null || k == Key.None)
		{
			return false;
		}
		if (Keyboard.current[k].isPressed)
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButton(MouseButton m, bool checkGameArea)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Mouse.current == null || m == MouseButton.None)
		{
			return false;
		}
		if (checkGameArea && (m == MouseButton.Left || m == MouseButton.Right) && !IsGameRelatedMouseInputValid(m))
		{
			return false;
		}
		if (GetMouseButton_Imp(m))
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButton(GamepadButtonEx? g)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		if (Gamepad.current == null || !g.HasValue)
		{
			return false;
		}
		if (ManagerBase<InputManager>.instance.IsStickDirection(g.Value))
		{
			if (ManagerBase<InputManager>.instance.GetStickDirection(g.Value))
			{
				SetInputMode(InputMode.Gamepad);
				return true;
			}
		}
		else if (Gamepad.current[(GamepadButton)g.Value].isPressed)
		{
			SetInputMode(InputMode.Gamepad);
			return true;
		}
		return false;
	}

	public static bool GetButton(DewBinding b, bool checkGameAreaForMouse)
	{
		if (b.HasPCAssigned())
		{
			foreach (PCBind item in b.pcBinds)
			{
				if (item.mouse != MouseButton.None && GetButton(item.mouse, checkGameAreaForMouse))
				{
					return true;
				}
				if (item.key != Key.None && GetButton(item.key))
				{
					return true;
				}
			}
		}
		if (b.HasGamepadAssigned())
		{
			using List<GamepadButtonEx>.Enumerator enumerator2 = b.gamepadBinds.GetEnumerator();
			if (enumerator2.MoveNext())
			{
				return GetButton(enumerator2.Current);
			}
		}
		return false;
	}

	public static bool GetButtonUp(Key k)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Keyboard.current == null || k == Key.None)
		{
			return false;
		}
		if (Keyboard.current[k].wasReleasedThisFrame)
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButtonUp(MouseButton m, bool checkGameArea)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceGamepad)
		{
			return false;
		}
		if (Mouse.current == null || m == MouseButton.None)
		{
			return false;
		}
		if (checkGameArea && (m == MouseButton.Left || m == MouseButton.Right) && !IsGameRelatedMouseInputValid(m))
		{
			return false;
		}
		if (GetMouseButtonUp_Imp(m))
		{
			SetInputMode(InputMode.KeyboardAndMouse);
			return true;
		}
		return false;
	}

	public static bool GetButtonUp(GamepadButtonEx? g)
	{
		if (isInputDisabled)
		{
			return false;
		}
		if (DewLaunchOptions.forceKeyboardAndMouse)
		{
			return false;
		}
		if (Gamepad.current == null || !g.HasValue)
		{
			return false;
		}
		if (ManagerBase<InputManager>.instance.IsStickDirection(g.Value))
		{
			if (ManagerBase<InputManager>.instance.GetStickDirectionUp(g.Value))
			{
				SetInputMode(InputMode.Gamepad);
				return true;
			}
		}
		else if (Gamepad.current[(GamepadButton)g.Value].wasReleasedThisFrame)
		{
			SetInputMode(InputMode.Gamepad);
			return true;
		}
		return false;
	}

	public static bool GetButtonUp(DewBinding b, bool checkGameAreaForMouse)
	{
		if (b.HasPCAssigned())
		{
			foreach (PCBind item in b.pcBinds)
			{
				if (item.mouse != MouseButton.None && GetButtonUp(item.mouse, checkGameAreaForMouse))
				{
					return true;
				}
				if (item.key != Key.None && GetButtonUp(item.key))
				{
					return true;
				}
			}
		}
		if (b.HasGamepadAssigned())
		{
			using List<GamepadButtonEx>.Enumerator enumerator2 = b.gamepadBinds.GetEnumerator();
			if (enumerator2.MoveNext())
			{
				return GetButtonUp(enumerator2.Current);
			}
		}
		return false;
	}

	public static bool IsGameRelatedMouseInputValid(MouseButton button)
	{
		if (button == MouseButton.Forward || button == MouseButton.Back)
		{
			return true;
		}
		if (Time.frameCount != _isMouseClickValidFrame)
		{
			if (SingletonBehaviour<GameArea>.softInstance == null)
			{
				_isMouseClickValid = ValidStatus.Yes;
			}
			else
			{
				Dew.RaycastAllUIElementsBelowCursor(_isMouseClickValidHits);
				_isMouseClickValid = ValidStatus.No;
				bool exceptLeft = false;
				foreach (RaycastResult hit in _isMouseClickValidHits)
				{
					UI_GameInputPassThrough pass = hit.gameObject.GetComponentInParent<UI_GameInputPassThrough>();
					if (pass != null)
					{
						if (pass.exceptLeftClick)
						{
							exceptLeft = true;
						}
						continue;
					}
					if (hit.gameObject == SingletonBehaviour<GameArea>.instance.gameObject)
					{
						_isMouseClickValid = ((!exceptLeft) ? ValidStatus.Yes : ValidStatus.YesExceptLeft);
					}
					break;
				}
				_isMouseClickValidFrame = Time.frameCount;
			}
		}
		if (_isMouseClickValid != ValidStatus.Yes)
		{
			if (_isMouseClickValid == ValidStatus.YesExceptLeft)
			{
				return button != MouseButton.Left;
			}
			return false;
		}
		return true;
	}
}
