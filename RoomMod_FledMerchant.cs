public class RoomMod_FledMerchant : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			PropEnt_Merchant_Jonas shop = Dew.FindActorOfType<PropEnt_Merchant_Jonas>();
			if (!(shop == null))
			{
				shop.Destroy();
				PlaceShrine<Shrine_Merchant_Backpack>(new PlaceShrineSettings
				{
					customPosition = shop.position,
					lockedUntilCleared = true,
					removeModifierOnUse = false
				});
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
