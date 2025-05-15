using System.Collections;
using UnityEngine;

public class PlayLobby_Tutorial : MonoBehaviour
{
	public RectTransform changeTraveler;

	public RectTransform difficultyBox;

	public RectTransform startButton;

	public RectTransform heroList;

	public RectTransform skillList;

	public RectTransform constellations;

	private void Start()
	{
		if (!PlayLobbyManager.isFirstTimePlayFlow)
		{
			if (!DewSave.profile.doneTutorials.Contains("PlayLobby_LobbyTutorial") || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				StartCoroutine(LobbyTutorialRoutine());
			}
			if (!DewSave.profile.doneTutorials.Contains("PlayLobby_TravelerSelection") || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				StartCoroutine(TravelerSelectionRoutine());
			}
		}
		IEnumerator LobbyTutorialRoutine()
		{
			yield return new WaitWhile(() => !LobbyUIManager.instance.IsState("Lobby"));
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = changeTraveler,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Lobby_ChangeTraveler"),
				textPlacement = TutorialHighlightTextPlacement.Top
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = difficultyBox,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Lobby_Difficulty"),
				textPlacement = TutorialHighlightTextPlacement.Top
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = startButton,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Lobby_StartIfReady"),
				textPlacement = TutorialHighlightTextPlacement.Top
			});
			if (!DewSave.profile.doneTutorials.Contains("PlayLobby_LobbyTutorial"))
			{
				DewSave.profile.doneTutorials.Add("PlayLobby_LobbyTutorial");
			}
		}
		IEnumerator TravelerSelectionRoutine()
		{
			yield return new WaitWhile(() => !LobbyUIManager.instance.IsState("Character"));
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = heroList,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Character_ChangeTraveler"),
				textPlacement = TutorialHighlightTextPlacement.Right
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = skillList,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Character_ChangeMemory"),
				textPlacement = TutorialHighlightTextPlacement.Right
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = constellations,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Character_Constellations"),
				textPlacement = TutorialHighlightTextPlacement.Left
			});
			if (!DewSave.profile.doneTutorials.Contains("PlayLobby_TravelerSelection"))
			{
				DewSave.profile.doneTutorials.Add("PlayLobby_TravelerSelection");
			}
		}
	}
}
