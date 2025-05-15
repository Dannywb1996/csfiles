using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_R_WrathOfEl))]
public class ACH_ANGER_MANAGEMENT_PROFESSIONAL : DewAchievementItem
{
	private class Ad_KillTimeCheck
	{
		public float firstDamageTime;
	}

	private const float AllowedTimeFrame = 5f;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewPlayer.local.hero.GetType() != typeof(Hero_Vesper))
		{
			return;
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		AchOnKillOrAssist(delegate(EventInfoKill k)
		{
			if (k.victim is BossMonster && (!k.victim.TryGetData<Ad_KillTimeCheck>(out var data) || Time.time - data.firstDamageTime < 5f))
			{
				Complete();
			}
		});
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		if (obj.victim is BossMonster && !obj.victim.HasData<Ad_KillTimeCheck>())
		{
			obj.victim.AddData(new Ad_KillTimeCheck
			{
				firstDamageTime = Time.time
			});
		}
	}
}
