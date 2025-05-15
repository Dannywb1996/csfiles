using System;

[AchUnlockOnComplete(typeof(Gem_R_Shock))]
public class ACH_THOUSAND_THUNDERS : DewAchievementItem
{
	private const int RequiredCritCount = 1000;

	[AchPersistentVar]
	private int _currentCritCount;

	public override int GetMaxProgress()
	{
		return 1000;
	}

	public override int GetCurrentProgress()
	{
		return _currentCritCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ClientEventManager>.instance.OnAttackHit += new Action<EventInfoAttackHit>(OnAttackHit);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnAttackHit -= new Action<EventInfoAttackHit>(OnAttackHit);
		}
	}

	private void OnAttackHit(EventInfoAttackHit obj)
	{
		if (obj.isCrit && !(obj.strength < 0.99f) && !(obj.attacker != base.hero) && !obj.victim.Status.hasDamageImmunity && base.hero.CheckEnemyOrNeutral(obj.victim))
		{
			_currentCritCount++;
			if (_currentCritCount >= 1000)
			{
				Complete();
			}
		}
	}
}
