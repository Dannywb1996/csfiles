using System;
using DewInternal;
using DG.Tweening;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;

[LogicUpdatePriority(3000)]
public class TutorialArrow : LogicBehaviour
{
	private class RefText
	{
		public string text;
	}

	public TextMeshProUGUI textUGUI;

	public float minTime = 3f;

	public float maxTime = 30f;

	public Func<bool> destroyCondition;

	public Action onDestroy;

	public Transform starTransform;

	public Transform boxTransform;

	public UILineRenderer lineRenderer;

	public float boxNormalizedDistance;

	public float boxSmoothTime;

	public float disappearTime = 1.25f;

	public Vector3 boxPunch;

	public float boxPunchDuration;

	private ArrowFollowMode _followMode;

	private Transform _followTarget;

	private Func<Vector3> _followFunc;

	private bool _isDestroyConditionMet;

	private float _creationTime;

	private Vector2[] _lineRendererPoints = new Vector2[2];

	private Vector3 _boxCv;

	private CanvasGroup _cg;

	private BoxPlacementMode _boxMode;

	private Vector2 _boxOffset;

	private Vector2 _boxOffsetCv;

	private Vector3? _customOffset;

	public float elapsedTime => Time.time - _creationTime;

	private void Awake()
	{
		_creationTime = Time.time;
		_cg = GetComponent<CanvasGroup>();
	}

