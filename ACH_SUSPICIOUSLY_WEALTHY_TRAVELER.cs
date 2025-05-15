using System;

[AchUnlockOnComplete(typeof(Gem_R_Wealth))]
public class ACH_SUSPICIOUSLY_WEALTHY_TRAVELER : DewAchievementItem
{
	private const int RequiredSpendAmount = 2000;

	[AchPersistentVar]
	private int _spentGold;

	public override int GetMaxProgress()
	{
		return 2000;
	}

	public override int GetCurrentProgress()
	{
		return _spentGold;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		DewPlayer.local.ClientEvent_OnSpendGold += new Action<int>(ClientEventOnSpendGold);
	}

	private void ClientEventOnSpendGold(int obj)
	{
		_spentGold += obj;
		if (_spentGold >= 2000)
		{
			Complete();
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (DewPlayer.local != null)
		{
			DewPlayer.local.ClientEvent_OnSpendGold -= new Action<int>(ClientEventOnSpendGold);
		}
	}
}
