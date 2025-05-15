using System;
using UnityEngine.Serialization;

[Serializable]
public struct BonusStats : IEquatable<BonusStats>
{
	public static readonly BonusStats Default;

	public float attackDamageFlat;

	public float attackDamagePercentage;

	public float abilityPowerFlat;

	public float abilityPowerPercentage;

	public float maxHealthFlat;

	public float maxHealthPercentage;

	public float maxManaFlat;

	public float maxManaPercentage;

	public float healthRegenFlat;

	public float healthRegenPercentage;

	public float manaRegenFlat;

	public float manaRegenPercentage;

	public float attackSpeedPercentage;

	public float critAmpFlat;

	public float critAmpPercentage;

	public float critChanceFlat;

	public float critChancePercentage;

	public float abilityHasteFlat;

	public float abilityHastePercentage;

	public float tenacityFlat;

	public float tenacityPercentage;

	public float movementSpeedPercentage;

	public float fireEffectAmpFlat;

	public float coldEffectAmpFlat;

	public float lightEffectAmpFlat;

	public float darkEffectAmpFlat;

	public float armorFlat;

	[FormerlySerializedAs("armorPercetnage")]
	public float armorPercentage;

	public static BonusStats operator +(BonusStats x, BonusStats y)
	{
		return new BonusStats
		{
			attackDamageFlat = x.attackDamageFlat + y.attackDamageFlat,
			attackDamagePercentage = DewMath.MultiplyPercentageBonuses(x.attackDamagePercentage, y.attackDamagePercentage),
			abilityPowerFlat = x.abilityPowerFlat + y.abilityPowerFlat,
			abilityPowerPercentage = DewMath.MultiplyPercentageBonuses(x.abilityPowerPercentage, y.abilityPowerPercentage),
			maxHealthFlat = x.maxHealthFlat + y.maxHealthFlat,
			maxHealthPercentage = DewMath.MultiplyPercentageBonuses(x.maxHealthPercentage, y.maxHealthPercentage),
			maxManaFlat = x.maxManaFlat + y.maxManaFlat,
			maxManaPercentage = DewMath.MultiplyPercentageBonuses(x.maxManaPercentage, y.maxManaPercentage),
			healthRegenFlat = x.healthRegenFlat + y.healthRegenFlat,
			healthRegenPercentage = DewMath.MultiplyPercentageBonuses(x.healthRegenPercentage, y.healthRegenPercentage),
			manaRegenFlat = x.manaRegenFlat + y.manaRegenFlat,
			manaRegenPercentage = DewMath.MultiplyPercentageBonuses(x.manaRegenPercentage, y.manaRegenPercentage),
			attackSpeedPercentage = x.attackSpeedPercentage + y.attackSpeedPercentage,
			critAmpFlat = x.critAmpFlat + y.critAmpFlat,
			critAmpPercentage = x.critAmpPercentage + y.critAmpPercentage,
			critChanceFlat = x.critChanceFlat + y.critChanceFlat,
			critChancePercentage = DewMath.MultiplyPercentageBonuses(x.critChancePercentage, y.critChancePercentage),
			abilityHasteFlat = x.abilityHasteFlat + y.abilityHasteFlat,
			abilityHastePercentage = DewMath.MultiplyPercentageBonuses(x.abilityHastePercentage, y.abilityHastePercentage),
			tenacityFlat = x.tenacityFlat + y.tenacityFlat,
			tenacityPercentage = DewMath.MultiplyPercentageBonuses(x.tenacityPercentage, y.tenacityPercentage),
			movementSpeedPercentage = x.movementSpeedPercentage + y.movementSpeedPercentage,
			fireEffectAmpFlat = x.fireEffectAmpFlat + y.fireEffectAmpFlat,
			coldEffectAmpFlat = x.coldEffectAmpFlat + y.coldEffectAmpFlat,
			lightEffectAmpFlat = x.lightEffectAmpFlat + y.lightEffectAmpFlat,
			darkEffectAmpFlat = x.darkEffectAmpFlat + y.darkEffectAmpFlat,
			armorFlat = x.armorFlat + y.armorFlat,
			armorPercentage = DewMath.MultiplyPercentageBonuses(x.armorPercentage, y.armorPercentage)
		};
	}

