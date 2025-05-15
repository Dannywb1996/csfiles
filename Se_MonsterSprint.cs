using UnityEngine;

public class Se_MonsterSprint : StatusEffect
{
	public bool isDecay;

	public float duration;

	public float amount;

	public float destroyDistance;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			SetTimer(duration);
			DoSpeed(amount).decay = isDecay;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer)
		{
			Entity target = base.victim.AI.context.targetEnemy;
			if (target != null && Vector2.Distance(base.victim.agentPosition.ToXY(), target.agentPosition.ToXY()) < destroyDistance)
			{
				Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
