[AchUnlockOnComplete(typeof(Gem_L_EternalFlame))]
public class ACH_PYRANAS_GRIN : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill kill)
		{
			if (kill.victim is Mon_LavaLand_BossInfernus)
			{
				Complete();
			}
		});
	}
}
