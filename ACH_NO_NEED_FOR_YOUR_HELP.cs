using System;

[AchUnlockOnComplete(typeof(LucidDream_SparklingDreamFlask))]
public class ACH_NO_NEED_FOR_YOUR_HELP : DewAchievementItem
{
	private const int RequiredEnterCount = 3;

	private bool _didFail;

	[AchPersistentVar]
	private int _enterCount;

	public override int GetCurrentProgress()
	{
		return _enterCount;
	}

	public override int GetMaxProgress()
	{
		return 3;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.onActorAdd += new Action<Actor>(OnActorAdd);
		AchWithGameResult(delegate(DewGameResult res)
		{
			if (!_didFail && res.result == DewGameResult.ResultType.DemoFinish)
			{
				_enterCount++;
				if (_enterCount >= 3)
				{
					Complete();
				}
			}
		});
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
		if (obj is Shrine_Guidance shrine_Guidance)
		{
			shrine_Guidance.ClientEvent_OnSuccessfulUse += (Action<Entity>)delegate
			{
				_didFail = true;
			};
		}
	}
}
