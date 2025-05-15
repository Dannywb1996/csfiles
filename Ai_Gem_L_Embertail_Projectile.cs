using System;
using UnityEngine;

public class Ai_Gem_L_Embertail_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public float procCoefficient = 0.5f;

	public float deviateMag = 4f;

	internal float _strengthMultiplier;

	private Vector3 _deviateVector;

	public GameObject attackEffect;

	internal bool isBounce;

	protected override void OnCreate()
	{
		_deviateVector = UnityEngine.Random.insideUnitSphere * (isBounce ? (deviateMag * 0.5f) : deviateMag);
		if (_deviateVector.y < 0f)
		{
			_deviateVector.y *= -1f;
		}
		base.OnCreate();
	}

	protected override Vector3 PositionSolver(float dt)
	{
		float num = (isBounce ? 0.65f : 1f);
		return base.PositionSolver(dt) + Mathf.Sin(base.normalizedPosition * MathF.PI) * _deviateVector * num;
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage, procCoefficient * _strengthMultiplier).SetElemental(ElementalType.Fire).SetDirection(base.rotation).ApplyRawMultiplier(_strengthMultiplier)
			.Dispatch(hit.entity, chain);
		if (attackEffect != null && hit.entity != null)
		{
			FxPlayNewNetworked(attackEffect, hit.entity);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
