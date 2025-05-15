using System;

public class AttackCriticalEffect : BasicEffect
{
	public Action onUse;

	internal override BasicEffectMask _mask => BasicEffectMask.AttackCritical;
}
