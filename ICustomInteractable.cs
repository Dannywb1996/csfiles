using UnityEngine;

public interface ICustomInteractable
{
	string nameRawText => null;

	string interactActionRawText => null;

	bool canAltInteract => false;

	string interactAltActionRawText => null;

	float? altInteractProgress => null;

	Vector3 worldOffset => new Vector3(0f, 4f, 0f);

	Cost cost => default(Cost);
}
