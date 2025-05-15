using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RoomMap))]
[RequireComponent(typeof(RoomMonsters))]
[RequireComponent(typeof(RoomProps))]
[RequireComponent(typeof(RoomRifts))]
[RequireComponent(typeof(RoomRewards))]
[RequireComponent(typeof(RoomModifiers))]
[RequireComponent(typeof(RoomEditorMaintenance))]
public class Room : SingletonDewNetworkBehaviour<Room>
{
	public SafeAction<Entity> ClientEvent_OnFootstep;

	public SafeAction<Room_Waypoint> ClientEvent_WaypointUnlocked;

	public SafeAction<Room_Waypoint> ClientEvent_WaypointLocked;

	public DewRoomMetadata metadata;

	public DewMusicItem music;

	public DewSurfaceData defaultSurface;

	public float[] availableCameraAngles = new float[1];

	public bool openRoomExitOnClear = true;

	[CompilerGenerated]
	[SyncVar]
	private int _003CnumOfActivatedCombatAreas_003Ek__BackingField;

	private RoomSection[] _sections;

	private Room_HeroSpawnPos[] _heroSpawnPoses;

	private bool _didSetupHeroSpawnPos;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisActive_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisRevisit_003Ek__BackingField;

	public SafeAction ClientEvent_OnRoomClear;

	public UnityEvent onRoomClear;

	private readonly List<Room_Waypoint> _unlockedWaypoints = new List<Room_Waypoint>();

	public readonly List<Vector3> playerPathablePoints = new List<Vector3>();

	public RoomMap map { get; private set; }

	public RoomMonsters monsters { get; private set; }

	public RoomProps props { get; private set; }

	public RoomRifts rifts { get; private set; }

	public RoomRewards rewards { get; private set; }

	public RoomModifiers modifiers { get; private set; }

	public int numOfActivatedCombatAreas
	{
		[CompilerGenerated]
		get
		{
			return _003CnumOfActivatedCombatAreas_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CnumOfActivatedCombatAreas_003Ek__BackingField = value;
		}
	}

	public IList<RoomSection> sections => _sections;

	public bool isActive
	{
		[CompilerGenerated]
		get
		{
			return _003CisActive_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisActive_003Ek__BackingField = value;
		}
	}

	public bool isRevisit
	{
		[CompilerGenerated]
		get
		{
			return _003CisRevisit_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisRevisit_003Ek__BackingField = value;
		}
	}

	public Vector3 heroSpawnPos { get; private set; }

	public Quaternion heroSpawnRot { get; private set; }

	public bool didClearRoom { get; private set; }

	public IReadOnlyList<Room_Waypoint> unlockedWaypoints => _unlockedWaypoints;

