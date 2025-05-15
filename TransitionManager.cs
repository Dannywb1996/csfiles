using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : ManagerBase<TransitionManager>
{
	public enum StateType
	{
		Normal,
		Loading
	}

	public Action onFadeIn;

	public Action onFadeOut;

	public Action<LoadingStatus> onStatusChanged;

	public float fadeTime;

	public CanvasGroup screenFadeCg;

	public GameObject busyObject;

	public GameObject fxTransition;

	public TextMeshProUGUI tipText;

	public TextMeshProUGUI[] loadingTexts;

	public float newTipCooldownTime = 1f;

	private float _lastTooltipChangeTime = float.NegativeInfinity;

	private readonly List<string> _remainingTooltips = new List<string>();

	public StateType state { get; private set; }

	private void Start()
	{
		tipText.enabled = false;
		screenFadeCg.alpha = 1f;
		screenFadeCg.gameObject.SetActive(value: true);
		busyObject.SetActive(value: false);
	}

	public void FadeOut(bool showTips)
	{
		if (state == StateType.Loading)
		{
			return;
		}
		state = StateType.Loading;
		if (showTips)
		{
			tipText.enabled = true;
			TryGetNewTip();
		}
		else
		{
			tipText.enabled = false;
		}
		fxTransition.gameObject.SetActive(value: true);
		DewEffect.Play(fxTransition);
		TextMeshProUGUI[] array = loadingTexts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].text = DewLocalization.GetUIValue("Loading");
		}
		try
		{
			onFadeOut?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public IEnumerator FadeOutRoutine(bool showTips)
	{
		FadeOut(showTips);
		yield return new WaitForSeconds(fadeTime);
	}

	public void TransitionToScene(string sceneName)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			FadeOut(showTips: false);
			yield return new WaitForSecondsRealtime(fadeTime);
			SceneManager.LoadScene(sceneName);
			FadeIn();
		}
	}

	private void TryGetNewTip()
	{
		if (!(Time.unscaledTime - _lastTooltipChangeTime < newTipCooldownTime))
		{
			_lastTooltipChangeTime = Time.unscaledTime;
			if (_remainingTooltips.Count == 0)
			{
				_remainingTooltips.AddRange(DewLocalization.data.tips.Keys);
			}
			int i = UnityEngine.Random.Range(0, _remainingTooltips.Count);
			tipText.text = DewLocalization.data.tips[_remainingTooltips[i]];
			_remainingTooltips.RemoveAt(i);
		}
	}

	public void FadeIn()
	{
		StartCoroutine(FadeInRoutine());
	}

	public IEnumerator FadeInRoutine()
	{
		if (state != StateType.Normal)
		{
			yield return null;
			yield return null;
			yield return null;
			try
			{
				onFadeIn?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			state = StateType.Normal;
			yield return null;
			yield return null;
			yield return null;
			DewEffect.Stop(fxTransition);
			yield return new WaitForSeconds(fadeTime);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		screenFadeCg.alpha = Mathf.MoveTowards(screenFadeCg.alpha, (state == StateType.Loading) ? 1 : 0, Time.unscaledDeltaTime / fadeTime);
		screenFadeCg.blocksRaycasts = state == StateType.Loading;
		screenFadeCg.gameObject.SetActive(screenFadeCg.alpha > 0.001f);
		if (state == StateType.Loading && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)))
		{
			TryGetNewTip();
		}
	}

	public void UpdateLoadingStatus(LoadingStatus status)
	{
		if (status == LoadingStatus.Empty)
		{
			TextMeshProUGUI[] array = loadingTexts;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].text = "";
			}
		}
		else
		{
			TextMeshProUGUI[] array = loadingTexts;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].text = DewLocalization.GetUIValue("Loading_" + status);
			}
		}
		try
		{
			onStatusChanged?.Invoke(status);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void SetBusy(bool value)
	{
		busyObject.SetActive(value);
	}
}
