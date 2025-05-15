using System;
using System.Collections;
using UnityEngine;

public class Ai_C_BackStep_Bomb : StandardProjectile
{
	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback Knockback;

	public float procCoefficient;

	public GameObject hitEffect;

	public float stunDuration;

	protected override void OnComplete()
	{
		base.OnComplete();
		StartCoroutine(Routine());
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Length; i++)
		{
			Entity entity = entities[i];
			FxPlayNewNetworked(hitEffect, entity);
			Damage(dmgFactor, procCoefficient).SetOriginPosition(base.info.point).Dispatch(entity);
			Knockback.ApplyWithOrigin(base.info.point, entity);
			if (stunDuration > 0f)
			{
				CreateBasicEffect(entity, new StunEffect(), stunDuration, "backstepStun", DuplicateEffectBehavior.UsePrevious);
			}
		}
		handle.Return();
		static IEnumerator Routine()
		{
			yield return new WaitForSeconds(1f);
		}
	}

	private void MirrorProcessed()
	{
	}
}
