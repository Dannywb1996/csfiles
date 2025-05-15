using System;

public class DewGameplaySettings_Platform : ICloneable, IInitializableSettings
{
	public bool skipIntro;

	public bool skipPhotosensitivityWarning;

	public void Initialize()
	{
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
