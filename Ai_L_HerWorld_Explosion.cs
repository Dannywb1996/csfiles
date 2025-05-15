public class Ai_L_HerWorld_Explosion : InstantDamageInstance
{
	public float stunDuration = 2.5f;

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}
}
