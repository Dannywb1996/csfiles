using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_E_ChainLightning : StandardProjectile
{
	public int maxChainCount = 6;

	public float chainDelay;

	public DewCollider chainRange;

	public ScalingValue firstDamage;

	public ScalingValue chainDamage;

	public float firstProcCoefficient = 1f;

	public float chainProcCoefficient = 0.75f;

	public bool canChainToSelf;

	public GameObject firstEffect;

	private Dictionary<Entity, int> _affectedEntities;

	private int _chainedCount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer && _chainedCount <= 0)
		{
			FxPlayNetworked(firstEffect);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		StartSequence(Sequence());
		IEnumerator Sequence()
		{
			Damage((_chainedCount <= 0) ? firstDamage : chainDamage, (_chainedCount <= 0) ? firstProcCoefficient : chainProcCoefficient).SetElemental(ElementalType.Light).Dispatch(base.info.target);
			if (_chainedCount >= maxChainCount)
			{
				Destroy();
			}
			else
			{
				yield return new SI.WaitForSeconds(chainDelay);
				if (_affectedEntities == null)
				{
					_affectedEntities = new Dictionary<Entity, int>();
				}
				if (!_affectedEntities.ContainsKey(base.info.target))
				{
					_affectedEntities.Add(base.info.target, 1);
				}
				else
				{
					_affectedEntities[base.info.target]++;
				}
				ArrayReturnHandle<Entity> handle;
				ReadOnlySpan<Entity> entities = chainRange.GetEntities(out handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.DistanceFromCenter
				});
				Entity entity = null;
				float num = float.NegativeInfinity;
				for (int i = 0; i < entities.Length; i++)
				{
					Entity entity2 = entities[i];
					if (!(entity2 == base.info.target) || canChainToSelf)
					{
						float num2 = (float)(_affectedEntities.GetValueOrDefault(entity2, 0) * -1000) + Vector3.SqrMagnitude(base.position - entity2.position);
						if (num < num2)
						{
							entity = entity2;
							num = num2;
						}
					}
				}
				if (entity == null)
				{
					Destroy();
				}
				else
				{
					CreateAbilityInstance(base.info.target.Visual.GetCenterPosition(), Quaternion.identity, new CastInfo(base.info.caster, entity), delegate(Ai_E_ChainLightning c)
					{
						c._affectedEntities = _affectedEntities;
						c._chainedCount = _chainedCount + 1;
					});
					Destroy();
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
