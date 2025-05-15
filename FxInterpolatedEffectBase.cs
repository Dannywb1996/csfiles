using UnityEngine;

public abstract class FxInterpolatedEffectBase : MonoBehaviour, IEffectComponent, IEffectWithSpeed
{
	public bool isLoop = true;

	public float delay;

	public float startTime = 0.25f;

	public float sustainTime;

	public float decayTime = 0.25f;

	private float _currentLinearValue;

	private float _emitStartTime;

	private bool _didInit;

	private EaseFunction _easeFunction;

	public bool isPlaying
	{
		get
		{
			if (!isEmitting)
			{
				return currentValue > 0f;
			}
			return true;
		}
	}

	public float currentValue { get; set; }

	public bool isEmitting { get; private set; }

	protected virtual void OnInit()
	{
		_easeFunction = EasingFunction.GetEasingFunction(DewEase.EaseInOutSine);
	}

	private void InitIfDidnt()
	{
		if (!_didInit)
		{
			_didInit = true;
			OnInit();
		}
	}

	protected void Awake()
	{
		InitIfDidnt();
	}

	protected virtual void Start()
	{
		ValueSetter(currentValue);
	}

	public virtual void Play()
	{
		InitIfDidnt();
		if (isLoop)
		{
			sustainTime = float.PositiveInfinity;
		}
		isEmitting = true;
		_emitStartTime = Time.time;
		if (delay <= 0.0001f && startTime <= 0.0001f)
		{
			_currentLinearValue = 1f;
			currentValue = _easeFunction(0f, 1f, _currentLinearValue);
			ValueSetter(currentValue);
		}
	}

	public virtual void Stop()
	{
		InitIfDidnt();
		if (isPlaying)
		{
			isEmitting = false;
		}
	}

	protected virtual void Update()
	{
		if (isEmitting && Time.time - _emitStartTime > delay)
		{
			if (startTime <= 0.0001f)
			{
				_currentLinearValue = 1f;
			}
			else
			{
				_currentLinearValue = Mathf.MoveTowards(_currentLinearValue, 1f, 1f / startTime * Time.deltaTime);
			}
		}
		else if (decayTime <= 0f)
		{
			_currentLinearValue = 0f;
		}
		else
		{
			_currentLinearValue = Mathf.MoveTowards(_currentLinearValue, 0f, 1f / decayTime * Time.deltaTime);
		}
		if (isEmitting && Time.time - _emitStartTime > delay + startTime + sustainTime)
		{
			isEmitting = false;
		}
		float newValue = _easeFunction(0f, 1f, _currentLinearValue);
		if (currentValue != newValue)
		{
			currentValue = newValue;
			ValueSetter(currentValue);
		}
	}

	protected abstract void ValueSetter(float value);

	public void ApplySpeedMultiplier(float speed)
	{
		delay /= speed;
		startTime /= speed;
		sustainTime /= speed;
		decayTime /= speed;
	}
}
