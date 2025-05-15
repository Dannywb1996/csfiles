using UnityEngine;

public static class ActivateDeactivateCanvasGroupExtensions
{
	public static void SetActivationState(this CanvasGroup cg, bool value)
	{
		cg.alpha = (value ? 1f : 0f);
		cg.interactable = value;
		cg.blocksRaycasts = value;
	}
}
