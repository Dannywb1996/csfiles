[AchUnlockOnComplete(typeof(St_R_PrecisionShot))]
public class ACH_UNTOUCHABLE : DewAchievementItem
{
	private const int RequiredBossKills = 1;

	private const float HealthThreshold = 0.85f;

	[AchPersistentVar]
	private int _bossKills;

	public override int GetMaxProgress()
	{
		return 1;
	}

	public override int GetCurrentProgress()
	{
		return _bossKills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewPlayer.local.hero.GetType() != typeof(Hero_Lacerta))
		{
			return;
		}
		AchOnKillOrAssist(delegate(EventInfoKill kill)
		{
			if (kill.victim is Monster { type: Monster.MonsterType.Boss } && !(DewPlayer.local.hero.normalizedHealth < 0.85f))
			{
				_bossKills++;
				if (_bossKills >= 1)
				{
					Complete();
				}
			}
		});
	}
}
