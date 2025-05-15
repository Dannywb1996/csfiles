using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices.Lobby;
using Mirror;
using Mirror.FizzySteam;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

public class DewNetworkManager : NetworkManager
{
	private struct ChangeSceneMessage : NetworkMessage
	{
		public string name;
	}

	private struct SetLoadingStatusMessage : NetworkMessage
	{
		public bool isLoading;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct SessionEndedMessage : NetworkMessage
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct SessionRestartingMessage : NetworkMessage
	{
	}

	public enum Mode
	{
		Singleplayer,
		MultiplayerHost,
		MultiplayerJoinLobby,
		MultiplayerHostRestart,
		MultiplayerJoinRestart
	}

	public static Mode networkMode = Mode.Singleplayer;

	public static LobbyType lobbyType = LobbyType.Public;

	public static int maxPlayers = 4;

	public static string joinTargetId;

	public static bool lanMode;

	public static LobbyDetails joinTargetLobby;

	public Action<DewPlayer> onHumanPlayerAdd;

	public Action<DewPlayer> onHumanPlayerRemove;

	public Action<DewNetworkBehaviour> onDewNetworkBehaviourStart;

	public Action<DewNetworkBehaviour> onDewNetworkBehaviourStop;

	public Action<bool> onLoadingStatusChanged;

	public Action onSessionEnd;

	private bool _didInvokeSessionEnded;

	private bool _isThisManagerRestarting;

	private bool _didConnect;

	private bool _isEnding;

	public bool didRegisterError { get; internal set; }

	public bool isBeingKicked { get; internal set; }

	public static DewNetworkManager instance
	{
		get
		{
			if (softInstance == null)
			{
				softInstance = UnityEngine.Object.FindObjectOfType<DewNetworkManager>();
			}
			if (softInstance == null)
			{
				softInstance = UnityEngine.Object.FindObjectOfType<DewNetworkManager>(includeInactive: true);
			}
			return softInstance;
		}
	}

	public static DewNetworkManager softInstance { get; private set; }

	public override void Awake()
	{
		if (lanMode)
		{
			transport = GetComponent<TelepathyTransport>();
		}
		else if (DewBuildProfile.current.platform == PlatformType.STEAM && DewBuildProfile.current.useSteamLobbyAndRelay)
		{
			transport = GetComponent<FizzySteamworks>();
		}
		if (!(instance != null) || !(instance != this))
		{
			softInstance = this;
			base.Awake();
			NetworkClient.RegisterHandler<ChangeSceneMessage>(HandleRoomChangedMessage);
			NetworkClient.RegisterHandler<SetLoadingStatusMessage>(HandleSetLoadingStatusMessage);
			NetworkClient.RegisterHandler<SessionEndedMessage>(HandleGameEndedMessage);
			NetworkClient.RegisterHandler<SessionRestartingMessage>(HandleGameRestartingMessage);
			InGameAnalyticsManager.RegisterHandlers();
			DewEffect.RegisterHandlers();
		}
	}

	private void InvokeSessionEndedIfDidnt()
	{
		if (_didInvokeSessionEnded)
		{
			return;
		}
		_didInvokeSessionEnded = true;
		try
		{
			onSessionEnd?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void HandleGameEndedMessage(SessionEndedMessage obj)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			InvokeSessionEndedIfDidnt();
			ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
			yield return new WaitForSecondsRealtime(1f);
			NetworkServer.Shutdown();
			NetworkClient.Shutdown();
			if (ManagerBase<GameLogicPackage>.instance != null)
			{
				UnityEngine.Object.Destroy(ManagerBase<GameLogicPackage>.instance.gameObject);
			}
			UnityEngine.Object.Destroy(ManagerBase<NetworkLogicPackage>.instance.gameObject);
			SceneManager.LoadScene("Title");
			ManagerBase<TransitionManager>.instance.FadeIn();
		}
	}

