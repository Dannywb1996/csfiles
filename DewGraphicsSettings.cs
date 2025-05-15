using System;
using UnityEngine;

public class DewGraphicsSettings : ICloneable, IInitializableSettings
{
	public int menuFrameLimit = 60;

	public int gameFrameLimit = -1;

	public bool vSync;

	public int resolutionWidth = Screen.currentResolution.width;

	public int resolutionHeight = Screen.currentResolution.height;

	public FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;

	public int displayIndex;

	public QualityOff3Levels screenSpaceReflections = QualityOff3Levels.Medium;

	public Quality3Levels shaderQuality = Quality3Levels.High;

	public Quality3Levels terrainQuality = Quality3Levels.High;

	public Quality3Levels fogQuality = Quality3Levels.Medium;

	public Quality3Levels vegetationQuality = Quality3Levels.High;

	public QualityOff4Levels shadowQuality = QualityOff4Levels.High;

	public void Initialize()
	{
		if (SystemInfo.deviceName == "steamdeck")
		{
			screenSpaceReflections = QualityOff3Levels.Off;
			shadowQuality = QualityOff4Levels.Off;
			fogQuality = Quality3Levels.Low;
			gameFrameLimit = 60;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
