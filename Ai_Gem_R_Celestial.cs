using System.Collections;
using UnityEngine;

public class Ai_Gem_R_Celestial : AbilityInstance
{
	public ScalingValue starDamage;

	public float delay;

	public float procCoefficient = 0.5f;

	public GameObject hitEffect;

	public DewCollider range;

	public GameObject explodeEffect;

	public bool followTarget;

	protected override IEnumerator OnCreateSequenced()
	{
		base.position = base.info.target.Visual.GetBasePosition();
		if (base.isServer)
		{
			yield return new SI.WaitForSeconds(delay);
			Damage(starDamage, procCoefficient).SetOriginPosition(base.position).SetElemental(ElementalType.Light).Dispatch(base.info.target, chain);
			FxPlayNetworked(explodeEffect);
			FxPlayNewNetworked(hitEffect, base.info.target);
			Destroy();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (base.info.target != null && followTarget)
		{
			base.position = base.info.target.Visual.GetBasePosition();
		}
	}

	private void MirrorProcessed()
	{
	}
}
