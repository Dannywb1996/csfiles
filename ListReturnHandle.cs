using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public struct ListReturnHandle<T>
{
	private readonly List<T> _resource;

	private bool _isReturned;

	public bool needToReturn
	{
		get
		{
			if (_resource != null)
			{
				return !_isReturned;
			}
			return false;
		}
	}

	internal ListReturnHandle(List<T> resource)
	{
		_resource = resource;
		_isReturned = false;
	}

	public void Return()
	{
		if (_isReturned)
		{
			Debug.LogWarning("Tried to return a resource twice");
			return;
		}
		_resource.Clear();
		CollectionPool<List<T>, T>.Release(_resource);
		_isReturned = true;
	}
}
