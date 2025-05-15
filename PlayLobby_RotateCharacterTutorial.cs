using System;
using DG.Tweening;
using UnityEngine;

public class PlayLobby_RotateCharacterTutorial : MonoBehaviour
{
	public PlayLobby_Character[] lobbyCharacters;

	private void Start()
	{
		if (DewSave.profile.doneTutorials.Contains("PlayLobby_RotateCharacterTutorial"))
		{
			base.gameObject.SetActive(value: false);
		}
		LobbyUIManager instance = LobbyUIManager.instance;
		instance.onStateChanged = (Action<string, string>)Delegate.Combine(instance.onStateChanged, new Action<string, string>(OnStateChanged));
	}

	private void OnStateChanged(string arg1, string arg2)
	{
		base.transform.DOKill(complete: true);
		base.transform.localScale = Vector3.zero;
		DOTween.Sequence().AppendInterval(1f).Append(base.transform.DOScale(Vector3.one, 0.5f))
			.SetId(base.transform);
	}

	private void Update()
	{
		PlayLobby_Character myChar = lobbyCharacters.GetOrDefault(DewPlayer.humanPlayers.IndexOf(DewPlayer.local));
		if (!(myChar != null))
		{
			return;
		}
		base.transform.position = ManagerBase<DewCamera>.instance.mainCamera.WorldToScreenPoint(myChar.model.GetAbovePosition() * 0.35f + myChar.model.GetCenterPosition() * 0.65f);
		if (myChar.characterRotation.HasValue)
		{
			if (!DewSave.profile.doneTutorials.Contains("PlayLobby_RotateCharacterTutorial"))
			{
				DewSave.profile.doneTutorials.Add("PlayLobby_RotateCharacterTutorial");
			}
			base.gameObject.SetActive(value: false);
		}
	}
}
