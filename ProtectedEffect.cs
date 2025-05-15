using System;

public class ProtectedEffect : BasicEffect
{
	public Action<EventInfoDamageNegatedByImmunity> onDamageNegated;

	internal override BasicEffectMask _mask => BasicEffectMask.Protected;
}
