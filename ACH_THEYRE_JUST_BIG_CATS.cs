[AchUnlockOnComplete(typeof(Gem_E_Fangs))]
public class ACH_THEYRE_JUST_BIG_CATS : DewAchievementItem
{
	private const int RequiredKillCount = 50;

	[AchPersistentVar]
	private int _currentKillCount;

	public override int GetMaxProgress()
	{
		return 50;
	}

	public override int GetCurrentProgress()
	{
		return _currentKillCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillLastHit(delegate(EventInfoKill k)
		{
			if (k.victim is Mon_Ink_DivineAnimal && (k.actor is Ai_R_Smite || k.actor is Ai_E_ChainLightning || k.actor is Ai_Gem_E_Thunder || k.actor is Ai_Gem_R_Shock_Lightning || k.actor is Se_R_LightningDance || k.actor is Ai_Q_Fleche_Dash { empoweredWithLightning: not false }))
			{
				_currentKillCount++;
				if (_currentKillCount >= 50)
				{
					Complete();
				}
			}
		});
	}
}
