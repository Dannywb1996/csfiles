[AchUnlockOnComplete(typeof(St_QR_ValiantHeart))]
public class ACH_LOOK_RAINBOW : DewAchievementItem
{
	public override int GetMaxProgress()
	{
		return 1;
	}

	public override int GetCurrentProgress()
	{
		return 0;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewPlayer.local.hero.GetType() != typeof(Hero_Bismuth))
		{
			return;
		}
		AchWithGameResult(delegate(DewGameResult r)
		{
			if (r.result == DewGameResult.ResultType.DemoFinish)
			{
				Complete();
			}
		});
	}
}
