using System;

[AchUnlockOnComplete(typeof(Gem_E_Overload))]
public class ACH_REAP_WHAT_YOU_SOW : DewAchievementItem
{
	private const int RequiredActivateCount = 16;

	[AchPersistentVar]
	private int _disintegrationCount;

	public override int GetMaxProgress()
	{
		return 16;
	}

	public override int GetCurrentProgress()
	{
		return _disintegrationCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.onActorAdd += new Action<Actor>(OnActorAdd);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (!(NetworkedManagerBase<ActorManager>.instance == null))
		{
			NetworkedManagerBase<ActorManager>.instance.onActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (obj is Shrine_Disintegration shrine_Disintegration)
		{
			shrine_Disintegration.ClientEvent_OnSuccessfulUse += new Action<Entity>(OnUseShrineDisintegration);
		}
	}

	private void OnUseShrineDisintegration(Entity e)
	{
		if (!e.IsNullInactiveDeadOrKnockedOut() && !(e.owner != DewPlayer.local))
		{
			_disintegrationCount++;
			if (_disintegrationCount >= 16)
			{
				Complete();
			}
		}
	}
}
