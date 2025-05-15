[AchUnlockOnComplete(typeof(LucidDream_HarmlessWhispers))]
public class ACH_JUST_STAY_STILL : DewAchievementItem
{
	private const int RequiredCCSkills = 4;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchCompleteWhen(delegate
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
			return num >= 4;
		});
		static bool Check(SkillTrigger st)
		{
			if (st != null)
			{
				return st.tags.HasFlag(DescriptionTags.HardCC);
			}
			return false;
		}
	}
}
