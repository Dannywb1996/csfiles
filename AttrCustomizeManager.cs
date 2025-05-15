using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Mirror;
using UnityEngine;

public class AttrCustomizeManager
{
	private class Ad_MonsterLevelUp
	{
		public StatBonus bonus;
	}

	private static int _lastObliviaxQuestZoneIndex;

	private static bool _globalOnceFlag;

	public static void ExecuteGlobalOnce()
	{
		_globalOnceFlag = true;
		ModifyBeneficialNodeMultiplier();
		ModifyHealRawMultiplier();
	}

	public static void ExecuteInGameOnce()
	{
		if (!_globalOnceFlag)
		{
			ExecuteGlobalOnce();
		}
		LucidDreamEmbraceMortality();
		LucidDreamBonVoyage();
		LucidDreamGrievousWounds();
		LucidDreamTheDarkestUrge();
		LucidDreamWild();
		LucidDreamMadLife();
		LucidDreamSparklingDreamFlask();
	}

	private static void LucidDreamSparklingDreamFlask()
	{
		if (!AttrCustomizeResources.Config.enableLucidDreamSparklingDreamFlask)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.onActorAdd += new Action<Actor>(LucidDreamSparklingDreamFlaskOnActorAdd);
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			LucidDreamSparklingDreamFlaskOnActorAdd(allActor);
		}
	}

	private static void LucidDreamSparklingDreamFlaskOnActorAdd(Actor obj)
	{
		if (obj is Ai_RegenOrb_Projectile ai_RegenOrb_Projectile)
		{
			ai_RegenOrb_Projectile.actionOverride = delegate(Entity target)
			{
				float value = GetBaseRewardAmount_DreamDust() * UnityEngine.Random.Range(0.9f, 1.1f) * 0.4f;
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(value), target.agentPosition, (Hero)target);
			};
		}
		Type type = obj.GetType();
		if (!(type.ToString() == "Shrine_Guidance"))
		{
			return;
		}
		FieldInfo field = type.GetField("actionOverride", BindingFlags.Instance | BindingFlags.Public);
		if (field != null)
		{
			field.SetValue(obj, (Action<Entity>)delegate(Entity target)
			{
				float value = GetBaseRewardAmount_DreamDust() * UnityEngine.Random.Range(0.9f, 1.1f) * 1.2f;
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(value), obj.position, (Hero)target);
			});
		}
	}

	public static float GetBaseRewardAmount_DreamDust()
	{
		int currentZoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		Loot_Gem lootInstance = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>();
		lootInstance.gemQualityMinByZoneIndex.Get(Rarity.Rare).Evaluate(currentZoneIndex);
		float f = lootInstance.gemQualityMaxByZoneIndex.Get(Rarity.Rare).Evaluate(currentZoneIndex);
		return (float)NetworkedManagerBase<GameManager>.instance.GetGemUpgradeDreamDustCost(Mathf.RoundToInt(f)) * (1f + (float)NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * 0.1f) * (1f + (float)currentZoneIndex * 0.125f);
	}

	private static void LucidDreamMadLife()
	{
		if (AttrCustomizeResources.Config.enableLucidDreamMadLife)
		{
			NetworkedManagerBase<GameManager>.instance.predictionStrengthOverride = () => 0.8f + UnityEngine.Random.value * 0.2f;
		}
	}

	private static void LucidDreamWild()
	{
		if (AttrCustomizeResources.Config.enableLucidDreamWild)
		{
			NetworkedManagerBase<ActorManager>.instance.onEntityAdd += new Action<Entity>(LucidDreamWildOnEntityAdd);
		}
	}

	private static void LucidDreamWildOnEntityAdd(Entity obj)
	{
		if (obj is Monster && !(obj.owner != DewPlayer.creep) && !obj.IsAnyBoss() && !obj.Status.HasStatusEffect<Se_HunterBuff>())
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateStatusEffect<Se_HunterBuff>(obj, new CastInfo(obj, obj)).enableGoldAndExpDrops = true;
			DewResources.GetByType<RoomMod_Hunted>().ApplyHunterStatBonusAndAIPrediction(obj, NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		}
	}

	private static void LucidDreamTheDarkestUrge()
	{
		if (!AttrCustomizeResources.Config.enableLucidDreamTheDarkestUrge)
		{
			return;
		}
		DewPlayer.creep.enemies.Add(DewPlayer.creep);
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			foreach (DewPlayer humanPlayer2 in DewPlayer.humanPlayers)
			{
				if (!(humanPlayer == humanPlayer2))
				{
					humanPlayer.neutrals.Add(humanPlayer2);
				}
			}
		}
		NetworkedManagerBase<ActorManager>.instance.onEntityAdd += new Action<Entity>(LucidDreamTheDarkestUrgeOnEntityAdd);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			LucidDreamTheDarkestUrgeOnEntityAdd(allEntity);
		}
	}

	private static void LucidDreamTheDarkestUrgeOnEntityAdd(Entity entity)
	{
		if (entity is Monster monster)
		{
			monster.ActorEvent_OnKill += new Action<EventInfoKill>(LucidDreamTheDarkestUrgeActorEventOnKill);
		}
	}

	private static void LucidDreamTheDarkestUrgeActorEventOnKill(EventInfoKill obj)
	{
		Entity firstEntity = obj.actor.firstEntity;
		if (!(firstEntity == null) && !(obj.victim == null) && obj.victim is Monster)
		{
			if (!firstEntity.TryGetData<Ad_MonsterLevelUp>(out var data))
			{
				data = new Ad_MonsterLevelUp
				{
					bonus = firstEntity.Status.AddStatBonus(new StatBonus())
				};
				firstEntity.AddData(data);
			}
			if (obj.victim.TryGetData<Ad_MonsterLevelUp>(out var data2))
			{
				data.bonus.attackDamagePercentage += data2.bonus.attackDamagePercentage;
				data.bonus.abilityPowerPercentage += data2.bonus.abilityPowerPercentage;
				data.bonus.attackSpeedPercentage += data2.bonus.attackSpeedPercentage;
				data.bonus.movementSpeedPercentage += data2.bonus.movementSpeedPercentage;
				data.bonus.abilityHasteFlat += data2.bonus.abilityHasteFlat;
			}
			data.bonus.maxHealthFlat += obj.victim.maxHealth * 0.8f;
			data.bonus.attackDamagePercentage += 25f;
			data.bonus.abilityPowerPercentage += 25f;
			data.bonus.attackSpeedPercentage += 10f;
			data.bonus.movementSpeedPercentage += 10f;
			data.bonus.abilityHasteFlat += 15f;
			firstEntity.Heal(obj.victim.maxHealth * 0.8f).Dispatch(firstEntity);
		}
	}

	private static void LucidDreamGrievousWounds()
	{
		if (!AttrCustomizeResources.Config.enableLucidDreamGrievousWounds)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.onEntityAdd += new Action<Entity>(LucidDreamGrievousWoundsOnEntityAdd);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			LucidDreamGrievousWoundsOnEntityAdd(allEntity);
		}
	}

	private static void LucidDreamGrievousWoundsOnEntityAdd(Entity entity)
	{
		if (entity is Hero)
		{
			entity.takenHealProcessor.Add(LucidDreamGrievousWoundsHealProcessor, 200);
			entity.takenShieldProcessor.Add(LucidDreamGrievousWoundsShieldProcessor);
		}
	}

	private static void LucidDreamGrievousWoundsHealProcessor(ref HealData data, Actor actor, Entity target)
	{
		data.ApplyRawMultiplier(0.5f);
	}

	private static void HealProcessor(ref HealData data, Actor actor, Entity target)
	{
		data.ApplyRawMultiplier(0.5f);
	}

	private static void LucidDreamGrievousWoundsShieldProcessor(ref HealData data, Actor actor, Entity target)
	{
		data.ApplyRawMultiplier(0.5f);
	}

	private static void LucidDreamBonVoyage()
	{
		if (AttrCustomizeResources.Config.enableLucidDreamBonVoyage)
		{
			NetworkedManagerBase<ZoneManager>.instance.isHuntAdvanceDisabled = true;
			NetworkedManagerBase<ZoneManager>.instance.hunterStartNodeIndex = -1;
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(LucidDreamBonVoyageClientEventOnRoomLoaded);
		}
	}

	private static void LucidDreamBonVoyageClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		NetworkedManagerBase<ZoneManager>.instance.isHuntAdvanceDisabled = true;
		NetworkedManagerBase<ZoneManager>.instance.hunterStartNodeIndex = -1;
	}

	private static void LucidDreamEmbraceMortality()
	{
		if (!AttrCustomizeResources.Config.enableLucidDreamEmbraceMortality)
		{
			return;
		}
		NetworkedManagerBase<ActorManager>.instance.onEntityAdd += new Action<Entity>(LucidDreamEmbraceMortalityOnEntityAdd);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			LucidDreamEmbraceMortalityOnEntityAdd(allEntity);
		}
	}

	private static void LucidDreamEmbraceMortalityOnEntityAdd(Entity entity)
	{
		entity.dealtDamageProcessor.Add(LucidDreamEmbraceMortalityProcessor);
	}

	private static void LucidDreamEmbraceMortalityProcessor(ref DamageData data, Actor actor, Entity target)
	{
		Entity firstEntity = actor.firstEntity;
		if (!(firstEntity == null) && !(target == null) && !(firstEntity == target))
		{
			data.ApplyAmplification(1f);
		}
	}

	private static void ModifyHealRawMultiplier()
	{
		foreach (DewDifficultySettings item in DewResources.FindAllByNameSubstring<DewDifficultySettings>("diff"))
		{
			item.healRawMultiplier = AttrCustomizeResources.Config.healRawMultiplier * item.healRawMultiplier;
		}
	}

	private static void ModifyBeneficialNodeMultiplier()
	{
		foreach (DewDifficultySettings item in DewResources.FindAllByNameSubstring<DewDifficultySettings>("diff"))
		{
			item.beneficialNodeMultiplier = AttrCustomizeResources.Config.beneficialNodeMultiplier * item.beneficialNodeMultiplier;
		}
	}

	public static void ExecuteInGameOnceAtGameLoaded()
	{
		SendStartGameNotice();
		ResetObliviaxQuestZone();
	}

	private static void ResetObliviaxQuestZone()
	{
		_lastObliviaxQuestZoneIndex = -100;
	}

	public static void ExecuteLoopOnce()
	{
	}

	public static void ExecuteZoneOnce()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex == 0)
		{
			ExecuteInGameOnceAtGameLoaded();
		}
		WorldReveal();
		RemoveRoomMod();
		DamageRanking();
	}

	private static void WorldReveal()
	{
		if (AttrCustomizeResources.Config.enableWorldReveal)
		{
			ConsoleCommands.WorldReveal();
		}
	}

	private static void DamageRanking()
	{
		if (!AttrCustomizeResources.Config.enableDamageRanking)
		{
			return;
		}
		DewGameResult dewGameResult = (DewGameResult)typeof(GameResultManager).GetField("_tracked", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(NetworkedManagerBase<GameResultManager>.instance);
		if (dewGameResult != null)
		{
			List<(string, float, float)> list = new List<(string, float, float)>();
			for (int i = 0; i < dewGameResult.players.Count; i++)
			{
				DewGameResult.PlayerData playerData = dewGameResult.players[i];
				string playerProfileName = playerData.playerProfileName;
				float dealtDamageToEnemies = playerData.dealtDamageToEnemies;
				float maxDealtSingleDamageToEnemy = playerData.maxDealtSingleDamageToEnemy;
				list.Add(ValueTuple.Create(playerProfileName, dealtDamageToEnemies, maxDealtSingleDamageToEnemy));
			}
			list.Sort(((string, float, float) a, (string, float, float) b) => b.Item2.CompareTo(a.Item2));
			StringBuilder sb = new StringBuilder();
			sb.Append("Damage Ranking:\n");
			for (int num = 0; num < list.Count; num++)
			{
				(string, float, float) tuple = list[num];
				string item = tuple.Item1;
				float item2 = tuple.Item2;
				float item3 = tuple.Item3;
				string text = item2.ToString("#,0", CultureInfo.InvariantCulture);
				string text2 = item3.ToString("#,0", CultureInfo.InvariantCulture);
				sb.Append(item + ":\n  Total Damage: " + text + " - Highest Hit: " + text2 + "\n");
			}
			Dew.CallDelayed(delegate
			{
				ChatManager.Message message = new ChatManager.Message
				{
					type = ChatManager.MessageType.Raw,
					content = sb.ToString()
				};
				NetworkedManagerBase<ChatManager>.instance.BroadcastChatMessage(message);
			}, 100);
		}
	}

	private static void QuestHuntedByObliviaxRepeatable()
	{
		List<ModifierData> modifiers = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers;
		bool flag = false;
		for (int i = 0; i < modifiers.Count; i++)
		{
			if (modifiers[i].type == "RoomMod_Hunted")
			{
				flag = true;
			}
		}
		int currentZoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		if (!flag)
		{
			return;
		}
		IReadOnlyList<DewQuest> activeQuests = NetworkedManagerBase<QuestManager>.instance.activeQuests;
		for (int j = 0; j < activeQuests.Count; j++)
		{
			if (activeQuests[j].GetType().Name == "Quest_HuntedByObliviax")
			{
				_lastObliviaxQuestZoneIndex = currentZoneIndex;
			}
		}
		if (_lastObliviaxQuestZoneIndex > -1 && _lastObliviaxQuestZoneIndex != currentZoneIndex && AttrCustomizeResources.Config.enableQuestHuntedByObliviaxRepeatable)
		{
			DewQuest prefab = DewResources.FindOneByTypeSubstring<DewQuest>("Quest_HuntedByObliviax");
			NetworkedManagerBase<QuestManager>.instance.StartQuest(prefab);
		}
	}

	private static void RemoveRoomMod()
	{
		List<WorldNodeData> list = NetworkedManagerBase<ZoneManager>.instance.nodes.ToList();
		if (!AttrCustomizeResources.Config.enableArtifactQuest)
		{
			for (int i = 0; i < list.Count; i++)
			{
				NetworkedManagerBase<ZoneManager>.instance.RemoveModifier<RoomMod_Artifact>(i);
			}
		}
		if (AttrCustomizeResources.Config.enableFragmentOfRadianceBossQuest)
		{
			return;
		}
		for (int j = 0; j < list.Count; j++)
		{
			int num = list[j].FindModifierIndex("RoomMod_FragmentOfRadiance_StartProp");
			if (num != 0)
			{
				NetworkedManagerBase<ZoneManager>.instance.RemoveModifier(j, num);
			}
		}
	}

	private static void SendStartGameNotice()
	{
		Dew.CallDelayed(delegate
		{
			NetworkedManagerBase<ChatManager>.instance.BroadcastChatMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.Raw,
				content = BuildStartGameNotice()
			});
		}, 200);
	}

	public static void ExecuteRoomOnce()
	{
		DropGold();
		QuestHuntedByObliviaxRepeatable();
	}

	private static void DropGold()
	{
		Room instance = SingletonDewNetworkBehaviour<Room>.instance;
		if (!instance.isRevisit)
		{
			Vector3 heroSpawnPosition = instance.GetHeroSpawnPosition();
			int num = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * AttrCustomizeResources.Config.firstVisitDropGoldCountAddByZone;
			int num2 = NetworkedManagerBase<ZoneManager>.instance.loopIndex * AttrCustomizeResources.Config.firstVisitDropGoldCountAddByLoop;
			int amount = (AttrCustomizeResources.Config.firstVisitDropGoldCount + num2 + num) * DewPlayer.humanPlayers.Count;
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount, heroSpawnPosition);
		}
	}

	public static float ExponentialGrowth(int x, double initialY, double multiplier)
	{
		if (multiplier - 0.0 < 1E-05)
		{
			return (float)initialY;
		}
		return (float)(initialY * Math.Pow(multiplier, x));
	}

	public static bool TryGetNodeIndexForNextGoal(DewQuest quest, GetNodeIndexSettings s, out int nodeIndex)
	{
		ZoneManager zoneManager = NetworkedManagerBase<ZoneManager>.instance;
		int currentNodeIndex = zoneManager.currentNodeIndex;
		SyncList<WorldNodeData> nodes = zoneManager.nodes;
		int exitNodeIndex = 0;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].type == WorldNodeType.ExitBoss)
			{
				exitNodeIndex = i;
				break;
			}
		}
		int currentDistToExit = zoneManager.GetNodeDistance(currentNodeIndex, exitNodeIndex);
		int i2 = (nodeIndex = Dew.SelectBestIndexWithScore(nodes, GetScore));
		return GetScore(nodes[i2], i2) > -5000f;
		float GetScore(WorldNodeData data, int num2)
		{
			float num = 0f;
			if (data.IsSidetrackNode())
			{
				num -= 10000f;
			}
			if (!s.allowedTypes.Contains(data.type))
			{
				num -= 10000f;
			}
			if (num2 == currentNodeIndex)
			{
				num = ((!AttrCustomizeResources.Config.enableCurrentNodeGenerateLostSoul || !(quest is Quest_LostSoul)) ? (num - 10000f) : (num - -11000f));
			}
			switch (data.status)
			{
			case WorldNodeStatus.Revealed:
			case WorldNodeStatus.RevealedFull:
				num -= 2.5f;
				break;
			case WorldNodeStatus.HasVisited:
				num -= 10000f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case WorldNodeStatus.Unexplored:
				break;
			}
			int nodeDistance = zoneManager.GetNodeDistance(currentNodeIndex, num2);
			if (nodeDistance < s.desiredDistance.x)
			{
				num -= (float)(s.desiredDistance.x - nodeDistance) * 1f;
			}
			num = ((nodeDistance <= s.desiredDistance.y) ? (num + 5f) : (num - (float)(nodeDistance - s.desiredDistance.y) * 1f));
			if (s.preferCloserToExit)
			{
				int num3 = currentDistToExit - zoneManager.GetNodeDistance(num2, exitNodeIndex);
				num = ((num3 <= 0) ? (num + (float)num3 * 3f) : (num + (float)num3 * 0.75f));
			}
			if (s.preferNoMainModifier && nodes[num2].HasMainModifier())
			{
				num -= 6f;
			}
			return num + UnityEngine.Random.Range(-1.5f, 1.5f);
		}
	}

	private static string BuildStartGameNotice()
	{
		/*Error: Empty body found. Decompiled assembly might be a reference assembly.*/;
	}

	static AttrCustomizeManager()
	{
		_lastObliviaxQuestZoneIndex = -100;
		_globalOnceFlag = false;
	}

	public static int GetMaxGemCount(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => AttrCustomizeResources.Config.skillQGemCount, 
			HeroSkillLocation.W => AttrCustomizeResources.Config.skillWGemCount, 
			HeroSkillLocation.E => AttrCustomizeResources.Config.skillEGemCount, 
			HeroSkillLocation.R => AttrCustomizeResources.Config.skillRGemCount, 
			HeroSkillLocation.Identity => AttrCustomizeResources.Config.skillIdentityGemCount, 
			HeroSkillLocation.Movement => AttrCustomizeResources.Config.skillMovementGemCount, 
			_ => 0, 
		};
	}
}
