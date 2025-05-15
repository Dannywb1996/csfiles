public interface IUpgradeSkillProvider
{
	int GetDreamDustCost(SkillTrigger target);

	int GetAddedLevel();

	bool RequestSkillUpgrade(SkillTrigger target);
}