	private void HandleSetLoadingStatusMessage(SetLoadingStatusMessage msg)
	{
		if (InGameUIManager.instance != null)
		{
			InGameUIManager.instance.SetState(msg.isLoading ? "Loading" : "Playing");
		}
		if (msg.isLoading)
		{
			ManagerBase<TransitionManager>.instance.FadeOut(showTips: true);
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
		}
		else
		{
			ManagerBase<TransitionManager>.instance.FadeIn();
		}
		try
		{
			onLoadingStatusChanged?.Invoke(msg.isLoading);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void HandleRoomChangedMessage(ChangeSceneMessage msg)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (NetworkServer.active)
			{
				NetworkClient.Ready();
			}
			else
			{
				NetworkClient.isLoadingScene = true;
				yield return SceneManager.LoadSceneAsync(msg.name, new LoadSceneParameters
				{
					loadSceneMode = LoadSceneMode.Single,
					localPhysicsMode = LocalPhysicsMode.None
				});
				yield return Resources.UnloadUnusedAssets();
				GarbageCollector.CollectIncremental(ulong.MaxValue);
				GC.Collect();
				NetworkClient.isLoadingScene = false;
				NetworkClient.PrepareToSpawnSceneObjects();
				if (!NetworkClient.ready)
				{
					NetworkClient.Ready();
				}
			}
		}
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		base.OnServerAddPlayer(conn);
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float startTime = Time.unscaledTime;
			yield return new WaitUntil(delegate
			{
				DewPlayer player = conn.GetPlayer();
				return Time.unscaledTime - startTime > 5f || (player != null && player.isPlayerNameSet);
			});
			if (!(conn.GetPlayer() == null))
			{
				NetworkedManagerBase<ChatManager>.instance.BroadcastChatMessage(new ChatManager.Message
				{
					type = ChatManager.MessageType.Notice,
					content = "Chat_Notice_PlayerJoinedGame",
					args = new string[1] { conn.GetPlayer().playerName }
				});
			}
		}
	}

