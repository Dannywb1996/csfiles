using System;

public class AttackOverrideEffect : BasicEffect
{
	private AbilityTrigger _trigger;

	public Action onUse;

	internal bool _shouldTriggerBeDisposed;

	internal override BasicEffectMask _mask => BasicEffectMask.AttackOverride;

	public AbilityTrigger trigger
	{
		get
		{
			return _trigger;
		}
		set
		{
			_trigger = value;
			if (base.victim != null)
			{
				base.victim.Status.DirtyStatusInfo();
			}
		}
	}
}
