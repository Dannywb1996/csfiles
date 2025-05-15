using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI.Extensions;

[DewResourceLink(ResourceLinkBy.Name)]
public class Emote : MonoBehaviour
{
	private enum InitMode
	{
		Normal,
		Preview,
		Stationary
	}

	public bool generatedFromServer;

	[NonSerialized]
	public float? customDuration;

	public Func<Vector3> posGetter;

	private InitMode _mode;

	private void Start()
	{
		if (_mode == InitMode.Stationary)
		{
			return;
		}
		DewEffect.Play(base.gameObject);
		base.transform.localScale *= 0.85f;
		if (_mode == InitMode.Normal)
		{
			UpdatePosition();
			DOTween.Sequence().AppendInterval(customDuration.HasValue ? customDuration.Value : 1.6f).Append(base.transform.DOScale(Vector3.zero, 0.15f))
				.AppendCallback(delegate
				{
					UnityEngine.Object.Destroy(base.gameObject);
				});
		}
	}

	public void SetupPreview()
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException();
		}
		_mode = InitMode.Preview;
	}

	public void SetupStationary()
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException();
		}
		_mode = InitMode.Stationary;
		RotateTransform[] componentsInChildren = GetComponentsInChildren<RotateTransform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		UIParticleSystem[] componentsInChildren2 = GetComponentsInChildren<UIParticleSystem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[i]);
		}
		ParticleSystem[] componentsInChildren3 = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren3[i]);
		}
		DOTweenAnimation[] componentsInChildren4 = GetComponentsInChildren<DOTweenAnimation>(includeInactive: true);
		foreach (DOTweenAnimation obj in componentsInChildren4)
		{
			obj.DOComplete();
			UnityEngine.Object.Destroy(obj);
		}
		Transform glow0 = base.transform.FindDeepChild("Glow0");
		Transform glow1 = base.transform.FindDeepChild("Glow1");
		Transform glowShadow = base.transform.FindDeepChild("GlowShadow");
		if (glow0 != null)
		{
			glow0.localScale *= 0.75f;
		}
		if (glow1 != null)
		{
			glow1.localScale *= 0.75f;
		}
		if (glowShadow != null)
		{
			glowShadow.localScale *= 0.75f;
		}
		foreach (Transform item in base.transform.FindDeepChildren("Extra Layer"))
		{
			item.gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		if (_mode == InitMode.Normal)
		{
			UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		try
		{
			base.transform.position = posGetter();
		}
		catch (Exception)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
