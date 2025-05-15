using System;

public class Ai_Gem_R_Snow_Projectile : StandardProjectile
{
	public ScalingValue damage;

	public ScalingValue empoweredDamage;

	public float slowDuration;

	public float durationAmpOnEmpower;

	internal float _strength;

	[NonSerialized]
	public bool isEmpowered;

	public float slowAmount
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
	}

	private void MirrorProcessed()
	{
	}
}
