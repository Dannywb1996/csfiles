using UnityEngine;

public class FxEffect : MonoBehaviour
{
	public bool playOnEnable;

	private void OnEnable()
	{
		if (playOnEnable)
		{
			Play();
		}
	}

	public void Play()
	{
		DewEffect.Play(base.gameObject);
	}

	public void Stop()
	{
		DewEffect.Stop(base.gameObject);
	}
}
