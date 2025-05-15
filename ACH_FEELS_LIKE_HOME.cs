using System;

[AchUnlockOnComplete(typeof(LucidDream_WILD))]
public class ACH_FEELS_LIKE_HOME : DewAchievementItem
{
	private const int RequiredVisitCount = 15;

	private int _visitCount;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(delegate
		{
			if (NetworkedManagerBase<ZoneManager>.instance.currentNode.HasModifier<RoomMod_Hunted>() || NetworkedManagerBase<ZoneManager>.instance.currentNode.HasModifier<RoomMod_Ambush>())
			{
				_visitCount++;
				if (_visitCount >= 15)
				{
					Complete();
				}
			}
		});
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		}
	}
}
