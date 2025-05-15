using System;
using UnityEngine;

public class Ai_Gem_R_Dusk_Projectile : StandardProjectile
{
	public float procCoefficient = 1f;

	public ScalingValue damage;

	public float deviateMag = 3f;

	internal float _strength;

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
		Damage(damage, procCoefficient).ApplyStrength(_strength).SetDirection(base.rotation).SetElemental(ElementalType.Dark)
			.Dispatch(hit.entity, chain);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
