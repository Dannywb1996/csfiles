using System;
using UnityEngine;

public struct DamageData
{
	[Flags]
	public enum SourceType
	{
		Default = 0,
		Physical = 1,
		Magic = 2,
		Pure = 4
	}

	public SourceType source;

	public ElementalType? elemental;

	internal ActorFlags _modifyFlags;

	public Actor actor { get; private set; }

	public float originalAmount { get; private set; }

	public float procCoefficient { get; private set; }

	public Vector3? direction { get; private set; }

	public Vector3? originPosition { get; private set; }

	public int? overrideElementalStacks { get; private set; }

	public bool isBlockedByImmunity { get; private set; }

	public float amplificationMultiplier { get; private set; }

	public float reductionMultiplier { get; private set; }

	public float flatModifier { get; private set; }

	public AttackEffectType attackEffectType { get; private set; }

	public float attackEffectStrength { get; private set; }

	public float currentAmount => Mathf.Max(0f, originalAmount + flatModifier) * amplificationMultiplier * reductionMultiplier;

	public DamageAttribute attributes { get; private set; }

	public DamageData(SourceType source, float originalAmount, float procCoefficient)
	{
		this.originalAmount = originalAmount;
		amplificationMultiplier = 1f;
		reductionMultiplier = 1f;
		flatModifier = 0f;
		this.source = source;
		direction = null;
		originPosition = null;
		elemental = null;
		actor = null;
		isBlockedByImmunity = false;
		attackEffectStrength = 0f;
		_modifyFlags = default(ActorFlags);
		this.procCoefficient = procCoefficient;
		attributes = DamageAttribute.None;
		attackEffectType = AttackEffectType.Others;
		overrideElementalStacks = null;
	}

	public DamageData(SourceType source, ScalingValue scalingValue, Entity from, int skillLevel, float procCoefficient)
	{
		float amount = scalingValue.GetValue(skillLevel, from);
		originalAmount = amount;
		amplificationMultiplier = 1f;
		reductionMultiplier = 1f;
		flatModifier = 0f;
		this.source = source;
		direction = null;
		originPosition = null;
		elemental = null;
		actor = null;
		isBlockedByImmunity = false;
		attackEffectStrength = 0f;
		_modifyFlags = default(ActorFlags);
		this.procCoefficient = procCoefficient;
		attributes = DamageAttribute.None;
		attackEffectType = AttackEffectType.Others;
		overrideElementalStacks = null;
	}

	public DamageData SetAttr(DamageAttribute attr)
	{
		attributes |= attr;
		return this;
	}

	public bool HasAttr(DamageAttribute attr)
	{
		return attributes.HasFlag(attr);
	}

	public DamageData BlockWithImmunity()
	{
		isBlockedByImmunity = true;
		return this;
	}

	public DamageData ApplyAmplification(float value)
	{
		if (value < 0f)
		{
			Debug.LogError(string.Format("{0} {1} cannot be a negative value: {2}", "ApplyAmplification", "value", value));
			return this;
		}
		amplificationMultiplier *= 1f + value;
		return this;
	}

	public DamageData SetDirection(Vector3 dir)
	{
		dir.Normalize();
		direction = dir;
		return this;
	}

	public DamageData DoAttackEffect(AttackEffectType type, float strength = 1f)
	{
		attackEffectType = type;
		attackEffectStrength = Mathf.Clamp01(strength);
		return this;
	}

	public DamageData SetDirection(Quaternion rot)
	{
		return SetDirection(rot * Vector3.forward);
	}

	public DamageData ApplyStrength(float multiplier)
	{
		ApplyRawMultiplier(multiplier);
		procCoefficient *= multiplier;
		return this;
	}

	public DamageData SetOriginPosition(Vector3 pos)
	{
		originPosition = pos;
		return this;
	}

	public DamageData SetElemental(ElementalType type)
	{
		elemental = type;
		return this;
	}

	public DamageData SetOverrideElementalStacks(int stacks)
	{
		overrideElementalStacks = stacks;
		return this;
	}

	public DamageData ApplyReduction(float value)
	{
		reductionMultiplier *= 1f - Mathf.Clamp(value, 0f, 1f);
		return this;
	}

	public DamageData AddFlatAmount(float amount)
	{
		if (amount < 0f)
		{
			throw new Exception("AddFlatAmount amount cannot be a negative value.");
		}
		flatModifier += amount;
		return this;
	}

	public DamageData SubtractFlatAmount(float amount)
	{
		if (amount < 0f)
		{
			throw new Exception("SubtractFlatAmount amount cannot be a negative value.");
		}
		flatModifier -= amount;
		return this;
	}

	public DamageData ApplyRawMultiplier(float multiplier)
	{
		originalAmount *= multiplier;
		return this;
	}

	public DamageData SetActor(Actor actor)
	{
		this.actor = actor;
		return this;
	}

	public bool IsAmountModifiedBy(Actor actor)
	{
		return _modifyFlags.Contains(actor);
	}

	public bool IsAmountModifiedBy(Type actorType)
	{
		return _modifyFlags.Contains(actorType);
	}

	public DamageData SetAmountModifiedBy(Actor actor)
	{
		_modifyFlags.Add(actor);
		return this;
	}

	public DamageData SetAmountModifiedBy(Type actorType)
	{
		_modifyFlags.Add(actorType);
		return this;
	}

	public DamageData SetAmountOrigin(FinalDamageData damage)
	{
		_modifyFlags = damage._modifyFlags.DeepCopy();
		attributes |= damage.attributes;
		return this;
	}

	public DamageData SetSourceType(SourceType type)
	{
		source = type;
		return this;
	}

	public DamageData SetAmountOrigin(FinalHealData heal)
	{
		_modifyFlags = heal._flags.DeepCopy();
		if (heal.isCrit)
		{
			attributes |= DamageAttribute.IsCrit;
		}
		return this;
	}

	public void Dispatch(Entity victim, ReactionChain chain = default(ReactionChain))
	{
		if (actor == null)
		{
			throw new InvalidOperationException("Actor not set");
		}
		actor.DealDamage(this, victim, chain);
	}
}
