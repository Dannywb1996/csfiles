public class Pickup_ManaOrb : PickupInstance
{
	public float amount = 100f;

	protected override bool CanBeUsedBy(Hero hero)
	{
		return hero.currentMana < hero.maxMana;
	}

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		DoManaHeal(new HealData(amount), hero);
	}

	private void MirrorProcessed()
	{
	}
}
