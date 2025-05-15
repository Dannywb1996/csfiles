using System;
using System.Collections;
using UnityEngine;

public class BoxTelegraphController : MonoBehaviour, IEffectComponent, IEffectWithSpeed
{
	private static readonly EaseFunction OuterSizeEase = EasingFunction.GetEasingFunction(DewEase.EaseOutExpo);

	private static readonly EaseFunction InnerSizeEase = EasingFunction.GetEasingFunction(DewEase.EaseOutQuad);

	private static readonly int Alpha = Shader.PropertyToID("_Alpha");

	public float duration = 1f;

	public BoxTelegraphAnimType animateX;

	public DewEase animateXEase;

	public BoxTelegraphAnimType animateY;

	public DewEase animateYEase;

	public float width = 4f;

	public float height = 20f;

	public SpriteRenderer[] whites;

	public SpriteRenderer[] reds;

	public SpriteRenderer[] outers;

	public SpriteRenderer[] inners;

	private Material[] _matWhites;

	private Material[] _matReds;

	private float _value;

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

	public bool isPlaying => value > 0.0001f;

	public void Play()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			value = 0f;
			UpdateProperties();
			yield return null;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				value = t / duration;
				yield return null;
			}
			value = 1f;
			Stop();
		}
	}

	public void Stop()
	{
		StopAllCoroutines();
		value = 0f;
	}

	private void Awake()
	{
		_matWhites = new Material[whites.Length];
		for (int i = 0; i < whites.Length; i++)
		{
			SpriteRenderer w = whites[i];
			w.gameObject.SetActive(value: true);
			_matWhites[i] = w.material;
		}
		_matReds = new Material[reds.Length];
		for (int j = 0; j < reds.Length; j++)
		{
			SpriteRenderer r = reds[j];
			r.gameObject.SetActive(value: true);
			_matReds[j] = r.material;
		}
		UpdateProperties();
	}

	private void UpdateProperties()
	{
		SpriteRenderer[] array = whites;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(isPlaying);
		}
		array = reds;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(isPlaying);
		}
		if (!isPlaying)
		{
			return;
		}
		Vector2 size = new Vector2(width, height);
		size /= base.transform.localScale.x;
		float outerSizeVal = OuterSizeEase(0f, 1f, Mathf.Clamp01(value * 1.5f));
		Vector2 outerSize = size;
		switch (animateX)
		{
		case BoxTelegraphAnimType.Center:
			outerSize.x *= outerSizeVal;
			break;
		case BoxTelegraphAnimType.FromStart:
			array = outers;
			foreach (SpriteRenderer o2 in array)
			{
				o2.transform.localPosition = new Vector3((0f - width) * 0.5f * (1f - outerSizeVal) / o2.transform.lossyScale.x * o2.transform.localScale.x, 0.1f, 0f);
			}
			outerSize.x *= outerSizeVal;
			break;
		case BoxTelegraphAnimType.FromEnd:
			array = outers;
			foreach (SpriteRenderer o in array)
			{
				o.transform.localPosition = new Vector3(width * 0.5f * (1f - outerSizeVal) / o.transform.lossyScale.x * o.transform.localScale.x, 0.1f, 0f);
			}
			outerSize.x *= outerSizeVal;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case BoxTelegraphAnimType.None:
			break;
		}
		switch (animateY)
		{
		case BoxTelegraphAnimType.Center:
			outerSize.y *= outerSizeVal;
			break;
		case BoxTelegraphAnimType.FromStart:
			array = outers;
			foreach (SpriteRenderer o4 in array)
			{
				o4.transform.localPosition = new Vector3(0f, 0.1f, (0f - height) * 0.5f * (1f - outerSizeVal) / o4.transform.lossyScale.z * o4.transform.localScale.x);
			}
			outerSize.y *= outerSizeVal;
			break;
		case BoxTelegraphAnimType.FromEnd:
			array = outers;
			foreach (SpriteRenderer o3 in array)
			{
				o3.transform.localPosition = new Vector3(0f, 0.1f, height * 0.5f * (1f - outerSizeVal) / o3.transform.lossyScale.z * o3.transform.localScale.x);
			}
			outerSize.y *= outerSizeVal;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case BoxTelegraphAnimType.None:
			break;
		}
		array = outers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].size = outerSize;
		}
		float innerSizeDelay = 0.3f;
		float innerSizeVal = InnerSizeEase(0f, 1f, Mathf.Clamp01(value * (1f + innerSizeDelay) - innerSizeDelay));
		Vector2 innerSize = size;
		switch (animateX)
		{
		case BoxTelegraphAnimType.Center:
			innerSize.x *= innerSizeVal;
			break;
		case BoxTelegraphAnimType.FromStart:
			array = inners;
			foreach (SpriteRenderer i3 in array)
			{
				i3.transform.localPosition = new Vector3((0f - width) * 0.5f * (1f - innerSizeVal) / i3.transform.lossyScale.x * i3.transform.localScale.x, 0.1f, 0f);
			}
			innerSize.x *= innerSizeVal;
			break;
		case BoxTelegraphAnimType.FromEnd:
			array = inners;
			foreach (SpriteRenderer i2 in array)
			{
				i2.transform.localPosition = new Vector3(width * 0.5f * (1f - innerSizeVal) / i2.transform.lossyScale.x * i2.transform.localScale.x, 0.1f, 0f);
			}
			innerSize.x *= innerSizeVal;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case BoxTelegraphAnimType.None:
			break;
		}
		switch (animateY)
		{
		case BoxTelegraphAnimType.Center:
			innerSize.y *= innerSizeVal;
			break;
		case BoxTelegraphAnimType.FromStart:
			array = inners;
			foreach (SpriteRenderer i5 in array)
			{
				i5.transform.localPosition = new Vector3(0f, 0.1f, (0f - height) * 0.5f * (1f - innerSizeVal) / i5.transform.lossyScale.z * i5.transform.localScale.x);
			}
			innerSize.y *= innerSizeVal;
			break;
		case BoxTelegraphAnimType.FromEnd:
			array = inners;
			foreach (SpriteRenderer i4 in array)
			{
				i4.transform.localPosition = new Vector3(0f, 0.1f, height * 0.5f * (1f - innerSizeVal) / i4.transform.lossyScale.z * i4.transform.localScale.x);
			}
			innerSize.y *= innerSizeVal;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case BoxTelegraphAnimType.None:
			break;
		}
		array = inners;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].size = innerSize;
		}
		Material[] matReds = _matReds;
		for (int i = 0; i < matReds.Length; i++)
		{
			matReds[i].SetFloat(Alpha, 0.125f + 0.15f * Mathf.Clamp01(value * 3f) + value * 0.2f);
		}
		matReds = _matWhites;
		for (int i = 0; i < matReds.Length; i++)
		{
			matReds[i].SetFloat(Alpha, value * 0.35f);
		}
	}

	private void OnDestroy()
	{
		Material[] matWhites = _matWhites;
		foreach (Material w in matWhites)
		{
			if (!(w == null))
			{
				UnityEngine.Object.Destroy(w);
			}
		}
		matWhites = _matReds;
		foreach (Material r in matWhites)
		{
			if (!(r == null))
			{
				UnityEngine.Object.Destroy(r);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		float invScale = 1f / base.transform.localScale.x;
		Vector3 p0 = new Vector3((0f - width) * 0.5f, 0f, height * 0.5f) * invScale;
		Vector3 p1 = new Vector3(width * 0.5f, 0f, height * 0.5f) * invScale;
		Vector3 p2 = new Vector3(width * 0.5f, 0f, (0f - height) * 0.5f) * invScale;
		Vector3 p3 = new Vector3((0f - width) * 0.5f, 0f, (0f - height) * 0.5f) * invScale;
		p0 = base.transform.TransformPoint(p0);
		p1 = base.transform.TransformPoint(p1);
		p2 = base.transform.TransformPoint(p2);
		p3 = base.transform.TransformPoint(p3);
		Gizmos.DrawLine(p0, p1);
		Gizmos.DrawLine(p1, p2);
		Gizmos.DrawLine(p2, p3);
		Gizmos.DrawLine(p3, p0);
	}

	public void ApplySpeedMultiplier(float speed)
	{
		duration /= speed;
	}
}
