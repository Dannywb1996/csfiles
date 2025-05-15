using System;

[Flags]
public enum BasicEffectMask : uint
{
	Slow = 1u,
	Speed = 2u,
	Haste = 4u,
	Cripple = 8u,
	Root = 0x10u,
	Silence = 0x20u,
	Stun = 0x40u,
	Blind = 0x80u,
	AttackCritical = 0x100u,
	AttackOverride = 0x200u,
	AttackEmpower = 0x400u,
	Invulnerable = 0x800u,
	Unstoppable = 0x1000u,
	Protected = 0x2000u,
	Uncollidable = 0x4000u,
	Invisible = 0x8000u,
	ArmorBoost = 0x10000u,
	ArmorReduction = 0x20000u,
	DeathInterrupt = 0x40000u,
	Shield = 0x80000u,
	HealReduction = 0x100000u
}
