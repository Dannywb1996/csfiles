using UnityEngine;

public static class RandomExtension
{
	public static Quaternion Spread(float minAngle, float maxAngle)
	{
		return Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Quaternion.AngleAxis(Random.Range(minAngle, maxAngle), Vector3.up);
	}
}