	public override void OnServerDisconnect(NetworkConnectionToClient conn)
	{
		DewPlayer p = conn.GetPlayer();
		if (p == null)
		{
			return;
		}
		if (p.isKicked)
		{
			NetworkedManagerBase<ChatManager>.instance.BroadcastChatMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Notice_PlayerKickedFromGame",
				args = new string[1] { conn.GetPlayer().playerName }
			});
		}
		else
		{
			NetworkedManagerBase<ChatManager>.instance.BroadcastChatMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Notice_PlayerExitedGame",
				args = new string[1] { conn.GetPlayer().playerName }
			});
		}
		NetworkIdentity[] array = conn.owned.ToArray();
		foreach (NetworkIdentity o in array)
		{
			if (o.TryGetComponent<Actor>(out var actor))
			{
				actor.Destroy();
			}
			else
			{
				NetworkServer.Destroy(o.gameObject);
			}
		}
		conn.owned.Clear();
		if (!NetworkServer.dontListen && ManagerBase<LobbyManager>.instance.service.currentLobby != null)
		{
			ManagerBase<LobbyManager>.instance.service.HandleUserLeavingGame(conn.address);
		}
		base.OnServerDisconnect(conn);
	}

	public override void OnClientConnect()
	{
		base.OnClientConnect();
		_didConnect = true;
	}

	public override void OnClientDisconnect()
	{
		base.OnClientDisconnect();
		if (_isThisManagerRestarting)
		{
			return;
		}
		if (!didRegisterError && (NetworkedManagerBase<GameManager>.instance == null || !NetworkedManagerBase<GameManager>.instance.isGameConcluded) && !isBeingKicked)
		{
			if (_didConnect)
			{
				DewSessionError.ShowError(new DewException(DewExceptionType.Disconnected));
			}
			else
			{
				DewSessionError.ShowError();
			}
			didRegisterError = true;
		}
		InvokeSessionEndedIfDidnt();
		Dew.CallDelayed(delegate
		{
			DewSave.SaveProfile();
			DewSave.SavePlatformSettings();
			ReturnToTitleImmediately();
		});
	}

	protected bool ShouldFadeIn()
	{
		if (!(NetworkedManagerBase<PlayLobbyManager>.instance == null))
		{
			return !PlayLobbyManager.isFirstTimePlayFlow;
		}
		return true;
	}

	public override void OnStartHost()
	{
		base.OnStartHost();
		if (networkMode == Mode.MultiplayerHostRestart && ManagerBase<LobbyManager>.instance.isLobbyLeader)
		{
			ManagerBase<LobbyManager>.instance.service.SetHasGameStarted(value: false);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		NetworkedManagerBase<ConsoleManager>.instance.ExecuteAutoExec(ConsoleManager.AutoExecKey.Network);
		NetworkedManagerBase<ConsoleManager>.instance.ExecuteAutoExec(NetworkServer.active ? ConsoleManager.AutoExecKey.NetworkServer : ConsoleManager.AutoExecKey.NetworkClient);
	}

	private void StartLobbyTimeoutTimer()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			for (int i = 0; i < 10; i++)
			{
				yield return new WaitForSeconds(1f);
				if (NetworkServer.active || NetworkClient.active)
				{
					yield break;
				}
			}
			didRegisterError = true;
			DewSessionError.ShowError(new DewException(DewExceptionType.LobbyTimeout));
			ReturnToTitleImmediately();
		}
	}

	public override void Start()
	{
		if (!(instance != this))
		{
			base.Start();
			if (!NetworkServer.active && !NetworkClient.active)
			{
				StartGame();
				LobbyManager lobbyManager = ManagerBase<LobbyManager>.instance;
				lobbyManager.onCurrentLobbyChanged = (Action)Delegate.Combine(lobbyManager.onCurrentLobbyChanged, new Action(OnCurrentLobbyChanged));
			}
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (ManagerBase<LobbyManager>.instance != null)
		{
			LobbyManager lobbyManager = ManagerBase<LobbyManager>.instance;
			lobbyManager.onCurrentLobbyChanged = (Action)Delegate.Remove(lobbyManager.onCurrentLobbyChanged, new Action(OnCurrentLobbyChanged));
			if (!_isThisManagerRestarting && networkMode != Mode.Singleplayer && !lanMode)
			{
				ManagerBase<LobbyManager>.instance.service.LeaveLobby();
			}
		}
	}

	private async void OnCurrentLobbyChanged()
	{
		if (NetworkServer.active && !NetworkServer.dontListen && ManagerBase<LobbyManager>.instance.service.currentLobby == null)
		{
			for (int i = 0; i < 3; i++)
			{
				Debug.Log($"Lobby deleted mid-session; creating a new one... (Try {i + 1})");
				try
				{
					await ManagerBase<LobbyManager>.instance.service.CreateLobby();
					foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
					{
						humanPlayer.TpcMakePlayerChangeLobby(ManagerBase<LobbyManager>.instance.service.currentLobby.id);
					}
					return;
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			Debug.LogWarning("Mid-session create lobby failed.");
		}
		if (!NetworkServer.active && NetworkClient.active && ManagerBase<LobbyManager>.instance.service.currentLobby == null && DewPlayer.local != null)
		{
			Debug.Log("Disconnected from lobby mid-session; requesting to join...");
			DewPlayer.local.CmdRequestToJoinCurrentLobby();
		}
	}

	public override void OnServerConnect(NetworkConnectionToClient conn)
	{
		base.OnServerConnect(conn);
	}

	private void ReturnToTitleImmediately()
	{
		NetworkServer.Shutdown();
		NetworkClient.Shutdown();
		if (ManagerBase<GameLogicPackage>.instance != null)
		{
			UnityEngine.Object.Destroy(ManagerBase<GameLogicPackage>.instance.gameObject);
		}
		if (ManagerBase<NetworkLogicPackage>.instance != null)
		{
			UnityEngine.Object.Destroy(ManagerBase<NetworkLogicPackage>.instance.gameObject);
		}
		if (SceneManager.GetActiveScene().name != "Title")
		{
			SceneManager.LoadScene("Title");
		}
	}

	public void EndSession()
	{
		if (!_isEnding)
		{
			_isEnding = true;
			if (NetworkServer.active)
			{
				NetworkServer.SendToAll(default(SessionEndedMessage));
			}
			else
			{
				HandleGameEndedMessage(default(SessionEndedMessage));
			}
		}
	}

	public IEnumerator LoadSceneAsync(string sceneName)
	{
		NetworkServer.SetAllClientsNotReady();
		NetworkServer.isLoadingScene = true;
		yield return null;
		NetworkServer.SendToAll(new ChangeSceneMessage
		{
			name = sceneName
		});
		yield return SceneManager.LoadSceneAsync(sceneName, new LoadSceneParameters
		{
			loadSceneMode = LoadSceneMode.Single,
			localPhysicsMode = LocalPhysicsMode.None
		});
		yield return Resources.UnloadUnusedAssets();
		GarbageCollector.CollectIncremental(ulong.MaxValue);
		GC.Collect();
		NetworkServer.SpawnObjects();
		NetworkServer.isLoadingScene = false;
	}

	public void SetLoadingStatus(bool isLoading)
	{
		if (NetworkServer.active)
		{
			NetworkServer.SendToAll(new SetLoadingStatusMessage
			{
				isLoading = isLoading
			});
		}
	}

	[Server]
	public void RestartSession()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewNetworkManager::RestartSession()' called when server was not active");
		}
		else if (!_isEnding)
		{
			_isEnding = true;
			NetworkServer.SendToAll(default(SessionRestartingMessage));
		}
	}

	private void HandleGameRestartingMessage(SessionRestartingMessage obj)
	{
		RestartAsync();
	}

	private async UniTaskVoid RestartAsync()
	{
		DewSave.SaveProfile();
		DewSave.SavePlatformSettings();
		switch (networkMode)
		{
		case Mode.Singleplayer:
			networkMode = Mode.Singleplayer;
			break;
		case Mode.MultiplayerHost:
		case Mode.MultiplayerHostRestart:
			networkMode = Mode.MultiplayerHostRestart;
			break;
		case Mode.MultiplayerJoinLobby:
		case Mode.MultiplayerJoinRestart:
			networkMode = Mode.MultiplayerJoinRestart;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_isThisManagerRestarting = true;
		ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
		await UniTask.WaitForSeconds(1f, ignoreTimeScale: true);
		NetworkServer.Shutdown();
		NetworkClient.Shutdown();
		await UniTask.WaitForSeconds(0.25f, ignoreTimeScale: true);
		if (ManagerBase<GameLogicPackage>.instance != null)
		{
			UnityEngine.Object.Destroy(ManagerBase<GameLogicPackage>.instance.gameObject);
		}
		UnityEngine.Object.Destroy(ManagerBase<NetworkLogicPackage>.instance.gameObject);
		await UniTask.WaitForSeconds(0.25f, ignoreTimeScale: true);
		SceneManager.LoadScene("PlayLobby");
		ManagerBase<TransitionManager>.instance.FadeIn();
	}

	protected virtual async UniTaskVoid StartGame()
	{
		Debug.Log(string.Format("{0} - StartGame - {1}", "DewNetworkManager", networkMode));
		try
		{
			switch (networkMode)
			{
			case Mode.Singleplayer:
				NetworkServer.dontListen = true;
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingGame);
				StartHost();
				break;
			case Mode.MultiplayerHost:
				NetworkServer.dontListen = false;
				NetworkServer.maxConnections = maxPlayers;
				if (!lanMode)
				{
					Debug.Log("DewNetworkManager - Creating lobby");
					await ManagerBase<LobbyManager>.instance.service.CreateLobby();
					Debug.Log("DewNetworkManager - Starting host");
				}
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingGame);
				StartHost();
				break;
			case Mode.MultiplayerJoinLobby:
				if (lanMode)
				{
					Debug.Log("LAN mode, connecting to " + joinTargetId);
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToGame);
					networkAddress = joinTargetId;
					StartClient();
				}
				else
				{
					await ManagerBase<LobbyManager>.instance.service.JoinLobby(joinTargetId);
					await ManagerBase<LobbyManager>.instance.service.JoinGameOfCurrentLobby();
				}
				break;
			case Mode.MultiplayerHostRestart:
				if (!ManagerBase<LobbyManager>.instance.isLobbyLeader)
				{
					throw new DewException(DewExceptionType.LobbyNotFound, "Cannot restart");
				}
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingGame);
				StartHost();
				break;
			case Mode.MultiplayerJoinRestart:
			{
				if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
				{
					throw new DewException(DewExceptionType.LobbyNotFound, "Cannot wait for restart");
				}
				float startTime = Time.unscaledTime;
				float timeout = 10f;
				Debug.Log(string.Format("{0} - Waiting for host, timeout: {1:0.#} seconds", "DewNetworkManager", timeout));
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForHostPlayer);
				await UniTask.WaitWhile(() => ManagerBase<LobbyManager>.instance.service.currentLobby != null && ManagerBase<LobbyManager>.instance.service.currentLobby.hasGameStarted && Time.unscaledTime - startTime < timeout);
				if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
				{
					throw new DewException(DewExceptionType.Disconnected);
				}
				if (ManagerBase<LobbyManager>.instance.service.currentLobby.hasGameStarted)
				{
					throw new DewException(DewExceptionType.LobbyTimeout, "Host did not restart in time");
				}
				Debug.Log("DewNetworkManager - Joining the restarted game");
				networkAddress = ManagerBase<LobbyManager>.instance.service.currentLobby.hostAddress;
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToGame);
				StartClient();
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (ShouldFadeIn())
			{
				ManagerBase<TransitionManager>.instance.FadeIn();
			}
		}
		catch (Exception ex)
		{
			if (!(ex is SteamException) && !(ex is DewException) && !(ex is EOSResultException))
			{
				Debug.LogException(ex);
			}
			didRegisterError = true;
			DewSessionError.ShowError(ex);
			ReturnToTitleImmediately();
		}
	}
}
