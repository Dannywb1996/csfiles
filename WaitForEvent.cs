using System;
using UnityEngine;
using UnityEngine.Events;

public class WaitForEvent : CustomYieldInstruction
{
	private Action _subscribeTarget;

	private UnityEvent _subscribeUnityEvent;

	private bool _keepWaiting = true;

	public override bool keepWaiting => _keepWaiting;

	public WaitForEvent(UnityEvent subscribeTarget)
	{
		_subscribeUnityEvent = subscribeTarget;
		_subscribeUnityEvent.AddListener(OnCallback);
	}

	private void OnCallback()
	{
		_keepWaiting = false;
		if (_subscribeTarget != null)
		{
			_subscribeTarget = (Action)Delegate.Remove(_subscribeTarget, new Action(OnCallback));
		}
		if (_subscribeUnityEvent != null)
		{
			_subscribeUnityEvent.RemoveListener(OnCallback);
		}
	}
}
public class WaitForEvent<T> : CustomYieldInstruction
{
	private Action<T> _subscribeTarget;

	private UnityEvent<T> _subscribeUnityEvent;

	private bool _keepWaiting = true;

	public override bool keepWaiting => _keepWaiting;

	public WaitForEvent(UnityEvent<T> subscribeTarget)
	{
		_subscribeUnityEvent = subscribeTarget;
		_subscribeUnityEvent.AddListener(OnCallback);
	}

	private void OnCallback(T _)
	{
		_keepWaiting = false;
		if (_subscribeTarget != null)
		{
			_subscribeTarget = (Action<T>)Delegate.Remove(_subscribeTarget, new Action<T>(OnCallback));
		}
		if (_subscribeUnityEvent != null)
		{
			_subscribeUnityEvent.RemoveListener(OnCallback);
		}
	}
}
