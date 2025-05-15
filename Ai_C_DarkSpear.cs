using UnityEngine;

public class Ai_C_DarkSpear : InstantDamageInstance
{
	public float stunDuration;

	public bool displaceToCenter;

	public float displaceDuration;

	public float randomMagnitude;

	public DewEase ease;

	public float attackEffectStrength;

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		dmg.DoAttackEffect(AttackEffectType.Others, attackEffectStrength);
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (stunDuration > 0f)
		{
			CreateBasicEffect(entity, new StunEffect(), stunDuration, "darkspear_stun");
		}
		if (displaceToCenter)
		{
			float num = Vector3.Dot(entity.agentPosition - base.info.caster.position, base.info.forward);
			Vector3 end = base.info.caster.position + base.info.forward * num + Random.insideUnitSphere * randomMagnitude;
			end = Dew.GetValidAgentDestination_Closest(base.info.caster.position, end);
			entity.Control.StartDisplacement(new DispByDestination
			{
				canGoOverTerrain = false,
				affectedByMovementSpeed = false,
				destination = end,
				duration = displaceDuration,
				ease = ease,
				isFriendly = false,
				rotateForward = false,
				isCanceledByCC = false
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
