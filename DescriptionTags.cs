using System;

[Flags]
public enum DescriptionTags
{
	None = 0,
	Fire = 1,
	Cold = 2,
	Dark = 4,
	Light = 8,
	Melee = 0x10,
	HardCC = 0x20,
	Heal = 0x40,
	Shield = 0x80,
	Attack = 0x100,
	Mobility = 0x200,
	Summon = 0x400
}
