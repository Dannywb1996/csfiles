using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Room_Waypoint : DewNetworkBehaviour, IPlayerPathableArea
{
	public GameObject lockedEffect;

	public GameObject unlockedEffect;

	public DewCollider dewCollider;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsUnlockedChanged")]
	private bool _003CisUnlocked_003Ek__BackingField;

	private RoomSection _section;

	public bool isUnlocked
	{
		[CompilerGenerated]
		get
		{
			return _003CisUnlocked_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisUnlocked_003Ek__BackingField = value;
		}
	}

	Vector3 IPlayerPathableArea.pathablePosition => base.transform.position;

	public bool Network_003CisUnlocked_003Ek__BackingField
	{
		get
		{
			return isUnlocked;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isUnlocked, 1uL, OnIsUnlockedChanged);
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		OnIsUnlockedChanged(oldVal: false, isUnlocked);
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		_section = SingletonDewNetworkBehaviour<Room>.instance.GetSectionFromWorldPos(base.transform.position);
		if (_section != null)
		{
			_section.monsters.onClearCombatArea.AddListener(Unlock);
		}
		dewCollider.receiveEntityCallbacks = true;
		dewCollider.onEntityEnter.AddListener(delegate(Entity e)
		{
			if (!isUnlocked && e is Hero && (!(_section != null) || (!_section.monsters.isCombatActive && (!_section.monsters.isMarkedAsCombatArea || _section.monsters.didClearCombatArea))))
			{
				Unlock();
			}
		});
		dewCollider.UpdateProxyCollider();
	}

	[Server]
	public void Unlock()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Waypoint::Unlock()' called when server was not active");
			return;
		}
		Network_003CisUnlocked_003Ek__BackingField = true;
		SingletonDewNetworkBehaviour<Room>.instance.AddUnlockedWaypoint(this);
	}

	[Server]
	public void Lock()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Waypoint::Lock()' called when server was not active");
			return;
		}
		Network_003CisUnlocked_003Ek__BackingField = true;
		SingletonDewNetworkBehaviour<Room>.instance.RemoveUnlockedWaypoint(this);
	}

	private void OnIsUnlockedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			FxPlay(unlockedEffect);
			FxStop(lockedEffect);
		}
		else
		{
			FxPlay(lockedEffect);
			FxStop(unlockedEffect);
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
			writer.WriteBool(isUnlocked);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(isUnlocked);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref isUnlocked, OnIsUnlockedChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isUnlocked, OnIsUnlockedChanged, reader.ReadBool());
		}
	}
}
