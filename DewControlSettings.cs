using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class DewControlSettings : ICloneable, IValidatableSettings
{
	public enum PresetType
	{
		MOBA,
		WASD
	}

	public AimAssistType targetAssist = AimAssistType.High;

	public bool attackByIssuingMoveOnEnemy;

	public bool autoTargetSelfIfPossible = true;

	public bool clickToCastInsteadOfHoldToCast;

	public int mouseSensitivity = -1;

	public DashDirection dashDirectionWhenDirectionalMovement;

	public DewBinding confirmCast = DewBinding.KeyboardAndMouseOnly(MouseButton.Left);

	public DewBinding move = DewBinding.KeyboardAndMouseOnly(MouseButton.Right);

	public DewBinding attackMoveNormal = DewBinding.KeyboardAndMouseOnly(Key.A);

	public DewBinding attackMoveImmediately = DewBinding.KeyboardAndMouseOnly(MouseButton.Left);

	public DewBinding attackMoveOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding attackInPlace = DewBinding.PCAndGamepad(GamepadButtonEx.Square);

	public DewBinding back = DewBinding.PCAndGamepad(GamepadButtonEx.B, Key.Escape);

	public DewBinding scoreboard = DewBinding.PCAndGamepad(GamepadButtonEx.DpadDown, Key.Tab);

	public DewBinding worldMap = DewBinding.PCAndGamepad(GamepadButtonEx.DpadRight, Key.M);

	public bool turnOffAimAssistMeleeDirectionalAttack;

	public bool attackMoveUseDistanceFromDestination = true;

	public DewBinding stop = DewBinding.PCAndGamepad(Key.S);

	public bool enableDirMoveKeys;

	public DewBinding moveUp = DewBinding.KeyboardOnly(Key.W);

	public DewBinding moveLeft = DewBinding.KeyboardOnly(Key.A);

	public DewBinding moveDown = DewBinding.KeyboardOnly(Key.S);

	public DewBinding moveRight = DewBinding.KeyboardOnly(Key.D);

	public DewBinding skillQ = DewBinding.PCAndGamepad(GamepadButtonEx.LeftTrigger, Key.Q);

	public DewBinding skillW = DewBinding.PCAndGamepad(GamepadButtonEx.LeftShoulder, Key.W);

	public DewBinding skillE = DewBinding.PCAndGamepad(GamepadButtonEx.RightShoulder, Key.E);

	public DewBinding skillR = DewBinding.PCAndGamepad(GamepadButtonEx.RightTrigger, Key.R);

	public DewBinding skillMovement = DewBinding.PCAndGamepad(GamepadButtonEx.A, Key.Space);

	public DewBinding skillQNormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillWNormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillENormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillRNormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillMovementNormal = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillQImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillWImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillEImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillRImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillMovementImmediately = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillQOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillWOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillEOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillROnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillMovementOnRelease = DewBinding.KeyboardAndMouseOnly();

	public DewBinding skillQSelf = DewBinding.KeyboardAndMouseOnly(new Key[2]
	{
		Key.LeftAlt,
		Key.Q
	});

	public DewBinding skillWSelf = DewBinding.KeyboardAndMouseOnly(new Key[2]
	{
		Key.LeftAlt,
		Key.W
	});

	public DewBinding skillESelf = DewBinding.KeyboardAndMouseOnly(new Key[2]
	{
		Key.LeftAlt,
		Key.E
	});

	public DewBinding skillRSelf = DewBinding.KeyboardAndMouseOnly(new Key[2]
	{
		Key.LeftAlt,
		Key.R
	});

	public DewBinding skillQEdit = DewBinding.KeyboardAndMouseOnly(Key.Digit1);

	public DewBinding skillWEdit = DewBinding.KeyboardAndMouseOnly(Key.Digit2);

	public DewBinding skillEEdit = DewBinding.KeyboardAndMouseOnly(Key.Digit3);

	public DewBinding skillREdit = DewBinding.KeyboardAndMouseOnly(Key.Digit4);

	public CastConfirmType defaultHeroAbilityCastType = CastConfirmType.Immediately;

	public CastConfirmType movementHeroAbilityCastType = CastConfirmType.Immediately;

	public DewBinding interact = DewBinding.PCAndGamepad(GamepadButtonEx.North, Key.F);

	public DewBinding interactAlt = DewBinding.PCAndGamepad(GamepadButtonEx.B, Key.G);

	public DewBinding editSkillHold = DewBinding.KeyboardAndMouseOnly(Key.LeftCtrl);

	public DewBinding editSkillToggle = DewBinding.PCAndGamepad(GamepadButtonEx.DpadUp);

	public DewBinding showDetails = DewBinding.PCAndGamepad(GamepadButtonEx.Select, Key.LeftAlt);

	public DewBinding zoomOut = DewBinding.PCAndGamepad(MouseButton.ScrollDown);

	public DewBinding zoomIn = DewBinding.PCAndGamepad(MouseButton.ScrollUp);

	public DewBinding chat = DewBinding.KeyboardAndMouseOnly(Key.Enter);

	public DewBinding ping = DewBinding.KeyboardAndMouseOnly(MouseButton.Middle);

	public DewBinding emote = DewBinding.KeyboardAndMouseOnly(Key.T);

	public DewBinding travelVote = DewBinding.PCAndGamepad(GamepadButtonEx.Select, Key.Tab);

	public DewBinding travelVoteCancel = DewBinding.PCAndGamepad(GamepadButtonEx.Start, Key.Escape);

	public DewBinding spectatorNextTarget = DewBinding.PCAndGamepad(GamepadButtonEx.A, Key.Space, MouseButton.Left);

	public DewBinding skip = DewBinding.PCAndGamepad(GamepadButtonEx.A, Key.Space);

	public DewBinding nextCategory = DewBinding.GamepadOnly(GamepadButtonEx.RightShoulder);

	public DewBinding prevCategory = DewBinding.GamepadOnly(GamepadButtonEx.LeftShoulder);

	public DewBinding gamepadTextInputLeftShift = DewBinding.GamepadOnly(GamepadButtonEx.LeftShoulder);

	public DewBinding gamepadTextInputRightShift = DewBinding.GamepadOnly(GamepadButtonEx.RightShoulder);

	public DewBinding gamepadTextInputBackspace = DewBinding.GamepadOnly(GamepadButtonEx.Square);

	public DewBinding menu = DewBinding.GamepadOnly(GamepadButtonEx.Start);

	public DewBinding selectInfoBar = DewBinding.GamepadOnly(GamepadButtonEx.DpadLeft);

	public DewBinding confirm = DewBinding.PCAndGamepad(GamepadButtonEx.A, MouseButton.Left);

	public DewBinding gamepadApply = DewBinding.GamepadOnly(GamepadButtonEx.Start);

	public DewBinding gamepadSecondary = DewBinding.GamepadOnly(GamepadButtonEx.North);

	public AimAssistType attackTargetAssistGamepad = AimAssistType.High;

	public JoystickClickAction leftJoystickClickAction = JoystickClickAction.Dodge;

	public JoystickClickAction rightJoystickClickAction = JoystickClickAction.PingAndEmotes;

	public float joystick0Dead = 0.1f;

	public float joystick1Dead = 0.1f;

	public float joystick0Max = 0.95f;

	public float joystick1Max = 0.95f;

	public bool gamepadAttackByAiming = true;

	public bool lockCursorInGame = true;

	public DewBinding GetSkillBinding(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => skillQ, 
			HeroSkillLocation.W => skillW, 
			HeroSkillLocation.E => skillE, 
			HeroSkillLocation.R => skillR, 
			HeroSkillLocation.Identity => DewBinding.MockBinding, 
			HeroSkillLocation.Movement => skillMovement, 
			_ => throw new ArgumentOutOfRangeException("type"), 
		};
	}

	public DewBinding GetSkillEditBinding(HeroSkillLocation type)
	{
		switch (type)
		{
		case HeroSkillLocation.Q:
			return skillQEdit;
		case HeroSkillLocation.W:
			return skillWEdit;
		case HeroSkillLocation.E:
			return skillEEdit;
		case HeroSkillLocation.R:
			return skillREdit;
		case HeroSkillLocation.Identity:
		case HeroSkillLocation.Movement:
			return DewBinding.MockBinding;
		default:
			throw new ArgumentOutOfRangeException("type");
		}
	}

	public object Clone()
	{
		object newObject = MemberwiseClone();
		FieldInfo[] fields = typeof(DewControlSettings).GetFields();
		foreach (FieldInfo f in fields)
		{
			if (!(f.FieldType != typeof(DewBinding)))
			{
				DewBinding original = (DewBinding)f.GetValue(newObject);
				f.SetValue(newObject, original.Clone());
			}
		}
		return newObject;
	}

	public string GetSettingsValueText(string keys, out BindingType type)
	{
		string[] array = keys.Split(",");
		foreach (string k in array)
		{
			FieldInfo field = typeof(DewControlSettings).GetField(k);
			if (!(field == null) && field.GetValue(DewSave.profile.controls) is DewBinding binding && binding.HasAssignedForCurrentMode())
			{
				return DewInput.GetReadableTextForCurrentMode(binding, out type);
			}
		}
		type = BindingType.None;
		return DewLocalization.GetUIValue("Key_None");
	}

	public object GetSettingsValue(string key)
	{
		FieldInfo field = typeof(DewControlSettings).GetField(key);
		if (field == null)
		{
			return null;
		}
		return field.GetValue(DewSave.profile.controls);
	}

	public string GetSettingsValueText(string keys)
	{
		BindingType type;
		return GetSettingsValueText(keys, out type);
	}

	public void Validate()
	{
		if (mouseSensitivity < 0)
		{
			mouseSensitivity = InputManager.GetMouseSensitivity();
		}
		mouseSensitivity = Mathf.Clamp(mouseSensitivity, 1, 20);
		FieldInfo[] fields = typeof(DewControlSettings).GetFields();
		DewControlSettings defaultSettings = new DewControlSettings();
		FieldInfo[] array = fields;
		foreach (FieldInfo f in array)
		{
			if (f.FieldType != typeof(DewBinding))
			{
				continue;
			}
			DewBinding defaultBinding = (DewBinding)f.GetValue(defaultSettings);
			if (!(f.GetValue(this) is DewBinding myBinding))
			{
				f.SetValue(this, defaultBinding);
				continue;
			}
			myBinding.canAssignKeyboard = defaultBinding.canAssignKeyboard;
			myBinding.canAssignGamepad = defaultBinding.canAssignGamepad;
			myBinding.canAssignMouse = defaultBinding.canAssignMouse;
			myBinding.gamepad = defaultBinding.gamepad;
			myBinding.gamepadBinds = new List<GamepadButtonEx>();
			foreach (GamepadButtonEx g in defaultBinding.gamepadBinds)
			{
				myBinding.gamepadBinds.Add(g);
			}
		}
	}

	public void ApplyPreset(PresetType type)
	{
		DewControlSettings def = new DewControlSettings();
		move = def.move;
		skillQ = def.skillQ;
		skillW = def.skillW;
		skillE = def.skillE;
		skillR = def.skillR;
		skillQNormal = def.skillQNormal;
		skillWNormal = def.skillWNormal;
		skillENormal = def.skillENormal;
		skillRNormal = def.skillRNormal;
		skillQSelf = def.skillQSelf;
		skillWSelf = def.skillWSelf;
		skillESelf = def.skillESelf;
		skillRSelf = def.skillRSelf;
		attackMoveImmediately = def.attackMoveImmediately;
		attackMoveNormal = def.attackMoveNormal;
		attackInPlace = def.attackInPlace;
		moveUp = def.moveUp;
		moveLeft = def.moveLeft;
		moveDown = def.moveDown;
		moveRight = def.moveRight;
		stop = def.stop;
		attackByIssuingMoveOnEnemy = false;
		clickToCastInsteadOfHoldToCast = false;
		switch (type)
		{
		case PresetType.MOBA:
			enableDirMoveKeys = false;
			break;
		case PresetType.WASD:
			enableDirMoveKeys = true;
			move.pcBinds.Clear();
			stop.pcBinds.Clear();
			attackMoveImmediately.pcBinds.Clear();
			attackMoveNormal.pcBinds.Clear();
			attackInPlace.pcBinds = new List<PCBind> { MouseButton.Left };
			skillQ.pcBinds = new List<PCBind> { MouseButton.Right };
			skillW.pcBinds = new List<PCBind> { Key.Q };
			skillE.pcBinds = new List<PCBind> { Key.E };
			skillR.pcBinds = new List<PCBind> { Key.R };
			skillQNormal.pcBinds = new List<PCBind>
			{
				new PCBind(MouseButton.Right, Key.LeftShift)
			};
			skillWNormal.pcBinds = new List<PCBind>
			{
				new PCBind(Key.Q, Key.LeftShift)
			};
			skillENormal.pcBinds = new List<PCBind>
			{
				new PCBind(Key.E, Key.LeftShift)
			};
			skillRNormal.pcBinds = new List<PCBind>
			{
				new PCBind(Key.R, Key.LeftShift)
			};
			skillQSelf.pcBinds = new List<PCBind>
			{
				new PCBind(MouseButton.Right, Key.LeftAlt)
			};
			skillWSelf.pcBinds = new List<PCBind>
			{
				new PCBind(Key.Q, Key.LeftAlt)
			};
			skillESelf.pcBinds = new List<PCBind>
			{
				new PCBind(Key.E, Key.LeftAlt)
			};
			skillRSelf.pcBinds = new List<PCBind>
			{
				new PCBind(Key.R, Key.LeftAlt)
			};
			clickToCastInsteadOfHoldToCast = true;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}
}
