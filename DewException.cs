using System;

public class DewException : Exception
{
	public DewExceptionType type { get; private set; }

	public DewException(DewExceptionType type, string extraMessage = "")
		: base(extraMessage)
	{
		this.type = type;
	}
}
