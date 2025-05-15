using System;
using Epic.OnlineServices;

public class EOSResultException : Exception
{
	public Result result;

	public EOSResultException(Result result)
		: base(result.ToString())
	{
		this.result = result;
	}
}
