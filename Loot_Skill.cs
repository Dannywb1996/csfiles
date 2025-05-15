using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Loot_Skill : Loot
{
	public PerRarityData<float> skillRarityChance;

	public PerRarityData<float> skillRarityChanceHigh;

	public PerRarityData<Formula> skillLevelMinByZoneIndex;

	public PerRarityData<Formula> skillLevelMaxByZoneIndex;

	public AnimationCurve levelRandomCurve;

	public override IEnumerator OnLootRoutine(ClearSectionEventData data)
	{
		Rarity rarity = SelectRarityNormal();
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			_ = humanPlayer;
			Vector3 spawnPos = Loot.GetSpawnPos(data);
			SelectSkillAndLevel(rarity, out var skill, out var level);
			Dew.CreateSkillTrigger(skill, spawnPos, level);
			yield return new WaitForSeconds(0.2f);
		}
	}

	public Rarity SelectRarityNormal()
	{
		return Loot.SelectRarity(skillRarityChance);
	}

	public Rarity SelectRarityHigh()
	{
		return Loot.SelectRarity(skillRarityChanceHigh);
	}

	public void SelectSkillAndLevel(Rarity rarity, out SkillTrigger skill, out int level)
	{
		HashSet<string> hashSet = NetworkedManagerBase<LootManager>.instance.poolSkillsByRarity[rarity];
		if (AttrCustomizeResources.Config.enableHeroSkillAddShop)
		{
			switch (rarity)
			{
			case Rarity.Common:
			{
				List<string> other3 = new List<string> { "St_Q_GoldenBurst", "St_Q_HandCannon", "St_Q_IncendiaryRounds", "St_Q_Lunge", "St_Q_Fleche", "St_Q_CruelSun", "St_Q_Discipline", "St_Q_Lunge", "St_Q_EtherealInfluence", "St_Q_SuperNova" };
				hashSet.UnionWith(other3);
				break;
			}
			case Rarity.Rare:
			{
				List<string> other2 = new List<string> { "St_R_DangerousTheory", "St_R_QuickTrigger", "St_R_PrecisionShot", "St_R_UnbreakableDetermination", "St_R_Parry", "St_R_SanctuaryOfEl", "St_R_Cataclysm", "St_R_Tranquility" };
				hashSet.UnionWith(other2);
				break;
			}
			case Rarity.Epic:
			{
				List<string> other = new List<string> { "St_D_DisintegratingClaw", "St_D_SalamanderPowder", "St_D_DoubleTap", "St_D_AstridsMasterpiece", "St_D_MercyOfEl", "St_D_Resolve", "St_D_ExoticMatter", "St_D_ConvergencePoint" };
				hashSet.UnionWith(other);
				break;
			}
			}
		}
		string[] removeSkills = AttrCustomizeResources.Config.removeSkills;
		for (int i = 0; i < removeSkills.Length; i++)
		{
			hashSet.Remove(removeSkills[i]);
		}
		skill = DewResources.GetByShortTypeName<SkillTrigger>(hashSet.ElementAt(Random.Range(0, hashSet.Count)));
		float a = skillLevelMinByZoneIndex.Get(rarity).Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		float b = skillLevelMaxByZoneIndex.Get(rarity).Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		float f = Mathf.Lerp(a, b, levelRandomCurve.Evaluate(Random.value));
		level = Mathf.Clamp(Mathf.RoundToInt(f), 1, 100);
	}
}
