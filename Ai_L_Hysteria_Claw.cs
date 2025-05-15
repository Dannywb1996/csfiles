using System;
using UnityEngine;

public class Ai_L_Hysteria_Claw : InstantDamageInstance
{
	public ScalingValue healAmount;

	public float healAmpThreshold;

	public float healAmp;

	public GameObject fxLeft;

	public GameObject fxRight;

	public DewAnimationClip animLeft;

	public DewAnimationClip animRight;

	public Dash dash;

	[NonSerialized]
	public bool isRight;

	private bool _didHit;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			dash.ApplyByDirection(base.info.caster, base.info.forward);
			if (!isRight)
			{
				FxPlayNetworked(fxLeft);
				base.info.caster.Animation.PlayAbilityAnimation(animLeft);
			}
			else
			{
				FxPlayNetworked(fxRight);
				base.info.caster.Animation.PlayAbilityAnimation(animRight);
			}
		}
	}

	protected override void OnCollisionCheck()
	{
		base.OnCollisionCheck();
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, hittable, base.info.caster, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		for (int i = 0; i < entities.Length; i++)
		{
			Entity entity = entities[i];
			if (!IsDuplicate(entity) && OnValidateTarget(entity))
			{
				AddToDuplicateTracker(entity);
				OnHit(entity);
			}
		}
		handle.Return();
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (!_didHit)
		{
			_didHit = true;
			dmg.DoAttackEffect(AttackEffectType.Others);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		HealData healData = Heal(healAmount).SetCanMerge();
		if (base.info.caster.normalizedHealth < healAmpThreshold)
		{
			healData.SetCrit();
			healData.ApplyAmplification(healAmp);
		}
		healData.Dispatch(base.info.caster);
	}

	private void MirrorProcessed()
	{
	}
}
