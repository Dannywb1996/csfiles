using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using EpicTransport;
using Steamworks;
using UnityEngine;

public class SteamManager : SteamManagerBase
{
	public class Friends : IDisposable
	{
		internal Friends()
		{
		}

		public void Dispose()
		{
		}

		public CSteamID GetFriendLobby(CSteamID friend)
		{
			uint appId = SteamUtils.GetAppID().m_AppId;
			if (SteamFriends.GetFriendGamePlayed(friend, out var info) && info.m_gameID.m_GameID == appId && info.m_steamIDLobby.IsValid())
			{
				return info.m_steamIDLobby;
			}
			throw new SteamException("Friend not in lobby");
		}

		public List<CSteamID> GetFriendsInGame(bool mustNotBePlaying)
		{
			List<CSteamID> result = new List<CSteamID>();
			int numOfFriends = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
			uint appId = SteamUtils.GetAppID().m_AppId;
			for (int i = 0; i < numOfFriends; i++)
			{
				CSteamID f = SteamFriends.GetFriendByIndex(numOfFriends, EFriendFlags.k_EFriendFlagImmediate);
				if (SteamFriends.GetFriendGamePlayed(f, out var info) && info.m_gameID.m_GameID == appId && (!mustNotBePlaying || !info.m_steamIDLobby.IsValid()))
				{
					result.Add(f);
				}
			}
			return result;
		}
	}

	public class User : IDisposable
	{
		internal User()
		{
			EOSSDKComponent.Instance.connectInterfaceCredentialType = ExternalCredentialType.SteamSessionTicket;
		}

		public void Dispose()
		{
		}
	}

	public SafeAction<bool> onGameOverlayShownChanged;

	private Callback<GameOverlayActivated_t> _onGameOverlayShownChanged;

	private Callback<GetTicketForWebApiResponse_t> _onGetTicketForWebApiResponse;

	public User user;

	private Dictionary<uint, UniTaskCompletionSource<GetTicketForWebApiResponse_t>> _getTicketForWebApis = new Dictionary<uint, UniTaskCompletionSource<GetTicketForWebApiResponse_t>>();

	public new static SteamManager instance => ManagerBase<SteamManagerBase>.instance as SteamManager;

	public override bool shouldRegisterUpdates => false;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (user == null)
		{
			user = new User();
			_onGetTicketForWebApiResponse = Callback<GetTicketForWebApiResponse_t>.Create(OnGetTicketForWebApiResponse_t);
			_onGameOverlayShownChanged = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		}
	}

	public async UniTask<SteamTicketForWebApi> GetTicketForWebApi(string pchIdentity = "DewOwnership")
	{
		_ = 1;
		try
		{
			if (!SteamManagerBase.Initialized)
			{
				Debug.Log("GetTicketForWebApi: Waiting for Steam");
				await UniTask.WaitUntil(() => SteamManagerBase.Initialized).Timeout(TimeSpan.FromSeconds(8.0));
			}
			HAuthTicket hAuthTicket = SteamUser.GetAuthTicketForWebApi(pchIdentity);
			if (hAuthTicket.m_HAuthTicket == 0)
			{
				throw new DewException(DewExceptionType.SteamAuthGetTicketFailed);
			}
			UniTaskCompletionSource<GetTicketForWebApiResponse_t> compSource = new UniTaskCompletionSource<GetTicketForWebApiResponse_t>();
			_getTicketForWebApis.Add(hAuthTicket.m_HAuthTicket, compSource);
			Debug.Log("GetTicketForWebApi(" + pchIdentity + "): Getting ticket");
			GetTicketForWebApiResponse_t response = await compSource.Task.Timeout(TimeSpan.FromSeconds(8.0));
			if (response.m_eResult == EResult.k_EResultOK)
			{
				Utf8String ticket = Epic.OnlineServices.Common.ToString(response.m_rgubTicket);
				Debug.Log(string.Format("{0}({1}): Got ticket with length: {2}", "GetTicketForWebApi", pchIdentity, ticket.Length));
				return new SteamTicketForWebApi
				{
					handle = hAuthTicket,
					ticket = ticket
				};
			}
			throw new DewException(DewExceptionType.SteamAuthGetTicketFailed, response.m_eResult.ToString());
		}
		catch (TimeoutException)
		{
			throw new DewException(DewExceptionType.SteamAuthGetTicketFailed);
		}
	}

	private void OnGetTicketForWebApiResponse_t(GetTicketForWebApiResponse_t param)
	{
		if (_getTicketForWebApis.TryGetValue(param.m_hAuthTicket.m_HAuthTicket, out var source))
		{
			_getTicketForWebApis.Remove(param.m_hAuthTicket.m_HAuthTicket);
			if (param.m_eResult == EResult.k_EResultOK)
			{
				source.TrySetResult(param);
			}
			else
			{
				source.TrySetException(new SteamException("Could not get ticket for web api", param.m_eResult));
			}
		}
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t param)
	{
		onGameOverlayShownChanged?.Invoke(param.m_bActive != 0);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (user != null)
		{
			user.Dispose();
		}
	}
}
