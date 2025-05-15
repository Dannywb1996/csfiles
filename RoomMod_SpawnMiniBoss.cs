using UnityEngine;

public class RoomMod_SpawnMiniBoss : RoomModifierBase
{
	private RoomSection _section;

	public override void OnStartServer()
	{
		base.OnStartServer();
		Rift_RoomExit exitPortal = Object.FindObjectOfType<Rift_RoomExit>();
		if (!(exitPortal == null))
		{
			_section = Dew.SelectBestWithScore(SingletonDewNetworkBehaviour<Room>.instance.sections, (RoomSection section, int i) => 0f - Vector2.Distance(exitPortal.transform.position.ToXY(), section.transform.position.ToXY()));
			if (!(_section == null) && !_section.monsters.spawnMiniBossInstead)
			{
				_section.monsters.spawnMiniBossInstead = true;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && _section != null)
		{
			_section.monsters.spawnMiniBossInstead = false;
			_section = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
