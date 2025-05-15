using UnityEngine;

[RequireComponent(typeof(Light))]
public class FxPointLight : FxInterpolatedEffectBase
{
	public bool animateIntensity = true;

	public float intensityMultiplier = 1f;

	public bool animateRange;

	public float rangeMultiplier = 1f;

	public bool animateColor;

	public Gradient colorOverValue = new Gradient();

	[SerializeField]
	[HideInInspector]
	private bool _didGetValues;

	[SerializeField]
	[HideInInspector]
	private float _originalIntensity;

	[SerializeField]
	[HideInInspector]
	private float _originalRange;

	private Light _light;

	protected override void OnInit()
	{
		base.OnInit();
		_light = GetComponent<Light>();
		if (!_didGetValues)
		{
			_originalIntensity = _light.intensity;
			_originalRange = _light.range;
			_didGetValues = true;
		}
	}

	protected override void ValueSetter(float value)
	{
		_light.enabled = value > 0f;
		if (animateIntensity)
		{
			_light.intensity = value * _originalIntensity * intensityMultiplier;
		}
		if (animateRange)
		{
			_light.range = value * _originalRange * rangeMultiplier;
		}
		if (animateColor)
		{
			_light.color = colorOverValue.Evaluate(value);
		}
	}
}
