using System;
using UnityEngine;

public class Ai_D_ExoticMatter_Projectile : StandardProjectile
{
	[NonSerialized]
	public int explosionStack;

	public ScalingValue damagePerStack;

	public GameObject explosionEffect;

	public GameObject explosionHitEffect;

	public DewCollider range;

	public float procCoefficient;

	public AnimationCurve scaleCurve;

	public AnimationCurve explodePitchCurve;

	public AnimationCurve explodeVolumeCurve;

	public Transform[] scaledTransforms;

	public DewAudioSource[] adjustedAudioSources;

	protected override void OnCreate()
	{
		float time = (float)explosionStack / (float)FindFirstOfType<Se_D_ExoticMatter>().maxStack;
		Transform[] array = scaledTransforms;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].localScale *= scaleCurve.Evaluate(time);
		}
		DewAudioSource[] array2 = adjustedAudioSources;
		foreach (DewAudioSource obj in array2)
		{
			obj.pitchMultiplier = explodePitchCurve.Evaluate(time);
			obj.volumeMultiplier = explodeVolumeCurve.Evaluate(time);
		}
		base.OnCreate();
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		FxPlayNewNetworked(explosionEffect, base.targetPosition, Quaternion.identity);
		base.transform.position = base.targetPosition;
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Length; i++)
		{
			Entity entity = entities[i];
			Damage(damagePerStack, procCoefficient).ApplyRawMultiplier(explosionStack).SetElemental(ElementalType.Light).SetOriginPosition(base.info.caster.position)
				.Dispatch(entity);
			FxPlayNewNetworked(explosionHitEffect, entity);
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
