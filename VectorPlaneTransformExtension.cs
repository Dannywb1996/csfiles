using UnityEngine;

public static class VectorPlaneTransformExtension
{
	public static Vector2 ToXY(this Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

	public static Vector3 ToXZ(this Vector2 v)
	{
		return new Vector3(v.x, 0f, v.y);
	}
}
