using System;

public class DewAudioSettings_User : ICloneable
{
	public float masterVolume = 0.8f;

	public float musicVolume = 0.8f;

	public float uiVolume = 1f;

	public float sfxVolume = 1f;

	public float envVolume = 0.8f;

	public BackgroundAudioBehavior backgroundAudioBehavior = BackgroundAudioBehavior.MuteMusic;

	public object Clone()
	{
		return MemberwiseClone();
	}
}
