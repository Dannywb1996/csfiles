using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class UGSManager : ManagerBase<UGSManager>
{
	public override bool shouldRegisterUpdates => false;

	public ServiceStatus status { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		TryInit();
	}

	public async void TryInit()
	{
		if (UnityServices.State == ServicesInitializationState.Initialized)
		{
			status = ServiceStatus.Ready;
		}
		else
		{
			if (UnityServices.State == ServicesInitializationState.Initializing)
			{
				return;
			}
			status = ServiceStatus.Loading;
			try
			{
				InitializationOptions initializationOptions = new InitializationOptions();
				string env = "production";
				Debug.Log("Unity Services environment is: " + env);
				initializationOptions.SetEnvironmentName(env);
				await UnityServices.InitializeAsync(initializationOptions);
				if (GlobalAnalyticsManager.IsAnalyticsEnabled())
				{
					AnalyticsService.Instance.StartDataCollection();
				}
				RegisterHandlers();
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
				status = ServiceStatus.Ready;
			}
			catch (Exception message)
			{
				Debug.Log(message);
				status = ServiceStatus.Error;
			}
		}
	}

	private void RegisterHandlers()
	{
		AuthenticationService.Instance.SignedIn += delegate
		{
			Debug.Log("PlayerID: " + AuthenticationService.Instance.PlayerId);
		};
		AuthenticationService.Instance.SignInFailed += delegate(RequestFailedException err)
		{
			Debug.Log(err);
		};
		AuthenticationService.Instance.SignedOut += delegate
		{
			Debug.Log("Player signed out.");
		};
		AuthenticationService.Instance.Expired += delegate
		{
			Debug.Log("Player session could not be refreshed and expired.");
		};
	}

	public async UniTask EnsureReady()
	{
		if (status != ServiceStatus.Ready)
		{
			Debug.Log("Ensuring ready");
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForService);
			float startTime = Time.unscaledTime;
			await UniTask.WaitWhile(() => (Time.unscaledTime - startTime < 1f || status == ServiceStatus.Loading) && Time.unscaledTime - startTime < 5f);
			if (status == ServiceStatus.Loading)
			{
				throw new Exception("Unity Game Services Timeout");
			}
			if (status == ServiceStatus.Error)
			{
				throw new Exception("Unity Game Services Unavailable");
			}
			Debug.Log("UGS is ready");
		}
	}

	public async UniTask WaitUntilLoad(float timeout)
	{
		float startTime = Time.unscaledTime;
		await UniTask.WaitWhile(() => status == ServiceStatus.Loading && Time.unscaledTime - startTime < timeout);
	}

	public async UniTask WaitUntilLoad()
	{
		await WaitUntilLoad(5f);
	}
}
