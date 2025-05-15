using UnityEngine;

public class ActionBase
{
	public bool isAllowedToMove;

	internal Entity _entity;

	internal float _addedUnscaledTime;

	internal bool _didDoFirstTick;

	public bool isFirstTick => !_didDoFirstTick;

	public virtual bool Tick()
	{
		return true;
	}

	public virtual Vector3? GetMoveDestination()
	{
		return null;
	}

	public virtual float GetMoveDestinationRequiredDistance()
	{
		return 0f;
	}

	public virtual bool ShouldCancelIfDisallowedToMove()
	{
		return false;
	}
}
