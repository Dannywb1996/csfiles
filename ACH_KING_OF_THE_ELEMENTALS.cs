using System;

[AchUnlockOnComplete(typeof(St_Q_Fleche))]
public class ACH_KING_OF_THE_ELEMENTALS : DewAchievementItem
{
	private const int KilledElementals = 45;

	[AchPersistentVar]
	private int _kills;

	public override int GetCurrentProgress()
	{
		return _kills;
	}

	public override int GetMaxProgress()
	{
		return 45;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewPlayer.local.hero.GetType() != typeof(Hero_Mist))
		{
			return;
		}
		AchOnKillOrAssist(delegate(EventInfoKill k)
		{
			if (k.victim is Monster monster && (monster.GetType().Name.Contains("Elemental", StringComparison.InvariantCultureIgnoreCase) || monster.GetType().Name.Contains("Treant", StringComparison.InvariantCultureIgnoreCase)))
			{
				_kills++;
				if (_kills >= 45)
				{
					Complete();
				}
			}
		});
	}
}
