using UnityEngine;

public class FxSimulateParticleSystem : MonoBehaviour, IEffectComponent
{
	public float time;

	public bool isPlaying => false;

	public void Play()
	{
		if (TryGetComponent<ParticleSystem>(out var ps))
		{
			ps.Simulate(time);
			ps.Play();
		}
	}

	public void Stop()
	{
	}
}
