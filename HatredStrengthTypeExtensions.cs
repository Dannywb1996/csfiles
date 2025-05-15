using System;

public static class HatredStrengthTypeExtensions
{
	public static int GetValueIndex(this HatredStrengthType s)
	{
		switch (s)
		{
		case HatredStrengthType.None:
		case HatredStrengthType.Mild:
			return 0;
		case HatredStrengthType.Potent:
			return 1;
		case HatredStrengthType.Powerful:
			return 2;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static T GetValue<T>(this T[] t, HatredStrengthType s)
	{
		return t.GetClamped(s.GetValueIndex());
	}
}
