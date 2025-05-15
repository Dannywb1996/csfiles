using System;

[AchUnlockOnComplete(typeof(St_E_ClutchesOfMalice))]
public class ACH_SNAKE_IN_THE_GRASS : DewAchievementItem
{
	private const int RequiredActivateCount = 12;

	[AchPersistentVar]
	private int _activatedCount;

	public override int GetMaxProgress()
	{
		return 12;
	}

	public override int GetCurrentProgress()
	{
		return _activatedCount;
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
		if (obj is Shrine_Merchant_Backpack shrine_Merchant_Backpack)
		{
			shrine_Merchant_Backpack.ClientEvent_OnSuccessfulUse += new Action<Entity>(OnSuccessfulUse);
		}
	}

	private void OnSuccessfulUse(Actor obj)
	{
		_activatedCount++;
		if (_activatedCount >= 12)
		{
			Complete();
		}
	}
}
