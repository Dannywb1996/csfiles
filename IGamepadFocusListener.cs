using UnityEngine;

public interface IGamepadFocusListener
{
	bool IsValid()
	{
		if (this is Component c)
		{
			return c != null;
		}
		return false;
	}

	void OnFocusStateChanged(bool state)
	{
	}
}
