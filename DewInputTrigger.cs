using System;
using UnityEngine;

public class DewInputTrigger
{
	internal enum State
	{
		None,
		Down,
		Held,
		Up
	}

	public static DewInputTrigger MockTrigger;

	public int priority;

	public UnityEngine.Object owner;

	public Func<DewBinding> binding;

	public Func<bool> isValidCheck;

	public bool checkGameAreaForMouse;

	public bool canConsume = true;

	public bool ignoreConsumeCheck;

	internal DewBinding _binding;

	internal bool _isValid;

	private bool _up;

	private bool _down;

	private bool _held;

	private bool _downRepeated;

	private float _nextDownRepeatedUnscaledTime = float.PositiveInfinity;

	internal bool _flag;

	private bool _prevFlag;

	internal bool _isSuppressed;

	public bool down
	{
		get
		{
			if (ManagerBase<InputManager>.instance == null)
			{
				return false;
			}
			ManagerBase<InputManager>.instance.PrepareInputs();
			return _down;
		}
	}

	public bool up
	{
		get
		{
			if (ManagerBase<InputManager>.instance == null)
			{
				return false;
			}
			ManagerBase<InputManager>.instance.PrepareInputs();
			return _up;
		}
	}

	public bool downRepeated
	{
		get
		{
			if (ManagerBase<InputManager>.instance == null)
			{
				return false;
			}
			ManagerBase<InputManager>.instance.PrepareInputs();
			return _downRepeated;
		}
	}

	public DewInputTrigger()
	{
		if (!(ManagerBase<InputManager>.instance == null))
		{
			ManagerBase<InputManager>.instance.AddTrigger(this);
		}
	}

	internal void Reset()
	{
		_flag = false;
	}

	internal int GetCheckPriorityOfGamepadBind(int index)
	{
		return priority * 100 + index;
	}

	internal int GetCheckPriorityOfPCBind(int index)
	{
		return priority * 100 + index - _binding.pcBinds[index].modifiers.Count * 10;
	}

	internal void Update()
	{
		_down = !_isSuppressed && !_prevFlag && _flag;
		_up = !_isSuppressed && _prevFlag && !_flag;
		_held = !_isSuppressed && _flag;
		_prevFlag = !_isSuppressed && _flag;
		if (!_flag)
		{
			_isSuppressed = false;
		}
		if (_down)
		{
			_downRepeated = true;
			_nextDownRepeatedUnscaledTime = Time.unscaledTime + 0.225f;
		}
		else if (_held && Time.unscaledTime > _nextDownRepeatedUnscaledTime)
		{
			_downRepeated = true;
			_nextDownRepeatedUnscaledTime += 0.07f;
		}
		else
		{
			_downRepeated = false;
		}
	}

	public static implicit operator bool(DewInputTrigger trg)
	{
		if (ManagerBase<InputManager>.instance == null)
		{
			return false;
		}
		ManagerBase<InputManager>.instance.PrepareInputs();
		return trg._held;
	}
}
