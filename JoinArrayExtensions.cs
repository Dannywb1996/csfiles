using System.Collections.Generic;
using System.Text;

public static class JoinArrayExtensions
{
	public static string JoinToString<T>(this IList<T> arr, string delimiter = " ")
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < arr.Count; i++)
		{
			T a = arr[i];
			sb.Append(a);
			if (i != arr.Count - 1)
			{
				sb.Append(delimiter);
			}
		}
		return sb.ToString();
	}
}
