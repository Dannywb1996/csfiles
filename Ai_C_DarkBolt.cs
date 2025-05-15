public class Ai_C_DarkBolt : StandardProjectile
{
	public ScalingValue damage;

	public float attackEffectStrength = 1f;

	public int penetrationCount = 1;

	private int _hitCount;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).SetDirection(base.rotation).DoAttackEffect(AttackEffectType.Others, attackEffectStrength).SetElemental(ElementalType.Dark)
			.Dispatch(hit.entity);
		_hitCount++;
		if (_hitCount > penetrationCount)
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