	private void Start()
	{
		UpdatePosition(init: true);
		boxTransform.DOPunchScale(boxPunch, boxPunchDuration);
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!_isDestroyConditionMet && destroyCondition != null && destroyCondition())
		{
			_isDestroyConditionMet = true;
		}
		if ((_isDestroyConditionMet && elapsedTime > minTime) || elapsedTime > maxTime)
		{
			_cg.alpha = Mathf.MoveTowards(_cg.alpha, 0f, dt / disappearTime);
			if (_cg.alpha <= 0.001f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (Time.timeScale > 0.0001f)
		{
			UpdatePosition(init: false);
		}
	}

	private void UpdatePosition(bool init)
	{
		switch (_followMode)
		{
		case ArrowFollowMode.None:
			return;
		case ArrowFollowMode.World:
			try
			{
				Vector3 pos2;
				if (_followTarget != null)
				{
					pos2 = _followTarget.position;
				}
				else
				{
					if (_followFunc == null)
					{
						return;
					}
					pos2 = _followFunc();
				}
				starTransform.position = Dew.mainCamera.WorldToScreenPoint(pos2);
			}
			catch (Exception)
			{
			}
			break;
		case ArrowFollowMode.Screen:
			try
			{
				Vector3 pos;
				if (_followTarget != null)
				{
					pos = _followTarget.position;
				}
				else
				{
					if (_followFunc == null)
					{
						return;
					}
					pos = _followFunc();
				}
				starTransform.position = pos;
			}
			catch (Exception)
			{
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		Vector3 viewPortPos = starTransform.position;
		viewPortPos.x /= Screen.width;
		viewPortPos.y /= Screen.height;
		Vector2 boxOffset;
		if (_customOffset.HasValue)
		{
			boxOffset = _customOffset.Value;
		}
		else
		{
			switch (_boxMode)
			{
			case BoxPlacementMode.TowardsCenter:
				boxOffset = (Vector2.one * 0.5f - (Vector2)viewPortPos).normalized * boxNormalizedDistance;
				break;
			case BoxPlacementMode.AwayFromCenter:
				boxOffset = (Vector2.one * 0.5f - (Vector2)viewPortPos).normalized * boxNormalizedDistance;
				boxOffset *= -1f;
				break;
			case BoxPlacementMode.Below:
				boxOffset = Vector2.down * 0.05f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		Vector2 boxTarget = (Vector2)viewPortPos + boxOffset;
		boxTarget.x *= Screen.width;
		boxTarget.y *= Screen.height;
		Vector2 offsetTarget = Vector2.zero;
		if (ManagerBase<GlobalUIManager>.instance.isTooltipShown)
		{
			Rect boxRect = ((RectTransform)boxTransform).GetScreenSpaceRect();
			Rect tooltipRect = ManagerBase<GlobalUIManager>.instance.tooltipScreenSpaceRect.Expand(20f);
			if (tooltipRect.Contains(boxTarget - boxRect.size * 0.5f) || tooltipRect.Contains(boxTarget) || tooltipRect.Contains(boxTarget + boxRect.size * 0.5f) || tooltipRect.Contains(boxTarget + new Vector2(0f - boxRect.size.x, boxRect.size.y) * 0.5f) || tooltipRect.Contains(boxTarget + new Vector2(boxRect.size.x, 0f - boxRect.size.y) * 0.5f))
			{
				offsetTarget = Vector2.up * (tooltipRect.yMax + boxRect.size.y * 0.5f - boxTarget.y);
			}
		}
		if (init)
		{
			_boxOffset = offsetTarget;
		}
		else
		{
			_boxOffset = Vector2.SmoothDamp(_boxOffset, offsetTarget, ref _boxOffsetCv, (_boxOffset.magnitude > offsetTarget.magnitude) ? 0.3f : 0.03f);
		}
		boxTarget += _boxOffset;
		if (init)
		{
			boxTransform.position = boxTarget;
		}
		else
		{
			boxTransform.position = Vector3.SmoothDamp(boxTransform.position, boxTarget, ref _boxCv, boxSmoothTime);
		}
		_lineRendererPoints[0] = base.transform.InverseTransformPoint(starTransform.position);
		_lineRendererPoints[1] = base.transform.InverseTransformPoint(boxTransform.position);
		lineRenderer.Points = _lineRendererPoints;
		lineRenderer.SetAllDirty();
	}

	private void OnDestroy()
	{
		try
		{
			onDestroy?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public TutorialArrow SetDuration(float duration)
	{
		minTime = 0f;
		maxTime = duration;
		return this;
	}

	public TutorialArrow SetDuration(float minTime, float maxTime)
	{
		this.minTime = minTime;
		this.maxTime = maxTime;
		return this;
	}

	public TutorialArrow SetRawText(string text, bool processBacktickExpressions)
	{
		if (processBacktickExpressions)
		{
			text = ProcessBacktickedExpressions(text);
		}
		textUGUI.text = text;
		return this;
	}

	public TutorialArrow SetBoxPlacement(BoxPlacementMode mode)
	{
		_boxMode = mode;
		return this;
	}

	public TutorialArrow SetBoxPlacementByOffset(Vector2 offset)
	{
		_customOffset = offset;
		return this;
	}

	public TutorialArrow SetLocalizedText(string key, object[] formatArgs = null)
	{
		string inputText = DewLocalization.GetUIValue(key);
		if (formatArgs != null)
		{
			inputText = string.Format(inputText, formatArgs);
		}
		textUGUI.text = ProcessBacktickedExpressions(inputText);
		return this;
	}

	private string ProcessBacktickedExpressions(string input)
	{
		RefText val = new RefText();
		DewLocalizationNodeParser.ParseBacktickedString(input, delegate(string normal)
		{
			val.text += normal;
		}, delegate(string tagged)
		{
			RefText refText = val;
			refText.text = refText.text + "<" + tagged + ">";
		}, delegate(string backticked)
		{
			RefText refText = val;
			refText.text = refText.text + "[" + DewSave.profile.controls.GetSettingsValueText(backticked) + "]";
		});
		val.text = val.text.Replace("[", "<color=yellow>").Replace("]", "</color>");
		return val.text;
	}

	public TutorialArrow FollowUIElement(Transform target)
	{
		_followTarget = target;
		_followMode = ArrowFollowMode.Screen;
		return this;
	}

	public TutorialArrow FollowScreenPos(Func<Vector3> func)
	{
		_followFunc = func;
		_followMode = ArrowFollowMode.Screen;
		return this;
	}

	public TutorialArrow FollowWorldTarget(Transform target)
	{
		_followTarget = target;
		_followMode = ArrowFollowMode.World;
		return this;
	}

	public TutorialArrow FollowWorldTarget(Func<Vector3> func)
	{
		_followFunc = func;
		_followMode = ArrowFollowMode.World;
		return this;
	}

	public TutorialArrow SetDestroyCondition(Func<bool> condition)
	{
		destroyCondition = condition;
		return this;
	}

	public TutorialArrow SetOnDestroy(Action func)
	{
		onDestroy = (Action)Delegate.Combine(onDestroy, func);
		return this;
	}

	public TutorialArrow MarkDestroy()
	{
		_isDestroyConditionMet = true;
		return this;
	}
}
