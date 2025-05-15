using UnityEngine;

public static class ArrayGetClampedExtensions
{
	public static T GetClamped<T>(this T[] arr, int index)
	{
		return arr[Mathf.Clamp(index, 0, arr.Length - 1)];
	}
}
