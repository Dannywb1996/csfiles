using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GamepadTextInput : SingletonBehaviour<UI_GamepadTextInput>
{
	public Action onIsShiftPressedChanged;

	[Multiline]
	public string qwertyLayout;

	public string qwertyShiftAlpha = "!@#$%^&*()";

	[Multiline]
	public string azertyLayout;

	public string azertyShiftAlpha = "1&é\"'(§è!çà";

	public TMP_InputField inputField;

	public Transform[] rowTransforms;

	public UI_GamepadTextInput_Key regularKeyPrefab;

	public UI_GamepadTextInput_Key leftShiftPrefab;

	public UI_GamepadTextInput_Key rightShiftPrefab;

	public UI_GamepadTextInput_Key backspacePrefab;

	public Button confirmButton;

	public Button clearButton;

	public Button cancelButton;

	private TMP_InputField _target;

	private Action _onConfirm;

	private Action _onCancel;

	private DewInputTrigger it_gamepadTextInputLeftShift;

	private DewInputTrigger it_gamepadTextInputRightShift;

	private DewInputTrigger it_gamepadTextInputBackspace;

	private UI_GamepadTextInput_Key _backspaceButton;

	public bool isAzerty { get; private set; }

	public bool isShiftPressed { get; private set; }

	private void Start()
	{
		SetAzertyMode(value: false);
		if (_target == null)
		{
			base.gameObject.SetActive(value: false);
		}
		confirmButton.onClick.AddListener(Confirm);
		clearButton.onClick.AddListener(Clear);
		cancelButton.onClick.AddListener(Cancel);
		it_gamepadTextInputLeftShift = new DewInputTrigger
		{
			owner = this,
			priority = -10,
			binding = () => DewSave.profile.controls.gamepadTextInputLeftShift,
			isValidCheck = () => base.gameObject.activeSelf
		};
		it_gamepadTextInputRightShift = new DewInputTrigger
		{
			owner = this,
			priority = -10,
			binding = () => DewSave.profile.controls.gamepadTextInputRightShift,
			isValidCheck = () => base.gameObject.activeSelf
		};
		it_gamepadTextInputBackspace = new DewInputTrigger
		{
			owner = this,
			priority = -10,
			binding = () => DewSave.profile.controls.gamepadTextInputBackspace,
			isValidCheck = () => base.gameObject.activeSelf
		};
		DewInput.onCurrentModeChanged = (Action<InputMode, InputMode>)Delegate.Combine(DewInput.onCurrentModeChanged, new Action<InputMode, InputMode>(OnCurrentModeChanged));
	}

	private void OnDestroy()
	{
		DewInput.onCurrentModeChanged = (Action<InputMode, InputMode>)Delegate.Remove(DewInput.onCurrentModeChanged, new Action<InputMode, InputMode>(OnCurrentModeChanged));
	}

	private void OnCurrentModeChanged(InputMode arg1, InputMode arg2)
	{
		if (arg2 == InputMode.KeyboardAndMouse && base.gameObject.activeInHierarchy)
		{
			Cancel();
		}
	}

	public void EraseOne()
	{
		if (inputField.text.Length > 0)
		{
			inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
		}
	}

	private void Update()
	{
		if (it_gamepadTextInputLeftShift.down || it_gamepadTextInputRightShift.down)
		{
			SetShift(value: true);
		}
		if (it_gamepadTextInputLeftShift.up || it_gamepadTextInputRightShift.up)
		{
			SetShift(value: false);
		}
		if (it_gamepadTextInputBackspace.downRepeated)
		{
			ManagerBase<GlobalUIManager>.instance.SimulateClickOnUIElement(_backspaceButton);
		}
	}

	public void StartInput(TMP_InputField target, Action onConfirm = null, Action onCancel = null)
	{
		if (!target.readOnly && target.isActiveAndEnabled && target.IsInteractable())
		{
			_target = target;
			_onConfirm = onConfirm;
			_onCancel = onCancel;
			inputField.lineType = target.lineType;
			inputField.lineLimit = target.lineLimit;
			inputField.text = target.text;
			if (target.placeholder is TextMeshProUGUI placeholder)
			{
				((TextMeshProUGUI)inputField.placeholder).text = placeholder.text;
			}
			else
			{
				((TextMeshProUGUI)inputField.placeholder).text = "";
			}
			inputField.characterLimit = target.characterLimit;
			inputField.characterValidation = target.characterValidation;
			isShiftPressed = false;
			onIsShiftPressedChanged?.Invoke();
			base.gameObject.SetActive(value: true);
		}
	}

	public void SetAzertyMode(bool value)
	{
		isAzerty = value;
		string[] rowsRaw = (value ? azertyLayout : qwertyLayout).Split("\n");
		bool didLeftShift = false;
		for (int rowIndex = 0; rowIndex < rowsRaw.Length; rowIndex++)
		{
			string[] row = rowsRaw[rowIndex].Trim().Split(" ");
			if (row.Length == 0)
			{
				continue;
			}
			for (int i = rowTransforms[rowIndex].childCount - 1; i >= 0; i--)
			{
				UnityEngine.Object.Destroy(rowTransforms[rowIndex].GetChild(i).gameObject);
			}
			string[] array = row;
			foreach (string k in array)
			{
				if (k == "Shift")
				{
					UnityEngine.Object.Instantiate(didLeftShift ? rightShiftPrefab : leftShiftPrefab, rowTransforms[rowIndex]).key = "Shift";
					didLeftShift = true;
				}
				else if (k == "Backspace")
				{
					UI_GamepadTextInput_Key b = UnityEngine.Object.Instantiate(backspacePrefab, rowTransforms[rowIndex]);
					b.key = "Backspace";
					_backspaceButton = b;
				}
				else
				{
					UnityEngine.Object.Instantiate(regularKeyPrefab, rowTransforms[rowIndex]).key = k;
				}
			}
		}
	}

	public void ToggleShift()
	{
		SetShift(!isShiftPressed);
	}

	public void SetShift(bool value)
	{
		if (isShiftPressed != value)
		{
			isShiftPressed = value;
			onIsShiftPressedChanged?.Invoke();
		}
	}

	public void PressCharacter(string key)
	{
		if (inputField.characterLimit <= 0 || inputField.text.Length < inputField.characterLimit)
		{
			inputField.text += (isShiftPressed ? GetShiftSubstitute(key) : key);
			inputField.ForceLabelUpdate();
		}
	}

	public string GetShiftSubstitute(string key)
	{
		if (int.TryParse(key, out var num))
		{
			return (isAzerty ? azertyShiftAlpha[num] : qwertyShiftAlpha[num]).ToString();
		}
		return key.ToUpper();
	}

	public void Cancel()
	{
		if (_target != null && _target.text != inputField.text)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				owner = this,
				validator = () => base.gameObject.activeSelf,
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
				defaultButton = DewMessageSettings.ButtonType.Cancel,
				destructiveConfirm = true,
				rawContent = DewLocalization.GetUIValue("GamepadTextInput_Message_ConfirmDiscardChanges"),
				onClose = delegate(DewMessageSettings.ButtonType b)
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						DoCancel();
					}
				}
			});
		}
		else
		{
			DoCancel();
		}
		void DoCancel()
		{
			_target = null;
			base.gameObject.SetActive(value: false);
			try
			{
				_onCancel?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void Confirm()
	{
		if (_target != null && _target.IsInteractable() && _target.isActiveAndEnabled)
		{
			_target.text = inputField.text;
		}
		_target = null;
		base.gameObject.SetActive(value: false);
		try
		{
			_onConfirm?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void Clear()
	{
		if (inputField.text.Length == 0)
		{
			return;
		}
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			owner = this,
			validator = () => base.gameObject.activeSelf,
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
			defaultButton = DewMessageSettings.ButtonType.Cancel,
			destructiveConfirm = true,
			rawContent = DewLocalization.GetUIValue("GamepadTextInput_Message_ConfirmClear"),
			onClose = delegate(DewMessageSettings.ButtonType b)
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					inputField.text = "";
				}
			}
		});
	}
}
