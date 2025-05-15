using System;
using UnityEngine;

public class Ai_L_Multishot_Arrow : StandardProjectile
{
	[NonSerialized]
	public bool triggerAttackEffect;

	[NonSerialized]
	public float strength;

	public ScalingValue arrowDamage;

	public float procCoefficient = 0.35f;

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
		DamageData damageData = Damage(arrowDamage, procCoefficient).SetOriginPosition(base.info.caster.position).ApplyStrength(strength);
		if (triggerAttackEffect)
		{
			damageData.DoAttackEffect(AttackEffectType.Others, strength);
		}
		damageData.Dispatch(hit.entity, chain);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
