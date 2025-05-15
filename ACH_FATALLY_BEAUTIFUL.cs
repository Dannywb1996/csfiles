[AchUnlockOnComplete(typeof(Gem_R_Composure))]
public class ACH_FATALLY_BEAUTIFUL : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchCompleteWhen(() => base.hero.Status.critChance >= 0.995f);
	}
}
