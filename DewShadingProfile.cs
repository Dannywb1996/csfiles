using UnityEngine;

[CreateAssetMenu(fileName = "Shading", menuName = "Dew/Dew Shading Profile", order = 1)]
public class DewShadingProfile : ScriptableObject
{
	public float termAlpha = 0.05f;

	public float termBeta = 0.025f;

	public float termGamma = 0.25f;

	public Texture lightRamp;

	public Texture directionalLightRamp;
}
