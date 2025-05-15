[AchUnlockOnComplete(typeof(Gem_R_Blood))]
public class ACH_BLOOD_FOR_BLOOD : DewAchievementItem
{
	private const int RequiredBossKills = 6;

	private const float HealthThreshold = 0.25f;

	[AchPersistentVar]
	private int _bossKills;

	public override int GetMaxProgress()
	{
		return 6;
	}

	public override int GetCurrentProgress()
	{
		return _bossKills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill kill)
		{
			if (kill.victim is BossMonster && !(DewPlayer.local.hero.normalizedHealth > 0.25f))
			{
				_bossKills++;
				if (_bossKills >= 6)
				{
					Complete();
				}
			}
		});
	}
}
