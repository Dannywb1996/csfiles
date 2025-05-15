using System;

[Serializable]
public class ChannelData
{
	public float duration = 0.25f;

	public Channel.BlockedAction blockedActions = Channel.BlockedAction.Everything;

	public float uncancellableTime;

	public AbilitySelfValidator selfValidator;

	public Channel Get()
	{
		return new Channel
		{
			duration = duration,
			blockedActions = blockedActions,
			uncancellableTime = uncancellableTime
		}.AddValidation(selfValidator);
	}
}
