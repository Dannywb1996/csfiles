[AchUnlockOnComplete(typeof(St_L_HerWorld))]
public class ACH_NIGHT_DANCER : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill kill)
		{
			if (kill.victim is Mon_Sky_BossNyx)
			{
				Complete();
			}
		});
	}
}
