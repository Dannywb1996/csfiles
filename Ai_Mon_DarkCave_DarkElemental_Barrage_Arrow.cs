using System;
using UnityEngine;

public class Ai_Mon_DarkCave_DarkElemental_Barrage_Arrow : StandardProjectile
{
	public float startHeight;

	public DewCollider range;

	public ScalingValue damage;

	public GameObject telegraph;

	public GameObject hitEffect;

	public GameObject explodeEffect;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(base.info.caster.position + Vector3.up * startHeight);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlayNew(telegraph, base.info.point, Quaternion.identity);
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		FxPlayNewNetworked(explodeEffect, base.position, Quaternion.identity);
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Length; i++)
		{
			Entity entity = entities[i];
			Damage(damage).SetElemental(ElementalType.Dark).Dispatch(entity);
			FxPlayNewNetworked(hitEffect, entity);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
