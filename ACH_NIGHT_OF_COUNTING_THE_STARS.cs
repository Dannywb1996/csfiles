using UnityEngine;

[AchUnlockOnComplete(typeof(St_Q_SuperNova))]
public class ACH_NIGHT_OF_COUNTING_THE_STARS : DewAchievementItem
{
	private const int RequiredAbilityPower = 130;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(DewPlayer.local.hero.GetType() != typeof(Hero_Yubar)))
		{
			AchCompleteWhen(() => Mathf.RoundToInt(DewPlayer.local.hero.Status.abilityPower) >= 130, 1f);
		}
	}
}
