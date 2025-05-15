using UnityEngine;

[AchUnlockOnComplete(typeof(St_QR_DistortedMind))]
public class ACH_DOUBLE_VISION : DewAchievementItem
{
	private const float RequiredDamageAmount = 250000f;

	[AchPersistentVar]
	private float _damageAmount;

	public override int GetMaxProgress()
	{
		return Mathf.RoundToInt(250000f);
	}

	public override int GetCurrentProgress()
	{
		return Mathf.FloorToInt(_damageAmount);
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewPlayer.local.hero.GetType() != typeof(Hero_Bismuth))
		{
			return;
		}
		AchOnDealDamage(delegate(EventInfoDamage d)
		{
			if (DewPlayer.local.hero.CheckEnemyOrNeutral(d.victim) && d.damage.elemental == ElementalType.Dark)
			{
				_damageAmount += d.damage.amount + d.damage.discardedAmount;
				if (_damageAmount >= 250000f)
				{
					Complete();
				}
			}
		});
	}
}
