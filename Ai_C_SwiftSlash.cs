using UnityEngine;

public class Ai_C_SwiftSlash : DashAttackInstance
{
	public float cooldownReductionRatio;

	public Transform knifeTransform;

	private bool _didHit;

	protected override void OnCreate()
	{
	}

	protected override void ActiveFrameUpdate()
	{
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
	}

	private void MirrorProcessed()
	{
	}
}
