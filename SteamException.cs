using System;

public class SteamException : Exception
{
	public object errorCode;

	public SteamException(string message, object errorCode = null)
		: base((errorCode == null) ? message : $"{message} ({errorCode})")
	{
		this.errorCode = errorCode;
	}
}
