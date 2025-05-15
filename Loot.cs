using System;
using System.Collections;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Type)]
public abstract class Loot : MonoBehaviour
{
	[NonSerialized]
	public float currentChance;

	public float startChanceMin;

	public float startChanceMax;

	public float addedChanceMultiplier = 1f;

	public abstract IEnumerator OnLootRoutine(ClearSectionEventData data);

	public static Vector3 GetSpawnPos(ClearSectionEventData data)
	{
		Rift_RoomExit[] array = UnityEngine.Object.FindObjectsOfType<Rift_RoomExit>(includeInactive: true);
		Vector3 spawnPos = Dew.GetValidAgentDestination_LinearSweep(data.lastMonsterPosition, data.lastMonsterPosition + (UnityEngine.Random.insideUnitSphere * 4.5f).Flattened());
		Rift_RoomExit[] array2 = array;
		foreach (Rift_RoomExit p in array2)
		{
			float dist = Vector3.Distance(p.interactPivot.position, spawnPos);
			float preferredDistance = UnityEngine.Random.Range(3f, 5f);
			if (dist < preferredDistance)
			{
				Vector3 dir = (spawnPos - p.interactPivot.position).Flattened().normalized;
				spawnPos += dir * (preferredDistance - dist);
				spawnPos = Dew.GetValidAgentDestination_LinearSweep(data.lastMonsterPosition, spawnPos);
			}
		}
		return spawnPos;
	}

	public static Rarity SelectRarity(PerRarityData<float> chances)
	{
		float val = UnityEngine.Random.value;
		Rarity rarity = Rarity.Common;
		if (val < chances.legendary)
		{
			rarity = Rarity.Legendary;
		}
		else if (val < chances.legendary + chances.epic)
		{
			rarity = Rarity.Epic;
		}
		else if (val < chances.legendary + chances.epic + chances.rare)
		{
			rarity = Rarity.Rare;
		}
		return rarity;
	}
}
