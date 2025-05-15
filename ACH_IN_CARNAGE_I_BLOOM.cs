using System;
using Mirror;

[AchUnlockOnComplete(typeof(Gem_E_Crimson))]
public class ACH_IN_CARNAGE_I_BLOOM : DewAchievementItem
{
	private const int RequiredUpgradeCount = 20;

	private int _currentUpgradeCount;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ClientEventManager>.instance.OnItemUpgraded += new Action<Hero, NetworkBehaviour>(OnItemUpgraded);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnItemUpgraded -= new Action<Hero, NetworkBehaviour>(OnItemUpgraded);
		}
	}

	private void OnItemUpgraded(Hero arg1, NetworkBehaviour arg2)
	{
		if (!(arg1 != base.hero) && arg2 is SkillTrigger skillTrigger && (skillTrigger.tags & DescriptionTags.Dark) != DescriptionTags.None)
		{
			_currentUpgradeCount++;
			if (_currentUpgradeCount >= 20)
			{
				Complete();
			}
		}
	}
}
