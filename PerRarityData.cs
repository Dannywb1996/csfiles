using System;

[Serializable]
public struct PerRarityData<T>
{
	public T common;

	public T rare;

	public T epic;

	public T legendary;

	public T character;

	public T identity;

	public T Get(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Common => common, 
			Rarity.Rare => rare, 
			Rarity.Epic => epic, 
			Rarity.Legendary => legendary, 
			Rarity.Character => character, 
			Rarity.Identity => identity, 
			_ => throw new ArgumentOutOfRangeException("rarity", rarity, null), 
		};
	}

	public void Set(Rarity rarity, T value)
	{
		switch (rarity)
		{
		case Rarity.Common:
			common = value;
			break;
		case Rarity.Rare:
			rare = value;
			break;
		case Rarity.Epic:
			epic = value;
			break;
		case Rarity.Legendary:
			legendary = value;
			break;
		case Rarity.Character:
			character = value;
			break;
		case Rarity.Identity:
			identity = value;
			break;
		default:
			throw new ArgumentOutOfRangeException("rarity", rarity, null);
		}
	}
}
