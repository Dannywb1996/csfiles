using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public static class Dew
{
	public static readonly Color CommonColor = new Color(76f / 85f, 0.9411765f, 0.9490196f);

	public static readonly Color RareColor = new Color(0.20392157f, 84f / 85f, 1f);

	public static readonly Color EpicColor = new Color(0.78039217f, 31f / 85f, 0.96862745f);

	public static readonly Color LegendaryColor = new Color(1f, 0.24313726f, 0.20392157f);

	public static readonly Color CharacterColor = new Color(1f, 41f / 51f, 22f / 85f);

	public static readonly Color IdentityColor = new Color(1f, 41f / 51f, 22f / 85f);

	public static readonly string CommonColorHex = GetHex(CommonColor);

	public static readonly string RareColorHex = GetHex(RareColor);

	public static readonly string EpicColorHex = GetHex(EpicColor);

	public static readonly string LegendaryColorHex = GetHex(LegendaryColor);

	public static readonly string CharacterColorHex = GetHex(CharacterColor);

	public static readonly string IdentityColorHex = GetHex(IdentityColor);

	public static readonly Color DestructionColor = new Color(1f, 0.20392157f, 0.20392157f);

	public static readonly Color LifeColor = new Color(63f / 85f, 1f, 27f / 85f);

	public static readonly Color ImaginationColor = new Color(0.23921569f, 0.95686275f, 1f);

	public static readonly Color FlexibleColor = new Color(1f, 0.9137255f, 0.5882353f);

	private static NavMeshPath _path = new NavMeshPath();

	private static List<Hero> _aliveHeroesBuffer = new List<Hero>();

	private static readonly string[] BigNumberSuffixes = new string[5] { "", "K", "M", "B", "T" };

	private static readonly long[] RequiredMasteryPointsToLevelUp = new long[30]
	{
		24000L, 24000L, 24000L, 24000L, 28000L, 28000L, 28000L, 28000L, 28000L, 32000L,
		32000L, 32000L, 32000L, 32000L, 32000L, 32000L, 32000L, 32000L, 32000L, 36000L,
		36000L, 36000L, 36000L, 36000L, 36000L, 36000L, 36000L, 36000L, 36000L, 40000L
	};

	private static readonly int[] DejavuCostLegendary = new int[3] { 500, 400, 300 };

	private static readonly int[] DejavuCostEpic = new int[4] { 350, 300, 250, 200 };

	private static readonly int[] DejavuCostRare = new int[5] { 240, 210, 180, 150, 120 };

	private static readonly int[] DejavuCostCommon = new int[6] { 150, 140, 130, 120, 110, 100 };

	public const float DestroyDelay = 0.3f;

	internal static List<GameObject> _destroyingGameObject = new List<GameObject>();

	private static Camera _mainCamera;

	private static Canvas _gameCanvas;

	private static List<Type> _allHeroes;

	private static List<Type> _allSkills;

	private static List<Type> _allHeroSkills;

	private static List<Type> _allGems;

	private static List<Type> _allArtifacts;

	private static List<Type> _allGameModifiers;

	private static List<Type> _allRoomModifiers;

	private static List<Type> _allAchievements;

	private static List<Type> _allTutorialItems;

	private static List<Type> _allLucidDreams;

	private static List<DewReverieItem> _allReveries;

	private static Dictionary<string, DewReverieItem> _reveriesByName;

	private static Dictionary<string, Type> _achievementsByName;

	private static List<DewStarItemOld> _allOldStars;

	private static List<Type> _allStarTypes;

	private static Dictionary<string, DewStarItemOld> _oldStarsByName;

	private static Dictionary<Type, List<DewStarItemOld>> _oldStarsByHeroType;

	private static Dictionary<DewStarItemOld, Type> _heroTypeOfOldStar;

	public static Camera mainCamera
	{
		get
		{
			if (_mainCamera == null)
			{
				_mainCamera = Camera.main;
			}
			return _mainCamera;
		}
	}

	public static Canvas gameCanvas
	{
		get
		{
			if (_gameCanvas == null)
			{
				_gameCanvas = GameObject.FindGameObjectWithTag("GameCanvas").GetComponent<Canvas>();
			}
			return _gameCanvas;
		}
	}

	public static IReadOnlyList<Type> allHeroes
	{
		get
		{
			if (_allHeroes == null)
			{
				InitAllTypeReferences();
			}
			return _allHeroes;
		}
	}

	public static IReadOnlyList<Type> allSkills
	{
		get
		{
			if (_allSkills == null)
			{
				InitAllTypeReferences();
			}
			return _allSkills;
		}
	}

	public static IReadOnlyList<Type> allHeroSkills
	{
		get
		{
			if (_allHeroSkills == null)
			{
				InitAllTypeReferences();
			}
			return _allHeroSkills;
		}
	}

	public static IReadOnlyList<Type> allGems
	{
		get
		{
			if (_allGems == null)
			{
				InitAllTypeReferences();
			}
			return _allGems;
		}
	}

	public static IReadOnlyList<Type> allArtifacts
	{
		get
		{
			if (_allArtifacts == null)
			{
				InitAllTypeReferences();
			}
			return _allArtifacts;
		}
	}

	public static IReadOnlyList<Type> allGameModifiers
	{
		get
		{
			if (_allGameModifiers == null)
			{
				InitAllTypeReferences();
			}
			return _allGameModifiers;
		}
	}

	public static IReadOnlyList<Type> allRoomModifiers
	{
		get
		{
			if (_allRoomModifiers == null)
			{
				InitAllTypeReferences();
			}
			return _allRoomModifiers;
		}
	}

	public static IReadOnlyList<Type> allAchievements
	{
		get
		{
			if (_allAchievements == null)
			{
				InitAllTypeReferences();
			}
			return _allAchievements;
		}
	}

	public static IReadOnlyList<Type> allTutorialItems
	{
		get
		{
			if (_allTutorialItems == null)
			{
				InitAllTypeReferences();
			}
			return _allTutorialItems;
		}
	}

	public static IReadOnlyList<Type> allLucidDreams
	{
		get
		{
			if (_allLucidDreams == null)
			{
				InitAllTypeReferences();
			}
			return _allLucidDreams;
		}
	}

	public static IReadOnlyList<DewReverieItem> allReveries
	{
		get
		{
			if (_allReveries == null)
			{
				InitAllTypeReferences();
			}
			return _allReveries;
		}
	}

	public static IReadOnlyDictionary<string, DewReverieItem> reveriesByName
	{
		get
		{
			if (_reveriesByName == null)
			{
				InitAllTypeReferences();
			}
			return _reveriesByName;
		}
	}

	public static IReadOnlyDictionary<string, Type> achievementsByName
	{
		get
		{
			if (_achievementsByName == null)
			{
				InitAllTypeReferences();
			}
			return _achievementsByName;
		}
	}

	public static IReadOnlyList<DewStarItemOld> allOldStars
	{
		get
		{
			if (_allOldStars == null)
			{
				InitAllTypeReferences();
			}
			return _allOldStars;
		}
	}

	public static IReadOnlyList<Type> allStarTypes
	{
		get
		{
			if (_allStarTypes == null)
			{
				InitAllTypeReferences();
			}
			return _allStarTypes;
		}
	}

	public static IReadOnlyDictionary<string, DewStarItemOld> oldStarsByName
	{
		get
		{
			if (_oldStarsByName == null)
			{
				InitAllTypeReferences();
			}
			return _oldStarsByName;
		}
	}

	public static IReadOnlyDictionary<Type, List<DewStarItemOld>> oldStarsByHeroType
	{
		get
		{
			if (_oldStarsByHeroType == null)
			{
				InitAllTypeReferences();
			}
			return _oldStarsByHeroType;
		}
	}

	public static IReadOnlyDictionary<DewStarItemOld, Type> heroTypeOfOldStar
	{
		get
		{
			if (_heroTypeOfOldStar == null)
			{
				InitAllTypeReferences();
			}
			return _heroTypeOfOldStar;
		}
	}

	public static string GetHex(Color color)
	{
		return $"#{(int)(color.r * 255f):X2}{(int)(color.g * 255f):X2}{(int)(color.b * 255f):X2}";
	}

	public static Color GetRarityColor(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Common => CommonColor, 
			Rarity.Rare => RareColor, 
			Rarity.Epic => EpicColor, 
			Rarity.Legendary => LegendaryColor, 
			Rarity.Character => CharacterColor, 
			Rarity.Identity => IdentityColor, 
			_ => throw new ArgumentOutOfRangeException("rarity", rarity, null), 
		};
	}

	public static string GetRarityColorHex(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Common => CommonColorHex, 
			Rarity.Rare => RareColorHex, 
			Rarity.Epic => EpicColorHex, 
			Rarity.Legendary => LegendaryColorHex, 
			Rarity.Character => CharacterColorHex, 
			Rarity.Identity => IdentityColorHex, 
			_ => throw new ArgumentOutOfRangeException("rarity", rarity, null), 
		};
	}

	public static Color GetStarCategoryColor(StarType type)
	{
		return type switch
		{
			StarType.Life => LifeColor, 
			StarType.Destruction => DestructionColor, 
			StarType.Imagination => ImaginationColor, 
			StarType.Flexible => FlexibleColor, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static Vector3 GetGoodRewardPosition(Vector3 pivot, float randomOffset = 2f)
	{
		if (!IsOkay(pivot))
		{
			Vector3 def = Vector3.zero;
			if (DewPlayer.humanPlayers.Count > 0)
			{
				def = GetClosestAliveHero(Vector3.zero).agentPosition;
			}
			FilterNonOkayValues(ref pivot, def);
		}
		List<Vector2> avoidedPositions = new List<Vector2>();
		IBanRoomNodesNearby[] array = FindInterfacesOfType<IBanRoomNodesNearby>(includeInactive: true);
		foreach (IBanRoomNodesNearby t in array)
		{
			avoidedPositions.Add(((Component)t).transform.position.ToXY());
		}
		foreach (Actor a in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (a is SkillTrigger st)
			{
				if (st.owner == null)
				{
					avoidedPositions.Add(a.position);
				}
			}
			else if (a is Gem g)
			{
				if (g.owner == null)
				{
					avoidedPositions.Add(a.position);
				}
			}
			else if (a is IInteractable)
			{
				avoidedPositions.Add(a.position);
			}
		}
		pivot = GetPositionOnGround(pivot);
		float randomMag = randomOffset;
		Vector3 pos = default(Vector3);
		for (int j = 0; j < 20; j++)
		{
			pos = pivot + UnityEngine.Random.insideUnitCircle.ToXZ() * randomMag;
			pos = GetValidAgentDestination_LinearSweep(pivot, pos);
			bool invalid = false;
			foreach (Vector2 bp in avoidedPositions)
			{
				if (Vector2.SqrMagnitude(pos.ToXY() - bp) < 5.0625f)
				{
					invalid = true;
					break;
				}
			}
			if (!invalid)
			{
				break;
			}
			randomMag += 0.25f;
		}
		return GetValidAgentDestination_Closest(GetClosestAliveHero(pos).agentPosition, pos);
	}

	public static Vector3 GetPositionOnGround(Vector3 position, float yDiffRange = 50f)
	{
		FilterNonOkayValues(ref position);
		RaycastHit hit1;
		if (Physics.Raycast(position + Vector3.up * yDiffRange, Vector3.down, out var hit0, yDiffRange + 1f, LayerMasks.Ground))
		{
			position.y = hit0.point.y;
		}
		else if (Physics.SphereCast(position, 0.4f, Vector3.down, out hit1, yDiffRange, LayerMasks.Ground))
		{
			position.y = hit1.point.y;
		}
		FilterNonOkayValues(ref position);
		return position;
	}

	public static Vector3 GetValidAgentPosition(Vector3 pos, float threshold = 10f)
	{
		FilterNonOkayValues(ref pos);
		float maxDistance = 0.1f;
		if (maxDistance > threshold)
		{
			maxDistance = threshold;
		}
		do
		{
			if (NavMesh.SamplePosition(pos, out var startHit, maxDistance, -1))
			{
				Vector3 p = startHit.position;
				FilterNonOkayValues(ref p);
				return p;
			}
			maxDistance = ((!(maxDistance < 0.5f)) ? (maxDistance + 0.5f) : (maxDistance + 0.1f));
		}
		while (!(maxDistance > threshold));
		if (Application.isPlaying)
		{
			Debug.LogWarning("GetValidAgentPosition position is not a valid point on NavMesh!");
		}
		return pos;
	}

	public static Vector3 GetValidAgentDestination_LinearSweep(Vector3 start, Vector3 end)
	{
		FilterNonOkayValues(ref start);
		FilterNonOkayValues(ref end, start);
		float maxDistance = 0.5f;
		Vector3 samplePos;
		float stepDistance;
		while (true)
		{
			if (NavMesh.SamplePosition(start, out var startHit, maxDistance, -1))
			{
				start = startHit.position;
				samplePos = end;
				stepDistance = 0.25f;
				if (Vector3.Distance(start, end) / stepDistance > 10f)
				{
					stepDistance = Vector3.Distance(start, end) / 10f;
				}
				break;
			}
			maxDistance += 0.5f;
			if (maxDistance > 10f)
			{
				Debug.LogWarning("GetValidAgentDestination_LinearSweep start position is not a valid point on NavMesh!");
				return start;
			}
		}
		int step = 0;
		do
		{
			step++;
			if (step >= 50)
			{
				break;
			}
			if (NavMesh.SamplePosition(samplePos, out var hit, 1f, -1) && NavMesh.CalculatePath(start, samplePos, -1, _path) && _path.status == NavMeshPathStatus.PathComplete)
			{
				Vector3 p = hit.position;
				FilterNonOkayValues(ref p, start);
				return p;
			}
			samplePos = GetPositionOnGround(Vector3.MoveTowards(samplePos, start, stepDistance));
		}
		while (Vector2.SqrMagnitude(samplePos.ToXY() - start.ToXY()) > 0.2f);
		return start;
	}

	public static Vector3 GetValidAgentDestination_Closest(Vector3 start, Vector3 end)
	{
		FilterNonOkayValues(ref start);
		FilterNonOkayValues(ref end, start);
		float maxDistance = 0.15f;
		Vector3 samplePos;
		float sampleRadius;
		float dist;
		int step;
		while (true)
		{
			if (NavMesh.SamplePosition(start, out var startHit, maxDistance, -1))
			{
				start = startHit.position;
				samplePos = end;
				sampleRadius = 1f;
				dist = Vector3.Distance(start, end);
				step = 0;
				break;
			}
			maxDistance += 0.5f;
			if (maxDistance > 10f)
			{
				Debug.LogWarning("GetValidAgentDestination_LinearSweep start position is not a valid point on NavMesh!");
				return start;
			}
		}
		while (true)
		{
			step++;
			if (step >= 10 || sampleRadius > 10f || sampleRadius > dist)
			{
				break;
			}
			if (NavMesh.SamplePosition(samplePos, out var hit, sampleRadius, -1) && NavMesh.CalculatePath(start, hit.position, -1, _path))
			{
				if (_path.status == NavMeshPathStatus.PathComplete)
				{
					Vector3 p = hit.position;
					FilterNonOkayValues(ref p, start);
					return hit.position;
				}
				if (_path.status == NavMeshPathStatus.PathPartial)
				{
					Vector3 p2 = _path.corners[_path.corners.Length - 1];
					FilterNonOkayValues(ref p2, start);
					return p2;
				}
			}
			sampleRadius *= 1.5f;
		}
		return GetValidAgentDestination_LinearSweep(start, end);
	}

	public static NavMeshPathStatus GetNavMeshPathStatus(Vector3 start, Vector3 end)
	{
		FilterNonOkayValues(ref start);
		FilterNonOkayValues(ref end, start);
		start = GetPositionOnGround(start);
		end = GetPositionOnGround(end);
		NavMeshPath path = new NavMeshPath();
		NavMeshQueryFilter filter = new NavMeshQueryFilter
		{
			agentTypeID = 0,
			areaMask = -1
		};
		NavMesh.CalculatePath(start, end, filter, path);
		return path.status;
	}

	public static NavMeshPath GetNavMeshPath(Vector3 start, Vector3 end)
	{
		FilterNonOkayValues(ref start);
		FilterNonOkayValues(ref end, start);
		NavMeshPath path = new NavMeshPath();
		NavMeshQueryFilter filter = new NavMeshQueryFilter
		{
			agentTypeID = 0,
			areaMask = -1
		};
		NavMesh.CalculatePath(start, end, filter, path);
		if (path.status == NavMeshPathStatus.PathInvalid && NavMesh.Raycast(start, end, out var hit, filter))
		{
			NavMesh.CalculatePath(start, hit.position, filter, path);
		}
		return path;
	}

	public static T GetUIComponentBelowCursor<T>(bool includeParent = true) where T : Component
	{
		ListReturnHandle<RaycastResult> handle;
		foreach (RaycastResult a in RaycastAllUIElementsBelowCursor(out handle))
		{
			T comp = (includeParent ? a.gameObject.GetComponentInParent<T>() : a.gameObject.GetComponent<T>());
			if (comp != null)
			{
				handle.Return();
				return comp;
			}
		}
		handle.Return();
		return null;
	}

	public static List<RaycastResult> RaycastAllUIElementsBelowCursor(out ListReturnHandle<RaycastResult> handle)
	{
		List<RaycastResult> list = DewPool.GetList(out handle);
		RaycastAllUIElementsBelowCursor(list);
		return list;
	}

	public static void RaycastAllUIElementsBelowCursor(List<RaycastResult> results)
	{
		PointerEventData pointerData = new PointerEventData(EventSystem.current)
		{
			pointerId = -1,
			position = Input.mousePosition
		};
		results.Clear();
		EventSystem.current.RaycastAll(pointerData, results);
	}

	public static List<RaycastResult> RaycastAllUIElementsBelowScreenPoint(Vector2 screenPoint, out ListReturnHandle<RaycastResult> handle)
	{
		List<RaycastResult> list = DewPool.GetList(out handle);
		RaycastAllUIElementsBelowScreenPoint(screenPoint, list);
		return list;
	}

	public static void RaycastAllUIElementsBelowScreenPoint(Vector2 screenPoint, List<RaycastResult> results)
	{
		PointerEventData pointerData = new PointerEventData(EventSystem.current)
		{
			pointerId = -1,
			position = screenPoint
		};
		results.Clear();
		EventSystem.current.RaycastAll(pointerData, results);
	}

	public static Type GetRequiredAchievementOfTarget(Type targetType)
	{
		foreach (Type t in allAchievements)
		{
			object[] customAttributes = t.GetCustomAttributes(typeof(AchUnlockOnComplete), inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (((AchUnlockOnComplete)customAttributes[i]).targetType == targetType)
				{
					return t;
				}
			}
		}
		return null;
	}

	public static Type GetRequiredAchievementOfTarget(string targetTypeName)
	{
		foreach (Type t in allAchievements)
		{
			object[] customAttributes = t.GetCustomAttributes(typeof(AchUnlockOnComplete), inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (((AchUnlockOnComplete)customAttributes[i]).targetType.Name == targetTypeName)
				{
					return t;
				}
			}
		}
		return null;
	}

	public static List<Type> GetUnlockedTargetsOfAchievement(Type achievementType)
	{
		List<Type> list = new List<Type>();
		object[] customAttributes = achievementType.GetCustomAttributes(typeof(AchUnlockOnComplete), inherit: false);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			AchUnlockOnComplete attr = (AchUnlockOnComplete)customAttributes[i];
			list.Add(attr.targetType);
		}
		return list;
	}

	public static Color GetAchievementColor(Type achievementType)
	{
		List<Type> unlocked = GetUnlockedTargetsOfAchievement(achievementType);
		if (unlocked.Count > 0)
		{
			if (unlocked[0].IsSubclassOf(typeof(Gem)))
			{
				return GetRarityColor(((Gem)DewResources.GetByType(unlocked[0])).rarity);
			}
			if (unlocked[0].IsSubclassOf(typeof(SkillTrigger)))
			{
				return GetRarityColor(((SkillTrigger)DewResources.GetByType(unlocked[0])).rarity);
			}
			if (unlocked[0].IsSubclassOf(typeof(LucidDream)))
			{
				return ((LucidDream)DewResources.GetByType(unlocked[0])).color;
			}
			if (unlocked[0].IsSubclassOf(typeof(Hero)))
			{
				return ((Hero)DewResources.GetByType(unlocked[0])).mainColor.WithV(1f);
			}
		}
		return CommonColor;
	}

	public static string GetAchievementColorHex(Type achievementType)
	{
		return GetHex(GetAchievementColor(achievementType));
	}

	public static Hero SelectRandomAliveHero(bool fallbackToDead = true)
	{
		_aliveHeroesBuffer.Clear();
		foreach (DewPlayer p in DewPlayer.humanPlayers)
		{
			if (!p.hero.IsNullInactiveDeadOrKnockedOut())
			{
				_aliveHeroesBuffer.Add(p.hero);
			}
		}
		if (_aliveHeroesBuffer.Count <= 0)
		{
			if (fallbackToDead && NetworkedManagerBase<ActorManager>.instance.allHeroes.Count > 0)
			{
				return NetworkedManagerBase<ActorManager>.instance.allHeroes[UnityEngine.Random.Range(0, NetworkedManagerBase<ActorManager>.instance.allHeroes.Count)];
			}
			return null;
		}
		return _aliveHeroesBuffer[UnityEngine.Random.Range(0, _aliveHeroesBuffer.Count)];
	}

	public static Hero GetClosestAliveHero(Vector3 pivot, bool fallbackToDead = true)
	{
		_aliveHeroesBuffer.Clear();
		foreach (DewPlayer p in DewPlayer.humanPlayers)
		{
			if (!p.hero.IsNullInactiveDeadOrKnockedOut())
			{
				_aliveHeroesBuffer.Add(p.hero);
			}
		}
		IReadOnlyList<Hero> list = _aliveHeroesBuffer;
		if (_aliveHeroesBuffer.Count <= 0 && fallbackToDead && NetworkedManagerBase<ActorManager>.instance.allHeroes.Count > 0)
		{
			list = NetworkedManagerBase<ActorManager>.instance.allHeroes;
		}
		Hero closest = null;
		float closestSqr = float.PositiveInfinity;
		foreach (Hero h in list)
		{
			float sqr = Vector3.SqrMagnitude(h.position - pivot);
			if (sqr < closestSqr)
			{
				closestSqr = sqr;
				closest = h;
			}
		}
		return closest;
	}

	public static IEnumerator WaitForClientsReadyRoutine()
	{
		if (!NetworkServer.active)
		{
			yield break;
		}
		bool didSetStatus = false;
		float startTime = Time.unscaledTime;
		while (true)
		{
			yield return new WaitForSecondsRealtime(0.15f);
			if (NetworkServer.connections.Values.All((NetworkConnectionToClient c) => c.isReady))
			{
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
				yield break;
			}
			if (Time.unscaledTime - startTime > 35f)
			{
				break;
			}
			if (!didSetStatus)
			{
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForOtherPlayers);
				didSetStatus = true;
			}
		}
		Debug.LogWarning("WARNING: Clients ready check timed out.");
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
	}

	public static IEnumerator WaitForAggroedEnemiesRoutine()
	{
		bool hasEnemiesNearby = false;
		foreach (DewPlayer h in DewPlayer.humanPlayers)
		{
			if (!h.hero.isKnockedOut)
			{
				ArrayReturnHandle<Entity> handle;
				int length = DewPhysics.OverlapCircleAllEntities(out handle, h.hero.agentPosition, 8f, (Entity e) => e.GetRelation(h.hero) == EntityRelation.Enemy).Length;
				handle.Return();
				if (length > 0)
				{
					hasEnemiesNearby = true;
					break;
				}
			}
		}
		if (hasEnemiesNearby)
		{
			yield return new WaitForSeconds(1.5f);
		}
		bool isClear = false;
		while (!isClear)
		{
			isClear = true;
			if (NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
			{
				break;
			}
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if (allEntity is Monster m && !m.IsNullInactiveDeadOrKnockedOut() && m.AI._aiContext.targetEnemy != null)
				{
					isClear = false;
					break;
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	public static T SelectRandomWeightedInList<T>(IList<T> list, Func<T, float> weightGetter)
	{
		if (list.Count <= 0)
		{
			return default(T);
		}
		float sumWeights = 0f;
		ArrayReturnHandle<float> handle;
		float[] chances = DewPool.GetArray(out handle, list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			float weight = weightGetter(list[i]);
			sumWeights += weight;
			chances[i] = weight;
		}
		float currentSum = 0f;
		float value = UnityEngine.Random.Range(0f, sumWeights);
		int selectedIndex = 0;
		for (int j = 0; j < list.Count; j++)
		{
			currentSum += chances[j];
			if (value < currentSum)
			{
				selectedIndex = j;
				break;
			}
		}
		handle.Return();
		return list[selectedIndex];
	}

	public static T SelectRandomWeightedInList<T>(ReadOnlySpan<T> list, Func<T, float> weightGetter)
	{
		if (list.Length <= 0)
		{
			return default(T);
		}
		float sumWeights = 0f;
		ArrayReturnHandle<float> handle;
		float[] chances = DewPool.GetArray(out handle, list.Length);
		for (int i = 0; i < list.Length; i++)
		{
			float weight = weightGetter(list[i]);
			sumWeights += weight;
			chances[i] = weight;
		}
		float currentSum = 0f;
		float value = UnityEngine.Random.Range(0f, sumWeights);
		int selectedIndex = 0;
		for (int j = 0; j < list.Length; j++)
		{
			currentSum += chances[j];
			if (value < currentSum)
			{
				selectedIndex = j;
				break;
			}
		}
		handle.Return();
		return list[selectedIndex];
	}

	public static T SelectRandomWeightedInReadOnlyList<T>(IReadOnlyList<T> list, Func<T, float> weightGetter)
	{
		if (list.Count <= 0)
		{
			return default(T);
		}
		float sumWeights = 0f;
		ArrayReturnHandle<float> handle;
		float[] chances = DewPool.GetArray(out handle, list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			float weight = weightGetter(list[i]);
			sumWeights += weight;
			chances[i] = weight;
		}
		float currentSum = 0f;
		float value = UnityEngine.Random.Range(0f, sumWeights);
		int selectedIndex = 0;
		for (int j = 0; j < list.Count; j++)
		{
			currentSum += chances[j];
			if (value < currentSum)
			{
				selectedIndex = j;
				break;
			}
		}
		handle.Return();
		return list[selectedIndex];
	}

	public static void QuitApplication(bool dontSave = false)
	{
		if (!dontSave)
		{
			try
			{
				DewSave.SaveProfile();
				DewSave.SavePlatformSettings();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		Application.Quit();
	}

	public static string GetCurrentMultiplayerCompatibilityVersion()
	{
		return GetMultiplayerCompatibilityVersion(Application.version);
	}

	public static string GetMultiplayerCompatibilityVersion(string bundleVersion)
	{
		if (bundleVersion.Contains("_"))
		{
			return bundleVersion.Split("_")[0];
		}
		return bundleVersion;
	}

	public static IControlPresetWindow GetControlPresetWindow()
	{
		return FindInterfaceOfType<IControlPresetWindow>(includeInactive: true);
	}

	public static T FindInterfaceOfType<T>(bool includeInactive) where T : class
	{
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive);
		for (int i = 0; i < array.Length; i++)
		{
			T target = array[i].GetComponent<T>();
			if (target != null)
			{
				return target;
			}
		}
		return null;
	}

	public static T[] FindInterfacesOfType<T>(bool includeInactive) where T : class
	{
		List<T> list = new List<T>();
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive);
		for (int i = 0; i < array.Length; i++)
		{
			T[] target = array[i].GetComponents<T>();
			if (target != null)
			{
				list.AddRange(target);
			}
		}
		return list.ToArray();
	}

	public static int SelectBestIndexWithScore<T>(IList<T> list, Func<T, int, float> scoreFunc, float fuzziness = 0f, DewRandomInstance random = null)
	{
		int best = 0;
		float bestScore = float.NegativeInfinity;
		for (int i = 0; i < list.Count; i++)
		{
			T item = list[i];
			float score = scoreFunc(item, i);
			score *= 1f + (random?.Range(0f - fuzziness, fuzziness) ?? UnityEngine.Random.Range(0f - fuzziness, fuzziness));
			if (score > bestScore)
			{
				bestScore = score;
				best = i;
			}
		}
		return best;
	}

	public static int SelectBestIndexWithScore<T>(ReadOnlySpan<T> list, Func<T, int, float> scoreFunc, float fuzziness = 0f, DewRandomInstance random = null)
	{
		int best = -1;
		float bestScore = float.NegativeInfinity;
		for (int i = 0; i < list.Length; i++)
		{
			T item = list[i];
			float score = scoreFunc(item, i);
			score *= 1f + (random?.Range(0f - fuzziness, fuzziness) ?? UnityEngine.Random.Range(0f - fuzziness, fuzziness));
			if (score > bestScore)
			{
				bestScore = score;
				best = i;
			}
		}
		return best;
	}

	public static T SelectBestWithScore<T>(IList<T> list, Func<T, int, float> scoreFunc, float fuzziness = 0f, DewRandomInstance random = null)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		return list[SelectBestIndexWithScore(list, scoreFunc, fuzziness, random)];
	}

	public static T SelectBestWithScore<T>(ReadOnlySpan<T> list, Func<T, int, float> scoreFunc, float fuzziness = 0f, DewRandomInstance random = null)
	{
		if (list.Length == 0)
		{
			return default(T);
		}
		return list[SelectBestIndexWithScore(list, scoreFunc, fuzziness, random)];
	}

	public static T SelectRandomWeightedInParams<T>(params (float, T)[] elements)
	{
		if (elements.Length == 0)
		{
			return default(T);
		}
		float sumWeights = 0f;
		for (int i = 0; i < elements.Length; i++)
		{
			sumWeights += elements[i].Item1;
		}
		float currentSum = 0f;
		float value = UnityEngine.Random.Range(0f, sumWeights);
		int selectedIndex = 0;
		for (int j = 0; j < elements.Length; j++)
		{
			currentSum += elements[j].Item1;
			if (value < currentSum)
			{
				selectedIndex = j;
				break;
			}
		}
		return elements[selectedIndex].Item2;
	}

	public static int SelectRandomWeightedIndexInParams(params float[] weights)
	{
		if (weights.Length == 0)
		{
			return -1;
		}
		float sumWeights = 0f;
		for (int i = 0; i < weights.Length; i++)
		{
			sumWeights += weights[i];
		}
		float currentSum = 0f;
		float value = UnityEngine.Random.Range(0f, sumWeights);
		int selectedIndex = 0;
		for (int j = 0; j < weights.Length; j++)
		{
			currentSum += weights[j];
			if (value < currentSum)
			{
				selectedIndex = j;
				break;
			}
		}
		return selectedIndex;
	}

	public static float GetClosestHeroDistance(Vector3 pivot, bool fallbackToDead = true)
	{
		float dist = float.PositiveInfinity;
		float deadDist = float.PositiveInfinity;
		foreach (DewPlayer h in DewPlayer.humanPlayers)
		{
			if (!(h.hero == null))
			{
				float d = Vector2.Distance(h.hero.agentPosition.ToXY(), pivot.ToXY());
				deadDist = Mathf.Min(deadDist, d);
				if (!h.hero.IsNullInactiveDeadOrKnockedOut())
				{
					dist = Mathf.Min(dist, d);
				}
			}
		}
		if (fallbackToDead && float.IsPositiveInfinity(dist))
		{
			return deadDist;
		}
		return dist;
	}

	public static string GetParentDirectory(string path)
	{
		int lastIndex = path.LastIndexOf("/");
		if (lastIndex <= -1)
		{
			return "";
		}
		return path.Substring(0, lastIndex);
	}

	public static T FindActorOfType<T>() where T : Actor
	{
		if (NetworkedManagerBase<ActorManager>.instance == null)
		{
			return null;
		}
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor is T t)
			{
				return t;
			}
		}
		return null;
	}

	public static List<T> FindAllActorsOfType<T>(out ListReturnHandle<T> handle) where T : Actor
	{
		List<T> list = DewPool.GetList(out handle);
		if (NetworkedManagerBase<ActorManager>.instance == null)
		{
			return list;
		}
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if (allActor is T t)
			{
				list.Add(t);
			}
		}
		return list;
	}

	public static string IntToRoman(int num)
	{
		string romanResult = string.Empty;
		string[] romanLetters = new string[13]
		{
			"M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX",
			"V", "IV", "I"
		};
		int[] numbers = new int[13]
		{
			1000, 900, 500, 400, 100, 90, 50, 40, 10, 9,
			5, 4, 1
		};
		int i = 0;
		while (num != 0)
		{
			if (num >= numbers[i])
			{
				num -= numbers[i];
				romanResult += romanLetters[i];
			}
			else
			{
				i++;
			}
		}
		return romanResult;
	}

	public static Type GetTypeFromShortName(string typeName)
	{
		if (DewResources.database.typeNameToType.TryGetValue(typeName, out var t))
		{
			return t;
		}
		Type type = Type.GetType(typeName);
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType(typeName);
			if (type != null)
			{
				return type;
			}
		}
		return null;
	}

	public static bool TryGetTypeFromShortName(string typeName, out Type type)
	{
		type = GetTypeFromShortName(typeName);
		return type != null;
	}

	public static int GetAliveHeroCount()
	{
		int total = 0;
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			if (!humanPlayer.hero.IsNullInactiveDeadOrKnockedOut())
			{
				total++;
			}
		}
		return total;
	}

	public static void RefreshRenderer()
	{
		ManagerBase<ShaderManager>.instance.StartCoroutine(Routine());
		static IEnumerator Routine()
		{
			QualitySettings.SetQualityLevel(0, applyExpensiveChanges: true);
			yield return null;
			QualitySettings.SetQualityLevel(3, applyExpensiveChanges: true);
			yield return null;
		}
	}

	public static string ConvertToStringInvariantCulture(object value)
	{
		if (value is float f)
		{
			return f.ToString(CultureInfo.InvariantCulture);
		}
		if (value is double d)
		{
			return d.ToString(CultureInfo.InvariantCulture);
		}
		if (value is int integer)
		{
			return integer.ToString(CultureInfo.InvariantCulture);
		}
		return value.ToString();
	}

	public static void CallOnReady(MonoBehaviour caller, Func<bool> condition, Action func)
	{
		ManagerBase<GlobalLogicPackage>.instance.StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (true)
			{
				if (caller == null)
				{
					yield break;
				}
				if (condition())
				{
					break;
				}
				yield return null;
			}
			func();
		}
	}

	public static void CallDelayed(Action func, int frameCount = 1)
	{
		ManagerBase<GlobalLogicPackage>.instance.StartCoroutine(Routine());
		IEnumerator Routine()
		{
			for (int i = 0; i < frameCount; i++)
			{
				yield return null;
			}
			func();
		}
	}

	public static void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	public static string GetProjectRootPath()
	{
		return Directory.GetParent(Application.dataPath).ToString();
	}

	public static bool IsMeleeHero(Hero.HeroClassType type)
	{
		if (type != Hero.HeroClassType.MeleeAttacker && type != Hero.HeroClassType.MeleeMage && type != Hero.HeroClassType.MeleeTank && type != Hero.HeroClassType.MeleeSupport)
		{
			return type == Hero.HeroClassType.MeleeSummoner;
		}
		return true;
	}

	public static bool IsRangedHero(Hero.HeroClassType type)
	{
		return !IsMeleeHero(type);
	}

	public static string FormatBigNumbers(float number, float threshold, string format)
	{
		if (number < 1000f)
		{
			return number.ToString(format);
		}
		int suffixIndex = 0;
		while (number >= threshold && suffixIndex < BigNumberSuffixes.Length - 1)
		{
			number /= 1000f;
			suffixIndex++;
		}
		return number.ToString(format) + BigNumberSuffixes[suffixIndex];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(float f)
	{
		if (float.IsNaN(f) || float.IsInfinity(f))
		{
			Debug.LogException(new InvalidOperationException("Non-okay floating value found"));
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(double d)
	{
		if (double.IsNaN(d) || double.IsInfinity(d))
		{
			Debug.LogException(new InvalidOperationException("Non-okay floating value found"));
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(Vector2 v)
	{
		if (IsOkay(v.x))
		{
			return IsOkay(v.y);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(Vector3 v)
	{
		if (IsOkay(v.x) && IsOkay(v.y))
		{
			return IsOkay(v.z);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(Vector4 v)
	{
		if (IsOkay(v.x) && IsOkay(v.y) && IsOkay(v.z))
		{
			return IsOkay(v.w);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsOkay(Displacement d)
	{
		if (d is DispByDestination dd)
		{
			if (IsOkay(dd.duration) && IsOkay(dd.destination))
			{
				return dd.duration > 0f;
			}
			return false;
		}
		if (d is DispByTarget dt)
		{
			if (IsOkay(dt.goalDistance))
			{
				return IsOkay(dt.speed);
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FilterNonOkayValues(ref Vector3 v, Vector3 fallback = default(Vector3))
	{
		FilterNonOkayValues(ref v.x, fallback.x);
		FilterNonOkayValues(ref v.y, fallback.y);
		FilterNonOkayValues(ref v.z, fallback.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FilterNonOkayValues(ref float v, float fallback = 0f)
	{
		if (!IsOkay(v))
		{
			v = (IsOkay(fallback) ? fallback : 0f);
		}
	}

	public static bool IsNearChristmas()
	{
		DateTime today = DateTime.Today;
		DateTime christmas = new DateTime(today.Year, 12, 25);
		if (today.Month == 1 && today.Day <= 7)
		{
			christmas = new DateTime(today.Year - 1, 12, 25);
		}
		else if (today.Month == 12 && today.Day > 25)
		{
			christmas = new DateTime(today.Year + 1, 12, 25);
		}
		return Math.Abs((christmas - today).Days) <= 7;
	}

	public static string GetReadableTimespan(TimeSpan t)
	{
		if (t.Days > 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Days_Plural"), t.Days);
		}
		if (t.Days == 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Day_Singular"), t.Days);
		}
		if (t.Hours > 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Hours_Plural"), t.Hours);
		}
		if (t.Hours == 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Hour_Singular"), t.Hours);
		}
		if (t.Minutes > 1)
		{
			return string.Format(DewLocalization.GetUIValue("Timespan_Minutes_Plural"), t.Minutes);
		}
		return string.Format(DewLocalization.GetUIValue("Timespan_Minute_Singular"), 1);
	}

	public static long GetRequiredMasteryPointsToLevelUp(int currentLevel)
	{
		return RequiredMasteryPointsToLevelUp.GetClamped(currentLevel);
	}

	public static int GetRequiredMasteryLevelForStarSlotUnlock(int index)
	{
		return 5 + index * 10;
	}

	public static int GetRequiredStardustForStarSlotUnlock(int index)
	{
		return 20 + index * 10;
	}

	public static int GetDejavuMaxWins(UnityEngine.Object target)
	{
		Rarity rarity = Rarity.Rare;
		if (target is SkillTrigger st)
		{
			rarity = st.rarity;
		}
		else if (target is Gem g)
		{
			rarity = g.rarity;
		}
		return rarity switch
		{
			Rarity.Common => 6, 
			Rarity.Rare => 5, 
			Rarity.Epic => 4, 
			Rarity.Legendary => 3, 
			_ => 3, 
		};
	}

	public static bool IsDejavuFree(UnityEngine.Object target)
	{
		return IsDejavuFree(DewSave.profile.itemStatistics[target.GetType().Name]);
	}

	public static bool IsDejavuFree(DewProfile.ItemStatistics data)
	{
		long now = DateTime.UtcNow.ToTimestamp();
		return data.dejavuCostReductionPeriodTimestamp >= now;
	}

	public static int GetDejavuCost(UnityEngine.Object target, int wins = -1)
	{
		Rarity rarity = Rarity.Rare;
		if (target is SkillTrigger st)
		{
			rarity = st.rarity;
		}
		else if (target is Gem g)
		{
			rarity = g.rarity;
		}
		if (!DewSave.profile.itemStatistics.TryGetValue(target.GetType().Name, out var data))
		{
			return int.MaxValue;
		}
		if (wins == -1)
		{
			wins = data.wins;
		}
		return rarity switch
		{
			Rarity.Common => DejavuCostCommon.GetClamped(wins - 1), 
			Rarity.Rare => DejavuCostRare.GetClamped(wins - 1), 
			Rarity.Epic => DejavuCostEpic.GetClamped(wins - 1), 
			Rarity.Legendary => DejavuCostLegendary.GetClamped(wins - 1), 
			_ => int.MaxValue, 
		};
	}

	public static bool IsExcludedFromPool(string type)
	{
		return DewResources.database.excludedFromPoolObjects.Contains(type);
	}

	public static bool IsHeroIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.availableHeroes != null && DewBuildProfile.current.content.availableHeroes.Count > 0 && !DewBuildProfile.current.content.availableHeroes.Contains(type))
		{
			return false;
		}
		return !IsExcludedFromPool(type);
	}

	public static bool IsSkillIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.availableSkills != null && DewBuildProfile.current.content.availableSkills.Count > 0 && !DewBuildProfile.current.content.availableSkills.Contains(type))
		{
			return false;
		}
		return !IsExcludedFromPool(type);
	}

	public static bool IsGemIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.availableGems != null && DewBuildProfile.current.content.availableGems.Count > 0 && !DewBuildProfile.current.content.availableGems.Contains(type))
		{
			return false;
		}
		return !IsExcludedFromPool(type);
	}

	public static bool IsArtifactIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.availableArtifacts != null && DewBuildProfile.current.content.availableArtifacts.Count > 0 && !DewBuildProfile.current.content.availableArtifacts.Contains(type))
		{
			return false;
		}
		return !IsExcludedFromPool(type);
	}

	public static bool IsAchievementIncludedInGame(string typeStr)
	{
		if (!achievementsByName.TryGetValue(typeStr, out var type))
		{
			return false;
		}
		List<Type> unlockedTargets = GetUnlockedTargetsOfAchievement(type);
		if (unlockedTargets.Count == 0)
		{
			return true;
		}
		foreach (Type t in unlockedTargets)
		{
			if (typeof(SkillTrigger).IsAssignableFrom(t) && IsSkillIncludedInGame(t.Name))
			{
				return true;
			}
			if (typeof(Gem).IsAssignableFrom(t) && IsGemIncludedInGame(t.Name))
			{
				return true;
			}
			if (typeof(Hero).IsAssignableFrom(t) && IsHeroIncludedInGame(t.Name))
			{
				return true;
			}
			if (typeof(LucidDream).IsAssignableFrom(t) && IsLucidDreamIncludedInGame(t.Name))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsStarIncludedInGame(string type)
	{
		if (!oldStarsByName.TryGetValue(type, out var item))
		{
			return false;
		}
		if (item is DewHeroStarItemOld hs && !IsHeroIncludedInGame(hs.heroType.Name))
		{
			return false;
		}
		if (DewBuildProfile.current.content.lockedStars != null && DewBuildProfile.current.content.lockedStars.Count != 0)
		{
			return !DewBuildProfile.current.content.lockedStars.Contains(type);
		}
		return true;
	}

	public static bool IsGameModifierIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.availableGameModifiers != null && DewBuildProfile.current.content.availableGameModifiers.Count > 0 && !DewBuildProfile.current.content.availableGameModifiers.Contains(type))
		{
			return false;
		}
		return !IsExcludedFromPool(type);
	}

	public static bool IsCurseIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.availableCurses != null && DewBuildProfile.current.content.availableCurses.Count > 0 && !DewBuildProfile.current.content.availableCurses.Contains(type))
		{
			return false;
		}
		return true;
	}

	public static bool IsRoomModifierIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.availableRoomModifiers != null && DewBuildProfile.current.content.availableRoomModifiers.Count > 0 && !DewBuildProfile.current.content.availableRoomModifiers.Contains(type))
		{
			return false;
		}
		return !IsExcludedFromPool(type);
	}

	public static bool IsLucidDreamIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.availableLucidDreams != null && DewBuildProfile.current.content.availableLucidDreams.Count > 0 && !DewBuildProfile.current.content.availableLucidDreams.Contains(type))
		{
			return false;
		}
		return !IsExcludedFromPool(type);
	}

	public static bool IsMonsterIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.excludedMonsters != null && DewBuildProfile.current.content.excludedMonsters.Count != 0)
		{
			return !DewBuildProfile.current.content.excludedMonsters.Contains(type);
		}
		return true;
	}

	public static bool IsTreasureIncludedInGame(string type)
	{
		if (DewBuildProfile.current.content.availableTreasures != null && DewBuildProfile.current.content.availableTreasures.Count > 0 && !DewBuildProfile.current.content.availableTreasures.Contains(type))
		{
			return false;
		}
		return !IsExcludedFromPool(type);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_destroyingGameObject = new List<GameObject>();
	}

	public static void Destroy(GameObject gameObject)
	{
		if (NetworkedManagerBase<GameManager>.instance == null || !NetworkedManagerBase<GameManager>.instance.isActiveAndEnabled)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		else if (!NetworkServer.active)
		{
			Debug.LogError($"Cannot call destroy on clients: {gameObject}");
		}
		else
		{
			NetworkedManagerBase<GameManager>.instance.StartCoroutine(DestroyRoutine(gameObject));
		}
	}

	public static bool IsAnyActorBeingDestroyed()
	{
		return _destroyingGameObject.Count > 0;
	}

	public static bool IsBeingDestroyed(GameObject gameObject)
	{
		return _destroyingGameObject.Contains(gameObject);
	}

	private static IEnumerator DestroyRoutine(GameObject gameObject)
	{
		while (!CanContinueDestroy())
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		if (gameObject == null)
		{
			throw new ArgumentNullException("gameObject");
		}
		if (_destroyingGameObject.Contains(gameObject))
		{
			Debug.LogWarning($"Tried to destroy a gameobject twice: {gameObject}");
			yield break;
		}
		gameObject.transform.parent = null;
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		_destroyingGameObject.Add(gameObject);
		ListReturnHandle<ICleanup> handle;
		List<ICleanup> cleanups = gameObject.GetComponentsNonAlloc(out handle);
		if (cleanups.Count > 0)
		{
			foreach (ICleanup cleanup in cleanups)
			{
				try
				{
					cleanup.OnCleanup();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, cleanup as UnityEngine.Object);
				}
			}
			yield return new WaitForSecondsRealtime(0.35f);
			while (!CanContinueDestroy())
			{
				yield return new WaitForSecondsRealtime(0.1f);
			}
			while (true)
			{
				bool canDestroy = true;
				foreach (ICleanup cleanup2 in cleanups)
				{
					try
					{
						if (!cleanup2.canDestroy)
						{
							canDestroy = false;
							break;
						}
					}
					catch (Exception exception2)
					{
						canDestroy = false;
						Debug.LogException(exception2, cleanup2 as UnityEngine.Object);
						break;
					}
				}
				if (canDestroy)
				{
					break;
				}
				yield return new WaitForSecondsRealtime(0.05f);
				while (!CanContinueDestroy())
				{
					yield return new WaitForSecondsRealtime(0.1f);
				}
			}
		}
		else
		{
			yield return new WaitForSecondsRealtime(0.35f);
			while (!CanContinueDestroy())
			{
				yield return new WaitForSecondsRealtime(0.1f);
			}
		}
		handle.Return();
		ListReturnHandle<ICustomDestroyRoutine> handle2;
		List<ICustomDestroyRoutine> customs = gameObject.GetComponentsNonAlloc(out handle2);
		if (customs.Count > 0)
		{
			NetworkServer.UnSpawn(gameObject);
			foreach (ICustomDestroyRoutine cs in customs)
			{
				try
				{
					cs.CustomDestroyRoutine();
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3, gameObject);
				}
			}
		}
		else
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		handle2.Return();
		_destroyingGameObject.Remove(gameObject);
	}

	private static bool CanContinueDestroy()
	{
		if (!NetworkClient.ready || NetworkServer.isLoadingScene)
		{
			return false;
		}
		if (!NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
		{
			return true;
		}
		foreach (KeyValuePair<int, NetworkConnectionToClient> connection in NetworkServer.connections)
		{
			if (!connection.Value.isReady)
			{
				return false;
			}
		}
		return true;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void InitAllTypeReferences()
	{
		try
		{
			_allHeroes = new List<Type>();
			_allSkills = new List<Type>();
			_allHeroSkills = new List<Type>();
			_allGems = new List<Type>();
			_allArtifacts = new List<Type>();
			_allGameModifiers = new List<Type>();
			_allAchievements = new List<Type>();
			_achievementsByName = new Dictionary<string, Type>();
			_allTutorialItems = new List<Type>();
			_allOldStars = new List<DewStarItemOld>();
			_allStarTypes = new List<Type>();
			_oldStarsByName = new Dictionary<string, DewStarItemOld>();
			_oldStarsByHeroType = new Dictionary<Type, List<DewStarItemOld>>();
			_heroTypeOfOldStar = new Dictionary<DewStarItemOld, Type>();
			_allRoomModifiers = new List<Type>();
			_allLucidDreams = new List<Type>();
			_allReveries = new List<DewReverieItem>();
			_reveriesByName = new Dictionary<string, DewReverieItem>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type[] types = assemblies[i].GetTypes();
				foreach (Type t in types)
				{
					if (t.IsSubclassOf(typeof(StarEffect)))
					{
						_allStarTypes.Add(t);
					}
					if (t.IsSubclassOf(typeof(Hero)))
					{
						_allHeroes.Add(t);
					}
					if (t.IsSubclassOf(typeof(SkillTrigger)))
					{
						_allSkills.Add(t);
					}
					if (t.IsSubclassOf(typeof(Gem)))
					{
						_allGems.Add(t);
					}
					if (t.IsSubclassOf(typeof(Artifact)))
					{
						_allArtifacts.Add(t);
					}
					if (t.IsSubclassOf(typeof(GameModifierBase)))
					{
						_allGameModifiers.Add(t);
					}
					if (t.IsSubclassOf(typeof(RoomModifierBase)))
					{
						_allRoomModifiers.Add(t);
					}
					if (t.IsSubclassOf(typeof(LucidDream)))
					{
						_allLucidDreams.Add(t);
					}
					if (t.IsSubclassOf(typeof(DewReverieItem)) && !t.IsAbstract)
					{
						DewReverieItem rev = (DewReverieItem)Activator.CreateInstance(t);
						_allReveries.Add(rev);
						_reveriesByName.Add(t.Name, rev);
					}
					if (t.IsSubclassOf(typeof(DewAchievementItem)))
					{
						_allAchievements.Add(t);
						_achievementsByName.Add(t.Name, t);
					}
					if (t.IsSubclassOf(typeof(DewInGameTutorialItem)))
					{
						_allTutorialItems.Add(t);
					}
					if (!t.IsSubclassOf(typeof(DewStarItemOld)) || t.IsAbstract)
					{
						continue;
					}
					DewStarItemOld star = (DewStarItemOld)Activator.CreateInstance(t);
					_allOldStars.Add(star);
					_oldStarsByName.Add(t.Name, star);
					if (t.IsSubclassOf(typeof(DewHeroStarItemOld)))
					{
						Type hero = ((DewHeroStarItemOld)star).heroType;
						if (hero != null)
						{
							if (!_oldStarsByHeroType.ContainsKey(hero))
							{
								_oldStarsByHeroType.Add(hero, new List<DewStarItemOld>());
							}
							_oldStarsByHeroType[hero].Add(star);
							_heroTypeOfOldStar.Add(star, hero);
						}
					}
					else
					{
						_heroTypeOfOldStar.Add(star, null);
					}
				}
			}
			foreach (Type h in _allHeroes)
			{
				if (IsHeroIncludedInGame(h.Name))
				{
					HeroSkill skill = DewResources.GetByType<Hero>(h).GetComponent<HeroSkill>();
					SkillTrigger[] loadoutSkills = skill.GetLoadoutSkills(HeroSkillLocation.Q);
					foreach (SkillTrigger s in loadoutSkills)
					{
						_allHeroSkills.Add(s.GetType());
					}
					loadoutSkills = skill.GetLoadoutSkills(HeroSkillLocation.R);
					foreach (SkillTrigger s2 in loadoutSkills)
					{
						_allHeroSkills.Add(s2.GetType());
					}
					loadoutSkills = skill.GetLoadoutSkills(HeroSkillLocation.Identity);
					foreach (SkillTrigger s3 in loadoutSkills)
					{
						_allHeroSkills.Add(s3.GetType());
					}
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			GlobalLogicPackage.CallOnReady(delegate
			{
				DewSessionError.ShowError(new DewException(DewExceptionType.CorruptGameFiles), isFatal: true);
			});
		}
	}

	public static T SpawnHero<T>(Vector3 position, Quaternion? rotation, DewPlayer owner, int level, HeroLoadoutData loadout, Action<T> beforeSpawn = null) where T : Hero
	{
		return SpawnHero(DewResources.GetByType<T>(), position, rotation, owner, level, loadout, delegate(T h)
		{
			beforeSpawn?.Invoke(h);
		});
	}

	public static T SpawnHero<T>(T hero, Vector3 position, Quaternion? rotation, DewPlayer owner, int level, HeroLoadoutData loadout, Action<T> beforeSpawn = null) where T : Hero
	{
		if (owner == null)
		{
			throw new Exception("Provided owner player is null!");
		}
		T val = InstantiateActor_Imp(hero, position, rotation, null);
		val.Status.level = level;
		val.owner = owner;
		val.loadout = loadout;
		return PrepareAndSpawnActor_Imp(val, beforeSpawn);
	}

	public static T SpawnEntity<T>(Vector3 position, Quaternion? rotation, Actor parent, DewPlayer owner, int level, Action<T> beforeSpawn = null) where T : Entity
	{
		return SpawnEntity(DewResources.GetByType<T>(), position, rotation, parent, owner, level, beforeSpawn);
	}

	public static T SpawnEntity<T>(T entity, Vector3 position, Quaternion? rotation, Actor parent, DewPlayer owner, int level, Action<T> beforeSpawn = null) where T : Entity
	{
		if (owner == null)
		{
			throw new Exception("Provided owner player is null!");
		}
		T val = InstantiateActor_Imp(entity, position, rotation, null);
		val.Status.level = level;
		val.parentActor = parent;
		val.owner = owner;
		return PrepareAndSpawnActor_Imp(val, beforeSpawn);
	}

	public static T CreateSkillTrigger<T>(Vector3 position, int level, DewPlayer tempOwner = null, Action<T> beforePrepare = null) where T : SkillTrigger
	{
		return CreateSkillTrigger(DewResources.GetByType<T>(), position, level, tempOwner, beforePrepare);
	}

	public static T CreateSkillTrigger<T>(T trigger, Vector3 position, int level, DewPlayer tempOwner = null, Action<T> beforePrepare = null) where T : SkillTrigger
	{
		T val = InstantiateActor_Imp(trigger, position, null, null);
		val.level = level;
		val.tempOwner = tempOwner;
		return PrepareAndSpawnActor_Imp(val, beforePrepare);
	}

	public static T CreateAbilityTrigger<T>(Action<T> beforeSpawn = null) where T : AbilityTrigger
	{
		return CreateAbilityTrigger(DewResources.GetByType<T>(), beforeSpawn);
	}

	public static T CreateAbilityTrigger<T>(T trigger, Action<T> beforePrepare = null) where T : AbilityTrigger
	{
		return CreateActor(trigger, Vector3.zero, null, null, beforePrepare);
	}

	public static T CreateGem<T>(Vector3 position, int quality, DewPlayer tempOwner = null, Action<T> beforePrepare = null) where T : Gem
	{
		return CreateGem(DewResources.GetByType<T>(), position, quality, tempOwner, beforePrepare);
	}

	public static T CreateGem<T>(T gem, Vector3 position, int quality, DewPlayer tempOwner = null, Action<T> beforePrepare = null) where T : Gem
	{
		T val = InstantiateActor_Imp(gem, position, null, null);
		val.quality = quality;
		val.tempOwner = tempOwner;
		return PrepareAndSpawnActor_Imp(val, beforePrepare);
	}

	public static T CreateStatusEffect<T>(Entity victim, Actor parent, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		return CreateStatusEffect(DewResources.GetByType<T>(), victim, parent, info, beforePrepare);
	}

	public static T CreateStatusEffect<T>(T prefab, Entity victim, Actor parent, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (victim == null)
		{
			Debug.LogWarning("Tried to create " + typeof(T).Name + " with null victim");
			return null;
		}
		if (prefab.isKilledByCrowdControlImmunity && (victim.Status.hasUnstoppable || victim.Status.hasInvulnerable))
		{
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnIgnoreCC(victim);
			return null;
		}
		T newSe = InstantiateActor_Imp(prefab, victim.position, null, parent);
		newSe.parentActor = parent;
		newSe.info = info;
		newSe.victim = victim;
		if (parent != null)
		{
			try
			{
				parent.ActorEvent_OnAbilityInstanceBeforePrepare?.Invoke(new EventInfoAbilityInstance
				{
					actor = parent,
					instance = newSe
				});
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		PrepareAndSpawnActor_Imp(newSe, beforePrepare);
		victim.Status.AddStatusEffect(newSe);
		if (parent != null)
		{
			try
			{
				parent.ActorEvent_OnAbilityInstanceCreated?.Invoke(new EventInfoAbilityInstance
				{
					actor = parent,
					instance = newSe
				});
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		return newSe;
	}

	public static T CreateAbilityInstance<T>(Vector3 position, Quaternion? rotation, Actor parent, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		return CreateAbilityInstance(DewResources.GetByType<T>(), position, rotation, parent, info, beforePrepare);
	}

	public static T CreateAbilityInstance<T>(T prefab, Vector3 position, Quaternion? rotation, Actor parent, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		if (prefab is StatusEffect)
		{
			throw new Exception("Use CreateStatusEffect for creating status effects");
		}
		T newAi = InstantiateActor_Imp(prefab, position, rotation, parent);
		newAi.parentActor = parent;
		newAi.info = info;
		if (parent != null)
		{
			try
			{
				parent.ActorEvent_OnAbilityInstanceBeforePrepare?.Invoke(new EventInfoAbilityInstance
				{
					actor = parent,
					instance = newAi
				});
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		PrepareAndSpawnActor_Imp(newAi, beforePrepare);
		if (parent != null)
		{
			try
			{
				parent.ActorEvent_OnAbilityInstanceCreated?.Invoke(new EventInfoAbilityInstance
				{
					actor = parent,
					instance = newAi
				});
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		return newAi;
	}

	public static T CreateActor<T>(Action<T> beforePrepare = null) where T : Actor
	{
		return CreateActor(Vector3.zero, null, null, beforePrepare);
	}

	public static T CreateActor<T>() where T : Actor
	{
		return CreateActor<T>(Vector3.zero, null);
	}

	public static T CreateActor<T>(Vector3 position, Quaternion? rotation, Actor parent = null, Action<T> beforePrepare = null) where T : Actor
	{
		return CreateActor(DewResources.GetByType<T>(), position, rotation, parent, beforePrepare);
	}

	public static T CreateActor<T>(T prefab, Vector3 position, Quaternion? rotation, Actor parent = null, Action<T> beforePrepare = null) where T : Actor
	{
		return PrepareAndSpawnActor_Imp(InstantiateActor_Imp(prefab, position, rotation, parent), beforePrepare);
	}

	private static T InstantiateActor_Imp<T>(T prefab, Vector3 position, Quaternion? rotation, Actor parent) where T : Actor
	{
		if (prefab == null)
		{
			throw new Exception("Actor you're trying to instantiate is null");
		}
		if (!NetworkServer.active)
		{
			throw new Exception("You can only create actors on server");
		}
		if (!rotation.HasValue)
		{
			rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		}
		T val = UnityEngine.Object.Instantiate(prefab, position, rotation.Value, null);
		val.parentActor = parent;
		return val;
	}

	private static T PrepareAndSpawnActor_Imp<T>(T newActor, Action<T> beforePrepare) where T : Actor
	{
		try
		{
			beforePrepare?.Invoke(newActor);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		newActor.PrepareAndSpawn();
		return newActor;
	}

	public static T InstantiateAndSpawn<T>(T prefab, Action<T> beforeSpawn = null) where T : Component
	{
		return InstantiateAndSpawn(prefab, Vector3.zero, null, beforeSpawn);
	}

	public static T InstantiateAndSpawn<T>(Vector3 position, Quaternion? rotation, Action<T> beforeSpawn = null) where T : Component
	{
		return InstantiateAndSpawn(DewResources.GetByType<T>(), position, rotation, beforeSpawn);
	}

	public static T InstantiateAndSpawn<T>(Action<T> beforeSpawn = null) where T : Component
	{
		return InstantiateAndSpawn(DewResources.GetByType<T>(), beforeSpawn);
	}

	public static T InstantiateAndSpawn<T>(T prefab, Vector3 position, Quaternion? rotation, Action<T> beforeSpawn = null) where T : Component
	{
		if (!NetworkServer.active)
		{
			throw new Exception("You can only instantiate and spawn on server.");
		}
		Type type = prefab.GetType();
		if (typeof(Hero).IsAssignableFrom(type))
		{
			throw new Exception($"Use SpawnHero for spawning heroes: {type}");
		}
		if (typeof(Entity).IsAssignableFrom(type))
		{
			throw new Exception($"Use SpawnEntity for spawning entities: {type}");
		}
		if (typeof(SkillTrigger).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateSkillTrigger for creating skill triggers: {type}");
		}
		if (typeof(AbilityTrigger).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateAbilityTrigger for creating ability triggers: {type}");
		}
		if (typeof(StatusEffect).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateStatusEffect for creating status effects: {type}");
		}
		if (typeof(AbilityInstance).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateAbilityInstance for creating ability instances: {type}");
		}
		if (typeof(Gem).IsAssignableFrom(type))
		{
			throw new Exception($"Use CreateGem for creating gems: {type}");
		}
		if (!rotation.HasValue)
		{
			rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		}
		T newT = UnityEngine.Object.Instantiate(prefab, position, rotation.Value, null);
		newT.name = prefab.name;
		beforeSpawn?.Invoke(newT);
		NetworkServer.Spawn(newT.gameObject);
		return newT;
	}
}
