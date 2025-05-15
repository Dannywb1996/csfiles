using System;

[AchUnlockOnComplete(typeof(St_L_MentalCorruption))]
public class ACH_BLACKCAT_DEJAVU : DewAchievementItem
{
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
		if (obj is Shrine_LoopCat shrine_LoopCat)
		{
			shrine_LoopCat.ClientEvent_OnSuccessfulUse += new Action<Entity>(OnUseShrineLoopCat);
		}
	}

	private void OnUseShrineLoopCat(Entity e)
	{
		Complete();
	}
}
