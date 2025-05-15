public class PropEnt_Stone_Gold : PropEntity
{
	public Formula amountByZoneIndex;

	public override bool isRegularReward => true;

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (base.isServer)
		{
			float floatAmount = amountByZoneIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(floatAmount), base.transform.position);
		}
	}

	private void MirrorProcessed()
	{
	}
}
