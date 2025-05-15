[AchUnlockOnComplete(typeof(St_R_FlamingWhip))]
public class ACH_RUN_ME_LIKE_A_RACE_HORSE : DewAchievementItem
{
	private const int RequiredFireStack = 20;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill k)
		{
			if (DewPlayer.local.hero.CheckEnemyOrNeutral(k.victim) && k.victim.Status.fireStack >= 20)
			{
				Complete();
			}
		});
	}
}
