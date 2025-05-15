using System;

[Flags]
public enum TeamRelation : byte
{
	Own = 1,
	Neutral = 2,
	Enemy = 4,
	Ally = 8
}
