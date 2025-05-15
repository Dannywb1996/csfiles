using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class GlobalAnalyticsManager : ManagerBase<GlobalAnalyticsManager>
{
	public override bool shouldRegisterUpdates => false;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Init()
	{
		if (IsAnalyticsEnabled())
		{
			Analytics.initializeOnStartup = true;
			Analytics.enabled = true;
			Analytics.deviceStatsEnabled = true;
			PerformanceReporting.enabled = true;
		}
		else
		{
			Analytics.initializeOnStartup = false;
			Analytics.enabled = false;
			Analytics.deviceStatsEnabled = false;
			PerformanceReporting.enabled = false;
			if (DewBuildProfile.current.disableAnalytics)
			{
				Debug.Log("Analytics disabled by build profile settings");
			}
		}
		SceneManager.activeSceneChanged += SceneManagerOnactiveSceneChanged;
	}

	public static bool IsAnalyticsEnabled()
	{
		float sampleRatio = 0.3f;
		float normalizedValue = (float)(SystemInfo.deviceUniqueIdentifier.GetHashCode() & 0x7FFFFFFF) / 2.1474836E+09f;
		if (!DewBuildProfile.current.disableAnalytics)
		{
			return normalizedValue <= sampleRatio;
		}
		return false;
	}

	private static async void SceneManagerOnactiveSceneChanged(Scene arg0, Scene arg1)
	{
		if (arg1.name != "Title")
		{
			return;
		}
		await UniTask.WaitForSeconds(1f, ignoreTimeScale: true);
		if (!(ManagerBase<GlobalAnalyticsManager>.instance == null))
		{
			ManagerBase<GlobalAnalyticsManager>.instance.SendUGSStats();
			if (DewBuildProfile.current.platform != PlatformType.STEAM || !DewBuildProfile.current.useSteamLobbyAndRelay)
			{
				ManagerBase<GlobalAnalyticsManager>.instance.SendEOSStats();
			}
		}
	}

	private async void SendUGSStats()
	{
		await UniTask.WaitWhile(() => ManagerBase<UGSManager>.instance.status == ServiceStatus.Loading);
		bool isSuccess = ManagerBase<UGSManager>.instance.status == ServiceStatus.Ready;
		InvokeEvent(new Event_SpecialUGSConnectivity
		{
			c_success = isSuccess
		});
		Debug.Log("UGS Connectivity: " + isSuccess);
	}

	private async void SendEOSStats()
	{
		await UniTask.WaitWhile(() => ManagerBase<EOSManager>.instance.status == ServiceStatus.Loading);
		bool isSuccess = ManagerBase<EOSManager>.instance.status == ServiceStatus.Ready;
		InvokeEvent(new Event_SpecialEOSConnectivity
		{
			c_success = isSuccess
		});
		Debug.Log("EOS Connectivity: " + isSuccess);
	}

	private static void InvokeEvent(Unity.Services.Analytics.Event ev)
	{
		try
		{
			AnalyticsService.Instance.RecordEvent(ev);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}
}
