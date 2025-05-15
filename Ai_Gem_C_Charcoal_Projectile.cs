using System;
using UnityEngine;

public class Ai_Gem_C_Charcoal_Projectile : StandardProjectile
{
	public ScalingValue normalDamage;

	public ScalingValue fireDamage;

	public float procCoefficient = 0.5f;

	public GameObject fireHitEffect;

	public float deviateMag = 3f;

	private Vector3 _deviateVector;

	internal float _strength = 1f;

	protected override void OnCreate()
	{
		_deviateVector = UnityEngine.Random.insideUnitSphere * deviateMag;
		if (_deviateVector.y < 0f)
		{
			_deviateVector.y *= -1f;
		}
		base.OnCreate();
	}

	protected override Vector3 PositionSolver(float dt)
	{
		return base.PositionSolver(dt) + Mathf.Sin(base.normalizedPosition * MathF.PI) * _deviateVector;
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (hit.entity.Status.fireStack > 0)
		{
			MagicDamage(GetValue(fireDamage) * _strength, procCoefficient * _strength).SetDirection(base.rotation).SetElemental(ElementalType.Fire).SetAttr(DamageAttribute.ForceMergeNumber)
				.Dispatch(hit.entity, chain);
			FxPlayNewNetworked(fireHitEffect, hit.entity);
		}
		else
		{
			MagicDamage(GetValue(normalDamage), procCoefficient * _strength).SetDirection(base.rotation).SetAttr(DamageAttribute.ForceMergeNumber).Dispatch(hit.entity, chain);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
