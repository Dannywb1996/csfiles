using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class AchievementManager : ManagerBase<AchievementManager>
{
	public static LastGamePlayReward lastGamePlayReward;

	internal List<DewAchievementItem> _trackedAchievements = new List<DewAchievementItem>();

	public SafeAction<Type> LocalClientEvent_OnAchievementComplete;

	public bool disableTrackingAchievements;

	private HashSet<Type> _incrementedPlayCount = new HashSet<Type>();

	public bool isTrackingAchievements { get; private set; }

	private void Start()
	{
		if (disableTrackingAchievements)
		{
			return;
		}
		lastGamePlayReward = null;
		NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameResult += (Action<DewGameResult>)delegate(DewGameResult obj)
		{
			if (isTrackingAchievements)
			{
				for (int num = _trackedAchievements.Count - 1; num >= 0; num--)
				{
					_trackedAchievements[num].FeedGameResult(obj);
				}
				StopTrackingAchievements();
			}
		};
		RegisterCallback_CalculateMasteryPoints();
		RegisterCallback_ItemStatistics();
		NetworkedManagerBase<QuestManager>.instance.ClientEvent_OnArtifactAppraised += new Action<string, bool>(ClientEventOnArtifactAppraised);
		DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
		dewNetworkManager.onSessionEnd = (Action)Delegate.Combine(dewNetworkManager.onSessionEnd, (Action)delegate
		{
			if (isTrackingAchievements)
			{
				StopTrackingAchievements();
			}
		});
		GameManager.CallOnReady(delegate
		{
			StartTrackingAchievements();
			DewPlayer.local.ClientEvent_OnHeroChanged += (Action<Hero, Hero>)delegate
			{
				if (isTrackingAchievements)
				{
					StopTrackingAchievements();
					StartTrackingAchievements();
				}
			};
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnDismantled += new Action<Hero, NetworkBehaviour>(Dismantled);
	}

	private void ClientEventOnArtifactAppraised(string artifactName, bool isNew)
	{
		if (lastGamePlayReward != null && isNew)
		{
			lastGamePlayReward.unlockedArtifacts.Add(artifactName);
		}
	}

	private void OnDestroy()
	{
		if (isTrackingAchievements)
		{
			StopTrackingAchievements();
		}
	}

	internal void StartTrackingAchievements()
	{
		if (isTrackingAchievements)
		{
			return;
		}
		isTrackingAchievements = true;
		if (lastGamePlayReward == null)
		{
			lastGamePlayReward = new LastGamePlayReward();
		}
		foreach (KeyValuePair<string, DewProfile.AchievementData> p in DewSave.profile.achievements)
		{
			if ((!p.Value.isCompleted || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth)) && Dew.IsAchievementIncludedInGame(p.Key))
			{
				DewAchievementItem newItem = (DewAchievementItem)Activator.CreateInstance(Dew.achievementsByName[p.Key]);
				_trackedAchievements.Add(newItem);
			}
		}
		for (int i = _trackedAchievements.Count - 1; i >= 0; i--)
		{
			DewAchievementItem a = _trackedAchievements[i];
			try
			{
				a.OnStartLocalClient();
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while starting achievement: " + a.name);
				Debug.LogException(exception, this);
				_trackedAchievements.RemoveAt(i);
			}
		}
		DewPlayer.local.hero.Skill.ClientHeroEvent_OnSkillPickup += new Action<SkillTrigger>(ClientHeroEventOnSkillPickup);
		DewPlayer.local.hero.Skill.ClientHeroEvent_OnGemPickup += new Action<Gem>(ClientHeroEventOnGemPickup);
		Debug.Log($"Started tracking {_trackedAchievements.Count} achievements");
	}

	private void Dismantled(Hero hero, NetworkBehaviour item)
	{
		if (!(DewPlayer.local == null) && !(hero != DewPlayer.local.hero))
		{
			if (item is SkillTrigger && DewSave.profile.DiscoverSkill(item.GetType().Name))
			{
				lastGamePlayReward?.discoveredSkills.Add(item.GetType());
			}
			else if (item is Gem && DewSave.profile.DiscoverGem(item.GetType().Name))
			{
				lastGamePlayReward?.discoveredGems.Add(item.GetType());
			}
		}
	}

	private void ClientHeroEventOnGemPickup(Gem obj)
	{
		if (DewSave.profile.DiscoverGem(obj.GetType().Name))
		{
			lastGamePlayReward?.discoveredGems.Add(obj.GetType());
		}
	}

	private void ClientHeroEventOnSkillPickup(SkillTrigger obj)
	{
		if (DewSave.profile.DiscoverSkill(obj.GetType().Name))
		{
			lastGamePlayReward?.discoveredSkills.Add(obj.GetType());
		}
	}

	public void StopTrackingAchievements()
	{
		if (!isTrackingAchievements)
		{
			return;
		}
		isTrackingAchievements = false;
		foreach (DewAchievementItem a in _trackedAchievements)
		{
			try
			{
				a.OnStopLocalClient();
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while stopping achievement: " + a.name);
				Debug.LogException(exception, this);
			}
		}
		if (DewPlayer.local != null && DewPlayer.local.hero != null)
		{
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnSkillPickup -= new Action<SkillTrigger>(ClientHeroEventOnSkillPickup);
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnGemPickup -= new Action<Gem>(ClientHeroEventOnGemPickup);
		}
		Debug.Log($"Stopped tracking {_trackedAchievements.Count} achievements");
		_trackedAchievements.Clear();
	}

	internal void CompleteAchievement(DewAchievementItem item)
	{
		string achKey = item.name;
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth) && DewSave.profile.achievements[achKey].isCompleted)
		{
			Debug.LogWarning("Tried to complete already completed achievement: " + item.name);
			return;
		}
		if (!_trackedAchievements.Contains(item))
		{
			Debug.LogWarning("Tried to complete non-tracked achievement: " + item.name);
			return;
		}
		Debug.Log("Achievement completed: " + achKey);
		int maxProgress = item.GetMaxProgress();
		DewProfile.AchievementData achievementData = DewSave.profile.achievements[achKey];
		achievementData.isNew = true;
		achievementData.isCompleted = true;
		achievementData.maxProgress = maxProgress;
		achievementData.currentProgress = maxProgress;
		achievementData.persistentVariables = null;
		foreach (Type t in Dew.GetUnlockedTargetsOfAchievement(item.GetType()))
		{
			if (t.IsSubclassOf(typeof(Hero)))
			{
				if (DewSave.profile.heroes[t.Name].status != UnlockStatus.Locked)
				{
					Debug.LogWarning("Tried to unlock already unlocked hero: " + t.Name);
					continue;
				}
				Debug.Log("New hero unlocked: " + t.Name);
				DewSave.profile.UnlockHero(t.Name);
			}
			else if (t.IsSubclassOf(typeof(SkillTrigger)))
			{
				if (DewSave.profile.skills[t.Name].status != UnlockStatus.Locked)
				{
					Debug.LogWarning("Tried to unlock already unlocked skill: " + t.Name);
					continue;
				}
				Debug.Log("New skill unlocked: " + t.Name);
				DewSave.profile.UnlockSkill(t.Name);
			}
			else if (t.IsSubclassOf(typeof(Gem)))
			{
				if (DewSave.profile.gems[t.Name].status != UnlockStatus.Locked)
				{
					Debug.LogWarning("Tried to unlock already unlocked gem: " + t.Name);
					continue;
				}
				Debug.Log("New gem unlocked: " + t.Name);
				DewSave.profile.UnlockGem(t.Name);
			}
			else if (t.IsSubclassOf(typeof(LucidDream)))
			{
				if (DewSave.profile.lucidDreams[t.Name].status != UnlockStatus.Locked)
				{
					Debug.LogWarning("Tried to unlock already unlocked lucid dream: " + t.Name);
					continue;
				}
				Debug.Log("New lucid dream unlocked: " + t.Name);
				DewSave.profile.UnlockLucidDream(t.Name);
			}
			else
			{
				Debug.LogWarning("Tried to unlock target of unknown type: " + t.Name);
			}
		}
		try
		{
			item.OnStopLocalClient();
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception occured while stopping achievement: " + item.name);
			Debug.LogException(exception, this);
		}
		DewSave.SaveProfile();
		_trackedAchievements.Remove(item);
		try
		{
			LocalClientEvent_OnAchievementComplete?.Invoke(item.GetType());
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2, this);
		}
		if (lastGamePlayReward != null)
		{
			lastGamePlayReward.unlockedAchievements.Add(item.GetType());
		}
		DewPlayer.local.CmdRequestStardust(item.grantedStardust);
		NetworkedManagerBase<ChatManager>.instance.CmdSendAchievementMessage(item.name);
	}

	private void RegisterCallback_CalculateMasteryPoints()
	{
		NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameResult += (Action<DewGameResult>)delegate(DewGameResult obj)
		{
			float remainingMinutes;
			float effectiveMinutes;
			if (isTrackingAchievements)
			{
				DewGameResult.PlayerData playerData = obj.players.Find((DewGameResult.PlayerData p) => p.isLocalPlayer);
				if (playerData != null)
				{
					if (lastGamePlayReward != null)
					{
						lastGamePlayReward.heroType = playerData.heroType;
					}
					if (lastGamePlayReward != null && DewBuildProfile.current.buildType != BuildType.DemoLite)
					{
						float b = (float)(playerData.heroicBossKills + 1) * 8f * 1000f;
						remainingMinutes = (float)playerData.combatTime / 60f * 1.35f;
						effectiveMinutes = 0f;
						AddToEffectiveMinutes(30f, 1.3f);
						AddToEffectiveMinutes(30f, 1f);
						AddToEffectiveMinutes(60f, 0.75f);
						AddToEffectiveMinutes(float.PositiveInfinity, 0.65f);
						float f = Mathf.Min(effectiveMinutes * 1000f, b) + UnityEngine.Random.Range(0f, 3000f);
						lastGamePlayReward.heroMasteryPoints = Mathf.RoundToInt(f);
						DewSave.profile.AddMasteryPoints(playerData.heroType, lastGamePlayReward.heroMasteryPoints);
						DewSave.SaveProfile();
					}
				}
			}
			void AddToEffectiveMinutes(float amount, float efficiency)
			{
				if (remainingMinutes >= amount)
				{
					effectiveMinutes += amount * efficiency;
					remainingMinutes -= amount;
				}
				else
				{
					effectiveMinutes += remainingMinutes * efficiency;
					remainingMinutes = 0f;
				}
			}
		};
	}

	private void RegisterCallback_ItemStatistics()
	{
		StartCoroutine(Routine());
		GameManager.CallOnReady(delegate
		{
			DewPlayer.local.ClientEvent_OnHeroChanged += (Action<Hero, Hero>)delegate
			{
				DewPlayer.local.hero.Skill.ClientHeroEvent_OnSkillPickup += (Action<SkillTrigger>)delegate(SkillTrigger obj)
				{
					IncrementPlayCount(obj.GetType());
				};
				DewPlayer.local.hero.Skill.ClientHeroEvent_OnGemPickup += (Action<Gem>)delegate(Gem obj)
				{
					IncrementPlayCount(obj.GetType());
				};
			};
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnSkillPickup += (Action<SkillTrigger>)delegate(SkillTrigger obj)
			{
				IncrementPlayCount(obj.GetType());
			};
			DewPlayer.local.hero.Skill.ClientHeroEvent_OnGemPickup += (Action<Gem>)delegate(Gem obj)
			{
				IncrementPlayCount(obj.GetType());
			};
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnItemUpgraded += (Action<Hero, NetworkBehaviour>)delegate(Hero hero, NetworkBehaviour target)
		{
			if (hero.isOwned && DewSave.profile.itemStatistics.TryGetValue(target.GetType().Name, out var value))
			{
				value.upgradeCount++;
			}
		};
		NetworkedManagerBase<ClientEventManager>.instance.OnDismantled += (Action<Hero, NetworkBehaviour>)delegate(Hero hero, NetworkBehaviour target)
		{
			if (hero.isOwned && DewSave.profile.itemStatistics.TryGetValue(target.GetType().Name, out var value))
			{
				value.dismantleCount++;
			}
		};
		NetworkedManagerBase<ClientEventManager>.instance.OnItemBought += (Action<Hero, NetworkBehaviour>)delegate(Hero hero, NetworkBehaviour target)
		{
			if (hero.isOwned && DewSave.profile.itemStatistics.TryGetValue(target.GetType().Name, out var value))
			{
				value.buyCount++;
			}
		};
		NetworkedManagerBase<ClientEventManager>.instance.OnItemSold += (Action<Hero, NetworkBehaviour>)delegate(Hero hero, NetworkBehaviour target)
		{
			if (hero.isOwned && DewSave.profile.itemStatistics.TryGetValue(target.GetType().Name, out var value))
			{
				value.sellCount++;
			}
		};
		NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameResult += (Action<DewGameResult>)delegate(DewGameResult obj)
		{
			if (isTrackingAchievements)
			{
				DewGameResult.PlayerData playerData = obj.players.Find((DewGameResult.PlayerData p) => p.isLocalPlayer);
				if (playerData != null)
				{
					bool flag = obj.result == DewGameResult.ResultType.DemoFinish;
					if (DewSave.profile.itemStatistics.TryGetValue(playerData.heroType, out var value))
					{
						if (flag)
						{
							value.wins++;
						}
						value.playCount++;
						value.playTimeSeconds += obj.elapsedGameTimeSeconds;
					}
					foreach (DewGameResult.SkillData current in playerData.skills)
					{
						if (DewSave.profile.itemStatistics.TryGetValue(current.name, out var value2))
						{
							if (flag)
							{
								value2.wins++;
							}
							value2.playCount++;
							value2.playTimeSeconds += UnityEngine.Random.Range(0, 5);
						}
					}
					foreach (DewGameResult.GemData current2 in playerData.gems)
					{
						if (DewSave.profile.itemStatistics.TryGetValue(current2.name, out var value3))
						{
							if (flag)
							{
								value3.wins++;
							}
							value3.playCount++;
							value3.playTimeSeconds += UnityEngine.Random.Range(0, 5);
						}
					}
					DewSave.SaveProfile();
				}
			}
		};
		static IEnumerator Routine()
		{
			while (true)
			{
				yield return new WaitForSeconds(5f);
				if (!(NetworkedManagerBase<GameManager>.softInstance == null) && !NetworkedManagerBase<GameManager>.instance.isGameTimePaused && !(DewPlayer.local == null) && !DewPlayer.local.hero.IsNullOrInactive())
				{
					foreach (KeyValuePair<int, AbilityTrigger> p in DewPlayer.local.hero.Ability.abilities)
					{
						if (p.Value is SkillTrigger && DewSave.profile.itemStatistics.TryGetValue(p.Value.GetType().Name, out var itemStatistics))
						{
							itemStatistics.playTimeSeconds += 5L;
						}
					}
					foreach (KeyValuePair<GemLocation, Gem> p2 in DewPlayer.local.hero.Skill.gems)
					{
						if (DewSave.profile.itemStatistics.TryGetValue(p2.Value.GetType().Name, out var itemStatistics2))
						{
							itemStatistics2.playTimeSeconds += 5L;
						}
					}
				}
			}
		}
	}

	private void IncrementPlayCount(Type target)
	{
		if (!_incrementedPlayCount.Contains(target))
		{
			_incrementedPlayCount.Add(target);
			if (DewSave.profile.itemStatistics.TryGetValue(target.Name, out var data))
			{
				data.playCount++;
			}
		}
	}
}
