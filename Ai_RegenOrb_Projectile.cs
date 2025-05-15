using System;
using UnityEngine;

public class Ai_RegenOrb_Projectile : StandardProjectile
{
	public float healRatio = 0.15f;

	public int ticks = 6;

	public float ticksInterval;

	public Action<Entity> actionOverride;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (actionOverride == null)
		{
			CreateStatusEffect(hit.entity, delegate(Se_GenericHealOverTime h)
			{
				h.Setup(healRatio * hit.entity.maxHealth, ticksInterval, ticks);
			});
		}
		else
		{
			try
			{
				actionOverride?.Invoke(hit.entity);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
