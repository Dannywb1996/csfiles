using System;
using UnityEngine;

public class WaitForPromise : CustomYieldInstruction
{
	public delegate void ResolveRejectAction(Action resolve, Action<Exception> reject);

	private bool _keepWaiting = true;

	private Exception _exception;

	public override bool keepWaiting
	{
		get
		{
			if (_exception != null)
			{
				throw _exception;
			}
			return _keepWaiting;
		}
	}

	public WaitForPromise(ResolveRejectAction init)
	{
		init?.Invoke(Resolve, Reject);
	}

	private void Resolve()
	{
		_keepWaiting = false;
	}

	private void Reject(Exception exception)
	{
		_exception = exception;
	}
}
