using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Intro_SceneController : MonoBehaviour
{
	public float fadeDuration = 0.7f;

	public CanvasGroup fadeCg;

	public PlayableDirector director;

	private float _lastSkipButtonShowTime = float.NegativeInfinity;

	private float _normalizedHoldProgress;

	private bool _isInTransition;

	private void Start()
	{
		fadeCg.alpha = 1f;
		if (DewSave.platformSettings.gameplay.skipIntro || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			GoToTitle();
			return;
		}
		director.stopped += delegate
		{
			GoToTitle();
		};
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float waitStartTime = Time.unscaledTime;
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}
			while (Time.unscaledTime - waitStartTime < 3f && Time.deltaTime > 0.05f)
			{
				yield return null;
			}
			director.Play();
		}
	}

	public void Skip()
	{
		if (!_isInTransition)
		{
			director.Stop();
			GoToTitle();
		}
	}

	public void GoToTitle()
	{
		if (!_isInTransition && !(this == null))
		{
			_isInTransition = true;
			StartCoroutine(Routine());
		}
		static IEnumerator Routine()
		{
			if (ManagerBase<TransitionManager>.instance != null)
			{
				ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
				yield return new WaitForSecondsRealtime(ManagerBase<TransitionManager>.instance.fadeTime);
			}
			SceneManager.LoadScene("Title");
		}
	}

	public void StartNextScene(bool stopTime)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Time.timeScale = 1f;
			yield return new WaitForSecondsRealtime(0.1f);
			if (stopTime)
			{
				Time.timeScale = 0f;
			}
			fadeCg.DOKill();
			fadeCg.DOFade(0f, fadeDuration).SetUpdate(isIndependentUpdate: true);
		}
	}

	public void EndScene()
	{
		fadeCg.DOKill();
		fadeCg.DOFade(1f, fadeDuration).SetUpdate(isIndependentUpdate: true);
	}
}
