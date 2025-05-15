using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class LootManager : NetworkedManagerBase<LootManager>
{
	private const float MinIntervalBetweenLoot = 0.4f;

	private const float MaxIntervalBetweenLoot = 0.8f;

	private HashSet<string> _poolGems = new HashSet<string>();

	private HashSet<string> _poolSkills = new HashSet<string>();

	private Dictionary<Rarity, HashSet<string>> _poolGemsByRarity = new Dictionary<Rarity, HashSet<string>>();

	private Dictionary<Rarity, HashSet<string>> _poolSkillsByRarity = new Dictionary<Rarity, HashSet<string>>();

	private Dictionary<Type, Loot> _lootInstances = new Dictionary<Type, Loot>();

	public IReadOnlyCollection<string> poolGems => _poolGems;

	public IReadOnlyCollection<string> poolSkills => _poolSkills;

	public IReadOnlyDictionary<Rarity, HashSet<string>> poolGemsByRarity => _poolGemsByRarity;

	public IReadOnlyDictionary<Rarity, HashSet<string>> poolSkillsByRarity => _poolSkillsByRarity;

	public IReadOnlyDictionary<Type, Loot> lootInstances => _lootInstances;

	public override void OnStartServer()
	{
		base.OnStartServer();
		foreach (Rarity val in Enum.GetValues(typeof(Rarity)))
		{
			_poolGemsByRarity.Add(val, new HashSet<string>());
			_poolSkillsByRarity.Add(val, new HashSet<string>());
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += new Action<EventInfoLoadRoom>(OnRoomLoadStarted);
	}

	private void OnRoomLoadStarted(EventInfoLoadRoom _)
	{
		StopAllCoroutines();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		List<string> unlockedItems = new List<string>();
		foreach (KeyValuePair<string, DewProfile.UnlockData> g in DewSave.profile.gems)
		{
			if (g.Value.isAvailableInGame && Dew.IsGemIncludedInGame(g.Key))
			{
				unlockedItems.Add(g.Key);
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> s in DewSave.profile.skills)
		{
			if (s.Value.isAvailableInGame && Dew.IsSkillIncludedInGame(s.Key))
			{
				SkillTrigger skill = DewResources.GetByShortTypeName<SkillTrigger>(s.Key);
				if (!(skill == null) && !skill.isCharacterSkill)
				{
					unlockedItems.Add(s.Key);
				}
			}
		}
		AddToPool(unlockedItems.ToArray());
	}

	[Command(requiresAuthority = false)]
	private void AddToPool(string[] list)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002EString_005B_005D(writer, list);
		SendCommandInternal("System.Void LootManager::AddToPool(System.String[])", -1233740984, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public T GetLootInstance<T>() where T : Loot
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T LootManager::GetLootInstance()' called when server was not active");
			return null;
		}
		if (_lootInstances.TryGetValue(typeof(T), out var ins))
		{
			return (T)ins;
		}
		T newLoot = UnityEngine.Object.Instantiate(DewResources.GetByType<T>(), base.transform);
		_lootInstances.Add(typeof(T), newLoot);
		return newLoot;
	}

	[Server]
	public void AddLootChance(Loot loot, float chance)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void LootManager::AddLootChance(Loot,System.Single)' called when server was not active");
			return;
		}
		chance *= loot.addedChanceMultiplier;
		if (_lootInstances.TryGetValue(loot.GetType(), out var ins))
		{
			ins.currentChance += chance;
			return;
		}
		Loot newLoot = UnityEngine.Object.Instantiate(loot, base.transform);
		_lootInstances.Add(loot.GetType(), newLoot);
		newLoot.currentChance = UnityEngine.Random.Range(newLoot.startChanceMin, newLoot.startChanceMax) + chance;
	}

	[Server]
	public void SpawnCurrentLoot(ClearSectionEventData data, bool allowMultipleReward)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void LootManager::SpawnCurrentLoot(ClearSectionEventData,System.Boolean)' called when server was not active");
		}
		else
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (!allowMultipleReward)
			{
				Loot biggest = null;
				float biggestValue = 0f;
				foreach (KeyValuePair<Type, Loot> pair in _lootInstances)
				{
					if (!(pair.Value.currentChance < 1f) && !(biggestValue >= pair.Value.currentChance))
					{
						biggest = pair.Value;
						biggestValue = pair.Value.currentChance;
					}
				}
				if (!(biggest == null))
				{
					biggest.currentChance -= 1f;
					yield return biggest.OnLootRoutine(data);
				}
			}
			else
			{
				List<Loot> list = new List<Loot>(_lootInstances.Values);
				list.Shuffle();
				foreach (Loot loot in list)
				{
					while (loot.currentChance >= 1f)
					{
						loot.currentChance -= 1f;
						yield return loot.OnLootRoutine(data);
						yield return new WaitForSeconds(UnityEngine.Random.Range(0.4f, 0.8f));
					}
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_AddToPool__String_005B_005D(string[] list)
	{
		for (int i = 0; i < list.Length; i++)
		{
			if (_poolSkills.Contains(list[i]) || _poolGems.Contains(list[i]))
			{
				continue;
			}
			UnityEngine.Object obj = DewResources.GetByShortTypeName(list[i]);
			if (!(obj == null))
			{
				if (obj is SkillTrigger st && Dew.IsSkillIncludedInGame(st.GetType().Name) && !st.isCharacterSkill)
				{
					_poolSkills.Add(list[i]);
					_poolSkillsByRarity[st.rarity].Add(list[i]);
				}
				if (obj is Gem g && Dew.IsGemIncludedInGame(g.GetType().Name))
				{
					_poolGems.Add(list[i]);
					_poolGemsByRarity[g.rarity].Add(list[i]);
				}
			}
		}
		if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Loading)
		{
			DewResources.UnloadUnused();
		}
	}

	protected static void InvokeUserCode_AddToPool__String_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command AddToPool called on client.");
		}
		else
		{
			((LootManager)obj).UserCode_AddToPool__String_005B_005D(GeneratedNetworkCode._Read_System_002EString_005B_005D(reader));
		}
	}

	static LootManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(LootManager), "System.Void LootManager::AddToPool(System.String[])", InvokeUserCode_AddToPool__String_005B_005D, requiresAuthority: false);
	}
}
