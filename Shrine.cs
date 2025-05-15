using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

public abstract class Shrine : Actor, IInteractable, IProp
{
	public SafeAction<Entity> ClientEvent_OnSuccessfulUse;

	private NavMeshObstacle _obstacle;

	public Sprite mapIcon;

	public Color mapIconColor;

	public MapItemVisibility mapVisibility;

	public GameObject model;

	public GameObject availableEffect;

	public GameObject unavailableEffect;

	public GameObject lockedEffect;

	public GameObject unlockEffect;

	public GameObject useLockedEffect;

	public GameObject useEffect;

	public float interactableDelay;

	[SyncVar]
	public int maxUseCount = 1;

	public float baseGoldCost = 30f;

	public float baseHealthCost;

	public float costMultiplierPerUse = 1.3f;

	public float useCooldown = 1f;

	public bool unavailableByDefault;

	[SerializeField]
	private bool _scaleSpawnRateWithPlayers = true;

	[CompilerGenerated]
	[SyncVar]
	private int _003CcurrentUseCount_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003CgoldCost_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003ChealthCost_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsAvailableChanged")]
	private bool _003CisAvailable_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsLockedChanged")]
	private bool _003CisLocked_003Ek__BackingField;

	public virtual bool isRegularReward => false;

	int IInteractable.priority => 50;

	public int currentUseCount
	{
		[CompilerGenerated]
		get
		{
			return _003CcurrentUseCount_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentUseCount_003Ek__BackingField = value;
		}
	}

	public int goldCost
	{
		[CompilerGenerated]
		get
		{
			return _003CgoldCost_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CgoldCost_003Ek__BackingField = value;
		}
	}

	public int healthCost
	{
		[CompilerGenerated]
		get
		{
			return _003ChealthCost_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003ChealthCost_003Ek__BackingField = value;
		}
	}

	public bool isAvailable
	{
		[CompilerGenerated]
		get
		{
			return _003CisAvailable_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisAvailable_003Ek__BackingField = value;
		}
	}

	public bool isLocked
	{
		[CompilerGenerated]
		get
		{
			return _003CisLocked_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisLocked_003Ek__BackingField = value;
		}
	}

	public Transform interactPivot => base.transform;

	public virtual bool canInteractWithMouse => false;

	public float focusDistance => 3f;

	public bool scaleSpawnRateWithPlayers => _scaleSpawnRateWithPlayers;

