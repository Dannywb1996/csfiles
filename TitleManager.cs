using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TitleManager : ManagerBase<TitleManager>, ISettingsChangedCallback
{
	private static bool IsFirstTitleScreenInThisSession;

	public ParticleSystem riftParticleSystem;

	public GameObject onlineCamera;

	public GameObject playCamera;

	public GameObject collectablesCamera;

	public GameObject extrasCamera;

	public GameObject settingsCamera;

	public GameObject didPlayTutorialObject;

	public GameObject didNotPlayTutorialObject;

	public UnityEvent onShowLangSelectionView;

	public View langSelectionView;

	public View splashView;

	[NonSerialized]
	public bool didConvertSave;

	public bool isInTransition { get; set; }

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		IsFirstTitleScreenInThisSession = true;
	}

	private void Start()
	{
		AchievementManager.lastGamePlayReward = null;
		DewResources.AddPreloadRule(this, delegate(PreloadInterface preload)
		{
			foreach (Type current in Dew.allHeroes)
			{
				if (Dew.IsHeroIncludedInGame(current.Name))
				{
					preload.AddType(current.Name);
				}
			}
		});
		DewResources.UnloadUnused();
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			DewSave.profile.didPlayTutorial = false;
			DewSave.profile.preferredDifficulty = "diffNormal";
			DewSave.profile.preferredHero = "Hero_Lacerta";
			foreach (KeyValuePair<string, List<HeroLoadoutData>> pair in DewSave.profile.heroLoadouts)
			{
				for (int i = 0; i < pair.Value.Count; i++)
				{
					pair.Value[i] = new HeroLoadoutData();
				}
			}
		}
		OnSettingsChanged();
		if (riftParticleSystem != null)
		{
			riftParticleSystem.Simulate(2f);
			riftParticleSystem.Play();
		}
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			ManagerBase<TransitionManager>.instance.FadeIn();
			Time.timeScale = 1f;
			Resources.UnloadUnusedAssets();
			ManagerBase<UIManager>.instance.SetState("Waiting");
			yield return new WaitForSecondsRealtime(0.5f);
			if (ManagerBase<UGSManager>.instance.status == ServiceStatus.Error)
			{
				ManagerBase<UGSManager>.instance.TryInit();
			}
			if (ManagerBase<EOSManager>.instance.status == ServiceStatus.Error)
			{
				ManagerBase<EOSManager>.instance.TryInit();
			}
			if (DewSave.profilePath == null)
			{
				List<DewProfileItem> profiles = DewSave.GetNormalProfiles();
				if (profiles.Count == 1)
				{
					DewSave.LoadProfile(profiles[0].path);
				}
				else if (profiles.Count == 0)
				{
					onShowLangSelectionView.Invoke();
					yield return new WaitWhile(() => langSelectionView.isShowing);
				}
			}
			if (IsFirstTitleScreenInThisSession)
			{
				splashView.Show();
				yield return new WaitWhile(() => splashView.isShowing);
			}
			List<DewProfileItem> allProfiles = DewSave.GetProfiles();
			if (IsFirstTitleScreenInThisSession && allProfiles.FindIndex((DewProfileItem p) => p.state == DewProfileState.Convertible) != -1 && !DewSave.platformSettings.dontShowProfileMigration)
			{
				ManagerBase<MessageManager>.instance.ShowMessageLocalized("Title_Profile_Message_HasConvertibleProfile");
				didConvertSave = false;
				ManagerBase<UIManager>.instance.SetState("ProfileSelection");
				StartCoroutine(Routine2());
			}
			else if (DewSave.profile != null && DewSave.profilePath != null)
			{
				ManagerBase<UIManager>.instance.SetState("Title");
			}
			else
			{
				ManagerBase<UIManager>.instance.SetState("ProfileSelection");
			}
		}
		IEnumerator Routine2()
		{
			yield return new WaitWhile(() => ManagerBase<UIManager>.instance.state == "ProfileSelection");
			if (!didConvertSave)
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
					rawContent = DewLocalization.GetUIValue("Title_Profile_Message_IgnoreConvertibleProfile"),
					defaultButton = DewMessageSettings.ButtonType.No,
					onClose = delegate(DewMessageSettings.ButtonType b)
					{
						if (b == DewMessageSettings.ButtonType.Yes)
						{
							ManagerBase<MessageManager>.instance.ShowMessageLocalized("Title_Profile_Message_IgnoreConvertibleProfile_Confirmed");
							DewSave.platformSettings.dontShowProfileMigration = true;
						}
					}
				});
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		settingsCamera.SetActive(ManagerBase<UIManager>.instance.IsState("Settings"));
		didPlayTutorialObject.SetActive(DewSave.profile.didPlayTutorial);
		didNotPlayTutorialObject.SetActive(!DewSave.profile.didPlayTutorial);
	}

	private void OnDestroy()
	{
		IsFirstTitleScreenInThisSession = false;
	}

	public void JoinLobby(string lobbyId)
	{
		if (!isInTransition && (ManagerBase<UIManager>.instance.IsState("Title") || ManagerBase<UIManager>.instance.IsState("FindLobby")))
		{
			DewNetworkManager.networkMode = DewNetworkManager.Mode.MultiplayerJoinLobby;
			DewNetworkManager.joinTargetId = lobbyId;
			PlayLobbyManager.isFirstTimePlayFlow = false;
			DewNetworkManager.lanMode = false;
			TransitionToScene("PlayLobby");
		}
	}

	public void CheckTutorial(Action callback)
	{
		if (DewSave.profile.didPlayTutorial)
		{
			callback?.Invoke();
			return;
		}
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			defaultButton = DewMessageSettings.ButtonType.No,
			rawContent = DewLocalization.GetUIValue("Title_Message_TutorialWarning_Ask"),
			onClose = delegate(DewMessageSettings.ButtonType res)
			{
				if (res == DewMessageSettings.ButtonType.Yes)
				{
					DewSave.profile.didPlayTutorial = true;
					ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
					{
						buttons = DewMessageSettings.ButtonType.Ok,
						rawContent = DewLocalization.GetUIValue("Title_Message_TutorialWarning_Confirm"),
						onClose = delegate
						{
							callback?.Invoke();
						}
					});
				}
			}
		});
	}

	public void EnterSingleplayer()
	{
		if (!isInTransition)
		{
			CheckTutorial(delegate
			{
				DewNetworkManager.networkMode = DewNetworkManager.Mode.Singleplayer;
				PlayLobbyManager.isFirstTimePlayFlow = false;
				DewNetworkManager.lanMode = false;
				TransitionToScene("PlayLobby");
			});
		}
	}

	public void EnterTutorial()
	{
		if (!isInTransition)
		{
			TransitionToScene("PlayTutorial");
		}
	}

	public void EnterCollectables()
	{
		if (!isInTransition)
		{
			TransitionToScene("Collectables");
		}
	}

	public void CreatePublicLobby()
	{
		if (!isInTransition)
		{
			CheckTutorial(delegate
			{
				DewNetworkManager.networkMode = DewNetworkManager.Mode.MultiplayerHost;
				DewNetworkManager.lobbyType = LobbyType.Public;
				PlayLobbyManager.isFirstTimePlayFlow = false;
				DewNetworkManager.lanMode = false;
				TransitionToScene("PlayLobby");
			});
		}
	}

	public void CreateFriendsOnlyLobby()
	{
	}

	public void CreateInviteOnlyLobby()
	{
		if (!isInTransition)
		{
			CheckTutorial(delegate
			{
				DewNetworkManager.networkMode = DewNetworkManager.Mode.MultiplayerHost;
				DewNetworkManager.lobbyType = LobbyType.InviteOnly;
				PlayLobbyManager.isFirstTimePlayFlow = false;
				DewNetworkManager.lanMode = false;
				TransitionToScene("PlayLobby");
			});
		}
	}

	public void TransitionToScene(string sceneName)
	{
		if (!isInTransition)
		{
			isInTransition = true;
			if (sceneName == "Collectables")
			{
				collectablesCamera.SetActive(value: true);
			}
			else if (sceneName == "PlayLobby" || sceneName == "PlayTutorial")
			{
				playCamera.SetActive(value: true);
			}
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return ManagerBase<TransitionManager>.instance.FadeOutRoutine(showTips: false);
			SceneManager.LoadScene(sceneName);
		}
	}

	public void ExitToDesktop()
	{
		DewSave.SaveProfile();
		DewSave.SavePlatformSettings();
		Application.Quit();
	}

	public void OnSettingsChanged()
	{
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			return;
		}
		foreach (KeyValuePair<string, DewProfile.StarData> star in DewSave.profile.stars)
		{
			star.Value.level = 0;
		}
		DewSave.profile.stardust = DewBuildProfile.current.defaultStardustAmount;
	}
}
