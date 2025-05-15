using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_E_Might))]
public class ACH_HEAVIER_THAN_ATLAS : DewAchievementItem
{
	private const int RequiredMaxHealthAmount = 3000;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchCompleteWhen(() => Mathf.RoundToInt(DewPlayer.local.hero.Status.maxHealth) >= 3000);
	}
}
