using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Canvas))]
public class View : MonoBehaviour
{
	public enum ShowBehavior
	{
		UseState,
		Manually
	}

	public static List<View> instances = new List<View>();

	public ShowBehavior showBehavior;

	private bool _show;

	public List<string> showOn = new List<string>();

	public List<string> ignoreOn = new List<string>();

	public GameObject showEffect;

	public GameObject hideEffect;

	public float fadeTime;

	public bool interactableWhenShown = true;

	public bool blockRaycastWhenShown = true;

	public bool disableGameObjectWhenHidden;

	public bool disablesInGamePlayingInput;

	public UnityEvent onShow;

	public UnityEvent onHide;

	private CanvasGroup _canvasGroup;

	private Canvas _canvas;

	public bool isShowing => _show;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		instances = new List<View>();
	}

	protected virtual void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
		_canvas = GetComponent<Canvas>();
		instances.Add(this);
	}

	protected virtual void Start()
	{
		if (Application.IsPlaying(this))
		{
			if (ManagerBase<UIManager>.instance != null)
			{
				UIManager instance = ManagerBase<UIManager>.instance;
				instance.onStateChanged = (Action<string, string>)Delegate.Combine(instance.onStateChanged, new Action<string, string>(OnStateChanged));
				UpdateShowState(ManagerBase<UIManager>.instance.state);
			}
			else
			{
				Debug.LogWarning("View '" + base.name + "' has no UIManager");
			}
			UpdateComponentStatus(_show);
		}
	}

	protected virtual void OnDestroy()
	{
		if (ManagerBase<UIManager>.instance != null)
		{
			UIManager instance = ManagerBase<UIManager>.instance;
			instance.onStateChanged = (Action<string, string>)Delegate.Remove(instance.onStateChanged, new Action<string, string>(OnStateChanged));
		}
		instances.Remove(this);
		if (InGameUIManager.instance != null)
		{
			InGameUIManager.instance.UpdateDisablePlayingInputByView();
		}
	}

	private void OnStateChanged(string arg1, string arg2)
	{
		UpdateShowState(arg2);
	}

	private void UpdateShowState(string newState)
	{
		if (showBehavior == ShowBehavior.UseState)
		{
			bool prevShow = _show;
			if (!ignoreOn.Contains(newState))
			{
				_show = showOn.Contains(newState);
			}
			if (!prevShow && _show)
			{
				OnShow();
			}
			else if (prevShow && !_show)
			{
				OnHide();
			}
		}
	}

	public void Show()
	{
		if (showBehavior != ShowBehavior.Manually)
		{
			throw new InvalidOperationException("Show Behaviour is not set to ShowBehaviour.Manually");
		}
		if (!_show)
		{
			_show = true;
			OnShow();
			Update();
		}
	}

	public void Hide()
	{
		if (showBehavior != ShowBehavior.Manually)
		{
			throw new InvalidOperationException("Show Behaviour is not set to ShowBehaviour.Manually");
		}
		if (_show)
		{
			_show = false;
			OnHide();
			Update();
		}
	}

	protected virtual void OnShow()
	{
		base.gameObject.SetActive(value: true);
		onShow?.Invoke();
		DewEffect.Play(showEffect);
		DewEffect.Stop(hideEffect);
	}

	protected virtual void OnHide()
	{
		onHide?.Invoke();
		DewEffect.Play(hideEffect);
		DewEffect.Stop(showEffect);
	}

	protected virtual void Update()
	{
		bool shouldShow = _show;
		UpdateComponentStatus(shouldShow);
	}

	private void UpdateComponentStatus(bool shouldShow)
	{
		if (fadeTime < 0.001f || !Application.IsPlaying(this))
		{
			_canvasGroup.alpha = (shouldShow ? 1 : 0);
		}
		else
		{
			_canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, shouldShow ? 1 : 0, Time.unscaledDeltaTime / fadeTime);
		}
		_canvasGroup.interactable = shouldShow && interactableWhenShown;
		_canvasGroup.blocksRaycasts = shouldShow && blockRaycastWhenShown;
		if (_canvas == null)
		{
			_canvas = GetComponent<Canvas>();
		}
		_canvas.enabled = _canvasGroup.alpha > 0.0001f;
		if (Application.IsPlaying(this))
		{
			if (disableGameObjectWhenHidden && !shouldShow && _canvasGroup.alpha < 0.0001f)
			{
				base.gameObject.SetActive(value: false);
			}
			if (InGameUIManager.instance != null)
			{
				InGameUIManager.instance.UpdateDisablePlayingInputByView();
			}
		}
	}

	private static bool IsParentOrSelf(Transform parent, Transform child)
	{
		if (parent == child)
		{
			return true;
		}
		if (child == null)
		{
			return false;
		}
		return IsParentOrSelf(parent, child.parent);
	}

	private static bool HasCommonElement(List<string> a, List<string> b)
	{
		foreach (string i in a)
		{
			foreach (string j in b)
			{
				if (i == j)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}
}
