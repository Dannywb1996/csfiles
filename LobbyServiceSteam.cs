using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyServiceSteam : LobbyServiceProvider
{
	private LobbyInstanceSteam _currentLobby;

	private List<CSteamID> _lobbyMembers = new List<CSteamID>();

	private CallResult<LobbyCreated_t> _lobbyCreated;

	private CallResult<LobbyEnter_t> _lobbyEnter;

	private CallResult<LobbyMatchList_t> _lobbyMatchList;

	private Callback<LobbyDataUpdate_t> _lobbyDataUpdate;

	private Callback<LobbyChatMsg_t> _lobbyChatMsg;

	private Callback<LobbyChatUpdate_t> _lobbyChatUpdate;

	private Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequested;

	private byte[] _receivedMsgBuffer = new byte[1024];

	private byte[] _sentMsgBuffer = new byte[1024];

	public override LobbyInstance currentLobby => _currentLobby;

	public IReadOnlyList<CSteamID> lobbyMembers => _lobbyMembers;

	private async void Start()
	{
		if (DewBuildProfile.current.platform == PlatformType.STEAM)
		{
			CreateCallbacks();
		}
	}

	private UniTask EnsureReady()
	{
		return UniTask.WaitUntil(() => SteamManagerBase.Initialized).Timeout(TimeSpan.FromSeconds(5.0), DelayType.UnscaledDeltaTime);
	}

	public override async UniTask CreateLobby()
	{
		await EnsureReady();
		if (_currentLobby != null)
		{
			await LeaveLobby();
		}
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingLobby);
		UniTaskCompletionSource<LobbyCreated_t> source = new UniTaskCompletionSource<LobbyCreated_t>();
		_lobbyCreated.Set(SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, GetInitialAttr_maxPlayers()), delegate(LobbyCreated_t t, bool failure)
		{
			if (t.m_eResult != EResult.k_EResultOK)
			{
				source.TrySetException(new SteamException("CreateLobby", t.m_eResult));
			}
			else if (failure)
			{
				source.TrySetException(new SteamException("CreateLobby"));
			}
			else
			{
				source.TrySetResult(t);
			}
		});
		LobbyCreated_t result = await source.Task.Timeout(TimeSpan.FromSeconds(20.0));
		_currentLobby = new LobbyInstanceSteam
		{
			_id = (CSteamID)result.m_ulSteamIDLobby,
			isLobbyLeader = true
		};
		await SetLobbyAttribute("version", GetInitialAttr_version());
		await SetLobbyAttribute("hostAddress", GetInitialAttr_hostAddress());
		await SetLobbyAttribute("hasGameStarted", GetInitialAttr_hasGameStarted());
		await SetLobbyAttribute("difficulty", GetInitialAttr_difficulty());
		await SetLobbyAttribute("maxPlayers", GetInitialAttr_maxPlayers());
		await SetLobbyAttribute("name", GetInitialAttr_name());
		await SetLobbyAttribute("isInviteOnly", GetInitialAttr_isInviteOnly());
		await SetLobbyAttribute("shortCode", GenerateShortCode());
		ApplyLobbyData((CSteamID)result.m_ulSteamIDLobby, ref _currentLobby);
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(_currentLobby._id);
		for (int num = 0; num < numLobbyMembers; num++)
		{
			AddLobbyMember(SteamMatchmaking.GetLobbyMemberByIndex(_currentLobby._id, num));
		}
		Debug.Log("Created lobby " + _currentLobby.id);
	}

	private string GenerateShortCode()
	{
		char[] array = new char[8];
		for (int i = 0; i < 8; i++)
		{
			int index = UnityEngine.Random.Range(0, "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".Length);
			array[i] = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"[index];
		}
		return new string(array);
	}

	public override async UniTask JoinLobby(object lobby)
	{
		string text = lobby.ToString();
		CSteamID lobbyId;
		if (ulong.TryParse(text, out var lobbyIdUint))
		{
			lobbyId = (CSteamID)lobbyIdUint;
		}
		else
		{
			if (text.Trim().Replace(" ", "").Length != 8)
			{
				throw new DewException(DewExceptionType.LobbyNotFound, "Code Invalid");
			}
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.GettingLobbyInformation);
			string text2 = text.Trim().Replace(" ", "").ToUpper();
			Debug.Log("JoinLobby(Steam) - Finding lobby with short code: " + text2);
			SteamMatchmaking.AddRequestLobbyListStringFilter("shortCode", text2, ELobbyComparison.k_ELobbyComparisonEqual);
			SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
			UniTaskCompletionSource<LobbyMatchList_t> s = new UniTaskCompletionSource<LobbyMatchList_t>();
			_lobbyMatchList.Set(SteamMatchmaking.RequestLobbyList(), delegate(LobbyMatchList_t t, bool failure)
			{
				if (failure)
				{
					s.TrySetException(new SteamException("GetLobbies"));
				}
				else
				{
					s.TrySetResult(t);
				}
			});
			if ((await s.Task.Timeout(TimeSpan.FromSeconds(20.0))).m_nLobbiesMatching == 0)
			{
				throw new DewException(DewExceptionType.LobbyNotFound);
			}
			lobbyId = SteamMatchmaking.GetLobbyByIndex(0);
		}
		Debug.Log("JoinLobby(Steam) - Joining lobby with ID: " + lobbyIdUint);
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToLobby);
		UniTaskCompletionSource<LobbyEnter_t> source = new UniTaskCompletionSource<LobbyEnter_t>();
		_lobbyEnter.Set(SteamMatchmaking.JoinLobby(lobbyId), delegate(LobbyEnter_t t, bool failure)
		{
			if (t.m_EChatRoomEnterResponse != 1)
			{
				source.TrySetException(new SteamException("JoinLobby", (EChatRoomEnterResponse)t.m_EChatRoomEnterResponse));
			}
			else if (failure)
			{
				source.TrySetException(new SteamException("JoinLobby"));
			}
			source.TrySetResult(t);
		});
		LobbyEnter_t lobbyEnter_t = await source.Task.Timeout(TimeSpan.FromSeconds(20.0));
		_currentLobby = new LobbyInstanceSteam
		{
			_id = new CSteamID(lobbyEnter_t.m_ulSteamIDLobby)
		};
		ApplyLobbyData(_currentLobby._id, ref _currentLobby);
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(_currentLobby._id);
		for (int num = 0; num < numLobbyMembers; num++)
		{
			AddLobbyMember(SteamMatchmaking.GetLobbyMemberByIndex(_currentLobby._id, num));
		}
		Debug.Log($"JoinLobby(Steam) - Joined {lobbyId}");
	}

	public override async UniTask LeaveLobby()
	{
		if (_currentLobby != null)
		{
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CleaningUpPreviousLobby);
			SteamMatchmaking.LeaveLobby(_currentLobby._id);
			Debug.Log($"Left lobby {_currentLobby._id}");
			_currentLobby = null;
			InvokeOnCurrentLobbyChanged();
		}
	}

	public override async UniTask HandleUserLeavingGame(string id)
	{
	}

	public override async UniTask GetLobbies(Action<LobbySearchResult> onUpdated, object continuationToken = null)
	{
		LobbySearchResult result = new LobbySearchResult();
		await Search(ELobbyDistanceFilter.k_ELobbyDistanceFilterClose, LobbyConnectionQuality.Best);
		onUpdated?.Invoke(result);
		await UniTask.WaitForSeconds(2f, ignoreTimeScale: true);
		await Search(ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault, LobbyConnectionQuality.Good);
		onUpdated?.Invoke(result);
		await UniTask.WaitForSeconds(2f, ignoreTimeScale: true);
		await Search(ELobbyDistanceFilter.k_ELobbyDistanceFilterFar, LobbyConnectionQuality.Okay);
		onUpdated?.Invoke(result);
		await UniTask.WaitForSeconds(2f, ignoreTimeScale: true);
		await Search(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide, LobbyConnectionQuality.Bad);
		onUpdated?.Invoke(result);
		async UniTask Search(ELobbyDistanceFilter dist, LobbyConnectionQuality quality)
		{
			SteamMatchmaking.AddRequestLobbyListStringFilter("version", Dew.GetCurrentMultiplayerCompatibilityVersion(), ELobbyComparison.k_ELobbyComparisonEqual);
			SteamMatchmaking.AddRequestLobbyListStringFilter("isInviteOnly", false.ToString(), ELobbyComparison.k_ELobbyComparisonEqual);
			SteamMatchmaking.AddRequestLobbyListStringFilter("hasGameStarted", false.ToString(), ELobbyComparison.k_ELobbyComparisonEqual);
			SteamMatchmaking.AddRequestLobbyListDistanceFilter(dist);
			UniTaskCompletionSource<LobbyMatchList_t> source = new UniTaskCompletionSource<LobbyMatchList_t>();
			_lobbyMatchList.Set(SteamMatchmaking.RequestLobbyList(), delegate(LobbyMatchList_t t, bool failure)
			{
				if (failure)
				{
					source.TrySetException(new SteamException("GetLobbies"));
				}
				else
				{
					source.TrySetResult(t);
				}
			});
			uint nLobbiesMatching = (await source.Task.Timeout(TimeSpan.FromSeconds(30.0))).m_nLobbiesMatching;
			List<LobbyInstanceSteam> list = new List<LobbyInstanceSteam>();
			for (int num = 0; num < nLobbiesMatching; num++)
			{
				try
				{
					CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(num);
					LobbyInstanceSteam data = new LobbyInstanceSteam();
					if (ApplyLobbyData(lobbyByIndex, ref data))
					{
						list.Add(data);
					}
				}
				catch (Exception)
				{
				}
			}
			foreach (LobbyInstanceSteam l in list)
			{
				if (result.lobbies.Find((LobbyInstance prev) => prev.id == l.id) == null)
				{
					l._connectionQuality = quality;
					result.lobbies.Add(l);
				}
			}
		}
	}

	public override async UniTask SetLobbyAttribute(string key, object value)
	{
		if (_currentLobby == null || !_currentLobby.isLobbyLeader || SteamMatchmaking.SetLobbyData(_currentLobby._id, key, value.ToString()))
		{
			return;
		}
		throw new SteamException("SetLobbyAttribute failed");
	}

	public void ActivateGameOverlayInviteDialog()
	{
		if (_currentLobby != null)
		{
			SteamFriends.ActivateGameOverlayInviteDialog(_currentLobby._id);
		}
	}

	public bool InviteToLobby(CSteamID friend)
	{
		if (_currentLobby == null)
		{
			return false;
		}
		return SteamMatchmaking.InviteUserToLobby(_currentLobby._id, friend);
	}

	public override string GetInitialAttr_hostAddress()
	{
		return SteamUser.GetSteamID().ToString();
	}

	public override async UniTask JoinGameOfCurrentLobby()
	{
		try
		{
			await UniTask.WaitWhile(() => _currentLobby != null && string.IsNullOrEmpty(_currentLobby.hostAddress)).Timeout(TimeSpan.FromSeconds(5.0), DelayType.Realtime);
		}
		catch (TimeoutException)
		{
			throw new DewException(DewExceptionType.LobbyTimeout);
		}
		await base.JoinGameOfCurrentLobby();
	}

	private void CreateCallbacks()
	{
		_lobbyCreated = CallResult<LobbyCreated_t>.Create();
		_lobbyEnter = CallResult<LobbyEnter_t>.Create();
		_lobbyMatchList = CallResult<LobbyMatchList_t>.Create();
		_lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(LobbyDataUpdateCallback);
		_lobbyChatMsg = Callback<LobbyChatMsg_t>.Create(LobbyChatMsgCallback);
		_lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(LobbyChatUpdateCallback);
		_gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(GameLobbyJoinRequestedCallback);
	}

	private void LobbyChatMsgCallback(LobbyChatMsg_t param)
	{
		try
		{
			if (currentLobby != null && !((CSteamID)param.m_ulSteamIDLobby != _currentLobby._id) && !((CSteamID)param.m_ulSteamIDUser != SteamMatchmaking.GetLobbyOwner(_currentLobby._id)))
			{
				CSteamID pSteamIDUser;
				EChatEntryType peChatEntryType;
				int lobbyChatEntry = SteamMatchmaking.GetLobbyChatEntry(_currentLobby._id, (int)param.m_iChatID, out pSteamIDUser, _receivedMsgBuffer, _receivedMsgBuffer.Length, out peChatEntryType);
				Span<byte> span = new Span<byte>(_receivedMsgBuffer, 1, lobbyChatEntry - 1);
				string payload = Encoding.Unicode.GetString(span);
				LobbyMessage lobbyMessage = new LobbyMessage
				{
					type = (LobbyMessageType)_receivedMsgBuffer[0],
					payload = payload
				};
				if (lobbyMessage.type != LobbyMessageType.GameServerAddressVersion)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (lobbyMessage.payload.Contains("|"))
				{
					string[] array = lobbyMessage.payload.Split("|");
					string hostAddress = array[0];
					string version = array[1];
					_currentLobby._hostAddress = hostAddress;
					_currentLobby._version = version;
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void LobbyChatUpdateCallback(LobbyChatUpdate_t param)
	{
		if (currentLobby != null && !((CSteamID)param.m_ulSteamIDLobby != _currentLobby._id))
		{
			CSteamID cSteamID = (CSteamID)param.m_ulSteamIDUserChanged;
			EChatMemberStateChange rgfChatMemberStateChange = (EChatMemberStateChange)param.m_rgfChatMemberStateChange;
			Debug.Log(string.Format("{0} {1} {2}", "LobbyChatUpdateCallback", rgfChatMemberStateChange, cSteamID));
			switch (rgfChatMemberStateChange)
			{
			case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
				AddLobbyMember(cSteamID);
				break;
			case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
				RemoveLobbyMember(cSteamID);
				break;
			case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
				RemoveLobbyMember(cSteamID);
				break;
			case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
				RemoveLobbyMember(cSteamID);
				break;
			case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
				RemoveLobbyMember(cSteamID);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void LobbyDataUpdateCallback(LobbyDataUpdate_t param)
	{
		CSteamID cSteamID = (CSteamID)param.m_ulSteamIDLobby;
		if (_currentLobby != null && cSteamID == _currentLobby._id)
		{
			ApplyLobbyData(cSteamID, ref _currentLobby);
		}
	}

	private bool ApplyLobbyData(CSteamID lobbyId, ref LobbyInstanceSteam data)
	{
		try
		{
			data._id = lobbyId;
			data._name = SteamMatchmaking.GetLobbyData(lobbyId, "name");
			data._difficulty = SteamMatchmaking.GetLobbyData(lobbyId, "difficulty");
			data._hasGameStarted = bool.Parse(SteamMatchmaking.GetLobbyData(lobbyId, "hasGameStarted"));
			data._currentPlayers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
			data._maxPlayers = Mathf.Clamp(SteamMatchmaking.GetLobbyMemberLimit(lobbyId), 0, 1024);
			data._version = SteamMatchmaking.GetLobbyData(lobbyId, "version");
			data._isInviteOnly = bool.Parse(SteamMatchmaking.GetLobbyData(lobbyId, "isInviteOnly"));
			data._shortCode = SteamMatchmaking.GetLobbyData(lobbyId, "shortCode");
			if (_currentLobby != null && _currentLobby._id == lobbyId)
			{
				InvokeOnCurrentLobbyChanged();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private void GameLobbyJoinRequestedCallback(GameLobbyJoinRequested_t param)
	{
		List<string> list = new List<string> { "Intro", "Title", "Collectables" };
		if (DewSave.profilePath != null)
		{
			if (list.Contains(SceneManager.GetActiveScene().name))
			{
				DewNetworkManager.networkMode = DewNetworkManager.Mode.MultiplayerJoinLobby;
				DewNetworkManager.joinTargetId = param.m_steamIDLobby.ToString();
				PlayLobbyManager.isFirstTimePlayFlow = false;
				DewNetworkManager.lanMode = false;
				ManagerBase<TransitionManager>.instance.TransitionToScene("PlayLobby");
			}
			else
			{
				ManagerBase<MessageManager>.instance.ShowMessageLocalized("Message_Steam_CannotAcceptInvitationInsideGame");
			}
		}
	}

	private void AddLobbyMember(CSteamID user)
	{
		if (!_lobbyMembers.Contains(user))
		{
			_lobbyMembers.Add(user);
		}
		if (currentLobby.isLobbyLeader)
		{
			BroadcastMessageToLobby(new LobbyMessage
			{
				type = LobbyMessageType.GameServerAddressVersion,
				payload = SteamUser.GetSteamID().ToString() + "|" + Dew.GetCurrentMultiplayerCompatibilityVersion()
			});
		}
	}

	private void BroadcastMessageToLobby(LobbyMessage message)
	{
		if (_currentLobby != null && _currentLobby.isLobbyLeader)
		{
			_sentMsgBuffer[0] = (byte)message.type;
			Span<byte> bytes = new Span<byte>(_sentMsgBuffer, 1, _sentMsgBuffer.Length - 1);
			int bytes2 = Encoding.Unicode.GetBytes(message.payload, bytes);
			SteamMatchmaking.SendLobbyChatMsg(_currentLobby._id, _sentMsgBuffer, bytes2 + 1);
		}
	}

	private void RemoveLobbyMember(CSteamID user)
	{
		if (_lobbyMembers.Contains(user))
		{
			_lobbyMembers.Remove(user);
		}
	}
}
