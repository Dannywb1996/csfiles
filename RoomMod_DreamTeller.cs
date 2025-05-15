using System.Linq;
using UnityEngine;

public class RoomMod_DreamTeller : RoomModifierBase
{
	public Vector2Int spawnedDreamDust;

	public Vector2Int spawnedGold;

	public Vector2Int spawnedStardust;

	private Mon_DreamTeller _dreamTeller;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (!base.isNewInstance)
		{
			return;
		}
		RoomSection final = SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection();
		final.TryGetGoodNodePosition(out var pos);
		_dreamTeller = SpawnEntity<Mon_DreamTeller>(pos, null, DewPlayer.environment, 1);
		foreach (RoomSection section in SingletonDewNetworkBehaviour<Room>.instance.sections)
		{
			section.monsters.combatAreaSettings = SectionCombatAreaType.No;
			section.monsters.isMarkedAsCombatArea = false;
		}
		final.clearRoomOnEnterFirstTime = true;
		int dd = Random.Range(spawnedDreamDust.x, spawnedDreamDust.y + 1);
		int g = Random.Range(spawnedGold.x, spawnedGold.y + 1);
		int sd = Random.Range(spawnedStardust.x, spawnedStardust.y + 1);
		for (int i = 0; i < dd; i++)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var shrinePos);
			SpawnEntity<PropEnt_Stone_DreamDust>(shrinePos, Quaternion.Euler(0f, Random.Range(0, 360), 0f), DewPlayer.environment, 1);
		}
		for (int j = 0; j < g; j++)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var shrinePos2);
			SpawnEntity<PropEnt_Stone_Gold>(shrinePos2, Quaternion.Euler(0f, Random.Range(0, 360), 0f), DewPlayer.environment, 1);
		}
		for (int k = 0; k < sd; k++)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var shrinePos3);
			CreateActor(shrinePos3, Quaternion.Euler(0f, Random.Range(0, 360), 0f), delegate(Shrine_Stardust s)
			{
				s.amount = 1;
			});
		}
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		Room_Barrier[] array = Object.FindObjectsOfType<Room_Barrier>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Open();
		}
		SingletonDewNetworkBehaviour<Room>.instance.monsters.RemoveAllCamps();
		Entity[] array2 = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] is Monster m && m.GetTeamRelation(DewPlayer.local) == TeamRelation.Enemy)
			{
				m.Destroy();
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && !_dreamTeller.IsNullInactiveDeadOrKnockedOut())
		{
			_dreamTeller.Destroy();
		}
	}

	public override bool IsAvailableInGame()
	{
		return NetworkedManagerBase<QuestManager>.instance.currentArtifact != null;
	}

	private void MirrorProcessed()
	{
	}
}
