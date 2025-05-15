using System;

public class AttackProjectile : StandardProjectile
{
	public bool isCrit;

	public float strength = 1f;

	[NonSerialized]
	public bool isMain = true;

	protected override void OnPrepare()
	{
		if ((object)base.info.target == null)
		{
			base.Networkmode = ProjectileMode.Direction;
			canCollideMidFlight = true;
			base.NetworkendDistance = base.firstTrigger.currentConfig.effectiveRange;
		}
		base.OnPrepare();
	}

	protected override void OnCreate()
	{
		if ((object)base.info.target == null)
		{
			base.Networkmode = ProjectileMode.Direction;
			canCollideMidFlight = true;
			base.NetworkendDistance = base.firstTrigger.currentConfig.effectiveRange;
		}
		base.OnCreate();
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DoBasicAttackHit(hit.entity, isCrit, isMain, strength, strength);
		DestroyIfActive();
	}

	private void MirrorProcessed()
	{
	}
}
