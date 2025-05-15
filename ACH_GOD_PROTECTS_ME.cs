using System;

[AchUnlockOnComplete(typeof(Gem_L_DivineFaith))]
public class ACH_GOD_PROTECTS_ME : DewAchievementItem
{
	private const int RequiredVisitCount = 4;

	private int _visitCount;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		_visitCount = 0;
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomLoaded);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		_visitCount = 0;
		if (NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomLoaded);
		}
	}

	private void OnRoomLoaded(EventInfoLoadRoom _)
	{
		GameManager.CallOnReady(delegate
		{
			if (!(NetworkedManagerBase<ZoneManager>.instance == null) && NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel >= 5 && NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted)
			{
				_visitCount++;
				if (_visitCount >= 4)
				{
					Complete();
				}
			}
		});
	}
}
