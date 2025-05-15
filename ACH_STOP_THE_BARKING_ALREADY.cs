using System;

[AchUnlockOnComplete(typeof(St_Q_MoonlightPact))]
public class ACH_STOP_THE_BARKING_ALREADY : DewAchievementItem
{
	private const int RequiredSummonCount = 5;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (base.hero is Hero_Nachia)
		{
			base.hero.ClientActorEvent_OnCreate += new Action<Actor>(ClientActorEventOnCreate);
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (base.hero != null)
		{
			base.hero.ClientActorEvent_OnCreate -= new Action<Actor>(ClientActorEventOnCreate);
		}
	}

	private void ClientActorEventOnCreate(Actor obj)
	{
		if (obj is Summon && base.hero.summons.Count >= 5)
		{
			Complete();
		}
	}
}
