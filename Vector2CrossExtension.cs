using UnityEngine;

public static class Vector2CrossExtension
{
	public static float Cross(this Vector2 v1, Vector2 v2)
	{
		return v1.x * v2.y - v1.y * v2.x;
	}
}
