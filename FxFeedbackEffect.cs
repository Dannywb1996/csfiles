using UnityEngine;

public class FxFeedbackEffect : MonoBehaviour, IEffectComponent
{
	public string effectName;

	public bool isPlaying => false;

	public void Play()
	{
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect(effectName);
	}

	public void Stop()
	{
	}
}
