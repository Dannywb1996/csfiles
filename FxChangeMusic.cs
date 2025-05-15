using UnityEngine;

public class FxChangeMusic : MonoBehaviour, IEffectComponent
{
	public DewMusicItem music;

	public bool isPlaying => false;

	public void Play()
	{
		ManagerBase<MusicManager>.instance.Play(music);
	}

	public void Stop()
	{
	}
}
