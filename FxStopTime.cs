using System.Collections;
using Mirror;
using UnityEngine;

public class FxStopTime : MonoBehaviour, IEffectComponent
{
	public float delay;

	public float duration = 0.3f;

	public bool isPlaying { get; private set; }

	public void Play()
	{
		if (NetworkServer.active)
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			isPlaying = true;
			yield return new WaitForSeconds(delay);
			NetworkedManagerBase<TimescaleManager>.instance.ChangeTimescale(0.1f, duration);
			yield return new WaitForSecondsRealtime(duration);
			isPlaying = false;
		}
	}

	public void Stop()
	{
		StopAllCoroutines();
		isPlaying = false;
	}
}
