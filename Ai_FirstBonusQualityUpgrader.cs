public class Ai_FirstBonusQualityUpgrader : AbilityInstance
{
	internal Actor target;

	internal int upgradeAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			if (target is Gem gem)
			{
				gem.quality += upgradeAmount;
			}
			else if (target is SkillTrigger skillTrigger)
			{
				skillTrigger.level += upgradeAmount;
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
