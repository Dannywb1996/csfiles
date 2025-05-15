using UnityEngine;

public class Ai_Gem_L_SuppressedArcanum : AbilityInstance
{
	public GameObject fxHit;

	public DewCollider range;

	public ScalingValue damage;

	public ScalingValue shieldAmount;

	public float shieldDuration;

	public ScalingValue currentHpSacrificeRatio;

	protected override void OnCreate()
	{
	}

	private void MirrorProcessed()
	{
	}
}
