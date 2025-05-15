using System;

public static class ReadOnlySpanExtension
{
	public static bool Contains<T>(this ReadOnlySpan<T> span, T item)
	{
		ReadOnlySpan<T> readOnlySpan = span;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			if (readOnlySpan[i].Equals(item))
			{
				return true;
			}
		}
		return false;
	}
}
