[AchUnlockOnComplete(typeof(Gem_L_HeartOfGold))]
public class ACH_MIDAS_HIMSELF : DewAchievementItem
{
	private const int RequiredAmount = 8000;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchCompleteWhen(() => DewPlayer.local.gold >= 8000);
	}
}
