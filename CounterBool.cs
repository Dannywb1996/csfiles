using System;
using UnityEngine;

public struct CounterBool
{
	private int _count;

	public int count => _count;

	public CounterBool(int count)
	{
		_count = count;
	}

	public static implicit operator bool(CounterBool v)
	{
		return v.count > 0;
	}

	public static CounterBool operator +(CounterBool v, int value)
	{
		if (value != 1)
		{
			throw new InvalidOperationException("Incrementing unexpected value");
		}
		if (v._count == -1)
		{
			Debug.Log("Negative counter resolved");
		}
		return new CounterBool(v._count + value);
	}

	public static CounterBool operator -(CounterBool v, int value)
	{
		if (value != 1)
		{
			throw new InvalidOperationException("Decrementing unexpected value");
		}
		if (v._count == 0)
		{
			Debug.LogException(new InvalidOperationException("Decrementing into negative counter value"));
		}
		return new CounterBool(v._count - value);
	}
}
