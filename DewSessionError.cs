using System;
using UnityEngine;

public class DewSessionError
{
	public object[] args;

	public static DewSessionError Error()
	{
		return new DewSessionError();
	}

	public static DewSessionError Error(Exception e)
	{
		DewSessionError dewSessionError = new DewSessionError();
		dewSessionError.args = new object[1] { e };
		return dewSessionError;
	}

	public static void ShowError(bool isFatal = false, bool isGame = true)
	{
		Error().Show(isFatal, isGame: true);
	}

	public static void ShowError(Exception e, bool isFatal = false, bool isGame = true)
	{
		Error(e).Show(isFatal, isGame);
	}

	private string GetTextContent(bool isGame)
	{
		DewNetworkManager.Mode networkMode = DewNetworkManager.networkMode;
		bool isHostOrSingleplayer = networkMode == DewNetworkManager.Mode.Singleplayer || networkMode == DewNetworkManager.Mode.MultiplayerHost;
		if (args != null && args[0] is SteamException { errorCode: not null } se && DewLocalization.TryGetUIValue($"Title_Message_SteamException_{se.errorCode}", out var val))
		{
			return val;
		}
		if (args != null && args[0] is EOSResultException le && DewLocalization.TryGetUIValue($"Title_Message_EOSResultException_{le.result}", out var val2))
		{
			return val2;
		}
		if (args != null && args[0] is DewException de && DewLocalization.TryGetUIValue($"Title_Message_DewException_{de.type}", out var val3))
		{
			if (!string.IsNullOrEmpty(de.Message))
			{
				return val3 + "\n\n" + de.Message;
			}
			return val3;
		}
		if (args != null && args.Length != 0 && args[0] is Exception e)
		{
			string exceptionStr = e.ToString();
			if (exceptionStr.Contains("HTTP/1.1 429"))
			{
				return DewLocalization.GetUIValue("Title_Message_DewException_TooManyRequests");
			}
			if (e is TimeoutException || exceptionStr.Contains("Request timeout"))
			{
				return DewLocalization.GetUIValue("Title_Message_DewException_Timeout");
			}
			if (exceptionStr.Contains("Cannot resolve destination host"))
			{
				return DewLocalization.GetUIValue("Title_Message_DewException_CannotResolveDestinationHost");
			}
			Debug.LogException(e);
		}
		string txt = DewLocalization.GetUIValue(isHostOrSingleplayer ? "Title_Message_GameStartError" : "Title_Message_ConnectionFailed");
		if (!isGame)
		{
			txt = DewLocalization.GetUIValue("Title_Message_EOSResultException_UnexpectedError");
		}
		if (args != null)
		{
			txt += "\n\n";
			object[] array = args;
			for (int i = 0; i < array.Length; i++)
			{
				txt = txt + array[i]?.ToString() + "\n";
			}
		}
		return txt;
	}

	public void Show(bool isFatal, bool isGame)
	{
		string textContent = GetTextContent(isGame);
		GlobalLogicPackage.CallOnReady(delegate
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				rawContent = textContent,
				onClose = (isFatal ? ((Action<DewMessageSettings.ButtonType>)delegate
				{
					Dew.QuitApplication();
				}) : null)
			});
		});
	}
}
