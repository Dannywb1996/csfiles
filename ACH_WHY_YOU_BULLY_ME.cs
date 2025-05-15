using System;

[AchUnlockOnComplete(typeof(LucidDream_TheDarkestUrge))]
public class ACH_WHY_YOU_BULLY_ME : DewAchievementItem
{
	private const int RequiredCount = 20;

	[AchPersistentVar]
	private int _count;

	public override int GetMaxProgress()
	{
		return 20;
	}

	public override int GetCurrentProgress()
	{
		return _count;
	}

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
		if (obj is PropEnt_Merchant_Jonas propEnt_Merchant_Jonas)
		{
			propEnt_Merchant_Jonas.EntityEvent_OnDeath += (Action<EventInfoKill>)delegate
			{
				_count++;
				if (_count >= 20)
				{
					Complete();
				}
			};
		}
		if (obj is Ai_Prop_Merchant_Flee)
		{
			_count++;
			if (_count >= 20)
			{
				Complete();
			}
		}
	}
}
