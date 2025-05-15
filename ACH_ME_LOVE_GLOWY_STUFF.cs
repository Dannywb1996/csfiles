[AchUnlockOnComplete(typeof(St_R_OrbOfLight))]
public class ACH_ME_LOVE_GLOWY_STUFF : DewAchievementItem
{
	private const int RequiredLightSkills = 4;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchWithGameResult(delegate(DewGameResult res)
		{
			if (res.result == DewGameResult.ResultType.DemoFinish)
			{
				int num = 0;
				if (Check(DewPlayer.local.hero.Skill.Q))
				{
					num++;
				}
				if (Check(DewPlayer.local.hero.Skill.W))
				{
					num++;
				}
				if (Check(DewPlayer.local.hero.Skill.E))
				{
					num++;
				}
				if (Check(DewPlayer.local.hero.Skill.R))
				{
					num++;
				}
				if (num >= 4)
				{
					Complete();
				}
			}
		});
		static bool Check(SkillTrigger st)
		{
			if (st != null)
			{
				return st.tags.HasFlag(DescriptionTags.Light);
			}
			return false;
		}
	}
}
