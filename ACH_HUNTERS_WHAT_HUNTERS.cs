[AchUnlockOnComplete(typeof(LucidDream_BonVoyage))]
public class ACH_HUNTERS_WHAT_HUNTERS : DewAchievementItem
{
	private const int RequiredEnterCount = 10;

	[AchPersistentVar]
	private int _enterPureWhiteDreamCount;

	private bool _didFail;

	public override int GetMaxProgress()
	{
		return 10;
	}

	public override int GetCurrentProgress()
	{
		return _enterPureWhiteDreamCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill k)
		{
			if (k.victim != null && k.victim is Monster { isHunter: not false })
			{
				_didFail = true;
			}
		});
		AchWithGameResult(delegate(DewGameResult res)
		{
			if (res.result == DewGameResult.ResultType.DemoFinish && !_didFail)
			{
				_enterPureWhiteDreamCount++;
				if (_enterPureWhiteDreamCount >= 10)
				{
					Complete();
				}
			}
		});
	}
}
