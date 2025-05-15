using UnityEngine;

public class FxEntityShake : FxInterpolatedEffectBase, IAttachableToEntity
{
	public float shakeIntensity = 0.1f;

	public float shakeInterval = 1f / 60f;

	public bool useUnscaledInterval;

	public bool onlyOnXZPlane = true;

	private EntityTransformModifier _modifier;

	private Entity _target;

	private float _lastShakeTime = float.NegativeInfinity;

	private void OnDestroy()
	{
		Cleanup();
	}

	public void OnAttachToEntity(Entity target)
	{
		_target = target;
	}

	protected override void ValueSetter(float value)
	{
		if (value < 0.0001f)
		{
			Cleanup();
		}
		else if (_target != null && _modifier == null)
		{
			_modifier = _target.Visual.GetNewTransformModifier();
		}
	}

	protected override void Update()
	{
		base.Update();
		float time = (useUnscaledInterval ? Time.unscaledTime : Time.time);
		if (_modifier != null && time - _lastShakeTime > shakeInterval)
		{
			_lastShakeTime = time;
			Vector3 dir = (onlyOnXZPlane ? Random.insideUnitCircle.ToXZ() : Random.insideUnitSphere);
			_modifier.worldOffset = dir * (shakeIntensity * base.currentValue);
		}
	}

	private void Cleanup()
	{
		if (_modifier != null)
		{
			_modifier.Stop();
			_modifier = null;
		}
	}
}
