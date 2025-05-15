using System;

public class DewAudioSettings_Platform : ICloneable, IInitializableSettings
{
	public bool useAdvancedPrioritization = true;

	public void Initialize()
	{
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
