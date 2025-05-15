using UnityEngine;

public class Ai_C_Starfall_Projectile : StandardProjectile
{
	public ScalingValue dmgFactor;

	public float startHeight;

	public float procCoefficient;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(base.targetEntity.position + Vector3.up * startHeight);
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(dmgFactor, procCoefficient).SetElemental(ElementalType.Light).SetDirection(_estimatedVelocity).SetOriginPosition(base.info.caster.position)
			.Dispatch(hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
