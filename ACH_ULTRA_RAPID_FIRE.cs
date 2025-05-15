using System;
using System.Collections.Generic;
using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_E_Insight))]
public class ACH_ULTRA_RAPID_FIRE : DewAchievementItem
{
	private const float CountTimeframe = 8f;

	private const float RequiredCastCount = 12f;

	private readonly List<float> _castTimings = new List<float>();

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		DewPlayer.local.hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		AchSetInterval(delegate
		{
			while (_castTimings.Count > 0 && Time.time - _castTimings[0] > 8f)
			{
				_castTimings.RemoveAt(0);
			}
			if ((float)_castTimings.Count >= 12f)
			{
				_castTimings.Clear();
				Complete();
			}
		}, 1f);
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.type != HeroSkillLocation.Movement)
		{
			_castTimings.Add(Time.time);
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (DewPlayer.local != null && DewPlayer.local.hero != null)
		{
			DewPlayer.local.hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}
}
