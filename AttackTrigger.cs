using UnityEngine;

public class AttackTrigger : AbilityTrigger
{
	public bool allowNonTargetedCast = true;

	public ScalingValue factor = new ScalingValue(0f, 1f, 0f, 0f);

	private int _nextEveryFourAttackIndex;

	private float _randomValue;

	protected override void Awake()
	{
		base.Awake();
		_randomValue = Random.value;
	}

	public override float GetCooldownTimeMultiplier(int configIndex)
	{
		if (base.owner == null)
		{
			return 1f;
		}
		return 1f / base.owner.Status.attackSpeedMultiplier;
	}

	public override float GetAnimationSpeed()
	{
		if (base.owner == null)
		{
			return 1f;
		}
		return base.owner.Status.attackSpeedMultiplier;
	}

	public override float GetChannelDurationMultiplier()
	{
		if (base.owner == null)
		{
			return 1f;
		}
		return 1f / base.owner.Status.attackSpeedMultiplier;
	}

	public override float GetPostDelayDurationMultiplier()
	{
		if (base.owner == null)
		{
			return 1f;
		}
		return 1f / base.owner.Status.attackSpeedMultiplier;
	}

	public void UpdateConfigIndexForCrit()
	{
		if (configs.Length > 1 && (base.owner.Status.hasAttackCritical || _randomValue < base.owner.Status.critChance))
		{
			base.currentConfigIndex = 1;
		}
		else if (!base.owner.Status.hasAttackCritical)
		{
			base.currentConfigIndex = 0;
		}
	}

	public override void OnCastStart(int configIndex, CastInfo info)
	{
		UpdateConfigIndexForCrit();
		base.OnCastStart(base.currentConfigIndex, info);
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance newInstance = base.OnCastComplete(configIndex, info);
		base.owner.EntityEvent_OnAttackFired?.Invoke(new EventInfoAttackFired
		{
			actor = this,
			instance = newInstance,
			info = info,
			isCrit = (configIndex == 1),
			everyFourAttackIndex = _nextEveryFourAttackIndex
		});
		_nextEveryFourAttackIndex++;
		if (_nextEveryFourAttackIndex > 3)
		{
			_nextEveryFourAttackIndex = 0;
		}
		_randomValue = Random.value;
		return newInstance;
	}

	public override void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
		base.OnCastCompleteBeforePrepare(cast);
		base.owner.EntityEvent_OnAttackFiredBeforePrepare?.Invoke(new EventInfoAttackFired
		{
			actor = this,
			instance = cast.instance,
			info = cast.info,
			isCrit = (cast.configIndex == 1),
			everyFourAttackIndex = _nextEveryFourAttackIndex
		});
	}

	public override void OnCastCompleteSetMinimumDelay(int configIndex, CastInfo info)
	{
		SetMinimumDelayAll(configs[configIndex].minimumDelay);
	}

	public override void OnCastCompleteSetCooldownTime(int configIndex, CastInfo info)
	{
		SetCooldownTimeAll(GetMaxCooldownTime(configIndex, scaled: false) - configs[configIndex].channel.duration, scaled: false);
	}

	public override void OnCastCompleteSetCharge(int configIndex, CastInfo info)
	{
		SetChargeAll(currentCharges[configIndex] - 1);
	}

	protected override void OnCastCancelSetCooldownTime(int configIndex, CastInfo info)
	{
		SetCooldownTimeAll(0f, scaled: false);
	}

	protected override void OnStartChannel(int configIndex, CastInfo info)
	{
		TriggerConfig config = configs[configIndex];
		Channel channel = ((config.castMethod.type != CastMethodType.Target || (object)info.target == null) ? config.channel.CreateChannel(delegate
		{
			OnCastComplete(configIndex, info);
		}, delegate
		{
			OnCastCancel(configIndex, info);
		}, config.selfValidator) : config.channel.CreateChannel(delegate
		{
			OnCastComplete(configIndex, info);
		}, delegate
		{
			OnCastCancel(configIndex, info);
		}, config.selfValidator, info.target, config.targetValidator));
		channel.duration *= GetChannelDurationMultiplier();
		if (base.owner.owner != null && base.owner.owner.isHumanPlayer)
		{
			channel.blockedActions &= ~Channel.BlockedAction.Attack;
			channel.blockedActions &= ~Channel.BlockedAction.Move;
			channel.blockedActions |= Channel.BlockedAction.Cancelable;
			base.owner.Control.SetAttackMovSpdDisadvantage(channel.duration);
		}
		channel.isAttack = true;
		base.owner.Control.StartChannel(channel);
	}

	protected override void OnRotateForward(int configIndex, CastInfo info)
	{
		TriggerConfig config = configs[configIndex];
		if (config.castMethod.type == CastMethodType.Target && (object)info.target == null)
		{
			base.owner.Control.Rotate(info.rotation, immediately: false, config.overrideRotation ? config.overrideRotationDuration : (-1f));
			return;
		}
		switch (config.castMethod.type)
		{
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			base.owner.Control.Rotate(info.rotation, immediately: false, config.overrideRotation ? config.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.Target:
			base.owner.Control.RotateTowards(info.target, immediately: false, config.overrideRotation ? config.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.Point:
			base.owner.Control.RotateTowards(info.point, immediately: false, config.overrideRotation ? config.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.None:
			break;
		}
	}

	private void MirrorProcessed()
	{
	}
}
