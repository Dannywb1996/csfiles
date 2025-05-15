public interface IMenuView
{
	bool IsShowing();

	bool CanShowMenu();

	void ShowMenu();

	void HideMenu();
}
