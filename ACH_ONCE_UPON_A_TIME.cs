[AchUnlockOnComplete(typeof(Hero_Bismuth))]
public class ACH_ONCE_UPON_A_TIME : DewAchievementItem
{
	private const float RequiredTime = 3f;

	private const int RequiredMovementSpeed = 900;

	private float _conditionTime;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchCompleteWhen(delegate
		{
			if (base.hero.Status.movementSpeedMultiplier * base.hero.Control.baseAgentSpeed * 100f > 900f)
			{
				_conditionTime += 0.5f;
			}
			else
			{
				_conditionTime = 0f;
			}
			return _conditionTime > 2.9f;
		}, 0.5f);
	}
}
