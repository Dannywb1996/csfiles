using UnityEngine;

public static class QuaternionFlattenExtension
{
	public static void Flatten(this ref Quaternion q)
	{
		q = Quaternion.Euler(0f, q.eulerAngles.y, 0f);
	}

	public static Quaternion Flattened(this Quaternion q)
	{
		return Quaternion.Euler(0f, q.eulerAngles.y, 0f);
	}
}
