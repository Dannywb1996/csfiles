using System;
using UnityEngine;

public class MeleeAttackInstance : AbilityInstance
{
	public bool isCrit;

	public DewCollider range;

	public bool scaleRangeWithTriggerRange = true;

	public float mainTargetAttackEffect = 1f;

	public float mainTargetDamage = 1f;

	public float subTargetAttackEffect;

	public float subTargetDamage = 0.5f;

	public GameObject fxHitMain;

	public GameObject fxHitSub;

	[NonSerialized]
	public int hitCount;

	public bool didHit => hitCount > 0;

	protected override void OnCreate()
	{
		if (base.info.target == null)
		{
			base.transform.rotation = base.info.rotation;
		}
		else
		{
			Vector3 delta = base.info.target.agentPosition - base.info.caster.agentPosition;
			if (delta.sqrMagnitude > 0.1f)
			{
				base.transform.rotation = Quaternion.LookRotation(delta);
			}
		}
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		if (scaleRangeWithTriggerRange)
		{
			range.transform.localScale *= base.firstTrigger.configs[0].effectiveRange;
		}
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> targets = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		Entity mainTarget = base.info.target;
		if ((object)mainTarget == null && targets.Length > 0)
		{
			mainTarget = targets[0];
		}
		if (mainTarget != null)
		{
			hitCount++;
		}
		ReadOnlySpan<Entity> readOnlySpan = targets;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			if (!(readOnlySpan[i] == mainTarget))
			{
				hitCount++;
			}
		}
		if (mainTarget != null)
		{
			FxPlayNetworked(fxHitMain, mainTarget);
			DoBasicAttackHit(mainTarget, isCrit, isMain: true, mainTargetDamage, mainTargetAttackEffect);
		}
		readOnlySpan = targets;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Entity t = readOnlySpan[i];
			if (!(t == mainTarget))
			{
				FxPlayNetworked(fxHitSub, t);
				DoBasicAttackHit(t, isCrit, isMain: false, subTargetDamage, subTargetAttackEffect);
			}
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
