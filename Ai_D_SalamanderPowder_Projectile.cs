using System;
using UnityEngine;

public class Ai_D_SalamanderPowder_Projectile : StandardProjectile
{
	public ScalingValue explosionDamage;

	public float onFireAmp = 1f;

	public float procCoefficient = 0.5f;

	public float deviateMag = 3f;

	private Vector3 _deviateVector;

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
		DamageData damageData = Damage(explosionDamage, procCoefficient).SetDirection(base.rotation).SetElemental(ElementalType.Fire);
		if (hit.entity.Status.fireStack > 0)
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
			damageData.ApplyAmplification(onFireAmp);
		}
		damageData.Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
