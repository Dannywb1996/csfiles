using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_Gem_R_Glaciate : AbilityInstance
{
	public float procCoefficient = 1f;

	public ScalingValue damage;

	public ScalingValue scale;

	public Transform[] scaledTransforms;

	public GameObject glaciateEffect;

	public GameObject hitEffect;

	public int sweepCount;

	public float sweepInterval;

	public DewCollider range;

	protected override IEnumerator OnCreateSequenced()
	{
		Transform[] array = scaledTransforms;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].localScale *= GetValue(scale);
		}
		FxPlay(glaciateEffect);
		if (!base.isServer)
		{
			yield break;
		}
		IEnumerable<List<Entity>> enumerable = range.SweepEntitiesFromOrigin(sweepCount, tvDefaultHarmfulEffectTargets);
		foreach (List<Entity> item in enumerable)
		{
			foreach (Entity item2 in item)
			{
				FxPlayNewNetworked(hitEffect, item2);
				Damage(damage, procCoefficient).SetElemental(ElementalType.Cold).SetOriginPosition(base.position).Dispatch(item2);
			}
			yield return new SI.WaitForSeconds(sweepInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
