public class Ai_Curse_IntermittentExplosion_Explosion : InstantDamageInstance
{
	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && !base.info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			CreateBasicEffect(base.info.caster, new SlowEffect
			{
				decay = true,
				strength = 50f
			}, 1f, "IntermittentSlow", DuplicateEffectBehavior.UsePrevious);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		base.position = base.info.caster.agentPosition;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && Se_Curse_IntermittentExplosion.ShouldBeDestroyed())
		{
			Destroy();
		}
	}

	protected override bool ShouldUseDefaultAbilityTargetValidator()
	{
		return false;
	}

	protected override bool OnValidateTarget(Entity entity)
	{
		return base.info.caster != entity;
	}

	private void MirrorProcessed()
	{
	}
}
