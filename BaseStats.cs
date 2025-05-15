using System;

[Serializable]
public struct BaseStats
{
	public static readonly BaseStats Default = new BaseStats
	{
		attackDamage = 60f,
		abilityPower = 80f,
		maxHealth = 100f,
		maxMana = 100f,
		healthRegen = 0f,
		manaRegen = 0f,
		critAmp = 1f,
		critChance = 0f,
		abilityHaste = 0f,
		fireEffectAmp = 0f,
		coldEffectAmp = 0f,
		lightEffectAmp = 0f,
		darkEffectAmp = 0f,
		armor = 0f
	};

	public float attackDamage;

	public float abilityPower;

	public float maxHealth;

	public float maxMana;

	public float healthRegen;

	public float manaRegen;

	public float critAmp;

	public float critChance;

	public float abilityHaste;

	public float tenacity;

	public float fireEffectAmp;

	public float coldEffectAmp;

	public float lightEffectAmp;

	public float darkEffectAmp;

	public float armor;
}
