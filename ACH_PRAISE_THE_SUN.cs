using System;

[AchUnlockOnComplete(typeof(Gem_L_SolarEye))]
public class ACH_PRAISE_THE_SUN : DewAchievementItem
{
	private const int RequiredFireStack = 4000;

	[AchPersistentVar]
	private int _addedFireStack;

	public override int GetMaxProgress()
	{
		return 4000;
	}

	public override int GetCurrentProgress()
	{
		return _addedFireStack;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ClientEventManager>.instance.OnApplyElemental += new Action<EventInfoApplyElemental>(OnApplyElemental);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnApplyElemental -= new Action<EventInfoApplyElemental>(OnApplyElemental);
		}
	}

	private void OnApplyElemental(EventInfoApplyElemental info)
	{
		if (info.type == ElementalType.Fire && !(info.actor == null) && !info.actor.firstEntity.IsNullInactiveDeadOrKnockedOut() && !(info.actor.firstEntity.owner != DewPlayer.local) && info.victim is Monster && info.actor.firstEntity.GetRelation(info.victim) == EntityRelation.Enemy)
		{
			_addedFireStack += info.addedStack;
			if (_addedFireStack >= 4000)
			{
				Complete();
			}
		}
	}
}
