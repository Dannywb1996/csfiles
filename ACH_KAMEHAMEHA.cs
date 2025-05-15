[AchUnlockOnComplete(typeof(St_L_WorldCracker))]
public class ACH_KAMEHAMEHA : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill kill)
		{
			if (kill.victim is Mon_Special_BossLightElemental)
			{
				Complete();
			}
		});
	}
}
