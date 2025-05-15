[AchUnlockOnComplete(typeof(LucidDream_GrievousWounds))]
public class ACH_VIVID_DREAM : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchWithGameResult(delegate(DewGameResult res)
		{
			if (res.result == DewGameResult.ResultType.DemoFinish && NetworkedManagerBase<GameManager>.instance.difficulty.name == "diffNightmare")
			{
				Complete();
			}
		});
	}
}
