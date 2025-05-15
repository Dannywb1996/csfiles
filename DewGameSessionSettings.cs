using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dew Game Session Settings", menuName = "Dew Game Session Settings")]
public class DewGameSessionSettings : SerializedScriptableObject
{
	[Header("Experience")]
	public int maxHeroLevel = 20;

	public float expDropDeviation = 0.1f;

	public float expGlobalMultiplier = 1f;

	public PerMonsterTypeData<float> expMultiplier;

	public float droppedExpMultiplierPerPlayer = 1f;

	public float gainedSkillHastePerSkillLevel = 4f;

	[Header("Regen Orb")]
	public PerMonsterTypeData<float> regenOrbDropChanceOnMaxHealth;

	public PerMonsterTypeData<float> regenOrbDropChanceOnLowHealth;

	[Header("Gold")]
	public Formula goldDropByMonsterLevel;

	public float goldDropDeviation = 0.1f;

	public float goldGlobalMultiplier = 1f;

	public PerMonsterTypeData<float> goldMultiplier;

	public float droppedGoldMultiplierPerPlayer = 1f;

	[Header("Stardust")]
	public PerMonsterTypeData<float> stardustDeathDropChance;

	public Vector2Int stardustDeathDropAmount;

	public Vector2Int stardustBossSoulAmount;

	[Header("Shrines")]
	public float shrineCostGlobalMultiplier = 1f;

	public float shrineCostMultiplierPerAmbientLevel = 1.25f;

	public float shrineCostMultiplierPerPlayer = 1.15f;

	public float maxHealthCost = 99f;

	public float minHealthCost = 5f;

	public float propSpawnMultiplierPerPlayer = 2f;

	[Header("Max Population (Spawned)")]
	public float maxGlobalPopulation = 7f;

	public float maxGlobalPopulationMultiplierPerPlayer = 1.1f;

	public Formula maxGlobalPopulationMultiplierByAmbientLevel;

	[Header("Max Population (Section)")]
	public float maxSectionPopulation = 5f;

	public float maxSectionPopulationMultiplierPerPlayerInSection = 1.1f;

	public Formula maxSectionPopulationMultiplierByAmbientLevel;

	[Header("Monsters")]
	public Formula miniBossSpawnChanceByNonMiniBossCombatNodesVisited;

	public float monsterWavesMultiplierPerPlayer = 1f;

	public float monsterSpawnPopulationMultiplierPerPlayer = 1f;

	public Formula monsterSpawnPopulationMultiplierByTurnIndex = "1";

	public float monsterBonusHealthPercentagePerPlayer;

	public float monsterBonusPowerPercentagePerPlayer = 20f;

	public float miniBossBonusHealthPercentagePerPlayer = 110f;

	public float miniBossBonusPowerPercentagePerPlayer = 17f;

	public float bossBonusHealthPercentagePerPlayer = 120f;

	public float bossBonusPowerPercentagePerPlayer = 17f;

	public float[,] mirageSkinChanceByZoneIndexAndPlayerCount = new float[6, 4];

	[Header("Hunters")]
	public AnimationCurve welcomingSpawnPopMultiplierByArea;

	[Header("Buy, Sell")]
	public float valueGlobalMultiplier = 1f;

	public PerRarityData<int> gemValue;

	public PerRarityData<int> skillValue;

	public float valueMultiplierPerSkillLevel;

	public float valueMultiplierPerGemQuality;

	public float sellValueMultiplier = 0.4f;

	[Header("Dismantle, Upgrades")]
	public Formula gemDismantleDreamDustByQuality;

	public Formula gemUpgradeDreamDustByQuality;

	public int gemAddedQualityOnUpgrade = 50;

	public Formula skillDismantleDreamDustByLevel;

	public Formula skillUpgradeDreamDustByLevel;

	public PerRarityData<float> gemDismantleDreamDustMultiplier;

	public PerRarityData<float> skillDismantleDreamDustMultiplier;

	[Header("Cleanses")]
	public Formula gemCleanseCostByQuality;

	public Formula skillCleanseCostByLevel;

	public int gemCleanseMinQuality;

	public int skillCleanseMinLevel;

	[Header("Combat Reward Chances By Zone Index")]
	public Formula combatRewardMemoryChance;

	public Formula combatRewardFantasyChance;

	[Header("Currency Rewards By Zone Index")]
	public float goldOnlyChance;

	public Formula goldOnlyRewardMin;

	public Formula goldOnlyRewardMax;

	public float dreamDustOnlyChance;

	public Formula dreamDustOnlyRewardMin;

	public Formula dreamDustOnlyRewardMax;

	public float bothChance;

	public Formula bothGoldRewardMin;

	public Formula bothGoldRewardMax;

	public Formula bothDreamDustRewardMin;

	public Formula bothDreamDustRewardMax;

	public float highRewardCurrencyMultiplier = 1.5f;

	[Header("Boss Rewards by Zone Index")]
	public Formula bossRewardsGoldMin;

	public Formula bossRewardsGoldMax;

	public Formula bossRewardsDreamDustMin;

	public Formula bossRewardsDreamDustMax;
}
