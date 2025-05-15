using System;

public class Ai_Gem_R_Accuracy_Projectile : StandardProjectile
{
	public ScalingValue damageRatio;

	[NonSerialized]
	public FinalDamageData data;

	protected override void OnEntity(EntityHit hit)
	{
	}

	private void MirrorProcessed()
	{
	}
}
