using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.None)]
public abstract class Room_Trap_Toggleable : Actor, IToggleableTrap, IBanRoomNodesNearby, IBanCampsNearby
{
	public GameObject fxOn;

	public GameObject fxOff;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsOnChanged")]
	private bool _003CisOn_003Ek__BackingField;

	public bool isOn
	{
		[CompilerGenerated]
		get
		{
			return _003CisOn_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisOn_003Ek__BackingField = value;
		}
	}

	public float startTime { get; private set; } = float.NegativeInfinity;

	public bool Network_003CisOn_003Ek__BackingField
	{
		get
		{
			return isOn;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isOn, 4uL, OnIsOnChanged);
		}
	}

	private void OnIsOnChanged(bool oldV, bool newV)
	{
		if (newV)
		{
			FxPlay(fxOn);
			FxStop(fxOff);
		}
		else
		{
			FxPlay(fxOff);
			FxStop(fxOn);
		}
	}

	protected virtual void OnStartTrapServer()
	{
	}

	protected virtual void OnStopTrapServer()
	{
	}

	[Server]
	public void StartTrap()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Trap_Toggleable::StartTrap()' called when server was not active");
		}
		else if (!isOn)
		{
			Network_003CisOn_003Ek__BackingField = true;
			startTime = Time.time;
			OnStartTrapServer();
		}
	}

	[Server]
	public void StopTrap()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Trap_Toggleable::StopTrap()' called when server was not active");
		}
		else if (isOn)
		{
			Network_003CisOn_003Ek__BackingField = false;
			OnStopTrapServer();
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(isOn);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(isOn);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref isOn, OnIsOnChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isOn, OnIsOnChanged, reader.ReadBool());
		}
	}
}
