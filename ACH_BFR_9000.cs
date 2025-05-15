using UnityEngine;

[AchUnlockOnComplete(typeof(St_D_DoubleTap))]
public class ACH_BFR_9000 : DewAchievementItem
{
	private const int RequiredAttackDamage = 75;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(DewPlayer.local.hero.GetType() != typeof(Hero_Lacerta)))
		{
			AchCompleteWhen(() => Mathf.RoundToInt(DewPlayer.local.hero.Status.attackDamage) >= 75, 1f);
		}
	}
}
