using System;

[AchUnlockOnComplete(typeof(LucidDream_MadLife))]
public class ACH_MASTER_OF_EVASION : DewAchievementItem
{
	private bool _didFail;

	private bool _didStartForest;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnTakeDamage(delegate(EventInfoDamage dmg)
		{
			if (!(dmg.actor == null))
			{
				Entity firstEntity = dmg.actor.firstEntity;
				if (!(firstEntity == null) && firstEntity is Monster monster && !(monster.owner != DewPlayer.creep))
				{
					_didFail = true;
				}
			}
		});
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action(ClientEventOnZoneLoaded);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action(ClientEventOnZoneLoaded);
		}
	}

	private void ClientEventOnZoneLoaded()
	{
		if (_didFail)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(delegate
		{
			if (_didStartForest)
			{
				Complete();
			}
			else if (NetworkedManagerBase<ZoneManager>.instance.currentZone.name == "Zone_Forest")
			{
				_didStartForest = true;
			}
		});
	}
}
