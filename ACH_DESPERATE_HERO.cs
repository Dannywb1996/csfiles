[AchUnlockOnComplete(typeof(Gem_E_Direness))]
public class ACH_DESPERATE_HERO : DewAchievementItem
{
	private const float HealthRatioThreshold = 0.4f;

	private const int KillsCount = 30;

	private int _kills;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillLastHit(delegate(EventInfoKill k)
		{
			if (DewPlayer.local.hero.CheckEnemyOrNeutral(k.victim) && !(DewPlayer.local.hero.normalizedHealth > 0.4f))
			{
				_kills++;
				if (_kills > 30)
				{
					Complete();
				}
			}
		});
		AchSetInterval(delegate
		{
			if (DewPlayer.local.hero.normalizedHealth > 0.4f)
			{
				_kills = 0;
			}
		}, 1f);
	}
}
