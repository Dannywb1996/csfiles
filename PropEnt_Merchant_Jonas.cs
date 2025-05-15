using System;
using UnityEngine;

public class PropEnt_Merchant_Jonas : PropEnt_Merchant_Base
{
	public int skillTypeCount = 3;

	public int gemTypeCount = 3;

	public Vector2 gemQuantity;

	public Vector2 skillQuantity;

	private int _fleeDamageCount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage obj)
		{
			if (!(obj.actor is ElementalStatusEffect))
			{
				_fleeDamageCount++;
			}
		};
		Dew.CallDelayed(delegate
		{
			if (_merchandises.Count > 0)
			{
				return;
			}
			foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
			{
				PopulatePlayerMerchandises(humanPlayer);
				remainingRefreshes[humanPlayer.netId] = humanPlayer.allowedShopRefreshes;
			}
		});
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (base.Status.maxHealth < 1E+09f)
		{
			base.Status.maxHealth = 1E+09f;
			base.Status.currentHealth = base.Status.maxHealth;
		}
		if (_fleeDamageCount >= 1 && _fleeDamageCount <= 10)
		{
			foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
			{
				OnRefresh(humanPlayer);
				remainingRefreshes[humanPlayer.netId] = humanPlayer.allowedShopRefreshes;
			}
			_fleeDamageCount = 11;
		}
		bool flag = _fleeDamageCount >= 120 || base.Status.missingHealth / base.Status.maxHealth >= 0.9f;
		if (base.AI.Helper_CanBeCast<At_Prop_Merchant_Flee>() && flag)
		{
			base.AI.Helper_CastAbilityAuto<At_Prop_Merchant_Flee>();
		}
	}

	private MerchandiseData GetSkill()
	{
		Loot_Skill lootInstance = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Skill>();
		Rarity rarity = lootInstance.SelectRarityNormal();
		lootInstance.SelectSkillAndLevel(rarity, out var skill, out var num);
		return new MerchandiseData
		{
			type = MerchandiseType.Skill,
			itemName = skill.GetType().Name,
			level = num,
			count = Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(skillQuantity.x, skillQuantity.y)))
		};
	}

	private MerchandiseData GetGem()
	{
		Loot_Gem lootInstance = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>();
		Rarity rarity = lootInstance.SelectRarityNormal();
		lootInstance.SelectGemAndQuality(rarity, out var gem, out var quality);
		return new MerchandiseData
		{
			type = MerchandiseType.Gem,
			itemName = gem.GetType().Name,
			level = quality,
			count = Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(gemQuantity.x, gemQuantity.y)))
		};
	}

	private void UpdateItemPrices(DewPlayer player, MerchandiseData[] arr)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			MerchandiseData merchandiseData = arr[i];
			if (merchandiseData.type == MerchandiseType.Gem)
			{
				Gem byShortTypeName = DewResources.GetByShortTypeName<Gem>(merchandiseData.itemName);
				merchandiseData.price = Cost.Gold(Gem.GetBuyGold(player, byShortTypeName.rarity, merchandiseData.level));
			}
			else if (merchandiseData.type == MerchandiseType.Skill)
			{
				SkillTrigger byShortTypeName2 = DewResources.GetByShortTypeName<SkillTrigger>(merchandiseData.itemName);
				merchandiseData.price = Cost.Gold(SkillTrigger.GetBuyGold(player, byShortTypeName2.rarity, merchandiseData.level));
			}
			else
			{
				merchandiseData.price = Cost.Gold(99999);
			}
			arr[i] = merchandiseData;
		}
	}

	protected override void OnRefresh(DewPlayer player)
	{
		base.OnRefresh(player);
		PopulatePlayerMerchandises(player);
	}

	private MerchandiseData[] GetBaseSkills()
	{
		MerchandiseData[] array = new MerchandiseData[skillTypeCount];
		for (int i = 0; i < skillTypeCount; i++)
		{
			array[i] = GetSkill();
		}
		return array;
	}

	private MerchandiseData[] GetBaseGems()
	{
		MerchandiseData[] array = new MerchandiseData[gemTypeCount];
		for (int i = 0; i < gemTypeCount; i++)
		{
			array[i] = GetGem();
		}
		return array;
	}

	private void PopulatePlayerMerchandises(DewPlayer player)
	{
		MerchandiseData[] baseSkills = GetBaseSkills();
		MerchandiseData[] baseGems = GetBaseGems();
		MerchandiseData[] array = new MerchandiseData[skillTypeCount + gemTypeCount + player.shopAddedItems * 2];
		Array.Copy(baseSkills, 0, array, 0, baseSkills.Length);
		int num = skillTypeCount;
		int num2 = num + player.shopAddedItems;
		for (int i = num; i < num2; i++)
		{
			array[i] = GetSkill();
		}
		Array.Copy(baseGems, 0, array, skillTypeCount + player.shopAddedItems, baseGems.Length);
		int num3 = skillTypeCount + player.shopAddedItems + gemTypeCount;
		num2 = num3 + player.shopAddedItems;
		for (int j = num3; j < num2; j++)
		{
			array[j] = GetGem();
		}
		UpdateItemPrices(player, array);
		_merchandises[player.netId] = array;
	}

	private void MirrorProcessed()
	{
	}
}
