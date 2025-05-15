using Mirror;

public class RoomMod_HeroSoul : RoomModifierBase
{
	private Shrine _spawnedShrine;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (uint.TryParse(base.modData.clientData, out var netId0) && NetworkServer.spawned.TryGetValue(netId0, out var netIdentity0) && netIdentity0.TryGetComponent<Hero>(out var hero) && hero.isKnockedOut)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var shrinePos);
			_spawnedShrine = CreateActor(shrinePos, null, delegate(Shrine_HeroSoul r)
			{
				r.targetHero = hero;
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && !_spawnedShrine.IsNullOrInactive())
		{
			_spawnedShrine.Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
