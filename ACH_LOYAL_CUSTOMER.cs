using System;

[AchUnlockOnComplete(typeof(Gem_R_Rejuvenation))]
public class ACH_LOYAL_CUSTOMER : DewAchievementItem
{
	private const int RequiredCount = 15;

	private int _currentCount;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.onActorAdd += new Action<Actor>(OnActorAdd);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.onActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (!(obj is Shrine_Guidance shrine_Guidance))
		{
			return;
		}
		shrine_Guidance.ClientEvent_OnSuccessfulUse += (Action<Entity>)delegate(Entity user)
		{
			if (!(user == null) && !(user.owner != DewPlayer.local))
			{
				_currentCount++;
				if (_currentCount >= 15)
				{
					Complete();
				}
			}
		};
	}
}
