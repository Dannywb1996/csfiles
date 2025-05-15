using System;
using System.Collections;
using UnityEngine;

public class Shrine_Concept : Shrine
{
	public GameObject fxEmpowered;

	[NonSerialized]
	public Gem gemOverride;

	[NonSerialized]
	public int? qualityOverride;

	[NonSerialized]
	public Vector3? positionOverride;

	public override bool isRegularReward => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return null;
			if (base.isServer && SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward && base.isAvailable)
			{
				FxPlayNetworked(fxEmpowered);
			}
		}
	}

	protected override bool OnUse(Entity entity)
	{
		StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			FxStopNetworked(fxEmpowered);
			Vector3 pivot = GetRandomSpawnPosition(entity.position);
			Loot_Gem loot = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>();
			yield return new WaitForSeconds(0.5f);
			foreach (DewPlayer h in DewPlayer.humanPlayers)
			{
				if (!h.hero.IsNullInactiveDeadOrKnockedOut())
				{
					Rarity rarity = (SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward ? loot.SelectRarityHigh() : loot.SelectRarityNormal());
					Vector3 pos = positionOverride ?? Dew.GetGoodRewardPosition(pivot);
					loot.SelectGemAndQuality(rarity, out var gem, out var quality);
					quality += SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality;
					if (gemOverride != null)
					{
						gem = gemOverride;
					}
					if (qualityOverride.HasValue)
					{
						quality = qualityOverride.Value;
					}
					Dew.CreateGem(gem, pos, quality, h);
					yield return null;
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
