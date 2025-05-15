using System.Collections;
using UnityEngine;

public class MusicManager : ManagerBase<MusicManager>
{
	public float fadeOutTime;

	public float volumeMultiplier;

	internal AudioSource _source;

	private float _currentNormalizedVolume;

	private DewMusicItem _next;

	private DewMusicItem _current;

	public bool isPlaying => _source.isPlaying;

	protected override void Awake()
	{
		base.Awake();
		_source = GetComponent<AudioSource>();
	}

	private void Start()
	{
		ManagerBase<AudioManager>.instance.SetupAudioSource(_source, AudioType.Music, AudioSpaceType.Global, DewAudioRollOffType.Unity, forceCriticalPriority: false);
	}

	public void Play(DewMusicItem music)
	{
		_next = music;
	}

	public void Stop()
	{
		_next = null;
	}

	public void Pause()
	{
		_source.Pause();
	}

	public void UnPause()
	{
		_source.UnPause();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (_current != _next)
		{
			_currentNormalizedVolume = Mathf.MoveTowards(_currentNormalizedVolume, 0f, 1f / fadeOutTime * dt);
			if (_currentNormalizedVolume < 0.0001f)
			{
				_source.Stop();
				_source.clip = ((_next == null) ? null : _next.clip);
				_current = _next;
				StopAllCoroutines();
				if (_current != null)
				{
					StartCoroutine(DelayedPlay());
				}
			}
		}
		else if (isPlaying)
		{
			_currentNormalizedVolume = Mathf.MoveTowards(_currentNormalizedVolume, 1f, 1f / _next.fadeInTime * dt);
		}
		float vol = _currentNormalizedVolume * volumeMultiplier * ((_current == null) ? 1f : _current.volumeMultiplier);
		_source.volume = vol;
		_source.pitch = ((_current != null) ? _current.pitch : 1f);
		if (Time.timeScale < 0.0001f)
		{
			if (isPlaying)
			{
				Pause();
			}
		}
		else if (_source.clip != null && !isPlaying)
		{
			UnPause();
		}
		IEnumerator DelayedPlay()
		{
			yield return new WaitForSeconds(_current.delay);
			_source.Play();
			_source.time = _current.startTime;
		}
	}
}
