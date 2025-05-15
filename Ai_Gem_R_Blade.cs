using UnityEngine;

public class Ai_Gem_R_Blade : AbilityInstance
{
	public ScalingValue dmgFactor;

	public float procCoefficient;

	public GameObject slashEffect;

	public GameObject hitEffect;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer && !base.info.target.IsNullInactiveDeadOrKnockedOut())
		{
			FxPlayNetworked(slashEffect, base.info.target);
			Damage(dmgFactor, procCoefficient).Dispatch(base.info.target, chain);
			FxPlayNetworked(hitEffect, base.info.target);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
