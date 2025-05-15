using System.Collections.Generic;
using UnityEngine;

public static class TransformDeepChildExtension
{
	public static Transform FindDeepChild(this Transform aParent, string aName)
	{
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(aParent);
		while (queue.Count > 0)
		{
			Transform c = queue.Dequeue();
			if (c.name == aName)
			{
				return c;
			}
			foreach (Transform t in c)
			{
				queue.Enqueue(t);
			}
		}
		return null;
	}

	public static IEnumerable<Transform> FindDeepChildren(this Transform aParent, string aName)
	{
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(aParent);
		while (queue.Count > 0)
		{
			Transform c = queue.Dequeue();
			if (c.name == aName)
			{
				yield return c;
			}
			foreach (Transform t in c)
			{
				queue.Enqueue(t);
			}
		}
	}
}
