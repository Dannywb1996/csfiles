using UnityEngine;

public struct EntityAIContext
{
	public Vector3? targetEnemyLastKnownPosition;

	internal float _retargetingTime;

	internal float _targetEnemyLoseElapsedTime;

	internal float _targetEnemyStartTime;

	internal bool _insideAIUpdate;

	public Entity targetEnemy { get; internal set; }

	public float deltaTime { get; internal set; }

	public float targetEnemyElapsedTime
	{
		get
		{
			if (targetEnemy == null)
			{
				return 0f;
			}
			return Time.time - _targetEnemyStartTime;
		}
	}
}
