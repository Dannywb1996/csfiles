using UnityEngine;

public class PropEnt_Merchant_Backpack : PropEntity
{
	public float spawnSkillRatio;

	public GameObject deathEffect;

	public float spawnDelay;

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		_ = base.isServer;
	}

	private void MirrorProcessed()
	{
	}
}
