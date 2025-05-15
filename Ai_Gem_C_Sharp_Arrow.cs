using System;
using UnityEngine;

public class Ai_Gem_C_Sharp_Arrow : StandardProjectile
{
	public ScalingValue arrowDamage;

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
		Damage(arrowDamage, procCoefficient).SetOriginPosition(base.info.caster.position).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
