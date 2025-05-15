using System.Collections;
using UnityEngine;

public class Ai_C_IceClaw : InstantDamageInstance
{
	public Dash dash;

	public Knockback knockback;

	public float knockbackStartStrength;

	public float knockbackEndStrength;

	public float knockbackEndDistanceThreshold;

	public int nextConfig = -1;

	public float changeTime;

	public float stunDuration = 1f;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield return base.OnCreateSequenced();
			yield break;
		}
		if (nextConfig > 0)
		{
			base.firstTrigger.ChangeConfigTimedOnce(nextConfig, changeTime);
		}
		dash.ApplyByDirection(base.info.caster, base.info.forward);
		yield return base.OnCreateSequenced();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		float distance = knockback.distance;
		knockback.distance *= Mathf.Lerp(knockbackStartStrength, knockbackEndStrength, Vector3.Distance(base.position, entity.position) / knockbackEndDistanceThreshold);
		knockback.ApplyWithDirection(base.info.forward, entity);
		knockback.distance = distance;
		CreateBasicEffect(entity, new StunEffect(), stunDuration, "IceClawStun", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}
}
