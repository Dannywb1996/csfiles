using System;

[AchUnlockOnComplete(typeof(Gem_E_Omega))]
public class ACH_OMEGA_POINT : DewAchievementItem
{
	private Type _skillType;

	private bool _hasFailed;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewPlayer.local.hero == null)
		{
			return;
		}
		DewPlayer.local.hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(OnSkillUse);
		AchWithGameResult(delegate(DewGameResult r)
		{
			if (r.result == DewGameResult.ResultType.DemoFinish && !_hasFailed)
			{
				Complete();
			}
		});
	}

	private void OnSkillUse(EventInfoSkillUse info)
	{
		if (!_hasFailed && !(info.skill == null) && info.skill.skillType != HeroSkillLocation.Identity && info.skill.skillType != HeroSkillLocation.Movement && !(_skillType == info.skill.GetType()))
		{
			if (_skillType == null)
			{
				_skillType = info.skill.GetType();
			}
			else
			{
				_hasFailed = true;
			}
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (!(DewPlayer.local == null) && !(DewPlayer.local.hero == null))
		{
			DewPlayer.local.hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(OnSkillUse);
		}
	}
}
