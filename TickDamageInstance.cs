using System.Collections;
using UnityEngine;

public class TickDamageInstance : DamageInstance
{
	public float delay = 0.05f;

	public float tickInterval = 0.2f;

	public int ticks = 20;

	public GameObject fxTelegraph;

	public GameObject fxLoop;

	public GameObject fxPerTick;

	private float _lastTickTime;

	private int _doneTicks;

	public float duration => (float)ticks * tickInterval + delay;

	protected override IEnumerator OnCreateSequenced()
	{
		if (base.isServer)
		{
			_lastTickTime = Time.time + delay - tickInterval - 0.001f;
			FxPlayNetworked(fxTelegraph);
			if (delay > 0f)
			{
				yield return new SI.WaitForSeconds(delay);
			}
			FxPlayNetworked(fxLoop);
			FxStopNetworked(fxTelegraph);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			FxStopNetworked(fxTelegraph);
			FxStopNetworked(fxLoop);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer)
		{
			return;
		}
		if (CheckShouldBeDestroyed())
		{
			Destroy();
		}
		else if (_doneTicks < ticks && !(Time.time - _lastTickTime < tickInterval))
		{
			_lastTickTime += tickInterval;
			FxPlayNewNetworked(fxPerTick);
			DoCollisionChecks();
			_doneTicks++;
			if (_doneTicks >= ticks && destroyWhenDone)
			{
				Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
