using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RoomComponentStartDependency(typeof(RoomModifiers))]
public class RoomMonsters : RoomComponent
{
	private struct ActorSaveData
	{
		public Type type;

		public ulong sceneId;

		public Dictionary<string, object> data;
	}

	private class RoomMonstersSaveData
	{
		public List<ActorSaveData> actors = new List<ActorSaveData>();
	}

	public class MonsterSpawnData
	{
		public Entity lastKiller;

		public Vector3 lastDeathPosition;

		public float remainingPopulation;
	}

	private class Camp
	{
		public Vector3 position;

		public bool wasInRange;

		public bool isSpawned;

		public List<SpawnEntry> entries = new List<SpawnEntry>();

		public float nextRegenerateChance = 1f;

		public float nextRegenerateTime = float.PositiveInfinity;

		public Camp(Vector3 position)
		{
			this.position = position;
		}

		public void DoCampTick()
		{
			bool flag = Dew.GetClosestHeroDistance(position) < 40f;
			if (!flag && isSpawned && AreAllEntitiesSleeping())
			{
				Despawn();
			}
			if (!wasInRange && flag)
			{
				if (entries.Count == 0)
				{
					RegenerateEntries();
				}
				else if (Time.time > nextRegenerateTime)
				{
					if (UnityEngine.Random.value < nextRegenerateChance)
					{
						RegenerateEntries();
					}
					else
					{
						nextRegenerateTime = Time.time + float.PositiveInfinity;
					}
				}
				if (!isSpawned && HasAnyAliveEntries())
				{
					Spawn();
				}
			}
			wasInRange = flag;
		}

		private bool AreAllEntitiesSleeping()
		{
			foreach (SpawnEntry entry in entries)
			{
				if (!entry.instance.IsNullOrInactive() && !entry.instance.isSleeping)
				{
					return false;
				}
			}
			return true;
		}

		private bool HasAnyAliveEntries()
		{
			foreach (SpawnEntry entry in entries)
			{
				if (!entry.isKilled)
				{
					return true;
				}
			}
			return false;
		}

		public void RegenerateEntries()
		{
			if (isSpawned)
			{
				throw new InvalidOperationException();
			}
			RoomMonsters monsters = SingletonDewNetworkBehaviour<Room>.instance.monsters;
			float original = UnityEngine.Random.Range(monsters.campPopulation.x, monsters.campPopulation.y);
			original = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(original) * SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier;
			IEnumerator<Monster> monsters2 = ((monsters.envSpawnPoolOverride != null) ? monsters.envSpawnPoolOverride : monsters.defaultRule.pool).GetMonsters(int.MaxValue);
			monsters2.MoveNext();
			float num = 0f;
			while (true)
			{
				float populationCost = monsters2.Current.populationCost;
				entries.Add(new SpawnEntry
				{
					prefab = monsters2.Current
				});
				num += populationCost;
				if (num > original)
				{
					break;
				}
				monsters2.MoveNext();
			}
			nextRegenerateChance *= 0.65f;
			nextRegenerateTime = float.PositiveInfinity;
		}

