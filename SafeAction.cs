using System;
using System.Collections.Generic;
using UnityEngine;

public class SafeAction
{
	private readonly List<Action> _handlers = new List<Action>();

	public void Invoke()
	{
		ListReturnHandle<Action> handle;
		List<Action> list = DewPool.GetList(out handle);
		list.AddRange(_handlers);
		foreach (Action h in list)
		{
			try
			{
				h();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		handle.Return();
	}

	public static SafeAction operator +(SafeAction e, Action handler)
	{
		if (e == null)
		{
			e = new SafeAction();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction operator -(SafeAction e, Action handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}
}
public class SafeAction<T>
{
	private readonly List<Action<T>> _handlers = new List<Action<T>>();

	public void Invoke(T arg)
	{
		ListReturnHandle<Action<T>> handle;
		List<Action<T>> list = DewPool.GetList(out handle);
		list.AddRange(_handlers);
		foreach (Action<T> h in list)
		{
			try
			{
				h(arg);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		handle.Return();
	}

	public static SafeAction<T> operator +(SafeAction<T> e, Action<T> handler)
	{
		if (e == null)
		{
			e = new SafeAction<T>();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction<T> operator -(SafeAction<T> e, Action<T> handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}
}
public class SafeAction<T1, T2>
{
	private readonly List<Action<T1, T2>> _handlers = new List<Action<T1, T2>>();

	public void Invoke(T1 a, T2 b)
	{
		ListReturnHandle<Action<T1, T2>> handle;
		List<Action<T1, T2>> list = DewPool.GetList(out handle);
		list.AddRange(_handlers);
		foreach (Action<T1, T2> h in list)
		{
			try
			{
				h(a, b);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		handle.Return();
	}

	public static SafeAction<T1, T2> operator +(SafeAction<T1, T2> e, Action<T1, T2> handler)
	{
		if (e == null)
		{
			e = new SafeAction<T1, T2>();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction<T1, T2> operator -(SafeAction<T1, T2> e, Action<T1, T2> handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}
}
public class SafeAction<T1, T2, T3>
{
	private readonly List<Action<T1, T2, T3>> _handlers = new List<Action<T1, T2, T3>>();

	public void Invoke(T1 a, T2 b, T3 c)
	{
		ListReturnHandle<Action<T1, T2, T3>> handle;
		List<Action<T1, T2, T3>> list = DewPool.GetList(out handle);
		list.AddRange(_handlers);
		foreach (Action<T1, T2, T3> h in list)
		{
			try
			{
				h(a, b, c);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		handle.Return();
	}

	public static SafeAction<T1, T2, T3> operator +(SafeAction<T1, T2, T3> e, Action<T1, T2, T3> handler)
	{
		if (e == null)
		{
			e = new SafeAction<T1, T2, T3>();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction<T1, T2, T3> operator -(SafeAction<T1, T2, T3> e, Action<T1, T2, T3> handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}
}
public class SafeAction<T1, T2, T3, T4>
{
	private readonly List<Action<T1, T2, T3, T4>> _handlers = new List<Action<T1, T2, T3, T4>>();

	public void Invoke(T1 a, T2 b, T3 c, T4 d)
	{
		ListReturnHandle<Action<T1, T2, T3, T4>> handle;
		List<Action<T1, T2, T3, T4>> list = DewPool.GetList(out handle);
		list.AddRange(_handlers);
		foreach (Action<T1, T2, T3, T4> h in list)
		{
			try
			{
				h(a, b, c, d);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		handle.Return();
	}

	public static SafeAction<T1, T2, T3, T4> operator +(SafeAction<T1, T2, T3, T4> e, Action<T1, T2, T3, T4> handler)
	{
		if (e == null)
		{
			e = new SafeAction<T1, T2, T3, T4>();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction<T1, T2, T3, T4> operator -(SafeAction<T1, T2, T3, T4> e, Action<T1, T2, T3, T4> handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}
}
