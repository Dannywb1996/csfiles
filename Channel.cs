using System;
using System.Collections.Generic;

public class Channel
{
	internal delegate bool ChannelValidator();

	[Flags]
	public enum BlockedAction : byte
	{
		None = 0,
		Move = 1,
		Ability = 2,
		Attack = 4,
		Cancelable = 0x80,
		Everything = 7,
		EverythingCancelable = 0x87
	}

	public float duration;

	public BlockedAction blockedActions;

	public float uncancellableTime;

	public Action onComplete;

	public Action onTick;

	public Action onCancel;

	public bool isAttack;

	internal List<ChannelValidator> _validators;

	internal Entity _owner;

	public float elapsedTime { get; internal set; }

	public bool isAlive { get; internal set; }

	public Channel AddOnComplete(Action callback)
	{
		onComplete = (Action)Delegate.Combine(onComplete, callback);
		return this;
	}

	public Channel AddOnCancel(Action callback)
	{
		onCancel = (Action)Delegate.Combine(onCancel, callback);
		return this;
	}

	public Channel AddOnTick(Action callback)
	{
		onTick = (Action)Delegate.Combine(onTick, callback);
		return this;
	}

	public Channel AddValidation(IEntityValidator validator)
	{
		if (_validators == null)
		{
			_validators = new List<ChannelValidator>();
		}
		_validators.Add(() => validator.Evaluate(_owner));
		return this;
	}

	public Channel AddValidation(IBinaryEntityValidator validator, Entity entity)
	{
		if (_validators == null)
		{
			_validators = new List<ChannelValidator>();
		}
		_validators.Add(() => validator.Evaluate(_owner, entity));
		return this;
	}

	public Channel Complete()
	{
		elapsedTime = duration;
		return this;
	}

	public Channel Cancel()
	{
		if (_validators == null)
		{
			_validators = new List<ChannelValidator>();
		}
		_validators.Add(() => false);
		return this;
	}

	public Channel Dispatch(Entity e)
	{
		e.Control.StartChannel(this);
		return this;
	}

	internal Channel AddValidation(ChannelValidator validator)
	{
		if (_validators == null)
		{
			_validators = new List<ChannelValidator>();
		}
		_validators.Add(validator);
		return this;
	}
}
