using System;

public class Pickup_BaseExpOrb : PickupInstance
{
	[NonSerialized]
	public float amount;

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		int a = DewMath.RandomRoundToInt(amount / (float)DewPlayer.humanPlayers.Count);
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			humanPlayer.hero.ReceiveExperience(a);
		}
	}

	private void MirrorProcessed()
	{
	}
}
