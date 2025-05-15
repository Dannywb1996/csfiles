[AchUnlockOnComplete(typeof(Gem_R_Wound))]
public class ACH_THROUGH_TIME_AND_SPACE : DewAchievementItem
{
	private const float RequiredAttackSpeed = 6f;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchCompleteWhen(() => !(DewPlayer.local.hero.Ability.attackAbility == null) && 1f / DewPlayer.local.hero.Ability.attackAbility.configs[0].cooldownTime * DewPlayer.local.hero.Status.attackSpeedMultiplier > 6f, 1f);
	}
}
