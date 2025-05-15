using System;

public class Ai_Gem_R_Ricochet : StandardProjectile
{
	[NonSerialized]
	public bool isHeal;

	[NonSerialized]
	public FinalDamageData originDamage;

	[NonSerialized]
	public FinalHealData originHeal;

	public ScalingValue ricochetRatio;

	protected override void OnEntity(EntityHit hit)
	{
	}

	private void MirrorProcessed()
	{
	}
}
