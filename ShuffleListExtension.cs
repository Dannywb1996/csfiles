using System.Collections.Generic;
using UnityEngine;

public static class ShuffleListExtension
{
	public static void Shuffle<T>(this IList<T> list, DewRandomInstance random = null)
	{
		for (int i = 0; i < list.Count; i++)
		{
			T temp = list[i];
			int randomIndex = random?.Range(i, list.Count) ?? Random.Range(i, list.Count);
			list[i] = list[randomIndex];
			list[randomIndex] = temp;
		}
	}
}
