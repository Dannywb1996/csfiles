using System;
using UnityEngine;

public class LobbyManager : ManagerBase<LobbyManager>
{
	public Action onLobbyListUpdated;

	public Action onCurrentLobbyChanged;

	public override bool shouldRegisterUpdates => false;

	public LobbyServiceProvider service { get; private set; }

	public bool isLobbyLeader
	{
		get
		{
			if (service.currentLobby != null)
			{
				return service.currentLobby.isLobbyLeader;
			}
			return false;
		}
	}

	private void Start()
	{
		SwitchService();
	}

	public void SwitchService()
	{
		if (DewBuildProfile.current.platform == PlatformType.STEAM && DewBuildProfile.current.useSteamLobbyAndRelay)
		{
			SwitchService(GetComponent<LobbyServiceSteam>());
		}
		else
		{
			SwitchService(GetComponent<LobbyServiceEOS>());
		}
	}

	public void SwitchService(LobbyServiceProvider newProvider)
	{
		if (newProvider == service)
		{
			return;
		}
		if (newProvider == null)
		{
			Debug.LogWarning("Null provided for new lobby service provider");
			return;
		}
		LobbyServiceProvider oldService = service;
		if (oldService != null)
		{
			oldService.LeaveLobby();
		}
		service = newProvider;
		Debug.Log("Lobby server is now " + service.GetType().Name);
	}

	public static string GetDefaultLobbyName()
	{
		return string.Format(DewLocalization.GetUIValue("LobbyDefaultName"), DewSave.profile.name);
	}
}
