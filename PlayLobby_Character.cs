using System;
using System.Linq;
using UnityEngine;

[LogicUpdatePriority(2000)]
public class PlayLobby_Character : LogicBehaviour
{
	public int playerIndex;

	public string forceSetup = "";

	public CharacterModelDisplay model;

	public GameObject fxClick;

	public float? characterRotation;

	private float _cv;

	private void Start()
	{
		LobbyUIManager instance = LobbyUIManager.instance;
		instance.onStateChanged = (Action<string, string>)Delegate.Combine(instance.onStateChanged, new Action<string, string>(OnStateChanged));
	}

	private void OnStateChanged(string arg1, string arg2)
	{
		model.isFocused = LobbyUIManager.instance.IsState("Character") && IsLocalPlayer();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		string currentType = forceSetup;
		if (forceSetup == "" && playerIndex < DewPlayer.humanPlayers.Count && playerIndex >= 0)
		{
			currentType = DewPlayer.humanPlayers[playerIndex].selectedHeroType;
		}
		if (model.heroType != currentType)
		{
			model.heroType = currentType;
			characterRotation = null;
			model.transform.localRotation = Quaternion.identity;
		}
		if (playerIndex < DewPlayer.humanPlayers.Count && playerIndex >= 0 && !model.accessories.SequenceEqual(DewPlayer.humanPlayers[playerIndex].selectedAccessories))
		{
			model.accessories = DewPlayer.humanPlayers[playerIndex].selectedAccessories.ToList();
			model.UpdateAccessories();
		}
	}

	private void UpdateTransparency(bool isLocalPlayer)
	{
		bool shouldHide = LobbyUIManager.instance.IsState("Character") && !isLocalPlayer;
		model.opacity = Mathf.MoveTowards(model.opacity, shouldHide ? 0.05f : 1f, Time.deltaTime * 1.5f);
	}

	public void Click()
	{
		DewEffect.Play(fxClick);
		if (IsLocalPlayer() && !DewPlayer.local.isReady)
		{
			LobbyUIManager.instance.OpenCharacterSelection();
		}
	}

	public bool IsLocalPlayer()
	{
		if (playerIndex >= 0 && playerIndex < DewPlayer.humanPlayers.Count)
		{
			return DewPlayer.humanPlayers[playerIndex].isLocalPlayer;
		}
		return false;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		bool isLocalPlayer = IsLocalPlayer();
		UpdateTransparency(isLocalPlayer);
		if (characterRotation.HasValue)
		{
			model.transform.localRotation = Quaternion.Euler(0f, characterRotation.Value, 0f);
			_cv = 0f;
		}
		else
		{
			float angle = model.transform.localRotation.eulerAngles.y;
			angle = Mathf.SmoothDampAngle(angle, 0f, ref _cv, 0.4f);
			model.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
		}
	}
}
