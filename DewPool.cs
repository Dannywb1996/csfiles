using System.Buffers;
using System.Collections.Generic;
using UnityEngine.Pool;

public static class DewPool
{
	public static T[] GetArray<T>(out ArrayReturnHandle<T> handle, int minSize = 128)
	{
		T[] arr = ArrayPool<T>.Shared.Rent(minSize);
		handle = new ArrayReturnHandle<T>(arr);
		return arr;
	}

	public static List<T> GetList<T>(out ListReturnHandle<T> handle)
	{
		List<T> arr = CollectionPool<List<T>, T>.Get();
		handle = new ListReturnHandle<T>(arr);
		return arr;
	}
}
