using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class QuestManager : NetworkedManagerBase<QuestManager>
{
	public SafeAction<DewQuest> ClientEvent_OnQuestStarted;

	public SafeAction<DewQuest> ClientEvent_OnQuestUpdated;

	public SafeAction<DewQuest, QuestFailReason> ClientEvent_OnQuestFailed;

	public SafeAction<DewQuest> ClientEvent_OnQuestCompleted;

	public SafeAction<DewQuest> ClientEvent_OnQuestRemoved;

	private readonly SyncList<DewQuest> _activeQuests = new SyncList<DewQuest>();

	public SafeAction<string, DewPlayer, Vector3> ClientEvent_OnArtifactPickedUp;

	public SafeAction<string> ClientEvent_OnArtifactRemoved;

	public SafeAction<string, bool> ClientEvent_OnArtifactAppraised;

	[CompilerGenerated]
	[SyncVar]
	private string _003CcurrentArtifact_003Ek__BackingField;

	[NonSerialized]
	public bool didCollectArtifactThisLoop;

	[NonSerialized]
	public List<string> undiscoveredArtifacts = new List<string>();

	public IReadOnlyList<DewQuest> activeQuests => _activeQuests;

	public Type[] artifactPool { get; private set; }

	public string currentArtifact
	{
		[CompilerGenerated]
		get
		{
			return _003CcurrentArtifact_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CcurrentArtifact_003Ek__BackingField = value;
		}
	}

	public string Network_003CcurrentArtifact_003Ek__BackingField
	{
		get
		{
			return currentArtifact;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref currentArtifact, 1uL, null);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		DoArtifactStartServer();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		DoArtifactStartClient();
		ClientEvent_OnQuestStarted += (Action<DewQuest>)delegate(DewQuest quest)
		{
			if (quest.IsVisibleLocally())
			{
				NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
				{
					type = ChatManager.MessageType.Notice,
					content = "Chat_QuestStarted",
					args = new string[1] { quest.questTitleRaw }
				});
			}
		};
		ClientEvent_OnQuestCompleted += (Action<DewQuest>)delegate(DewQuest quest)
		{
			if (quest.IsVisibleLocally())
			{
				NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
				{
					type = ChatManager.MessageType.Notice,
					content = "Chat_QuestCompleted",
					args = new string[1] { quest.questTitleRaw }
				});
			}
		};
		ClientEvent_OnQuestFailed += (Action<DewQuest, QuestFailReason>)delegate(DewQuest quest, QuestFailReason reason)
		{
			if (quest.IsVisibleLocally())
			{
				string text = quest.questTitleRaw;
				if (reason != QuestFailReason.NotSpecified)
				{
					text = text + " (" + DewLocalization.GetUIValue("InGame_Quest_Message_QuestFailed_Reason_" + reason) + ")";
				}
				NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
				{
					type = ChatManager.MessageType.Notice,
					content = "Chat_QuestFailed",
					args = new string[1] { text }
				});
			}
		};
	}

	public bool TryGetQuest(Type type, out DewQuest quest)
	{
		quest = null;
		foreach (DewQuest q in activeQuests)
		{
			if (type.IsInstanceOfType(q))
			{
				quest = q;
				return true;
			}
		}
		return false;
	}

	public bool TryGetQuest<T>(out T quest) where T : DewQuest
	{
		DewQuest q;
		bool result = TryGetQuest(typeof(T), out q);
		quest = (T)q;
		return result;
	}

	public bool HasQuest<T>() where T : DewQuest
	{
		T quest;
		return TryGetQuest<T>(out quest);
	}

	public bool HasQuest(Type type)
	{
		DewQuest quest;
		return TryGetQuest(type, out quest);
	}

	[Server]
	public T StartQuest<T>(Action<T> beforePrepare = null) where T : DewQuest
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T QuestManager::StartQuest(System.Action`1<T>)' called when server was not active");
			return null;
		}
		return StartQuest(DewResources.GetByType<T>(), beforePrepare);
	}

	[Server]
	public T StartQuest<T>(T prefab, Action<T> beforePrepare = null) where T : DewQuest
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T QuestManager::StartQuest(T,System.Action`1<T>)' called when server was not active");
			return null;
		}
		T quest = Dew.CreateActor(prefab, Vector3.zero, Quaternion.identity, null, beforePrepare);
		_activeQuests.Add(quest);
		GameManager.CallOnReady(delegate
		{
			RpcInvokeQuestStarted(quest);
		});
		return quest;
	}

	internal void CompleteQuest(DewQuest quest)
	{
		if (!quest.IsNullOrInactive())
		{
			_activeQuests.Remove(quest);
			GameManager.CallOnReady(delegate
			{
				RpcInvokeQuestComplete(quest);
				quest.Destroy();
			});
		}
	}

	internal void FailQuest(DewQuest quest, QuestFailReason reason)
	{
		if (!quest.IsNullOrInactive())
		{
			_activeQuests.Remove(quest);
			GameManager.CallOnReady(delegate
			{
				RpcInvokeQuestFail(quest, reason);
				quest.Destroy();
			});
		}
	}

	internal void RemoveQuest(DewQuest quest)
	{
		if (!quest.IsNullOrInactive())
		{
			_activeQuests.Remove(quest);
			GameManager.CallOnReady(delegate
			{
				RpcInvokeQuestRemoved(quest);
				quest.Destroy();
			});
		}
	}

	[ClientRpc]
	private void RpcInvokeQuestStarted(DewQuest quest)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(quest);
		SendRPCInternal("System.Void QuestManager::RpcInvokeQuestStarted(DewQuest)", 771195746, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeQuestComplete(DewQuest quest)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(quest);
		SendRPCInternal("System.Void QuestManager::RpcInvokeQuestComplete(DewQuest)", -180534462, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeQuestFail(DewQuest quest, QuestFailReason reason)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(quest);
		GeneratedNetworkCode._Write_QuestFailReason(writer, reason);
		SendRPCInternal("System.Void QuestManager::RpcInvokeQuestFail(DewQuest,QuestFailReason)", 1041690671, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeQuestRemoved(DewQuest quest)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(quest);
		SendRPCInternal("System.Void QuestManager::RpcInvokeQuestRemoved(DewQuest)", 326586497, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void DoArtifactStartServer()
	{
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnLoopStarted += (Action)delegate
		{
			didCollectArtifactThisLoop = false;
		};
		artifactPool = Dew.allArtifacts.Where((Type t) => Dew.IsArtifactIncludedInGame(t.Name)).ToArray();
	}

	private void DoArtifactStartClient()
	{
		string[] undiscovered = DewSave.profile.artifacts.Keys.Where((string key) => DewSave.profile.artifacts[key].status != UnlockStatus.Complete && Dew.IsArtifactIncludedInGame(key)).ToArray();
		AddToUndiscoveredArtifacts(undiscovered);
		InGameUIManager inGameUIManager = InGameUIManager.instance;
		inGameUIManager.onStateChanged = (Action<string, string>)Delegate.Combine(inGameUIManager.onStateChanged, (Action<string, string>)delegate(string from, string to)
		{
			if (!(DewPlayer.local == null))
			{
				DewPlayer.local.isReadingArtifactStory = to == "Artifact";
			}
		});
	}

	[Server]
	public void PickUpArtifact(string artifactName, DewPlayer player, Vector3 worldPos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void QuestManager::PickUpArtifact(System.String,DewPlayer,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		Network_003CcurrentArtifact_003Ek__BackingField = artifactName;
		RpcInvokePickUpArtifact(artifactName, player, worldPos);
	}

	[Server]
	public void RemoveArtifact()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void QuestManager::RemoveArtifact()' called when server was not active");
		}
		else if (currentArtifact != null)
		{
			RpcInvokeClearArtifact(currentArtifact);
			Network_003CcurrentArtifact_003Ek__BackingField = null;
		}
	}

	[ClientRpc]
	private void RpcInvokePickUpArtifact(string artifactName, DewPlayer player, Vector3 worldPos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(artifactName);
		writer.WriteNetworkBehaviour(player);
		writer.WriteVector3(worldPos);
		SendRPCInternal("System.Void QuestManager::RpcInvokePickUpArtifact(System.String,DewPlayer,UnityEngine.Vector3)", 433620941, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeClearArtifact(string previous)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(previous);
		SendRPCInternal("System.Void QuestManager::RpcInvokeClearArtifact(System.String)", 1120690416, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	private void AddToUndiscoveredArtifacts(string[] list)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002EString_005B_005D(writer, list);
		SendCommandInternal("System.Void QuestManager::AddToUndiscoveredArtifacts(System.String[])", 1830451372, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	public void DiscoverArtifactAndShowStory(string artifact)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(artifact);
		SendRPCInternal("System.Void QuestManager::DiscoverArtifactAndShowStory(System.String)", -509047786, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public QuestManager()
	{
		InitSyncObject(_activeQuests);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeQuestStarted__DewQuest(DewQuest quest)
	{
		if (!quest.IsVisibleLocally())
		{
			return;
		}
		try
		{
			ClientEvent_OnQuestStarted?.Invoke(quest);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeQuestStarted__DewQuest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeQuestStarted called on server.");
		}
		else
		{
			((QuestManager)obj).UserCode_RpcInvokeQuestStarted__DewQuest(reader.ReadNetworkBehaviour<DewQuest>());
		}
	}

	protected void UserCode_RpcInvokeQuestComplete__DewQuest(DewQuest quest)
	{
		try
		{
			ClientEvent_OnQuestCompleted?.Invoke(quest);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeQuestComplete__DewQuest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeQuestComplete called on server.");
		}
		else
		{
			((QuestManager)obj).UserCode_RpcInvokeQuestComplete__DewQuest(reader.ReadNetworkBehaviour<DewQuest>());
		}
	}

	protected void UserCode_RpcInvokeQuestFail__DewQuest__QuestFailReason(DewQuest quest, QuestFailReason reason)
	{
		ClientEvent_OnQuestFailed?.Invoke(quest, reason);
	}

	protected static void InvokeUserCode_RpcInvokeQuestFail__DewQuest__QuestFailReason(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeQuestFail called on server.");
		}
		else
		{
			((QuestManager)obj).UserCode_RpcInvokeQuestFail__DewQuest__QuestFailReason(reader.ReadNetworkBehaviour<DewQuest>(), GeneratedNetworkCode._Read_QuestFailReason(reader));
		}
	}

	protected void UserCode_RpcInvokeQuestRemoved__DewQuest(DewQuest quest)
	{
		try
		{
			ClientEvent_OnQuestRemoved?.Invoke(quest);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeQuestRemoved__DewQuest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeQuestRemoved called on server.");
		}
		else
		{
			((QuestManager)obj).UserCode_RpcInvokeQuestRemoved__DewQuest(reader.ReadNetworkBehaviour<DewQuest>());
		}
	}

	protected void UserCode_RpcInvokePickUpArtifact__String__DewPlayer__Vector3(string artifactName, DewPlayer player, Vector3 worldPos)
	{
		ClientEvent_OnArtifactPickedUp?.Invoke(artifactName, player, worldPos);
	}

	protected static void InvokeUserCode_RpcInvokePickUpArtifact__String__DewPlayer__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokePickUpArtifact called on server.");
		}
		else
		{
			((QuestManager)obj).UserCode_RpcInvokePickUpArtifact__String__DewPlayer__Vector3(reader.ReadString(), reader.ReadNetworkBehaviour<DewPlayer>(), reader.ReadVector3());
		}
	}

	protected void UserCode_RpcInvokeClearArtifact__String(string previous)
	{
		Color c = (DewResources.GetByShortTypeName<Artifact>(previous).mainColor + Color.white) * 0.5f;
		c.a = 1f;
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			color = c,
			rawText = string.Format(DewLocalization.GetUIValue("InGame_Message_ArtifactRemoved"), DewLocalization.GetArtifactName(DewLocalization.GetArtifactKey(previous))),
			worldPosGetter = DewPlayer.local.hero.Visual.GetCenterPosition
		});
		try
		{
			ClientEvent_OnArtifactRemoved.Invoke(previous);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcInvokeClearArtifact__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeClearArtifact called on server.");
		}
		else
		{
			((QuestManager)obj).UserCode_RpcInvokeClearArtifact__String(reader.ReadString());
		}
	}

	protected void UserCode_AddToUndiscoveredArtifacts__String_005B_005D(string[] list)
	{
		foreach (string a in list)
		{
			if (Dew.IsArtifactIncludedInGame(a) && !undiscoveredArtifacts.Contains(a))
			{
				undiscoveredArtifacts.Add(a);
			}
		}
	}

	protected static void InvokeUserCode_AddToUndiscoveredArtifacts__String_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command AddToUndiscoveredArtifacts called on client.");
		}
		else
		{
			((QuestManager)obj).UserCode_AddToUndiscoveredArtifacts__String_005B_005D(GeneratedNetworkCode._Read_System_002EString_005B_005D(reader));
		}
	}

	protected void UserCode_DiscoverArtifactAndShowStory__String(string artifact)
	{
		if (base.isServer)
		{
			undiscoveredArtifacts.Remove(artifact);
		}
		ClientEvent_OnArtifactAppraised?.Invoke(artifact, DewSave.profile.artifacts[artifact].status != UnlockStatus.Complete);
		if (DewSave.profile.artifacts[artifact].status != UnlockStatus.Complete)
		{
			DewSave.profile.DiscoverArtifact(artifact);
			DewSave.SaveProfile();
		}
	}

	protected static void InvokeUserCode_DiscoverArtifactAndShowStory__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC DiscoverArtifactAndShowStory called on server.");
		}
		else
		{
			((QuestManager)obj).UserCode_DiscoverArtifactAndShowStory__String(reader.ReadString());
		}
	}

	static QuestManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(QuestManager), "System.Void QuestManager::AddToUndiscoveredArtifacts(System.String[])", InvokeUserCode_AddToUndiscoveredArtifacts__String_005B_005D, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(QuestManager), "System.Void QuestManager::RpcInvokeQuestStarted(DewQuest)", InvokeUserCode_RpcInvokeQuestStarted__DewQuest);
		RemoteProcedureCalls.RegisterRpc(typeof(QuestManager), "System.Void QuestManager::RpcInvokeQuestComplete(DewQuest)", InvokeUserCode_RpcInvokeQuestComplete__DewQuest);
		RemoteProcedureCalls.RegisterRpc(typeof(QuestManager), "System.Void QuestManager::RpcInvokeQuestFail(DewQuest,QuestFailReason)", InvokeUserCode_RpcInvokeQuestFail__DewQuest__QuestFailReason);
		RemoteProcedureCalls.RegisterRpc(typeof(QuestManager), "System.Void QuestManager::RpcInvokeQuestRemoved(DewQuest)", InvokeUserCode_RpcInvokeQuestRemoved__DewQuest);
		RemoteProcedureCalls.RegisterRpc(typeof(QuestManager), "System.Void QuestManager::RpcInvokePickUpArtifact(System.String,DewPlayer,UnityEngine.Vector3)", InvokeUserCode_RpcInvokePickUpArtifact__String__DewPlayer__Vector3);
		RemoteProcedureCalls.RegisterRpc(typeof(QuestManager), "System.Void QuestManager::RpcInvokeClearArtifact(System.String)", InvokeUserCode_RpcInvokeClearArtifact__String);
		RemoteProcedureCalls.RegisterRpc(typeof(QuestManager), "System.Void QuestManager::DiscoverArtifactAndShowStory(System.String)", InvokeUserCode_DiscoverArtifactAndShowStory__String);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteString(currentArtifact);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteString(currentArtifact);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref currentArtifact, null, reader.ReadString());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref currentArtifact, null, reader.ReadString());
		}
	}
}
