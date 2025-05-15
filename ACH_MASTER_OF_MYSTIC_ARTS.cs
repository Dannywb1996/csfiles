[AchUnlockOnComplete(typeof(St_E_MysticDagger))]
public class ACH_MASTER_OF_MYSTIC_ARTS : DewAchievementItem
{
	private const int RequiredSkills = 4;

	private const int CooldownTimeThreshold = 3;

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
				return st.currentConfigMaxCooldownTime < 3f;
			}
			return false;
		}
	}
}
