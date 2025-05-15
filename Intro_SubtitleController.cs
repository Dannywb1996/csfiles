using System.Collections;
using TMPro;
using UnityEngine;

public class Intro_SubtitleController : MonoBehaviour
{
	public float minShowSpeed = 20f;

	public float showTime = 2f;

	public float showDurationAfterDone;

	public GameObject perCharacterEffect;

	public TextMeshProUGUI text;

	public GameObject subtitleObject;

	private int _nextSubtitle;

	private void Start()
	{
		subtitleObject.SetActive(value: false);
	}

	public void AdvanceSubtitle()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			subtitleObject.SetActive(value: true);
			text.text = DewLocalization.GetUIValue($"Intro_Subtitle_{_nextSubtitle}");
			_nextSubtitle++;
			int len = text.text.Length;
			for (int i = 1; i <= len; i += 2)
			{
				DewEffect.PlayNew(perCharacterEffect);
				text.maxVisibleCharacters = i;
				yield return new WaitForSecondsRealtime(Mathf.Min(showTime / (float)len, 1f / minShowSpeed));
			}
			text.maxVisibleCharacters = len;
			yield return new WaitForSecondsRealtime(showDurationAfterDone);
			subtitleObject.SetActive(value: false);
		}
	}
}
