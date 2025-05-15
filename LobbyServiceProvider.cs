using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EpicTransport;
using UnityEngine;

public abstract class LobbyServiceProvider : MonoBehaviour
{
	public bool isRefreshingLobby { get; private set; }

	public abstract LobbyInstance currentLobby { get; }

	public LobbySearchResult foundLobbies { get; set; } = new LobbySearchResult
	{
		continuationToken = null,
		lobbies = new List<LobbyInstance>()
	};

	public abstract UniTask CreateLobby();

	public abstract UniTask JoinLobby(object lobby);

	public abstract UniTask LeaveLobby();

	public abstract UniTask HandleUserLeavingGame(string id);

	public async UniTaskVoid RefreshLobbies(object continuationToken = null)
	{
		if (!isRefreshingLobby)
		{
			isRefreshingLobby = true;
			try
			{
				await GetLobbies(Notify, continuationToken);
			}
			catch (Exception e)
			{
				DewSessionError.ShowError(e);
			}
			isRefreshingLobby = false;
		}
		void Notify(LobbySearchResult res)
		{
			foundLobbies = res;
			try
			{
				ManagerBase<LobbyManager>.instance.onLobbyListUpdated?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void InvokeOnCurrentLobbyChanged()
	{
		try
		{
			ManagerBase<LobbyManager>.instance.onCurrentLobbyChanged?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public abstract UniTask GetLobbies(Action<LobbySearchResult> onUpdated, object continuationToken = null);

	public string GetInitialAttr_version()
	{
		return Dew.GetCurrentMultiplayerCompatibilityVersion();
	}

	public virtual string GetInitialAttr_hostAddress()
	{
		return EOSSDKComponent.LocalUserProductIdString;
	}

	public bool GetInitialAttr_hasGameStarted()
	{
		return NetworkedManagerBase<GameManager>.instance != null;
	}

	public string GetInitialAttr_difficulty()
	{
		if (!(NetworkedManagerBase<GameManager>.instance != null))
		{
			return "";
		}
		return NetworkedManagerBase<GameManager>.instance.difficulty.name;
	}

	public int GetInitialAttr_maxPlayers()
	{
		return AttrCustomizeResources.Config.maxPlayer;
	}

	public string GetInitialAttr_name()
	{
		if (!string.IsNullOrEmpty(DewSave.profile.preferredLobbyName))
		{
			return DewSave.profile.preferredLobbyName;
		}
		return LobbyManager.GetDefaultLobbyName();
	}

	public bool GetInitialAttr_isInviteOnly()
	{
		return DewNetworkManager.lobbyType != LobbyType.Public;
	}

	public virtual async UniTask JoinGameOfCurrentLobby()
	{
		if (currentLobby == null || string.IsNullOrEmpty(currentLobby.hostAddress))
		{
			await LeaveLobby();
			throw new DewException(DewExceptionType.UnknownLobby);
		}
		if (currentLobby.hasGameStarted)
		{
			await LeaveLobby();
			throw new DewException(DewExceptionType.GameAlreadyStarted);
		}
		if (currentLobby.version != Dew.GetCurrentMultiplayerCompatibilityVersion())
		{
			string hostVersion = (string.IsNullOrEmpty(currentLobby.version) ? "Unknown" : currentLobby.version);
			await LeaveLobby();
			throw new DewException(DewExceptionType.VersionMismatch, "Host " + hostVersion + " != Local " + Dew.GetCurrentMultiplayerCompatibilityVersion());
		}
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToGame);
		DewNetworkManager.instance.networkAddress = currentLobby.hostAddress;
		DewNetworkManager.instance.StartClient();
	}

	public abstract UniTask SetLobbyAttribute(string key, object value);

	public async UniTaskVoid SetDifficulty(string newDifficulty)
	{
		try
		{
			await SetLobbyAttribute("difficulty", newDifficulty);
		}
		catch (Exception e)
		{
			DewSessionError.ShowError(e);
		}
	}

	public async UniTaskVoid SetLobbyName(string newName)
	{
		try
		{
			await SetLobbyAttribute("name", newName);
		}
		catch (Exception e)
		{
			DewSessionError.ShowError(e);
		}
	}

	public async UniTaskVoid SetHasGameStarted(bool value)
	{
		try
		{
			await SetLobbyAttribute("hasGameStarted", value);
		}
		catch (Exception e)
		{
			DewSessionError.ShowError(e);
		}
	}
}
