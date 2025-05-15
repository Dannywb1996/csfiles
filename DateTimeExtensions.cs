using System;

public static class DateTimeExtensions
{
	public static long ToTimestamp(this DateTime dt)
	{
		return ((DateTimeOffset)dt).ToUnixTimeSeconds();
	}

	public static DateTime ToDateTime(this long timestamp)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
	}

	public static TimeSpan ToTimeSpan(this long seconds)
	{
		return TimeSpan.FromSeconds(seconds);
	}
}