	public int NetworkmaxUseCount
	{
		get
		{
			return maxUseCount;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref maxUseCount, 4uL, null);
		}
	}

	public int Network_003CcurrentUseCount_003Ek__BackingField
	{
		get
		{
			return currentUseCount;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref currentUseCount, 8uL, null);
		}
	}

	public int Network_003CgoldCost_003Ek__BackingField
	{
		get
		{
			return goldCost;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref goldCost, 16uL, null);
		}
	}

	public int Network_003ChealthCost_003Ek__BackingField
	{
		get
		{
			return healthCost;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref healthCost, 32uL, null);
		}
	}

	public bool Network_003CisAvailable_003Ek__BackingField
	{
		get
		{
			return isAvailable;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isAvailable, 64uL, OnIsAvailableChanged);
		}
	}

	public bool Network_003CisLocked_003Ek__BackingField
	{
		get
		{
			return isLocked;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isLocked, 128uL, OnIsLockedChanged);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_ = NetworkedManagerBase<GameManager>.instance.gss;
		CalculateCost();
		Network_003CisAvailable_003Ek__BackingField = !unavailableByDefault;
	}

	[Server]
	public void MakeAvailable()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine::MakeAvailable()' called when server was not active");
		}
		else
		{
			Network_003CisAvailable_003Ek__BackingField = true;
		}
	}

	[Server]
	public void MakeUnavailable()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine::MakeUnavailable()' called when server was not active");
		}
		else
		{
			Network_003CisAvailable_003Ek__BackingField = false;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (availableEffect != null)
		{
			FxStop(availableEffect);
		}
		if (unavailableEffect != null)
		{
			FxStop(unavailableEffect);
		}
		if (!(model != null))
		{
			return;
		}
		ListReturnHandle<Renderer> handle;
		foreach (Renderer item in model.GetComponentsInChildrenNonAlloc(out handle))
		{
			item.enabled = false;
		}
		handle.Return();
	}

	[Server]
	public void CalculateCost()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine::CalculateCost()' called when server was not active");
			return;
		}
		DewGameSessionSettings gss = NetworkedManagerBase<GameManager>.instance.gss;
		float floatGoldCost = baseGoldCost;
		int threatLevel = NetworkedManagerBase<GameManager>.instance.ambientLevel;
		floatGoldCost *= 1f + (gss.shrineCostMultiplierPerAmbientLevel - 1f) * (float)(threatLevel - 1);
		floatGoldCost *= 1f + (gss.shrineCostMultiplierPerPlayer - 1f) * NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: false);
		floatGoldCost *= 1f + (costMultiplierPerUse - 1f) * (float)currentUseCount;
		floatGoldCost *= gss.shrineCostGlobalMultiplier;
		Network_003CgoldCost_003Ek__BackingField = Mathf.RoundToInt(floatGoldCost);
		float floatHealthCost = baseHealthCost;
		floatHealthCost *= 1f + (costMultiplierPerUse - 1f) * (float)currentUseCount;
		Network_003ChealthCost_003Ek__BackingField = Mathf.RoundToInt(floatHealthCost);
	}

	protected abstract bool OnUse(Entity entity);

	public virtual Cost GetCost(Entity activator)
	{
		float healthPercentage = healthCost;
		int gold = goldCost;
		if (healthPercentage > 0f)
		{
			healthPercentage = Mathf.Clamp(healthPercentage, NetworkedManagerBase<GameManager>.instance.gss.minHealthCost, NetworkedManagerBase<GameManager>.instance.gss.maxHealthCost);
		}
		return new Cost
		{
			gold = gold,
			healthPercentage = Mathf.RoundToInt(healthPercentage)
		};
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!isAvailable)
		{
			OnIsAvailableChanged(oldVal: true, newVal: false);
		}
	}

	public AffordType CanAfford(Entity entity)
	{
		return GetCost(entity).CanAfford(entity);
	}

	private void OnIsAvailableChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			if (availableEffect != null)
			{
				FxPlay(availableEffect);
			}
			if (unavailableEffect != null)
			{
				FxStop(unavailableEffect);
			}
		}
		else
		{
			if (availableEffect != null)
			{
				FxStop(availableEffect);
			}
			if (unavailableEffect != null)
			{
				FxPlay(unavailableEffect);
			}
		}
	}

	private void OnIsLockedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			FxPlay(lockedEffect);
			FxStop(unlockEffect);
		}
		else
		{
			FxPlay(unlockEffect);
			FxStop(lockedEffect);
		}
	}

	public virtual bool CanInteract(Entity entity)
	{
		if (isAvailable)
		{
			return Time.time - base.creationTime > interactableDelay;
		}
		return false;
	}

	public virtual void OnInteract(Entity entity, bool alt)
	{
		if (!alt && base.isServer)
		{
			if (isLocked)
			{
				FxPlayNew(useLockedEffect);
			}
			else if (CanAfford(entity) == AffordType.Yes && currentUseCount < maxUseCount && isAvailable && OnUse(entity))
			{
				DoPostUseRoutines(entity);
			}
		}
	}

	public void DoPostUseRoutines(Entity entity)
	{
		Cost cost = GetCost(entity);
		if (currentUseCount < maxUseCount)
		{
			currentUseCount++;
		}
		FxPlayNewNetworked(useEffect);
		entity.owner.SpendGold(cost.gold);
		if ((float)cost.healthPercentage > 0.5f)
		{
			CreateStatusEffect(entity, default(CastInfo), delegate(Se_HealthCost h)
			{
				h.totalAmount = entity.maxHealth * (float)cost.healthPercentage / 100f;
			});
		}
		CalculateCost();
		if (useCooldown > 0.0001f)
		{
			Network_003CisAvailable_003Ek__BackingField = false;
			StartCoroutine(Routine());
		}
		else
		{
			Network_003CisAvailable_003Ek__BackingField = currentUseCount < maxUseCount;
		}
		RpcInvokeOnSuccessfulUse(entity);
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(useCooldown);
			Network_003CisAvailable_003Ek__BackingField = currentUseCount < maxUseCount;
		}
	}

	[ClientRpc]
	private void RpcInvokeOnSuccessfulUse(Entity entity)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(entity);
		SendRPCInternal("System.Void Shrine::RpcInvokeOnSuccessfulUse(Entity)", 823538102, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public Vector3 GetClosestPointOnShrine(Vector3 pos)
	{
		if (NavMesh.Raycast(pos, base.transform.position, out var hit, -1))
		{
			return hit.position;
		}
		return base.transform.position;
	}

	public Vector3 GetRandomSpawnPosition(Vector3 activatorPos)
	{
		Vector3 vector = (GetClosestPointOnShrine(activatorPos) + base.transform.position) / 2f;
		return Dew.GetValidAgentDestination_LinearSweep(vector, vector + (UnityEngine.Random.insideUnitSphere * 3f).Flattened());
	}

	public override bool ShouldBeSaved()
	{
		return true;
	}

	public override void OnSaveActor(Dictionary<string, object> data)
	{
		base.OnSaveActor(data);
		data["maxUseCount"] = maxUseCount;
		data["currentUseCount"] = currentUseCount;
		data["goldCost"] = goldCost;
		data["healthCost"] = healthCost;
		data["isAvailable"] = isAvailable;
	}

	public override void OnLoadActor(Dictionary<string, object> data)
	{
		base.OnLoadActor(data);
		NetworkmaxUseCount = (int)data["maxUseCount"];
		Network_003CcurrentUseCount_003Ek__BackingField = (int)data["currentUseCount"];
		Network_003CgoldCost_003Ek__BackingField = (int)data["goldCost"];
		Network_003ChealthCost_003Ek__BackingField = (int)data["healthCost"];
		Network_003CisAvailable_003Ek__BackingField = (bool)data["isAvailable"];
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeOnSuccessfulUse__Entity(Entity entity)
	{
		try
		{
			ClientEvent_OnSuccessfulUse?.Invoke(entity);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSuccessfulUse__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSuccessfulUse called on server.");
		}
		else
		{
			((Shrine)obj).UserCode_RpcInvokeOnSuccessfulUse__Entity(reader.ReadNetworkBehaviour<Entity>());
		}
	}

	static Shrine()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine), "System.Void Shrine::RpcInvokeOnSuccessfulUse(Entity)", InvokeUserCode_RpcInvokeOnSuccessfulUse__Entity);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(maxUseCount);
			writer.WriteInt(currentUseCount);
			writer.WriteInt(goldCost);
			writer.WriteInt(healthCost);
			writer.WriteBool(isAvailable);
			writer.WriteBool(isLocked);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteInt(maxUseCount);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteInt(currentUseCount);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteInt(goldCost);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteInt(healthCost);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteBool(isAvailable);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteBool(isLocked);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref maxUseCount, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref currentUseCount, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref goldCost, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref healthCost, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref isAvailable, OnIsAvailableChanged, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref isLocked, OnIsLockedChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref maxUseCount, null, reader.ReadInt());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref currentUseCount, null, reader.ReadInt());
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref goldCost, null, reader.ReadInt());
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref healthCost, null, reader.ReadInt());
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isAvailable, OnIsAvailableChanged, reader.ReadBool());
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isLocked, OnIsLockedChanged, reader.ReadBool());
		}
	}
}
