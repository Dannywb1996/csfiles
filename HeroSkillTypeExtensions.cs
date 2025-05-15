public static class HeroSkillTypeExtensions
{
	public static bool IsMainSkill(this HeroSkillLocation h)
	{
		if ((uint)h <= 4u)
		{
			return true;
		}
		return false;
	}
}
