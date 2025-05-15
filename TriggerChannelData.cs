using System;

[Serializable]
public class TriggerChannelData
{
	public float duration = 0.25f;

	public Channel.BlockedAction blockedActions = Channel.BlockedAction.Everything;

	public Channel CreateChannel(Action onComplete, Action onCancel, IEntityValidator selfValidator)
	{
		Channel channel = new Channel();
		channel.duration = duration;
		channel.blockedActions = blockedActions;
		channel.onComplete = onComplete;
		channel.onCancel = onCancel;
		channel.AddValidation(selfValidator);
		return channel;
	}

	public Channel CreateChannel(Action onComplete, Action onCancel, IEntityValidator selfValidator, Entity target, IBinaryEntityValidator targetValidator)
	{
		Channel channel = CreateChannel(onComplete, onCancel, selfValidator);
		channel.AddValidation(targetValidator, target);
		return channel;
	}
}
