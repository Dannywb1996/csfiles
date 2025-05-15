using UnityEngine;

public static class IInteractableUnityNullCheckExtension
{
	public static bool IsUnityNull(this IInteractable interactable)
	{
		if (interactable is Object unityObj)
		{
			return unityObj == null;
		}
		return interactable == null;
	}
}
