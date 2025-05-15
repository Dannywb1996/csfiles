using System;
using UnityEngine;

public class Ai_C_BeamOfLight : AbilityInstance
{
	public float targetRadius = 8f;

	public float beamInterval = 0.01666667f;

	public GameObject beamEffect;

	public GameObject targetEffect;

	public GameObject handEffect;

	public GameObject healEffect;

	public LineRenderer lineRenderer;

	public DewAnimationClip canceledClip;

	public float maxDistance;

	public float selfSlow;

	public AbilityTargetValidator hittable;

	public float tickInterval;

	public ScalingValue ticks;

	public ScalingValue tickDamage;

	public ScalingValue tickHeal;

	public float procCoefficient = 0.25f;

	private float _lastTickTime;

	private int _ticksDone;

	private int _maxTicks;

	private bool _atTarget;

	private float _lastBeamInterval;

	private StatusEffect _slow;

	private AbilityTrigger _firstTrigger;

	public GameObject bossHealEffect;

	protected override void OnCreate()
	{
		base.OnCreate();
		beamEffect.transform.position = base.info.caster.Visual.GetBonePosition(HumanBodyBones.RightHand);
		_atTarget = false;
		_lastBeamInterval = Time.time;
		lineRenderer.SetPosition(0, base.info.caster.Visual.GetBonePosition(HumanBodyBones.RightHand));
		lineRenderer.SetPosition(1, base.info.target.Visual.GetCenterPosition());
		FxPlay(beamEffect);
		FxPlay(targetEffect, base.info.target);
		FxPlay(handEffect, base.info.caster);
		lineRenderer.enabled = true;
		if (!base.isServer)
		{
			return;
		}
		_firstTrigger = base.firstTrigger;
		DestroyOnDeath(base.info.caster);
		_maxTicks = Mathf.RoundToInt(GetValue(ticks));
		float duration = tickInterval * (10f / (9f + (float)base.skillLevel)) * (float)(_maxTicks - 1);
		base.info.caster.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack | Channel.BlockedAction.Cancelable),
			duration = duration,
			onCancel = delegate
			{
				if (base.isActive)
				{
					Destroy();
				}
			},
			uncancellableTime = 0.5f
		}.AddValidation(AbilitySelfValidator.Default));
		if (selfSlow > 0f)
		{
			float strength = selfSlow * (16f / (15f + (float)base.skillLevel));
			_slow = CreateBasicEffect(base.info.caster, new SlowEffect
			{
				strength = strength
			}, duration, "purification");
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!(base.info.target == null))
		{
			if (Time.time - _lastBeamInterval > beamInterval)
			{
				_atTarget = !_atTarget;
				beamEffect.transform.position = (_atTarget ? base.info.target.Visual.GetCenterPosition() : base.info.caster.Visual.GetBonePosition(HumanBodyBones.RightHand));
				_lastBeamInterval = Time.time;
			}
			lineRenderer.SetPosition(0, base.info.caster.Visual.GetBonePosition(HumanBodyBones.RightHand));
			lineRenderer.SetPosition(1, base.info.target.Visual.GetCenterPosition());
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer)
		{
			return;
		}
		if (_firstTrigger != null)
		{
			_firstTrigger.fillAmount = 1f - (float)_ticksDone / (float)_maxTicks;
		}
		if (base.info.target != null)
		{
			base.info.caster.Control.RotateTowards(base.info.target.position, immediately: false, 0.1f);
		}
		float num = tickInterval * (16f / (15f + (float)base.skillLevel));
		if (Time.time - _lastTickTime < num)
		{
			return;
		}
		_lastTickTime = Time.time;
		_ticksDone++;
		float radius = targetRadius * (1f + 0.16f * (float)base.skillLevel);
		float num2 = maxDistance * (1f + 0.32f * (float)base.skillLevel);
		float radius2 = 15f * (1f + 0.16f * (float)base.skillLevel);
		if (base.info.target.IsNullInactiveDeadOrKnockedOut() || !hittable.Evaluate(base.info.caster, base.info.target))
		{
			if (_ticksDone >= _maxTicks)
			{
				Destroy();
				return;
			}
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> readOnlySpan = DewPhysics.OverlapCircleAllEntities(out handle, base.info.caster.agentPosition, radius, tvDefaultHarmfulEffectTargets);
			if (readOnlySpan.Length <= 0)
			{
				handle.Return();
				Destroy();
				return;
			}
			float target = ((base.info.target != null) ? CastInfo.GetAngle(base.info.target.position - base.info.caster.position) : base.info.caster.rotation.eulerAngles.y);
			Entity entity = null;
			float num3 = float.NegativeInfinity;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				Entity entity2 = readOnlySpan[i];
				float num4 = 0f - Mathf.Abs(Mathf.DeltaAngle(CastInfo.GetAngle(entity2.position - base.info.caster.position), target)) + ((base.info.caster.GetRelation(entity2) == EntityRelation.Ally) ? (-1000f) : 0f);
				if (num3 <= num4)
				{
					num3 = num4;
					entity = entity2;
				}
			}
			if (entity == null)
			{
				handle.Return();
				Destroy();
				return;
			}
			CastInfo castInfo = base.info;
			castInfo.target = entity;
			base.info = castInfo;
			handle.Return();
			FxStopNetworked(targetEffect);
			FxPlayNetworked(targetEffect, base.info.target);
		}
		bool flag = base.info.target.IsAnyBoss();
		float num5 = Mathf.Clamp01((1f - base.info.caster.currentHealth / base.info.caster.maxHealth) / 0.7f);
		DamageData damageData = Damage(tickDamage, procCoefficient).SetElemental(ElementalType.Light).SetOriginPosition(base.info.caster.position).SetAttr(DamageAttribute.DamageOverTime);
		float num6 = 1f - (float)((90.0 + 10.0 * (1.0 - 1.0 / Math.Pow(2.0, Math.Min(100, NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex)))) / 100.0);
		float amount = (flag ? (GetValue(tickHeal) * 3.6f / num6) : GetValue(tickHeal));
		HealData healData = Heal(amount);
		if (num5 > 0.35f)
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
			healData.SetCrit();
		}
		if (flag)
		{
			damageData.SetAttr(DamageAttribute.IsCrit);
			healData.SetCrit();
		}
		damageData.Dispatch(base.info.target);
		ArrayReturnHandle<Entity> handle2;
		ReadOnlySpan<Entity> readOnlySpan2 = DewPhysics.OverlapCircleAllEntities(out handle2, base.info.caster.agentPosition, radius2, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true,
			sortComparer = CollisionCheckSettings.Random
		});
		Entity entity3 = base.info.caster;
		for (int j = 0; j < readOnlySpan2.Length; j++)
		{
			Entity entity4 = readOnlySpan2[j];
			if (entity4.normalizedHealth - 0.0001f < entity3.normalizedHealth)
			{
				entity3 = entity4;
			}
		}
		if (!entity3.IsNullInactiveDeadOrKnockedOut())
		{
			healData.Dispatch(entity3);
			FxPlayNewNetworked(healEffect, entity3);
			if (flag)
			{
				FxPlayNewNetworked(bossHealEffect, entity3);
			}
		}
		handle2.Return();
		if (_ticksDone >= _maxTicks)
		{
			Destroy();
		}
		else if (Vector3.Distance(base.info.caster.position, base.info.target.position) > num2)
		{
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			FxStopNetworked(beamEffect);
			FxStopNetworked(targetEffect);
			FxStopNetworked(handEffect);
		}
		lineRenderer.enabled = false;
		if (base.isServer && base.info.caster != null)
		{
			base.info.caster.Animation.StopAbilityAnimation(canceledClip);
			if (_slow != null && _slow.isActive)
			{
				_slow.Destroy();
			}
		}
		if (base.isServer && _firstTrigger != null)
		{
			_firstTrigger.fillAmount = 0f;
		}
	}
}
