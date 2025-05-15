using UnityEngine;

public class Ai_E_FlameJet_Projectile : StandardProjectile
{
	public class Ad_FlameJet
	{
		public Entity caster;

		public float hitTime;
	}

	public float procCoefficient = 0.4f;

	public float minHitInterval = 0.5f;

	public GameObject hitEffect;

	public ScalingValue damage;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (hit.entity.TryFindData((Ad_FlameJet a) => a.caster == base.info.caster, out var result))
		{
			if (Time.time - result.hitTime < minHitInterval)
			{
				return;
			}
			result.hitTime = Time.time;
		}
		else
		{
			hit.entity.AddData(new Ad_FlameJet
			{
				caster = base.info.caster,
				hitTime = Time.time
			});
		}
		FxPlayNewNetworked(hitEffect, hit.entity);
		Damage(damage, procCoefficient).SetElemental(ElementalType.Fire).SetAttr(DamageAttribute.DamageOverTime).SetOriginPosition(base.info.caster.position)
			.Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
