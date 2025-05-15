using System;
using System.Runtime.InteropServices;

public static class SI
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct DoImmediately
	{
	}

	public struct WaitForSeconds
	{
		internal float _seconds;

		public WaitForSeconds(float seconds)
		{
			_seconds = seconds;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct WaitForNextLogicUpdate
	{
	}

	public struct WaitForCondition
	{
		internal Func<bool> _condition;

		public WaitForCondition(Func<bool> condition)
		{
			_condition = condition;
		}
	}
}
