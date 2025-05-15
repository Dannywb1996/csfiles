using Mirror;
using UnityEngine;

public class LobbyUIManager : UIManager
{
	public GameObject lobbyEffect;

	public new static LobbyUIManager instance => ManagerBase<UIManager>.instance as LobbyUIManager;

	public new static LobbyUIManager softInstance => ManagerBase<UIManager>.softInstance as LobbyUIManager;

	private void Start()
	{
		UpdateUIStatus();
		DewEffect.Play(lobbyEffect);
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 1, delegate
		{
			if (DewInput.currentMode != InputMode.Gamepad)
			{
				return false;
			}
			if (base.state != "Lobby")
			{
				return false;
			}
			NetworkedManagerBase<PlayLobbyManager>.instance.GoBack();
			return true;
		});
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		UpdateUIStatus();
	}

	private void UpdateUIStatus()
	{
		int num;
		if (NetworkServer.active || NetworkClient.active)
		{
			num = (NetworkClient.isConnecting ? 1 : 0);
			if (num == 0)
			{
				goto IL_0036;
			}
		}
		else
		{
			num = 1;
		}
		if (base.state != "Loading")
		{
			SetState("Loading");
		}
		goto IL_0036;
		IL_0036:
		if (num == 0 && base.state == "Loading")
		{
			SetState("Lobby");
		}
	}

	protected override void OnStateChanged(string oldState, string newState)
	{
		base.OnStateChanged(oldState, newState);
		if (newState == "Constellations" && oldState != "Constellations")
		{
			DewEffect.Stop(lobbyEffect);
		}
		else if (newState != "Constellations" && oldState == "Constellations")
		{
			DewEffect.Play(lobbyEffect);
		}
	}

	public void OpenCharacterSelection()
	{
		if (!(DewPlayer.local == null) && !DewPlayer.local.isReady)
		{
			SetState("Character");
		}
	}
}
