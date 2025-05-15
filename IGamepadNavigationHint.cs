public interface IGamepadNavigationHint
{
	bool TryGetUp(out IGamepadFocusable next);

	bool TryGetLeft(out IGamepadFocusable next);

	bool TryGetRight(out IGamepadFocusable next);

	bool TryGetDown(out IGamepadFocusable next);
}
