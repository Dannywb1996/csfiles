using System;
using System.Collections.Generic;
using MagicaCloth2;
using UnityEngine;
using UnityEngine.Rendering;

public class GlobalLogicPackage : ManagerBase<GlobalLogicPackage>
{
	private static List<Action> _callbacks = new List<Action>();

	[NonSerialized]
	public bool showFps;

	private GUIStyle _fpsStyle;

	private float _averageFps = 1f;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void AddGlobalLogicPackage()
	{
		if (!(UnityEngine.Object.FindObjectOfType<GlobalLogicPackage>() != null))
		{
			UnityEngine.Object.Instantiate(Resources.Load<GameObject>("GlobalLogicPackage"));
		}
	}

	public static void CallOnReady(Action callback)
	{
		if (ManagerBase<GlobalLogicPackage>.instance != null)
		{
			try
			{
				callback?.Invoke();
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
		_callbacks.Add(callback);
	}

	protected override void Awake()
	{
		base.Awake();
		MagicaManager.SetInitializationLocation(MagicaManager.InitializationLocation.Awake);
		Debug.Log($"== System Report START ==\r\nVersion: {Application.version}\r\nCPU: {SystemInfo.processorType}\r\nCPU Cores: {SystemInfo.processorCount}\r\nCPU Frequency: {SystemInfo.processorFrequency}MHz\r\n\r\nRAM: {SystemInfo.systemMemorySize}MB\r\n\r\nGraphics Device Name: {SystemInfo.graphicsDeviceName}\r\nGraphics Device Type: {SystemInfo.graphicsDeviceType}\r\nGraphics Memory: {SystemInfo.graphicsMemorySize}MB\r\nGraphics Device Version: {SystemInfo.graphicsDeviceVersion}\r\nGraphics Shader Level: {SystemInfo.graphicsShaderLevel}\r\n\r\nOperating System: {SystemInfo.operatingSystem}\r\nDevice Model: {SystemInfo.deviceModel}\r\n\r\nSupports Compute Shaders: {SystemInfo.supportsComputeShaders}\r\nSupports Instancing: {SystemInfo.supportsInstancing}\r\n== System Report END ==\r\n");
	}

	private void Start()
	{
		if (ManagerBase<GlobalLogicPackage>.instance != null && ManagerBase<GlobalLogicPackage>.instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		DebugManager.instance.enableRuntimeUI = false;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		foreach (Action c in _callbacks)
		{
			try
			{
				c?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_callbacks.Clear();
	}

	private void OnGUI()
	{
		if (showFps)
		{
			float e = 0.97f;
			if (!float.IsNormal(_averageFps))
			{
				_averageFps = 0f;
			}
			_averageFps = e * _averageFps + (1f - e) * 1f / Time.unscaledDeltaTime;
			if (_fpsStyle == null)
			{
				_fpsStyle = new GUIStyle
				{
					fontSize = 28,
					normal = new GUIStyleState
					{
						textColor = Color.green
					}
				};
			}
			GUILayout.Label($"{_averageFps:0} ({1f / Time.unscaledDeltaTime:0})", _fpsStyle);
		}
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Bye bye!");
	}
}
