using System;
using System.Collections;
using UnityEngine;

public class Shrine_Memory : Shrine
{
	public GameObject fxEmpowered;

	[NonSerialized]
	public SkillTrigger skillOverride;

	[NonSerialized]
	public int? levelOverride;

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
			Loot_Skill loot = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Skill>();
			yield return new WaitForSeconds(0.5f);
			foreach (DewPlayer h in DewPlayer.humanPlayers)
			{
				if (!h.hero.IsNullInactiveDeadOrKnockedOut())
				{
					Rarity rarity = (SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward ? loot.SelectRarityHigh() : loot.SelectRarityNormal());
					Vector3 pos = positionOverride ?? Dew.GetGoodRewardPosition(pivot);
					loot.SelectSkillAndLevel(rarity, out var skill, out var level);
					level += SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel;
					if (skillOverride != null)
					{
						skill = skillOverride;
					}
					if (levelOverride.HasValue)
					{
						level = levelOverride.Value;
					}
					Dew.CreateSkillTrigger(skill, pos, level, h);
					yield return null;
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
