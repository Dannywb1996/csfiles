using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class PlayLobbyManager : NetworkedManagerBase<PlayLobbyManager>
{
	public static bool isFirstTimePlayFlow;

	public SafeAction<string> ClientEvent_OnLocalPlayerHeroChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnHasStartedChanged")]
	private bool _003ChasStarted_003Ek__BackingField;

	public Rift_RoomExit rift;

	public readonly SyncList<string> availableLucidDreams = new SyncList<string>();

	public SafeAction ClientEvent_OnAvailableLucidDreamsChanged;

	public bool isEveryoneReady => numOfReadyPlayers == numOfReadyPlayersMax;

	public int numOfReadyPlayers { get; private set; }

	public int numOfReadyPlayersMax { get; private set; }

	public bool hasStarted
	{
		[CompilerGenerated]
		get
		{
			return _003ChasStarted_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003ChasStarted_003Ek__BackingField = value;
		}
	}

	public bool Network_003ChasStarted_003Ek__BackingField
	{
		get
		{
			return hasStarted;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref hasStarted, 1uL, OnHasStartedChanged);
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void OnInit()
	{
		isFirstTimePlayFlow = false;
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		NetworkedManagerBase<GameSettingsManager>.instance.SetDifficulty(DewSave.profile.preferredDifficulty);
		NetworkedManagerBase<GameSettingsManager>.instance.ClearLucidDreams();
		if (DewSave.profile.preferredLucidDreams != null)
		{
			foreach (string t in DewSave.profile.preferredLucidDreams)
			{
				NetworkedManagerBase<GameSettingsManager>.instance.AddLucidDream(t);
			}
		}
		if (isFirstTimePlayFlow)
		{
			isFirstTimePlayFlow = false;
			DewPlayer.local.CmdSetHeroType("Hero_Lacerta");
			StartGame_Imp(immediately: true);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			DewSave.profile.didReadConstellationNotice = false;
		}
		DewResources.AddPreloadRule(this, delegate(PreloadInterface preload)
		{
			preload.KeepEverything();
			foreach (Type current in Dew.allHeroes)
			{
				if (Dew.IsHeroIncludedInGame(current.Name))
				{
					preload.AddType(current.Name);
				}
			}
		});
		DewResources.UnloadUnused();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		numOfReadyPlayers = 0;
		numOfReadyPlayersMax = 0;
		foreach (DewPlayer h in DewPlayer.humanPlayers)
		{
			if (!h.isHostPlayer)
			{
				numOfReadyPlayersMax++;
				if (h.isReady)
				{
					numOfReadyPlayers++;
				}
			}
		}
	}

	public void GoBack()
	{
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			defaultButton = DewMessageSettings.ButtonType.No,
			owner = this,
			rawContent = DewLocalization.GetUIValue("PlayLobby_QuitToMenuConfirm"),
			onClose = delegate(DewMessageSettings.ButtonType b)
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					DewNetworkManager.instance.EndSession();
				}
			}
		});
	}

	public void SetReady(bool ready)
	{
		if (NetworkServer.active)
		{
			throw new InvalidOperationException();
		}
		DewPlayer.local.CmdSetIsReady(ready);
	}

	public void ToggleReady()
	{
		if (NetworkServer.active)
		{
			throw new InvalidOperationException();
		}
		if (NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost > DewSave.profile.stardust)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				owner = this,
				rawContent = DewLocalization.GetUIValue("Dejavu_InsufficientStardust_Message"),
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
				onClose = delegate(DewMessageSettings.ButtonType b)
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						DewPlayer.local.CmdSetDejavuItem(null);
						NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost = 0;
						DewPlayer.local.CmdSetIsReady(!DewPlayer.local.isReady);
					}
				}
			});
		}
		else
		{
			DewPlayer.local.CmdSetIsReady(!DewPlayer.local.isReady);
		}
	}

	[ClientRpc]
	private void RpcPlayRiftAnimations()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void PlayLobbyManager::RpcPlayRiftAnimations()", 1063045727, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void StartGame_Imp(bool immediately)
	{
		if (!hasStarted && NetworkServer.active && isEveryoneReady)
		{
			DewNetworkManager.instance.StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			Network_003ChasStarted_003Ek__BackingField = true;
			DewSave.profile.preferredDifficulty = NetworkedManagerBase<GameSettingsManager>.instance.difficulty;
			if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
			{
				ManagerBase<LobbyManager>.instance.service.SetHasGameStarted(value: true);
			}
			if (!immediately)
			{
				RpcPlayRiftAnimations();
				yield return new WaitForSecondsRealtime(rift.chargeDuration + 0.65f);
			}
			DewNetworkManager.instance.SetLoadingStatus(isLoading: true);
			yield return new WaitForSecondsRealtime(1f);
			yield return DewNetworkManager.instance.LoadSceneAsync("PlayGame");
		}
	}

	public void StartGame()
	{
		if (NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost > DewSave.profile.stardust)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				owner = this,
				rawContent = DewLocalization.GetUIValue("Dejavu_InsufficientStardust_Message"),
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
				onClose = delegate(DewMessageSettings.ButtonType b)
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						DewPlayer.local.CmdSetDejavuItem(null);
						NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost = 0;
						StartGame_Imp(immediately: false);
					}
				}
			});
		}
		else
		{
			StartGame_Imp(immediately: false);
		}
	}

	private void OnHasStartedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			LobbyUIManager.instance.SetState("Started");
		}
		else if (LobbyUIManager.instance.IsState("Started"))
		{
			LobbyUIManager.instance.SetState("Lobby");
		}
	}

	[Server]
	public void UpdateAvailableLucidDreams()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayLobbyManager::UpdateAvailableLucidDreams()' called when server was not active");
			return;
		}
		availableLucidDreams.Clear();
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			foreach (string d in humanPlayer.lucidDreams)
			{
				if (!availableLucidDreams.Contains(d))
				{
					availableLucidDreams.Add(d);
				}
			}
		}
		RpcInvokeAvailableLucidDreamsChanged();
	}

	[ClientRpc]
	private void RpcInvokeAvailableLucidDreamsChanged()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void PlayLobbyManager::RpcInvokeAvailableLucidDreamsChanged()", 1398707723, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public PlayLobbyManager()
	{
		InitSyncObject(availableLucidDreams);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayRiftAnimations()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			DewEffect.Play(rift.fxActivate);
			DewEffect.Play(rift.fxCharge);
			yield return new WaitForSeconds(rift.chargeDuration);
			DewEffect.Stop(rift.fxCharge);
			DewEffect.Play(rift.fxOpen);
			DewEffect.Play(rift.fxLoop);
		}
	}

	protected static void InvokeUserCode_RpcPlayRiftAnimations(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayRiftAnimations called on server.");
		}
		else
		{
			((PlayLobbyManager)obj).UserCode_RpcPlayRiftAnimations();
		}
	}

	protected void UserCode_RpcInvokeAvailableLucidDreamsChanged()
	{
		Dew.CallDelayed(delegate
		{
			ClientEvent_OnAvailableLucidDreamsChanged?.Invoke();
		});
	}

	protected static void InvokeUserCode_RpcInvokeAvailableLucidDreamsChanged(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeAvailableLucidDreamsChanged called on server.");
		}
		else
		{
			((PlayLobbyManager)obj).UserCode_RpcInvokeAvailableLucidDreamsChanged();
		}
	}

	static PlayLobbyManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(PlayLobbyManager), "System.Void PlayLobbyManager::RpcPlayRiftAnimations()", InvokeUserCode_RpcPlayRiftAnimations);
		RemoteProcedureCalls.RegisterRpc(typeof(PlayLobbyManager), "System.Void PlayLobbyManager::RpcInvokeAvailableLucidDreamsChanged()", InvokeUserCode_RpcInvokeAvailableLucidDreamsChanged);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(hasStarted);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(hasStarted);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref hasStarted, OnHasStartedChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref hasStarted, OnHasStartedChanged, reader.ReadBool());
		}
	}
}
