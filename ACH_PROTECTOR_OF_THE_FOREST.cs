[AchUnlockOnComplete(typeof(St_L_Hysteria))]
public class ACH_PROTECTOR_OF_THE_FOREST : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill kill)
		{
			if (kill.victim is Mon_Forest_BossDemon)
			{
				Complete();
			}
		});
	}
}
