using System.Collections.Generic;

public static class ArrayGetOrDefaultExtensions
{
	public static T GetOrDefault<T>(this IList<T> array, int index, T defaultValue = default(T))
	{
		if (array == null || index < 0 || index >= array.Count)
		{
			return defaultValue;
		}
		return array[index];
	}
}
