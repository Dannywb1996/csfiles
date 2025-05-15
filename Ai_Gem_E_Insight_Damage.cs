using System.Collections;
using UnityEngine;

public class Ai_Gem_E_Insight_Damage : AbilityInstance
{
	public ScalingValue damage;

	public float procCoefficient = 0.75f;

	public GameObject hitEffect;

	internal float _delay;

	protected override IEnumerator OnCreateSequenced()
	{
		if (base.isServer)
		{
			yield return new SI.WaitForSeconds(_delay);
			if (base.info.caster == null || base.info.target == null)
			{
				Destroy();
				yield break;
			}
			Damage(damage, procCoefficient).SetElemental(ElementalType.Light).SetOriginPosition(base.info.caster.position).Dispatch(base.info.target, chain);
			FxPlayNewNetworked(hitEffect, base.info.target);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
