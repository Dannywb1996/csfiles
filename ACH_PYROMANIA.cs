[AchUnlockOnComplete(typeof(St_E_FlameJet))]
public class ACH_PYROMANIA : DewAchievementItem
{
	private const int KillsCount = 250;

	[AchPersistentVar]
	private int _kills;

	public override int GetMaxProgress()
	{
		return 250;
	}

	public override int GetCurrentProgress()
	{
		return _kills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill k)
		{
			if (DewPlayer.local.hero.CheckEnemyOrNeutral(k.victim) && k.victim.Status.fireStack != 0)
			{
				_kills++;
				if (_kills >= 250)
				{
					Complete();
				}
			}
		});
	}
}
