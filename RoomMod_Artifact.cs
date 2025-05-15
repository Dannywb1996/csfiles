using System;
using System.Collections;
using UnityEngine;

public class RoomMod_Artifact : RoomModifierBase
{
	public float dropChance = 0.1f;

	private Vector3 _lastKillPosition;

	private bool _didDrop;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (base.isNewInstance)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDeath += new Action<EventInfoKill>(HandleMonsterDeath);
			SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(DropArtifact);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDeath -= new Action<EventInfoKill>(HandleMonsterDeath);
		}
	}

	private void HandleMonsterDeath(EventInfoKill obj)
	{
		if (!_didDrop && obj.victim is Monster)
		{
			_lastKillPosition = obj.victim.agentPosition;
			if (UnityEngine.Random.value < dropChance)
			{
				DropArtifact();
			}
		}
	}

	private void DropArtifact()
	{
		if (!_didDrop)
		{
			_didDrop = true;
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1f));
			if (!(this == null) && base.isServer && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
			{
				Artifact prefab = DewResources.GetByType<Artifact>(Dew.SelectRandomWeightedInReadOnlyList(NetworkedManagerBase<QuestManager>.instance.artifactPool, (Type t) => (!NetworkedManagerBase<QuestManager>.instance.undiscoveredArtifacts.Contains(t.Name)) ? 1f : 2.5f));
				CreateActor(prefab, Dew.GetGoodRewardPosition(_lastKillPosition), null);
				RemoveModifier();
			}
		}
	}

	public override bool IsAvailableInGame()
	{
		if (!NetworkedManagerBase<QuestManager>.instance.didCollectArtifactThisLoop)
		{
			return NetworkedManagerBase<QuestManager>.instance.currentArtifact == null;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
