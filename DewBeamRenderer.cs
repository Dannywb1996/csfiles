using UnityEngine;

[LogicUpdatePriority(1100)]
public class DewBeamRenderer : LogicBehaviour
{
	public LineRenderer lineRenderer;

	public GameObject startEffect;

	public GameObject endEffect;

	public GameObject acrossEffect;

	public float acrossEffectInterval = 0.01666667f;

	private Vector3 _startPoint;

	private Vector3 _endPoint;

	private float _lastAcrossTime;

	private bool _acrossEffectAtEnd;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (acrossEffect != null)
		{
			_acrossEffectAtEnd = false;
			acrossEffect.transform.position = _startPoint;
			_lastAcrossTime = Time.time;
		}
		SetPoints(_startPoint, _endPoint);
		if (acrossEffect != null)
		{
			DewEffect.Play(acrossEffect);
		}
		if (startEffect != null)
		{
			DewEffect.Play(startEffect);
		}
		if (endEffect != null)
		{
			DewEffect.Play(endEffect);
		}
		lineRenderer.enabled = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (acrossEffect != null)
		{
			DewEffect.Stop(acrossEffect);
		}
		if (startEffect != null)
		{
			DewEffect.Stop(startEffect);
		}
		if (endEffect != null)
		{
			DewEffect.Stop(endEffect);
		}
		lineRenderer.enabled = false;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (acrossEffect != null && Time.time - _lastAcrossTime > acrossEffectInterval)
		{
			_acrossEffectAtEnd = !_acrossEffectAtEnd;
			acrossEffect.transform.position = (_acrossEffectAtEnd ? _endPoint : _startPoint);
			_lastAcrossTime = Time.time;
		}
	}

	public void SetStartPoint(Vector3 point)
	{
		_startPoint = point;
		lineRenderer.SetPosition(0, point);
		if (startEffect != null)
		{
			startEffect.transform.position = point;
		}
	}

	public void SetEndPoint(Vector3 point)
	{
		_endPoint = point;
		lineRenderer.SetPosition(1, point);
		if (endEffect != null)
		{
			endEffect.transform.position = point;
		}
	}

	public void SetPoints(Vector3 start, Vector3 end)
	{
		SetStartPoint(start);
		SetEndPoint(end);
	}
}