	public static BonusStats operator +(BonusStats x, StatBonus y)
	{
		return new BonusStats
		{
			attackDamageFlat = x.attackDamageFlat + y.attackDamageFlat,
			attackDamagePercentage = DewMath.MultiplyPercentageBonuses(x.attackDamagePercentage, y.attackDamagePercentage),
			abilityPowerFlat = x.abilityPowerFlat + y.abilityPowerFlat,
			abilityPowerPercentage = DewMath.MultiplyPercentageBonuses(x.abilityPowerPercentage, y.abilityPowerPercentage),
			maxHealthFlat = x.maxHealthFlat + y.maxHealthFlat,
			maxHealthPercentage = DewMath.MultiplyPercentageBonuses(x.maxHealthPercentage, y.maxHealthPercentage),
			maxManaFlat = x.maxManaFlat + y.maxManaFlat,
			maxManaPercentage = DewMath.MultiplyPercentageBonuses(x.maxManaPercentage, y.maxManaPercentage),
			healthRegenFlat = x.healthRegenFlat + y.healthRegenFlat,
			healthRegenPercentage = DewMath.MultiplyPercentageBonuses(x.healthRegenPercentage, y.healthRegenPercentage),
			manaRegenFlat = x.manaRegenFlat + y.manaRegenFlat,
			manaRegenPercentage = DewMath.MultiplyPercentageBonuses(x.manaRegenPercentage, y.manaRegenPercentage),
			attackSpeedPercentage = x.attackSpeedPercentage + y.attackSpeedPercentage,
			critAmpFlat = x.critAmpFlat + y.critAmpFlat,
			critAmpPercentage = x.critAmpPercentage + y.critAmpPercentage,
			critChanceFlat = x.critChanceFlat + y.critChanceFlat,
			critChancePercentage = DewMath.MultiplyPercentageBonuses(x.critChancePercentage, y.critChancePercentage),
			abilityHasteFlat = x.abilityHasteFlat + y.abilityHasteFlat,
			abilityHastePercentage = DewMath.MultiplyPercentageBonuses(x.abilityHastePercentage, y.abilityHastePercentage),
			tenacityFlat = x.tenacityFlat + y.tenacityFlat,
			tenacityPercentage = DewMath.MultiplyPercentageBonuses(x.tenacityPercentage, y.tenacityPercentage),
			movementSpeedPercentage = x.movementSpeedPercentage + y.movementSpeedPercentage,
			fireEffectAmpFlat = x.fireEffectAmpFlat + y.fireEffectAmpFlat,
			coldEffectAmpFlat = x.coldEffectAmpFlat + y.coldEffectAmpFlat,
			lightEffectAmpFlat = x.lightEffectAmpFlat + y.lightEffectAmpFlat,
			darkEffectAmpFlat = x.darkEffectAmpFlat + y.darkEffectAmpFlat,
			armorFlat = x.armorFlat + y.armorFlat,
			armorPercentage = DewMath.MultiplyPercentageBonuses(x.armorPercentage, y.armorPercentage)
		};
	}

	public bool Equals(BonusStats other)
	{
		if (attackDamageFlat.Equals(other.attackDamageFlat) && attackDamagePercentage.Equals(other.attackDamagePercentage) && abilityPowerFlat.Equals(other.abilityPowerFlat) && abilityPowerPercentage.Equals(other.abilityPowerPercentage) && maxHealthFlat.Equals(other.maxHealthFlat) && maxHealthPercentage.Equals(other.maxHealthPercentage) && maxManaFlat.Equals(other.maxManaFlat) && maxManaPercentage.Equals(other.maxManaPercentage) && healthRegenFlat.Equals(other.healthRegenFlat) && healthRegenPercentage.Equals(other.healthRegenPercentage) && manaRegenFlat.Equals(other.manaRegenFlat) && manaRegenPercentage.Equals(other.manaRegenPercentage) && attackSpeedPercentage.Equals(other.attackSpeedPercentage) && critAmpFlat.Equals(other.critAmpFlat) && critAmpPercentage.Equals(other.critAmpPercentage) && critChanceFlat.Equals(other.critChanceFlat) && critChancePercentage.Equals(other.critChancePercentage) && abilityHasteFlat.Equals(other.abilityHasteFlat) && abilityHastePercentage.Equals(other.abilityHastePercentage) && tenacityFlat.Equals(other.tenacityFlat) && tenacityPercentage.Equals(other.tenacityPercentage) && movementSpeedPercentage.Equals(other.movementSpeedPercentage) && fireEffectAmpFlat.Equals(other.fireEffectAmpFlat) && coldEffectAmpFlat.Equals(other.coldEffectAmpFlat) && lightEffectAmpFlat.Equals(other.lightEffectAmpFlat) && darkEffectAmpFlat.Equals(other.darkEffectAmpFlat) && armorFlat.Equals(other.armorFlat))
		{
			return armorPercentage.Equals(other.armorPercentage);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BonusStats other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(attackDamageFlat);
		hashCode.Add(attackDamagePercentage);
		hashCode.Add(abilityPowerFlat);
		hashCode.Add(abilityPowerPercentage);
		hashCode.Add(maxHealthFlat);
		hashCode.Add(maxHealthPercentage);
		hashCode.Add(maxManaFlat);
		hashCode.Add(maxManaPercentage);
		hashCode.Add(healthRegenFlat);
		hashCode.Add(healthRegenPercentage);
		hashCode.Add(manaRegenFlat);
		hashCode.Add(manaRegenPercentage);
		hashCode.Add(attackSpeedPercentage);
		hashCode.Add(critAmpFlat);
		hashCode.Add(critAmpPercentage);
		hashCode.Add(critChanceFlat);
		hashCode.Add(critChancePercentage);
		hashCode.Add(abilityHasteFlat);
		hashCode.Add(abilityHastePercentage);
		hashCode.Add(tenacityFlat);
		hashCode.Add(tenacityPercentage);
		hashCode.Add(movementSpeedPercentage);
		hashCode.Add(fireEffectAmpFlat);
		hashCode.Add(coldEffectAmpFlat);
		hashCode.Add(lightEffectAmpFlat);
		hashCode.Add(darkEffectAmpFlat);
		hashCode.Add(armorFlat);
		hashCode.Add(armorPercentage);
		return hashCode.ToHashCode();
	}

	public static bool operator ==(BonusStats left, BonusStats right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BonusStats left, BonusStats right)
	{
		return !left.Equals(right);
	}
}
