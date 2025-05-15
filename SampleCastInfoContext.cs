using System;

public struct SampleCastInfoContext
{
	public enum CastOnButtonType
	{
		None,
		ByButton,
		ByButtonRelease,
		ByButtonPress
	}

	public CastMethodData castMethod;

	public AbilityTargetValidator targetValidator;

	public AbilityTrigger trigger;

	public bool showCastIndicator;

	public bool isInitialInfoSet;

	public CastInfo currentInfo;

	public CastOnButtonType castOnButton;

	public float angleSpeedLimit;

	[NonSerialized]
	public DewInputTrigger castKey;

	[NonSerialized]
	public Action<CastInfo> castCallback;

	[NonSerialized]
	public Action<CastInfo> updateCallback;

	[NonSerialized]
	public Action cancelCallback;
}
