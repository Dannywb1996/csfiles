[AchUnlockOnComplete(typeof(St_L_ShoutOfOblivion))]
public class ACH_CHAOS_DESTRUCTION_OBLIVION : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill kill)
		{
			if (kill.victim is Mon_Special_BossObliviax)
			{
				Complete();
			}
		});
	}
}
