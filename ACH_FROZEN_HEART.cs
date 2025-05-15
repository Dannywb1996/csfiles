[AchUnlockOnComplete(typeof(Gem_L_GlacialCore))]
public class ACH_FROZEN_HEART : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill kill)
		{
			if (kill.victim is Mon_SnowMountain_BossSkoll)
			{
				Complete();
			}
		});
	}
}
