using System;
using UnityEngine;

public class Ai_Gem_C_Void_Explosion : AbilityInstance
{
	public DewCollider range;

	public ScalingValue damage;

	public GameObject mainEffect;

	public GameObject hitEffect;

	[NonSerialized]
	public float strength;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(mainEffect);
		if (base.isServer)
		{
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Length; i++)
			{
				Entity entity = entities[i];
				Damage(damage).ApplyStrength(strength).SetElemental(ElementalType.Dark).Dispatch(entity, chain);
				FxPlayNewNetworked(hitEffect, entity);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
