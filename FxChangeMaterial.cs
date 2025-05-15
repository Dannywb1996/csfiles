using UnityEngine;

public class FxChangeMaterial : MonoBehaviour, IEffectComponent
{
	public Renderer[] renderers;

	public Material[] materials;

	public bool isPlaying => false;

	public void Play()
	{
		Renderer[] array = renderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sharedMaterials = materials;
		}
	}

	public void Stop()
	{
	}
}
