using System;
using System.Collections;
using UnityEngine;

public class Se_GenericHealOverTime : StatusEffect
{
	public float totalAmount;

	public int ticks;

	public float tickInterval;

	public GameObject perHealEffect;

	public void Setup(float totalAmount, float tickInterval, int ticks)
	{
		this.totalAmount = totalAmount;
		this.tickInterval = tickInterval;
		this.ticks = ticks;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield break;
		}
		for (int i = 0; i < ticks; i++)
		{
			if (base.victim.Status.isDead || !base.victim.isActive || base.victim is Hero { isKnockedOut: not false })
			{
				Destroy();
				break;
			}
			FxPlayNewNetworked(perHealEffect, base.victim);
			if (ticks <= 1 || Math.Abs(base.victim.currentHealth - base.victim.maxHealth) > 0.0001f)
			{
				HealData heal = new HealData(totalAmount / (float)ticks);
				heal.SetCanMerge();
				DoHeal(heal, base.victim, chain);
			}
			if (i < ticks - 1)
			{
				yield return new SI.WaitForSeconds(tickInterval);
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
