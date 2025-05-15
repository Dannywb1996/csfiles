using UnityEngine;

[DewResourceLink(ResourceLinkBy.Guid)]
[CreateAssetMenu(fileName = "New Dew Audio Clip", menuName = "Dew Audio Clip")]
public class DewAudioClip : ScriptableObject, ILinkedByGuid
{
	public AudioClip[] clips;

	public float pitch = 1f;

	public bool randomizePitch;

	public float minPitch = 0.9f;

	public float maxPitch = 1.1f;

	public float volume = 0.45f;

	[field: HideInInspector]
	[field: SerializeField]
	public string resourceId { get; set; }

	public AudioClip GetAudioClip()
	{
		if (clips == null || clips.Length == 0)
		{
			Debug.LogWarning("DewAudioClip '" + base.name + "' has no clips configured.", this);
			return null;
		}
		return clips[Random.Range(0, clips.Length)];
	}

	public float GetPitch()
	{
		if (!randomizePitch)
		{
			return pitch;
		}
		return Random.Range(minPitch, maxPitch);
	}
}
