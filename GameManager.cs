using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class GameManager : NetworkedManagerBase<GameManager>
{
	public DewGameSessionSettings gss;

	[CompilerGenerated]
	[SyncVar(hook = "OnDifficultyChanged")]
	private DewDifficultySettings _003Cdifficulty_003Ek__BackingField;

	public bool disableStuckCheck;

	public SafeAction<DewDifficultySettings, DewDifficultySettings> ClientEvent_OnDifficultyChanged;

	public SafeAction<int, int> ClientEvent_OnAmbientLevelChanged;

	public SafeAction ClientEvent_OnGameConcluded;

	[CompilerGenerated]
	[SyncVar(hook = "OnAmbientLevelChanged")]
	private int _003CambientLevel_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsGameTimePausedChanged")]
	private bool _003CisGameTimePaused_003Ek__BackingField;

	[SyncVar]
	private float _gameTimeBase;

	[SyncVar]
	private float _gameTimeTickStartTime;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsGameConcludedChanged")]
	private bool _003CisGameConcluded_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CmaxAndSpawnedPopulationMultiplier_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CspawnedPopulation_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisCleanseEnabled_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisPureDreamEnabled_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisGameOverEnabled_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private string _003CgameId_003Ek__BackingField;

	private float _gameOverTime;

	public Func<float> predictionStrengthOverride;

	private static List<Action> _lazyCalledFunctions;

	public DewDifficultySettings difficulty
	{
		[CompilerGenerated]
		get
		{
			return _003Cdifficulty_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cdifficulty_003Ek__BackingField = value;
		}
	}

	public int ambientLevel
	{
		[CompilerGenerated]
		get
		{
			return _003CambientLevel_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CambientLevel_003Ek__BackingField = value;
		}
	}

	public bool isGameTimePaused
	{
		[CompilerGenerated]
		get
		{
			return _003CisGameTimePaused_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisGameTimePaused_003Ek__BackingField = value;
		}
	}

	public float elapsedGameTime => _gameTimeBase + ((!isGameTimePaused) ? ((float)NetworkTime.time - _gameTimeTickStartTime) : 0f);

	public bool isGameConcluded
	{
		[CompilerGenerated]
		get
		{
			return _003CisGameConcluded_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisGameConcluded_003Ek__BackingField = value;
		}
	}

	public float maxAndSpawnedPopulationMultiplier
	{
		[CompilerGenerated]
		get
		{
			return _003CmaxAndSpawnedPopulationMultiplier_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxAndSpawnedPopulationMultiplier_003Ek__BackingField = value;
		}
	} = 1f;

	public float spawnedPopulation
	{
		[CompilerGenerated]
		get
		{
			return _003CspawnedPopulation_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CspawnedPopulation_003Ek__BackingField = value;
		}
	}

	public float maxSpawnedPopulation
	{
		get
		{
			float value = gss.maxGlobalPopulation;
			value *= 1f + (gss.maxGlobalPopulationMultiplierPerPlayer - 1f) * GetMultiplayerDifficultyFactor(reduceWhenDead: true) + (gss.maxGlobalPopulationMultiplierByAmbientLevel.Evaluate(ambientLevel - 1) - 1f);
			value *= difficulty.maxPopulationMultiplier;
			if (SingletonDewNetworkBehaviour<Room>.softInstance != null)
			{
				value *= SingletonDewNetworkBehaviour<Room>.softInstance.monsters.maxPopulationMultiplier;
			}
			return value * maxAndSpawnedPopulationMultiplier;
		}
	}

	public bool isSpawnOverPopulation => spawnedPopulation >= maxSpawnedPopulation;

	public bool isCleanseEnabled
	{
		[CompilerGenerated]
		get
		{
			return _003CisCleanseEnabled_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisCleanseEnabled_003Ek__BackingField = value;
		}
	}

	public bool isPureDreamEnabled
	{
		[CompilerGenerated]
		get
		{
			return _003CisPureDreamEnabled_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisPureDreamEnabled_003Ek__BackingField = value;
		}
	}

	public bool isGameOverEnabled
	{
		[CompilerGenerated]
		get
		{
			return _003CisGameOverEnabled_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisGameOverEnabled_003Ek__BackingField = value;
		}
	} = true;

	public bool isCombatRiftEnabled { get; set; }

	public bool isLucidRiftEnabled { get; set; }

	public string gameId
	{
		[CompilerGenerated]
		get
		{
			return _003CgameId_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CgameId_003Ek__BackingField = value;
		}
	}

	public DewDifficultySettings Network_003Cdifficulty_003Ek__BackingField
	{
		get
		{
			return difficulty;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref difficulty, 1uL, OnDifficultyChanged);
		}
	}

	public int Network_003CambientLevel_003Ek__BackingField
	{
		get
		{
			return ambientLevel;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref ambientLevel, 2uL, OnAmbientLevelChanged);
		}
	}

	public bool Network_003CisGameTimePaused_003Ek__BackingField
	{
		get
		{
			return isGameTimePaused;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isGameTimePaused, 4uL, OnIsGameTimePausedChanged);
		}
	}

	public float Network_gameTimeBase
	{
		get
		{
			return _gameTimeBase;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _gameTimeBase, 8uL, null);
		}
	}

	public float Network_gameTimeTickStartTime
	{
		get
		{
			return _gameTimeTickStartTime;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _gameTimeTickStartTime, 16uL, null);
		}
	}

	public bool Network_003CisGameConcluded_003Ek__BackingField
	{
		get
		{
			return isGameConcluded;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isGameConcluded, 32uL, OnIsGameConcludedChanged);
		}
	}

	public float Network_003CmaxAndSpawnedPopulationMultiplier_003Ek__BackingField
	{
		get
		{
			return maxAndSpawnedPopulationMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref maxAndSpawnedPopulationMultiplier, 64uL, null);
		}
	}

	public float Network_003CspawnedPopulation_003Ek__BackingField
	{
		get
		{
			return spawnedPopulation;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref spawnedPopulation, 128uL, null);
		}
	}

	public bool Network_003CisCleanseEnabled_003Ek__BackingField
	{
		get
		{
			return isCleanseEnabled;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isCleanseEnabled, 256uL, null);
		}
	}

	public bool Network_003CisPureDreamEnabled_003Ek__BackingField
	{
		get
		{
			return isPureDreamEnabled;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isPureDreamEnabled, 512uL, null);
		}
	}

	public bool Network_003CisGameOverEnabled_003Ek__BackingField
	{
		get
		{
			return isGameOverEnabled;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isGameOverEnabled, 1024uL, null);
		}
	}

	public string Network_003CgameId_003Ek__BackingField
	{
		get
		{
			return gameId;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref gameId, 2048uL, null);
		}
	}

	private void OnIsGameConcludedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			ClientEvent_OnGameConcluded?.Invoke();
		}
	}

	[Server]
	public void UpdateSpawnedPopulation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::UpdateSpawnedPopulation()' called when server was not active");
			return;
		}
		float p = 0f;
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity is Monster { campPosition: null } m)
			{
				p += m.populationCost;
			}
		}
		Network_003CspawnedPopulation_003Ek__BackingField = p;
	}

	protected override void Awake()
	{
		base.Awake();
		if (NetworkServer.active)
		{
			Network_003Cdifficulty_003Ek__BackingField = GetDifficulty();
		}
	}

	protected virtual DewDifficultySettings GetDifficulty()
	{
		return null;
	}

	public virtual IList<string> GetLucidDreams()
	{
		return NetworkedManagerBase<GameSettingsManager>.instance.lucidDreams;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		SetAmbientLevel(1);
		NetworkServer.maxConnections = 0;
		Network_gameTimeBase = 0f;
		Network_gameTimeTickStartTime = (float)NetworkTime.time;
		NetworkedManagerBase<ActorManager>.instance.onEntityAdd += (Action<Entity>)delegate(Entity e)
		{
			difficulty.ApplyDifficultyModifiers(e);
		};
		SetupAIDifficulty();
		EntityAI.DisableAI = false;
		Network_003CgameId_003Ek__BackingField = Guid.NewGuid().ToString();
		foreach (GameModifierBase mod in DewResources.FindAllByTypeSubstring<GameModifierBase>("GameMod_"))
		{
			if (Dew.IsGameModifierIncludedInGame(mod.GetType().Name) && mod.IsAvailableInGame())
			{
				Debug.Log("Creating " + mod.GetType().Name);
				Dew.CreateActor(mod, Vector3.zero, Quaternion.identity);
			}
		}
		foreach (string lucidDream in GetLucidDreams())
		{
			LucidDream prefab = DewResources.GetByShortTypeName<LucidDream>(lucidDream);
			if (!(prefab == null))
			{
				Debug.Log("Creating " + prefab.GetType().Name);
				Dew.CreateActor(prefab, Vector3.zero, Quaternion.identity);
			}
		}
		foreach (DewPlayer h in DewPlayer.humanPlayers)
		{
			if (DewBuildProfile.current.startGold > 0)
			{
				h.EarnGold(DewBuildProfile.current.startGold);
			}
			if (DewBuildProfile.current.startDreamDust > 0)
			{
				h.EarnDreamDust(DewBuildProfile.current.startDreamDust);
			}
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		CallOnReady(delegate
		{
			NetworkedManagerBase<ConsoleManager>.instance.ExecuteAutoExec(ConsoleManager.AutoExecKey.Game);
			NetworkedManagerBase<ConsoleManager>.instance.ExecuteAutoExec(base.isServer ? ConsoleManager.AutoExecKey.GameServer : ConsoleManager.AutoExecKey.GameClient);
		});
		DewResources.AddPreloadRule(this, delegate(PreloadInterface preload)
		{
			foreach (string current in DewResources.loadedGuids)
			{
				if (!DewResources.database.guidToType.TryGetValue(current, out var value))
				{
					preload.AddGuid(current);
				}
				else
				{
					string text = value.Name;
					if (!text.StartsWith("Gem_") && !text.StartsWith("Mon_") && !text.StartsWith("St_") && !text.StartsWith("Hero_") && !text.StartsWith("At_") && !text.StartsWith("Artifact_") && !text.StartsWith("RoomMod_") && !text.StartsWith("GameMod_") && !text.StartsWith("Ai_") && !text.StartsWith("Se_"))
					{
						preload.AddGuid(current);
					}
				}
			}
		});
		DewResources.AddPreloadRule(this, delegate(PreloadInterface preload)
		{
			preload.AddType("MockAbilityInstance");
			preload.AddType("Se_GenericEffectContainer");
			preload.AddType("Se_Elm_Fire");
			preload.AddType("Se_Elm_Dark");
			preload.AddType("Se_Elm_Cold");
			preload.AddType("Se_Elm_Light");
			preload.AddType("Se_HealthCost");
			preload.AddType("Se_HunterBuff");
			preload.AddType("Se_InConversation");
			preload.AddType("Se_PortalTransition");
			preload.AddType("Se_BarrierPassThrough");
			preload.AddType("Se_GenericShield_Stacking");
			preload.AddType("Se_GenericShield_OneShot");
			preload.AddType("Se_HeroKnockedOut");
			preload.AddType("Ai_HunterArtillery_Small");
			preload.AddType("Ai_HunterArtillery_Big");
			preload.AddType("Pickup_LargeGoldOrb");
			preload.AddType("Pickup_MediumGoldOrb");
			preload.AddType("Pickup_SmallGoldOrb");
			preload.AddType("Pickup_LargeExpOrb");
			preload.AddType("Pickup_MediumExpOrb");
			preload.AddType("Pickup_SmallExpOrb");
			preload.AddType("Pickup_RegenOrb");
		});
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		DoLogicUpdate_LazyCall(dt);
		if (base.isServer && !isGameConcluded)
		{
			CheckGameOver(dt);
		}
	}

	private void CheckGameOver(float dt)
	{
		if (!isGameOverEnabled || NetworkedManagerBase<ActorManager>.instance.allHeroes.Count == 0 || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			_gameOverTime = 0f;
			return;
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!allHero.isKnockedOut)
			{
				_gameOverTime = 0f;
				return;
			}
		}
		_gameOverTime += dt;
		if (_gameOverTime >= 4f)
		{
			WrapUpAndShowResult(DewGameResult.ResultType.GameOver);
		}
	}

	public void ConcludeDemo()
	{
		NetworkedManagerBase<ZoneManager>.instance.DoDeadEndTravel();
		WrapUpAndShowResult(DewGameResult.ResultType.DemoFinish);
	}

	public void WrapUpAndShowResult(DewGameResult.ResultType type)
	{
		if (!isGameConcluded)
		{
			Network_003CisGameConcluded_003Ek__BackingField = true;
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (type != DewGameResult.ResultType.GameOver)
			{
				yield return new WaitForSeconds(1.5f);
			}
			NetworkedManagerBase<GameResultManager>.instance.WrapUp(type);
			do
			{
				yield return new WaitForSecondsRealtime(0.25f);
			}
			while (!DewPlayer.humanPlayers.All((DewPlayer player) => player.isReady));
			if (DewPlayer.humanPlayers.Count <= 1 && DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				DewNetworkManager.instance.EndSession();
			}
			else
			{
				DewNetworkManager.instance.RestartSession();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_lazyCalledFunctions.Clear();
		foreach (GameObject g in Dew._destroyingGameObject)
		{
			if (g != null)
			{
				UnityEngine.Object.Destroy(g);
			}
		}
		Dew._destroyingGameObject.Clear();
		Actor[] array = UnityEngine.Object.FindObjectsOfType<Actor>(includeInactive: true);
		foreach (Actor a in array)
		{
			if (a != null)
			{
				UnityEngine.Object.Destroy(a.gameObject);
			}
		}
		EffectAutoDestroy[] array2 = UnityEngine.Object.FindObjectsOfType<EffectAutoDestroy>(includeInactive: true);
		foreach (EffectAutoDestroy a2 in array2)
		{
			if (a2 != null)
			{
				UnityEngine.Object.Destroy(a2.gameObject);
			}
		}
	}

	private void OnAmbientLevelChanged(int oldVal, int newVal)
	{
		Debug.Log($"Ambient level is now {newVal}");
		ClientEvent_OnAmbientLevelChanged?.Invoke(oldVal, newVal);
	}

	private void OnIsGameTimePausedChanged(bool oldVal, bool newVal)
	{
		if (base.isServer)
		{
			if (newVal)
			{
				Network_gameTimeBase = _gameTimeBase + (float)NetworkTime.time - _gameTimeTickStartTime;
			}
			else
			{
				Network_gameTimeTickStartTime = (float)NetworkTime.time;
			}
		}
	}

	public virtual void LoadNextZone()
	{
	}

	private void OnDifficultyChanged(DewDifficultySettings old, DewDifficultySettings newVal)
	{
		Debug.Log("Difficulty is at " + newVal.name + ".");
		SetupAIDifficulty();
		ClientEvent_OnDifficultyChanged?.Invoke(old, newVal);
	}

	private void SetupAIDifficulty()
	{
		EntityAI.PositionSampleCount = difficulty.positionSampleCount;
		EntityAI.PositionSampleLagBehindFrames = difficulty.positionSampleLagBehindFrames;
		EntityAI.PositionSampleInterval = difficulty.positionSampleInterval;
	}

	[Server]
	public void SetElapsedGameTime(float newTime)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::SetElapsedGameTime(System.Single)' called when server was not active");
			return;
		}
		Network_gameTimeBase = newTime;
		Network_gameTimeTickStartTime = (float)NetworkTime.time;
	}

	[ClientRpc]
	public void SetDisconnectedForEveryoneElse()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void GameManager::SetDisconnectedForEveryoneElse()", -1253838744, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void SetAmbientLevel(int newLevel)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameManager::SetAmbientLevel(System.Int32)' called when server was not active");
		}
		else
		{
			Network_003CambientLevel_003Ek__BackingField = Mathf.Max(newLevel, 1);
		}
	}

	public float GetExpDropFromEntity(Entity ent)
	{
		if (!(ent is Monster monster))
		{
			return 0f;
		}
		float amount = 5f * Mathf.Pow(1.2f, Mathf.Min(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * 4, 30));
		amount *= gss.expMultiplier.Get(monster.type);
		amount = GetAdjustedDroppedExperienceAmount(amount);
		return amount * gss.expGlobalMultiplier * UnityEngine.Random.Range(1f - gss.expDropDeviation, 1f + gss.expDropDeviation);
	}

	public int GetGoldDropFromEntity(Entity ent)
	{
		if (!(ent is Monster monster))
		{
			return 0;
		}
		float amount = gss.goldDropByMonsterLevel.Evaluate(ent.level);
		amount *= gss.goldMultiplier.Get(monster.type);
		amount = GetAdjustedDroppedGoldAmount(Mathf.RoundToInt(amount));
		return DewMath.RandomRoundToInt(amount * gss.goldGlobalMultiplier * UnityEngine.Random.Range(1f - gss.goldDropDeviation, 1f + gss.goldDropDeviation));
	}

	public int GetAdjustedMonsterWaves(int original)
	{
		float bonus = gss.monsterWavesMultiplierPerPlayer - 1f;
		bonus *= GetMultiplayerDifficultyFactor(reduceWhenDead: true);
		return DewMath.RandomRoundToInt((float)original * (bonus + 1f));
	}

	public float GetAdjustedMonsterSpawnPopulation(float original, bool ignoreTurnMultiplier = false, bool ignoreCoopMultiplier = false)
	{
		float multiplayerMultiplier = (ignoreCoopMultiplier ? 1f : (1f + (gss.monsterSpawnPopulationMultiplierPerPlayer - 1f) * GetMultiplayerDifficultyFactor(reduceWhenDead: true)));
		float turnMultiplier = (ignoreTurnMultiplier ? 1f : gss.monsterSpawnPopulationMultiplierByTurnIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentTurnIndex));
		return original * multiplayerMultiplier * turnMultiplier * maxAndSpawnedPopulationMultiplier;
	}

	public float GetAdjustedDroppedExperienceAmount(float original)
	{
		float bonus = gss.droppedExpMultiplierPerPlayer - 1f;
		bonus *= GetMultiplayerDifficultyFactor(reduceWhenDead: false);
		return DewMath.RandomRoundToInt(original * (bonus + 1f));
	}

	public int GetAdjustedDroppedGoldAmount(int original)
	{
		float bonus = gss.droppedGoldMultiplierPerPlayer - 1f;
		bonus *= GetMultiplayerDifficultyFactor(reduceWhenDead: true);
		return DewMath.RandomRoundToInt((float)original * (bonus + 1f));
	}

	public float GetMonsterBonusHealthPercentageByMultiplayer(Monster m)
	{
		float bonus = GetMultiplayerDifficultyFactor(reduceWhenDead: true);
		switch (m.type)
		{
		case Monster.MonsterType.Lesser:
		case Monster.MonsterType.Normal:
			return bonus * gss.monsterBonusHealthPercentagePerPlayer;
		case Monster.MonsterType.MiniBoss:
			return bonus * gss.miniBossBonusHealthPercentagePerPlayer;
		case Monster.MonsterType.Boss:
			return bonus * gss.bossBonusHealthPercentagePerPlayer;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public float GetMonsterBonusPowerPercentage(Monster m)
	{
		float bonus = GetMultiplayerDifficultyFactor(reduceWhenDead: true);
		switch (m.type)
		{
		case Monster.MonsterType.Lesser:
		case Monster.MonsterType.Normal:
			return bonus * gss.monsterBonusPowerPercentagePerPlayer;
		case Monster.MonsterType.MiniBoss:
			return bonus * gss.miniBossBonusPowerPercentagePerPlayer;
		case Monster.MonsterType.Boss:
			return bonus * gss.bossBonusPowerPercentagePerPlayer;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public int GetGemUpgradeDreamDustCost(Gem gem)
	{
		return GetGemUpgradeDreamDustCost(gem.quality);
	}

	public int GetGemUpgradeDreamDustCost(int quality)
	{
		return Mathf.Max(1, Mathf.RoundToInt(gss.gemUpgradeDreamDustByQuality.Evaluate(quality)));
	}

	public int GetGemUpgradeAddedQuality()
	{
		return gss.gemAddedQualityOnUpgrade;
	}

	public int GetSkillUpgradeDreamDustCost(SkillTrigger skill)
	{
		return GetSkillUpgradeDreamDustCost(skill.level);
	}

	public int GetSkillUpgradeDreamDustCost(int level)
	{
		return Mathf.Max(1, Mathf.RoundToInt(gss.skillUpgradeDreamDustByLevel.Evaluate(level)));
	}

	public float GetPredictionStrength()
	{
		try
		{
			if (predictionStrengthOverride != null)
			{
				return predictionStrengthOverride();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return difficulty.predictionStrengthCurve.Evaluate(UnityEngine.Random.value);
	}

	public float GetMultiplayerDifficultyFactor(bool reduceWhenDead)
	{
		if (!reduceWhenDead)
		{
			return Mathf.Max((float)DewPlayer.humanPlayers.Count - 1f, 0f);
		}
		int total = DewPlayer.humanPlayers.Count;
		int alive = 0;
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			if (!humanPlayer.hero.IsNullInactiveDeadOrKnockedOut())
			{
				alive++;
			}
		}
		return Mathf.Max((float)alive + (float)(total - alive) * 0.55f - 1f, 0f);
	}

	public float GetSpecialSkillChanceMultiplier()
	{
		return difficulty.specialSkillChanceMultiplier;
	}

	public int GetCleanseSkillMinLevel()
	{
		return gss.skillCleanseMinLevel;
	}

	public int GetCleanseGemMinQuality()
	{
		return gss.gemCleanseMinQuality;
	}

	public int GetCleanseReturnedDreamDust(DewPlayer player, SkillTrigger skill)
	{
		int min = GetCleanseSkillMinLevel();
		if (skill.level <= min)
		{
			return 0;
		}
		float floatAmount = 0f;
		for (int i = skill.level - 1; i >= min; i--)
		{
			floatAmount += (float)GetSkillUpgradeDreamDustCost(i);
		}
		floatAmount *= player.cleanseRefundMultiplier;
		return Mathf.Max(Mathf.RoundToInt(floatAmount), 1);
	}

	public int GetCleanseReturnedDreamDust(DewPlayer player, Gem gem)
	{
		int min = GetCleanseGemMinQuality();
		if (gem.quality <= min)
		{
			return 0;
		}
		float floatAmount = 0f;
		int addedQuality = GetGemUpgradeAddedQuality();
		int quality;
		for (quality = gem.quality; quality >= min + addedQuality; quality -= addedQuality)
		{
			floatAmount += (float)GetGemUpgradeDreamDustCost(quality - addedQuality);
		}
		int remaining = quality - min;
		floatAmount += (float)GetGemUpgradeDreamDustCost(min) * ((float)remaining / (float)addedQuality);
		floatAmount *= player.cleanseRefundMultiplier;
		return Mathf.Max(Mathf.RoundToInt(floatAmount), 1);
	}

	public int GetCleanseGoldCost(SkillTrigger skill)
	{
		return Mathf.RoundToInt(gss.skillCleanseCostByLevel.Evaluate(skill.level));
	}

	public int GetCleanseGoldCost(Gem gem)
	{
		return Mathf.RoundToInt(gss.gemCleanseCostByQuality.Evaluate(gem.quality));
	}

	public float GetGainedSkillHastePerSkillLevel(SkillTrigger skillTrigger)
	{
		if (skillTrigger.type != SkillType.Ultimate)
		{
			return gss.gainedSkillHastePerSkillLevel;
		}
		return 0f;
	}

	public float GetRegularMonsterHealthMultiplierByScaling(float scalingDifficultyFactor, float zi = float.NaN)
	{
		if (float.IsNaN(zi))
		{
			zi = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		}
		zi *= scalingDifficultyFactor;
		return Mathf.Lerp(GetWeakHealthScalingMultiplier_Imp(zi), GetStrongHealthScalingMultiplier_Imp(zi), 0.15f);
	}

	public float GetRegularMonsterDamageMultiplierByScaling(float scalingDifficultyFactor, float zi = float.NaN)
	{
		if (float.IsNaN(zi))
		{
			zi = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		}
		zi *= scalingDifficultyFactor;
		return GetUniversalDamageScalingMultiplier_Imp(zi);
	}

	public float GetMiniBossMonsterHealthMultiplierByScaling(float scalingDifficultyFactor, float zi = float.NaN)
	{
		if (float.IsNaN(zi))
		{
			zi = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		}
		zi *= scalingDifficultyFactor;
		return Mathf.Lerp(GetWeakHealthScalingMultiplier_Imp(zi), GetStrongHealthScalingMultiplier_Imp(zi), 0.7f);
	}

	public float GetMiniBossMonsterDamageMultiplierByScaling(float scalingDifficultyFactor, float zi = float.NaN)
	{
		if (float.IsNaN(zi))
		{
			zi = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		}
		zi *= scalingDifficultyFactor;
		return GetUniversalDamageScalingMultiplier_Imp(zi);
	}

	public float GetBossMonsterHealthMultiplierByScaling(float scalingDifficultyFactor, float zi = float.NaN)
	{
		if (float.IsNaN(zi))
		{
			zi = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		}
		zi *= scalingDifficultyFactor;
		return GetStrongHealthScalingMultiplier_Imp(zi);
	}

	public float GetBossMonsterDamageMultiplierByScaling(float scalingDifficultyFactor, float zi = float.NaN)
	{
		if (float.IsNaN(zi))
		{
			zi = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
		}
		zi *= scalingDifficultyFactor;
		return GetUniversalDamageScalingMultiplier_Imp(zi);
	}

	private float GetStrongHealthScalingMultiplier_Imp(float zi)
	{
		return GetMultiplier_Imp(zi, 0.0566392339721777, 0.209036492019735, 0.407011935005335, 0.999998990190336, 30);
	}

	private float GetWeakHealthScalingMultiplier_Imp(float zi)
	{
		return GetMultiplier_Imp(zi, -4.10471677947602E-08, 0.481667070394692, 0.19833143379239, 0.999997791915299, 20);
	}

	private float GetUniversalDamageScalingMultiplier_Imp(float zi)
	{
		return GetMultiplier_Imp(zi, 0.000109450559046085, 0.505623339094561, 0.249726671939049, 0.917160073613465, 8);
	}

	private float GetMultiplier_Imp(double x, double a, double b, double c, double d, int linearStart)
	{
		if (x <= (double)linearStart)
		{
			return (float)Get(x);
		}
		double delta = Get(linearStart + 1) - Get(linearStart);
		return (float)(Get(linearStart) + delta * (x - (double)linearStart));
		double Get(double param)
		{
			return a * param * param * param + b * param * param + c * param + d;
		}
	}

	protected virtual bool IsLazyCallReady()
	{
		if (base.isActiveAndEnabled && ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Normal && NetworkClient.active && NetworkClient.ready && !NetworkServer.isLoadingScene && DewPlayer.local != null)
		{
			return DewPlayer.local.hero != null;
		}
		return false;
	}

	private void DoLogicUpdate_LazyCall(float dt)
	{
		if (_lazyCalledFunctions.Count <= 0 || !IsLazyCallReady())
		{
			return;
		}
		List<Action> list = new List<Action>(_lazyCalledFunctions);
		_lazyCalledFunctions.Clear();
		foreach (Action a in list)
		{
			try
			{
				a?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public static void CallOnReady(Action action)
	{
		if (NetworkedManagerBase<GameManager>.instance != null && NetworkedManagerBase<GameManager>.instance.IsLazyCallReady())
		{
			action?.Invoke();
		}
		else
		{
			_lazyCalledFunctions.Add(action);
		}
	}

	static GameManager()
	{
		_lazyCalledFunctions = new List<Action>();
		RemoteProcedureCalls.RegisterRpc(typeof(GameManager), "System.Void GameManager::SetDisconnectedForEveryoneElse()", InvokeUserCode_SetDisconnectedForEveryoneElse);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_SetDisconnectedForEveryoneElse()
	{
		if (!NetworkServer.active)
		{
			DewNetworkManager.instance.didRegisterError = true;
			DewSessionError.ShowError(new DewException(DewExceptionType.Disconnected));
		}
	}

	protected static void InvokeUserCode_SetDisconnectedForEveryoneElse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetDisconnectedForEveryoneElse called on server.");
		}
		else
		{
			((GameManager)obj).UserCode_SetDisconnectedForEveryoneElse();
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteDewDifficultySettings(difficulty);
			writer.WriteInt(ambientLevel);
			writer.WriteBool(isGameTimePaused);
			writer.WriteFloat(_gameTimeBase);
			writer.WriteFloat(_gameTimeTickStartTime);
			writer.WriteBool(isGameConcluded);
			writer.WriteFloat(maxAndSpawnedPopulationMultiplier);
			writer.WriteFloat(spawnedPopulation);
			writer.WriteBool(isCleanseEnabled);
			writer.WriteBool(isPureDreamEnabled);
			writer.WriteBool(isGameOverEnabled);
			writer.WriteString(gameId);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteDewDifficultySettings(difficulty);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(ambientLevel);
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(isGameTimePaused);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteFloat(_gameTimeBase);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteFloat(_gameTimeTickStartTime);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteBool(isGameConcluded);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteFloat(maxAndSpawnedPopulationMultiplier);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteFloat(spawnedPopulation);
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteBool(isCleanseEnabled);
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteBool(isPureDreamEnabled);
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteBool(isGameOverEnabled);
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteString(gameId);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref difficulty, OnDifficultyChanged, reader.ReadDewDifficultySettings());
			GeneratedSyncVarDeserialize(ref ambientLevel, OnAmbientLevelChanged, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref isGameTimePaused, OnIsGameTimePausedChanged, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _gameTimeBase, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _gameTimeTickStartTime, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref isGameConcluded, OnIsGameConcludedChanged, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref maxAndSpawnedPopulationMultiplier, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref spawnedPopulation, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref isCleanseEnabled, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref isPureDreamEnabled, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref isGameOverEnabled, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref gameId, null, reader.ReadString());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref difficulty, OnDifficultyChanged, reader.ReadDewDifficultySettings());
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref ambientLevel, OnAmbientLevelChanged, reader.ReadInt());
		}
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isGameTimePaused, OnIsGameTimePausedChanged, reader.ReadBool());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _gameTimeBase, null, reader.ReadFloat());
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _gameTimeTickStartTime, null, reader.ReadFloat());
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isGameConcluded, OnIsGameConcludedChanged, reader.ReadBool());
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref maxAndSpawnedPopulationMultiplier, null, reader.ReadFloat());
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref spawnedPopulation, null, reader.ReadFloat());
		}
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isCleanseEnabled, null, reader.ReadBool());
		}
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isPureDreamEnabled, null, reader.ReadBool());
		}
		if ((num & 0x400L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isGameOverEnabled, null, reader.ReadBool());
		}
		if ((num & 0x800L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref gameId, null, reader.ReadString());
		}
	}
}
