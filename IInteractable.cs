using UnityEngine;

public interface IInteractable
{
	Transform interactPivot { get; }

	bool canInteractWithMouse { get; }

	float focusDistance { get; }

	int priority { get; }

	bool CanInteract(Entity entity);

	void OnInteract(Entity entity, bool alt);
}
