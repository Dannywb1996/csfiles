public class Shrine_Enlightenment : ChoiceShrine
{
	protected override void SetupSelections()
	{
		_choices.Clear();
		Loot_Gem loot = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>();
		Rarity rarity = loot.SelectRarityNormal();
		int duplicateCount = 0;
		for (int i = 0; i < itemCount; i++)
		{
			loot.SelectGemAndQuality(rarity, out var gem, out var quality);
			if (duplicateCount < 10)
			{
				bool isDuplicate = false;
				for (int j = 0; j < i; j++)
				{
					if (_choices[j].typeName == gem.GetType().Name)
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
				level = quality + SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality,
				typeName = gem.GetType().Name
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
