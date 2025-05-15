using System;
using UnityEngine;
using UnityEngine.AI;

public class Ai_C_PressurePoint : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public float stunDmgRatio;

	public GameObject punchEffect;

	public GameObject hitEffect;

	public GameObject trailEffect;

	public GameObject stunEffect;

	public float stunDuration;

	public float knockbackMaxDist;

	public float knockbackSpeed;

	public DewEase easeOnHitWall;

	public DewEase ease;

	public float wallStunDmg => GetValue(dmgFactor) * stunDmgRatio;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		DestroyOnDeath(base.info.caster);
		FxPlayNetworked(punchEffect);
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Length; i++)
		{
			Entity ent = entities[i];
			FxPlayNewNetworked(hitEffect, ent);
			FxPlayNewNetworked(trailEffect, ent);
			Damage(dmgFactor).SetDirection(base.rotation).Dispatch(ent);
			if (ent.Status.hasCrowdControlImmunity)
			{
				continue;
			}
			ent.Control.Rotate(-base.info.forward, immediately: true);
			Vector3 vector = ent.agentPosition + base.info.forward * knockbackMaxDist;
			if (NavMesh.Raycast(ent.agentPosition, vector, out var hit, -1))
			{
				vector = hit.position;
				ent.Control.StartDisplacement(new DispByDestination
				{
					destination = vector,
					canGoOverTerrain = false,
					duration = Vector2.Distance(hit.position.ToXY(), ent.agentPosition.ToXY()) / knockbackSpeed,
					ease = easeOnHitWall,
					isCanceledByCC = false,
					isFriendly = false,
					rotateForward = false,
					onFinish = delegate
					{
						Damage(dmgFactor).ApplyRawMultiplier(stunDmgRatio).SetOriginPosition(base.info.caster.agentPosition).SetDirection(base.info.forward)
							.Dispatch(ent);
						CreateBasicEffect(ent, new StunEffect(), stunDuration, "pressurepoint_stun", DuplicateEffectBehavior.UsePrevious);
						FxPlayNetworked(stunEffect, ent);
					}
				});
			}
			else
			{
				ent.Control.StartDisplacement(new DispByDestination
				{
					destination = vector,
					canGoOverTerrain = false,
					duration = knockbackMaxDist / knockbackSpeed,
					ease = ease,
					rotateForward = false,
					isCanceledByCC = false,
					isFriendly = false
				});
			}
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
