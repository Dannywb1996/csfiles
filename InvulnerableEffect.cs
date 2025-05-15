using System;

public class InvulnerableEffect : BasicEffect
{
	public Action<EventInfoDamageNegatedByImmunity> onDamageNegated;

	internal override BasicEffectMask _mask => BasicEffectMask.Invulnerable;
}
