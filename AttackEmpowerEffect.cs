using System;

public class AttackEmpowerEffect : BasicEffect
{
	public Action<EventInfoAttackEffect, int> onAttackEffect;

	public Action onDepleted;

	public int maxTriggerCount = int.MaxValue;

	internal int _nextIndex;

	internal override BasicEffectMask _mask => BasicEffectMask.AttackEmpower;
}
