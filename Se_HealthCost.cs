using System;
using System.Collections;
using UnityEngine;

public class Se_HealthCost : StatusEffect
{
	[NonSerialized]
	public float totalAmount;

	public GameObject goldEffect;

	public GameObject blueEffect;

	public int ticks;

	public float tickInterval;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(base.victim.Visual.hasGoldDissolve ? goldEffect : blueEffect, base.victim);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(base.victim.Visual.hasGoldDissolve ? goldEffect : blueEffect);
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield break;
		}
		for (int i = 0; i < ticks; i++)
		{
			if (base.victim.IsNullInactiveDeadOrKnockedOut())
			{
				Destroy();
				yield break;
			}
			float amount = totalAmount / (float)ticks;
			amount = Mathf.Clamp(amount, 0f, base.victim.currentHealth - 1f);
			PureDamage(amount).SetAttr(DamageAttribute.IgnoreShield).SetAttr(DamageAttribute.IgnoreDamageImmunity).SetAttr(DamageAttribute.IgnoreArmor)
				.SetAttr(DamageAttribute.DamageOverTime)
				.Dispatch(base.victim);
			yield return new SI.WaitForSeconds(tickInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
