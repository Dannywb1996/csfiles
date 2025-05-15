using System;

public class Se_GenericShield_OneShot : StatusEffect
{
	public ShieldEffect shield = new ShieldEffect();

	public bool isDecay = true;

	private float _lastNormalizedDuration;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			if (shield.amount < 0.0001f)
			{
				Destroy();
				return;
			}
			ShieldEffect shieldEffect = shield;
			shieldEffect.onDamageNegated = (Action<EventInfoDamageNegatedByShield>)Delegate.Combine(shieldEffect.onDamageNegated, new Action<EventInfoDamageNegatedByShield>(OnDamageNegated));
			DoBasicEffect(shield);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && isDecay && base.normalizedDuration.HasValue)
		{
			if (base.normalizedDuration.Value < _lastNormalizedDuration)
			{
				shield.amount = shield.amount * base.normalizedDuration.Value / _lastNormalizedDuration;
			}
			_lastNormalizedDuration = base.normalizedDuration.Value;
		}
	}

	private void OnDamageNegated(EventInfoDamageNegatedByShield obj)
	{
		if (obj.shield.amount < 0.0001f)
		{
			DestroyIfActive();
		}
	}

	private void MirrorProcessed()
	{
	}
}
