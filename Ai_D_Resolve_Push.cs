using System;
using UnityEngine;

public class Ai_D_Resolve_Push : AbilityInstance
{
	public DewCollider range;

	public Knockback knockback;

	public GameObject hitEffect;

	public float rotOverrideDuration;

	public float stunDuration;

	public ScalingValue maxHpRatio = ".04 .01x";

	public float minHealAmount;

	[NonSerialized]
	public MeleeAttackInstance attack;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		if (rotOverrideDuration > 0f)
		{
			base.info.caster.Control.Rotate(base.info.angle, immediately: true, rotOverrideDuration);
		}
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
		if (entities.Length > 0 || (attack != null && attack.didHit))
		{
			DoHeal(new HealData(Mathf.Max(minHealAmount, GetValue(maxHpRatio) * base.info.caster.maxHealth)), base.info.caster);
		}
		ReadOnlySpan<Entity> readOnlySpan = entities;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Entity entity = readOnlySpan[i];
			knockback.ApplyWithDirection(base.info.rotation, entity);
			if (stunDuration > 0f)
			{
				CreateBasicEffect(entity, new StunEffect(), stunDuration, "determination_stun");
			}
			FxPlayNewNetworked(hitEffect, entity);
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
