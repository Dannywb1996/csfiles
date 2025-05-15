using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameAnalyticsManager : ManagerBase<InGameAnalyticsManager>
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct DisableAnalyticsMessage : NetworkMessage
	{
	}

	private class Ad_KillTimeTracker
	{
		public float aliveDuration;

		public float aliveDurationExcludeDamageImmunity;
	}

	private struct CurrentRoomTrackInfo
	{
		public Coroutine _tracker;

		public int clearedSections;

		public float combatDps;

		public float combatDpsWithoutDiscarded;

		public float timeTakenInCombat;

		public float timeTaken;

		public double totalDamageDealt;

		public double totalDamageDealtWithoutDiscarded;
	}

	private int _totalDreamDustIncome;

	private int _totalGoldIncome;

	private int _totalStardustIncome;

	private bool _didSendRunEnd;

	private CurrentRoomTrackInfo _roomInfo;

	public override bool shouldRegisterUpdates => false;

	public bool isAnalyticsDisabled { get; private set; }

	internal static void RegisterHandlers()
	{
		NetworkClient.RegisterHandler<DisableAnalyticsMessage>(HandleDisableAnalyticsMessage);
	}

	public void DisableAnalytics()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("Analytics can be disabled on server.");
		}
		else
		{
			NetworkServer.SendToAll(default(DisableAnalyticsMessage));
		}
	}

	public void DisableAnalyticsLocal()
	{
		if (!isAnalyticsDisabled)
		{
			Debug.Log("Analytics disabled for this session.");
			isAnalyticsDisabled = true;
		}
	}

	private static void HandleDisableAnalyticsMessage(DisableAnalyticsMessage msg)
	{
		ManagerBase<InGameAnalyticsManager>.instance.DisableAnalyticsLocal();
	}

	private void Start()
	{
		if (DewBuildProfile.current.disableAnalytics)
		{
			Debug.Log("This build has analytics disabled.");
			DisableAnalyticsLocal();
		}
		UA_Init();
	}

	private void OnDestroy()
	{
		UA_Cleanup();
	}

	private void UA_Init()
	{
		GameManager.CallOnReady(delegate
		{
			Hero hero = DewPlayer.local.hero;
			int num = ((DewInput.currentMode != InputMode.KeyboardAndMouse) ? 2 : (DewSave.profile.controls.enableDirMoveKeys ? 1 : 0));
			Dictionary<string, object> parameters = new Dictionary<string, object>
			{
				{
					"c_loadoutQ",
					hero.Skill.Q.GetType().Name
				},
				{
					"c_loadoutR",
					hero.Skill.R.GetType().Name
				},
				{
					"c_loadoutTrait",
					hero.Skill.Identity.GetType().Name
				},
				{ "c_controlType", num }
			};
			UA_InvokeRunEvent("RunStart", parameters);
			UA_InvokeRunEvent("RunEnterRoom");
			UA_InvokeRunEvent("RunEnterZone");
			try
			{
				foreach (Type current in Dew.allHeroes)
				{
					if (Dew.IsHeroIncludedInGame(current.Name) && DewSave.profile.heroes.TryGetValue(current.Name, out var value) && value.isAvailableInGame)
					{
						UA_InvokeEventRaw("HeroPreference", new Dictionary<string, object>
						{
							{ "c_type", current.Name },
							{
								"c_chosen",
								hero.GetType() == current
							}
						});
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			try
			{
				IList<string> lucidDreams = NetworkedManagerBase<GameManager>.instance.GetLucidDreams();
				foreach (Type current2 in Dew.allLucidDreams)
				{
					if (Dew.IsLucidDreamIncludedInGame(current2.Name) && DewSave.profile.lucidDreams.TryGetValue(current2.Name, out var value2) && value2.isAvailableInGame)
					{
						UA_InvokeEventRaw("LucidDreamPreference", new Dictionary<string, object>
						{
							{ "c_type", current2.Name },
							{
								"c_chosen",
								lucidDreams.Contains(current2.Name)
							}
						});
					}
				}
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)delegate
			{
				UA_InvokeRunEvent("RunEnterRoom");
			};
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoadStarted += (Action)delegate
			{
				UA_InvokeRunEvent("RunExitZone", new Dictionary<string, object> { 
				{
					"c_clearedNodes",
					NetworkedManagerBase<ZoneManager>.instance.clearedCombatRooms
				} });
			};
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += (Action)delegate
			{
				UA_InvokeRunEvent("RunEnterZone");
			};
			DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
			dewNetworkManager.onSessionEnd = (Action)Delegate.Combine(dewNetworkManager.onSessionEnd, new Action(InvokeRunEndIfDidnt));
			NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameResult += (Action<DewGameResult>)delegate(DewGameResult result)
			{
				if (DewPlayer.local != null && DewPlayer.local.hero != null && DewPlayer.local.hero.Skill != null)
				{
					HeroSkill skill = DewPlayer.local.hero.Skill;
					if (skill.Q != null)
					{
						UA_InvokeItemEvent("ItemFinalize", skill.Q);
					}
					if (skill.W != null)
					{
						UA_InvokeItemEvent("ItemFinalize", skill.W);
					}
					if (skill.E != null)
					{
						UA_InvokeItemEvent("ItemFinalize", skill.E);
					}
					if (skill.R != null)
					{
						UA_InvokeItemEvent("ItemFinalize", skill.R);
					}
					if (skill.Identity != null)
					{
						UA_InvokeItemEvent("ItemFinalize", skill.Identity);
					}
					foreach (KeyValuePair<GemLocation, Gem> gem in skill.gems)
					{
						UA_InvokeItemEvent("ItemFinalize", gem.Value);
					}
				}
				UA_InvokeRunEvent("RunResult", new Dictionary<string, object> { 
				{
					"c_gameResult",
					result.result.ToString()
				} });
				InvokeRunEndIfDidnt();
			};
			DewPlayer.local.hero.ClientHeroEvent_OnKnockedOut += (Action<EventInfoKill>)delegate(EventInfoKill kill)
			{
				UA_InvokeRunEvent("RunKnockedOut", new Dictionary<string, object> { 
				{
					"c_actorType",
					(kill.actor == null) ? "Unknown" : kill.actor.GetType().Name
				} });
			};
			NetworkedManagerBase<ActorManager>.instance.onEntityAdd += (Action<Entity>)delegate(Entity e)
			{
				StartCoroutine(Routine2());
				IEnumerator Routine2()
				{
					yield return null;
					if (!e.IsNullInactiveDeadOrKnockedOut() && e.IsAnyBoss())
					{
						Ad_KillTimeTracker tracker = new Ad_KillTimeTracker();
						e.AddData(tracker);
						float lastTrackTime = Time.time;
						while (!e.IsNullInactiveDeadOrKnockedOut())
						{
							yield return new WaitForSeconds((tracker.aliveDuration < 5f) ? 0.1f : 0.5f);
							if (ManagerBase<CameraManager>.instance.isPlayingCutscene)
							{
								lastTrackTime = Time.time;
							}
							else
							{
								float deltaTime = Time.time - lastTrackTime;
								lastTrackTime = Time.time;
								tracker.aliveDuration += deltaTime;
								if (!e.Status.hasDamageImmunity)
								{
									tracker.aliveDurationExcludeDamageImmunity += deltaTime;
								}
							}
						}
					}
				}
			};
			DewPlayer.local.hero.ClientHeroEvent_OnKillOrAssist += (Action<EventInfoKill>)delegate(EventInfoKill kill)
			{
				if (kill.victim.IsAnyBoss() && kill.victim.TryGetData<Ad_KillTimeTracker>(out var data))
				{
					UA_InvokeRunEvent("RunKillBoss", new Dictionary<string, object>
					{
						{
							"c_monsterType",
							kill.victim.GetType().Name
						},
						{ "c_timeTaken", data.aliveDuration },
						{ "c_timeTakenWithoutImmunity", data.aliveDurationExcludeDamageImmunity }
					});
				}
			};
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)delegate
			{
				if (_roomInfo._tracker != null)
				{
					StopCoroutine(_roomInfo._tracker);
				}
				_roomInfo = default(CurrentRoomTrackInfo);
				_roomInfo._tracker = StartCoroutine(Routine());
				SingletonDewNetworkBehaviour<Room>.instance.ClientEvent_OnRoomClear += (Action)delegate
				{
					if (_roomInfo._tracker != null)
					{
						StopCoroutine(_roomInfo._tracker);
						if (SingletonDewNetworkBehaviour<Room>.instance != null)
						{
							UA_InvokeRunEvent("RunClearNormalRoom", new Dictionary<string, object>
							{
								{
									"c_clearedSections",
									SingletonDewNetworkBehaviour<Room>.instance.numOfActivatedCombatAreas
								},
								{ "c_timeTakenInCombat", _roomInfo.timeTakenInCombat },
								{ "c_timeTaken", _roomInfo.timeTaken },
								{
									"c_combatDps",
									(float)(_roomInfo.totalDamageDealt / (double)_roomInfo.timeTakenInCombat)
								},
								{
									"c_combatDpsWithoutDiscarded",
									(float)(_roomInfo.totalDamageDealtWithoutDiscarded / (double)_roomInfo.timeTakenInCombat)
								},
								{
									"c_currentNodeHuntLevel",
									NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted ? NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel : 0
								}
							});
						}
						_roomInfo = default(CurrentRoomTrackInfo);
					}
				};
			};
			NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage info)
			{
				if (_roomInfo._tracker != null && !DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut() && info.actor.IsDescendantOf(DewPlayer.local.hero) && info.victim is Monster)
				{
					_roomInfo.totalDamageDealt += info.damage.amount + info.damage.discardedAmount;
					_roomInfo.totalDamageDealtWithoutDiscarded += info.damage.amount;
				}
			};
			DewPlayer.local.ClientEvent_OnEarnStardust += (Action<int>)delegate(int amount)
			{
				_totalStardustIncome += amount;
			};
			_totalGoldIncome = DewPlayer.local.gold;
			DewPlayer.local.ClientEvent_OnEarnGold += (Action<int>)delegate(int amount)
			{
				_totalGoldIncome += amount;
			};
			_totalDreamDustIncome = DewPlayer.local.dreamDust;
			DewPlayer.local.ClientEvent_OnEarnDreamDust += (Action<int>)delegate(int amount)
			{
				_totalDreamDustIncome += amount;
			};
			NetworkedManagerBase<ClientEventManager>.instance.OnDismantled += (Action<Hero, NetworkBehaviour>)delegate(Hero hero2, NetworkBehaviour target)
			{
				if (hero2.isOwned)
				{
					UA_InvokeItemEvent("ItemDismantled", target);
				}
			};
			NetworkedManagerBase<ClientEventManager>.instance.OnItemBought += (Action<Hero, NetworkBehaviour>)delegate(Hero hero2, NetworkBehaviour target)
			{
				if (hero2.isOwned)
				{
					UA_InvokeItemEvent("ItemBought", target);
				}
			};
			NetworkedManagerBase<ClientEventManager>.instance.OnItemSold += (Action<Hero, NetworkBehaviour>)delegate(Hero hero2, NetworkBehaviour target)
			{
				if (hero2.isOwned)
				{
					UA_InvokeItemEvent("ItemSold", target);
				}
			};
			NetworkedManagerBase<ClientEventManager>.instance.OnItemUpgraded += (Action<Hero, NetworkBehaviour>)delegate(Hero hero2, NetworkBehaviour target)
			{
				if (hero2.isOwned)
				{
					UA_InvokeItemEvent("ItemUpgraded", target);
				}
			};
			NetworkedManagerBase<ClientEventManager>.instance.OnItemCleansed += (Action<Hero, NetworkBehaviour>)delegate(Hero hero2, NetworkBehaviour target)
			{
				if (hero2.isOwned)
				{
					UA_InvokeItemEvent("ItemCleansed", target);
				}
			};
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnGemPickup += (Action<Gem>)delegate(Gem gem)
			{
				UA_InvokeItemEvent("ItemPickedUp", gem);
			};
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnGemDrop += (Action<Gem>)delegate(Gem gem)
			{
				UA_InvokeItemEvent("ItemDropped", gem);
			};
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnSkillPickup += (Action<SkillTrigger>)delegate(SkillTrigger skill)
			{
				UA_InvokeItemEvent("ItemPickedUp", skill);
			};
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnSkillDrop += (Action<SkillTrigger>)delegate(SkillTrigger skill)
			{
				UA_InvokeItemEvent("ItemDropped", skill);
			};
		});
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.25f);
			if (SingletonDewNetworkBehaviour<Room>.instance.isRevisit || NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.Combat || !SingletonDewNetworkBehaviour<Room>.instance.monsters.clearRoomOnClearAllCombatAreas || (NetworkedManagerBase<ZoneManager>.instance.currentNode.HasMainModifier() && !NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted))
			{
				_roomInfo = default(CurrentRoomTrackInfo);
			}
			else
			{
				float lastTrackTime = Time.time;
				while (true)
				{
					yield return new WaitForSeconds(0.25f);
					float deltaTime = Time.time - lastTrackTime;
					lastTrackTime = Time.time;
					if (!ManagerBase<CameraManager>.instance.isPlayingCutscene && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
					{
						if (DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut())
						{
							break;
						}
						_roomInfo.timeTaken += deltaTime;
						if (DewPlayer.local.hero.isInCombat)
						{
							_roomInfo.timeTakenInCombat += deltaTime;
						}
					}
				}
				_roomInfo = default(CurrentRoomTrackInfo);
			}
		}
	}

	private void UA_Cleanup()
	{
		if (DewNetworkManager.instance != null)
		{
			DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
			dewNetworkManager.onSessionEnd = (Action)Delegate.Remove(dewNetworkManager.onSessionEnd, new Action(InvokeRunEndIfDidnt));
		}
	}

	private void InvokeRunEndIfDidnt()
	{
		if (!_didSendRunEnd)
		{
			_didSendRunEnd = true;
			UA_InvokeRunEvent("RunEnd");
		}
	}

	private void UA_InvokeEventRaw(string eventName, Dictionary<string, object> parameters)
	{
		if (isAnalyticsDisabled || DewPlayer.local == null)
		{
			return;
		}
		try
		{
			AnalyticsService.Instance.CustomData(eventName, parameters);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void UA_InvokeRunEvent(string eventName, Dictionary<string, object> parameters = null)
	{
		if (!isAnalyticsDisabled && !(DewPlayer.local == null))
		{
			if (parameters == null)
			{
				parameters = new Dictionary<string, object>();
			}
			UA_AddGameParameters(parameters);
			UA_AddRunSpecificParameters(parameters);
			UA_InvokeEventRaw(eventName, parameters);
		}
	}

	private void UA_InvokeItemEvent(string eventName, NetworkBehaviour item, Dictionary<string, object> parameters = null)
	{
		if (!isAnalyticsDisabled && !(DewPlayer.local == null))
		{
			if (parameters == null)
			{
				parameters = new Dictionary<string, object>();
			}
			UA_AddGameParameters(parameters);
			UA_AddItemSpecificParameters(parameters, item);
			UA_InvokeEventRaw(eventName, parameters);
		}
	}

	private void UA_AddGameParameters(Dictionary<string, object> dict)
	{
		try
		{
			Hero localHero = DewPlayer.local.hero;
			dict.Add("g_id", NetworkedManagerBase<GameManager>.instance.gameId);
			dict.Add("g_hero", (localHero != null) ? localHero.GetType().Name : "");
			dict.Add("g_heroLevel", (localHero != null) ? localHero.level : 0);
			dict.Add("g_runElapsedTime", NetworkedManagerBase<GameManager>.instance.elapsedGameTime);
			dict.Add("g_ambientLevel", NetworkedManagerBase<GameManager>.instance.ambientLevel);
			dict.Add("g_zoneIndex", NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
			dict.Add("g_roomIndex", NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex);
			dict.Add("g_equipment", (localHero != null) ? new AnalyticsEquipmentData(localHero).ToBase64() : "");
			dict.Add("g_difficulty", NetworkedManagerBase<GameManager>.instance.difficulty.name);
			dict.Add("g_playerCount", DewPlayer.humanPlayers.Count);
			dict.Add("g_room", SceneManager.GetActiveScene().name);
			dict.Add("g_zone", NetworkedManagerBase<ZoneManager>.instance.currentZone.name);
			dict.Add("g_totalPlayTime", DewSave.profile.totalPlayTimeMinutes);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void UA_AddRunSpecificParameters(Dictionary<string, object> dict)
	{
		try
		{
			SkillTrigger bestSkill = UA_GetBestSkill();
			string bestSkillType = ((bestSkill != null) ? bestSkill.GetType().Name : "");
			int bestSkillLevel = ((bestSkill != null) ? bestSkill.level : 0);
			Gem bestGem = UA_GetBestGem();
			string bestGemType = ((bestGem != null) ? bestGem.GetType().Name : "");
			int bestGemQuality = ((bestGem != null) ? bestGem.quality : 0);
			dict.Add("r_bestSkill", bestSkillType);
			dict.Add("r_bestSkillLevel", bestSkillLevel);
			dict.Add("r_bestGem", bestGemType);
			dict.Add("r_bestGemQuality", bestGemQuality);
			dict.Add("r_totalGoldIncome", _totalGoldIncome);
			dict.Add("r_totalDreamDustIncome", _totalDreamDustIncome);
			dict.Add("r_totalStardustIncome", _totalStardustIncome);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private SkillTrigger UA_GetBestSkill()
	{
		if (DewPlayer.local == null || DewPlayer.local.hero == null)
		{
			return null;
		}
		SkillTrigger bestSkill = null;
		int bestLevel = -1;
		HeroSkill s = DewPlayer.local.hero.Skill;
		if (s.Q != null && s.Q.level > bestLevel)
		{
			bestLevel = s.Q.level;
			bestSkill = s.Q;
		}
		if (s.W != null && s.W.level > bestLevel)
		{
			bestLevel = s.W.level;
			bestSkill = s.W;
		}
		if (s.E != null && s.E.level > bestLevel)
		{
			bestLevel = s.E.level;
			bestSkill = s.E;
		}
		if (s.R != null && s.R.level > bestLevel)
		{
			bestLevel = s.R.level;
			bestSkill = s.R;
		}
		if (s.Identity != null && s.Identity.level > bestLevel)
		{
			bestLevel = s.Identity.level;
			bestSkill = s.Identity;
		}
		return bestSkill;
	}

	private Gem UA_GetBestGem()
	{
		if (DewPlayer.local == null || DewPlayer.local.hero == null)
		{
			return null;
		}
		Gem bestGem = null;
		int bestQuality = -1;
		foreach (KeyValuePair<GemLocation, Gem> g in DewPlayer.local.hero.Skill.gems)
		{
			if (bestQuality < g.Value.quality)
			{
				bestQuality = g.Value.quality;
				bestGem = g.Value;
			}
		}
		return bestGem;
	}

	private void UA_AddItemSpecificParameters(Dictionary<string, object> dict, NetworkBehaviour item)
	{
		try
		{
			dict.Add("i_itemType", item.GetType().Name);
			if (item is SkillTrigger skill)
			{
				dict.Add("i_itemLevel", skill.level);
			}
			else if (item is Gem gem)
			{
				dict.Add("i_itemLevel", gem.quality);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
