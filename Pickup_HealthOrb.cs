public class Pickup_HealthOrb : PickupInstance
{
	public float amount = 100f;

	protected override bool CanBeUsedBy(Hero hero)
	{
		return hero.currentHealth < hero.maxHealth;
	}

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		DoHeal(new HealData(amount), hero);
	}

	private void MirrorProcessed()
	{
	}
}
