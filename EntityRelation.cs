using System;

[Flags]
public enum EntityRelation : byte
{
	Self = 1,
	Neutral = 2,
	Enemy = 4,
	Ally = 8
}
