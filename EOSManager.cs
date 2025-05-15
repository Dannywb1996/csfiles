using System;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using EpicTransport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EOSManager : ManagerBase<EOSManager>
{
	public static bool ResetEOSOnTitle = true;

	public override bool shouldRegisterUpdates => false;

	public ServiceStatus status { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		ResetEOSOnTitle = true;
	}

	private void Start()
	{
		if (DewBuildProfile.current.platform != PlatformType.STEAM || !DewBuildProfile.current.useSteamLobbyAndRelay)
		{
			TryInit();
			SceneManager.activeSceneChanged += SceneManagerOnactiveSceneChanged;
		}
	}

	public async void TryInit()
	{
		if (EOSSDKComponent.Initialized)
		{
			status = ServiceStatus.Ready;
			return;
		}
		status = ServiceStatus.Loading;
		try
		{
			if (DewBuildProfile.current.platform == PlatformType.STEAM)
			{
				await UniTask.WaitWhile(() => !SteamManagerBase.Initialized);
				EOSSDKComponent.SetConnectInterfaceCredentialToken((await SteamManager.instance.GetTicketForWebApi("epiconlineservices")).ticket);
				EOSSDKComponent.Instance.connectInterfaceCredentialType = ExternalCredentialType.SteamSessionTicket;
			}
			else
			{
				EOSSDKComponent.Instance.connectInterfaceCredentialType = ExternalCredentialType.DeviceidAccessToken;
			}
			Debug.Log("Initializing EOS with " + EOSSDKComponent.Instance.connectInterfaceCredentialType);
			EOSSDKComponent.Initialize();
			await UniTask.WaitWhile(() => EOSSDKComponent.Instance != null && (!EOSSDKComponent.Initialized || EOSSDKComponent.IsConnecting));
			if (EOSSDKComponent.Instance == null || !EOSSDKComponent.Initialized)
			{
				status = ServiceStatus.Error;
			}
			else
			{
				status = ServiceStatus.Ready;
			}
		}
		catch (Exception exception)
		{
			status = ServiceStatus.Error;
			Debug.LogException(exception);
		}
	}

	private void OnDestroy()
	{
		SceneManager.activeSceneChanged -= SceneManagerOnactiveSceneChanged;
	}

	private void SceneManagerOnactiveSceneChanged(Scene from, Scene to)
	{
		if ((DewBuildProfile.current.platform != PlatformType.STEAM || !DewBuildProfile.current.useSteamLobbyAndRelay) && !(to.name != "Title") && ResetEOSOnTitle)
		{
			ResetEOS();
		}
	}

	public void ResetEOS()
	{
		status = ServiceStatus.Loading;
		Debug.Log("Destroying EOS...");
		UnityEngine.Object.DestroyImmediate(EOSSDKComponent.Instance.gameObject);
		Debug.Log("Creating EOS...");
		UnityEngine.Object.Instantiate(Resources.Load<GameObject>("EOSSDKComponent"), ManagerBase<GlobalLogicPackage>.instance.transform);
		Debug.Log("Done. Initializing...");
		TryInit();
	}
}
