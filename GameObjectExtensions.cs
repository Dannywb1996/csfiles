using UnityEngine;

public static class GameObjectExtensions
{
	public static void SetActiveAll(this GameObject[] gobjs, bool value)
	{
		foreach (GameObject g in gobjs)
		{
			if (!(g == null))
			{
				g.SetActive(value);
			}
		}
	}
}
