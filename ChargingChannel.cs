using System;
using Mirror;
using UnityEngine;

public class ChargingChannel
{
	public CastMethodData castMethod;

	internal NetworkIdentity _effectParent;

	internal ChargingChannelData _data;

	internal Action<ChargingChannel> _onChargeFull;

	internal Action<ChargingChannel> _onCast;

	internal Action<ChargingChannel> _onComplete;

	internal Action<ChargingChannel> _onCancel;

	private Channel _channel;

	private AbilityTrigger _trigger;

	private DewPlayer _player;

	private StatusEffect _slow;

	private bool _isInitialInfoSet;

	private bool _didReceiveAnyUpdates;

	private NetworkedOnScreenTimerHandle _timer;

	public CastInfo castInfo { get; private set; }

	public float chargeAmount { get; private set; }

	public float elapsedTime { get; private set; }

	public float normalizedTime { get; private set; }

	public bool isActive { get; private set; }

	public ChargingChannel SetInitialInfo(CastInfo info)
	{
		_isInitialInfoSet = true;
		castInfo = info;
		return this;
	}

	public ChargingChannel OnChargeFull(Action<ChargingChannel> callback)
	{
		_onChargeFull = (Action<ChargingChannel>)Delegate.Combine(_onChargeFull, callback);
		return this;
	}

	public ChargingChannel OnCast(Action<ChargingChannel> callback)
	{
		_onCast = (Action<ChargingChannel>)Delegate.Combine(_onCast, callback);
		return this;
	}

	public ChargingChannel OnComplete(Action<ChargingChannel> callback)
	{
		_onComplete = (Action<ChargingChannel>)Delegate.Combine(_onComplete, callback);
		return this;
	}

	public ChargingChannel OnCancel(Action<ChargingChannel> callback)
	{
		_onCancel = (Action<ChargingChannel>)Delegate.Combine(_onCancel, callback);
		return this;
	}

	public ChargingChannel UpdateCastMethod()
	{
		if (!isActive)
		{
			return this;
		}
		_player.UpdateSampleCastInfo(castMethod);
		return this;
	}

	private void HandleOnCancel()
	{
		if (isActive)
		{
			_onCancel?.Invoke(this);
			if (_data.cancelAnim != null)
			{
				_channel._owner.Animation.PlayAbilityAnimation(_data.cancelAnim);
			}
			if (_data.cancelEffect != null)
			{
				DewEffect.PlayNetworked(_effectParent, _data.cancelEffect, _channel._owner);
			}
			Deactivate();
		}
	}

	private void HandleOnTick()
	{
		if (!isActive)
		{
			return;
		}
		elapsedTime = _channel.elapsedTime;
		normalizedTime = _channel.elapsedTime / _data.completeDuration;
		if (_channel.elapsedTime < _data.chargeFullDuration)
		{
			chargeAmount = elapsedTime / _data.chargeFullDuration;
		}
		else if (chargeAmount < 1f)
		{
			_onChargeFull?.Invoke(this);
			chargeAmount = 1f;
			if (_data.chargeFullAnim != null)
			{
				_channel._owner.Animation.PlayAbilityAnimation(_data.chargeFullAnim);
			}
			if (_data.chargeFullEffect != null)
			{
				if (_data.chargeEffect != null)
				{
					DewEffect.StopNetworked(_effectParent, _data.chargeEffect);
				}
				DewEffect.PlayNetworked(_effectParent, _data.chargeFullEffect, _channel._owner);
			}
		}
		if (_trigger != null && _data.setFillAmount)
		{
			_trigger.fillAmount = chargeAmount;
		}
		if (_data.rotateForwardWhenCharging && _didReceiveAnyUpdates)
		{
			RotateForward();
		}
	}

	private void HandleOnComplete()
	{
		if (isActive)
		{
			_onComplete?.Invoke(this);
			if (_data.completeAnim != null)
			{
				_channel._owner.Animation.PlayAbilityAnimation(_data.completeAnim);
			}
			if (_data.completeEffect != null)
			{
				DewEffect.PlayNetworked(_effectParent, _data.completeEffect, _channel._owner);
			}
			if (_data.completeDazeDuration > 0f)
			{
				_channel._owner.Control.StartDaze(_data.completeDazeDuration);
			}
			if (_data.rotateForwardWhenComplete)
			{
				RotateForward();
			}
			Deactivate();
		}
	}

