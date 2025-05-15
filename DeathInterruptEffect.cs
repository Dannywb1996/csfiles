using System;

public class DeathInterruptEffect : BasicEffect
{
	public Action<EventInfoKill> onInterrupt;

	public int priority;

	internal override BasicEffectMask _mask => BasicEffectMask.DeathInterrupt;
}
