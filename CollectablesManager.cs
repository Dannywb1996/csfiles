using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectablesManager : ManagerBase<CollectablesManager>
{
	private bool _isInTransition;

	private void Start()
	{
		Time.timeScale = 1f;
		Resources.UnloadUnusedAssets();
		ManagerBase<TransitionManager>.instance.FadeIn();
	}

	public void GoBack()
	{
		if (DewInput.currentMode == InputMode.Gamepad)
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
						TransitionToScene("Title");
					}
				}
			});
		}
		else
		{
			TransitionToScene("Title");
		}
	}

	public void TransitionToScene(string sceneName)
	{
		if (!_isInTransition)
		{
			_isInTransition = true;
			DewSave.SaveProfile();
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
			yield return new WaitForSeconds(ManagerBase<TransitionManager>.instance.fadeTime);
			SceneManager.LoadScene(sceneName);
		}
	}

	public void ShowUnlockEverythingMessage()
	{
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			owner = this,
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.Cancel),
			defaultButton = DewMessageSettings.ButtonType.Cancel,
			rawContent = DewLocalization.GetUIValue("Collectables_UnlockEverything_ConfirmNotice"),
			destructiveConfirm = true,
			onClose = delegate(DewMessageSettings.ButtonType b)
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					UnlockEverything();
				}
			}
		});
	}

	private void UnlockEverything()
	{
		ConsoleCommands.AchCompleteAll();
		ConsoleCommands.DiscoverAll();
		TransitionToScene("Collectables");
		ManagerBase<MessageManager>.instance.ShowMessageLocalized("Collectables_UnlockEverything_Done");
	}
}
