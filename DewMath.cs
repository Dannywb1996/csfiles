using UnityEngine;

public static class DewMath
{
	public static int RandomRoundToInt(float value, DewRandomInstance random = null)
	{
		if (float.IsPositiveInfinity(value))
		{
			return int.MaxValue;
		}
		if (float.IsNegativeInfinity(value))
		{
			return int.MinValue;
		}
		int intAmount = Mathf.FloorToInt(value);
		if ((random?.value ?? Random.value) < value - (float)intAmount)
		{
			intAmount++;
		}
		return intAmount;
	}

	public static float MultiplyPercentageBonuses(float a, float b)
	{
		return ((1f + a * 0.01f) * (1f + b * 0.01f) - 1f) * 100f;
	}
}
