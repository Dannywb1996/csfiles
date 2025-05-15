using System;

[AchUnlockOnComplete(typeof(LucidDream_EmbraceMortality))]
public class ACH_EMBRACE_MORTALITY : DewAchievementItem
{
	private const int RequiredCurseCount = 6;

	private int _curseCount;

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
		if (obj is CurseStatusEffect curseStatusEffect && !(curseStatusEffect.victim != base.hero))
		{
			_curseCount++;
			if (_curseCount >= 6)
			{
				Complete();
			}
		}
	}
}
