using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : ManagerBase<UIManager>
{
	[SerializeField]
	private string _state;

	[SerializeField]
	private List<string> _availableStates = new List<string>();

	public Action<string, string> onStateChanged;

	public string state => _state;

	public IReadOnlyCollection<string> availableStates => _availableStates;

	public bool IsState(string state)
	{
		return _state == state;
	}

	public void SetState(string newState)
	{
		if (!_availableStates.Contains(newState))
		{
			throw new ArgumentOutOfRangeException("Invalid state provided in SetState: " + newState);
		}
		if (_state == newState)
		{
			return;
		}
		string oldState = _state;
		_state = newState;
		try
		{
			onStateChanged?.Invoke(oldState, state);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			OnStateChanged(oldState, newState);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	protected virtual void OnStateChanged(string oldState, string newState)
	{
	}

	public virtual bool ShouldDoAutoFocus()
	{
		return true;
	}
}
