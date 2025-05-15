using System.Collections;
using UnityEngine;

public class Ai_Gem_L_SolarEye : AbilityInstance
{
	public float interval;

	public ScalingValue dmgFactor;

	public float procCoefficient;

	public GameObject fxHit;

	public GameObject fxInstance;

	[HideInInspector]
	public int totalStack;

	[HideInInspector]
	public int maxTickCount;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield break;
		}
		DestroyOnDeath(base.info.caster);
		Entity e = base.info.target;
		FxPlayNetworked(fxInstance, e);
		int stackPerTick = totalStack / maxTickCount;
		int exceededStack = totalStack % maxTickCount;
		int count = 0;
		while (count < maxTickCount && !e.IsNullInactiveDeadOrKnockedOut())
		{
			int overrideElementalStacks = stackPerTick;
			if (exceededStack > 0)
			{
				overrideElementalStacks = stackPerTick + 1;
				exceededStack--;
			}
			FxPlayNewNetworked(fxHit, e);
			Damage(dmgFactor, procCoefficient).SetElemental(ElementalType.Fire).SetOverrideElementalStacks(overrideElementalStacks).Dispatch(e);
			count++;
			if (count == maxTickCount - 3)
			{
				FxStopNetworked(fxInstance);
			}
			yield return new SI.WaitForSeconds(interval);
		}
		FxStopNetworked(fxInstance);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
