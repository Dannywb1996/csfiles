using UnityEngine;
using UnityEngine.SceneManagement;

public class FPSManager : ManagerBase<FPSManager>, ISettingsChangedCallback
{
	public override bool shouldRegisterUpdates => false;

	private void Start()
	{
		UpdateFPSLimit();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
	}

	private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (!(this == null))
		{
			UpdateFPSLimit();
		}
	}

	public void UpdateFPSLimit()
	{
		if (DewSave.platformSettings.graphics.vSync)
		{
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = -1;
			return;
		}
		QualitySettings.vSyncCount = 0;
		switch (SceneManager.GetActiveScene().name)
		{
		case "Intro":
		case "Title":
		case "PlayLobby":
		case "Collectables":
			SetMenuFPS();
			return;
		}
		if (InGameUIManager.instance != null && InGameUIManager.instance.IsState("Menu"))
		{
			SetMenuFPS();
		}
		else
		{
			SetGameFPS();
		}
	}

	private void SetMenuFPS()
	{
		Application.targetFrameRate = DewSave.platformSettings.graphics.menuFrameLimit;
	}

	private void SetGameFPS()
	{
		Application.targetFrameRate = DewSave.platformSettings.graphics.gameFrameLimit;
	}

	public void OnSettingsChanged()
	{
		UpdateFPSLimit();
	}
}
