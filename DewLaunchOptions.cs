using System;
using System.Linq;
using UnityEngine;

public static class DewLaunchOptions
{
	public static bool forceKeyboardAndMouse;

	public static bool forceGamepad;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		forceKeyboardAndMouse = commandLineArgs.Contains("-forcepc");
		forceGamepad = commandLineArgs.Contains("-forcegamepad");
		Debug.Log("Launch options:");
		Debug.Log(string.Format("- {0}: {1}", "forceKeyboardAndMouse", forceKeyboardAndMouse));
		Debug.Log(string.Format("- {0}: {1}", "forceGamepad", forceGamepad));
	}
}