		public void Spawn()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				SpawnEntry value = entries[i];
				if (value.isKilled)
				{
					continue;
				}
				int index = i;
				Vector3 pos = Dew.GetPositionOnGround(position + UnityEngine.Random.insideUnitSphere * 6f);
				pos = Dew.GetValidAgentDestination_LinearSweep(position, pos);
				value.instance = Dew.SpawnEntity(value.prefab, pos, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), NetworkedManagerBase<ActorManager>.instance.serverActor, DewPlayer.creep, NetworkedManagerBase<GameManager>.instance.ambientLevel, delegate(Entity entity)
				{
					entity._accumulatedSleepTime = float.PositiveInfinity;
					entity.Visual.NetworkskipSpawning = true;
					if (entity is Monster monster)
					{
						monster.campPosition = pos;
					}
					entity.EntityEvent_OnDeath += (Action<EventInfoKill>)delegate
					{
						SpawnEntry value2 = entries[index];
						value2.isKilled = true;
						entries[index] = value2;
						nextRegenerateTime = Time.time + float.PositiveInfinity;
						if (isSpawned)
						{
							foreach (SpawnEntry entry in entries)
							{
								if (!entry.isKilled)
								{
									return;
								}
							}
							isSpawned = false;
						}
					};
					SingletonDewNetworkBehaviour<Room>.instance.monsters.onBeforeSpawn?.Invoke(entity);
				});
				SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn?.Invoke(value.instance);
				entries[i] = value;
			}
			isSpawned = true;
		}

		public void Despawn()
		{
			for (int i = 0; i < entries.Count; i++)
			{
				SpawnEntry value = entries[i];
				if (!value.instance.IsNullOrInactive())
				{
					value.instance.Destroy();
					value.instance = null;
					entries[i] = value;
				}
			}
			isSpawned = false;
		}
	}

	private struct SpawnEntry
	{
		public Entity prefab;

		public Entity instance;

		public bool isKilled;
	}

	public const float CombatAreaScoreRandomness = 0.3f;

	public Action<Entity> onBeforeSpawn;

	public Action<Entity> onAfterSpawn;

	public int insertedCombatAreas;

	public float spawnedPopMultiplier = 1f;

	public float maxPopulationMultiplier = 1f;

	public float addedMirageChance;

	public float addedHunterChance;

	public bool clearRoomOnClearAllCombatAreas = true;

	public Dictionary<SpawnMonsterSettings, Coroutine> ongoingSpawns = new Dictionary<SpawnMonsterSettings, Coroutine>();

	public SafeAction onWelcomingSpawnStart;

	public SafeAction onWelcomingSpawnEnd;

	private static List<MiniBossEffect> _miniBossEffects;

	public const float CampWanderRange = 6f;

	private const float RespawnChanceMultiplier = 0.65f;

	private const float CampRespawnTimeAfterBeingSeen = float.PositiveInfinity;

	private const float CampScoreFuzziness = 0.15f;

	private const float BanCampDistanceFromHeroSpawn = 25f;

	private const float BanCampDistanceFromBanPosition = 3f;

	private const float CampActivateDistanceFromHero = 40f;

	private const float EnvSpawnTickInterval = 1f;

	[Space(16f)]
	public bool disableEnvSpawn;

	public MonsterPool envSpawnPoolOverride;

	public float campDensity;

	public Vector2 campPopulation = new Vector2(0.5f, 3f);

	private List<Camp> _camps;

	private float _nextEnvSpawnTickTime;

	public const float OverpopulationStallDelayMin = 0.5f;

	public const float OverpopulationStallDelayMax = 1.5f;

	public const float SpawnDelayMin = 0.1f;

	public const float SpawnDelayMax = 0.5f;

	public MonsterSpawnRule defaultRule;

	private static NavMeshPath _tempPath;

	private static int _bossesSpawnedCount;

	public bool didSetupCombatAreas { get; private set; }

	public bool isDoingHunterWelcomingSpawn { get; private set; }

	public override void OnRoomStart(bool isRevisit)
	{
		base.OnRoomStart(isRevisit);
		DewResources.AddPreloadRule(this, delegate(PreloadInterface preload)
		{
			if (defaultRule != null && defaultRule.pool != null)
			{
				preload.AddFromMonsterPool(defaultRule.pool);
			}
			foreach (RoomSection section in base.room.sections)
			{
				if (!(section == null) && !(section.monsters.ruleOverride == null) && !(section.monsters.ruleOverride.pool == null))
				{
					preload.AddFromMonsterPool(section.monsters.ruleOverride.pool);
				}
			}
		});
	}

	public override void OnRoomStartServer(WorldNodeSaveData save)
	{
		base.OnRoomStartServer(save);
		if (defaultRule == null)
		{
			defaultRule = NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultMonsters;
		}
		if (save == null)
		{
			SetupCombatAreas();
			DoEnvSpawnRoomStart();
			if (NetworkedManagerBase<ZoneManager>.instance.currentNode.HasMainModifier())
			{
				RoomSection sectionFromWorldPos = base.room.GetSectionFromWorldPos(base.room.heroSpawnPos);
				if (sectionFromWorldPos != null)
				{
					sectionFromWorldPos.monsters.addedInitDelay += Vector2.one * 1.5f;
				}
			}
			if (!clearRoomOnClearAllCombatAreas)
			{
				return;
			}
			RefValue<int> remaining = new RefValue<int>(0);
			foreach (RoomSection s in base.room.sections)
			{
				if (!s.monsters.isMarkedAsCombatArea)
				{
					continue;
				}
				remaining.value++;
				s.monsters.onClearCombatArea.AddListener(delegate
				{
					remaining.value--;
					if (remaining.value <= 0)
					{
						s.monsters.StartCoroutine(Routine());
					}
				});
			}
		}
		else
		{
			foreach (ActorSaveData actor2 in save.Get<RoomMonstersSaveData>().actors)
			{
				Actor actor;
				if (actor2.sceneId == 0L)
				{
					actor = DewResources.GetByType<Actor>(actor2.type).OnLoadCreateActor(actor2.data, null);
				}
				else
				{
					actor = null;
					foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
					{
						if (allActor.netIdentity.sceneId == actor2.sceneId)
						{
							actor = allActor;
							break;
						}
					}
					if (actor == null)
					{
						Debug.LogWarning($"Could not find scene actor to load in: {actor2.type.Name} ({actor2.sceneId})");
						continue;
					}
				}
				actor.OnLoadActor(actor2.data);
			}
			Room_Barrier[] array = UnityEngine.Object.FindObjectsOfType<Room_Barrier>();
			for (int num = 0; num < array.Length; num++)
			{
				array[num].Open();
			}
		}
		Rift[] rifts;
		if (NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted)
		{
			isDoingHunterWelcomingSpawn = true;
			onWelcomingSpawnStart?.Invoke();
			rifts = UnityEngine.Object.FindObjectsOfType<Rift>();
			Rift[] array2 = rifts;
			for (int num2 = 0; num2 < array2.Length; num2++)
			{
				array2[num2].isLocked = true;
			}
			StartCoroutine(Routine2());
		}
		static IEnumerator Routine()
		{
			yield return Dew.WaitForAggroedEnemiesRoutine();
			if (!SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
			{
				SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
			}
		}
		IEnumerator Routine2()
		{
			yield return new WaitWhile(() => NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition);
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1f));
			if (NetworkedManagerBase<ZoneManager>.instance.currentNode.HasMainModifier() && save == null)
			{
				yield return new WaitForSeconds(1.25f);
			}
			SpawnMonsters(new SpawnMonsterSettings
			{
				rule = defaultRule,
				initDelayMultiplier = 0.1f,
				spawnPopulationMultiplier = NetworkedManagerBase<GameManager>.instance.gss.welcomingSpawnPopMultiplierByArea.Evaluate(base.room.map.mapData.area),
				onFinish = delegate
				{
					Rift[] array3 = rifts;
					for (int i = 0; i < array3.Length; i++)
					{
						array3[i].isLocked = false;
					}
					isDoingHunterWelcomingSpawn = false;
					onWelcomingSpawnEnd?.Invoke();
				}
			});
		}
	}

	public override void OnRoomStopServer(WorldNodeSaveData save)
	{
		base.OnRoomStopServer(save);
		DoEnvSpawnRoomStop();
		foreach (KeyValuePair<SpawnMonsterSettings, Coroutine> ongoingSpawn in ongoingSpawns)
		{
			StopCoroutine(ongoingSpawn.Value);
			ongoingSpawn.Key.onFinish?.Invoke();
		}
		ongoingSpawns.Clear();
		StopAllCoroutines();
		RoomMonstersSaveData roomMonstersSaveData = new RoomMonstersSaveData();
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor.isActive && allActor.isDestroyedOnRoomChange && allActor.ShouldBeSaved() && !allActor.IsExcludedFromRoomSave())
			{
				ActorSaveData item = new ActorSaveData
				{
					type = allActor.GetType(),
					sceneId = allActor.netIdentity.sceneId,
					data = new Dictionary<string, object>()
				};
				try
				{
					allActor.OnSaveActor(item.data);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				roomMonstersSaveData.actors.Add(item);
			}
		}
		save.Set(roomMonstersSaveData);
	}

	public override void OnRoomStop()
	{
		base.OnRoomStop();
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			allActor.netIdentity.sceneId = 0uL;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		DoEnvSpawnLogicUpdate(dt);
	}

	private void SetupCombatAreas()
	{
		didSetupCombatAreas = true;
		int requiredRiftAreas = 0;
		int numOfCurrentRiftAreas = 0;
		List<RoomSection> combatAreas = new List<RoomSection>();
		int inserted = 0;
		foreach (RoomSection section in base.room.sections)
		{
			if (section.monsters.combatAreaSettings == SectionCombatAreaType.Yes)
			{
				inserted--;
				InsertCombatArea(section);
			}
		}
		if (insertedCombatAreas <= 0)
		{
			return;
		}
		if (combatAreas.Count == 0)
		{
			List<RoomSection> list = new List<RoomSection>();
			foreach (RoomSection section2 in base.room.sections)
			{
				if (section2.monsters.combatAreaSettings == SectionCombatAreaType.Random)
				{
					list.Add(section2);
				}
			}
			if (list.Count == 0)
			{
				LogNotEnoughCandidates();
				return;
			}
			RoomSection roomSection = list[UnityEngine.Random.Range(0, list.Count)];
			InsertCombatArea(roomSection);
		}
		Dictionary<RoomSection, float> dictionary = new Dictionary<RoomSection, float>();
		while (true)
		{
			dictionary.Clear();
			foreach (RoomSection section3 in base.room.sections)
			{
				if (combatAreas.Contains(section3) || section3.monsters.combatAreaSettings != SectionCombatAreaType.Random)
				{
					continue;
				}
				float num = float.PositiveInfinity;
				foreach (RoomSection item in combatAreas)
				{
					float navDistanceTo = section3.GetNavDistanceTo(item);
					if (navDistanceTo < num)
					{
						num = navDistanceTo;
					}
				}
				float num2 = num * (1f + UnityEngine.Random.Range(-0.3f, 0.3f));
				if (numOfCurrentRiftAreas < requiredRiftAreas && section3.GetComponentInChildren<Room_RiftPos>() != null)
				{
					num2 += 100f;
				}
				dictionary.Add(section3, num2);
			}
			if (dictionary.Count == 0)
			{
				LogNotEnoughCandidates();
				break;
			}
			float num3 = float.NegativeInfinity;
			RoomSection roomSection2 = null;
			foreach (KeyValuePair<RoomSection, float> item2 in dictionary)
			{
				if (!(item2.Value <= num3))
				{
					num3 = item2.Value;
					roomSection2 = item2.Key;
				}
			}
			InsertCombatArea(roomSection2);
			inserted++;
			if (inserted >= insertedCombatAreas)
			{
				if (numOfCurrentRiftAreas >= requiredRiftAreas)
				{
					break;
				}
				Debug.Log($"Inserted {inserted} combat areas but did not meet minimum rift area requirement ({numOfCurrentRiftAreas}/{requiredRiftAreas}), continuing...");
			}
		}
		void InsertCombatArea(RoomSection roomSection3)
		{
			combatAreas.Add(roomSection3);
			roomSection3.monsters.isMarkedAsCombatArea = true;
			if (roomSection3.GetComponentInChildren<Room_RiftPos>() != null)
			{
				numOfCurrentRiftAreas++;
			}
		}
		void LogNotEnoughCandidates()
		{
			Debug.LogWarning("Room " + SceneManager.GetActiveScene().name + " does not have enough combat area candidates");
			Debug.LogWarning($"Inserted: {inserted}/{insertedCombatAreas}, Rift: {numOfCurrentRiftAreas}/{requiredRiftAreas}");
		}
	}

	[Server]
	public void OverrideMonsterType(Monster m)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomMonsters::OverrideMonsterType(Monster)' called when server was not active");
			return;
		}
		if (defaultRule == null)
		{
			defaultRule = NetworkedManagerBase<ZoneManager>.instance.currentZone.defaultMonsters;
		}
		defaultRule = UnityEngine.Object.Instantiate(defaultRule);
		defaultRule.pool = UnityEngine.Object.Instantiate(defaultRule.pool);
		defaultRule.pool.entries.Clear();
		defaultRule.pool.entries.Add(new MonsterPool.SpawnRuleEntry
		{
			chance = 1f,
			monsterRef = m.ToAssetRef(),
			minCount = 1,
			maxCount = 1
		});
		foreach (RoomSection section in base.room.sections)
		{
			section.monsters.ruleOverride = defaultRule;
		}
	}

	[Server]
	public void FinishAllOngoingSpawns()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomMonsters::FinishAllOngoingSpawns()' called when server was not active");
			return;
		}
		foreach (KeyValuePair<SpawnMonsterSettings, Coroutine> ongoingSpawn in ongoingSpawns)
		{
			ongoingSpawn.Key.onFinish?.Invoke();
			StopCoroutine(ongoingSpawn.Value);
		}
		SingletonDewNetworkBehaviour<Room>.instance.monsters.ongoingSpawns.Clear();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_miniBossEffects = null;
	}

	public void SpawnMiniBoss(SpawnMonsterSettings settings, Entity ent = null, StatusEffect se = null)
	{
		SelectMiniBoss(out var entity, out var effect);
		if (ent == null)
		{
			ent = entity;
		}
		if (se == null)
		{
			se = effect;
		}
		if (!(ent is ISpawnableAsMiniBoss))
		{
			Debug.LogWarning("Invalid entity for mini boss spawn provided: " + ent.name);
			return;
		}
		settings.rule = ScriptableObject.CreateInstance<MonsterSpawnRule>();
		settings.rule.isBossSpawn = true;
		settings.rule.pool = ScriptableObject.CreateInstance<MonsterPool>();
		settings.rule.pool.entries = new List<MonsterPool.SpawnRuleEntry>();
		settings.rule.pool.entries.Add(new MonsterPool.SpawnRuleEntry
		{
			monsterRef = ent.ToAssetRef(),
			chance = 1f,
			minCount = 1,
			maxCount = 1
		});
		settings.rule.initialDelay = Vector2.one * 0.5f;
		settings.rule.wavesMax = 1;
		settings.rule.wavesMin = 1;
		settings.rule.spawnMinDistance = 6f;
		settings.rule.spawnMaxDistance = 9f;
		settings.rule.onOverPopulation = OverpopulationBehavior.Ignore;
		settings.rule.populationPerWave = ((Monster)ent).populationCost * 0.55f * Vector2.one;
		settings.rule.waveTimeoutMin = float.PositiveInfinity;
		settings.rule.waveTimeoutMax = float.PositiveInfinity;
		if (settings.spawnPosGetter == null && settings.section != null)
		{
			settings.spawnPosGetter = () => settings.section.pathablePivot;
		}
		SpawnMonsterSettings spawnMonsterSettings = settings;
		spawnMonsterSettings.beforeSpawn = (Action<Entity>)Delegate.Combine(spawnMonsterSettings.beforeSpawn, (Action<Entity>)delegate(Entity e)
		{
			((Monster)e).Networktype = Monster.MonsterType.MiniBoss;
			((ISpawnableAsMiniBoss)e).OnBeforeSpawnAsMiniBoss();
		});
		SpawnMonsterSettings spawnMonsterSettings2 = settings;
		spawnMonsterSettings2.afterSpawn = (Action<Entity>)Delegate.Combine(spawnMonsterSettings2.afterSpawn, (Action<Entity>)delegate(Entity e)
		{
			CallOnCreateAsMiniBoss(e);
			e.CreateStatusEffect(se, e, new CastInfo(e));
			e.EntityEvent_OnDeath += (Action<EventInfoKill>)delegate
			{
				StartCoroutine(DropRewardsRoutine(e));
			};
		});
		SpawnMonsters(settings);
		static IEnumerator DropRewardsRoutine(Entity e)
		{
			yield return new WaitForSeconds(1.5f);
			Vector3 pos = e.agentPosition;
			int zoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
			int dropType = UnityEngine.Random.Range(0, 4);
			foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
			{
				if (!humanPlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(pos);
					switch (dropType)
					{
					case 0:
					{
						Loot_Skill lootInstance2 = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Skill>();
						Rarity rarity2 = (SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward ? lootInstance2.SelectRarityHigh() : lootInstance2.SelectRarityNormal());
						lootInstance2.SelectSkillAndLevel(rarity2, out var skill, out var level);
						level += SingletonDewNetworkBehaviour<Room>.instance.rewards.skillBonusLevel;
						if (skill != null)
						{
							Dew.CreateSkillTrigger(skill, goodRewardPosition, level, humanPlayer);
						}
						break;
					}
					case 1:
					{
						Loot_Gem lootInstance = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>();
						Rarity rarity = (SingletonDewNetworkBehaviour<Room>.instance.rewards.giveHighRarityReward ? lootInstance.SelectRarityHigh() : lootInstance.SelectRarityNormal());
						lootInstance.SelectGemAndQuality(rarity, out var gem, out var quality);
						quality += SingletonDewNetworkBehaviour<Room>.instance.rewards.gemBonusQuality;
						if (gem != null)
						{
							Dew.CreateGem(gem, goodRewardPosition, quality, humanPlayer);
						}
						break;
					}
					case 2:
					{
						int[] values2 = new int[7] { 100, 200, 300, 500, 700, 900, 1200 };
						float[] weights2 = new float[7] { 50f, 25f, 12f, 6f, 4f, 2f, 1f };
						int amount2 = Mathf.RoundToInt((float)WeightedRandom(values2, weights2) * (1f + 0.1f * (float)zoneIndex));
						NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, amount2, goodRewardPosition);
						break;
					}
					default:
					{
						int[] values = new int[7] { 50, 100, 150, 200, 300, 400, 500 };
						float[] weights = new float[7] { 50f, 25f, 12f, 6f, 4f, 2f, 1f };
						int amount = Mathf.RoundToInt((float)WeightedRandom(values, weights) * (1f + 0.1f * (float)zoneIndex));
						NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount, goodRewardPosition);
						break;
					}
					}
					yield return null;
				}
			}
		}
		static int WeightedRandom(int[] values, float[] weights)
		{
			float num = 0f;
			for (int i = 0; i < weights.Length; i++)
			{
				num += weights[i];
			}
			float num2 = UnityEngine.Random.value * num;
			float num3 = 0f;
			for (int j = 0; j < values.Length; j++)
			{
				num3 += weights[j];
				if (num2 <= num3)
				{
					return values[j];
				}
			}
			return values[^1];
		}
	}

	[ClientRpc]
	private void CallOnCreateAsMiniBoss(Entity e)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(e);
		SendRPCInternal("System.Void RoomMonsters::CallOnCreateAsMiniBoss(Entity)", 1965624741, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void SelectMiniBoss(out Entity entity, out MiniBossEffect effect)
	{
		if (_miniBossEffects == null)
		{
			_miniBossEffects = new List<MiniBossEffect>();
			foreach (MiniBossEffect item in DewResources.FindAllByType<MiniBossEffect>())
			{
				_miniBossEffects.Add(item);
			}
		}
		List<Entity> list = new List<Entity>();
		foreach (MonsterPool.SpawnRuleEntry filteredEntry in defaultRule.pool.GetFilteredEntries())
		{
			if (filteredEntry.monster is ISpawnableAsMiniBoss && !list.Contains(filteredEntry.monster))
			{
				list.Add(filteredEntry.monster);
			}
		}
		entity = list[UnityEngine.Random.Range(0, list.Count)];
		effect = _miniBossEffects[UnityEngine.Random.Range(0, _miniBossEffects.Count)];
	}

	private void DoEnvSpawnRoomStart()
	{
		if (disableEnvSpawn)
		{
			return;
		}
		_camps = new List<Camp>();
		int num = Mathf.RoundToInt(campDensity * base.room.map.mapData.area);
		IBanCampsNearby[] array = Dew.FindInterfacesOfType<IBanCampsNearby>(includeInactive: false);
		Vector2[] array2 = new Vector2[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			IBanCampsNearby banCampsNearby = array[i];
			array2[i] = ((Component)banCampsNearby).transform.position.ToXY();
		}
		for (int j = 0; j < num; j++)
		{
			AddCampPos(array2);
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			allHero.Control.ClientEvent_OnTeleport += new Action<Vector3, Vector3>(ClientEventOnTeleport);
		}
	}

	private void DoEnvSpawnRoomStop()
	{
		if (NetworkedManagerBase<ActorManager>.instance == null)
		{
			return;
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			allHero.Control.ClientEvent_OnTeleport -= new Action<Vector3, Vector3>(ClientEventOnTeleport);
		}
	}

	private void DoEnvSpawnLogicUpdate(float dt)
	{
		if (base.isServer && _camps != null && !(Time.time < _nextEnvSpawnTickTime))
		{
			_nextEnvSpawnTickTime = Time.time + 1f;
			DoCampTickImmediately();
		}
	}

	private void DoCampTickImmediately()
	{
		if (_camps != null)
		{
			for (int i = 0; i < _camps.Count; i++)
			{
				_camps[i].DoCampTick();
			}
		}
	}

	private void ClientEventOnTeleport(Vector3 arg1, Vector3 arg2)
	{
		if (Vector2.Distance(arg1.ToXY(), arg2.ToXY()) > 5f)
		{
			DoCampTickImmediately();
		}
	}

	private void AddCampPos(Vector2[] bannedPositions)
	{
		bool flag = NetworkedManagerBase<ZoneManager>.instance.currentNode.HasMainModifier();
		List<Vector2> list = new List<Vector2>();
		IReadOnlyList<(int, int)> innerPropNodeIndices = base.room.map.mapData.innerPropNodeIndices;
		for (int i = 0; i < 30; i++)
		{
			Vector2 worldPos = base.room.map.mapData.cells.GetWorldPos(innerPropNodeIndices[UnityEngine.Random.Range(0, innerPropNodeIndices.Count)]);
			list.Add(worldPos);
		}
		Vector3 heroSpawnPos = base.room.heroSpawnPos;
		Vector3 position = Dew.SelectBestWithScore(list, delegate(Vector2 v, int _)
		{
			if (Vector2.Distance(heroSpawnPos.ToXY(), v) < 25f)
			{
				return float.NegativeInfinity;
			}
			Vector2[] array = bannedPositions;
			for (int j = 0; j < array.Length; j++)
			{
				if (Vector2.Distance(array[j], v) < 3f)
				{
					return float.NegativeInfinity;
				}
			}
			float num = float.PositiveInfinity;
			foreach (Camp camp in _camps)
			{
				num = Mathf.Min(num, Vector2.Distance(camp.position.ToXY(), v));
			}
			return num;
		}, 0.15f).ToXZ();
		position = Dew.GetPositionOnGround(position);
		position = Dew.GetValidAgentPosition(position);
		if (!flag || !(Vector2.Distance(position.ToXY(), heroSpawnPos.ToXY()) < 25f))
		{
			_camps.Add(new Camp(position));
		}
	}

	[Server]
	public void RemoveAllCamps()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomMonsters::RemoveAllCamps()' called when server was not active");
		}
		else
		{
			if (_camps == null)
			{
				return;
			}
			foreach (Camp camp in _camps)
			{
				foreach (SpawnEntry entry in camp.entries)
				{
					if (!entry.instance.IsNullInactiveDeadOrKnockedOut())
					{
						entry.instance.Destroy();
					}
				}
			}
			_camps.Clear();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_camps == null)
		{
			return;
		}
		foreach (Camp camp in _camps)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(camp.position, 1.5f);
		}
	}

	private IEnumerator WaitForPopulationRoutine(MonsterSpawnRule rule, RoomSection section, float requiredPopulation, RefValue<bool> didFail)
	{
		if (requiredPopulation <= 0f || !IsOverPop())
		{
			didFail.value = false;
			yield break;
		}
		switch (rule.onOverPopulation)
		{
		case OverpopulationBehavior.Stall:
		{
			float stallStart = Time.time;
			do
			{
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
				if (Time.time - stallStart > rule.stallCancelTimeout)
				{
					didFail.value = true;
					yield break;
				}
			}
			while (IsOverPop());
			break;
		}
		case OverpopulationBehavior.Cancel:
			didFail.value = true;
			yield break;
		default:
			throw new ArgumentOutOfRangeException();
		case OverpopulationBehavior.Ignore:
			break;
		}
		didFail.value = false;
		bool IsOverPop()
		{
			if (!NetworkedManagerBase<GameManager>.instance.isSpawnOverPopulation)
			{
				if (section != null)
				{
					return section.monsters.isOverPopulation;
				}
				return false;
			}
			return true;
		}
	}

	public void SpawnMonsters(SpawnMonsterSettings settings)
	{
		ongoingSpawns.Add(settings, StartCoroutine(SpawnMonstersRoutine(settings)));
	}

	public UniTask SpawnMonstersAsync(SpawnMonsterSettings settings)
	{
		UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
		settings.onFinish = (Action)Delegate.Combine(settings.onFinish, (Action)delegate
		{
			completionSource.TrySetResult();
		});
		SpawnMonsters(settings);
		return completionSource.Task;
	}

	private IEnumerator SpawnMonstersRoutine(SpawnMonsterSettings s)
	{
		if (ongoingSpawns.ContainsKey(s))
		{
			throw new InvalidOperationException();
		}
		yield return null;
		while (NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
		{
			yield return null;
		}
		MonsterSpawnData monsterSpawnData = s.monsterSpawnData;
		MonsterSpawnRule rule = s.rule;
		float waitStartTime = Time.time;
		float timeToWait = UnityEngine.Random.Range(rule.initialDelay.x, rule.initialDelay.y) * s.initDelayMultiplier + s.initDelayFlat;
		yield return new WaitWhile(() => Time.time - waitStartTime < timeToWait && !s.isCutsceneSkipped);
		int waves = UnityEngine.Random.Range(rule.wavesMin, rule.wavesMax + 1);
		waves = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterWaves(waves);
		float hunterChance = addedHunterChance;
		float[,] mirageSkinChanceByZoneIndexAndPlayerCount = NetworkedManagerBase<GameManager>.instance.gss.mirageSkinChanceByZoneIndexAndPlayerCount;
		float mirageChance = mirageSkinChanceByZoneIndexAndPlayerCount[Mathf.Clamp(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex, 0, mirageSkinChanceByZoneIndexAndPlayerCount.GetLength(0) - 1), Mathf.Clamp(Dew.GetAliveHeroCount() - 1, 0, mirageSkinChanceByZoneIndexAndPlayerCount.GetLength(1) - 1)];
		if (mirageChance > 0.0001f)
		{
			mirageChance += addedMirageChance * AttrCustomizeResources.Config.monsterMirageChanceMultiple;
		}
		if (rule.isBossSpawn)
		{
			int num = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * AttrCustomizeResources.Config.bossCountAddByZone;
			int num2 = NetworkedManagerBase<ZoneManager>.instance.loopIndex * AttrCustomizeResources.Config.bossCountAddByLoop;
			int num3 = AttrCustomizeResources.Config.bossCount + num2 + num;
			int limitBossCount = AttrCustomizeResources.Config.limitBossCount;
			if (limitBossCount > 0)
			{
				num3 = Mathf.Max(0, Mathf.Min(num3, limitBossCount - _bossesSpawnedCount));
			}
			waves = num3;
			hunterChance = AttrCustomizeResources.Config.bossHunterChance;
			mirageChance = AttrCustomizeResources.Config.bossMirageChance;
		}
		int waveIndex = 0;
		while (true)
		{
			float waveStartTime;
			float waveTimeout;
			float nextWaveThreshold;
			if (waveIndex < waves)
			{
				waveStartTime = Time.time;
				waveTimeout = UnityEngine.Random.Range(rule.waveTimeoutMin, rule.waveTimeoutMax);
				if (rule.isBossSpawn && AttrCustomizeResources.Config.enableBossSpawnAllOnce)
				{
					waveTimeout = 0f;
				}
				float population = UnityEngine.Random.Range(rule.populationPerWave.x, rule.populationPerWave.y);
				population = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(population, s.ignoreTurnPopMultiplier, s.ignoreCoopPopMultiplier) * s.spawnPopulationMultiplier * spawnedPopMultiplier;
				nextWaveThreshold = UnityEngine.Random.Range(rule.nextWavePopulationThreshold.x, rule.nextWavePopulationThreshold.y);
				nextWaveThreshold = NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(nextWaveThreshold, s.ignoreTurnPopMultiplier, s.ignoreCoopPopMultiplier);
				nextWaveThreshold = Mathf.Clamp(nextWaveThreshold, 0.0001f, population - 0.1f);
				IEnumerator<Monster> enumerator = rule.pool.GetMonsters(int.MaxValue);
				enumerator.MoveNext();
				float spawnedPop = 0f;
				bool isFirstSpawn = true;
				while (true)
				{
					if (isFirstSpawn)
					{
						isFirstSpawn = false;
					}
					else
					{
						yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.5f));
					}
					float popCost = enumerator.Current.populationCost;
					if (!rule.isBossSpawn)
					{
						RefValue<bool> didFail = new RefValue<bool>(v: false);
						yield return WaitForPopulationRoutine(rule, s.section, popCost, didFail);
						if ((bool)didFail)
						{
							break;
						}
					}
					if (s.earlyFinishCondition != null && s.earlyFinishCondition())
					{
						goto end_IL_02cd;
					}
					Entity entity = SpawnMonsterImp(s, monsterSpawnData, enumerator.Current, popCost);
					if (entity != null)
					{
						spawnedPop += popCost;
						if (UnityEngine.Random.value < hunterChance && !entity.Status.HasStatusEffect<Se_HunterBuff>())
						{
							entity.CreateStatusEffect<Se_HunterBuff>(entity, new CastInfo(entity));
						}
						if (UnityEngine.Random.value < mirageChance && entity is Monster { type: not Monster.MonsterType.Lesser } && !entity.Status.HasStatusEffect<Se_MirageSkin_Delusion>())
						{
							entity.CreateStatusEffect<Se_MirageSkin_Delusion>(entity, new CastInfo(entity));
						}
					}
					if (!rule.isBossSpawn && !(spawnedPop > population))
					{
						enumerator.MoveNext();
						continue;
					}
					goto IL_06db;
				}
				if (rule.onOverPopulation == OverpopulationBehavior.Stall)
				{
					Debug.Log(rule.name + " timed out due to overpopulation");
				}
				else
				{
					Debug.Log(rule.name + " canceled due to overpopulation");
				}
			}
			while (monsterSpawnData.remainingPopulation > 0.05f)
			{
				yield return new WaitForSeconds(0.25f);
			}
			break;
			IL_06db:
			while (monsterSpawnData.remainingPopulation > nextWaveThreshold && Time.time - waveStartTime < waveTimeout)
			{
				yield return new WaitForSeconds(0.25f);
			}
			waveIndex++;
			continue;
			end_IL_02cd:
			break;
		}
		s.onFinish?.Invoke();
		ongoingSpawns.Remove(s);
	}

	internal (Vector3, Quaternion) GetSpawnMonsterPosRot(SpawnMonsterSettings s, Entity monster)
	{
		Vector3 vector = ((monster is Monster { spawnPosOverride: var spawnPosOverride } && spawnPosOverride.HasValue) ? spawnPosOverride.Value : ((s.spawnPosGetter != null) ? s.spawnPosGetter() : ((!(s.section != null)) ? GetSpawnPositionNearPlayer(s.rule.spawnMinDistance, s.rule.spawnMaxDistance, s.hero) : s.section.monsters.GetSpawnPositionInSection(s.rule.spawnMinDistance, s.rule.spawnMaxDistance))));
		Quaternion item;
		if (monster is Monster { spawnRotOverride: var spawnRotOverride } && spawnRotOverride.HasValue)
		{
			item = spawnRotOverride.Value;
		}
		else if (s.spawnRotGetter != null)
		{
			item = s.spawnRotGetter();
		}
		else
		{
			Vector3 vector2 = NetworkedManagerBase<ActorManager>.instance.allHeroes[0].position;
			float num = Vector3.Distance(vector, NetworkedManagerBase<ActorManager>.instance.allHeroes[0].position);
			for (int i = 1; i < NetworkedManagerBase<ActorManager>.instance.allHeroes.Count; i++)
			{
				Vector3 position = NetworkedManagerBase<ActorManager>.instance.allHeroes[i].position;
				float num2 = Vector3.Distance(vector, position);
				if (!(num2 >= num))
				{
					num = num2;
					vector2 = position;
				}
			}
			item = Quaternion.LookRotation(vector2 - vector).Flattened();
		}
		return (vector, item);
	}

	private Entity SpawnMonsterImp(SpawnMonsterSettings s, MonsterSpawnData monsterSpawnData, Entity monster, float popCost)
	{
		try
		{
			(Vector3, Quaternion) spawnMonsterPosRot = GetSpawnMonsterPosRot(s, monster);
			Entity entity = Dew.SpawnEntity(monster, spawnMonsterPosRot.Item1, spawnMonsterPosRot.Item2, NetworkedManagerBase<ActorManager>.instance.serverActor, DewPlayer.creep, NetworkedManagerBase<GameManager>.instance.ambientLevel, delegate(Entity e)
			{
				if (s.isCutsceneSkipped)
				{
					e.Visual.NetworkskipSpawning = true;
				}
				onBeforeSpawn?.Invoke(e);
				s.beforeSpawn?.Invoke(e);
				monsterSpawnData.remainingPopulation += popCost;
				e.EntityEvent_OnDeath += (Action<EventInfoKill>)delegate(EventInfoKill kill)
				{
					monsterSpawnData.remainingPopulation -= popCost;
					monsterSpawnData.lastKiller = kill.actor.firstEntity;
					monsterSpawnData.lastDeathPosition = kill.victim.agentPosition;
				};
			});
			onAfterSpawn?.Invoke(entity);
			s.afterSpawn?.Invoke(entity);
			Hero hero = null;
			float num = float.PositiveInfinity;
			foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
			{
				if (!(humanPlayer.hero == null) && !humanPlayer.hero.isKnockedOut)
				{
					float num2 = Vector2.SqrMagnitude(humanPlayer.hero.position.ToXY() - spawnMonsterPosRot.Item1.ToXY());
					if (num2 < num)
					{
						num = num2;
						hero = humanPlayer.hero;
					}
				}
			}
			if (hero != null)
			{
				entity.Control.MoveToDestination(hero.position, immediately: false);
			}
			return entity;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	private Vector3 GetSpawnPositionNearPlayer(float minDist, float maxDist, Hero target = null)
	{
		if (target == null)
		{
			target = Dew.SelectRandomAliveHero();
		}
		for (int i = 0; i < 30; i++)
		{
			Vector3 vector = target.agentPosition + UnityEngine.Random.insideUnitSphere.Flattened().normalized * UnityEngine.Random.Range(minDist, maxDist);
			if (i < 29)
			{
				if (i < 10)
				{
					bool flag = false;
					bool flag2 = true;
					foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
					{
						float num = Vector2.Distance(humanPlayer.hero.agentPosition.ToXY(), vector.ToXY());
						if (num < minDist)
						{
							flag = true;
							break;
						}
						if (num < maxDist)
						{
							flag2 = false;
						}
					}
					if (flag || flag2)
					{
						continue;
					}
				}
				if (FilterSpawnPosition(vector, out var filteredPos, target.agentPosition))
				{
					return filteredPos;
				}
				continue;
			}
			Debug.LogWarning("Using fallback spawn position for room '" + SceneManager.GetActiveScene().name + "'");
			FilterSpawnPosition(vector, out var filteredPos2, target.agentPosition);
			return filteredPos2;
		}
		throw new InvalidOperationException("");
	}

	internal static bool FilterSpawnPosition(Vector3 pos, out Vector3 filteredPos, Vector3? pathPivot)
	{
		if (_tempPath == null)
		{
			_tempPath = new NavMeshPath();
		}
		if (!Physics.Raycast(pos + Vector3.up * 50f, Vector3.down, out var hitInfo, 100f, LayerMasks.Ground))
		{
			filteredPos = pos;
			return false;
		}
		if (!NavMesh.SamplePosition(hitInfo.point, out var hit, 5f, -1))
		{
			filteredPos = hitInfo.point;
			return false;
		}
		if ((pathPivot.HasValue && !NavMesh.CalculatePath(pathPivot.Value, hit.position, -1, _tempPath)) || _tempPath.status != NavMeshPathStatus.PathComplete)
		{
			filteredPos = hit.position;
			return false;
		}
		filteredPos = hit.position;
		return true;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CallOnCreateAsMiniBoss__Entity(Entity e)
	{
		((ISpawnableAsMiniBoss)e).OnCreateAsMiniBoss();
	}

	protected static void InvokeUserCode_CallOnCreateAsMiniBoss__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC CallOnCreateAsMiniBoss called on server.");
		}
		else
		{
			((RoomMonsters)obj).UserCode_CallOnCreateAsMiniBoss__Entity(reader.ReadNetworkBehaviour<Entity>());
		}
	}

	static RoomMonsters()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(RoomMonsters), "System.Void RoomMonsters::CallOnCreateAsMiniBoss(Entity)", InvokeUserCode_CallOnCreateAsMiniBoss__Entity);
	}
}
