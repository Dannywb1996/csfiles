using UnityEngine;

public class Ai_E_MysticDagger : StandardProjectile
{
	public GameObject fxReduceCooldownOnCaster;

	public ScalingValue damage;

	public ScalingValue cooldownReduction;

	public float reducedAmount
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
