using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_R_SerpentineBlessing))]
public class ACH_SIBLING_FIGHT : DewAchievementItem
{
	private class Ad_FirstDamageTime
	{
		public float damageTime;
	}

	private const float KillTimeframeSeconds = 6f;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (!(base.hero is Hero_Nachia))
		{
			return;
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		AchOnKillOrAssist(delegate(EventInfoKill k)
		{
			if (k.victim is Mon_Forest_BossDemon && (!k.victim.TryGetData<Ad_FirstDamageTime>(out var data) || Time.time - data.damageTime < 6f))
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
		if (obj.victim is Mon_Forest_BossDemon && !obj.victim.HasData<Ad_FirstDamageTime>())
		{
			obj.victim.AddData(new Ad_FirstDamageTime
			{
				damageTime = Time.time
			});
		}
	}
}
