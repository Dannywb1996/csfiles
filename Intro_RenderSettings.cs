using UnityEngine;

[ExecuteAlways]
public class Intro_RenderSettings : MonoBehaviour
{
	public Material skybox;

	[ColorUsage(false, true)]
	public Color ambientColor;

	public Color fogColor;

	public float fogStart;

	public float fogEnd;

	private void OnEnable()
	{
		SetSettings();
	}

	private void SetSettings()
	{
		RenderSettings.skybox = skybox;
		RenderSettings.ambientLight = ambientColor;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogStartDistance = fogStart;
		RenderSettings.fogEndDistance = fogEnd;
	}
}
