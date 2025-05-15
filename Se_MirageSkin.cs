using System;
using UnityEngine;

public class Se_MirageSkin : StatusEffect
{
	public GameObject fxDamage;

	public GameObject fxDamageOverTime;

	public GameObject fxBreak;

	public float fxDamagedCooldown;

	public float breakStunDuration;

	public float maxHealthRatio;

	public float shieldMaxHealthRatio;

	[NonSerialized]
	public float? customAmount;

	private float _lastDamageEffectTime;

	private float _lastDamageOverTimeEffectTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		DoUnstoppable();
		base.victim.Status.CalculateStats();
		float shieldAmount;
		if (customAmount.HasValue)
		{
			shieldAmount = customAmount.Value;
		}
		else
		{
			shieldAmount = base.victim.maxHealth * shieldMaxHealthRatio;
			base.victim.Status.AddStatBonus(new StatBonus
			{
				maxHealthPercentage = maxHealthRatio * 100f - 100f
			});
		}
		base.victim.Status.mirageSkinInitAmount = shieldAmount;
		DoShield(shieldAmount, delegate(EventInfoDamageNegatedByShield obj)
		{
			bool flag = obj.damage.HasAttr(DamageAttribute.DamageOverTime);
			if (flag && Time.time - _lastDamageOverTimeEffectTime > fxDamagedCooldown)
			{
				FxPlayNewNetworked(fxDamageOverTime, base.victim);
			}
			else if (!flag && Time.time - _lastDamageEffectTime > fxDamagedCooldown)
			{
				FxPlayNewNetworked(fxDamage, base.victim);
			}
			if (base.isActive && obj.shield.amount < 0.0001f)
			{
				FxPlayNetworked(fxBreak, base.victim);
				Destroy();
				base.victim.Status.UpdateStatusInfo();
				if (breakStunDuration > 0.001f)
				{
					base.victim.Stagger(obj.damage.direction);
					CreateBasicEffect(base.victim, new StunEffect(), breakStunDuration, "MirageSkinBreakStun");
				}
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
