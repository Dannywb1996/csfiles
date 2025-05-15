using System;
using System.Collections.Generic;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_D_MercyOfEl))]
public class ACH_MASTER_OF_HAMMER : DewAchievementItem
{
	private const int TimeWindow = 5;

	private const int RequiredAttacks = 9;

	private List<float> _attackTimes = new List<float>();

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(DewPlayer.local.hero.GetType() != typeof(Hero_Vesper)))
		{
			DewPlayer.local.hero.ClientActorEvent_OnCreate += new Action<Actor>(ClientActorEventOnCreate);
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (DewPlayer.local != null && DewPlayer.local.hero != null)
		{
			DewPlayer.local.hero.ClientActorEvent_OnCreate -= new Action<Actor>(ClientActorEventOnCreate);
		}
	}

	private void ClientActorEventOnCreate(Actor obj)
	{
		if (!(obj is Ai_Atk_VesperMace) && !(obj is Ai_Atk_VesperMace_Crit))
		{
			return;
		}
		for (int num = _attackTimes.Count - 1; num >= 0; num--)
		{
			if (Time.time - _attackTimes[num] > 5f)
			{
				_attackTimes.RemoveAt(num);
			}
		}
		_attackTimes.Insert(0, Time.time);
		if (_attackTimes.Count >= 9)
		{
			_attackTimes.Clear();
			Complete();
		}
	}
}
