using UnityEngine;

[AchUnlockOnComplete(typeof(St_D_ConvergencePoint))]
public class ACH_ODE_TO_THE_STARS : DewAchievementItem
{
	private const int RequiredAttackDamage = 50;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(DewPlayer.local.hero.GetType() != typeof(Hero_Yubar)))
		{
			AchCompleteWhen(() => Mathf.RoundToInt(DewPlayer.local.hero.Status.attackDamage) >= 50, 1f);
		}
	}
}
