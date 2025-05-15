using System;
using UnityEngine;

public class Ai_L_MentalCorruption_Projectile : StandardProjectile
{
	public ScalingValue initDamageAmount;

	public float cooldownRefundRatio = 0.5f;

	public DewCollider range;

	[NonSerialized]
	public bool isSecondary;

	public float deviateMag = 3f;

	private Vector3 _deviateVector;

	private bool _didSpread;

	protected override void OnCreate()
	{
		if (!isSecondary)
		{
			base.position = base.info.caster.Visual.GetCenterPosition();
		}
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
		hit.entity.EntityEvent_OnDeath += new Action<EventInfoKill>(RefundCooldown);
		Damage(initDamageAmount).SetOriginPosition(base.info.caster.position).SetElemental(ElementalType.Dark).Dispatch(hit.entity);
		CreateStatusEffect<Se_L_MentalCorruption>(hit.entity, new CastInfo(base.info.caster));
		if (!isSecondary && !_didSpread)
		{
			Spread();
		}
		Destroy();
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (!isSecondary && !_didSpread)
		{
			Spread();
		}
	}

	private void Spread()
	{
		_didSpread = true;
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Length; i++)
		{
			Entity entity = entities[i];
			if (!(entity == base.targetEntity))
			{
				CreateAbilityInstance(base.targetEntity.agentPosition, Quaternion.identity, new CastInfo(base.info.caster, entity), delegate(Ai_L_MentalCorruption_Projectile p)
				{
					p.isSecondary = true;
				});
			}
		}
		handle.Return();
	}

	private void RefundCooldown(EventInfoKill obj)
	{
		AbilityTrigger abilityTrigger = base.firstTrigger;
		if (!(abilityTrigger == null) && abilityTrigger.isActive && !(abilityTrigger.owner == null) && abilityTrigger.owner.isActive)
		{
			abilityTrigger.ApplyCooldownReductionByRatio(cooldownRefundRatio);
		}
	}

	private void MirrorProcessed()
	{
	}
}
