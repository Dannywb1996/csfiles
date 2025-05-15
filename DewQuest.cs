using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public abstract class DewQuest : Actor
{
	private class QuestListener
	{
		public Action onNodesChanged;

		public Action onRoomLoaded;

		public Action onZoneLoaded;

		public Action onDestroyActor;
	}

	private List<QuestListener> _listeners = new List<QuestListener>();

	public QuestType type;

	[CompilerGenerated]
	[SyncVar(hook = "OnProgressTypeChanged")]
	private QuestProgressType _003CprogressType_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnProgressChanged")]
	private string _003CcurrentProgress_003Ek__BackingField;

	private string _questTitleRaw;

	private string _questShortDescriptionRaw;

	private string _questDetailedDescriptionRaw;

	[CompilerGenerated]
	[SyncVar]
	private int _003CaddedRoomIndex_003Ek__BackingField;

	public QuestProgressType progressType
	{
		[CompilerGenerated]
		get
		{
			return _003CprogressType_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CprogressType_003Ek__BackingField = value;
		}
	}

	public string currentProgress
	{
		[CompilerGenerated]
		get
		{
			return _003CcurrentProgress_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentProgress_003Ek__BackingField = value;
		}
	}

	public string questTitleRaw
	{
		get
		{
			return _questTitleRaw;
		}
		set
		{
			_questTitleRaw = value;
			InvokeQuestUpdated_Local();
		}
	}

	public string questShortDescriptionRaw
	{
		get
		{
			return _questShortDescriptionRaw;
		}
		set
		{
			_questShortDescriptionRaw = value;
			InvokeQuestUpdated_Local();
		}
	}

	public string questDetailedDescriptionRaw
	{
		get
		{
			return _questDetailedDescriptionRaw;
		}
		set
		{
			_questDetailedDescriptionRaw = value;
			InvokeQuestUpdated_Local();
		}
	}

	public int addedRoomIndex
	{
		[CompilerGenerated]
		get
		{
			return _003CaddedRoomIndex_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CaddedRoomIndex_003Ek__BackingField = value;
		}
	}

	public override bool isDestroyedOnRoomChange => false;

	public QuestProgressType Network_003CprogressType_003Ek__BackingField
	{
		get
		{
			return progressType;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref progressType, 4uL, OnProgressTypeChanged);
		}
	}

	public string Network_003CcurrentProgress_003Ek__BackingField
	{
		get
		{
			return currentProgress;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref currentProgress, 8uL, OnProgressChanged);
		}
	}

	public int Network_003CaddedRoomIndex_003Ek__BackingField
	{
		get
		{
			return addedRoomIndex;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref addedRoomIndex, 16uL, null);
		}
	}

	private void OnProgressTypeChanged(QuestProgressType _, QuestProgressType __)
	{
		InvokeQuestUpdated_Local();
	}

	private void OnProgressChanged(string _, string __)
	{
		InvokeQuestUpdated_Local();
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_003CaddedRoomIndex_003Ek__BackingField = NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (string.IsNullOrEmpty(questTitleRaw))
		{
			questTitleRaw = DewLocalization.GetUIValue(GetType().Name + "_Title");
		}
		if (string.IsNullOrEmpty(questShortDescriptionRaw))
		{
			questShortDescriptionRaw = DewLocalization.GetUIValue(GetType().Name + "_Description");
		}
		if (base.isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action(OnZoneLoaded);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomLoaded);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnNodesChanged += new Action(OnNodesChanged);
		}
	}

	private void OnNodesChanged()
	{
		QuestListener[] array = _listeners.ToArray();
		foreach (QuestListener g in array)
		{
			try
			{
				g.onNodesChanged?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void OnRoomLoaded(EventInfoLoadRoom _)
	{
		QuestListener[] array = _listeners.ToArray();
		foreach (QuestListener g in array)
		{
			try
			{
				g.onRoomLoaded?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void OnZoneLoaded()
	{
		QuestListener[] array = _listeners.ToArray();
		foreach (QuestListener g in array)
		{
			try
			{
				g.onZoneLoaded?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!base.isServer || NetworkedManagerBase<ZoneManager>.instance == null)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action(OnZoneLoaded);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomLoaded);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnNodesChanged -= new Action(OnNodesChanged);
		QuestListener[] array = _listeners.ToArray();
		foreach (QuestListener m in array)
		{
			try
			{
				m.onDestroyActor?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_listeners.Clear();
	}

	public virtual bool IsVisibleLocally()
	{
		return true;
	}

	[Server]
	public void CompleteQuest()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewQuest::CompleteQuest()' called when server was not active");
		}
		else
		{
			NetworkedManagerBase<QuestManager>.instance.CompleteQuest(this);
		}
	}

	[Server]
	public void FailQuest(QuestFailReason reason)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewQuest::FailQuest(QuestFailReason)' called when server was not active");
		}
		else
		{
			NetworkedManagerBase<QuestManager>.instance.FailQuest(this, reason);
		}
	}

	[Server]
	public void RemoveQuest()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewQuest::RemoveQuest()' called when server was not active");
		}
		else
		{
			NetworkedManagerBase<QuestManager>.instance.RemoveQuest(this);
		}
	}

	[ClientRpc]
	public void InvokeQuestUpdated()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void DewQuest::InvokeQuestUpdated()", 1537358562, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	public void SetLocalizedTitle(string key)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(key);
		SendRPCInternal("System.Void DewQuest::SetLocalizedTitle(System.String)", -147393230, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	public void SetLocalizedDescription(string key)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(key);
		SendRPCInternal("System.Void DewQuest::SetLocalizedDescription(System.String)", -199031730, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void SetNextGoal_ReachNode(NextGoalSettings settings)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewQuest::SetNextGoal_ReachNode(NextGoalSettings)' called when server was not active");
		}
		else
		{
			if (!base.isActive)
			{
				return;
			}
			int nodeIndex;
			bool isSuboptimal = !NetworkedManagerBase<ZoneManager>.instance.TryGetNodeIndexForNextGoal(settings.nodeIndexSettings, out nodeIndex) || NetworkedManagerBase<ZoneManager>.instance.GetHunterProgress() > 0.55f;
			if (!settings.ignoreSuboptimalSituation && isSuboptimal)
			{
				QuestListener listener = new QuestListener();
				_listeners.Add(listener);
				QuestListener questListener = listener;
				questListener.onZoneLoaded = (Action)Delegate.Combine(questListener.onZoneLoaded, (Action)delegate
				{
					_listeners.Remove(listener);
					SetNextGoal_ReachNode(settings);
				});
				if (!settings.dontChangeTitle)
				{
					SetLocalizedTitle(settings.localizedTitleKey);
				}
				if (!settings.dontChangeDescription)
				{
					SetLocalizedDescription("Quest_TravelToNextWorld");
				}
			}
			else
			{
				SetNextGoal_ReachNode_Imp(settings, nodeIndex);
			}
		}
	}

	private void SetNextGoal_ReachNode_Imp(NextGoalSettings settings, int nodeIndex)
	{
		if (!settings.dontChangeTitle)
		{
			SetLocalizedTitle(settings.localizedTitleKey);
		}
		if (!settings.dontChangeDescription)
		{
			SetLocalizedDescription(settings.localizedDescriptionKey);
		}
		QuestListener listener = new QuestListener();
		_listeners.Add(listener);
		int zoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		if (!string.IsNullOrEmpty(settings.addedModifier))
		{
			int id = NetworkedManagerBase<ZoneManager>.instance.AddModifier(nodeIndex, new ModifierData
			{
				type = settings.addedModifier,
				clientData = settings.modifierData
			});
			listener.onDestroyActor = (Action)Delegate.Combine(listener.onDestroyActor, (Action)delegate
			{
				if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex == zoneIndex && settings.revertModifierOnRemove)
				{
					NetworkedManagerBase<ZoneManager>.instance.RemoveModifier(nodeIndex, id);
				}
			});
			listener.onNodesChanged = (Action)Delegate.Combine(listener.onNodesChanged, (Action)delegate
			{
				if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex == zoneIndex && !NetworkedManagerBase<ZoneManager>.instance.nodes[nodeIndex].HasModifier(id))
				{
					InvokeFailCallback();
					if (settings.failQuestOnFail)
					{
						if (NetworkedManagerBase<ZoneManager>.instance.nodes[nodeIndex].HasModifier<RoomMod_Hunted>())
						{
							FailQuest(QuestFailReason.HunterOccupied);
						}
						else
						{
							FailQuest(QuestFailReason.NotSpecified);
						}
					}
				}
			});
		}
		if (!string.IsNullOrEmpty(settings.roomOverride))
		{
			WorldNodeData n = NetworkedManagerBase<ZoneManager>.instance.nodes[nodeIndex];
			string beforeRoom = n.room;
			n.room = settings.roomOverride;
			NetworkedManagerBase<ZoneManager>.instance.visitedNodesSaveData[nodeIndex] = null;
			if (n.status == WorldNodeStatus.HasVisited)
			{
				n.status = WorldNodeStatus.Revealed;
			}
			NetworkedManagerBase<ZoneManager>.instance.nodes[nodeIndex] = n;
			listener.onDestroyActor = (Action)Delegate.Combine(listener.onDestroyActor, (Action)delegate
			{
				if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex == zoneIndex && settings.revertRoomOnRemove)
				{
					WorldNodeData value = NetworkedManagerBase<ZoneManager>.instance.nodes[nodeIndex];
					value.room = beforeRoom;
					NetworkedManagerBase<ZoneManager>.instance.nodes[nodeIndex] = value;
				}
			});
			listener.onNodesChanged = (Action)Delegate.Combine(listener.onNodesChanged, (Action)delegate
			{
				if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex == zoneIndex && NetworkedManagerBase<ZoneManager>.instance.nodes[nodeIndex].room != settings.roomOverride)
				{
					InvokeFailCallback();
					if (settings.failQuestOnFail)
					{
						FailQuest(QuestFailReason.NotSpecified);
					}
				}
			});
		}
		listener.onRoomLoaded = (Action)Delegate.Combine(listener.onRoomLoaded, (Action)delegate
		{
			if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex != zoneIndex)
			{
				InvokeFailCallback();
				if (settings.failQuestOnFail)
				{
					FailQuest(QuestFailReason.MissedDestination);
				}
			}
			else if (NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex == nodeIndex)
			{
				try
				{
					settings.onReachDestination?.Invoke();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		});
		void InvokeFailCallback()
		{
			try
			{
				settings.onFail?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void InvokeQuestUpdated_Local()
	{
		try
		{
			NetworkedManagerBase<QuestManager>.instance.ClientEvent_OnQuestUpdated?.Invoke(this);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override bool ShouldBeSaved()
	{
		return false;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_InvokeQuestUpdated()
	{
		InvokeQuestUpdated_Local();
	}

	protected static void InvokeUserCode_InvokeQuestUpdated(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeQuestUpdated called on server.");
		}
		else
		{
			((DewQuest)obj).UserCode_InvokeQuestUpdated();
		}
	}

	protected void UserCode_SetLocalizedTitle__String(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			key = GetType().Name + "_Title";
		}
		questTitleRaw = DewLocalization.GetUIValue(key);
	}

	protected static void InvokeUserCode_SetLocalizedTitle__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetLocalizedTitle called on server.");
		}
		else
		{
			((DewQuest)obj).UserCode_SetLocalizedTitle__String(reader.ReadString());
		}
	}

	protected void UserCode_SetLocalizedDescription__String(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			key = GetType().Name + "_Description";
		}
		questShortDescriptionRaw = DewLocalization.GetUIValue(key);
	}

	protected static void InvokeUserCode_SetLocalizedDescription__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetLocalizedDescription called on server.");
		}
		else
		{
			((DewQuest)obj).UserCode_SetLocalizedDescription__String(reader.ReadString());
		}
	}

	static DewQuest()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(DewQuest), "System.Void DewQuest::InvokeQuestUpdated()", InvokeUserCode_InvokeQuestUpdated);
		RemoteProcedureCalls.RegisterRpc(typeof(DewQuest), "System.Void DewQuest::SetLocalizedTitle(System.String)", InvokeUserCode_SetLocalizedTitle__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewQuest), "System.Void DewQuest::SetLocalizedDescription(System.String)", InvokeUserCode_SetLocalizedDescription__String);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_QuestProgressType(writer, progressType);
			writer.WriteString(currentProgress);
			writer.WriteInt(addedRoomIndex);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			GeneratedNetworkCode._Write_QuestProgressType(writer, progressType);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteString(currentProgress);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteInt(addedRoomIndex);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref progressType, OnProgressTypeChanged, GeneratedNetworkCode._Read_QuestProgressType(reader));
			GeneratedSyncVarDeserialize(ref currentProgress, OnProgressChanged, reader.ReadString());
			GeneratedSyncVarDeserialize(ref addedRoomIndex, null, reader.ReadInt());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref progressType, OnProgressTypeChanged, GeneratedNetworkCode._Read_QuestProgressType(reader));
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref currentProgress, OnProgressChanged, reader.ReadString());
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref addedRoomIndex, null, reader.ReadInt());
		}
	}
}
