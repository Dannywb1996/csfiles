public class AttrCustomizeConfig
{
	public static readonly AttrCustomizeConfig DefaultConfig;

	public int maxPlayer;

	public float enemyMovementSpeedPercentage;

	public float enemyAttackSpeedPercentage;

	public float enemyAbilityHasteFlat;

	public float bossHealthMultiplier;

	public float bossDamageMultiplier;

	public float miniBossHealthMultiplier;

	public float miniBossDamageMultiplier;

	public float littleMonsterHealthMultiplier;

	public float littleMonsterDamageMultiplier;

	public float extraHealthGrowthMultiplier;

	public float extraDamageGrowthMultiplier;

	public float beneficialNodeMultiplier;

	public int skillQGemCount;

	public int skillWGemCount;

	public int skillEGemCount;

	public int skillRGemCount;

	public int skillIdentityGemCount;

	public int skillMovementGemCount;

	public int shopAddedItems;

	public int shopRefreshes;

	public int bossCount;

	public int bossCountAddByLoop;

	public int bossCountAddByZone;

	public float maxAndSpawnedPopulationMultiplier;

	public string[] startSkills;

	public int[] startSkillsLevel;

	public string[] startGems;

	public int[] startGemsQuality;

	public bool enableHeroSkillAddShop;

	public string[] removeSkills;

	public string[] removeGems;

	public bool enableMistAllowAnyDirection;

	public int firstVisitDropGoldCount;

	public int firstVisitDropGoldCountAddByLoop;

	public int firstVisitDropGoldCountAddByZone;

	public bool enableHealthReduceMultiplierAddByZone;

	public bool enableCurrentNodeGenerateLostSoul;

	public bool enableBossRoomGenerateLostSoul;

	public bool enableArtifactQuest;

	public bool enableFragmentOfRadianceBossQuest;

	public bool enableQuestHuntedByObliviaxRepeatable;

	public bool enableDamageRanking;

	public bool enableBossSpawnAllOnce;

	public float bossMirageChance;

	public float bossHunterChance;

	public float monsterMirageChanceMultiple;

	public bool enableWorldReveal;

	public float bossSingleInjuryHealthMultiplier;

	public float healRawMultiplier;

	public bool enableLucidDreamEmbraceMortality;

	public bool enableLucidDreamBonVoyage;

	public bool enableLucidDreamGrievousWounds;

	public bool enableLucidDreamTheDarkestUrge;

	public bool enableLucidDreamWild;

	public bool enableLucidDreamMadLife;

	public bool enableLucidDreamSparklingDreamFlask;

	public int limitBossCount;

	static AttrCustomizeConfig()
	{
		DefaultConfig = new AttrCustomizeConfig
		{
			maxPlayer = 4,
			enemyMovementSpeedPercentage = 1f,
			enemyAttackSpeedPercentage = 1f,
			enemyAbilityHasteFlat = 1f,
			bossHealthMultiplier = 1f,
			bossDamageMultiplier = 1f,
			miniBossHealthMultiplier = 1f,
			miniBossDamageMultiplier = 1f,
			littleMonsterHealthMultiplier = 1f,
			littleMonsterDamageMultiplier = 1f,
			extraHealthGrowthMultiplier = 0f,
			extraDamageGrowthMultiplier = 0f,
			beneficialNodeMultiplier = 1f,
			skillQGemCount = 3,
			skillWGemCount = 3,
			skillEGemCount = 3,
			skillRGemCount = 3,
			skillIdentityGemCount = 0,
			skillMovementGemCount = 0,
			shopAddedItems = 1,
			shopRefreshes = 1,
			bossCount = 1,
			bossCountAddByLoop = 0,
			bossCountAddByZone = 0,
			limitBossCount = 1,
			maxAndSpawnedPopulationMultiplier = 1.5f,
			startSkills = new string[0],
			startSkillsLevel = new int[0],
			startGems = new string[0],
			startGemsQuality = new int[0],
			enableHeroSkillAddShop = false,
			removeSkills = new string[0],
			removeGems = new string[0],
			enableMistAllowAnyDirection = true,
			firstVisitDropGoldCount = 0,
			firstVisitDropGoldCountAddByLoop = 0,
			firstVisitDropGoldCountAddByZone = 0,
			enableHealthReduceMultiplierAddByZone = false,
			enableCurrentNodeGenerateLostSoul = false,
			enableBossRoomGenerateLostSoul = false,
			enableArtifactQuest = true,
			enableFragmentOfRadianceBossQuest = true,
			enableQuestHuntedByObliviaxRepeatable = false,
			enableDamageRanking = false,
			enableBossSpawnAllOnce = false,
			bossMirageChance = 0f,
			bossHunterChance = 0f,
			monsterMirageChanceMultiple = 1f,
			enableWorldReveal = false,
			bossSingleInjuryHealthMultiplier = 1f,
			healRawMultiplier = 1f,
			enableLucidDreamEmbraceMortality = false,
			enableLucidDreamBonVoyage = false,
			enableLucidDreamGrievousWounds = false,
			enableLucidDreamTheDarkestUrge = false,
			enableLucidDreamWild = false,
			enableLucidDreamMadLife = false,
			enableLucidDreamSparklingDreamFlask = false
		};
	}
}
