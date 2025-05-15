using System;
using UnityEngine;

public struct HealData
{
	internal ActorFlags _flags;

	public Actor actor { get; private set; }

	public float originalAmount { get; private set; }

	public bool isCrit { get; private set; }

	public float amplificationMultiplier { get; private set; }

	public float reductionMultiplier { get; private set; }

	public float flatModifier { get; private set; }

	public bool canMerge { get; private set; }

	public float currentAmount => Mathf.Max(0f, originalAmount + flatModifier) * amplificationMultiplier * reductionMultiplier;

	public HealData(float originalAmount)
	{
		this.originalAmount = originalAmount;
		isCrit = false;
		amplificationMultiplier = 1f;
		reductionMultiplier = 1f;
		flatModifier = 0f;
		canMerge = false;
		_flags = default(ActorFlags);
		actor = null;
	}

	public HealData SetCrit()
	{
		isCrit = true;
		return this;
	}

	public HealData SetCanMerge()
	{
		canMerge = true;
		return this;
	}

	public HealData ApplyAmplification(float value)
	{
		if (value < 0f)
		{
			throw new Exception("ApplyAmplification value cannot be a negative value.");
		}
		amplificationMultiplier *= 1f + value;
		return this;
	}

	public HealData ApplyReduction(float value)
	{
		reductionMultiplier *= 1f - Mathf.Clamp(value, 0f, 1f);
		return this;
	}

	public HealData AddAmount(float amount)
	{
		if (amount < 0f)
		{
			throw new Exception("AddAmount amount cannot be a negative value.");
		}
		flatModifier += amount;
		return this;
	}

	public HealData SubtractAmount(float amount)
	{
		if (amount < 0f)
		{
			throw new Exception("SubtractAmount amount cannot be a negative value.");
		}
		flatModifier -= amount;
		return this;
	}

	public HealData ApplyRawMultiplier(float multiplier)
	{
		originalAmount *= multiplier;
		return this;
	}

	public HealData SetActor(Actor actor)
	{
		this.actor = actor;
		return this;
	}

	public bool IsAmountModifiedBy(Actor actor)
	{
		return _flags.Contains(actor);
	}

	public bool IsAmountModifiedBy(Type actorType)
	{
		return _flags.Contains(actorType);
	}

	public HealData SetAmountModifiedBy(Actor actor)
	{
		_flags.Add(actor);
		return this;
	}

	public HealData SetAmountModifiedBy(Type actorType)
	{
		_flags.Add(actorType);
		return this;
	}

	public HealData SetAmountOrigin(FinalDamageData damage)
	{
		_flags = damage._modifyFlags.DeepCopy();
		isCrit |= damage.HasAttr(DamageAttribute.IsCrit);
		return this;
	}

	public HealData SetAmountOrigin(FinalHealData heal)
	{
		_flags = heal._flags.DeepCopy();
		isCrit |= heal.isCrit;
		return this;
	}

	public void Dispatch(Entity victim, ReactionChain chain = default(ReactionChain))
	{
		if (actor == null)
		{
			throw new InvalidOperationException("Actor not set");
		}
		actor.DoHeal(this, victim, chain);
	}
}
