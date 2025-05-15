using UnityEngine;

public class DewPlatformSettings : IInitializableSettings
{
	public string lastProfilePath;

	public bool dontShowProfileMigration;

	public DewGraphicsSettings graphics = new DewGraphicsSettings();

	public DewAudioSettings_Platform audio = new DewAudioSettings_Platform();

	public DewGameplaySettings_Platform gameplay = new DewGameplaySettings_Platform();

	public void Initialize()
	{
		graphics.Initialize();
		audio.Initialize();
		gameplay.Initialize();
	}

	public void Validate()
	{
		if (graphics == null)
		{
			graphics = new DewGraphicsSettings();
		}
		if (audio == null)
		{
			audio = new DewAudioSettings_Platform();
		}
		if (gameplay == null)
		{
			gameplay = new DewGameplaySettings_Platform();
		}
		DewSave.ValidateEnumValues(graphics);
		DewSave.ValidateEnumValues(audio);
		DewSave.ValidateEnumValues(gameplay);
		if (graphics.resolutionWidth < 640 || graphics.resolutionHeight < 480)
		{
			Resolution curr = Screen.currentResolution;
			graphics.resolutionWidth = curr.width;
			graphics.resolutionHeight = curr.height;
		}
	}
}
