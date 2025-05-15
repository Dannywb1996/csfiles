using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Rift : DewNetworkBehaviour, IInteractable, IBanRoomNodesNearby
{
	private const float InternalCooldownTimePerHero = 0.5f;

	[SyncVar]
	private float _openTime;

	public GameObject fxCharge;

	public GameObject fxOpen;

	public GameObject fxLoop;

	public GameObject fxLocked;

	public GameObject fxUnlocked;

	public GameObject fxActivate;

	public float chargeDuration;

	public float afterOpenInteractableDelay;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsOpenChanged")]
	private bool _003CisOpen_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsLockedChanged")]
	private bool _003CisLocked_003Ek__BackingField;

	private Dictionary<Hero, float> _lastUseTimes = new Dictionary<Hero, float>();

	public static Rift instance { get; private set; }

	int IInteractable.priority => 100;

	public bool isOpen
	{
		[CompilerGenerated]
		get
		{
			return _003CisOpen_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisOpen_003Ek__BackingField = value;
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

	public bool canInteractWithMouse => false;

	public float focusDistance => 2.5f;

	public float Network_openTime
	{
		get
		{
			return _openTime;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _openTime, 1uL, null);
		}
	}

	public bool Network_003CisOpen_003Ek__BackingField
	{
		get
		{
			return isOpen;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isOpen, 2uL, OnIsOpenChanged);
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
			GeneratedSyncVarSetter(value, ref isLocked, 4uL, OnIsLockedChanged);
		}
	}

	private void OnIsOpenChanged(bool oldVal, bool newVal)
	{
		if (isOpen)
		{
			FxPlay(fxLoop);
			if (isLocked)
			{
				FxPlay(fxLocked);
			}
			else
			{
				FxStop(fxLocked);
			}
		}
		else
		{
			FxStop(fxLoop);
			FxStop(fxLocked);
		}
	}

	private void OnIsLockedChanged(bool oldVal, bool newVal)
	{
		if (!isOpen)
		{
			return;
		}
		if (newVal)
		{
			FxPlay(fxLocked);
			FxStop(fxUnlocked);
			return;
		}
		FxStop(fxLocked);
		FxPlay(fxUnlocked);
		if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = base.transform.position;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		instance = this;
	}

	public virtual bool CanInteract(Entity entity)
	{
		if (isOpen && !NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
		{
			return NetworkTime.time - (double)_openTime > (double)afterOpenInteractableDelay;
		}
		return false;
	}

	void IInteractable.OnInteract(Entity entity, bool alt)
	{
		if (alt || !base.isServer || NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition || !isOpen || isLocked || !(entity is Hero h) || (_lastUseTimes.TryGetValue(h, out var lastUseTime) && Time.time - lastUseTime < 0.5f))
		{
			return;
		}
		_lastUseTimes[h] = Time.time;
		try
		{
			if (OnInteractRift(h))
			{
				FxPlayNetworked(fxActivate);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected virtual bool OnInteractRift(Hero hero)
	{
		return false;
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Rift::Open()' called when server was not active");
			return;
		}
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (!isOpen)
			{
				FxPlayNetworked(fxCharge);
				yield return new WaitForSeconds(chargeDuration);
				FxStopNetworked(fxCharge);
				FxPlayNetworked(fxOpen);
				Network_openTime = (float)NetworkTime.time;
				Network_003CisOpen_003Ek__BackingField = true;
				if (!isLocked)
				{
					RpcSetObjectivePosition();
				}
			}
		}
	}

	[ClientRpc]
	private void RpcSetObjectivePosition()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Rift::RpcSetObjectivePosition()", 211845117, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void Close()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Rift::Close()' called when server was not active");
		}
		else
		{
			Network_003CisOpen_003Ek__BackingField = false;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSetObjectivePosition()
	{
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = base.transform.position;
	}

	protected static void InvokeUserCode_RpcSetObjectivePosition(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetObjectivePosition called on server.");
		}
		else
		{
			((Rift)obj).UserCode_RpcSetObjectivePosition();
		}
	}

	static Rift()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Rift), "System.Void Rift::RpcSetObjectivePosition()", InvokeUserCode_RpcSetObjectivePosition);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(_openTime);
			writer.WriteBool(isOpen);
			writer.WriteBool(isLocked);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteFloat(_openTime);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(isOpen);
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(isLocked);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _openTime, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref isOpen, OnIsOpenChanged, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref isLocked, OnIsLockedChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _openTime, null, reader.ReadFloat());
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isOpen, OnIsOpenChanged, reader.ReadBool());
		}
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isLocked, OnIsLockedChanged, reader.ReadBool());
		}
	}
}
