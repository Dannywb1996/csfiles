using Mirror;
using UnityEngine;

[RoomComponentStartDependency(typeof(RoomModifiers))]
public class RoomProps : RoomComponent
{
	private const int RandomNodeAnySectionMaxTries = 5;

	public PropSpawnRule globalRule;

	public PropSpawnRule defaultPerSectionRule;

	public override void OnRoomStartServer(WorldNodeSaveData save)
	{
		base.OnRoomStartServer(save);
		if (globalRule == null && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Combat)
		{
			globalRule = NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultProps;
		}
		if (save != null)
		{
			return;
		}
		if (globalRule != null)
		{
			SpawnProps(globalRule, null);
		}
		for (int i = 0; i < base.room.sections.Count; i++)
		{
			RoomSection s = base.room.sections[i];
			Section_Props cp = s.GetComponent<Section_Props>();
			if (cp != null && cp.sectionRule != null)
			{
				SpawnProps(cp.sectionRule, s);
			}
			else if (defaultPerSectionRule != null)
			{
				SpawnProps(defaultPerSectionRule, s);
			}
		}
	}

	private void SpawnProps(PropSpawnRule rule, RoomSection section)
	{
		if (rule == null)
		{
			return;
		}
		foreach (PropSpawnRule.PropEntry e in rule.entries)
		{
			int roomIndex = NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex;
			if (roomIndex < e.roomIndexRange.x || roomIndex > e.roomIndexRange.y)
			{
				continue;
			}
			int count = 0;
			float remainingChance = e.chance;
			IProp iprop = e.prop.GetComponent<IProp>();
			if (iprop.isRegularReward && base.room.rewards.isRegularRewardDisabled)
			{
				continue;
			}
			if (iprop.scaleSpawnRateWithPlayers)
			{
				remainingChance *= 1f + (NetworkedManagerBase<GameManager>.instance.gss.propSpawnMultiplierPerPlayer - 1f) * NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: false);
			}
			while (remainingChance > 1f)
			{
				remainingChance -= 1f;
				count += Random.Range(e.count.x, e.count.y + 1);
			}
			if (Random.value < remainingChance)
			{
				count += Random.Range(e.count.x, e.count.y + 1);
			}
			for (int i = 0; i < count; i++)
			{
				if (iprop.isSingleton)
				{
					Object anotherInstance = Object.FindObjectOfType(iprop.GetType());
					if (anotherInstance != null && !(anotherInstance is MonoBehaviour { isActiveAndEnabled: false }) && !(anotherInstance is Actor { isActive: false }))
					{
						break;
					}
				}
				Vector3 pos = default(Vector3);
				if (iprop.customSpawnPosition.HasValue)
				{
					pos = iprop.customSpawnPosition.Value;
				}
				else
				{
					TryGetGoodNodePosition(out pos);
				}
				Quaternion rot = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
				if (iprop.customSpawnRotation.HasValue)
				{
					rot = iprop.customSpawnRotation.Value;
				}
				Actor a2;
				if (e.prop.TryGetComponent<Entity>(out var ent))
				{
					Dew.SpawnEntity(ent, pos, rot, null, DewPlayer.environment, NetworkedManagerBase<GameManager>.instance.ambientLevel);
				}
				else if (e.prop.TryGetComponent<Actor>(out a2))
				{
					Dew.CreateActor(a2, pos, rot);
				}
				else
				{
					Dew.InstantiateAndSpawn(e.prop.GetComponent<NetworkIdentity>(), pos, rot);
				}
			}
		}
	}

	public bool TryGetGoodNodePosition(out Vector3 position)
	{
		position = default(Vector3);
		int iteration = 0;
		while (true)
		{
			iteration++;
			if (iteration >= 5)
			{
				break;
			}
			if (Dew.SelectRandomWeightedInList(base.room.sections, (RoomSection s) => (s.TryGetComponent<Section_Props>(out var component) && component.excludeFromGlobalRule) ? 0f : s.area).TryGetGoodNodePosition(out position))
			{
				return true;
			}
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
