using UnityEngine;

[CreateAssetMenu(fileName = "New Dew Difficulty Settings", menuName = "Dew Difficulty Settings")]
[DewResourceLink(ResourceLinkBy.Name)]
public class DewDifficultySettings : ScriptableObject
{
	public Color difficultyColor;

	public Sprite icon;

	public float iconScale = 1f;

	public float maxPopulationMultiplier;

	public float regenOrbChanceMultiplier;

	public AnimationCurve predictionStrengthCurve;

	public float healRawMultiplier;

	public float scoreMultiplier;

	public float specialSkillChanceMultiplier = 1f;

	public int gainedStardustAmountOffset;

	public bool enableBleedOuts = true;

	public Vector2Int lostSoulDistance;

	public float beneficialNodeMultiplier = 1f;

	public float harmfulNodeMultiplier = 1f;

	public float hunterSpreadChance;

	public float enemyHealthPercentage;

	public float enemyPowerPercentage;

	public float enemyMovementSpeedPercentage;

	public float enemyAttackSpeedPercentage;

	public float enemyAbilityHasteFlat;

	public float scalingFactor = 1f;

	public int positionSampleCount;

	public int positionSampleLagBehindFrames;

	public float positionSampleInterval;

	internal void ApplyDifficultyModifiers(Entity entity)
	{
		if (entity is Monster)
		{
			scalingFactor *= 1.0008f;
			entity.Status.AddStatBonus(new StatBonus
			{
				maxHealthPercentage = enemyHealthPercentage,
				attackDamagePercentage = enemyPowerPercentage,
				abilityPowerPercentage = enemyPowerPercentage,
				movementSpeedPercentage = enemyMovementSpeedPercentage * AttrCustomizeResources.Config.enemyMovementSpeedPercentage,
				attackSpeedPercentage = enemyAttackSpeedPercentage * AttrCustomizeResources.Config.enemyAttackSpeedPercentage,
				abilityHasteFlat = enemyAbilityHasteFlat * AttrCustomizeResources.Config.enemyAbilityHasteFlat
			});
		}
		Monster monster = entity as Monster;
		if (monster != null)
		{
			float bossMonsterHealthMultiplierByScaling;
			float bossMonsterDamageMultiplierByScaling;
			if (monster is BossMonster)
			{
				bossMonsterHealthMultiplierByScaling = NetworkedManagerBase<GameManager>.instance.GetBossMonsterHealthMultiplierByScaling(scalingFactor);
				bossMonsterDamageMultiplierByScaling = NetworkedManagerBase<GameManager>.instance.GetBossMonsterDamageMultiplierByScaling(scalingFactor);
				bossMonsterHealthMultiplierByScaling *= AttrCustomizeResources.Config.bossHealthMultiplier;
				bossMonsterDamageMultiplierByScaling *= AttrCustomizeResources.Config.bossDamageMultiplier;
			}
			else if (monster.type == Monster.MonsterType.MiniBoss)
			{
				bossMonsterHealthMultiplierByScaling = NetworkedManagerBase<GameManager>.instance.GetMiniBossMonsterHealthMultiplierByScaling(scalingFactor);
				bossMonsterDamageMultiplierByScaling = NetworkedManagerBase<GameManager>.instance.GetMiniBossMonsterDamageMultiplierByScaling(scalingFactor);
				bossMonsterHealthMultiplierByScaling *= AttrCustomizeResources.Config.miniBossHealthMultiplier;
				bossMonsterDamageMultiplierByScaling *= AttrCustomizeResources.Config.miniBossDamageMultiplier;
			}
			else
			{
				bossMonsterHealthMultiplierByScaling = NetworkedManagerBase<GameManager>.instance.GetRegularMonsterHealthMultiplierByScaling(scalingFactor);
				bossMonsterDamageMultiplierByScaling = NetworkedManagerBase<GameManager>.instance.GetRegularMonsterDamageMultiplierByScaling(scalingFactor);
				bossMonsterHealthMultiplierByScaling *= AttrCustomizeResources.Config.littleMonsterHealthMultiplier;
				bossMonsterDamageMultiplierByScaling *= AttrCustomizeResources.Config.littleMonsterDamageMultiplier;
			}
			int currentZoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
			bossMonsterHealthMultiplierByScaling = AttrCustomizeManager.ExponentialGrowth(currentZoneIndex, bossMonsterHealthMultiplierByScaling, AttrCustomizeResources.Config.extraHealthGrowthMultiplier);
			bossMonsterDamageMultiplierByScaling = AttrCustomizeManager.ExponentialGrowth(currentZoneIndex, bossMonsterDamageMultiplierByScaling, AttrCustomizeResources.Config.extraDamageGrowthMultiplier);
			entity.Status.AddStatBonus(new StatBonus
			{
				maxHealthPercentage = (bossMonsterHealthMultiplierByScaling - 1f) * 100f,
				attackDamagePercentage = (bossMonsterDamageMultiplierByScaling - 1f) * 100f,
				abilityPowerPercentage = (bossMonsterDamageMultiplierByScaling - 1f) * 100f
			});
		}
		else if (entity is Hero)
		{
			entity.takenHealProcessor.Add(delegate(ref HealData data, Actor actor, Entity target)
			{
				data.ApplyRawMultiplier(NetworkedManagerBase<GameManager>.instance.difficulty.healRawMultiplier);
			}, 100);
		}
	}
}
