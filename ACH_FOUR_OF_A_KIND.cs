using System;

[AchUnlockOnComplete(typeof(St_L_CoinExplosion))]
public class ACH_FOUR_OF_A_KIND : DewAchievementItem
{
	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchWithGameResult(delegate(DewGameResult res)
		{
			if (res.result == DewGameResult.ResultType.DemoFinish && !(base.hero.Skill.Q == null) && !(base.hero.Skill.W == null) && !(base.hero.Skill.E == null) && !(base.hero.Skill.R == null))
			{
				Type type = base.hero.Skill.Q.GetType();
				if (!(base.hero.Skill.W.GetType() != type) && !(base.hero.Skill.E.GetType() != type) && !(base.hero.Skill.R.GetType() != type))
				{
					Complete();
				}
			}
		});
	}
}
