using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UI_ShowAndHideObject : MonoBehaviour
{
	private const float ShowDuration = 0.25f;

	private const float HideDuration = 0.25f;

	private CanvasGroup _cg;

	private bool _defaultBlocksRaycast;

	private Vector3? _defaultScale;

	private bool _isShown;

	public CanvasGroup cg
	{
		get
		{
			if (_cg == null)
			{
				_cg = GetComponent<CanvasGroup>();
				_defaultBlocksRaycast = _cg.blocksRaycasts;
			}
			return _cg;
		}
	}

	protected virtual void OnEnable()
	{
		_isShown = true;
	}

	public void Show()
	{
		if (_isShown)
		{
			return;
		}
		if (!_defaultScale.HasValue)
		{
			_defaultScale = base.transform.localScale;
		}
		_isShown = true;
		base.gameObject.SetActive(value: true);
		cg.alpha = 0f;
		cg.blocksRaycasts = _defaultBlocksRaycast;
		base.transform.localScale = _defaultScale.Value * 0.7f;
		base.transform.DOKill(complete: true);
		base.transform.DOScale(_defaultScale.Value, 0.25f).SetUpdate(isIndependentUpdate: true);
		cg.DOKill();
		cg.DOFade(1f, 0.25f).SetUpdate(isIndependentUpdate: true);
		try
		{
			OnShow();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void Hide()
	{
		if (!_isShown)
		{
			return;
		}
		if (!_defaultScale.HasValue)
		{
			_defaultScale = base.transform.localScale;
		}
		_isShown = false;
		cg.alpha = 1f;
		cg.blocksRaycasts = false;
		base.transform.localScale = _defaultScale.Value;
		base.transform.DOKill(complete: true);
		base.transform.DOScale(_defaultScale.Value * 0.75f, 0.25f).SetUpdate(isIndependentUpdate: true);
		DOTween.Sequence().SetId(cg).Append(cg.DOFade(0f, 0.25f))
			.AppendCallback(delegate
			{
				base.gameObject.SetActive(value: false);
			})
			.SetUpdate(isIndependentUpdate: true);
		try
		{
			OnHide();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected virtual void OnShow()
	{
	}

	protected virtual void OnHide()
	{
	}
}
