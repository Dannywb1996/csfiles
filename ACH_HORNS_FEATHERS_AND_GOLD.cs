using UnityEngine;

[AchUnlockOnComplete(typeof(Hero_Aurena))]
public class ACH_HORNS_FEATHERS_AND_GOLD : DewAchievementItem
{
	private const float RequiredHealAmount = 25000f;

	[AchPersistentVar]
	private float _healAmount;

	public override int GetMaxProgress()
	{
		return Mathf.RoundToInt(25000f);
	}

	public override int GetCurrentProgress()
	{
		return Mathf.FloorToInt(_healAmount);
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnDoHeal(delegate(EventInfoHeal h)
		{
			_healAmount += h.amount + h.discardedAmount;
			if (_healAmount >= 25000f)
			{
				Complete();
			}
		});
	}
}
