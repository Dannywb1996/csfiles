using UnityEngine;

public static class PerDataHelper
{
	public static PerRarityData<int> GetRandom(PerRarityData<int> min, PerRarityData<int> max)
	{
		return new PerRarityData<int>
		{
			common = Random.Range(min.common, max.common + 1),
			rare = Random.Range(min.rare, max.rare + 1),
			epic = Random.Range(min.epic, max.epic + 1),
			legendary = Random.Range(min.legendary, max.legendary + 1)
		};
	}

	public static PerRarityData<float> GetRandom(PerRarityData<float> min, PerRarityData<float> max)
	{
		return new PerRarityData<float>
		{
			common = Random.Range(min.common, max.common),
			rare = Random.Range(min.rare, max.rare),
			epic = Random.Range(min.epic, max.epic),
			legendary = Random.Range(min.legendary, max.legendary)
		};
	}
}
