using System.Collections;
using UnityEngine;

public class DewAudioSourceSetup : MonoBehaviour
{
	public AudioType type;

	public AudioSpaceType space;

	public DewAudioRollOffType rolloff;

	private bool _isDynamic => AudioManager.NeedsSetupEverytime(type);

	private bool _hasNoAudioSource => GetComponent<AudioSource>() == null;

	private void Start()
	{
		AudioSource comp = GetComponent<AudioSource>();
		if (comp == null)
		{
			Debug.LogWarning("AudioSourceSetup without AudioSource: " + base.transform.GetScenePath());
		}
		StartCoroutine(DelayedSetup());
		IEnumerator DelayedSetup()
		{
			yield return new WaitUntil(() => ManagerBase<AudioManager>.softInstance != null);
			ManagerBase<AudioManager>.instance.SetupAudioSource(comp, type, space, rolloff, forceCriticalPriority: false);
		}
	}
}
