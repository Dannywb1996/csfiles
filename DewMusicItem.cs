using UnityEngine;

[CreateAssetMenu(fileName = "mus", menuName = "Dew Music Item")]
public class DewMusicItem : ScriptableObject
{
	public AudioClip clip;

	public float delay = 1.5f;

	public float fadeInTime = 2f;

	public float volumeMultiplier = 1f;

	public float pitch = 1f;

	public float startTime;
}
