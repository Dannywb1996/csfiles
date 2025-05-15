using System;

public class NetworkedOnScreenTimerHandle
{
	public string localeKey;

	public string skillKey;

	[NonSerialized]
	public Func<float> valueGetter;

	internal int _id;
}
