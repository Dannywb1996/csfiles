using System;
using UnityEngine;

public class ShieldEffect : BasicEffect
{
	private float _amount;

	public Action<EventInfoDamageNegatedByShield> onDamageNegated;

	public Action<float, float> onAmountModified;

	internal override BasicEffectMask _mask => BasicEffectMask.Shield;

	public float amount
	{
		get
		{
			return _amount;
		}
		set
		{
			float from = _amount;
			if (from == value)
			{
				return;
			}
			_amount = value;
			if (base.victim != null)
			{
				base.victim.Status.DirtyStatusInfo();
			}
			try
			{
				onAmountModified?.Invoke(from, value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
