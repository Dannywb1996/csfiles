[AchUnlockOnComplete(typeof(Gem_E_Flexibility))]
public class ACH_FLEXIBLE_DIET : DewAchievementItem
{
	private const float HealthThresholdLow = 0.4f;

	private const float HealthThresholdHigh = 0.75f;

	private const int RepeatCount = 10;

	private int _step;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchSetInterval(delegate
		{
			Hero hero = DewPlayer.local.hero;
			float num = hero.currentHealth / hero.maxHealth;
			if (_step % 2 == 0)
			{
				if (num <= 0.4f)
				{
					_step++;
				}
			}
			else if (num >= 0.75f)
			{
				_step++;
			}
			if (_step >= 20)
			{
				Complete();
			}
		}, 0.5f);
	}
}
