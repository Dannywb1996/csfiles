using UnityEngine;

public class Ai_MiniBoss_SpinningArrow_Arrow : StandardProjectile
{
	private class Ad_SpinningArrowHit
	{
		public float lastMainHitTime;
	}

	public ScalingValue dmg;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			DestroyOnDeath(base.info.caster);
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (!hit.entity.TryGetData<Ad_SpinningArrowHit>(out var data))
		{
			data = new Ad_SpinningArrowHit();
			hit.entity.AddData(data);
		}
		float num = 1f;
		if (Time.time - data.lastMainHitTime > 2f)
		{
			num *= 1.65f;
			data.lastMainHitTime = Time.time;
		}
		else
		{
			num *= 0.35f;
		}
		Damage(dmg).ApplyRawMultiplier(num).SetDirection(base.rotation).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
