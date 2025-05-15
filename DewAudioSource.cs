using System;
using System.Collections;
using UnityEngine;

public class DewAudioSource : MonoBehaviour, IEffectComponent, IEffectWithOwnerContext
{
	public const float PitchChangeByTimescaleMultiplier = 0.75f;

	public AudioType type = AudioType.SFX;

	public AudioSpaceType space;

	public DewAudioRollOffType rolloff;

	public DewAudioClip clip;

	public AudioClip rawClip;

	public bool isLoop;

	public bool stopWhenEffectStop;

	public bool forceHighestPriority;

	public float fadeOutTime = 0.1f;

	public float delay;

	public float volumeMultiplier = 1f;

	public float pitchMultiplier = 1f;

	public bool playOnEnable;

	[SerializeField]
	[HideInInspector]
	private AudioSource _audioSource;

	private Coroutine _delayedPlay;

	private Coroutine _ongoingFade;

	private bool _didSetup;

	private float _pitch;

	public bool isPlaying
	{
		get
		{
			if (_audioSource == null)
			{
				return false;
			}
			if (!_audioSource.isPlaying && _delayedPlay == null)
			{
				return _ongoingFade != null;
			}
			return true;
		}
	}

	private void OnEnable()
	{
		if (playOnEnable)
		{
			Play();
		}
	}

	void IEffectComponent.Play()
	{
		Play();
	}

	void IEffectComponent.Stop()
	{
		if (stopWhenEffectStop || isLoop)
		{
			Stop();
		}
	}

	private void Update()
	{
		if (_audioSource != null && isPlaying && type != AudioType.UI && NetworkedManagerBase<TimescaleManager>.softInstance != null)
		{
			float desired = _pitch;
			desired *= NetworkedManagerBase<TimescaleManager>.softInstance.effectTimescale * 0.75f + 0.25f;
			if (Math.Abs(_audioSource.pitch - desired) > 0.001f)
			{
				float speed = ((_audioSource.pitch < desired) ? 0.75f : 8f);
				_audioSource.pitch = Mathf.MoveTowards(_audioSource.pitch, desired, Time.unscaledDeltaTime * speed);
			}
		}
	}

	public void Play(float fadeInTime = 0f)
	{
		if (!base.enabled || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (clip == null && rawClip == null)
		{
			Debug.LogWarning("Clip not set in DewAudioSource '" + base.name + "'", this);
			return;
		}
		if (fadeInTime < 0f)
		{
			fadeInTime = 0f;
		}
		if (_audioSource == null)
		{
			CreateAudioSource();
		}
		_audioSource.loop = isLoop;
		if (clip != null)
		{
			_audioSource.clip = clip.GetAudioClip();
			_audioSource.pitch = clip.GetPitch() * pitchMultiplier;
			_audioSource.volume = clip.volume * volumeMultiplier;
		}
		else
		{
			_audioSource.clip = rawClip;
			_audioSource.pitch = pitchMultiplier;
			_audioSource.volume = volumeMultiplier;
		}
		_pitch = _audioSource.pitch;
		if (!_didSetup || AudioManager.NeedsSetupEverytime(type))
		{
			_didSetup = true;
			ManagerBase<AudioManager>.instance.SetupAudioSource(_audioSource, type, space, rolloff, forceHighestPriority);
		}
		if (_ongoingFade != null)
		{
			StopCoroutine(_ongoingFade);
			_ongoingFade = null;
		}
		if (fadeInTime > 0f)
		{
			_ongoingFade = StartCoroutine(CoroutineFade(delay, 0f, clip.volume * volumeMultiplier, fadeInTime, stopAfterDone: false));
		}
		if (_delayedPlay != null)
		{
			StopCoroutine(_delayedPlay);
			_delayedPlay = null;
		}
		if (delay > 0f)
		{
			_delayedPlay = StartCoroutine(CoroutineDelayedPlay(delay));
		}
		else
		{
			_audioSource.Play();
		}
	}

	public void Stop()
	{
		Stop(fadeOutTime);
	}

	public void Stop(float customFadeOutTime)
	{
		if (base.isActiveAndEnabled)
		{
			if (_audioSource == null)
			{
				CreateAudioSource();
			}
			if (_delayedPlay != null)
			{
				StopCoroutine(_delayedPlay);
				_delayedPlay = null;
			}
			if (_ongoingFade != null)
			{
				StopCoroutine(_ongoingFade);
				_ongoingFade = null;
			}
			if (customFadeOutTime > 0f)
			{
				_ongoingFade = StartCoroutine(CoroutineFade(0f, _audioSource.volume, 0f, customFadeOutTime, stopAfterDone: true));
			}
			else
			{
				_audioSource.Stop();
			}
		}
	}

	private void CreateAudioSource()
	{
		_audioSource = base.gameObject.AddComponent<AudioSource>();
		_audioSource.playOnAwake = false;
		_audioSource.Stop();
	}

	private IEnumerator CoroutineDelayedPlay(float delay)
	{
		yield return new WaitForSeconds(delay);
		_delayedPlay = null;
		_audioSource.Play();
	}

	private IEnumerator CoroutineFade(float delay, float from, float to, float duration, bool stopAfterDone)
	{
		_audioSource.volume = from;
		yield return new WaitForSeconds(delay);
		if (duration <= 0f)
		{
			_audioSource.volume = to;
			yield break;
		}
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			_audioSource.volume = Mathf.Lerp(from, to, t / duration);
			yield return null;
		}
		_audioSource.volume = to;
		if (stopAfterDone)
		{
			_audioSource.Stop();
		}
		_ongoingFade = null;
	}

	public void SetOwnerContext(EffectOwnerContext context)
	{
		if (type != AudioType.UI && type != AudioType.Music)
		{
			switch (context)
			{
			case EffectOwnerContext.None:
				type = AudioType.GameOthers;
				break;
			case EffectOwnerContext.Self:
				type = AudioType.GameSelf;
				break;
			case EffectOwnerContext.Boss:
				type = AudioType.GameBoss;
				break;
			case EffectOwnerContext.OtherPlayers:
				type = AudioType.GameOtherPlayers;
				break;
			case EffectOwnerContext.Others:
				type = AudioType.GameOthers;
				break;
			default:
				throw new ArgumentOutOfRangeException("context", context, null);
			}
		}
	}
}
