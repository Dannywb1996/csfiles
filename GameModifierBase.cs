public class GameModifierBase : Actor, IExcludeFromPool
{
	public bool excludeFromPool;

	public override bool isDestroyedOnRoomChange => false;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public override bool ShouldBeSaved()
	{
		return false;
	}

	public virtual bool IsAvailableInGame()
	{
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
