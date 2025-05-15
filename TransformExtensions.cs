using UnityEngine;

public static class TransformExtensions
{
	public static bool IsSelfOrDescendantOf(this Transform child, Transform transform)
	{
		while (child != null && transform != child)
		{
			child = child.parent;
		}
		return transform == child;
	}
}
