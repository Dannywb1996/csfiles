using System;

public abstract class DewHeroStarItemOld : DewStarItemOld
{
	public abstract Type heroType { get; }

	public override bool ShouldInitInGame()
	{
		if (base.hero != null)
		{
			return base.hero.GetType() == heroType;
		}
		return false;
	}
}
