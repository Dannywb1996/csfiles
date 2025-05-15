using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UI_EmoteWheel : SingletonBehaviour<UI_EmoteWheel>, ISettingsChangedCallback
{
	public UI_EmoteWheel_Item[] wheelItems;

	public DewInputTrigger it_emote;

	public DewInputTrigger it_emoteConfirm;

	public DewInputTrigger it_emoteCancel;

	public UILineRenderer lineRenderer;

	public GameObject fxFocusChanged;

	public GameObject fxShow;

	public GameObject fxHide;

	private CanvasGroup _cg;

	private int _previousFocused = -1;

	internal bool _gpeIsHolding;

	internal bool _gpeIsRightStick;

	internal float _gpeStartUnscaledTime = float.NegativeInfinity;

	internal float _gpeEndUnscaledTime = float.NegativeInfinity;

	internal bool _gpeIsEmoteMode;

	public bool isShown => _cg.alpha > 0.05f;

	private void Start()
	{
		_cg = GetComponent<CanvasGroup>();
		it_emote = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.emote,
			isValidCheck = CheckValid,
			checkGameAreaForMouse = true,
			priority = 0
		};
		it_emoteConfirm = new DewInputTrigger
		{
			owner = this,
			binding = () => DewBinding.KeyboardAndMouseOnly(MouseButton.Left),
			isValidCheck = () => isShown,
			checkGameAreaForMouse = false,
			priority = -10
		};
		it_emoteCancel = new DewInputTrigger
		{
			owner = this,
			binding = () => DewBinding.KeyboardAndMouseOnly(MouseButton.Right),
			isValidCheck = () => isShown,
			checkGameAreaForMouse = false,
			priority = -10
		};
		DewInput.onCurrentModeChanged = (Action<InputMode, InputMode>)Delegate.Combine(DewInput.onCurrentModeChanged, new Action<InputMode, InputMode>(OnCurrentModeChanged));
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 5000, delegate
		{
			if (_gpeIsHolding && _gpeIsEmoteMode)
			{
				_gpeEndUnscaledTime = Time.unscaledTime;
				_gpeIsHolding = false;
				Hide();
				return true;
			}
			if (isShown)
			{
				Hide();
				return true;
			}
			return false;
		});
		SetupItems();
		_cg.alpha = 0f;
		Hide();
	}

	private void OnDestroy()
	{
		DewInput.onCurrentModeChanged = (Action<InputMode, InputMode>)Delegate.Remove(DewInput.onCurrentModeChanged, new Action<InputMode, InputMode>(OnCurrentModeChanged));
	}

	private void OnCurrentModeChanged(InputMode arg1, InputMode arg2)
	{
		if (isShown)
		{
			Hide();
		}
	}

	public bool CheckValid()
	{
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			return false;
		}
		if (NetworkedManagerBase<GameManager>.softInstance != null)
		{
			if (ManagerBase<ControlManager>.instance.shouldProcessCharacterInputAllowKnockedOut)
			{
				return ManagerBase<GlobalUIManager>.instance.focused == null;
			}
			return false;
		}
		if (LobbyUIManager.softInstance != null)
		{
			if (LobbyUIManager.instance.IsState("Lobby"))
			{
				return !ControlManager.IsInputFieldFocused();
			}
			return false;
		}
		return false;
	}

	private void Update()
	{
		if (it_emote.down)
		{
			Show();
		}
		if (DewInput.currentMode == InputMode.Gamepad && (ManagerBase<ControlManager>.softInstance == null || ManagerBase<ControlManager>.softInstance.shouldProcessCharacterInputAllowKnockedOut || ManagerBase<ControlManager>.softInstance.ShouldProcessPingInput()))
		{
			GetCharInputOfGamepadPingAndEmote();
		}
		if (!isShown)
		{
			return;
		}
		Vector2 cursorPos = GetWheelCursorPosition();
		lineRenderer.Points = new Vector2[2]
		{
			Vector2.zero,
			base.transform.InverseTransformPoint(cursorPos)
		};
		int closestIndex = GetClosestEmoteIndex(cursorPos);
		if (_previousFocused != closestIndex)
		{
			_previousFocused = closestIndex;
			DewEffect.Play(fxFocusChanged);
			for (int i = 0; i < wheelItems.Length; i++)
			{
				wheelItems[i].SetHighlight(i == closestIndex);
			}
		}
		if (it_emote.up || it_emoteConfirm.down)
		{
			if (!string.IsNullOrEmpty(DewSave.profile.equippedEmotes[closestIndex]))
			{
				NetworkedManagerBase<ChatManager>.instance.CmdSendEmote(DewSave.profile.equippedEmotes[closestIndex]);
			}
			Hide();
		}
		if (it_emoteCancel.down)
		{
			Hide();
		}
	}

	private void SetupItems()
	{
		for (int i = 0; i < wheelItems.Length; i++)
		{
			if (i >= DewSave.profile.equippedEmotes.Count)
			{
				wheelItems[i].Setup(null);
			}
			else
			{
				wheelItems[i].Setup(DewSave.profile.equippedEmotes[i]);
			}
		}
	}

	private void Hide()
	{
		DewEffect.Play(fxHide);
		_cg.DOKill();
		_cg.alpha = 0f;
		_cg.blocksRaycasts = false;
		lineRenderer.enabled = false;
	}

	public void Show()
	{
		DewEffect.Play(fxShow);
		SetupItems();
		_cg.DOKill(complete: true);
		_cg.DOFade(1f, 0.2f);
		base.transform.DOKill(complete: true);
		base.transform.localScale = Vector3.one * 0.9f;
		base.transform.DOScale(1f, 0.2f);
		_cg.blocksRaycasts = true;
		if (DewInput.currentMode == InputMode.KeyboardAndMouse)
		{
			base.transform.position = Input.mousePosition;
		}
		else
		{
			base.transform.position = new Vector2((float)Screen.width / 2f, (float)Screen.height / 7f * 4f);
		}
		lineRenderer.enabled = true;
		lineRenderer.Points = new Vector2[2]
		{
			Vector2.zero,
			Vector2.zero
		};
		_previousFocused = -1;
	}

	public void OnSettingsChanged()
	{
		SetupItems();
	}

	private void GetCharInputOfGamepadPingAndEmote()
	{
		if (DewInput.currentMode != InputMode.Gamepad)
		{
			_gpeIsHolding = false;
		}
		else if (!_gpeIsHolding)
		{
			if (DewSave.profile.controls.leftJoystickClickAction == JoystickClickAction.PingAndEmotes && DewInput.GetButtonDown(GamepadButtonEx.LeftStick))
			{
				_gpeIsHolding = true;
				_gpeIsRightStick = false;
				_gpeStartUnscaledTime = Time.unscaledTime;
				_gpeIsEmoteMode = ManagerBase<ControlManager>.softInstance == null;
				if (_gpeIsEmoteMode)
				{
					Show();
				}
			}
			else if (DewSave.profile.controls.rightJoystickClickAction == JoystickClickAction.PingAndEmotes && DewInput.GetButtonDown(GamepadButtonEx.RightStick))
			{
				_gpeIsHolding = true;
				_gpeIsRightStick = true;
				_gpeStartUnscaledTime = Time.unscaledTime;
				_gpeIsEmoteMode = ManagerBase<ControlManager>.softInstance == null;
				if (_gpeIsEmoteMode)
				{
					Show();
				}
			}
		}
		else if (DewInput.GetButtonDown(GamepadButtonEx.B))
		{
			Hide();
			_gpeEndUnscaledTime = Time.unscaledTime;
			_gpeIsHolding = false;
		}
		else if (!DewInput.GetButton(_gpeIsRightStick ? GamepadButtonEx.RightStick : GamepadButtonEx.LeftStick))
		{
			if (!_gpeIsEmoteMode)
			{
				ManagerBase<ControlManager>.instance.SendPingGamepad();
			}
			else
			{
				int closest = GetClosestEmoteIndex(GetWheelCursorPosition());
				if (!string.IsNullOrEmpty(DewSave.profile.equippedEmotes[closest]))
				{
					NetworkedManagerBase<ChatManager>.instance.CmdSendEmote(DewSave.profile.equippedEmotes[closest]);
				}
				Hide();
			}
			_gpeEndUnscaledTime = Time.unscaledTime;
			_gpeIsHolding = false;
		}
		else if (!_gpeIsEmoteMode && Time.unscaledTime - _gpeStartUnscaledTime > 0.15f)
		{
			_gpeIsEmoteMode = true;
			Show();
		}
	}

	private Vector2 GetWheelCursorPosition()
	{
		if (DewInput.currentMode == InputMode.KeyboardAndMouse)
		{
			return Input.mousePosition;
		}
		float radius = ((RectTransform)base.transform).GetScreenSpaceRect().size.x * 0.65f;
		Vector2 joystick = (_gpeIsRightStick ? DewInput.GetRightJoystick() : DewInput.GetLeftJoystick());
		return (Vector2)base.transform.position + radius * joystick;
	}

	private int GetClosestEmoteIndex(Vector2 cursorPos)
	{
		return Dew.SelectBestIndexWithScore((IList<UI_EmoteWheel_Item>)wheelItems, (Func<UI_EmoteWheel_Item, int, float>)((UI_EmoteWheel_Item item, int _) => 1f / Vector2.Distance(cursorPos, item.transform.position)), 0f, (DewRandomInstance)null);
	}
}
