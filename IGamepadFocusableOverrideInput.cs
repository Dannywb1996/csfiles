public interface IGamepadFocusableOverrideInput
{
	void OnGamepadUpdate()
	{
	}

	bool OnGamepadDpadUp()
	{
		return false;
	}

	bool OnGamepadDpadLeft()
	{
		return false;
	}

	bool OnGamepadDpadRight()
	{
		return false;
	}

	bool OnGamepadDpadDown()
	{
		return false;
	}

	bool OnGamepadConfirm()
	{
		return false;
	}

	bool OnGamepadBack()
	{
		return false;
	}
}
