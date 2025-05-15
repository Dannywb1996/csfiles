using System;

public struct GemLocation : IEquatable<GemLocation>
{
	public HeroSkillLocation skill;

	public int index;

	public GemLocation(HeroSkillLocation skill, int index)
	{
		this.skill = skill;
		this.index = index;
	}

	public bool Equals(GemLocation other)
	{
		if (skill == other.skill)
		{
			return index == other.index;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is GemLocation other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine((int)skill, index);
	}

	public static bool operator ==(GemLocation c1, GemLocation c2)
	{
		return c1.Equals(c2);
	}

	public static bool operator !=(GemLocation c1, GemLocation c2)
	{
		return !c1.Equals(c2);
	}
}
