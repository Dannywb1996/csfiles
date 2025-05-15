public class Se_Elm_Light : ElementalStatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			base.victim.takenDamageProcessor.Add(DamageAmplifier);
			base.victim.Status.lightStack = base.stack;
		}
	}

	private void DamageAmplifier(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && data.elemental == ElementalType.Light)
		{
			if (base.stack >= 3)
			{
				data.SetAttr(DamageAttribute.IsCrit);
			}
			Hero attacker = actor.FindFirstOfType<Hero>();
			float attackerAmp = 0f;
			if (attacker != null)
			{
				attackerAmp = attacker.Status.lightEffectAmp;
			}
			float amp = (float)base.stack * (0.05f + attackerAmp);
			data.ApplyAmplification(amp);
			data.SetAmountModifiedBy(this);
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if ((newStack != 0 || !base.victim.Status.isDead) && base.isServer)
		{
			base.victim.Status.lightStack = newStack;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && base.victim != null)
		{
			base.victim.takenDamageProcessor.Remove(DamageAmplifier);
			if (base.victim.Status.isAlive)
			{
				base.victim.Status.lightStack = 0;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
