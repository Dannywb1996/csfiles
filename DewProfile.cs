using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class DewProfile
{
	public class HeroStarSlotUnlockData
	{
		public List<int> addedDestruction = new List<int>();

		public List<int> addedLife = new List<int>();

		public List<int> addedImagination = new List<int>();

		public List<int> addedFlexible = new List<int>();

		public List<int> Get(StarType type)
		{
			return type switch
			{
				StarType.Life => addedLife, 
				StarType.Destruction => addedDestruction, 
				StarType.Imagination => addedImagination, 
				StarType.Flexible => addedFlexible, 
				_ => throw new ArgumentOutOfRangeException("type", type, null), 
			};
		}

		public void Set(StarType type, List<int> list)
		{
			switch (type)
			{
			case StarType.Life:
				addedLife = list;
				break;
			case StarType.Destruction:
				addedDestruction = list;
				break;
			case StarType.Imagination:
				addedImagination = list;
				break;
			case StarType.Flexible:
				addedFlexible = list;
				break;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
		}
	}

	public class HeroMasteryData
	{
		public int currentLevel;

		public long currentPoints;

		public long totalPoints;
	}

	public class AchievementData
	{
		public bool isNew;

		public bool isCompleted;

		public int currentProgress;

		public int maxProgress;

		public long startTime;

		public Dictionary<string, string> persistentVariables;
	}

	public class DailyReverieData : ReverieDataBase
	{
		public long nextRefillTimestamp;

		public bool wasNeverFilled;
	}

	public class SpecialReverieData : ReverieDataBase
	{
		public long timeLimitTimestamp;
	}

	public class ReverieDataBase
	{
		public string type;

		public bool isComplete;

		public int currentProgress;

		public int maxProgress;

		public int grantedStardust;

		public string[] grantedItems;

		public Dictionary<string, string> persistentVariables;

		public bool IsEmpty()
		{
			return string.IsNullOrEmpty(type);
		}
	}

	public class UnlockData
	{
		public UnlockStatus status;

		public bool didReadMemory;

		public bool isNewHeroOrHeroSkill;

		public bool isAvailableInGame => status != UnlockStatus.Locked;
	}

	public class CosmeticsData
	{
		public bool isUnlocked;

		public bool isNew;

		public long unlockDate;

		public string ownershipKey;
	}

	public class StarData
	{
		public int level;

		public StarData Clone()
		{
			return (StarData)MemberwiseClone();
		}

		public bool IsUnchanged(StarData other)
		{
			return level == other.level;
		}
	}

	public class ItemStatistics
	{
		public int wins;

		public int playCount;

		public int upgradeCount;

		public int dismantleCount;

		public int buyCount;

		public int sellCount;

		public long playTimeSeconds;

		public long dejavuCostReductionPeriodTimestamp;
	}

	public const int CurrentSaveVersion = 5;

	public string name;

	public long creationDate = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

	public long totalPlayTimeMinutes;

	public string language = "";

	public int stardust;

	public int spentStardust;

	public string preferredDifficulty = "diffNormal";

	public string preferredHero = "Hero_Lacerta";

	public string preferredNametag = "";

	public string preferredLobbyName = "";

	public string preferredDejavuItem = "";

	public List<string> preferredLucidDreams = new List<string>();

	public int saveVersion;

	public List<string> equippedEmotes = new List<string>();

	public int completedReveries;

	public SpecialReverieData specialReverie;

	public List<DailyReverieData> reverieSlots = new List<DailyReverieData>();

	public List<string> lastReverieTypes = new List<string>();

	public int remainingRerolls;

	public long nextRerollReplenishTimestamp;

	public bool didReadLoopNotice;

	public bool didReadConstellationNotice;

	public bool didReadPrivateDemoNotice;

	public bool didPlayTutorial;

	public List<string> experienceFlags = new List<string>();

	public bool didMeetDreamTeller;

	public DewGameplaySettings_User gameplay = new DewGameplaySettings_User();

	public DewControlSettings controls = new DewControlSettings();

	public DewAudioSettings_User audio = new DewAudioSettings_User();

	public Dictionary<string, List<HeroLoadoutData>> heroLoadouts = new Dictionary<string, List<HeroLoadoutData>>();

	public Dictionary<string, int> heroSelectedLoadoutIndex = new Dictionary<string, int>();

	public Dictionary<string, List<string>> heroEquippedAccs = new Dictionary<string, List<string>>();

	public Dictionary<string, HeroMasteryData> heroMasteries = new Dictionary<string, HeroMasteryData>();

	public Dictionary<string, HeroStarSlotUnlockData> heroUnlockedStarSlots = new Dictionary<string, HeroStarSlotUnlockData>();

	public int totalMasteryLevel;

	public Dictionary<string, AchievementData> achievements = new Dictionary<string, AchievementData>();

	public Dictionary<string, StarData> stars = new Dictionary<string, StarData>();

	public Dictionary<string, StarData> newStars = new Dictionary<string, StarData>();

	public Dictionary<string, CosmeticsData> emotes = new Dictionary<string, CosmeticsData>();

	public Dictionary<string, CosmeticsData> accessories = new Dictionary<string, CosmeticsData>();

	public Dictionary<string, CosmeticsData> nametags = new Dictionary<string, CosmeticsData>();

	public Dictionary<string, UnlockData> heroes = new Dictionary<string, UnlockData>();

	public Dictionary<string, UnlockData> skills = new Dictionary<string, UnlockData>();

	public Dictionary<string, UnlockData> gems = new Dictionary<string, UnlockData>();

	public Dictionary<string, UnlockData> artifacts = new Dictionary<string, UnlockData>();

	public Dictionary<string, UnlockData> lucidDreams = new Dictionary<string, UnlockData>();

	public Dictionary<string, ItemStatistics> itemStatistics = new Dictionary<string, ItemStatistics>();

	public List<DewGameResult> favoriteGameResults = new List<DewGameResult>();

	public List<DewGameResult> lastGameResults = new List<DewGameResult>();

	public List<string> doneTutorials = new List<string>();

	public List<string> seenGuides = new List<string>();

	public void Initialize()
	{
		stardust = DewBuildProfile.current.defaultStardustAmount;
		saveVersion = 5;
		gameplay.Initialize();
	}

	public void Validate()
	{
		if (gameplay == null)
		{
			gameplay = new DewGameplaySettings_User();
		}
		if (controls == null)
		{
			controls = new DewControlSettings();
		}
		if (audio == null)
		{
			audio = new DewAudioSettings_User();
		}
		if (heroLoadouts == null)
		{
			heroLoadouts = new Dictionary<string, List<HeroLoadoutData>>();
		}
		if (heroSelectedLoadoutIndex == null)
		{
			heroSelectedLoadoutIndex = new Dictionary<string, int>();
		}
		if (heroEquippedAccs == null)
		{
			heroEquippedAccs = new Dictionary<string, List<string>>();
		}
		if (heroUnlockedStarSlots == null)
		{
			heroUnlockedStarSlots = new Dictionary<string, HeroStarSlotUnlockData>();
		}
		if (achievements == null)
		{
			achievements = new Dictionary<string, AchievementData>();
		}
		if (stars == null)
		{
			stars = new Dictionary<string, StarData>();
		}
		if (heroes == null)
		{
			heroes = new Dictionary<string, UnlockData>();
		}
		if (skills == null)
		{
			skills = new Dictionary<string, UnlockData>();
		}
		if (gems == null)
		{
			gems = new Dictionary<string, UnlockData>();
		}
		if (artifacts == null)
		{
			artifacts = new Dictionary<string, UnlockData>();
		}
		if (lucidDreams == null)
		{
			lucidDreams = new Dictionary<string, UnlockData>();
		}
		if (doneTutorials == null)
		{
			doneTutorials = new List<string>();
		}
		if (seenGuides == null)
		{
			seenGuides = new List<string>();
		}
		if (reverieSlots == null)
		{
			reverieSlots = new List<DailyReverieData>();
		}
		if (lastReverieTypes == null)
		{
			lastReverieTypes = new List<string>();
		}
		if (specialReverie == null)
		{
			specialReverie = new SpecialReverieData();
		}
		if (itemStatistics == null)
		{
			itemStatistics = new Dictionary<string, ItemStatistics>();
		}
		if (heroMasteries == null)
		{
			heroMasteries = new Dictionary<string, HeroMasteryData>();
		}
		DewSave.ValidateEnumValues(gameplay);
		DewSave.ValidateEnumValues(controls);
		DewSave.ValidateEnumValues(audio);
		if (saveVersion == 0)
		{
			saveVersion = 1;
		}
		if (saveVersion == 1)
		{
			saveVersion = 2;
			bool wasWasdControl = controls.enableDirMoveKeys;
			controls = new DewControlSettings();
			controls.ApplyPreset(wasWasdControl ? DewControlSettings.PresetType.WASD : DewControlSettings.PresetType.MOBA);
			GlobalLogicPackage.CallOnReady(delegate
			{
				ManagerBase<MessageManager>.instance.ShowMessageLocalized("Message_Warning_ControlSettingsReset");
			});
		}
		if (saveVersion == 2)
		{
			saveVersion = 3;
			FieldInfo[] fields = typeof(DewControlSettings).GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo f in fields)
			{
				if (!(f.FieldType != typeof(DewBinding)))
				{
					DewBinding binding = (DewBinding)f.GetValue(controls);
					binding.pcBinds = new List<PCBind>();
					if (binding.keyboard != Key.None)
					{
						binding.pcBinds.Add(new PCBind
						{
							key = binding.keyboard,
							modifiers = new List<Key>(binding.keyModifiers)
						});
					}
					if (binding.mouse != MouseButton.None)
					{
						binding.pcBinds.Add(new PCBind
						{
							mouse = binding.mouse,
							modifiers = new List<Key>(binding.keyModifiers)
						});
					}
				}
			}
		}
		if (saveVersion == 3)
		{
			saveVersion = 4;
			controls = new DewControlSettings();
			favoriteGameResults.Clear();
			lastGameResults.Clear();
			if (spentStardust == 0)
			{
				foreach (KeyValuePair<string, StarData> star in stars)
				{
					stardust += star.Value.level * 45;
				}
			}
			else
			{
				stardust += spentStardust;
			}
			spentStardust = 0;
			stars.Clear();
			artifacts.Clear();
			foreach (KeyValuePair<string, UnlockData> gem in gems)
			{
				gem.Value.didReadMemory = false;
			}
			foreach (KeyValuePair<string, UnlockData> skill2 in skills)
			{
				skill2.Value.didReadMemory = false;
			}
			GlobalLogicPackage.CallOnReady(delegate
			{
				ManagerBase<MessageManager>.instance.ShowMessageLocalized("Message_Warning_ControlSettingsReset");
			});
		}
		if (saveVersion == 4)
		{
			saveVersion = 5;
			foreach (KeyValuePair<string, UnlockData> gem2 in gems)
			{
				gem2.Value.didReadMemory = false;
			}
			foreach (KeyValuePair<string, UnlockData> skill3 in skills)
			{
				skill3.Value.didReadMemory = false;
			}
			foreach (KeyValuePair<string, UnlockData> artifact in artifacts)
			{
				artifact.Value.didReadMemory = false;
			}
		}
		controls.Validate();
		if (!ValidateProfileName(name))
		{
			name = "Traveler";
		}
		if (language == "zh-HK")
		{
			language = "zh-TW";
		}
		if (!DewLocalization.buildData.dataByLanguage.ContainsKey(language))
		{
			language = DewLocalization.GetRecommendedSupportedLanguage();
		}
		string[] array = achievements.Keys.ToArray();
		foreach (string n in array)
		{
			if (!Dew.achievementsByName.ContainsKey(n) || !Dew.IsAchievementIncludedInGame(n))
			{
				achievements.Remove(n);
			}
		}
		foreach (Type t in Dew.allAchievements)
		{
			if (!achievements.ContainsKey(t.Name) && Dew.IsAchievementIncludedInGame(t.Name))
			{
				DewAchievementItem a = (DewAchievementItem)Activator.CreateInstance(t);
				achievements.Add(t.Name, new AchievementData
				{
					isCompleted = false,
					currentProgress = 0,
					maxProgress = a.GetMaxProgress(),
					persistentVariables = new Dictionary<string, string>(),
					startTime = totalPlayTimeMinutes
				});
			}
		}
		foreach (KeyValuePair<string, AchievementData> p in achievements)
		{
			if (p.Value.isNew && !p.Value.isCompleted)
			{
				p.Value.isNew = false;
			}
		}
		HashSet<string> lockedTargets = new HashSet<string>();
		foreach (KeyValuePair<string, AchievementData> p2 in achievements)
		{
			if (p2.Value.isCompleted)
			{
				continue;
			}
			foreach (Type t2 in Dew.GetUnlockedTargetsOfAchievement(Dew.achievementsByName[p2.Key]))
			{
				if (typeof(Gem).IsAssignableFrom(t2) || typeof(SkillTrigger).IsAssignableFrom(t2) || typeof(LucidDream).IsAssignableFrom(t2))
				{
					lockedTargets.Add(t2.Name);
				}
				else if (typeof(Hero).IsAssignableFrom(t2))
				{
					lockedTargets.Add(t2.Name);
					HeroSkill skill = DewResources.GetByType<Hero>(t2).GetComponent<HeroSkill>();
					SkillTrigger[] loadoutSkills = skill.GetLoadoutSkills(HeroSkillLocation.Q);
					foreach (SkillTrigger s in loadoutSkills)
					{
						lockedTargets.Add(s.GetType().Name);
					}
					loadoutSkills = skill.GetLoadoutSkills(HeroSkillLocation.R);
					foreach (SkillTrigger s2 in loadoutSkills)
					{
						lockedTargets.Add(s2.GetType().Name);
					}
					loadoutSkills = skill.GetLoadoutSkills(HeroSkillLocation.Identity);
					foreach (SkillTrigger s3 in loadoutSkills)
					{
						lockedTargets.Add(s3.GetType().Name);
					}
				}
			}
		}
		foreach (Type o in Dew.allHeroes)
		{
			if (!heroes.ContainsKey(o.Name))
			{
				heroes.Add(o.Name, new UnlockData());
			}
			if (!heroMasteries.ContainsKey(o.Name))
			{
				heroMasteries.Add(o.Name, new HeroMasteryData());
			}
			if (!itemStatistics.ContainsKey(o.Name))
			{
				itemStatistics.Add(o.Name, new ItemStatistics());
			}
			if (!heroLoadouts.ContainsKey(o.Name))
			{
				heroLoadouts.Add(o.Name, new List<HeroLoadoutData>());
			}
			if (!heroUnlockedStarSlots.ContainsKey(o.Name))
			{
				heroUnlockedStarSlots.Add(o.Name, new HeroStarSlotUnlockData());
			}
			if (!heroEquippedAccs.ContainsKey(o.Name))
			{
				heroEquippedAccs.Add(o.Name, new List<string>());
			}
			if (!heroSelectedLoadoutIndex.ContainsKey(o.Name))
			{
				heroSelectedLoadoutIndex.Add(o.Name, 0);
			}
		}
		foreach (Type o2 in Dew.allSkills)
		{
			if (!skills.ContainsKey(o2.Name))
			{
				skills.Add(o2.Name, new UnlockData());
			}
			if (!itemStatistics.ContainsKey(o2.Name))
			{
				itemStatistics.Add(o2.Name, new ItemStatistics());
			}
		}
		foreach (Type o3 in Dew.allGems)
		{
			if (!gems.ContainsKey(o3.Name))
			{
				gems.Add(o3.Name, new UnlockData());
			}
			if (!itemStatistics.ContainsKey(o3.Name))
			{
				itemStatistics.Add(o3.Name, new ItemStatistics());
			}
		}
		foreach (Type o4 in Dew.allArtifacts)
		{
			if (!artifacts.ContainsKey(o4.Name))
			{
				artifacts.Add(o4.Name, new UnlockData());
			}
		}
		foreach (Type o5 in Dew.allLucidDreams)
		{
			if (!lucidDreams.ContainsKey(o5.Name))
			{
				lucidDreams.Add(o5.Name, new UnlockData());
			}
		}
		array = heroes.Keys.ToArray();
		foreach (string k in array)
		{
			if (Dew.allHeroes.All((Type type) => type.Name != k))
			{
				heroes.Remove(k);
			}
		}
		array = skills.Keys.ToArray();
		foreach (string k2 in array)
		{
			if (Dew.allSkills.All((Type type) => type.Name != k2))
			{
				skills.Remove(k2);
			}
		}
		array = gems.Keys.ToArray();
		foreach (string k3 in array)
		{
			if (Dew.allGems.All((Type type) => type.Name != k3))
			{
				gems.Remove(k3);
			}
		}
		array = artifacts.Keys.ToArray();
		foreach (string k4 in array)
		{
			if (Dew.allArtifacts.All((Type type) => type.Name != k4))
			{
				artifacts.Remove(k4);
			}
		}
		array = lucidDreams.Keys.ToArray();
		foreach (string k5 in array)
		{
			if (Dew.allLucidDreams.All((Type type) => type.Name != k5))
			{
				lucidDreams.Remove(k5);
			}
		}
		int numOfUnlockedHeroes = 0;
		int numOfUnlockedSkills = 0;
		int numOfUnlockedGems = 0;
		int numOfUnlockedArtifacts = 0;
		int numOfUnlockedLucidDreams = 0;
		array = heroes.Keys.ToArray();
		foreach (string k6 in array)
		{
			if (Dew.IsHeroIncludedInGame(k6))
			{
				if (lockedTargets.Contains(k6))
				{
					LockHero(k6);
					continue;
				}
				UnlockHero(k6);
				numOfUnlockedHeroes++;
			}
		}
		array = skills.Keys.ToArray();
		foreach (string k7 in array)
		{
			if (Dew.IsSkillIncludedInGame(k7))
			{
				if (lockedTargets.Contains(k7))
				{
					LockSkill(k7);
					continue;
				}
				UnlockSkill(k7);
				numOfUnlockedSkills++;
			}
		}
		array = gems.Keys.ToArray();
		foreach (string k8 in array)
		{
			if (lockedTargets.Contains(k8))
			{
				LockGem(k8);
				continue;
			}
			UnlockGem(k8);
			numOfUnlockedGems++;
		}
		array = artifacts.Keys.ToArray();
		foreach (string k9 in array)
		{
			UnlockData data = artifacts[k9];
			if (data.status == UnlockStatus.Locked)
			{
				data.status = UnlockStatus.NotDiscovered;
			}
			if (data.status == UnlockStatus.Complete)
			{
				numOfUnlockedArtifacts++;
			}
		}
		array = lucidDreams.Keys.ToArray();
		foreach (string k10 in array)
		{
			if (lockedTargets.Contains(k10))
			{
				LockLucidDream(k10);
				continue;
			}
			UnlockLucidDream(k10);
			numOfUnlockedLucidDreams++;
		}
		int numCompleted = 0;
		foreach (KeyValuePair<string, AchievementData> p3 in achievements)
		{
			DewAchievementItem a2 = (DewAchievementItem)Activator.CreateInstance(Dew.achievementsByName[p3.Key]);
			p3.Value.maxProgress = a2.GetMaxProgress();
			if (p3.Value.isCompleted)
			{
				numCompleted++;
				p3.Value.currentProgress = p3.Value.maxProgress;
				p3.Value.persistentVariables = null;
			}
			else if (p3.Value.persistentVariables == null)
			{
				p3.Value.persistentVariables = new Dictionary<string, string>();
			}
		}
		if (DewBuildProfile.current.buildType == BuildType.DemoLite)
		{
			foreach (DewStarItemOld s4 in Dew.allOldStars)
			{
				stars.TryAdd(s4.GetType().Name, new StarData());
			}
			array = stars.Keys.ToArray();
			foreach (string k11 in array)
			{
				if (!Dew.oldStarsByName.Keys.Contains(k11))
				{
					stars.Remove(k11);
				}
			}
			foreach (KeyValuePair<string, StarData> p4 in stars)
			{
				if (!Dew.IsStarIncludedInGame(p4.Key))
				{
					p4.Value.level = 0;
				}
			}
			foreach (KeyValuePair<string, StarData> s5 in stars)
			{
				s5.Value.level = Mathf.Clamp(s5.Value.level, 0, Dew.oldStarsByName[s5.Key].maxLevel);
			}
		}
		else
		{
			foreach (Type s6 in Dew.allStarTypes)
			{
				newStars.TryAdd(s6.Name, new StarData());
			}
			array = newStars.Keys.ToArray();
			foreach (string k12 in array)
			{
				if (Dew.allStarTypes.All((Type type) => type.Name != k12))
				{
					newStars.Remove(k12);
				}
			}
		}
		int numOfUnlockedStarLevels = 0;
		foreach (KeyValuePair<string, StarData> star2 in stars)
		{
			numOfUnlockedStarLevels += star2.Value.level;
		}
		int numOfAllStarLevels = 0;
		foreach (DewStarItemOld p5 in Dew.allOldStars)
		{
			numOfAllStarLevels += p5.maxLevel;
		}
		while (DewSave.profile.reverieSlots.Count > 3)
		{
			DewSave.profile.reverieSlots.RemoveAt(DewSave.profile.reverieSlots.Count - 1);
		}
		while (DewSave.profile.reverieSlots.Count < 3)
		{
			DewSave.profile.reverieSlots.Add(new DailyReverieData
			{
				type = null,
				currentProgress = 0,
				maxProgress = 0,
				grantedStardust = 0,
				persistentVariables = null,
				isComplete = false,
				nextRefillTimestamp = 0L,
				wasNeverFilled = true
			});
		}
		for (int i = DewSave.profile.reverieSlots.Count - 1; i >= 0; i--)
		{
			DailyReverieData r = DewSave.profile.reverieSlots[i];
			if (!string.IsNullOrEmpty(r.type) && !Dew.reveriesByName.ContainsKey(r.type))
			{
				DewSave.profile.reverieSlots[i] = new DailyReverieData
				{
					type = null,
					currentProgress = 0,
					maxProgress = 0,
					grantedStardust = 0,
					persistentVariables = null,
					isComplete = false,
					nextRefillTimestamp = 0L,
					wasNeverFilled = false
				};
			}
		}
		foreach (DailyReverieData r2 in DewSave.profile.reverieSlots)
		{
			if (r2.maxProgress <= 0)
			{
				r2.maxProgress = 1;
			}
			if (r2.isComplete)
			{
				r2.currentProgress = r2.maxProgress;
			}
		}
		foreach (Emote e in DewResources.FindAllByNameSubstring<Emote>("Emote_"))
		{
			if (!e.generatedFromServer && !emotes.ContainsKey(e.name))
			{
				emotes.Add(e.name, new CosmeticsData
				{
					isNew = false,
					isUnlocked = false,
					unlockDate = 0L,
					ownershipKey = null
				});
			}
		}
		foreach (Accessory a3 in DewResources.FindAllByNameSubstring<Accessory>("Acc_"))
		{
			if (!a3.generatedFromServer && !accessories.ContainsKey(a3.name))
			{
				accessories.Add(a3.name, new CosmeticsData
				{
					isNew = false,
					isUnlocked = false,
					unlockDate = 0L,
					ownershipKey = null
				});
			}
		}
		foreach (Nametag nt in DewResources.FindAllByNameSubstring<Nametag>("Nametag_"))
		{
			if (!nt.generatedFromServer && !nametags.ContainsKey(nt.name))
			{
				nametags.Add(nt.name, new CosmeticsData
				{
					isNew = false,
					isUnlocked = false,
					unlockDate = 0L,
					ownershipKey = null
				});
			}
		}
		string[] defaultUnlocks = new string[7] { "Emote_LeafPuppy_ThumbsUp", "Emote_LeafPuppy_GG", "Emote_LeafPuppy_Happy", "Emote_LeafPuppy_Hi", "Emote_LeafPuppy_Negative", "Emote_LeafPuppy_Sad", "Emote_LeafPuppy_Shocked" };
		array = defaultUnlocks;
		foreach (string u in array)
		{
			if (emotes.TryGetValue(u, out var data2) && !data2.isUnlocked)
			{
				UnlockEmote(u, null);
				emotes[u].isNew = false;
			}
		}
		if (equippedEmotes.Count == 0)
		{
			equippedEmotes.AddRange(defaultUnlocks);
		}
		while (equippedEmotes.Count > 9)
		{
			equippedEmotes.RemoveAt(equippedEmotes.Count - 1);
		}
		while (equippedEmotes.Count < 9)
		{
			equippedEmotes.Add(null);
		}
		array = heroEquippedAccs.Keys.ToArray();
		foreach (string k13 in array)
		{
			if (heroEquippedAccs[k13] == null)
			{
				heroEquippedAccs[k13] = new List<string>();
			}
			for (int i2 = heroEquippedAccs[k13].Count - 1; i2 >= 0; i2--)
			{
				if (!DewResources.database.nameToGuid.ContainsKey(heroEquippedAccs[k13][i2]))
				{
					heroEquippedAccs[k13].RemoveAt(i2);
				}
			}
		}
		array = emotes.Keys.ToArray();
		foreach (string key in array)
		{
			Emote emote = DewResources.GetByName<Emote>(key);
			if (emote == null || !emote.generatedFromServer)
			{
				continue;
			}
			if (string.IsNullOrEmpty(emotes[key].ownershipKey))
			{
				emotes.Remove(key);
				continue;
			}
			DecryptedItemData data3 = DewItem.GetDecryptedItemData(emotes[key].ownershipKey);
			if (data3 == null || data3.item != key)
			{
				emotes.Remove(key);
			}
		}
		array = accessories.Keys.ToArray();
		foreach (string key2 in array)
		{
			Accessory acc = DewResources.GetByName<Accessory>(key2);
			if (acc == null || !acc.generatedFromServer)
			{
				continue;
			}
			if (string.IsNullOrEmpty(accessories[key2].ownershipKey))
			{
				accessories.Remove(key2);
				continue;
			}
			DecryptedItemData data4 = DewItem.GetDecryptedItemData(accessories[key2].ownershipKey);
			if (data4 == null || data4.item != key2)
			{
				accessories.Remove(key2);
			}
		}
		array = nametags.Keys.ToArray();
		foreach (string key3 in array)
		{
			Nametag nt2 = DewResources.GetByName<Nametag>(key3);
			if (nt2 == null || !nt2.generatedFromServer)
			{
				continue;
			}
			if (string.IsNullOrEmpty(nametags[key3].ownershipKey))
			{
				nametags.Remove(key3);
				continue;
			}
			DecryptedItemData data5 = DewItem.GetDecryptedItemData(nametags[key3].ownershipKey);
			if (data5 == null || data5.item != key3)
			{
				nametags.Remove(key3);
			}
		}
		array = heroUnlockedStarSlots.Keys.ToArray();
		foreach (string k14 in array)
		{
			if (heroUnlockedStarSlots[k14] == null)
			{
				heroUnlockedStarSlots[k14] = new HeroStarSlotUnlockData();
			}
			Hero h;
			HeroStarSlotUnlockData d;
			if (Dew.IsHeroIncludedInGame(k14))
			{
				h = DewResources.GetByShortTypeName<Hero>(k14);
				if (!(h == null))
				{
					d = heroUnlockedStarSlots[k14];
					ValidateType(StarType.Destruction);
					ValidateType(StarType.Life);
					ValidateType(StarType.Imagination);
					ValidateType(StarType.Flexible);
				}
			}
			void ValidateType(StarType type)
			{
				List<int> list = d.Get(type);
				if (list == null)
				{
					list = new List<int>();
					d.Set(type, list);
				}
				HeroConstellationSettings s7 = h.GetConstellationSettings(type);
				List<int> unlockableSlots = new List<int>();
				for (int j = s7.defaultCount; j < s7.maxCount; j++)
				{
					unlockableSlots.Add(j);
				}
				for (int l = 0; l < list.Count; l++)
				{
					if (unlockableSlots.Contains(list[l]))
					{
						unlockableSlots.Remove(list[l]);
					}
					else
					{
						list[l] = -1;
					}
				}
				for (int m = 0; m < list.Count; m++)
				{
					if (list[m] == -1 && unlockableSlots.Count > 0)
					{
						list[m] = unlockableSlots[0];
						unlockableSlots.RemoveAt(0);
					}
				}
			}
		}
		array = heroLoadouts.Keys.ToArray();
		foreach (string heroName in array)
		{
			if (!Dew.IsHeroIncludedInGame(heroName) || DewResources.GetByShortTypeName<Hero>(heroName) == null)
			{
				continue;
			}
			if (heroLoadouts[heroName] == null)
			{
				heroLoadouts[heroName] = new List<HeroLoadoutData>();
			}
			while (heroLoadouts[heroName].Count > 5)
			{
				heroLoadouts[heroName].RemoveAt(heroLoadouts[heroName].Count - 1);
			}
			while (heroLoadouts[heroName].Count < 5)
			{
				heroLoadouts[heroName].Add(new HeroLoadoutData());
			}
			foreach (HeroLoadoutData item in heroLoadouts[heroName])
			{
				item.Validate_Imp(heroName, isRepair: true, checkStarLevels: false, heroUnlockedStarSlots[heroName]);
			}
		}
		array = heroSelectedLoadoutIndex.Keys.ToArray();
		foreach (string k15 in array)
		{
			if (heroSelectedLoadoutIndex[k15] < 0 || heroSelectedLoadoutIndex[k15] >= 5)
			{
				heroSelectedLoadoutIndex[k15] = 0;
			}
		}
		DewSave.AddMissingServerGeneratedItemsToProfile(this);
		UpdateTotalMasteryLevel();
		Debug.Log($"Profile '{name}' validated: Achievements({numCompleted}/{achievements.Count}), Heroes({numOfUnlockedHeroes}/{Dew.allHeroes.Count}), Skills({numOfUnlockedSkills}/{Dew.allSkills.Count}), Gems({numOfUnlockedGems}/{Dew.allGems.Count}), Stars({numOfUnlockedStarLevels}/{numOfAllStarLevels}), Artifacts({numOfUnlockedArtifacts}/{Dew.allArtifacts.Count}), LucidDreams({numOfUnlockedLucidDreams}/{Dew.allLucidDreams.Count})");
	}

	public void UnlockHero(string s)
	{
		UnlockData data = heroes[s];
		if (data.status != UnlockStatus.Locked)
		{
			return;
		}
		data.status = UnlockStatus.Complete;
		data.didReadMemory = false;
		data.isNewHeroOrHeroSkill = true;
		HeroSkill skill = DewResources.GetByShortTypeName<Hero>(s).GetComponent<HeroSkill>();
		List<string> associatedSkills = new List<string>();
		SkillTrigger[] loadoutSkills = skill.GetLoadoutSkills(HeroSkillLocation.Q);
		foreach (SkillTrigger sk in loadoutSkills)
		{
			associatedSkills.Add(sk.GetType().Name);
		}
		loadoutSkills = skill.GetLoadoutSkills(HeroSkillLocation.R);
		foreach (SkillTrigger sk2 in loadoutSkills)
		{
			associatedSkills.Add(sk2.GetType().Name);
		}
		loadoutSkills = skill.GetLoadoutSkills(HeroSkillLocation.Identity);
		foreach (SkillTrigger sk3 in loadoutSkills)
		{
			associatedSkills.Add(sk3.GetType().Name);
		}
		foreach (string sk4 in associatedSkills)
		{
			if (skills[sk4].status == UnlockStatus.Locked)
			{
				Type required = Dew.GetRequiredAchievementOfTarget(sk4);
				if (!(required != null) || achievements[required.Name].isCompleted)
				{
					UnlockSkill(sk4);
				}
			}
		}
	}

	public void LockHero(string s)
	{
		UnlockData data = heroes[s];
		if (data.status != UnlockStatus.Locked)
		{
			data.status = UnlockStatus.Locked;
			data.didReadMemory = false;
			data.isNewHeroOrHeroSkill = false;
		}
	}

	public void UnlockSkill(string s)
	{
		UnlockData data = skills[s];
		bool isCharacterSkill = Dew.allHeroSkills.FirstOrDefault((Type t) => t.Name == s) != null;
		if (data.status == UnlockStatus.Locked)
		{
			if (!isCharacterSkill && Dew.GetRequiredAchievementOfTarget(s) == null)
			{
				data.status = UnlockStatus.NotDiscovered;
				data.isNewHeroOrHeroSkill = false;
			}
			else
			{
				data.status = UnlockStatus.Complete;
				data.isNewHeroOrHeroSkill = isCharacterSkill;
			}
			data.didReadMemory = false;
		}
	}

	public void LockSkill(string s)
	{
		UnlockData data = skills[s];
		if (data.status != UnlockStatus.Locked)
		{
			data.status = UnlockStatus.Locked;
			data.didReadMemory = false;
			data.isNewHeroOrHeroSkill = false;
		}
	}

	public void UnlockGem(string g)
	{
		UnlockData data = gems[g];
		if (data.status == UnlockStatus.Locked)
		{
			if (Dew.GetRequiredAchievementOfTarget(g) == null)
			{
				data.status = UnlockStatus.NotDiscovered;
			}
			else
			{
				data.status = UnlockStatus.Complete;
			}
			data.didReadMemory = false;
			data.isNewHeroOrHeroSkill = false;
		}
	}

	public void LockGem(string g)
	{
		UnlockData data = gems[g];
		if (data.status != UnlockStatus.Locked)
		{
			data.status = UnlockStatus.Locked;
			data.didReadMemory = false;
			data.isNewHeroOrHeroSkill = false;
		}
	}

	public bool DiscoverGem(string g)
	{
		UnlockData data = gems[g];
		if (data.status != UnlockStatus.NotDiscovered)
		{
			return false;
		}
		data.status = UnlockStatus.Complete;
		data.didReadMemory = false;
		data.isNewHeroOrHeroSkill = false;
		return true;
	}

	public bool DiscoverSkill(string s)
	{
		UnlockData data = skills[s];
		if (data.status != UnlockStatus.NotDiscovered)
		{
			return false;
		}
		data.status = UnlockStatus.Complete;
		data.didReadMemory = false;
		data.isNewHeroOrHeroSkill = false;
		return true;
	}

	public void DiscoverArtifact(string g)
	{
		UnlockData data = artifacts[g];
		if (data.status == UnlockStatus.NotDiscovered)
		{
			data.status = UnlockStatus.Complete;
			data.didReadMemory = false;
			data.isNewHeroOrHeroSkill = false;
		}
	}

	public void UnlockLucidDream(string l)
	{
		UnlockData data = lucidDreams[l];
		if (data.status == UnlockStatus.Locked)
		{
			data.status = UnlockStatus.Complete;
			data.didReadMemory = false;
			data.isNewHeroOrHeroSkill = false;
		}
	}

	public void LockLucidDream(string l)
	{
		UnlockData data = lucidDreams[l];
		if (data.status != UnlockStatus.Locked)
		{
			data.status = UnlockStatus.Locked;
			data.didReadMemory = false;
			data.isNewHeroOrHeroSkill = false;
		}
	}

	public void UnlockEmote(string emoteName, string ownershipKey)
	{
		if (!emotes.TryGetValue(emoteName, out var data))
		{
			if (string.IsNullOrEmpty(ownershipKey))
			{
				return;
			}
			data = new CosmeticsData();
			emotes[emoteName] = data;
		}
		if (!data.isUnlocked)
		{
			data.unlockDate = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
			data.isUnlocked = true;
			data.isNew = true;
			data.ownershipKey = ownershipKey;
		}
	}

	public void LockEmote(string emoteName)
	{
		CosmeticsData data = emotes[emoteName];
		if (data.isUnlocked)
		{
			data.isUnlocked = false;
			data.isNew = false;
			data.unlockDate = 0L;
			data.ownershipKey = null;
		}
	}

	public void UnlockAccessory(string accName, string ownershipKey)
	{
		if (!accessories.TryGetValue(accName, out var data))
		{
			if (string.IsNullOrEmpty(ownershipKey))
			{
				return;
			}
			data = new CosmeticsData();
			accessories[accName] = data;
		}
		if (!data.isUnlocked)
		{
			data.unlockDate = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
			data.isUnlocked = true;
			data.isNew = true;
			data.ownershipKey = ownershipKey;
		}
	}

	public void LockAccessory(string accName)
	{
		CosmeticsData data = accessories[accName];
		if (data.isUnlocked)
		{
			data.isUnlocked = false;
			data.isNew = false;
			data.unlockDate = 0L;
			data.ownershipKey = null;
		}
	}

	public void UnlockNametag(string ntName, string ownershipKey)
	{
		if (!nametags.TryGetValue(ntName, out var data))
		{
			if (string.IsNullOrEmpty(ownershipKey))
			{
				return;
			}
			data = new CosmeticsData();
			nametags[ntName] = data;
		}
		if (!data.isUnlocked)
		{
			data.unlockDate = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
			data.isUnlocked = true;
			data.isNew = true;
			data.ownershipKey = ownershipKey;
		}
	}

	public void LockNametag(string ntName)
	{
		CosmeticsData data = nametags[ntName];
		if (data.isUnlocked)
		{
			data.isUnlocked = false;
			data.isNew = false;
			data.unlockDate = 0L;
			data.ownershipKey = null;
		}
	}

	public void UnlockServerGeneratedItem(DecryptedItemData item)
	{
		if (item != null)
		{
			if (item.item.StartsWith("Emote_"))
			{
				DewSave.profile.UnlockEmote(item.item, item.ownershipKey);
			}
			else if (item.item.StartsWith("Acc_"))
			{
				DewSave.profile.UnlockAccessory(item.item, item.ownershipKey);
			}
			else if (item.item.StartsWith("Nametag_"))
			{
				DewSave.profile.UnlockNametag(item.item, item.ownershipKey);
			}
			else
			{
				Debug.Log("Unknown item type: " + item.item);
			}
		}
	}

	public int GetLockedGemCount()
	{
		return gems.Count((KeyValuePair<string, UnlockData> g) => g.Value.status == UnlockStatus.Locked);
	}

	public int GetUnlockedGemCount()
	{
		return gems.Count((KeyValuePair<string, UnlockData> g) => g.Value.status != UnlockStatus.Locked);
	}

	public int GetLockedSkillCount()
	{
		return skills.Count((KeyValuePair<string, UnlockData> g) => g.Value.status == UnlockStatus.Locked);
	}

	public int GetUnlockedSkillCount()
	{
		return skills.Count((KeyValuePair<string, UnlockData> g) => g.Value.status != UnlockStatus.Locked);
	}

	public int GetLockedLucidDreamsCount()
	{
		return lucidDreams.Count((KeyValuePair<string, UnlockData> g) => g.Value.status == UnlockStatus.Locked);
	}

	public int GetUnlockedLucidDreamsCount()
	{
		return lucidDreams.Count((KeyValuePair<string, UnlockData> g) => g.Value.status != UnlockStatus.Locked);
	}

	public static bool ValidateProfileName(string name)
	{
		if (name.Length > 30)
		{
			return false;
		}
		if (name.Length < 1)
		{
			return false;
		}
		return true;
	}

	public void AddNewGameResult(DewGameResult newResult)
	{
		lastGameResults.Insert(0, newResult);
		while (lastGameResults.Count > 20)
		{
			lastGameResults.RemoveAt(lastGameResults.Count - 1);
		}
	}

	public void AddMasteryPoints(string heroType, long points)
	{
		if (points <= 0 || !heroMasteries.TryGetValue(heroType, out var data))
		{
			return;
		}
		data.totalPoints += points;
		data.currentPoints += points;
		while (true)
		{
			long maxPoints = Dew.GetRequiredMasteryPointsToLevelUp(data.currentLevel);
			if (data.currentPoints < maxPoints)
			{
				break;
			}
			data.currentLevel++;
			data.currentPoints -= maxPoints;
		}
		UpdateTotalMasteryLevel();
	}

	public void RemoveMasteryPoints(string heroType, long points)
	{
		if (points <= 0 || !heroMasteries.TryGetValue(heroType, out var data))
		{
			return;
		}
		data.totalPoints -= points;
		data.currentPoints -= points;
		while (data.currentPoints < 0)
		{
			data.currentLevel--;
			if (data.currentLevel == -1)
			{
				data.currentLevel = 0;
				data.currentPoints = 0L;
				break;
			}
			data.currentPoints += Dew.GetRequiredMasteryPointsToLevelUp(data.currentLevel);
		}
		UpdateTotalMasteryLevel();
	}

	private void UpdateTotalMasteryLevel()
	{
		totalMasteryLevel = 0;
		foreach (KeyValuePair<string, HeroMasteryData> heroMastery in heroMasteries)
		{
			totalMasteryLevel += heroMastery.Value.currentLevel;
		}
	}
}
