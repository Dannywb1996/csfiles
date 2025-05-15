using System;
using UnityEngine;

public class Se_HunterBuff : StatusEffect
{
	public float populationMultiplier = 2f;

	public float movementSpeedPercentage = 50f;

	public float attackSpeedPercentage = 50f;

	public float abilityHaste = 50f;

	public float shadowWalkFarChance = 0.25f;

	public float shadowWalkCloseChance = 0.25f;

	public Vector2 shadowWalkInterval;

	public AbilitySelfValidator shadowWalkValidator;

	public float shadowWalkDistanceThresholdNoAttack = 4f;

	public float shadowWalkDistanceThresholdOffset = 1f;

	[NonSerialized]
	public bool enableGoldAndExpDrops;

	private float _currentShadowWalkInterval;

	private float _lastShadowWalkTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		Monster m = base.victim as Monster;
		if (m != null)
		{
			m.Visual.deathBehavior = EntityVisual.EntityDeathBehavior.HideModel;
			m.Visual.deathEffect = null;
			m.Sound.voiceDeath = null;
		}
		if (base.isServer)
		{
			if (m != null)
			{
				m.isHunter = true;
				m.populationCost *= populationMultiplier;
			}
			base.victim.Status.AddStatBonus(new StatBonus
			{
				movementSpeedPercentage = movementSpeedPercentage,
				attackSpeedPercentage = attackSpeedPercentage,
				abilityHasteFlat = abilityHaste
			});
			_lastShadowWalkTime = Time.time;
			_currentShadowWalkInterval = UnityEngine.Random.Range(shadowWalkInterval.x, shadowWalkInterval.y);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer || !(Time.time - _lastShadowWalkTime > _currentShadowWalkInterval))
		{
			return;
		}
		_lastShadowWalkTime = Time.time;
		_currentShadowWalkInterval = UnityEngine.Random.Range(shadowWalkInterval.x, shadowWalkInterval.y);
		if (base.victim.Control.IsActionBlocked(EntityControl.BlockableAction.Ability) != EntityControl.BlockStatus.Allowed || !shadowWalkValidator.Evaluate(base.victim) || base.victim.Control.attackTarget == null || base.victim.Control.ongoingChannels.Count > 0)
		{
			return;
		}
		float distanceThreshold = ((!(base.victim.Ability.attackAbility != null)) ? shadowWalkDistanceThresholdNoAttack : (base.victim.Ability.attackAbility.currentConfig.effectiveRange + shadowWalkDistanceThresholdOffset));
		Vector3 targetPos = base.victim.Control.attackTarget.position;
		if (Vector3.Distance(base.victim.position, targetPos) < distanceThreshold)
		{
			if (UnityEngine.Random.value > shadowWalkCloseChance)
			{
				return;
			}
		}
		else if (UnityEngine.Random.value > shadowWalkFarChance)
		{
			return;
		}
		CreateAbilityInstance<Ai_HunterBuff_ShadowWalk>(base.victim.position, base.victim.rotation, new CastInfo(base.victim, base.victim.Control.attackTarget));
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.victim is Monster { isActive: not false } m)
		{
			m.isHunter = false;
		}
	}

	private void MirrorProcessed()
	{
	}
}
