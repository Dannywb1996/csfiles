using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[Serializable]
public class DewBinding : ICloneable
{
	public static readonly DewBinding MockBinding = new DewBinding();

	public bool canAssignKeyboard;

	public bool canAssignMouse;

	public bool canAssignGamepad;

	public List<PCBind> pcBinds = new List<PCBind>();

	public List<GamepadButtonEx> gamepadBinds = new List<GamepadButtonEx>();

	[Obsolete]
	public List<Key> keyModifiers = new List<Key>();

	[Obsolete]
	public Key keyboard;

	[Obsolete]
	public MouseButton mouse;

	[Obsolete]
	public GamepadButtonEx? gamepad;

	public static DewBinding KeyboardOnly(params object[] keys)
	{
		DewBinding obj = new DewBinding
		{
			canAssignGamepad = false,
			canAssignKeyboard = true,
			canAssignMouse = false
		};
		AddBindings(obj, keys);
		return obj;
	}

	public static DewBinding KeyboardAndMouseOnly(params object[] keys)
	{
		DewBinding obj = new DewBinding
		{
			canAssignGamepad = false,
			canAssignKeyboard = true,
			canAssignMouse = true
		};
		AddBindings(obj, keys);
		return obj;
	}

	public static DewBinding GamepadOnly(params object[] keys)
	{
		DewBinding obj = new DewBinding
		{
			canAssignGamepad = true,
			canAssignKeyboard = false,
			canAssignMouse = false
		};
		AddBindings(obj, keys);
		return obj;
	}

	public static DewBinding PCAndGamepad(params object[] keys)
	{
		DewBinding obj = new DewBinding
		{
			canAssignGamepad = true,
			canAssignKeyboard = true,
			canAssignMouse = true
		};
		AddBindings(obj, keys);
		return obj;
	}

	private static void AddBindings(DewBinding b, object[] objs)
	{
		foreach (object o in objs)
		{
			if (o is Key key)
			{
				b.pcBinds.Add(key);
			}
			else if (o is MouseButton mb0)
			{
				b.pcBinds.Add(mb0);
			}
			else if (o is GamepadButtonEx gp)
			{
				b.gamepadBinds.Add(gp);
			}
			else if (o is GamepadButton gpp)
			{
				b.gamepadBinds.Add((GamepadButtonEx)gpp);
			}
			else if (o is Key[] keysWithModifiers)
			{
				AddObjWithModifiers(keysWithModifiers);
			}
			else if (o is object[] objWithModifiers)
			{
				AddObjWithModifiers(objWithModifiers);
			}
		}
		void AddObjWithModifiers(IEnumerable arr)
		{
			PCBind newBind = new PCBind();
			b.pcBinds.Add(newBind);
			foreach (object o2 in arr)
			{
				if (o2 is Key k)
				{
					if (DewInput.ModifierKeys.Contains(k))
					{
						newBind.modifiers.Add(k);
					}
					else
					{
						newBind.mouse = MouseButton.None;
						newBind.key = k;
					}
				}
				else if (o2 is MouseButton mb1)
				{
					newBind.key = Key.None;
					newBind.mouse = mb1;
				}
			}
		}
	}

	public object Clone()
	{
		DewBinding clone = (DewBinding)MemberwiseClone();
		clone.pcBinds = new List<PCBind>();
		foreach (PCBind k in pcBinds)
		{
			clone.pcBinds.Add((PCBind)k.Clone());
		}
		clone.gamepadBinds = new List<GamepadButtonEx>(gamepadBinds);
		clone.keyModifiers = new List<Key>(keyModifiers);
		return clone;
	}

	public bool HasAssignedForCurrentMode()
	{
		if (DewInput.currentMode != InputMode.Gamepad)
		{
			return HasPCAssigned();
		}
		return HasGamepadAssigned();
	}

	public bool HasPCAssigned()
	{
		if (canAssignMouse || canAssignKeyboard)
		{
			return pcBinds.Count > 0;
		}
		return false;
	}

	public bool HasGamepadAssigned()
	{
		if (canAssignGamepad)
		{
			return gamepadBinds.Count > 0;
		}
		return false;
	}

	public BindingType GetBindingTypes()
	{
		BindingType binds = BindingType.None;
		if (canAssignGamepad)
		{
			binds |= BindingType.Gamepad;
		}
		if (canAssignMouse)
		{
			binds |= BindingType.Mouse;
		}
		if (canAssignKeyboard)
		{
			binds |= BindingType.Keyboard;
		}
		return binds;
	}

	public DewBinding CloneWith(GamepadButtonEx added)
	{
		DewBinding obj = (DewBinding)Clone();
		obj.canAssignGamepad = true;
		obj.gamepadBinds.Add(added);
		return obj;
	}

	public DewBinding CloneWith(MouseButton added)
	{
		DewBinding obj = (DewBinding)Clone();
		obj.canAssignMouse = true;
		obj.pcBinds.Add(new PCBind(added));
		return obj;
	}

	public DewBinding CloneWith(Key added)
	{
		DewBinding obj = (DewBinding)Clone();
		obj.canAssignKeyboard = true;
		obj.pcBinds.Add(new PCBind(added));
		return obj;
	}
}
