using System;
using UnityEngine;

[Serializable]
public struct FloatCost
{
	public float gold;

	public float dreamDust;

	public float stardust;

	public float healthPercentage;

	public static implicit operator FloatCost(Cost cost)
	{
		return new FloatCost
		{
			gold = cost.gold,
			dreamDust = cost.dreamDust,
			stardust = cost.stardust,
			healthPercentage = cost.healthPercentage
		};
	}

	public static explicit operator Cost(FloatCost floatCost)
	{
		return new Cost
		{
			gold = Mathf.RoundToInt(floatCost.gold),
			dreamDust = Mathf.RoundToInt(floatCost.dreamDust),
			stardust = Mathf.RoundToInt(floatCost.stardust),
			healthPercentage = Mathf.RoundToInt(floatCost.healthPercentage)
		};
	}

	public static FloatCost operator +(FloatCost a, FloatCost b)
	{
		return new FloatCost
		{
			gold = a.gold + b.gold,
			dreamDust = a.dreamDust + b.dreamDust,
			stardust = a.stardust + b.stardust,
			healthPercentage = a.healthPercentage + b.healthPercentage
		};
	}

	public static FloatCost operator +(FloatCost a, Cost b)
	{
		return a + (FloatCost)b;
	}

	public static FloatCost operator -(FloatCost a, FloatCost b)
	{
		return new FloatCost
		{
			gold = a.gold - b.gold,
			dreamDust = a.dreamDust - b.dreamDust,
			stardust = a.stardust - b.stardust,
			healthPercentage = a.healthPercentage - b.healthPercentage
		};
	}

	public static FloatCost operator -(FloatCost a, Cost b)
	{
		return a - (FloatCost)b;
	}

	public static FloatCost operator *(FloatCost a, float scalar)
	{
		return new FloatCost
		{
			gold = a.gold * scalar,
			dreamDust = a.dreamDust * scalar,
			stardust = a.stardust * scalar,
			healthPercentage = a.healthPercentage * scalar
		};
	}

	public static FloatCost operator *(float scalar, FloatCost a)
	{
		return a * scalar;
	}

	public static FloatCost operator /(FloatCost a, float scalar)
	{
		return new FloatCost
		{
			gold = a.gold / scalar,
			dreamDust = a.dreamDust / scalar,
			stardust = a.stardust / scalar,
			healthPercentage = a.healthPercentage / scalar
		};
	}
}