	private void HandleOnCast(CastInfo info)
	{
		if (isActive)
		{
			castInfo = info;
			if (_data.rotateForwardWhenCast)
			{
				RotateForward();
			}
			if (_data.castAnim != null)
			{
				_channel._owner.Animation.PlayAbilityAnimation(_data.castAnim);
			}
			if (_data.castEffect != null)
			{
				DewEffect.PlayNetworked(_effectParent, _data.castEffect, _channel._owner);
			}
			if (_data.castDazeDuration > 0f)
			{
				_channel._owner.Control.StartDaze(_data.castDazeDuration);
			}
			try
			{
				_onCast?.Invoke(this);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			if (_data.completeWhenCast)
			{
				_channel.Complete();
				Deactivate();
			}
			_didReceiveAnyUpdates = true;
		}
	}

	private void HandleInfoUpdate(CastInfo info)
	{
		if (isActive)
		{
			castInfo = info;
			_didReceiveAnyUpdates = true;
		}
	}

	public ChargingChannel Dispatch(Entity entity, AbilityTrigger trigger)
	{
		if (isActive)
		{
			return this;
		}
		Channel.BlockedAction blocked = Channel.BlockedAction.Ability | Channel.BlockedAction.Attack;
		if (!_data.canMove)
		{
			blocked |= Channel.BlockedAction.Move;
		}
		if (_data.canCancel)
		{
			blocked |= Channel.BlockedAction.Cancelable;
		}
		_channel = new Channel
		{
			blockedActions = blocked,
			duration = _data.completeDuration,
			onCancel = HandleOnCancel,
			onTick = HandleOnTick,
			onComplete = HandleOnComplete
		};
		_trigger = trigger;
		_channel.AddValidation(_data.selfValidator);
		_player = entity.owner;
		if (_player == null)
		{
			throw new InvalidOperationException("Player not found");
		}
		entity.Control.StartChannel(_channel);
		if (_data.chargeAnim != null)
		{
			_channel._owner.Animation.PlayAbilityAnimation(_data.chargeAnim);
		}
		if (_data.chargeEffect != null)
		{
			DewEffect.PlayNetworked(_effectParent, _data.chargeEffect, _channel._owner);
		}
		if (_data.canMove && _data.selfSlowAmount > 0f)
		{
			_slow = entity.CreateBasicEffect(entity, new SlowEffect
			{
				strength = _data.selfSlowAmount
			}, _data.completeDuration);
		}
		_player.StartSampleCastInfo(new SampleCastInfoContext
		{
			castMethod = castMethod,
			targetValidator = _data.targetValidator,
			trigger = trigger,
			castCallback = HandleOnCast,
			updateCallback = HandleInfoUpdate,
			cancelCallback = HandleOnCancel,
			showCastIndicator = _data.showCastIndicator,
			angleSpeedLimit = _data.angleSpeedLimit,
			castOnButton = (_data.castOnButtonRelease ? SampleCastInfoContext.CastOnButtonType.ByButton : SampleCastInfoContext.CastOnButtonType.None),
			isInitialInfoSet = _isInitialInfoSet,
			currentInfo = castInfo
		});
		if (_data.showOnScreenTimer)
		{
			_timer = new NetworkedOnScreenTimerHandle
			{
				skillKey = DewLocalization.GetSkillKey(trigger.GetType()),
				valueGetter = () => chargeAmount
			};
			_player.ShowOnScreenTimer(_timer);
		}
		isActive = true;
		return this;
	}

	private void RotateForward()
	{
		if (!isActive)
		{
			return;
		}
		_channel._owner.Control.StopOverrideRotation();
		switch (castMethod.type)
		{
		case CastMethodType.None:
			_channel._owner.Control.RotateTowards(_channel._owner.owner.cursorWorldPos, immediately: false, _data.rotateForwardOverrideRoationTime);
			break;
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			_channel._owner.Control.Rotate(castInfo.forward, immediately: false, _data.rotateForwardOverrideRoationTime);
			break;
		case CastMethodType.Target:
			if (castInfo.target != null && castInfo.target.isActive)
			{
				_channel._owner.Control.RotateTowards(castInfo.target.position, immediately: false, _data.rotateForwardOverrideRoationTime);
			}
			break;
		case CastMethodType.Point:
			_channel._owner.Control.Rotate(castInfo.point, immediately: false, _data.rotateForwardOverrideRoationTime);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void Deactivate()
	{
		if (isActive)
		{
			if (_slow != null && _slow.isActive)
			{
				_slow.Destroy();
			}
			_player.StopSampleCastInfo();
			if (_data.chargeAnim != null)
			{
				_player.hero.Animation.StopAbilityAnimation(_data.chargeAnim);
			}
			if (_data.chargeFullAnim != null)
			{
				_player.hero.Animation.StopAbilityAnimation(_data.chargeFullAnim);
			}
			if (_data.chargeEffect != null)
			{
				DewEffect.StopNetworked(_effectParent, _data.chargeEffect);
			}
			if (_data.chargeFullEffect != null)
			{
				DewEffect.StopNetworked(_effectParent, _data.chargeFullEffect);
			}
			if (_timer != null && _player != null)
			{
				_player.HideOnScreenTimer(_timer);
				_timer = null;
			}
			if (_trigger != null && _data.setFillAmount)
			{
				_trigger.fillAmount = 0f;
			}
			isActive = false;
		}
	}
}
