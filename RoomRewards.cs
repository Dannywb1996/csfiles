using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

[RoomComponentStartDependency(typeof(RoomModifiers))]
[RoomComponentStartDependency(typeof(RoomMonsters))]
public class RoomRewards : RoomComponent
{
	private struct DroppedItem
	{
		public Type type;

		public int level;

		public Vector3 pos;
	}

	public bool disableSpecialRoomReward;

	[NonSerialized]
	public bool giveHighRarityReward;

	[NonSerialized]
	public int skillBonusLevel;

	[NonSerialized]
	public int gemBonusQuality;

	private bool _isSpecialRoom => SceneManager.GetActiveScene().name.Contains("_Special_");

	public bool isRegularRewardDisabled { get; private set; }

	[Server]
	public void DisableRegularRewards()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomRewards::DisableRegularRewards()' called when server was not active");
		}
		else
		{
			if (isRegularRewardDisabled)
			{
				return;
			}
			isRegularRewardDisabled = true;
			Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
			foreach (Actor a in array)
			{
				if (a.FindFirstAncestorOfType<RoomModifierBase>() != null || !(a is IProp prop))
				{
					continue;
				}
				if (a is Shrine_Memory m && !giveHighRarityReward)
				{
					if (m.isAvailable)
					{
						m.Destroy();
						if (NetworkedManagerBase<ZoneManager>.instance._nextRewards != null)
						{
							NetworkedManagerBase<ZoneManager>.instance._nextRewards.Insert(0, RoomRewardFlowItemType.Skill);
						}
					}
				}
				else if (a is Shrine_Concept c && !giveHighRarityReward)
				{
					if (c.isAvailable)
					{
						c.Destroy();
						if (NetworkedManagerBase<ZoneManager>.instance._nextRewards != null)
						{
							NetworkedManagerBase<ZoneManager>.instance._nextRewards.Insert(0, RoomRewardFlowItemType.Gem);
						}
					}
				}
				else if (prop.isRegularReward)
				{
					a.Destroy();
				}
			}
		}
	}

	public override void OnRoomStartServer(WorldNodeSaveData save)
	{
		base.OnRoomStartServer(save);
		if (save == null)
		{
			if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Combat)
			{
				StartCoroutine(Routine());
			}
			SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(delegate
			{
				StartCoroutine(Routine2());
			});
		}
		IEnumerator Routine()
		{
			yield return null;
			SpawnCombatCoreShrines();
		}
		IEnumerator Routine2()
		{
			yield return new WaitForSeconds(1.2f);
			WorldNodeData node = NetworkedManagerBase<ZoneManager>.instance.currentNode;
			if (!disableSpecialRoomReward && node.type == WorldNodeType.Special)
			{
				DropSpecialReward();
			}
		}
	}

	[Server]
	public void DropSpecialReward()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomRewards::DropSpecialReward()' called when server was not active");
			return;
		}
		Vector3 pivot = Rift.instance.transform.position;
		Hero closestHero = Dew.GetClosestAliveHero(pivot);
		Vector3 rewardPos = pivot + (closestHero.agentPosition - pivot).Flattened().normalized * 3.5f;
		rewardPos = Dew.GetValidAgentDestination_Closest(closestHero.agentPosition, rewardPos);
		DewGameSessionSettings gss = NetworkedManagerBase<GameManager>.instance.gss;
		float memory = gss.combatRewardMemoryChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		float fantasy = gss.combatRewardFantasyChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		float sum = memory + fantasy;
		float value = UnityEngine.Random.value * sum;
		value -= memory;
		if (value < 0f)
		{
			DropSkillsForEveryone(rewardPos, isHighQuality: false);
			return;
		}
		value -= fantasy;
		DropGemsForEveryone(rewardPos, isHighQuality: false);
	}

	[Server]
	public void SpawnCombatCoreShrines()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomRewards::SpawnCombatCoreShrines()' called when server was not active");
		}
		else if (giveHighRarityReward)
		{
			DewGameSessionSettings gss = NetworkedManagerBase<GameManager>.instance.gss;
			float memoryWeight = gss.combatRewardMemoryChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
			float fantasyWeight = gss.combatRewardFantasyChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
			float sum = memoryWeight + fantasyWeight;
			if (UnityEngine.Random.value * sum < memoryWeight)
			{
				SpawnLockedShrineOnFinalSection<Shrine_Memory>();
			}
			else
			{
				SpawnLockedShrineOnFinalSection<Shrine_Concept>();
			}
		}
		else
		{
			if (isRegularRewardDisabled)
			{
				return;
			}
			bool isFirstCombatRewardFlow = false;
			if (NetworkedManagerBase<ZoneManager>.instance._nextRewards == null)
			{
				isFirstCombatRewardFlow = true;
				NetworkedManagerBase<ZoneManager>.instance._nextRewards = new List<RoomRewardFlowItemType>();
			}
			if (NetworkedManagerBase<ZoneManager>.instance._nextRewards.Count == 0)
			{
				if (isFirstCombatRewardFlow)
				{
					if (DewBuildProfile.current.customRewardFlowFirstTime != null && DewBuildProfile.current.customRewardFlowFirstTime.Length != 0)
					{
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.AddRange(DewBuildProfile.current.customRewardFlowFirstTime);
					}
					else
					{
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.Skill);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.Skill);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.None);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.None);
					}
					NetworkedManagerBase<ZoneManager>.instance._nextRewards.Shuffle();
				}
				else
				{
					if (DewBuildProfile.current.customRewardFlow != null && DewBuildProfile.current.customRewardFlow.Length != 0)
					{
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.AddRange(DewBuildProfile.current.customRewardFlow);
					}
					else
					{
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.Skill);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.Gem);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.None);
						NetworkedManagerBase<ZoneManager>.instance._nextRewards.Add(RoomRewardFlowItemType.None);
					}
					NetworkedManagerBase<ZoneManager>.instance._nextRewards.Shuffle();
				}
			}
			RoomRewardFlowItemType type = NetworkedManagerBase<ZoneManager>.instance._nextRewards[0];
			NetworkedManagerBase<ZoneManager>.instance._nextRewards.RemoveAt(0);
			switch (type)
			{
			case RoomRewardFlowItemType.Skill:
				SpawnLockedShrineOnFinalSection<Shrine_Memory>();
				break;
			case RoomRewardFlowItemType.Gem:
				SpawnLockedShrineOnFinalSection<Shrine_Concept>();
				break;
			case RoomRewardFlowItemType.Any:
			{
				DewGameSessionSettings gss2 = NetworkedManagerBase<GameManager>.instance.gss;
				float memory = gss2.combatRewardMemoryChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
				gss2.combatRewardFantasyChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
				if (UnityEngine.Random.value < memory)
				{
					SpawnLockedShrineOnFinalSection<Shrine_Memory>();
				}
				else
				{
					SpawnLockedShrineOnFinalSection<Shrine_Concept>();
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			case RoomRewardFlowItemType.None:
				break;
			}
		}
	}

	private void SpawnLockedShrineOnFinalSection<T>() where T : Shrine
	{
		SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection().TryGetGoodNodePosition(out var pos);
		T shrine = Dew.CreateActor<T>(pos, null);
		shrine.isLocked = true;
		SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(delegate
		{
			if (shrine != null)
			{
				shrine.isLocked = false;
			}
		});
	}

	public void DropChaosReward(Vector3 pos, bool isHighQuality)
	{
		Dew.CreateActor(Dew.GetGoodRewardPosition(pos), null, null, delegate(Shrine_Chaos chaos)
		{
			chaos.SetRandomRarity(isHighQuality);
		});
		foreach (DewPlayer p in DewPlayer.humanPlayers)
		{
			if (!p.hero.IsNullInactiveDeadOrKnockedOut() && !(UnityEngine.Random.value > p.doubleChaosChance))
			{
				Dew.CreateActor(Dew.GetGoodRewardPosition(pos), null, null, delegate(Shrine_Chaos chaos)
				{
					chaos.SetRandomRarity(isHighQuality);
					chaos.playersOverride = new DewPlayer[1] { p };
				});
			}
		}
	}

	public void DropGreatReward(Vector3 pos)
	{
		DropCurrencyReward(pos, isHighQuality: true);
		DropGemsForEveryone(pos, isHighQuality: true);
		DropSkillsForEveryone(pos, isHighQuality: true);
	}

	public void DropCurrencyReward(Vector3 pos, bool isHighQuality)
	{
		DewGameSessionSettings gss = NetworkedManagerBase<GameManager>.instance.gss;
		float rand = UnityEngine.Random.value;
		float multiplier = (isHighQuality ? gss.highRewardCurrencyMultiplier : 1f);
		int zoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		rand -= gss.goldOnlyChance;
		if (rand < 0f)
		{
			int amount = DewMath.RandomRoundToInt(UnityEngine.Random.Range(gss.goldOnlyRewardMin.Evaluate(zoneIndex), gss.goldOnlyRewardMax.Evaluate(zoneIndex)) * multiplier);
			{
				foreach (DewPlayer h in DewPlayer.humanPlayers)
				{
					NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount, pos, h.hero);
				}
				return;
			}
		}
		rand -= gss.dreamDustOnlyChance;
		if (rand < 0f)
		{
			int amount2 = DewMath.RandomRoundToInt(UnityEngine.Random.Range(gss.dreamDustOnlyRewardMin.Evaluate(zoneIndex), gss.dreamDustOnlyRewardMax.Evaluate(zoneIndex)) * multiplier);
			{
				foreach (DewPlayer h2 in DewPlayer.humanPlayers)
				{
					NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount2, pos, h2.hero);
				}
				return;
			}
		}
		rand -= gss.bothChance;
		int gold = DewMath.RandomRoundToInt(UnityEngine.Random.Range(gss.bothGoldRewardMin.Evaluate(zoneIndex), gss.bothGoldRewardMax.Evaluate(zoneIndex)) * multiplier);
		foreach (DewPlayer h3 in DewPlayer.humanPlayers)
		{
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, gold, pos, h3.hero);
		}
		int dreamDust = DewMath.RandomRoundToInt(UnityEngine.Random.Range(gss.bothDreamDustRewardMin.Evaluate(zoneIndex), gss.bothDreamDustRewardMax.Evaluate(zoneIndex)) * multiplier);
		foreach (DewPlayer h4 in DewPlayer.humanPlayers)
		{
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, dreamDust, pos, h4.hero);
		}
	}

	public void DropGemsForEveryone(Vector3 pos, bool isHighQuality, int bonusQuality = 0)
	{
		Loot_Gem loot = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>();
		Rarity rarity = (isHighQuality ? loot.SelectRarityHigh() : loot.SelectRarityNormal());
		foreach (DewPlayer h in DewPlayer.humanPlayers)
		{
			Hero target = (h.hero.isKnockedOut ? Dew.SelectRandomAliveHero() : h.hero);
			Vector3 p = (pos + target.position) * 0.5f;
			p = Dew.GetGoodRewardPosition(p);
			loot.SelectGemAndQuality(rarity, out var gem, out var quality);
			Dew.CreateGem(gem, p, quality + gemBonusQuality + bonusQuality, target.owner);
		}
	}

	public void DropSkillsForEveryone(Vector3 pos, bool isHighQuality, int bonusLevel = 0)
	{
		Loot_Skill loot = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Skill>();
		Rarity rarity = (isHighQuality ? loot.SelectRarityHigh() : loot.SelectRarityNormal());
		foreach (DewPlayer h in DewPlayer.humanPlayers)
		{
			Hero target = (h.hero.isKnockedOut ? Dew.SelectRandomAliveHero() : h.hero);
			Vector3 p = (pos + target.position) * 0.5f;
			p = Dew.GetGoodRewardPosition(p);
			loot.SelectSkillAndLevel(rarity, out var skill, out var level);
			Dew.CreateSkillTrigger(skill, p, level + skillBonusLevel + bonusLevel, target.owner);
		}
	}

	private void MirrorProcessed()
	{
	}
}
