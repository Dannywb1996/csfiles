using UnityEngine;

public class Room_Trap_CyclicActivator : Room_Trap_ActivatorBase
{
	public float cycleInterval = 3f;

	public float[] activateTimes = new float[1];

	public float minDistanceFromPlayerToBeActive = 25f;

	public float timeScale = 1f;

	private float _currentCycleTime = float.NaN;

	private float _lastTickTime;

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.isServer || NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
		{
			return;
		}
		dt *= timeScale;
		float time = Time.time * timeScale;
		if (float.IsNaN(_currentCycleTime))
		{
			_currentCycleTime = time;
			_lastTickTime = _currentCycleTime - dt;
		}
		float[] array = activateTimes;
		foreach (float at in array)
		{
			float currentActivationTime = _currentCycleTime + at;
			if (time > currentActivationTime && currentActivationTime >= _lastTickTime && Dew.GetClosestHeroDistance(base.transform.position) < minDistanceFromPlayerToBeActive)
			{
				Activate();
			}
		}
		if (time - _currentCycleTime >= cycleInterval)
		{
			_currentCycleTime += cycleInterval;
			_lastTickTime = _currentCycleTime;
		}
		else
		{
			_lastTickTime = time;
		}
	}

	private void MirrorProcessed()
	{
	}
}
