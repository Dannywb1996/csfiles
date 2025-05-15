using UnityEngine;

public class Ai_C_GlacialStomp : InstantDamageInstance
{
	public float knockupStrength;

	public float stunDuration = 1f;

	public float shieldDuration = 3f;

	public ScalingValue perHitShield;

	protected override void OnHit(Entity entity)
	{
		entity.Visual.KnockUp(knockupStrength, isFriendly: false);
		base.OnHit(entity);
		GiveShield(base.info.caster, GetValue(perHitShield), shieldDuration);
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "glacialstomp_stun");
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		dmg.SetDirection(Vector3.up);
	}

	private void MirrorProcessed()
	{
	}
}
