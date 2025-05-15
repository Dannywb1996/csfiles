using UnityEngine;

public class Ai_C_FlashFreeze : InstantDamageInstance
{
	public GameObject fxEmpoweredHit;

	public float damageAmpChilled;

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
	}

	private void MirrorProcessed()
	{
	}
}
