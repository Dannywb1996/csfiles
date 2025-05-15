using UnityEngine;

public class Ai_Atk_AurenaSwipe : MeleeAttackInstance
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (startEffectNoStop != null && DewAnimationClip.GetEntryIndex(base.info.animSelectValue, 2) == 1)
		{
			Vector3 localScale = startEffectNoStop.transform.localScale;
			localScale.x *= -1f;
			startEffectNoStop.transform.localScale = localScale;
		}
	}

	private void MirrorProcessed()
	{
	}
}
