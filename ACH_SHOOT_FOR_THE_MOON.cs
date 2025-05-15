using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_R_Accuracy))]
public class ACH_SHOOT_FOR_THE_MOON : DewAchievementItem
{
	private const int RequiredKills = 80;

	[AchPersistentVar]
	private int _kills;

	public override int GetMaxProgress()
	{
		return 80;
	}

	public override int GetCurrentProgress()
	{
		return _kills;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillLastHit(delegate(EventInfoKill kill)
		{
			if (kill.victim is Monster && !(Vector2.Distance(kill.victim.position.ToXY(), base.hero.position.ToXY()) < 13.5f))
			{
				_kills++;
				if (_kills >= 80)
				{
					Complete();
				}
			}
		});
	}
}
