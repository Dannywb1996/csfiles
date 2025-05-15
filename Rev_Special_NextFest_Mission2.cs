using System;

public class Rev_Special_NextFest_Mission2 : DewSpecialReverieItem
{
	[AchPersistentVar]
	private int _killCount;

	public override int grantedStardust => 0;

	public override bool excludeFromPool => true;

	public override string[] grantedItems => new string[3] { "Emote_NextFest_YubarBaffled", "Acc_NextFest_GoldenCrown", "Nametag_NextFest_GoldenTriumph" };

	public override TimeSpan timeLimit => new TimeSpan(14, 1, 0, 0);

	public override int GetCurrentProgress()
	{
		return _killCount;
	}

	public override int GetMaxProgress()
	{
		return 16;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist(delegate(EventInfoKill k)
		{
			if (k.victim is BossMonster bossMonster)
			{
				if (bossMonster.isHiddenBoss)
				{
					_killCount += 999;
				}
				else
				{
					_killCount++;
				}
				if (_killCount >= 16)
				{
					Complete();
				}
			}
		});
	}
}
