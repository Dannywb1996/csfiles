using System;
using UnityEngine;

public class Ai_Gem_L_PureWhite_Projectile : StandardProjectile
{
	public ScalingValue damageMultiplier;

	public float procCoefficient = 0.3f;

	public float deviateMag = 3f;

	[NonSerialized]
	public FinalDamageData damageData;

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
		DefaultDamage(GetValue(damageMultiplier) * (damageData.amount + damageData.discardedAmount), procCoefficient).SetElemental(ElementalType.Light).SetAmountOrigin(damageData).Dispatch(hit.entity, chain);
	}

	private void MirrorProcessed()
	{
	}
}
