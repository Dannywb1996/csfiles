using UnityEngine;

[LogicUpdatePriority(2000)]
public class PlayLobby_Fireplace : LogicBehaviour
{
	public float intensityDeviation;

	public float intensitySmoothTime;

	public float intensityNewTargetTime;

	public float positionDeviation;

	public float positionSmoothTime;

	public float positionNewTargetTime;

	private Vector3 _originalPosition;

	private float _originalIntensity;

	private Vector3 _positionCv;

	private Vector3 _positionTarget;

	private float _positionSetTime = float.NegativeInfinity;

	private float _intensityCv;

	private float _intensityTarget;

	private float _intensitySetTime = float.NegativeInfinity;

	private Light _light;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_originalPosition = base.transform.position;
		_originalIntensity = _light.intensity;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (Time.time - _positionSetTime > positionNewTargetTime)
		{
			_positionSetTime = Time.time;
			_positionTarget = _originalPosition + positionDeviation * Random.insideUnitSphere;
		}
		if (Time.time - _intensitySetTime > intensityNewTargetTime)
		{
			_intensitySetTime = Time.time;
			_intensityTarget = _originalIntensity + intensityDeviation * (1f - Random.value * 2f);
		}
		base.transform.position = Vector3.SmoothDamp(base.transform.position, _positionTarget, ref _positionCv, positionSmoothTime);
		_light.intensity = Mathf.SmoothDamp(_light.intensity, _intensityTarget, ref _intensityCv, intensitySmoothTime);
	}
}