	public int Network_003CnumOfActivatedCombatAreas_003Ek__BackingField
	{
		get
		{
			return numOfActivatedCombatAreas;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref numOfActivatedCombatAreas, 1uL, null);
		}
	}

	public bool Network_003CisActive_003Ek__BackingField
	{
		get
		{
			return isActive;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isActive, 2uL, null);
		}
	}

	public bool Network_003CisRevisit_003Ek__BackingField
	{
		get
		{
			return isRevisit;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isRevisit, 4uL, null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		map = GetComponent<RoomMap>();
		monsters = GetComponent<RoomMonsters>();
		props = GetComponent<RoomProps>();
		rifts = GetComponent<RoomRifts>();
		rewards = GetComponent<RoomRewards>();
		modifiers = GetComponent<RoomModifiers>();
		if (defaultSurface == null)
		{
			defaultSurface = Resources.Load<GameObject>("Footsteps/Surface_Default").GetComponent<DewSurfaceData>();
		}
	}

	private void Start()
	{
		IPlayerPathableArea[] array = Dew.FindInterfacesOfType<IPlayerPathableArea>(includeInactive: true);
		foreach (IPlayerPathableArea p in array)
		{
			playerPathablePoints.Add(Dew.GetValidAgentPosition(p.pathablePosition));
		}
		_sections = UnityEngine.Object.FindObjectsOfType<RoomSection>();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		onRoomClear.AddListener(delegate
		{
			if (NetworkServer.active)
			{
				RpcInvokeOnRoomClear();
			}
		});
	}

	[ClientRpc]
	private void RpcInvokeOnRoomClear()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Room::RpcInvokeOnRoomClear()", 109277974, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if (NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex < 0)
		{
			return;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.roomRotIndex < 0)
		{
			if (!NetworkedManagerBase<ZoneManager>.instance._usedAngleIndexes.TryGetValue(base.name, out var used))
			{
				used = new List<int>();
				NetworkedManagerBase<ZoneManager>.instance._usedAngleIndexes.Add(base.name, used);
			}
			ListReturnHandle<int> handle;
			List<int> unused = DewPool.GetList(out handle);
			for (int i = 0; i < availableCameraAngles.Length; i++)
			{
				if (!used.Contains(i))
				{
					unused.Add(i);
				}
			}
			int newRotIndex;
			if (unused.Count == 0)
			{
				newRotIndex = UnityEngine.Random.Range(0, availableCameraAngles.Length);
				used.Clear();
				used.Add(newRotIndex);
			}
			else
			{
				newRotIndex = unused[UnityEngine.Random.Range(0, unused.Count)];
				used.Add(newRotIndex);
			}
			handle.Return();
			NetworkedManagerBase<ZoneManager>.instance.SetRoomRotIndex(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, newRotIndex);
		}
		SyncCameraAngle();
	}

	[Server]
	public void SyncCameraAngle()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::SyncCameraAngle()' called when server was not active");
			return;
		}
		int index = NetworkedManagerBase<ZoneManager>.instance.currentNode.roomRotIndex;
		SetCameraAngleIndex_Local(index);
		RpcSetCameraAngleIndex_Imp(index);
	}

	[ClientRpc]
	internal void RpcSetCameraAngleIndex_Imp(int index)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(index);
		SendRPCInternal("System.Void Room::RpcSetCameraAngleIndex_Imp(System.Int32)", 45229983, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void SetCameraAngleIndex_Local(int index)
	{
		if (availableCameraAngles == null || index < 0 || index >= availableCameraAngles.Length)
		{
			ManagerBase<CameraManager>.instance.entityCamAngle = 0f;
			Debug.LogWarning("Received invalid camera angle for room " + base.name);
		}
		else
		{
			ManagerBase<CameraManager>.instance.entityCamAngle = availableCameraAngles[index];
		}
		ManagerBase<CameraManager>.instance.SnapCameraToFocusedEntity();
	}

	public override void OnLateStart()
	{
		base.OnLateStart();
		if (music == null && NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss && !NetworkedManagerBase<ZoneManager>.instance.isSidetracking)
		{
			music = NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultMusic;
		}
		ManagerBase<MusicManager>.instance.Play(music);
	}

	public Vector3 GetHeroSpawnPosition()
	{
		Vector3 vector = heroSpawnPos;
		Vector3 pos = vector + UnityEngine.Random.onUnitSphere * 2.5f;
		pos = Dew.GetPositionOnGround(pos);
		return Dew.GetValidAgentDestination_LinearSweep(vector, pos);
	}

	public Quaternion GetHeroSpawnRotation()
	{
		return heroSpawnRot;
	}

	public RoomSection GetSectionFromWorldPos(Vector3 pos)
	{
		foreach (RoomSection s in sections)
		{
			if (s.OverlapPoint(pos.ToXY()))
			{
				return s;
			}
		}
		return null;
	}

	[Server]
	public void ClearRoom()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::ClearRoom()' called when server was not active");
		}
		else
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (!didClearRoom)
			{
				didClearRoom = true;
				try
				{
					onRoomClear?.Invoke();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				foreach (Entity e in new List<Entity>(NetworkedManagerBase<ActorManager>.instance.allEntities))
				{
					if (!e.IsNullInactiveDeadOrKnockedOut() && e is Monster m && !(m.owner != DewPlayer.creep) && !m.AI.disableAI && !NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted && !(Time.time - m.creationTime < 3f) && (m.isSleeping || m.AI.context.targetEnemy == null))
					{
						Debug.Log("Destroying leftover monster: " + e.GetActorReadableName());
						e.Destroy();
					}
				}
				if (openRoomExitOnClear)
				{
					rifts.OpenRifts();
				}
			}
			yield break;
		}
	}

	[Server]
	public void AddUnlockedWaypoint(Room_Waypoint waypoint)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::AddUnlockedWaypoint(Room_Waypoint)' called when server was not active");
		}
		else if (!_unlockedWaypoints.Contains(waypoint))
		{
			_unlockedWaypoints.Add(waypoint);
			RpcInvokeWaypointEvent(waypoint, isUnlocked: true);
		}
	}

	[Server]
	public void RemoveUnlockedWaypoint(Room_Waypoint waypoint)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::RemoveUnlockedWaypoint(Room_Waypoint)' called when server was not active");
		}
		else if (_unlockedWaypoints.Contains(waypoint))
		{
			_unlockedWaypoints.Remove(waypoint);
			RpcInvokeWaypointEvent(waypoint, isUnlocked: false);
		}
	}

	[ClientRpc]
	private void RpcInvokeWaypointEvent(Room_Waypoint waypoint, bool isUnlocked)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(waypoint);
		writer.WriteBool(isUnlocked);
		SendRPCInternal("System.Void Room::RpcInvokeWaypointEvent(Room_Waypoint,System.Boolean)", -1139962996, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void StartRoom(WorldNodeSaveData save)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::StartRoom(WorldNodeSaveData)' called when server was not active");
			return;
		}
		_sections = UnityEngine.Object.FindObjectsOfType<RoomSection>();
		Network_003CisActive_003Ek__BackingField = true;
		Network_003CisRevisit_003Ek__BackingField = save != null;
		didClearRoom = save != null;
		if (!_didSetupHeroSpawnPos)
		{
			Room_HeroSpawnPos[] poses = UnityEngine.Object.FindObjectsOfType<Room_HeroSpawnPos>();
			Transform selected = poses[UnityEngine.Random.Range(0, poses.Length)].transform;
			_didSetupHeroSpawnPos = true;
			heroSpawnPos = Dew.GetValidAgentPosition(selected.position);
			heroSpawnRot = selected.rotation;
		}
		RoomComponent[] rcs = GetComponentsInChildren<RoomComponent>();
		rcs = GetSortedByStartDependency(rcs);
		RoomComponent[] array = rcs;
		foreach (RoomComponent r in array)
		{
			try
			{
				r.isRoomActive = true;
				r.OnRoomStartServer(save);
				r.OnRoomStart(save != null);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		RpcInvokeRoomStart(save != null);
	}

	[ClientRpc]
	private void RpcInvokeRoomStart(bool isRevisit)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isRevisit);
		SendRPCInternal("System.Void Room::RpcInvokeRoomStart(System.Boolean)", -1474880575, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void StopRoom(WorldNodeSaveData save)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::StopRoom(WorldNodeSaveData)' called when server was not active");
			return;
		}
		Network_003CisActive_003Ek__BackingField = false;
		RoomComponent[] componentsInChildren = GetComponentsInChildren<RoomComponent>();
		foreach (RoomComponent r in componentsInChildren)
		{
			try
			{
				r.isRoomActive = false;
				r.OnRoomStopServer(save);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		RpcInvokeRoomStop();
	}

	[ClientRpc]
	private void RpcInvokeRoomStop()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Room::RpcInvokeRoomStop()", 1906869436, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void OverrideHeroSpawn(Vector3 pos, Quaternion rot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room::OverrideHeroSpawn(UnityEngine.Vector3,UnityEngine.Quaternion)' called when server was not active");
			return;
		}
		_didSetupHeroSpawnPos = true;
		heroSpawnPos = pos;
		heroSpawnRot = rot;
	}

	public RoomSection GetFinalSection()
	{
		Vector3 finalRiftPos = ((Rift_RoomExit.instance != null) ? Rift_RoomExit.instance.transform.position : Rift.instance.transform.position);
		return Dew.SelectBestWithScore(sections, (RoomSection section, int i) => 0f - Vector2.Distance(finalRiftPos.ToXY(), section.transform.position.ToXY()));
	}

	private RoomComponent[] GetSortedByStartDependency(RoomComponent[] arr)
	{
		RoomComponentStartDependencyAttribute[][] deps = new RoomComponentStartDependencyAttribute[arr.Length][];
		for (int i = 0; i < arr.Length; i++)
		{
			object[] attrs = arr[i].GetType().GetCustomAttributes(typeof(RoomComponentStartDependencyAttribute), inherit: true);
			deps[i] = new RoomComponentStartDependencyAttribute[attrs.Length];
			for (int j = 0; j < attrs.Length; j++)
			{
				deps[i][j] = (RoomComponentStartDependencyAttribute)attrs[j];
			}
		}
		Dictionary<Type, int> indices = new Dictionary<Type, int>();
		for (int k = 0; k < arr.Length; k++)
		{
			indices[arr[k].GetType()] = k;
		}
		List<RoomComponent> sortedArr = new List<RoomComponent>();
		Queue<RoomComponent> queue = new Queue<RoomComponent>();
		foreach (RoomComponent rc in arr)
		{
			if (deps[indices[rc.GetType()]].Length == 0)
			{
				queue.Enqueue(rc);
			}
		}
		while (queue.Count > 0)
		{
			RoomComponent rc2 = queue.Dequeue();
			sortedArr.Add(rc2);
			for (int m = 0; m < arr.Length; m++)
			{
				if (deps[m].Any((RoomComponentStartDependencyAttribute dep) => dep.targetRoomComponent == rc2.GetType()))
				{
					deps[m] = deps[m].Where((RoomComponentStartDependencyAttribute dep) => dep.targetRoomComponent != rc2.GetType()).ToArray();
					if (deps[m].Length == 0)
					{
						queue.Enqueue(arr[m]);
					}
				}
			}
		}
		if (sortedArr.Count < arr.Length)
		{
			throw new Exception("Circular dependency detected.");
		}
		return sortedArr.ToArray();
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeOnRoomClear()
	{
		ClientEvent_OnRoomClear?.Invoke();
	}

	protected static void InvokeUserCode_RpcInvokeOnRoomClear(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnRoomClear called on server.");
		}
		else
		{
			((Room)obj).UserCode_RpcInvokeOnRoomClear();
		}
	}

	protected void UserCode_RpcSetCameraAngleIndex_Imp__Int32(int index)
	{
		if (!base.isServer)
		{
			SetCameraAngleIndex_Local(index);
		}
	}

	protected static void InvokeUserCode_RpcSetCameraAngleIndex_Imp__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCameraAngleIndex_Imp called on server.");
		}
		else
		{
			((Room)obj).UserCode_RpcSetCameraAngleIndex_Imp__Int32(reader.ReadInt());
		}
	}

	protected void UserCode_RpcInvokeWaypointEvent__Room_Waypoint__Boolean(Room_Waypoint waypoint, bool isUnlocked)
	{
		try
		{
			if (isUnlocked)
			{
				ClientEvent_WaypointUnlocked?.Invoke(waypoint);
			}
			else
			{
				ClientEvent_WaypointLocked?.Invoke(waypoint);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeWaypointEvent__Room_Waypoint__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeWaypointEvent called on server.");
		}
		else
		{
			((Room)obj).UserCode_RpcInvokeWaypointEvent__Room_Waypoint__Boolean(reader.ReadNetworkBehaviour<Room_Waypoint>(), reader.ReadBool());
		}
	}

	protected void UserCode_RpcInvokeRoomStart__Boolean(bool isRevisit)
	{
		if (base.isServer)
		{
			return;
		}
		RoomComponent[] rcs = GetComponentsInChildren<RoomComponent>();
		rcs = GetSortedByStartDependency(rcs);
		RoomComponent[] array = rcs;
		foreach (RoomComponent r in array)
		{
			try
			{
				r.isRoomActive = true;
				r.OnRoomStart(isRevisit);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected static void InvokeUserCode_RpcInvokeRoomStart__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeRoomStart called on server.");
		}
		else
		{
			((Room)obj).UserCode_RpcInvokeRoomStart__Boolean(reader.ReadBool());
		}
	}

	protected void UserCode_RpcInvokeRoomStop()
	{
		RoomComponent[] componentsInChildren = GetComponentsInChildren<RoomComponent>();
		foreach (RoomComponent r in componentsInChildren)
		{
			try
			{
				r.isRoomActive = false;
				r.OnRoomStop();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected static void InvokeUserCode_RpcInvokeRoomStop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeRoomStop called on server.");
		}
		else
		{
			((Room)obj).UserCode_RpcInvokeRoomStop();
		}
	}

	static Room()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcInvokeOnRoomClear()", InvokeUserCode_RpcInvokeOnRoomClear);
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcSetCameraAngleIndex_Imp(System.Int32)", InvokeUserCode_RpcSetCameraAngleIndex_Imp__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcInvokeWaypointEvent(Room_Waypoint,System.Boolean)", InvokeUserCode_RpcInvokeWaypointEvent__Room_Waypoint__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcInvokeRoomStart(System.Boolean)", InvokeUserCode_RpcInvokeRoomStart__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(Room), "System.Void Room::RpcInvokeRoomStop()", InvokeUserCode_RpcInvokeRoomStop);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(numOfActivatedCombatAreas);
			writer.WriteBool(isActive);
			writer.WriteBool(isRevisit);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(numOfActivatedCombatAreas);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(isActive);
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(isRevisit);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref numOfActivatedCombatAreas, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref isActive, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref isRevisit, null, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref numOfActivatedCombatAreas, null, reader.ReadInt());
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isActive, null, reader.ReadBool());
		}
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isRevisit, null, reader.ReadBool());
		}
	}
}
