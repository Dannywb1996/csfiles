using UnityEngine;

public static class Vector3FlattenExtension
{
	public static Vector3 Flattened(this Vector3 v)
	{
		return new Vector3(v.x, 0f, v.z);
	}

	public static void Flatten(this ref Vector3 v)
	{
		v.y = 0f;
	}

	public static Vector3 WithX(this Vector3 v, float newX)
	{
		return new Vector3(newX, v.y, v.z);
	}

	public static Vector3 WithY(this Vector3 v, float newY)
	{
		return new Vector3(v.x, newY, v.z);
	}

	public static Vector2 WithZ(this Vector3 v, float newZ)
	{
		return new Vector3(v.x, v.y, newZ);
	}

	public static Vector2 WithX(this Vector2 v, float newX)
	{
		return new Vector2(newX, v.y);
	}

	public static Vector2 WithY(this Vector2 v, float newY)
	{
		return new Vector2(v.x, newY);
	}
}
