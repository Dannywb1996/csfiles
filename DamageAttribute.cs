using System;

[Flags]
public enum DamageAttribute : long
{
	None = 0L,
	IsCrit = 1L,
	IgnoreArmor = 2L,
	IgnoreShield = 4L,
	IgnoreDamageImmunity = 8L,
	AreaOfEffect = 0x10L,
	DamageOverTime = 0x20L,
	ForceMergeNumber = 0x40L
}
