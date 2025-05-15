using System;
using System.Collections;
using UnityEngine;

public class Ai_Gem_E_Thunder : AbilityInstance
{
	public float delay;

	public GameObject thunderEffect;

	public GameObject hitEffect;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public ScalingValue damage;

	protected override IEnumerator OnCreateSequenced()
	{
		if (base.isServer)
		{
			yield return new SI.WaitForSeconds(delay);
			FxPlayNetworked(thunderEffect);
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> entities = range.GetEntities(out handle, hittable, base.info.caster);
			for (int i = 0; i < entities.Length; i++)
			{
				Damage(damage).SetElemental(ElementalType.Light).Dispatch(entities[i]);
				FxPlayNewNetworked(hitEffect, entities[i]);
			}
			handle.Return();
			Destroy();
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (base.info.target != null)
		{
			base.transform.position = base.info.target.Visual.GetBasePosition();
		}
	}

	private void MirrorProcessed()
	{
	}
}
