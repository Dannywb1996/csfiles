using System.Collections;
using UnityEngine;

public class FxAnimatorSetTrigger : MonoBehaviour, IEffectComponent
{
	public float delay;

	public string trigger;

	public bool isPlaying => false;

	public void Play()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (delay > 0f)
			{
				yield return new WaitForSeconds(delay);
			}
			GetComponent<Animator>().SetTrigger(trigger);
		}
	}

	public void Stop()
	{
	}
}
