using System.Collections;
using UnityEngine;

public class Shrine_Merchant_Backpack : Shrine, ICustomInteractable
{
	public float spawnSkillRatio;

	public float spawnDelay;

	public string nameRawText => DewLocalization.GetUIValue(GetType().Name + "_Name");

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Tooltip_PickUp");

	public Vector3 worldOffset => new Vector3(0f, 3f, 0f);

	protected override bool OnUse(Entity entity)
	{
		StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			Vector3 pivot = Dew.GetPositionOnGround(base.transform.position);
			if (Random.value <= spawnSkillRatio)
			{
				Loot_Skill loot = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Skill>();
				yield return new WaitForSeconds(spawnDelay);
				foreach (DewPlayer h in DewPlayer.humanPlayers)
				{
					if (!h.hero.IsNullInactiveDeadOrKnockedOut())
					{
						Rarity rarity = loot.SelectRarityNormal();
						Vector3 pos = Dew.GetGoodRewardPosition(pivot);
						loot.SelectSkillAndLevel(rarity, out var skill, out var level);
						level += SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel;
						Dew.CreateSkillTrigger(skill, pos, level, h);
						yield return null;
					}
				}
			}
			else
			{
				Loot_Gem loot2 = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>();
				yield return new WaitForSeconds(spawnDelay);
				foreach (DewPlayer h2 in DewPlayer.humanPlayers)
				{
					if (!h2.hero.IsNullInactiveDeadOrKnockedOut())
					{
						Rarity rarity2 = loot2.SelectRarityNormal();
						Vector3 pos2 = Dew.GetGoodRewardPosition(pivot);
						loot2.SelectGemAndQuality(rarity2, out var gem, out var quality);
						quality += SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality;
						Dew.CreateGem(gem, pos2, quality, h2);
						yield return null;
					}
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
