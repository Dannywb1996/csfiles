public class Ai_Gem_R_Spiral_Fireball : StandardProjectile
{
	public ScalingValue damage;

	public float procCoefficient = 0.5f;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage, procCoefficient).SetDirection(base.rotation).SetElemental(ElementalType.Fire).Dispatch(hit.entity);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
