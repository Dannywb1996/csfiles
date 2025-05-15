using System;
using UnityEngine;

[Serializable]
public struct Cost
{
	public int gold;

	public int dreamDust;

	public int stardust;

	public int healthPercentage;

	public AffordType CanAfford(Entity entity)
	{
		if (entity.owner.gold < gold)
		{
			return AffordType.NoGold;
		}
		if (entity.currentHealth / entity.maxHealth < (float)healthPercentage / 100f)
		{
			return AffordType.NoHealth;
		}
		return AffordType.Yes;
	}

	public static Cost Gold(int amount)
	{
		return new Cost
		{
			gold = amount
		};
	}

	public static Cost DreamDust(int amount)
	{
		return new Cost
		{
			dreamDust = amount
		};
	}

	public static Cost Stardust(int amount)
	{
		return new Cost
		{
			stardust = amount
		};
	}

	public static Cost HealthPercentage(int amount)
	{
		return new Cost
		{
			healthPercentage = amount
		};
	}

	public override string ToString()
	{
		if (gold == 0 && dreamDust == 0 && stardust == 0 && healthPercentage == 0)
		{
			return DewLocalization.GetUIValue("Generic_Free_NoCost");
		}
		string res = "";
		if (gold != 0)
		{
			if (res != "")
			{
				res += ", ";
			}
			res += string.Format(DewLocalization.GetUIValue("Currency_Template_Gold"), Mathf.Abs(gold).ToString("#,##0"));
		}
		if (dreamDust != 0)
		{
			if (res != "")
			{
				res += ", ";
			}
			res += string.Format(DewLocalization.GetUIValue("Currency_Template_DreamDust"), Mathf.Abs(dreamDust).ToString("#,##0"));
		}
		if (stardust != 0)
		{
			if (res != "")
			{
				res += ", ";
			}
			res += string.Format(DewLocalization.GetUIValue("Currency_Template_Stardust"), Mathf.Abs(stardust).ToString("#,##0"));
		}
		if (healthPercentage != 0)
		{
			if (res != "")
			{
				res += ", ";
			}
			res += string.Format(DewLocalization.GetUIValue("Currency_Template_HealthPercentage"), Mathf.Abs(healthPercentage).ToString("#,##0"));
		}
		return res;
	}

	public static Cost operator +(Cost a, Cost b)
	{
		return new Cost
		{
			gold = a.gold + b.gold,
			dreamDust = a.dreamDust + b.dreamDust,
			stardust = a.stardust + b.stardust,
			healthPercentage = a.healthPercentage + b.healthPercentage
		};
	}

	public static FloatCost operator +(Cost a, FloatCost b)
	{
		return new FloatCost
		{
			gold = (float)a.gold + b.gold,
			dreamDust = (float)a.dreamDust + b.dreamDust,
			stardust = (float)a.stardust + b.stardust,
			healthPercentage = (float)a.healthPercentage + b.healthPercentage
		};
	}

	public static Cost operator -(Cost a, Cost b)
	{
		return new Cost
		{
			gold = a.gold - b.gold,
			dreamDust = a.dreamDust - b.dreamDust,
			stardust = a.stardust - b.stardust,
			healthPercentage = a.healthPercentage - b.healthPercentage
		};
	}

	public static FloatCost operator -(Cost a, FloatCost b)
	{
		return new FloatCost
		{
			gold = (float)a.gold - b.gold,
			dreamDust = (float)a.dreamDust - b.dreamDust,
			stardust = (float)a.stardust - b.stardust,
			healthPercentage = (float)a.healthPercentage - b.healthPercentage
		};
	}

	public static Cost operator *(Cost a, int scalar)
	{
		return new Cost
		{
			gold = a.gold * scalar,
			dreamDust = a.dreamDust * scalar,
			stardust = a.stardust * scalar,
			healthPercentage = a.healthPercentage * scalar
		};
	}

	public static Cost operator *(int scalar, Cost a)
	{
		return a * scalar;
	}

	public static FloatCost operator *(Cost a, float scalar)
	{
		return new FloatCost
		{
			gold = (float)a.gold * scalar,
			dreamDust = (float)a.dreamDust * scalar,
			stardust = (float)a.stardust * scalar,
			healthPercentage = (float)a.healthPercentage * scalar
		};
	}

	public static FloatCost operator *(float scalar, Cost a)
	{
		return a * scalar;
	}

	public static FloatCost operator /(Cost a, float scalar)
	{
		return new FloatCost
		{
			gold = (float)a.gold / scalar,
			dreamDust = (float)a.dreamDust / scalar,
			stardust = (float)a.stardust / scalar,
			healthPercentage = (float)a.healthPercentage / scalar
		};
	}

	public static Cost operator /(Cost a, int scalar)
	{
		return new Cost
		{
			gold = a.gold / scalar,
			dreamDust = a.dreamDust / scalar,
			stardust = a.stardust / scalar,
			healthPercentage = a.healthPercentage / scalar
		};
	}
}
