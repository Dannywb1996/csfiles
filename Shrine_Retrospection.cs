public class Shrine_Retrospection : ChoiceShrine
{
	protected override void SetupSelections()
	{
		_choices.Clear();
		Loot_Skill loot = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Skill>();
		Rarity rarity = loot.SelectRarityNormal();
		int duplicateCount = 0;
		for (int i = 0; i < itemCount; i++)
		{
			loot.SelectSkillAndLevel(rarity, out var skill, out var level);
			if (duplicateCount < 10)
			{
				bool isDuplicate = false;
				for (int j = 0; j < i; j++)
				{
					if (_choices[j].typeName == skill.GetType().Name)
					{
						isDuplicate = true;
						break;
					}
				}
				if (isDuplicate)
				{
					duplicateCount++;
					i--;
					continue;
				}
			}
			_choices.Add(new ChoiceShrineItem
			{
				level = level + SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel,
				typeName = skill.GetType().Name
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
