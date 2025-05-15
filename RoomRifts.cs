using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class RoomRifts : RoomComponent
{
	private class RiftSaveData
	{
		public Dictionary<ulong, bool> isRiftOpen = new Dictionary<ulong, bool>();
	}

	public List<string> openedSidetrackRifts = new List<string>();

	private Rift_Sidetrack[] _sidetrackRifts;

	private List<(Vector3, Quaternion)> _sidetrackRiftPositions = new List<(Vector3, Quaternion)>();

	public override void OnRoomStartServer(WorldNodeSaveData save)
	{
		base.OnRoomStartServer(save);
		Rift[] rifts = Object.FindObjectsOfType<Rift>();
		if (save != null)
		{
			foreach (KeyValuePair<ulong, bool> pair in save.Get<RiftSaveData>().isRiftOpen)
			{
				Rift[] array = rifts;
				foreach (Rift r in array)
				{
					if (r.netIdentity.sceneId == pair.Key)
					{
						r.isOpen = pair.Value;
						if (r.isOpen && r.fxLoop.TryGetComponent<ParticleSystem>(out var ps))
						{
							ps.Simulate(2f);
							ps.Play();
						}
						break;
					}
				}
			}
		}
		if (Rift_RoomExit.instance != null)
		{
			Rift_RoomExit.instance.GetSidetrackPortalPositions(_sidetrackRiftPositions);
		}
		_sidetrackRifts = DewResources.FindAllByNameSubstring<Rift_Sidetrack>("Rift_Sidetrack_").ToArray();
	}

	public override void OnRoomStopServer(WorldNodeSaveData save)
	{
		base.OnRoomStopServer(save);
		Rift[] array = Object.FindObjectsOfType<Rift>();
		RiftSaveData newSave = new RiftSaveData();
		Rift[] array2 = array;
		foreach (Rift r in array2)
		{
			newSave.isRiftOpen.Add(r.netIdentity.sceneId, value: true);
		}
		save.Set(newSave);
	}

	public void OpenRifts()
	{
		if (Rift_RoomExit.instance != null)
		{
			Rift_RoomExit.instance.Open();
		}
		else if (Rift_Sidetrack.instance != null)
		{
			Rift_Sidetrack.instance.Open();
		}
		else
		{
			Debug.LogWarning("No rift to open on clear room: " + base.name);
		}
		List<Rift_Sidetrack> wantsToSpawn = new List<Rift_Sidetrack>();
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Combat)
		{
			Rift_Sidetrack[] sidetrackRifts = _sidetrackRifts;
			foreach (Rift_Sidetrack s in sidetrackRifts)
			{
				if (s.isValid && (openedSidetrackRifts.Contains(s.name) || (!(Random.value >= s.spawnChance) && !NetworkedManagerBase<ZoneManager>.instance._bannedSidetracksForCurrentLoop.Contains(s.name) && (s.allowedZones == null || s.allowedZones.Length == 0 || s.allowedZones.Contains(NetworkedManagerBase<ZoneManager>.instance.currentZone.name)))))
				{
					wantsToSpawn.Add(s);
				}
			}
		}
		int sumOfZones = DewBuildProfile.current.content.zoneCountByTier.Sum();
		if ((DewBuildProfile.current.buildType != BuildType.DemoLite || !DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth)) && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss && NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex % sumOfZones == sumOfZones - 1)
		{
			wantsToSpawn.Add(DewResources.GetByName<Rift_Sidetrack>("Rift_Sidetrack_TheDream"));
		}
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(1.8f);
			for (int j = 0; j < wantsToSpawn.Count; j++)
			{
				Rift_Sidetrack prefab = wantsToSpawn[j];
				CreateSidetrackRift(prefab);
				yield return new WaitForSeconds(0.35f);
			}
		}
	}

	[Server]
	public void CreateSidetrackRift(Rift_Sidetrack prefab)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomRifts::CreateSidetrackRift(Rift_Sidetrack)' called when server was not active");
			return;
		}
		if (_sidetrackRiftPositions.Count == 0)
		{
			Debug.LogWarning("No space to create " + prefab.name + " in " + SingletonDewNetworkBehaviour<Room>.instance.name);
			return;
		}
		(Vector3, Quaternion) pos = _sidetrackRiftPositions[0];
		_sidetrackRiftPositions.RemoveAt(0);
		if (prefab.oncePerLoop)
		{
			NetworkedManagerBase<ZoneManager>.instance._bannedSidetracksForCurrentLoop.Add(prefab.name);
		}
		Debug.Log("Creating " + prefab.name + " in " + SingletonDewNetworkBehaviour<Room>.instance.name);
		Rift_Sidetrack newRift = Dew.InstantiateAndSpawn(prefab, pos.Item1, pos.Item2);
		if (Rift_RoomExit.instance != null && Rift_RoomExit.instance.isLocked)
		{
			newRift.isLocked = true;
		}
		newRift.Open();
	}

	private void MirrorProcessed()
	{
	}
}
