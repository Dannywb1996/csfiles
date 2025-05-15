using UnityEngine;

public class Ai_Gem_E_Predation_Pickup : PickupInstance
{
	public class Ad_Predation
	{
		public StatBonus bonus;
	}

	public ScalingValue addedAp;

	public ScalingValue addedAd;

	internal Hero _targetHero;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && _targetHero.IsNullInactiveDeadOrKnockedOut())
		{
			Destroy();
		}
	}

	protected override bool CanBeUsedBy(Hero hero)
	{
		if (base.CanBeUsedBy(hero))
		{
			return _targetHero == hero;
		}
		return false;
	}

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		if (!(hero != _targetHero))
		{
			if (!hero.TryGetData<Ad_Predation>(out var data))
			{
				data = new Ad_Predation
				{
					bonus = new StatBonus()
				};
				hero.Status.AddStatBonus(data.bonus);
				hero.AddData(data);
			}
			if (Random.value < 0.5f)
			{
				data.bonus.abilityPowerFlat += GetValue(addedAp);
			}
			else
			{
				data.bonus.attackDamageFlat += GetValue(addedAd);
			}
			if (base.gem != null && base.gem is Gem_E_Predation gem_E_Predation)
			{
				gem_E_Predation.UpdateStack();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
