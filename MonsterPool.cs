using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Monster Pool", menuName = "Monster Pool")]
public class MonsterPool : ScriptableObject
{
	[Serializable]
	public class SpawnRuleEntry
	{
		public AssetReferenceGameObject monsterRef;

		public float chance = 1f;

		public int minCount = 1;

		public int maxCount = 1;

		public List<MonsterSpawnCondition> conditions;

		public bool useAnd = true;

		public Monster monster => DewResources.GetByGuid<Monster>(monsterRef.AssetGUID);

		public bool EvaluateCondition()
		{
			if (conditions == null || conditions.Count <= 0)
			{
				return true;
			}
			if (useAnd)
			{
				foreach (MonsterSpawnCondition condition in conditions)
				{
					if (!condition.EvaluateCondition())
					{
						return false;
					}
				}
				return true;
			}
			foreach (MonsterSpawnCondition condition2 in conditions)
			{
				if (condition2.EvaluateCondition())
				{
					return true;
				}
			}
			return false;
		}

		public SpawnRuleEntry Clone()
		{
			SpawnRuleEntry clone = (SpawnRuleEntry)MemberwiseClone();
			if (clone.conditions != null)
			{
				clone.conditions = new List<MonsterSpawnCondition>(clone.conditions);
			}
			return clone;
		}
	}

	private const int SkipWithoutSelectDisableRNGThreshold = 10;

	private const int SkipWithoutSelectLimit = 16;

	public bool scrambleOrder = true;

	public List<SpawnRuleEntry> entries = new List<SpawnRuleEntry>();

	public List<SpawnRuleEntry> GetFilteredEntries()
	{
		List<SpawnRuleEntry> list = new List<SpawnRuleEntry>();
		foreach (SpawnRuleEntry e in entries)
		{
			if (Dew.IsMonsterIncludedInGame(DewResources.database.guidToType[e.monsterRef.AssetGUID].Name))
			{
				list.Add(e);
			}
		}
		return list;
	}

	public IEnumerator<Monster> GetMonsters(int sectionIndex)
	{
		int entryIndex = 0;
		int skipCountWithoutSelect = 0;
		List<SpawnRuleEntry> sampledEntries = GetFilteredEntries();
		if (scrambleOrder)
		{
			sampledEntries.Shuffle();
		}
		while (true)
		{
			SpawnRuleEntry currentEntry = sampledEntries[entryIndex];
			int count = UnityEngine.Random.Range(currentEntry.minCount, currentEntry.maxCount + 1);
			if (((skipCountWithoutSelect >= 10) ? float.NegativeInfinity : UnityEngine.Random.value) > currentEntry.chance || !currentEntry.EvaluateCondition() || count <= 0)
			{
				if (skipCountWithoutSelect <= 16)
				{
					skipCountWithoutSelect++;
					IncrementEntryIndex();
					continue;
				}
				Debug.LogWarning("Spawn rule '" + base.name + "' skipped too much without any spawn. Wrong configuration? Ignoring requirements...");
			}
			for (int i = 0; i < count; i++)
			{
				skipCountWithoutSelect = 0;
				yield return currentEntry.monster;
			}
			IncrementEntryIndex();
		}
		void IncrementEntryIndex()
		{
			entryIndex++;
			if (entryIndex == sampledEntries.Count)
			{
				entryIndex = 0;
				if (scrambleOrder)
				{
					sampledEntries = GetFilteredEntries();
					sampledEntries.Shuffle();
				}
			}
		}
	}
}
