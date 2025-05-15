[AchUnlockOnComplete(typeof(St_Q_Discipline))]
public class ACH_PEAK_PERFORMANCE : DewAchievementItem
{
	private const int RequiredMovementSkills = 3;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewPlayer.local.hero.GetType() != typeof(Hero_Vesper))
		{
			return;
		}
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
			return num >= 3;
		});
		static bool Check(SkillTrigger st)
		{
			if (st != null)
			{
				return st.configs[0].selfValidator.isMovementAbility;
			}
			return false;
		}
	}
}
