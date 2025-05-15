using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Section_Monsters : RoomSectionComponent
{
	public SectionCombatAreaType combatAreaSettings;

	public CombatAreaActivateCondition activateCondition;

	public MonsterSpawnRule ruleOverride;

	public Transform spawnPosOverride;

	public Vector2 addedInitDelay;

	public bool spawnMiniBossInstead;

	public UnityEvent onStartCombatArea = new UnityEvent();

	public UnityEvent onClearCombatArea = new UnityEvent();

	private Room_Barrier[] _barriers;

	private static NavMeshPath _tempPath;

	public bool didClearCombatArea { get; set; }

	public bool isCombatActive { get; private set; }

	public bool isMarkedAsCombatArea { get; internal set; }

	public float maxPopulation
	{
		get
		{
			DewGameSessionSettings gss = NetworkedManagerBase<GameManager>.instance.gss;
			float baseValue = gss.maxSectionPopulation;
			float val = baseValue;
			val += baseValue * (gss.maxSectionPopulationMultiplierByAmbientLevel.Evaluate(NetworkedManagerBase<GameManager>.instance.ambientLevel - 1) - 1f);
			val += baseValue * (gss.maxSectionPopulationMultiplierPerPlayerInSection - 1f) * (float)Mathf.Max(base.section.numOfHeroes - 1, 0);
			if (SingletonDewNetworkBehaviour<Room>.softInstance != null)
			{
				val *= SingletonDewNetworkBehaviour<Room>.softInstance.monsters.maxPopulationMultiplier;
			}
			return val * NetworkedManagerBase<GameManager>.instance.maxAndSpawnedPopulationMultiplier;
		}
	}

	public float population { get; private set; }

	public bool isOverPopulation => population >= maxPopulation;

	protected override void Awake()
	{
		base.Awake();
		base.section.onEntitiesChanged.AddListener(delegate
		{
			population = 0f;
			foreach (Entity entity in base.section.entities)
			{
				if (entity is Monster monster)
				{
					population += monster.populationCost;
				}
			}
		});
	}

	private void Start()
	{
		_barriers = GetComponentsInChildren<Room_Barrier>(includeInactive: true);
		(activateCondition switch
		{
			CombatAreaActivateCondition.OnEnterFirstTime => base.section.onEnterFirstTime, 
			CombatAreaActivateCondition.OnEveryonePresent => base.section.onEveryonePresent, 
			_ => throw new ArgumentOutOfRangeException(), 
		}).AddListener(delegate
		{
			SpawnMonsterSettings settings;
			if (isMarkedAsCombatArea)
			{
				isCombatActive = true;
				NetworkedManagerBase<ZoneManager>.instance.currentRoom.numOfActivatedCombatAreas++;
				foreach (Entity entity in base.section.entities)
				{
					if (entity is Hero hero)
					{
						hero.MarkAsInCombat();
					}
				}
				Room_Barrier[] barriers = _barriers;
				foreach (Room_Barrier room_Barrier in barriers)
				{
					if (!(room_Barrier == null))
					{
						room_Barrier.Close();
					}
				}
				try
				{
					onStartCombatArea.Invoke();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				MonsterSpawnRule rule = ((ruleOverride != null) ? ruleOverride : SingletonDewNetworkBehaviour<Room>.instance.monsters.defaultRule);
				RoomMonsters.MonsterSpawnData monsterSpawnData = new RoomMonsters.MonsterSpawnData();
				settings = new SpawnMonsterSettings
				{
					rule = rule,
					section = base.section,
					spawnPosGetter = ((spawnPosOverride == null) ? null : ((Func<Vector3>)(() => spawnPosOverride.position))),
					spawnRotGetter = ((spawnPosOverride == null) ? null : ((Func<Quaternion>)(() => spawnPosOverride.rotation))),
					initDelayFlat = UnityEngine.Random.Range(addedInitDelay.x, addedInitDelay.y),
					monsterSpawnData = monsterSpawnData,
					onFinish = delegate
					{
						isCombatActive = false;
						didClearCombatArea = true;
						Room_Barrier[] barriers2 = _barriers;
						foreach (Room_Barrier room_Barrier2 in barriers2)
						{
							if (!(room_Barrier2 == null))
							{
								room_Barrier2.Open();
							}
						}
						try
						{
							onClearCombatArea.Invoke();
						}
						catch (Exception exception2)
						{
							Debug.LogException(exception2);
						}
					}
				};
				if (spawnMiniBossInstead)
				{
					StartCoroutine(Routine());
				}
				else
				{
					SingletonDewNetworkBehaviour<Room>.instance.monsters.SpawnMonsters(settings);
				}
			}
			IEnumerator Routine()
			{
				yield return Dew.WaitForAggroedEnemiesRoutine();
				SingletonDewNetworkBehaviour<Room>.instance.monsters.SpawnMiniBoss(settings);
			}
		});
	}

	public Vector3 GetSpawnPositionInSection(float minDist, float maxDist)
	{
		return GetSpawnPositionInSection(delegate(Vector3 pos)
		{
			bool flag = false;
			bool flag2 = true;
			foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
			{
				float num = Vector2.Distance(humanPlayer.hero.agentPosition.ToXY(), pos.ToXY());
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
			return !flag && !flag2;
		});
	}

	public Vector3 GetSpawnPositionInSection(Func<Vector3, bool> evaluator = null)
	{
		if (_tempPath == null)
		{
			_tempPath = new NavMeshPath();
		}
		for (int tries = 0; tries < 30; tries++)
		{
			Vector3 worldPos = base.section.GetRandomWorldPosition();
			if (tries < 29)
			{
				if ((tries >= 10 || evaluator == null || evaluator(worldPos)) && RoomMonsters.FilterSpawnPosition(worldPos, out var filtered, base.section.pathablePivot))
				{
					Debug.DrawLine(filtered, filtered + Vector3.up, Color.red, 2f);
					return filtered;
				}
				continue;
			}
			Debug.LogWarning("Using fallback spawn position for '" + SceneManager.GetActiveScene().name + "::" + base.name + "'");
			RoomMonsters.FilterSpawnPosition(worldPos, out var filtered2, base.section.pathablePivot);
			return filtered2;
		}
		throw new InvalidOperationException("");
	}

	private void OnDrawGizmos()
	{
		if (combatAreaSettings == SectionCombatAreaType.Yes || isMarkedAsCombatArea)
		{
			DewGizmos.DrawText("Combat", base.transform.position + Vector3.down * 4f, Color.red, 12);
		}
	}
}
