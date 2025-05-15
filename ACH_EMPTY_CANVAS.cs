[AchUnlockOnComplete(typeof(Gem_L_PureWhite))]
public class ACH_EMPTY_CANVAS : DewAchievementItem
{
	private const int RequiredEnterCount = 10;

	[AchPersistentVar]
	private int _enterCount;

	public override int GetMaxProgress()
	{
		return 10;
	}

	public override int GetCurrentProgress()
	{
		return _enterCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchWithGameResult(delegate(DewGameResult r)
		{
			if (r.result == DewGameResult.ResultType.DemoFinish)
			{
				_enterCount++;
				if (_enterCount >= 10)
				{
					Complete();
				}
			}
		});
	}
}
