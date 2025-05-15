using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Loot_Gem : Loot
{
	public PerRarityData<float> gemRarityChance;

	public PerRarityData<float> gemRarityChanceHigh;

	public PerRarityData<Formula> gemQualityMinByZoneIndex;

	public PerRarityData<Formula> gemQualityMaxByZoneIndex;

	public AnimationCurve qualityRandomCurve;

	public override IEnumerator OnLootRoutine(ClearSectionEventData data)
	{
		Rarity rarity = SelectRarityNormal();
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			_ = humanPlayer;
			Vector3 spawnPos = Loot.GetSpawnPos(data);
			SelectGemAndQuality(rarity, out var gem, out var quality);
			Dew.CreateGem(gem, spawnPos, quality);
			yield return new WaitForSeconds(0.2f);
		}
	}

	public Rarity SelectRarityNormal()
	{
		return Loot.SelectRarity(gemRarityChance);
	}

	public Rarity SelectRarityHigh()
	{
		return Loot.SelectRarity(gemRarityChanceHigh);
	}

	public int SelectQuality(Rarity rarity)
	{
		float a = gemQualityMinByZoneIndex.Get(rarity).Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		float b = gemQualityMaxByZoneIndex.Get(rarity).Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(a, b, qualityRandomCurve.Evaluate(Random.value)) / 10f) * 10, 10, 2000);
	}

	public void SelectGemAndQuality(Rarity rarity, out Gem gem, out int quality)
	{
		HashSet<string> hashSet = NetworkedManagerBase<LootManager>.instance.poolGemsByRarity[rarity];
		string[] removeGems = AttrCustomizeResources.Config.removeGems;
		for (int i = 0; i < removeGems.Length; i++)
		{
			hashSet.Remove(removeGems[i]);
		}
		gem = DewResources.GetByShortTypeName<Gem>(hashSet.ElementAt(Random.Range(0, hashSet.Count)));
		quality = SelectQuality(rarity);
	}
}
