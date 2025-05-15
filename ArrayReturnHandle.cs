using System.Buffers;
using UnityEngine;

public struct ArrayReturnHandle<T>
{
	private readonly T[] _resource;

	private bool _isReturned;

	internal ArrayReturnHandle(T[] resource)
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
		ArrayPool<T>.Shared.Return(_resource);
		_isReturned = true;
	}
}
