using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Loot Rule", menuName = "Loot Rule")]
public class LootRule : ScriptableObject
{
	[Serializable]
	public class Entry
	{
		public Loot loot;

		public float minChance;

		public float maxChance;
	}

	public bool allowMultipleReward;

	public List<Entry> entries = new List<Entry>();
}
