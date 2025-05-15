using UnityEngine;

[ExecuteAlways]
public class DisableOcclusionTransparency : MonoBehaviour
{
	public static DisableOcclusionTransparency instance;

	private static readonly int Cached = Shader.PropertyToID("_DisableOcclusionTransparency");

	private void OnEnable()
	{
		instance = this;
		Shader.SetGlobalInt(Cached, 1);
	}

	private void OnDisable()
	{
		if (instance == this)
		{
			instance = null;
		}
		Shader.SetGlobalInt(Cached, 0);
	}
}
