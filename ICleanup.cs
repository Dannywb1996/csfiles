public interface ICleanup
{
	bool canDestroy { get; }

	void OnCleanup();
}
