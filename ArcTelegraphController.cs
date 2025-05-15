using System;
using System.Collections;
using UnityEngine;

public class ArcTelegraphController : MonoBehaviour, IEffectComponent, IEffectWithSpeed
{
	private static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");

	private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");

	private static readonly int ArcAngle = Shader.PropertyToID("_ArcAngle");

	private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");

	private static readonly int Color = Shader.PropertyToID("_Color");

	private static readonly int MainTex = Shader.PropertyToID("_MainTex");

	private static readonly int DistortTex = Shader.PropertyToID("_DistortTex");

	public float duration = 1f;

	public float arcAngle = 360f;

	public float outerRadius;

	public float innerRadius = 3f;

	public MeshRenderer whiteBorder;

	public MeshRenderer fillBorder;

	public MeshRenderer fillArea;

	public MeshRenderer maxBorder;

	private float _value;

	private Material _matWhiteBorder;

	private Material _matFillBorder;

	private Material _matFillArea;

	private Material _matMaxBorder;

	public float value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				UpdateProperties();
			}
		}
	}

	public bool isPlaying => _value > 0f;

	private void Awake()
	{
		_matWhiteBorder = UnityEngine.Object.Instantiate(whiteBorder.sharedMaterial);
		whiteBorder.sharedMaterial = _matWhiteBorder;
		_matFillBorder = UnityEngine.Object.Instantiate(fillBorder.sharedMaterial);
		fillBorder.sharedMaterial = _matFillBorder;
		_matFillArea = UnityEngine.Object.Instantiate(fillArea.sharedMaterial);
		fillArea.sharedMaterial = _matFillArea;
		_matMaxBorder = UnityEngine.Object.Instantiate(maxBorder.sharedMaterial);
		maxBorder.sharedMaterial = _matMaxBorder;
		UpdateProperties();
	}

	private void OnValidate()
	{
		outerRadius = Mathf.Max(innerRadius, outerRadius, 0f);
		innerRadius = Mathf.Clamp(innerRadius, 0f, outerRadius);
		arcAngle = Mathf.Clamp(arcAngle, 0f, 360f);
	}

	private void OnDestroy()
	{
		if (_matWhiteBorder != null)
		{
			UnityEngine.Object.Destroy(_matWhiteBorder);
		}
		if (_matFillBorder != null)
		{
			UnityEngine.Object.Destroy(_matFillBorder);
		}
		if (_matFillArea != null)
		{
			UnityEngine.Object.Destroy(_matFillArea);
		}
		if (_matMaxBorder != null)
		{
			UnityEngine.Object.Destroy(_matMaxBorder);
		}
	}

	private void UpdateProperties()
	{
		whiteBorder.gameObject.SetActive(isPlaying);
		fillBorder.gameObject.SetActive(isPlaying);
		fillArea.gameObject.SetActive(isPlaying);
		maxBorder.gameObject.SetActive(isPlaying);
		if (isPlaying)
		{
			float linearV = value;
			float animV = EasingFunction.EaseOutQuad(0f, 1f, linearV);
			SetScaleAll(outerRadius * 2f);
			SetPropertyMax(OuterRadius, 0.5f);
			SetPropertyMax(InnerRadius, innerRadius / outerRadius * 0.5f);
			SetPropertyFill(OuterRadius, Mathf.Lerp(0.5f * innerRadius / outerRadius, 0.5f, animV));
			SetPropertyFill(InnerRadius, innerRadius / outerRadius * 0.5f);
			float scale = whiteBorder.transform.lossyScale.x;
			SetPropertyAll(ArcAngle, arcAngle);
			SetPropertyAll(OutlineWidth, 0.5f / scale);
			SetTextureScaleAll(MainTex, Vector2.one * (scale * 0.11f));
			SetTextureScaleAll(DistortTex, Vector2.one * (scale * 0.11f));
			_matMaxBorder.SetColor(Color, new Color(0.75f, 0f, 0f, 0.3f + linearV * 0.15f));
			_matWhiteBorder.SetColor(Color, new Color(1f, 1f, 1f, linearV * 0.5f - 0.15f));
			_matFillBorder.SetColor(Color, new Color(1f, 0f, 0f, 0.25f + linearV * 0.25f));
			_matFillArea.SetColor(Color, new Color(1f, 0f, 0f, 0.15f + linearV * 0.35f));
		}
	}

	private void SetPropertyAll(int id, float val)
	{
		_matWhiteBorder.SetFloat(id, val);
		_matFillBorder.SetFloat(id, val);
		_matFillArea.SetFloat(id, val);
		_matMaxBorder.SetFloat(id, val);
	}

	private void SetTextureScaleAll(int id, Vector2 scale)
	{
		_matWhiteBorder.SetTextureScale(id, scale);
		_matFillBorder.SetTextureScale(id, scale);
		_matFillArea.SetTextureScale(id, scale);
		_matMaxBorder.SetTextureScale(id, scale);
	}

	private void SetPropertyFill(int id, float val)
	{
		_matWhiteBorder.SetFloat(id, val);
		_matFillBorder.SetFloat(id, val);
		_matFillArea.SetFloat(id, val);
	}

	private void SetPropertyMax(int id, float val)
	{
		_matMaxBorder.SetFloat(id, val);
	}

	private void SetScaleAll(float val)
	{
		whiteBorder.transform.localScale = new Vector3(val, val, val);
		fillBorder.transform.localScale = new Vector3(val, val, val);
		fillArea.transform.localScale = new Vector3(val, val, val);
		maxBorder.transform.localScale = new Vector3(val, val, val);
	}

	public void Play()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			value = 0.001f;
			yield return null;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				value = t / duration;
				yield return null;
			}
			value = 1f;
			yield return null;
			value = 0f;
			Stop();
		}
	}

	public void Stop()
	{
		StopAllCoroutines();
		value = 0f;
	}

	private void OnDrawGizmos()
	{
		_ = base.transform.position;
		int usedSegments = Mathf.CeilToInt(36f * (Mathf.Min(arcAngle, 360f) / 360f));
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		if (arcAngle >= 359f)
		{
			for (int i = 0; i < 36; i++)
			{
				float angle1 = (float)i / 36f * 2f * MathF.PI;
				float angle2 = (float)(i + 1) / 36f * 2f * MathF.PI;
				Vector3 point1Outer = Vector3.zero + new Vector3(Mathf.Sin(angle1) * outerRadius, 0f, Mathf.Cos(angle1) * outerRadius);
				Vector3 point2Outer = Vector3.zero + new Vector3(Mathf.Sin(angle2) * outerRadius, 0f, Mathf.Cos(angle2) * outerRadius);
				Vector3 vector = Vector3.zero + new Vector3(Mathf.Sin(angle1) * innerRadius, 0f, Mathf.Cos(angle1) * innerRadius);
				Vector3 point2Inner = Vector3.zero + new Vector3(Mathf.Sin(angle2) * innerRadius, 0f, Mathf.Cos(angle2) * innerRadius);
				Gizmos.DrawLine(point1Outer, point2Outer);
				Gizmos.DrawLine(vector, point2Inner);
			}
		}
		else
		{
			float startAngle = (0f - arcAngle) * 0.5f * (MathF.PI / 180f);
			float angleStep = arcAngle * (MathF.PI / 180f) / (float)usedSegments;
			for (int j = 0; j <= usedSegments; j++)
			{
				float angle3 = startAngle + (float)j * angleStep;
				Vector3 pointOuter = Vector3.zero + new Vector3(Mathf.Sin(angle3) * outerRadius, 0f, Mathf.Cos(angle3) * outerRadius);
				Vector3 pointInner = Vector3.zero + new Vector3(Mathf.Sin(angle3) * innerRadius, 0f, Mathf.Cos(angle3) * innerRadius);
				if (j > 0)
				{
					float prevAngle = startAngle + (float)(j - 1) * angleStep;
					Vector3 prevPointOuter = Vector3.zero + new Vector3(Mathf.Sin(prevAngle) * outerRadius, 0f, Mathf.Cos(prevAngle) * outerRadius);
					Vector3 vector2 = Vector3.zero + new Vector3(Mathf.Sin(prevAngle) * innerRadius, 0f, Mathf.Cos(prevAngle) * innerRadius);
					Gizmos.DrawLine(prevPointOuter, pointOuter);
					Gizmos.DrawLine(vector2, pointInner);
				}
			}
			float firstAngle = startAngle;
			float lastAngle = startAngle + arcAngle * (MathF.PI / 180f);
			Vector3 firstOuter = Vector3.zero + new Vector3(Mathf.Sin(firstAngle) * outerRadius, 0f, Mathf.Cos(firstAngle) * outerRadius);
			Vector3 firstInner = Vector3.zero + new Vector3(Mathf.Sin(firstAngle) * innerRadius, 0f, Mathf.Cos(firstAngle) * innerRadius);
			Vector3 vector3 = Vector3.zero + new Vector3(Mathf.Sin(lastAngle) * outerRadius, 0f, Mathf.Cos(lastAngle) * outerRadius);
			Vector3 lastInner = Vector3.zero + new Vector3(Mathf.Sin(lastAngle) * innerRadius, 0f, Mathf.Cos(lastAngle) * innerRadius);
			Gizmos.DrawLine(firstOuter, firstInner);
			Gizmos.DrawLine(vector3, lastInner);
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	public void ApplySpeedMultiplier(float speed)
	{
		duration /= speed;
	}
}
