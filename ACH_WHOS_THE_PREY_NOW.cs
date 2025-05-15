[AchUnlockOnComplete(typeof(Gem_E_Predation))]
public class ACH_WHOS_THE_PREY_NOW : DewAchievementItem
{
	private const int KillsCount = 70;

	[AchPersistentVar]
	private int _kills;

	public override int GetCurrentProgress()
	{
		return _kills;
	}

	public override int GetMaxProgress()
	{
		return 70;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill k)
		{
			if (k.victim is Monster { isHunter: not false })
			{
				_kills++;
				if (_kills >= 70)
				{
					Complete();
				}
			}
		});
	}
}
