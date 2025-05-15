using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DewInternal;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using EpicTransport;
using IngameDebugConsole;
using Mirror;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityJSON;
using VolumetricFogAndMist2;

public static class ConsoleCommands
{
	public struct Ad_Blue
	{
		public StatBonus bonus;
	}

	public struct Ad_Red
	{
		public StatBonus bonus;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Ad_OneHit
	{
	}

	public struct Ad_Buddha
	{
		public StatusEffect eff;
	}

	public class DebugGameSave
	{
		public struct PlayerData
		{
			public string name;

			public int gold;

			public int dreamDust;

			public HeroData hero;

			public PlayerData(DewPlayer player)
			{
				name = player.playerName;
				gold = player.gold;
				dreamDust = player.dreamDust;
				hero = new HeroData(player.hero);
			}
		}

		public struct HeroData
		{
			public Vector3 heroPos;

			public Quaternion heroRot;

			public string heroName;

			public int level;

			public HeroLoadoutData loadout;

			public List<SkillData> skills;

			public List<GemData> gems;

			public StatBonus chaosStats;

			public float heroHealth;

			public HeroData(Hero hero)
			{
				heroName = hero.GetType().Name;
				level = hero.level;
				loadout = hero.loadout;
				skills = new List<SkillData>();
				if (hero.Skill.Q != null)
				{
					skills.Add(new SkillData(HeroSkillLocation.Q, hero.Skill.Q));
				}
				if (hero.Skill.W != null)
				{
					skills.Add(new SkillData(HeroSkillLocation.W, hero.Skill.W));
				}
				if (hero.Skill.E != null)
				{
					skills.Add(new SkillData(HeroSkillLocation.E, hero.Skill.E));
				}
				if (hero.Skill.R != null)
				{
					skills.Add(new SkillData(HeroSkillLocation.R, hero.Skill.R));
				}
				if (hero.Skill.Identity != null)
				{
					skills.Add(new SkillData(HeroSkillLocation.Identity, hero.Skill.Identity));
				}
				if (hero.Skill.Movement != null)
				{
					skills.Add(new SkillData(HeroSkillLocation.Movement, hero.Skill.Movement));
				}
				gems = new List<GemData>();
				foreach (KeyValuePair<GemLocation, Gem> p in hero.Skill.gems)
				{
					gems.Add(new GemData(p.Key, p.Value));
				}
				chaosStats = null;
				if (hero.TryGetData<Shrine_Chaos.Ad_ChaosStats>(out var stat))
				{
					chaosStats = stat.bonus;
				}
				heroHealth = hero.currentHealth;
				heroPos = hero.position;
				heroRot = hero.rotation;
			}
		}

		public struct SkillData
		{
			public HeroSkillLocation location;

			public string name;

			public int level;

			public SkillData(HeroSkillLocation type, SkillTrigger skill)
			{
				location = type;
				name = skill.GetType().Name;
				level = skill.level;
			}
		}

		public struct GemData
		{
			public GemLocation location;

			public string name;

			public int quality;

			public GemData(GemLocation type, Gem gem)
			{
				location = type;
				name = gem.GetType().Name;
				quality = gem.quality;
			}
		}

		public string zone;

		public string room;

		public int ambientLevel;

		public string difficulty;

		public int roomIndex;

		public int currentZoneClearedNodes;

		public float elapsedGameTime;

		public PlayerData[] players;

		public static DebugGameSave Capture()
		{
			DebugGameSave newSave = new DebugGameSave();
			newSave.zone = NetworkedManagerBase<ZoneManager>.instance.currentZone.name;
			newSave.room = SceneManager.GetActiveScene().name;
			newSave.ambientLevel = NetworkedManagerBase<GameManager>.instance.ambientLevel;
			newSave.elapsedGameTime = NetworkedManagerBase<GameManager>.instance.elapsedGameTime;
			newSave.roomIndex = NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex;
			newSave.currentZoneClearedNodes = NetworkedManagerBase<ZoneManager>.instance.currentZoneClearedNodes;
			newSave.difficulty = NetworkedManagerBase<GameManager>.instance.difficulty.name;
			newSave.players = new PlayerData[DewPlayer.humanPlayers.Count];
			for (int i = 0; i < DewPlayer.humanPlayers.Count; i++)
			{
				newSave.players[i] = new PlayerData(DewPlayer.humanPlayers[i]);
			}
			return newSave;
		}
	}

	private struct Ad_AtkSpd
	{
		public StatBonus bonus;
	}

	private struct Ad_CritChance
	{
		public StatBonus bonus;
	}

	private struct Ad_Color
	{
		public EntityColorModifier modifier;
	}

	private struct Ad_Transform
	{
		public EntityTransformModifier modifier;
	}

	private struct Ad_DealtDmg
	{
		public float multiplier;
	}

	private struct Ad_TakenDmg
	{
		public float multiplier;
	}

	[DewConsoleMethod("Remove current artifact", CommandType.GameServerCheat)]
	public static void Artifact()
	{
		if (NetworkedManagerBase<QuestManager>.instance.currentArtifact == null)
		{
			Log("Current artifact is null");
			return;
		}
		Log("Removed current artifact: " + NetworkedManagerBase<QuestManager>.instance.currentArtifact);
		NetworkedManagerBase<QuestManager>.instance.RemoveArtifact();
	}

	[DewConsoleMethod("Set current artifact", CommandType.GameServerCheat)]
	public static void Artifact(string substr)
	{
		Artifact artifact = DewResources.FindOneByTypeSubstring<Artifact>(substr);
		if (artifact == null)
		{
			Log("Could not find artifact with substr: " + substr);
			return;
		}
		Log("Set current artifact to " + artifact.GetType().Name);
		NetworkedManagerBase<QuestManager>.instance.PickUpArtifact(artifact.GetType().Name, GetPlayer(), GetPlayer().hero.Visual.GetCenterPosition());
	}

	[ConsoleMethod("autoexec", "Add command to auto-exec of certain key", new string[] { })]
	public static void AutoExec(string key, string command)
	{
		if (TryResolveAutoExecKey(key, out var k))
		{
			string prev = PlayerPrefs.GetString("AutoExec_" + k, "");
			prev = prev + "\n" + command;
			PlayerPrefs.SetString("AutoExec_" + k, prev);
			PlayerPrefs.Save();
			Log($"Added auto-exec \"{command}\" to {k}");
			AutoExecList();
		}
	}

	[ConsoleMethod("autoexeclist", "List commands in all auto-exec", new string[] { })]
	public static void AutoExecList()
	{
		Enum.GetNames(typeof(ConsoleManager.AutoExecKey));
		ConsoleManager.AutoExecKey[] array = (ConsoleManager.AutoExecKey[])Enum.GetValues(typeof(ConsoleManager.AutoExecKey));
		for (int i = 0; i < array.Length; i++)
		{
			ConsoleManager.AutoExecKey k = array[i];
			string text = PlayerPrefs.GetString("AutoExec_" + k, "");
			Log($"Auto-exec of key {k}:");
			string[] array2 = text.Split("\n");
			foreach (string p in array2)
			{
				if (!string.IsNullOrWhiteSpace(p.Trim()))
				{
					Log(" - " + p);
				}
			}
		}
	}

	[ConsoleMethod("autoexecclear", "Clear auto-exec of certain key", new string[] { })]
	public static void AutoExecClear(string key)
	{
		if (TryResolveAutoExecKey(key, out var k))
		{
			PlayerPrefs.SetString("AutoExec_" + k, "");
			PlayerPrefs.Save();
			Log("Cleared auto-exec of " + k);
		}
	}

	[ConsoleMethod("autoexecclearall", "Clear auto-exec of all keys", new string[] { })]
	public static void AutoExecClearAll()
	{
		ConsoleManager.AutoExecKey[] values = (ConsoleManager.AutoExecKey[])Enum.GetValues(typeof(ConsoleManager.AutoExecKey));
		ConsoleManager.AutoExecKey[] array = values;
		for (int i = 0; i < array.Length; i++)
		{
			ConsoleManager.AutoExecKey k = array[i];
			PlayerPrefs.SetString("AutoExec_" + k, "");
		}
		PlayerPrefs.Save();
		Log("Cleared all auto-exec: " + values.JoinToString(", "));
	}

	private static bool TryResolveAutoExecKey(string str, out ConsoleManager.AutoExecKey key)
	{
		string[] names = Enum.GetNames(typeof(ConsoleManager.AutoExecKey));
		ConsoleManager.AutoExecKey[] values = (ConsoleManager.AutoExecKey[])Enum.GetValues(typeof(ConsoleManager.AutoExecKey));
		for (int i = 0; i < names.Length; i++)
		{
			if (names[i].Equals(str, StringComparison.InvariantCultureIgnoreCase))
			{
				key = values[i];
				return true;
			}
		}
		Log("Invalid key provided, possible keys: " + names.JoinToString(", "));
		key = ConsoleManager.AutoExecKey.Global;
		return false;
	}

	public static void Log(object message)
	{
		string msg = message.ToString();
		if (NetworkedManagerBase<ConsoleManager>.instance == null || NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			Debug.Log(msg);
		}
		else
		{
			NetworkedManagerBase<ConsoleManager>.instance.executionContext.player.SendLog(msg);
		}
	}

	public static void LogWarning(object message)
	{
		string msg = message.ToString();
		if (NetworkedManagerBase<ConsoleManager>.instance == null || NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			Debug.Log(msg);
		}
		else
		{
			NetworkedManagerBase<ConsoleManager>.instance.executionContext.player.SendLogWarning(msg);
		}
	}

	public static void LogError(object message)
	{
		string msg = message.ToString();
		if (NetworkedManagerBase<ConsoleManager>.instance == null || NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			Debug.Log(msg);
		}
		else
		{
			NetworkedManagerBase<ConsoleManager>.instance.executionContext.player.SendLogError(msg);
		}
	}

	public static void PrintNoEntitySelected()
	{
		Log("No entity selected");
	}

	public static void PrintNoEntityControlled()
	{
		Log("No entity controlled");
	}

	public static Entity GetConsoleSelectedEntity()
	{
		if (NetworkedManagerBase<ConsoleManager>.instance.executionContext.player != null)
		{
			return NetworkedManagerBase<ConsoleManager>.instance.executionContext.selection;
		}
		return NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity;
	}

	public static bool TryGetConsoleSelectedEntity(out Entity ent)
	{
		ent = ((NetworkedManagerBase<ConsoleManager>.instance.executionContext.player != null) ? NetworkedManagerBase<ConsoleManager>.instance.executionContext.selection : NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity);
		return ent != null;
	}

	public static Entity GetControllingEntity()
	{
		if (NetworkedManagerBase<ConsoleManager>.instance.executionContext.player != null)
		{
			return NetworkedManagerBase<ConsoleManager>.instance.executionContext.player.controllingEntity;
		}
		return ManagerBase<ControlManager>.instance.controllingEntity;
	}

	public static Vector3 GetCursorWorldPos()
	{
		if (NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			return ControlManager.GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false);
		}
		return NetworkedManagerBase<ConsoleManager>.instance.executionContext.cursorWorldPos;
	}

	public static DewPlayer GetPlayer()
	{
		if (NetworkedManagerBase<ConsoleManager>.instance.executionContext.player == null)
		{
			return DewPlayer.local;
		}
		return NetworkedManagerBase<ConsoleManager>.instance.executionContext.player;
	}

	[DewConsoleMethod("Bind specific command to specific keyboard key.", CommandType.Game)]
	public static void Bind(string key, string command)
	{
		ConsoleBindItemType type = ConsoleBindItemType.Down;
		if (key.StartsWith("+"))
		{
			key = key.Substring(1);
			type = ConsoleBindItemType.Down;
		}
		else if (key.StartsWith("-"))
		{
			key = key.Substring(1);
			type = ConsoleBindItemType.Up;
		}
		if (TryResolveKeyCode(key, out var code))
		{
			NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Add(new ConsoleBindItem
			{
				type = type,
				key = code,
				command = command
			});
			Log(string.Format("Added bind {0}{1}: {2}", (type == ConsoleBindItemType.Down) ? "+" : "-", code, command));
		}
	}

	[DewConsoleMethod("List all binds currently active.", CommandType.Game)]
	public static void BindList()
	{
		if (NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count == 0)
		{
			Log("No console command binds are active.");
			return;
		}
		Log($"Currently {NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count} console command binds are active.");
		foreach (ConsoleBindItem p in NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds)
		{
			Log(string.Format(" - {0}{1}: {2}", (p.type == ConsoleBindItemType.Down) ? "+" : "-", p.key, p.command));
		}
	}

	[DewConsoleMethod("Remove bind from specific key.", CommandType.Game)]
	public static void Unbind(string key)
	{
		ConsoleBindItemType type = ConsoleBindItemType.Down;
		if (key.StartsWith("+"))
		{
			key = key.Substring(1);
			type = ConsoleBindItemType.Down;
		}
		else if (key.StartsWith("-"))
		{
			key = key.Substring(1);
			type = ConsoleBindItemType.Up;
		}
		if (!TryResolveKeyCode(key, out var code))
		{
			return;
		}
		bool didRemove = false;
		for (int i = NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count - 1; i >= 0; i--)
		{
			ConsoleBindItem b = NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds[i];
			if (b.key == code && b.type == type)
			{
				didRemove = true;
				Log(string.Format("Removing {0}{1}: {2}", (b.type == ConsoleBindItemType.Down) ? "+" : "-", b.key, b.command));
				NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.RemoveAt(i);
			}
		}
		if (!didRemove)
		{
			Log(string.Format("Could not find any binds on {0}{1}", (type == ConsoleBindItemType.Down) ? "+" : "-", code));
		}
	}

	[DewConsoleMethod("Remove all binds.", CommandType.Game)]
	public static void UnbindAll()
	{
		if (NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count == 0)
		{
			Log("No console command binds to remove.");
			return;
		}
		Log($"Removed {NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Count} console command binds.");
		NetworkedManagerBase<ConsoleManager>.instance.activeCommandBinds.Clear();
	}

	private static bool TryResolveKeyCode(string str, out KeyCode code)
	{
		string[] names = Enum.GetNames(typeof(KeyCode));
		KeyCode[] values = (KeyCode[])Enum.GetValues(typeof(KeyCode));
		for (int i = 0; i < names.Length; i++)
		{
			if (names[i].Equals(str, StringComparison.InvariantCultureIgnoreCase))
			{
				code = values[i];
				return true;
			}
		}
		code = KeyCode.None;
		List<string> possible = new List<string>();
		for (int j = 0; j < names.Length; j++)
		{
			if (names[j].Contains(str, StringComparison.InvariantCultureIgnoreCase))
			{
				possible.Add(names[j]);
			}
		}
		if (possible.Count == 0)
		{
			Log("Cannot find requested keycode.");
		}
		else
		{
			Log("Cannot find requested keycode, did you mean: " + possible.JoinToString(", "));
		}
		return false;
	}

	[DewConsoleMethod("Create chaos of given rarity.", CommandType.GameServerCheat)]
	public static void Chaos(string rarityStr)
	{
		foreach (object v in Enum.GetValues(typeof(Rarity)))
		{
			Rarity rarity = (Rarity)v;
			if (rarity.ToString().Contains(rarityStr, StringComparison.InvariantCultureIgnoreCase))
			{
				Dew.CreateActor(Dew.GetGoodRewardPosition(GetCursorWorldPos()), null, null, delegate(Shrine_Chaos chaos)
				{
					chaos.Networkrarity = rarity;
				});
				return;
			}
		}
		Log("Cannot find rarity with substr: " + rarityStr);
	}

	[DewConsoleMethod("Clear room.", CommandType.GameServerCheat)]
	public static void Clear()
	{
		if (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
		{
			Log("Room is already cleared");
			return;
		}
		foreach (Entity ent in new List<Entity>(NetworkedManagerBase<ActorManager>.instance.allEntities))
		{
			if (ent.isActive && ent is Monster)
			{
				ent.Kill();
			}
		}
		SingletonDewNetworkBehaviour<Room>.instance.monsters.FinishAllOngoingSpawns();
		foreach (RoomSection section in SingletonDewNetworkBehaviour<Room>.instance.sections)
		{
			section.monsters.combatAreaSettings = SectionCombatAreaType.No;
			section.monsters.isMarkedAsCombatArea = false;
			section.monsters.didClearCombatArea = true;
		}
		SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
	}

	[DewConsoleMethod("Reset selected entity's ability cooldowns and gem cooldowns.", CommandType.GameServerCheat)]
	public static void Reset()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		foreach (KeyValuePair<int, AbilityTrigger> ability in ent.Ability.abilities)
		{
			ability.Value.ResetCooldown();
		}
		if (!(ent is Hero h))
		{
			return;
		}
		foreach (KeyValuePair<GemLocation, Gem> g in h.Skill.gems)
		{
			if (g.Value.isCooldownEnabled || g.Value.isRateLimited)
			{
				g.Value.ResetCooldown();
			}
		}
	}

	[DewConsoleMethod("Kill selected entity", CommandType.GameServerCheat)]
	public static void Kill()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		Log("Killing " + ent.name);
		ent.Kill();
	}

	[DewConsoleMethod("Kill all monsters", CommandType.GameServerCheat)]
	public static void KillAll()
	{
		foreach (Entity ent in new List<Entity>(NetworkedManagerBase<ActorManager>.instance.allEntities))
		{
			if (ent.isActive && ent is Monster)
			{
				Log("Killing " + ent.name);
				ent.Kill();
			}
		}
	}

	[DewConsoleMethod("Kill all entities by substring of their type", CommandType.GameServerCheat)]
	public static void KillAll(string substr)
	{
		Entity[] array = NetworkedManagerBase<ActorManager>.instance.allEntities.ToArray();
		foreach (Entity ent in array)
		{
			if (ent.isActive && ent.GetType().Name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
			{
				Log("Killing " + ent.name);
				ent.Kill();
			}
		}
	}

	[DewConsoleMethod("Destroy all actors by substring of their type", CommandType.GameServerCheat)]
	public static void DestroyAll(string substr)
	{
		Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
		foreach (Actor a in array)
		{
			if (a.isActive && a.GetType().Name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
			{
				Log("Destroying " + a.name);
				a.Destroy();
			}
		}
	}

	[DewConsoleMethod("Amplify damages from controlled entity by 99x", CommandType.GameServerCheat)]
	public static void OneHit()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else if (ent.HasData<Ad_OneHit>())
		{
			ent.RemoveData<Ad_OneHit>();
			ent.dealtDamageProcessor.Remove(AmplifyDamageBy);
			Log("One hit off");
		}
		else
		{
			ent.AddData(default(Ad_OneHit));
			ent.dealtDamageProcessor.Add(AmplifyDamageBy);
			Log("One hit on");
		}
		static void AmplifyDamageBy(ref DamageData data, Actor actor, Entity target)
		{
			if (!actor.IsDescendantOf(target))
			{
				data.ApplyAmplification(1000f);
			}
		}
	}

	[DewConsoleMethod("Grant 1000 mana regen and 1000 ability haste to selected entity", CommandType.GameServerCheat)]
	public static void Blue()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.TryGetData<Ad_Blue>(out var data))
		{
			ent.Status.RemoveStatBonus(data.bonus);
			Log($"Blue off for {ent}");
			ent.RemoveData<Ad_Blue>();
			return;
		}
		StatBonus bonus = new StatBonus
		{
			manaRegenFlat = 100000f,
			abilityHasteFlat = 100000f
		};
		ent.Status.AddStatBonus(bonus);
		ent.AddData(new Ad_Blue
		{
			bonus = bonus
		});
		Log($"Blue on for {ent}");
	}

	[DewConsoleMethod("Grant 1000 health regen to selected entity", CommandType.GameServerCheat)]
	public static void Red()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.TryGetData<Ad_Red>(out var data))
		{
			ent.Status.RemoveStatBonus(data.bonus);
			Log($"Red off for {ent}");
			ent.RemoveData<Ad_Red>();
			return;
		}
		StatBonus bonus = new StatBonus
		{
			healthRegenFlat = 100000f,
			maxHealthPercentage = 10000f
		};
		ent.Status.AddStatBonus(bonus);
		ent.AddData(new Ad_Red
		{
			bonus = bonus
		});
		Log($"Red on for {ent}");
	}

	[DewConsoleMethod("Knock up selected entity by given strength", CommandType.GameServerCheat)]
	public static void KnockUp(float strength)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			ent.Visual.KnockUp(strength, isFriendly: true);
		}
	}

	[DewConsoleMethod("Prevent death of selected entity", CommandType.GameServerCheat)]
	public static void Buddha()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.TryGetData<Ad_Buddha>(out var data))
		{
			data.eff.Destroy();
			Log($"Buddha off for {ent}");
			ent.RemoveData<Ad_Buddha>();
			return;
		}
		Se_GenericEffectContainer eff = ent.CreateBasicEffect(ent, new DeathInterruptEffect
		{
			onInterrupt = delegate
			{
				HealData heal = new HealData(1f);
				ent.DoHeal(heal, ent);
			}
		}, float.PositiveInfinity, "console_buddha");
		ent.AddData(new Ad_Buddha
		{
			eff = eff
		});
		Log($"Buddha on for {ent}");
	}

	[DewConsoleMethod("Set health of selected entity", CommandType.GameServerCheat)]
	public static void Health(float amount)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			ent.Status.SetHealth(amount);
		}
	}

	[DewConsoleMethod("Set mana of selected entity", CommandType.GameServerCheat)]
	public static void Mana(float amount)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			ent.Status.SetMana(amount);
		}
	}

	[DewConsoleMethod("Revive my hero", CommandType.GameServerCheat)]
	public static void Revive()
	{
		Hero hero = GetConsoleSelectedEntity() as Hero;
		if (hero == null || !hero.Status.TryGetStatusEffect<Se_HeroKnockedOut>(out var knock))
		{
			Log("Hero is not knocked out");
			return;
		}
		knock.Destroy();
		hero.Status.SetHealth(hero.maxHealth);
		if (hero.position.y < -100f)
		{
			hero.Control.Teleport(SingletonDewNetworkBehaviour<Room>.instance.GetHeroSpawnPosition());
		}
	}

	[DewConsoleMethod("Drop exp at cursor position", CommandType.GameServerCheat)]
	public static void Exp(int amount)
	{
		NetworkedManagerBase<PickupManager>.instance.DropExp(amount, GetCursorWorldPos());
	}

	[DewConsoleMethod("GOLDSSS", CommandType.GameServerCheat)]
	public static void Gold(int amount)
	{
		DewPlayer.local.gold = amount;
	}

	[DewConsoleMethod("DreamDUSTS", CommandType.GameServerCheat)]
	public static void DreamDust(int amount)
	{
		DewPlayer.local.dreamDust = amount;
	}

	[DewConsoleMethod("Make selected player broke", CommandType.GameServerCheat)]
	public static void Broke()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.owner == null)
		{
			Log("Selected entity not owned by a player");
			return;
		}
		ent.owner.gold = 0;
		ent.owner.dreamDust = 0;
		Log($"Successfully made {ent.owner} broke");
	}

	[DewConsoleMethod("Damage selected entity", CommandType.GameServerCheat)]
	public static void Damage(float amount)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.DealDamage(new DamageData(DamageData.SourceType.Default, amount, 1f), ent);
		}
	}

	[DewConsoleMethod("Grant shield", CommandType.GameServerCheat)]
	public static void Shield(float amount, float duration)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.GiveShield(ent, amount, duration);
		}
	}

	[DewConsoleMethod("Grant shield", CommandType.GameServerCheat)]
	public static void ShieldDecay(float amount, float duration)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.GiveShield(ent, amount, duration, isDecay: true);
		}
	}

	[DewConsoleMethod("Grant invulnerability", CommandType.GameServerCheat)]
	public static void Invul(float duration)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateBasicEffect(ent, new InvulnerableEffect(), duration, "ConsoleInvul", DuplicateEffectBehavior.DoNothing);
		}
	}

	[DewConsoleMethod("Grant protected", CommandType.GameServerCheat)]
	public static void Protect(float duration)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateBasicEffect(ent, new ProtectedEffect(), duration, "ConsoleProt", DuplicateEffectBehavior.DoNothing);
		}
	}

	[DewConsoleMethod("Get random skills and gems", CommandType.GameServerCheat)]
	public static void RandomEquipment()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (!(ent is Hero h))
		{
			Log("Selected entity is not hero");
			return;
		}
		GetRandomSkill(out var type, out var level);
		SetQ(type, level);
		GetRandomSkill(out type, out level);
		SetW(type, level);
		GetRandomSkill(out type, out level);
		SetE(type, level);
		GetRandomSkill(out type, out level);
		SetR(type, level);
		GemLocation[] array = h.Skill.gems.Keys.ToArray();
		foreach (GemLocation loc in array)
		{
			h.Skill.UnequipGem(loc, Vector3.zero).Destroy();
		}
		foreach (HeroSkillLocation s in new List<HeroSkillLocation>
		{
			HeroSkillLocation.Q,
			HeroSkillLocation.W,
			HeroSkillLocation.E,
			HeroSkillLocation.R
		})
		{
			for (int j = 0; j < 3; j++)
			{
				GetRandomGem(out type, out level);
				GemLocation loc2;
				Gem gem;
				while (h.Skill.TryGetEquippedGemOfSameType(DewResources.database.typeNameToType[type], out loc2, out gem))
				{
					GetRandomGem(out type, out level);
				}
				h.Skill.EquipGem(new GemLocation(s, j), Dew.CreateGem(DewResources.GetByShortTypeName<Gem>(type), Vector3.zero, level));
			}
		}
		static void GetRandomGem(out string reference, out int quality)
		{
			Loot_Gem lootInstance = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>();
			Rarity rarity = lootInstance.SelectRarityNormal();
			lootInstance.SelectGemAndQuality(rarity, out var gem2, out quality);
			reference = gem2.GetType().Name;
		}
		static void GetRandomSkill(out string reference, out int level2)
		{
			Loot_Skill lootInstance = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Skill>();
			Rarity rarity = lootInstance.SelectRarityNormal();
			lootInstance.SelectSkillAndLevel(rarity, out var skill, out level2);
			reference = skill.GetType().Name;
		}
	}

	[DewConsoleMethod("Make an entity say some lines", CommandType.GameServerCheat)]
	public static void MakeSay(string key)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
		{
			startConversationKey = key,
			player = GetPlayer(),
			speakers = new Entity[1] { ent }
		});
	}

	[DewConsoleMethod("Make nearby entities say some lines", CommandType.GameServerCheat)]
	public static void MakeSayNearby(string key)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> ents = DewPhysics.OverlapCircleAllEntities(out handle, ent.agentPosition, 5f, new CollisionCheckSettings
		{
			includeUncollidable = true
		});
		NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
		{
			startConversationKey = key,
			player = GetPlayer(),
			speakers = ents.ToArray()
		});
		handle.Return();
	}

	[DewConsoleMethod("Test monster spawn pos", CommandType.Game)]
	public static void TestMonsterSpawn(int count)
	{
		if (GetPlayer().hero.section == null)
		{
			Log("Need to be on section");
			return;
		}
		Mon_RedGiant mon = DewResources.GetByType<Mon_RedGiant>();
		for (int i = 0; i < count; i++)
		{
			Vector3 item = SingletonDewNetworkBehaviour<Room>.instance.monsters.GetSpawnMonsterPosRot(new SpawnMonsterSettings
			{
				rule = SingletonDewNetworkBehaviour<Room>.instance.monsters.defaultRule,
				section = GetPlayer().hero.section
			}, mon).Item1;
			Debug.DrawLine(item, item + Vector3.up * 5f, Color.green, 3f);
		}
	}

	[DewConsoleMethod("Make entity look at cursor", CommandType.GameServerCheat)]
	public static void Look()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			ent.Control.RotateTowards(GetCursorWorldPos(), immediately: true, 3600f);
		}
	}

	[DewConsoleMethod("Select entity by substring of their type", CommandType.Game)]
	public static void Sel(string substr)
	{
		foreach (Entity e in NetworkedManagerBase<ActorManager>.instance.allEntities.Reverse())
		{
			if (e.GetType().Name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
			{
				NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity = e;
				Log("Selected " + e.GetActorReadableName());
				return;
			}
		}
		Log("No entity found with substr: " + substr);
	}

	[DewConsoleMethod("Select newest entity", CommandType.Game)]
	public static void SelNewest()
	{
		Entity e = NetworkedManagerBase<ActorManager>.instance.allEntities[^1];
		NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity = e;
		Log("Selected " + e.GetActorReadableName());
	}

	[DewConsoleMethod("Select my hero", CommandType.Game)]
	public static void Sel()
	{
		Hero e = DewPlayer.local.hero;
		NetworkedManagerBase<ConsoleManager>.instance.localSelectedEntity = e;
		Log("Selected " + e.GetActorReadableName());
	}

	[DewConsoleMethod("Enable renderer locally", CommandType.Game)]
	public static void RendererEnable()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			ent.Visual.EnableRenderersLocal();
		}
	}

	[DewConsoleMethod("Disable renderer locally", CommandType.Game)]
	public static void RendererDisable()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			ent.Visual.DisableRenderersLocal();
		}
	}

	[DewConsoleMethod("Auto-cast specific ability", CommandType.GameServerCheat)]
	public static void Cast(string substr)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		AbilityTrigger abil = null;
		foreach (KeyValuePair<int, AbilityTrigger> p in ent.Ability.abilities)
		{
			if (p.Value.GetType().Name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
			{
				abil = p.Value;
				break;
			}
		}
		if (abil == null)
		{
			Log("Could not find specified trigger");
			return;
		}
		CastInfo info = new CastInfo
		{
			caster = ent
		};
		if (abil.currentConfig.castMethod.type != CastMethodType.None)
		{
			Entity target = null;
			if (ent.AI.context.targetEnemy != null && abil.currentConfig.targetValidator.Evaluate(ent, ent.AI.context.targetEnemy))
			{
				target = ent.AI.context.targetEnemy;
			}
			else
			{
				ArrayReturnHandle<Entity> handle;
				ReadOnlySpan<Entity> readOnlySpan = DewPhysics.OverlapCircleAllEntities(out handle, ent.position, 20f, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.DistanceFromCenter
				});
				for (int i = 0; i < readOnlySpan.Length; i++)
				{
					Entity candidate = readOnlySpan[i];
					if (abil.currentConfig.targetValidator.Evaluate(ent, candidate))
					{
						target = candidate;
						break;
					}
				}
				handle.Return();
			}
			if (abil.currentConfig.castMethod.type == CastMethodType.Target && target == null)
			{
				Log("Valid target not found");
				return;
			}
			info = abil.GetPredictedCastInfoToTarget(target);
		}
		if (abil.currentConfig.maxCharges <= 0)
		{
			abil.currentConfig.maxCharges = 1;
		}
		if (abil.currentConfigCurrentCharge <= 0)
		{
			abil.SetCharge(abil.currentConfigIndex, 1);
		}
		if (!ent.AI.disableAI)
		{
			ent.AI.StartCoroutine(Routine());
		}
		ent.Control.Stop();
		ent.Control.CancelOngoingChannels();
		ent.Control.Cast(abil, info);
		Log("Trying to cast " + abil.GetActorReadableName());
		IEnumerator Routine()
		{
			ent.AI.disableAI = true;
			yield return new WaitForSeconds(1f);
			ent.AI.disableAI = false;
		}
	}

	[DewConsoleMethod("Discard and change your current hero, with same level as your hero", CommandType.GameServerCheat)]
	public static void Hero(string substr)
	{
		Hero ent = DewResources.FindOneByTypeSubstring<Hero>(substr);
		if (ent == null)
		{
			Log("Could not find hero with query: " + substr);
			return;
		}
		Hero prev = GetPlayer().hero;
		HeroLoadoutData loadout = DewSave.profile.heroLoadouts[ent.GetType().Name][DewSave.profile.heroSelectedLoadoutIndex[ent.GetType().Name]];
		Hero spawned = Dew.SpawnHero(ent, prev.position, prev.rotation, prev.owner, prev.level, loadout);
		Dew.Destroy(prev.gameObject);
		GetPlayer().hero = spawned;
		GetPlayer().controllingEntity = spawned;
	}

	[DewConsoleMethod("Discard and change your current hero", CommandType.GameServerCheat)]
	public static void Hero(string substr, int level)
	{
		Hero ent = DewResources.FindOneByTypeSubstring<Hero>(substr);
		if (ent == null)
		{
			Log("Could not find hero with query: " + substr);
			return;
		}
		Hero prev = GetPlayer().hero;
		Hero spawned = Dew.SpawnHero(ent, prev.position, prev.rotation, prev.owner, level, new HeroLoadoutData());
		Dew.Destroy(prev.gameObject);
		GetPlayer().hero = spawned;
		GetPlayer().controllingEntity = spawned;
	}

	[DewConsoleMethod("Save status of current game", CommandType.Game)]
	public static void SaveGame()
	{
		SaveGame("default");
	}

	[DewConsoleMethod("Save status of current game", CommandType.Game)]
	public static void SaveGame(string key)
	{
		if (!NetworkServer.active)
		{
			LogWarning("Warning: SaveGame is intended to be run on server, saved game might be incomplete...");
		}
		string strData = JSON.Serialize(DebugGameSave.Capture());
		PlayerPrefs.SetString("debuggamesave_" + key, strData);
		PlayerPrefs.Save();
		Log("Saved game as '" + key + "'");
		Log(strData);
	}

	[DewConsoleMethod("Load hero save", CommandType.GameCheat)]
	public static void LoadHero()
	{
		LoadHero("default");
	}

	[DewConsoleMethod("Load hero save", CommandType.GameCheat)]
	public static void LoadHero(string key)
	{
		string saveStr = PlayerPrefs.GetString("debuggamesave_" + key, JSON.Serialize(new DebugGameSave()));
		DebugLogConsole.ExecuteCommand("loadherojson {" + saveStr + "}");
	}

	[DewConsoleMethod("Load hero save", CommandType.GameServerCheat)]
	public static void LoadHeroJson(string saveStr)
	{
		DebugGameSave.PlayerData[] players = JSON.Deserialize<DebugGameSave>(saveStr).players;
		for (int i = 0; i < players.Length; i++)
		{
			LoadPlayer_Imp(players[i]);
		}
	}

	[DewConsoleMethod("Load world save", CommandType.GameCheat)]
	public static void LoadWorld()
	{
		LoadWorld("default");
	}

	[DewConsoleMethod("Load world save", CommandType.GameCheat)]
	public static void LoadWorld(string key)
	{
		string saveStr = PlayerPrefs.GetString("debuggamesave_" + key, JSON.Serialize(new DebugGameSave()));
		DebugLogConsole.ExecuteCommand("loadworldjson {" + saveStr + "}");
	}

	[DewConsoleMethod("Load hero save", CommandType.GameServerCheat)]
	public static void LoadWorldJson(string saveStr)
	{
		LoadWorld_Imp(JSON.Deserialize<DebugGameSave>(saveStr));
	}

	[DewConsoleMethod("Load game", CommandType.GameCheat)]
	public static void LoadGame()
	{
		LoadGame("default");
	}

	[DewConsoleMethod("Load game save", CommandType.GameCheat)]
	public static void LoadGame(string key)
	{
		string saveStr = PlayerPrefs.GetString("debuggamesave_" + key, JSON.Serialize(new DebugGameSave()));
		DebugLogConsole.ExecuteCommand("loadgamejson {" + saveStr + "}");
	}

	[DewConsoleMethod("Load game save", CommandType.GameServerCheat)]
	public static void LoadGameJson(string saveStr)
	{
		DebugGameSave save = JSON.Deserialize<DebugGameSave>(saveStr);
		LoadWorld_Imp(save);
		DebugGameSave.PlayerData[] players = save.players;
		for (int i = 0; i < players.Length; i++)
		{
			LoadPlayer_Imp(players[i]);
		}
		NetworkedManagerBase<GameManager>.instance.StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(1.1f);
			yield return new WaitWhile(() => NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition);
			DebugGameSave.PlayerData[] players2 = save.players;
			for (int num = 0; num < players2.Length; num++)
			{
				DebugGameSave.PlayerData pd = players2[num];
				DewPlayer p = LoadPlayer_Imp_GetPlayer(pd);
				if (!(p == null))
				{
					p.hero.Control.Teleport(pd.hero.heroPos);
					p.hero.Control.Rotate(pd.hero.heroRot, immediately: true);
					p.hero.Control.Stop();
				}
			}
		}
	}

	private static void LoadWorld_Imp(DebugGameSave save)
	{
		throw new NotImplementedException();
	}

	private static DewPlayer LoadPlayer_Imp_GetPlayer(DebugGameSave.PlayerData data)
	{
		foreach (DewPlayer p in DewPlayer.humanPlayers)
		{
			if (p.playerName == data.name)
			{
				return p;
			}
		}
		return null;
	}

	private static void LoadPlayer_Imp(DebugGameSave.PlayerData data)
	{
		DewPlayer owner = LoadPlayer_Imp_GetPlayer(data);
		if (owner == null)
		{
			Log("Couldn't find player '" + data.name + "'");
			return;
		}
		owner.gold = data.gold;
		owner.dreamDust = data.dreamDust;
		Hero heroPrefab = DewResources.GetByShortTypeName<Hero>(data.hero.heroName);
		if (heroPrefab == null)
		{
			Log("Could not find hero: " + data.hero.heroName);
		}
		Hero prev = owner.hero;
		Hero hero = Dew.SpawnHero(heroPrefab, prev.position, prev.rotation, prev.owner, data.hero.level, data.hero.loadout);
		Dew.Destroy(prev.gameObject);
		owner.hero = hero;
		owner.controllingEntity = hero;
		hero.Status.SetHealth(data.hero.heroHealth);
		hero.StartCoroutine(Routine());
		Log("Loaded save");
		IEnumerator Routine()
		{
			yield return null;
			yield return null;
			if (data.hero.chaosStats != null)
			{
				hero.AddData(new Shrine_Chaos.Ad_ChaosStats
				{
					bonus = data.hero.chaosStats
				});
				hero.Status.AddStatBonus(data.hero.chaosStats);
			}
			foreach (DebugGameSave.SkillData s in data.hero.skills)
			{
				SkillTrigger newSkillPrefab = DewResources.GetByShortTypeName<SkillTrigger>(s.name);
				if (newSkillPrefab == null)
				{
					Debug.LogWarning("Replacement skill " + s.name + " is unavailable");
				}
				else
				{
					if (hero.Ability.abilities.ContainsKey((int)s.location))
					{
						hero.Skill.UnequipSkill(s.location, hero.position, ignoreCanReplace: true).Destroy();
					}
					SkillTrigger newSkill = Dew.CreateSkillTrigger(newSkillPrefab, hero.position, s.level);
					hero.Skill.EquipSkill(s.location, newSkill, ignoreCanReplace: true);
				}
			}
			foreach (DebugGameSave.GemData g in data.hero.gems)
			{
				Gem newGemPrefab = DewResources.GetByShortTypeName<Gem>(g.name);
				if (newGemPrefab == null)
				{
					Debug.LogWarning("Gem " + g.name + " is unavailable");
				}
				else
				{
					Gem newGem = Dew.CreateGem(newGemPrefab, hero.position, g.quality);
					hero.Skill.EquipGem(g.location, newGem);
				}
			}
		}
	}

	[DewConsoleMethod("Reset chaos stat of selected entity", CommandType.GameServerCheat)]
	public static void ResetChaos()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (!ent.TryGetData<Shrine_Chaos.Ad_ChaosStats>(out var chaos))
		{
			Log($"Chaos stat does not exist on {ent}");
			return;
		}
		ent.Status.RemoveStatBonus(chaos.bonus);
		ent.RemoveData<Shrine_Chaos.Ad_ChaosStats>();
	}

	[DewConsoleMethod("Control selected entity", CommandType.GameServerCheat)]
	public static void Control()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			GetPlayer().controllingEntity = ent;
		}
	}

	[DewConsoleMethod("Take ownership of the selected entity.", CommandType.GameServerCheat)]
	public static void Takeown()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		ent.netIdentity.AssignClientAuthority(GetPlayer().connectionToClient);
		Log("Took ownership of " + ent.name);
	}

	[DewConsoleMethod("Take away ownership of the selected entity.", CommandType.GameServerCheat)]
	public static void Disown()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (!ent.isOwned)
		{
			Log($"{ent} does not belong to a player.");
			return;
		}
		ent.netIdentity.RemoveClientAuthority();
		Log($"Took away ownership of {ent}");
	}

	[DewConsoleMethod("Spawn an entity as your own at cursor position, with same level as your hero", CommandType.GameServerCheat)]
	public static void SpawnMine(string substring)
	{
		DoSpawning(substring, 1u, GetPlayer(), GetCursorWorldPos(), GetPlayer().hero.level);
	}

	[DewConsoleMethod("Spawn an entity as your own at cursor position, with same level as your hero", CommandType.GameServerCheat)]
	public static void SpawnMine(string substring, uint num)
	{
		DoSpawning(substring, num, GetPlayer(), GetCursorWorldPos(), GetPlayer().hero.level);
	}

	[DewConsoleMethod("Create a networked object at cursor position", CommandType.GameServerCheat)]
	public static void Create(string substring)
	{
		Component prefab = null;
		Component[] prefabs = DewResources.FindAllByTypeSubstring<Component>(substring).ToArray();
		if (prefabs.Length == 0)
		{
			Log("Could not find object with query: " + substring);
			return;
		}
		prefab = Dew.SelectBestWithScore((IList<Component>)prefabs, (Func<Component, int, float>)((Component comp, int index) => (comp is RoomModifierBase) ? (-10000f) : (1f / (float)comp.name.Length)), 0f, (DewRandomInstance)null);
		Log("Spawning " + prefab);
		Vector3 pos = GetCursorWorldPos();
		Quaternion rot = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
		if (prefab is IProp { customSpawnRotation: not null, customSpawnRotation: var customSpawnRotation })
		{
			rot = customSpawnRotation.Value;
		}
		if (prefab is AbilityInstance ai)
		{
			TryGetConsoleSelectedEntity(out var caster);
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(ai, pos, rot, new CastInfo(caster));
		}
		else if (prefab is Actor a)
		{
			Dew.CreateActor(a, GetCursorWorldPos(), rot);
		}
		else
		{
			Dew.InstantiateAndSpawn(prefab, GetCursorWorldPos(), rot);
		}
	}

	[DewConsoleMethod("Spawn a mini boss", CommandType.GameServerCheat)]
	public static void SpawnMiniBoss()
	{
		SingletonDewNetworkBehaviour<Room>.instance.monsters.SpawnMiniBoss(new SpawnMonsterSettings());
	}

	[DewConsoleMethod("Spawn an elite monster of specific entity type", CommandType.GameServerCheat)]
	public static void SpawnMiniBoss(string substring)
	{
		Entity ent = DewResources.FindAllByTypeSubstring<Entity>(substring).FirstOrDefault((Entity p) => p is ISpawnableAsMiniBoss);
		if (ent == null)
		{
			Log("Could not find mini boss with query: " + substring);
		}
		else
		{
			SingletonDewNetworkBehaviour<Room>.instance.monsters.SpawnMiniBoss(new SpawnMonsterSettings(), ent);
		}
	}

	[DewConsoleMethod("Spawn an elite monster of specific entity type and effect type", CommandType.GameServerCheat)]
	public static void SpawnMiniBoss(string entitySubstring, string effectSubstring)
	{
		Entity ent = DewResources.FindAllByTypeSubstring<Entity>(entitySubstring).FirstOrDefault((Entity p) => p is ISpawnableAsMiniBoss);
		if (ent == null)
		{
			Log("Could not find mini boss with query: " + entitySubstring);
			return;
		}
		foreach (MiniBossEffect c in DewResources.FindAllByType<MiniBossEffect>())
		{
			if (c.GetType().Name.Contains(effectSubstring, StringComparison.OrdinalIgnoreCase))
			{
				SingletonDewNetworkBehaviour<Room>.instance.monsters.SpawnMiniBoss(new SpawnMonsterSettings(), ent, c);
				return;
			}
		}
		Log("Could not find effect with query: " + effectSubstring);
	}

	[DewConsoleMethod("Spawn a creep entity at cursor position, with ambient level.", CommandType.GameServerCheat)]
	public static void Spawn(string substring)
	{
		DoSpawning(substring, 1u, DewPlayer.creep, GetCursorWorldPos(), NetworkedManagerBase<GameManager>.instance.ambientLevel);
	}

	[DewConsoleMethod("Spawn creep entities at cursor position, with ambient level.", CommandType.GameServerCheat)]
	public static void Spawn(string substring, uint num)
	{
		DoSpawning(substring, num, DewPlayer.creep, GetCursorWorldPos(), NetworkedManagerBase<GameManager>.instance.ambientLevel);
	}

	[DewConsoleMethod("Spawn a neutral entity at cursor position, with ambient level.", CommandType.GameServerCheat)]
	public static void SpawnNeutral(string substring)
	{
		DoSpawning(substring, 1u, DewPlayer.environment, GetCursorWorldPos(), NetworkedManagerBase<GameManager>.instance.ambientLevel);
	}

	[DewConsoleMethod("Spawn neutral entities at cursor position, with ambient level.", CommandType.GameServerCheat)]
	public static void SpawnNeutral(string substring, uint num)
	{
		DoSpawning(substring, num, DewPlayer.environment, GetCursorWorldPos(), NetworkedManagerBase<GameManager>.instance.ambientLevel);
	}

	[DewConsoleMethod("Teleport selected entity to cursor position", CommandType.GameServerCheat)]
	public static void Teleport()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		ent.Control.Teleport(GetCursorWorldPos());
		ent.Control.Stop();
	}

	[DewConsoleMethod("Add atkspd bonus to entity", CommandType.GameServerCheat)]
	public static void AtkSpd(float strength)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.TryGetData<Ad_AtkSpd>(out var data))
		{
			data.bonus.attackSpeedPercentage = strength;
		}
		else
		{
			StatBonus bonus = new StatBonus
			{
				attackSpeedPercentage = strength
			};
			ent.Status.AddStatBonus(bonus);
			ent.AddData(new Ad_AtkSpd
			{
				bonus = bonus
			});
		}
		Log($"{ent.GetActorReadableName()} AtkSpd bonus is now {strength}%");
	}

	[DewConsoleMethod("Add crit chance bonus to entity", CommandType.GameServerCheat)]
	public static void Crit(float strength)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.TryGetData<Ad_CritChance>(out var data))
		{
			data.bonus.critChanceFlat = strength;
		}
		else
		{
			StatBonus bonus = new StatBonus
			{
				critChanceFlat = strength
			};
			ent.Status.AddStatBonus(bonus);
			ent.AddData(new Ad_CritChance
			{
				bonus = bonus
			});
		}
		Log($"{ent.GetActorReadableName()} Crit chance bonus is now {strength:#.0}");
	}

	[DewConsoleMethod("Apply status effect to selected entity", CommandType.GameServerCheat)]
	public static void StatusEffect(string substring)
	{
		StatusEffect found = DewResources.FindOneByTypeSubstring<StatusEffect>(substring);
		if (found == null)
		{
			Log("Status effect not found with query: " + substring);
			return;
		}
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		Log($"Applying {found.GetType().Name} to {ent}");
		Dew.CreateStatusEffect(found, ent, null, new CastInfo(ent, ent));
	}

	[DewConsoleMethod("Toggle Entity AI", CommandType.GameServerCheat)]
	public static void Ai()
	{
		Ai(EntityAI.DisableAI);
	}

	[DewConsoleMethod("Toggle Entity AI", CommandType.GameServerCheat)]
	public static void Ai(bool value)
	{
		EntityAI.DisableAI = !value;
		if (value)
		{
			Log("Entity AI is now ENABLED.");
		}
		else
		{
			Log("Entity AI is now DISABLED.");
		}
		if (value)
		{
			return;
		}
		foreach (Entity e in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (e.AI.isAITicking)
			{
				e.Control.Stop();
			}
		}
	}

	[DewConsoleMethod("Color this entity locally.", CommandType.Game)]
	public static void ECBase(Color color)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		EntityColorModifier mod;
		if (!ent.TryGetData<Ad_Color>(out var c))
		{
			mod = ent.Visual.GetNewColorModifier();
			ent.AddData(new Ad_Color
			{
				modifier = mod
			});
		}
		else
		{
			mod = c.modifier;
		}
		mod.baseColor = color;
		Log($"Changed {ent.GetActorReadableName()} base color to {color}");
	}

	[DewConsoleMethod("Set this entity's emission locally.", CommandType.Game)]
	public static void ECEmission(Color color)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		EntityColorModifier mod;
		if (!ent.TryGetData<Ad_Color>(out var c))
		{
			mod = ent.Visual.GetNewColorModifier();
			ent.AddData(new Ad_Color
			{
				modifier = mod
			});
		}
		else
		{
			mod = c.modifier;
		}
		mod.emission = color;
		Log($"Changed {ent.GetActorReadableName()} emission to {color}");
	}

	[DewConsoleMethod("Set this entity's dissolve locally.", CommandType.Game)]
	public static void ECDissolve(float amount)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		EntityColorModifier mod;
		if (!ent.TryGetData<Ad_Color>(out var c))
		{
			mod = ent.Visual.GetNewColorModifier();
			ent.AddData(new Ad_Color
			{
				modifier = mod
			});
		}
		else
		{
			mod = c.modifier;
		}
		mod.dissolveAmount = amount;
		Log($"Changed {ent.GetActorReadableName()} dissolve amount to {amount}");
	}

	[DewConsoleMethod("Set this entity's transform locally.", CommandType.Game)]
	public static void ETLocal(Vector3 value)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		EntityTransformModifier mod;
		if (!ent.TryGetData<Ad_Transform>(out var c))
		{
			mod = ent.Visual.GetNewTransformModifier();
			ent.AddData(new Ad_Transform
			{
				modifier = mod
			});
		}
		else
		{
			mod = c.modifier;
		}
		mod.localOffset = value;
		Log($"Changed {ent.GetActorReadableName()} local offset to {value}");
	}

	[DewConsoleMethod("Set this entity's transform locally.", CommandType.Game)]
	public static void ETWorld(Vector3 value)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		EntityTransformModifier mod;
		if (!ent.TryGetData<Ad_Transform>(out var c))
		{
			mod = ent.Visual.GetNewTransformModifier();
			ent.AddData(new Ad_Transform
			{
				modifier = mod
			});
		}
		else
		{
			mod = c.modifier;
		}
		mod.worldOffset = value;
		Log($"Changed {ent.GetActorReadableName()} world offset to {value}");
	}

	[DewConsoleMethod("Set this entity's transform locally.", CommandType.Game)]
	public static void ETScale(Vector3 value)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		EntityTransformModifier mod;
		if (!ent.TryGetData<Ad_Transform>(out var c))
		{
			mod = ent.Visual.GetNewTransformModifier();
			ent.AddData(new Ad_Transform
			{
				modifier = mod
			});
		}
		else
		{
			mod = c.modifier;
		}
		mod.scaleMultiplier = value;
		Log($"Changed {ent.GetActorReadableName()} scale multiplier to {value}");
	}

	[DewConsoleMethod("Set multiplier for dealt damage of this entity", CommandType.GameServerCheat)]
	public static void DealtDmg(float multiplier)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.TryGetData<Ad_DealtDmg>(out var _))
		{
			ent.RemoveData<Ad_DealtDmg>();
			ent.AddData(new Ad_DealtDmg
			{
				multiplier = multiplier
			});
			return;
		}
		ent.AddData(new Ad_DealtDmg
		{
			multiplier = multiplier
		});
		ent.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if (!data.IsAmountModifiedBy(typeof(Ad_DealtDmg)) && ent.TryGetData<Ad_DealtDmg>(out var data2))
			{
				data.SetAmountModifiedBy(typeof(Ad_DealtDmg));
				data.ApplyRawMultiplier(data2.multiplier);
			}
		});
		Log($"{ent.GetActorReadableName()} now deals {multiplier}x damage");
	}

	[DewConsoleMethod("Set multiplier for taken damage of this entity", CommandType.GameServerCheat)]
	public static void TakenDmg(float multiplier)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.TryGetData<Ad_TakenDmg>(out var _))
		{
			ent.RemoveData<Ad_TakenDmg>();
			ent.AddData(new Ad_TakenDmg
			{
				multiplier = multiplier
			});
			return;
		}
		ent.AddData(new Ad_TakenDmg
		{
			multiplier = multiplier
		});
		ent.takenDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if (!data.IsAmountModifiedBy(typeof(Ad_TakenDmg)) && ent.TryGetData<Ad_TakenDmg>(out var data2))
			{
				data.SetAmountModifiedBy(typeof(Ad_TakenDmg));
				data.ApplyRawMultiplier(data2.multiplier);
			}
		});
		Log($"{ent.GetActorReadableName()} now takes {multiplier}x damage");
	}

	private static Entity[] DoSpawning(string substring, uint num, DewPlayer owner, Vector3 spawnPos, int level)
	{
		Entity[] spawned = new Entity[num];
		Entity ent = DewResources.FindOneByTypeSubstring<Entity>(substring);
		if (ent == null)
		{
			Log("Could not find entity with query: " + substring);
			return null;
		}
		if (num < 1)
		{
			Log("Cannot spawn less than 1 entity");
			return null;
		}
		for (int i = 0; i < num; i++)
		{
			if (num == 1)
			{
				spawned[i] = Dew.SpawnEntity(ent, spawnPos + UnityEngine.Random.insideUnitSphere * 0.1f, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), null, owner, level);
			}
			else
			{
				spawned[i] = Dew.SpawnEntity(ent, spawnPos, Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), null, owner, level);
			}
		}
		return spawned;
	}

	[DewConsoleMethod("Unlock all locally generated items", CommandType.Anywhere)]
	public static void ItemUnlockAll()
	{
		string[] array = DewSave.profile.emotes.Keys.ToArray();
		foreach (string e in array)
		{
			DewSave.profile.UnlockEmote(e, null);
		}
		array = DewSave.profile.accessories.Keys.ToArray();
		foreach (string e2 in array)
		{
			DewSave.profile.UnlockAccessory(e2, null);
		}
		array = DewSave.profile.nametags.Keys.ToArray();
		foreach (string e3 in array)
		{
			DewSave.profile.UnlockNametag(e3, null);
		}
		DewSave.SaveProfile();
	}

	[DewConsoleMethod("Lock all items", CommandType.Anywhere)]
	public static void ItemLockAll()
	{
		string[] array = DewSave.profile.emotes.Keys.ToArray();
		foreach (string e in array)
		{
			DewSave.profile.LockEmote(e);
		}
		array = DewSave.profile.accessories.Keys.ToArray();
		foreach (string e2 in array)
		{
			DewSave.profile.LockAccessory(e2);
		}
		array = DewSave.profile.nametags.Keys.ToArray();
		foreach (string e3 in array)
		{
			DewSave.profile.LockNametag(e3);
		}
		DewSave.SaveProfile();
	}

	[DewConsoleMethod("Revalidate all players' items", CommandType.Network)]
	public static async void ItemRevalidate(string itemName)
	{
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			humanPlayer.RevalidateAllItems();
		}
	}

	[DewConsoleMethod("Show information about current lobby")]
	public static void LobbyInfo()
	{
		if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
		{
			Log("Not in any lobby");
		}
		else if (ManagerBase<LobbyManager>.instance.service.currentLobby != null)
		{
			Log("- " + ManagerBase<LobbyManager>.instance.service.currentLobby.GetType().Name);
			Type type = ManagerBase<LobbyManager>.instance.service.currentLobby.GetType();
			PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] array = props;
			foreach (PropertyInfo f in array)
			{
				Debug.Log($"{f.Name}: {f.GetValue(ManagerBase<LobbyManager>.instance.service.currentLobby)}");
			}
			FieldInfo[] array2 = fields;
			foreach (FieldInfo f2 in array2)
			{
				Debug.Log($"{f2.Name}: {f2.GetValue(ManagerBase<LobbyManager>.instance.service.currentLobby)}");
			}
		}
	}

	[DewConsoleMethod("Show information about current lobby")]
	public static void LobbyLeave()
	{
		Log("Leaving lobby");
		ManagerBase<LobbyManager>.instance.service.LeaveLobby();
	}

	[DewConsoleMethod("Show all matching localization data")]
	public static void Loc(string pattern)
	{
		Log("All matches for: " + pattern);
		Log("===============================");
		foreach (KeyValuePair<string, AchievementData> p in DewLocalization.data.achievements)
		{
			if (p.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Achievements::" + p.Key + ": " + p.Value.name);
			}
		}
		foreach (KeyValuePair<string, ArtifactData> p2 in DewLocalization.data.artifacts)
		{
			if (p2.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Artifacts::" + p2.Key + ": " + p2.Value.name);
			}
		}
		foreach (KeyValuePair<string, SkillData> p3 in DewLocalization.data.skills)
		{
			if (p3.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Skills::" + p3.Key + ": " + p3.Value.configs[0].name);
			}
		}
		foreach (KeyValuePair<string, ConversationData> p4 in DewLocalization.data.conversations)
		{
			if (p4.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log($"Conversations::{p4.Key}: {p4.Value.lines[0]}");
			}
		}
		foreach (KeyValuePair<string, CurseData> p5 in DewLocalization.data.curses)
		{
			if (p5.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Curses::" + p5.Key + ": " + p5.Value.name);
			}
		}
		foreach (KeyValuePair<string, GemData> p6 in DewLocalization.data.gems)
		{
			if (p6.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Gems::" + p6.Key + ": " + p6.Value.name);
			}
		}
		foreach (KeyValuePair<string, StarData> p7 in DewLocalization.data.stars)
		{
			if (p7.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Stars::" + p7.Key + ": " + p7.Value.name);
			}
		}
		foreach (KeyValuePair<string, string> p8 in DewLocalization.data.tips)
		{
			if (p8.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Tips::" + p8.Key + ": " + p8.Value);
			}
		}
		foreach (KeyValuePair<string, TreasureData> p9 in DewLocalization.data.treasures)
		{
			if (p9.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("Treasures::" + p9.Key + ": " + p9.Value.name);
			}
		}
		foreach (KeyValuePair<string, string> p10 in DewLocalization.data.ui)
		{
			if (p10.Key.EqualsWildcard(pattern, ignoreCase: true))
			{
				Log("UIs::" + p10.Key + ": " + p10.Value);
			}
		}
		Log("===============================");
	}

	[DewConsoleMethod("Set hunter progress of this world", CommandType.GameServerCheat)]
	public static void HuntWorld(float ratio)
	{
		for (int i = 0; i < NetworkedManagerBase<ZoneManager>.instance.hunterStatuses.Count; i++)
		{
			NetworkedManagerBase<ZoneManager>.instance.hunterStatuses[i] = ((i == NetworkedManagerBase<ZoneManager>.instance.hunterStartNodeIndex) ? HunterStatus.AboutToBeTaken : HunterStatus.None);
		}
		ratio = Mathf.Clamp01(ratio);
		for (int j = 0; j < 50; j++)
		{
			if (NetworkedManagerBase<ZoneManager>.instance.GetHunterProgress() > ratio)
			{
				break;
			}
			NetworkedManagerBase<ZoneManager>.instance.AdvanceHunterTurn(forceMove: true);
		}
		NetworkedManagerBase<ZoneManager>.instance.UpdateModifiersByHunterStatus();
	}

	[DewConsoleMethod("Advance hunters", CommandType.GameServerCheat)]
	public static void HuntAdvance()
	{
		NetworkedManagerBase<ZoneManager>.instance.AdvanceHunterTurn(forceMove: true);
		NetworkedManagerBase<ZoneManager>.instance.UpdateModifiersByHunterStatus();
	}

	[DewConsoleMethod("Set current hunter level", CommandType.GameServerCheat)]
	public static void HuntLevel(int level)
	{
		NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel = Mathf.Max(level - 1, 0);
	}

	[DewConsoleMethod("Reset Good Prop Position Test")]
	public static void TestPropPosReset()
	{
		foreach (RoomSection section in SingletonDewNetworkBehaviour<Room>.instance.sections)
		{
			section.ResetUsedNodeIndices();
		}
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>();
		foreach (Transform t in array)
		{
			if (!(t.name != "TEST PROP POS"))
			{
				UnityEngine.Object.Destroy(t.gameObject);
			}
		}
		Log("Reset prop position indices and removed test markers");
	}

	[DewConsoleMethod("Test Good Prop Position")]
	public static void TestPropPos(int count)
	{
		Log($"Testing {count} positions");
		for (int i = 0; i < count; i++)
		{
			Vector3 shrinePos;
			bool isOptimal = SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out shrinePos);
			GameObject newProp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			newProp.name = "TEST PROP POS";
			newProp.transform.position = shrinePos;
			newProp.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", isOptimal ? Color.green : Color.red);
			if (!isOptimal)
			{
				newProp.transform.localScale = Vector3.Scale(newProp.transform.localScale, new Vector3(0.5f, 1.5f, 0.5f));
			}
		}
	}

	[DewConsoleMethod("Test Pure Dream", CommandType.GameServerCheat)]
	public static void TestPureDream()
	{
		Entity[] s = DoSpawning("spider", 1u, DewPlayer.creep, GetCursorWorldPos(), NetworkedManagerBase<GameManager>.instance.ambientLevel);
		s[0].CreateStatusEffect(DewResources.GetByShortTypeName<StatusEffect>("Se_PureDream"), s[0], default(CastInfo));
	}

	[DewConsoleMethod("Test Good Prop Position at Current Section")]
	public static void TestPropPosSection(int count)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		if (ent.section == null)
		{
			Log("Selected entity is not on a section");
			return;
		}
		Log($"Testing {count} positions in current section");
		for (int i = 0; i < count; i++)
		{
			Vector3 shrinePos;
			bool isOptimal = ent.section.TryGetGoodNodePosition(out shrinePos);
			GameObject newProp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			newProp.name = "TEST PROP POS";
			newProp.transform.position = shrinePos;
			newProp.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", isOptimal ? Color.green : Color.red);
			if (!isOptimal)
			{
				newProp.transform.localScale = Vector3.Scale(newProp.transform.localScale, new Vector3(0.5f, 1.5f, 0.5f));
			}
		}
	}

	[DewConsoleMethod("Load next zone", CommandType.GameServerCheat)]
	public static void NextZone()
	{
		Log("Loading next zone");
		NetworkedManagerBase<GameManager>.instance.LoadNextZone();
	}

	[DewConsoleMethod("Change current zone. Does not increment ambient level nor zone index.", CommandType.GameServerCheat)]
	public static void Zone(string substr)
	{
		Zone found = DewResources.FindOneByIdSubstring<Zone>(substr);
		if (found == null)
		{
			Log("Could not find a zone with '" + substr + "'");
			return;
		}
		Log("Loading zone " + found.name);
		NetworkedManagerBase<ZoneManager>.instance.LoadZone(found, incrementParameters: false);
	}

	[DewConsoleMethod("Print info about current zone", CommandType.GameServer)]
	public static void ZoneInfo()
	{
		Log($"{NetworkedManagerBase<ZoneManager>.instance.currentZone.name}::{SingletonDewNetworkBehaviour<Room>.instance}");
		Log($"currentNodeIndex: {NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex}");
		Log($"currentTurnIndex: {NetworkedManagerBase<ZoneManager>.instance.currentTurnIndex}");
	}

	[DewConsoleMethod("Open world view", CommandType.GameCheat)]
	public static void World()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss)
		{
			Log("Cannot use World command on boss node");
		}
		else
		{
			InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.Shown;
		}
	}

	[DewConsoleMethod("Regenerate world", CommandType.GameServerCheat)]
	public static void WorldRegen()
	{
		NetworkedManagerBase<ZoneManager>.instance.GenerateWorld();
		NetworkedManagerBase<ZoneManager>.instance.TravelToNode(0);
		Log("Regenerated world");
	}

	[DewConsoleMethod("Regenerate world without loading node", CommandType.GameServerCheat)]
	public static void WorldRegenNoLoad()
	{
		NetworkedManagerBase<ZoneManager>.instance.GenerateWorld();
		NetworkedManagerBase<ZoneManager>.instance.SetCurrentNodeIndexAndRevealAdjacent(0);
		Log("Regenerated world without loading node");
	}

	[DewConsoleMethod("Regenerate world", CommandType.GameServerCheat)]
	public static void WorldRegen(int seed)
	{
		NetworkedManagerBase<ZoneManager>.instance.GenerateWorld(seed);
		NetworkedManagerBase<ZoneManager>.instance.TravelToNode(0);
		Log("Regenerated world with seed " + seed);
	}

	[DewConsoleMethod("Regenerate world without loading node", CommandType.GameServerCheat)]
	public static void WorldRegenNoLoad(int seed)
	{
		NetworkedManagerBase<ZoneManager>.instance.GenerateWorld(seed);
		NetworkedManagerBase<ZoneManager>.instance.SetCurrentNodeIndexAndRevealAdjacent(0);
		Log("Regenerated world without loading node with seed " + seed);
	}

	[DewConsoleMethod("Reveal all nodes", CommandType.GameServerCheat)]
	public static void WorldReveal()
	{
		NetworkedManagerBase<ZoneManager>.instance.RevealWorld();
		Log("Revealed world");
	}

	[DewConsoleMethod("Fully reveal all nodes", CommandType.GameServerCheat)]
	public static void WorldRevealFull()
	{
		NetworkedManagerBase<ZoneManager>.instance.RevealWorld(fully: true);
		Log("Fully revealed world");
	}

	[DewConsoleMethod("Load current node again as first visit", CommandType.GameServerCheat)]
	public static void NodeReload()
	{
		NetworkedManagerBase<ZoneManager>.instance.visitedNodesSaveData[NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex] = null;
		NetworkedManagerBase<ZoneManager>.instance.LoadNode(new LoadNodeSettings
		{
			from = -1,
			to = NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex,
			advanceTurn = false
		});
	}

	[DewConsoleMethod("Load a room as current node as first visit.", "Room", CommandType.GameServerCheat)]
	public static void LoadRoom(string substr)
	{
		string scene = DewResources.FindRoomNameBySubstring(substr);
		if (scene == null)
		{
			Log("Cannot find scene with substring: " + substr);
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.SetRoom(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, scene);
		NetworkedManagerBase<ZoneManager>.instance.visitedNodesSaveData[NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex] = null;
		NetworkedManagerBase<ZoneManager>.instance.LoadNode(new LoadNodeSettings
		{
			from = -1,
			to = NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex,
			advanceTurn = false
		});
	}

	[DewConsoleMethod("Change room rotation.", CommandType.GameServerCheat)]
	public static void RoomRot()
	{
		if (!(SingletonDewNetworkBehaviour<Room>.instance == null))
		{
			int newRot = (NetworkedManagerBase<ZoneManager>.instance.currentNode.roomRotIndex + 1) % SingletonDewNetworkBehaviour<Room>.instance.availableCameraAngles.Length;
			NetworkedManagerBase<ZoneManager>.instance.SetRoomRotIndex(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, newRot);
			SingletonDewNetworkBehaviour<Room>.instance.SyncCameraAngle();
			Log($"Set camera angle to {SingletonDewNetworkBehaviour<Room>.instance.availableCameraAngles[newRot]}° ({newRot + 1}/{SingletonDewNetworkBehaviour<Room>.instance.availableCameraAngles.Length})");
		}
	}

	[DewConsoleMethod("Move to a node by index / type / modifier", "Node", CommandType.GameServerCheat)]
	public static void LoadNode(string node)
	{
		if (NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
		{
			Log("Already in transition");
			return;
		}
		if (!int.TryParse(node, out var index))
		{
			index = -1;
			for (int i = 0; i < NetworkedManagerBase<ZoneManager>.instance.nodes.Count; i++)
			{
				if (i == NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex)
				{
					continue;
				}
				WorldNodeData n = NetworkedManagerBase<ZoneManager>.instance.nodes[i];
				if (n.type.ToString().Contains(node, StringComparison.InvariantCultureIgnoreCase))
				{
					Log("Found node with type " + n.type);
					index = i;
					break;
				}
				bool found = false;
				foreach (ModifierData m in n.modifiers)
				{
					if (m.type.Contains(node, StringComparison.InvariantCultureIgnoreCase))
					{
						Log("Found node with modifier " + m.type);
						index = i;
						found = true;
						break;
					}
				}
				if (found)
				{
					break;
				}
			}
		}
		if (index == -1)
		{
			Log("Could not find node with specific type/modifier '" + node + "'");
			return;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex == index)
		{
			Log($"Already on node {index}");
			return;
		}
		if (index < 0 || index >= NetworkedManagerBase<ZoneManager>.instance.nodes.Count)
		{
			Log($"Invalid node index. Valid range: 0~{NetworkedManagerBase<ZoneManager>.instance.nodes.Count - 1}");
			return;
		}
		Log($"Moving from {NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex} to {index}");
		NetworkedManagerBase<ZoneManager>.instance.TravelToNode(index);
	}

	[DewConsoleMethod("Print out some information about current node.", CommandType.GameServer)]
	public static void NodeInfo()
	{
		Log(NetworkedManagerBase<ZoneManager>.instance.currentZone.name ?? "");
		Log($"- Index: {NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex}");
		Log($"- Type: {NetworkedManagerBase<ZoneManager>.instance.currentNode.type}");
		Log($"- Status: {NetworkedManagerBase<ZoneManager>.instance.currentNode.status}");
		Log($"- Is Hunted: {NetworkedManagerBase<ZoneManager>.instance.isCurrentNodeHunted}");
		Log("- Room: " + NetworkedManagerBase<ZoneManager>.instance.currentNode.room);
		Log($"- Room Rot: {ManagerBase<CameraManager>.instance.entityCamAngle}°");
		Log($"- RoomArea: {SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area}");
		Log($"- Mods: {NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers.Count} modifiers");
		foreach (ModifierData m in NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers)
		{
			Log("  - " + m.type + ": " + m.clientData);
		}
	}

	[DewConsoleMethod("Add modifier to this node", CommandType.GameServerCheat)]
	public static void Mod(string substr)
	{
		Mod(substr, null);
	}

	[DewConsoleMethod("Add modifier to this node", CommandType.GameServerCheat)]
	public static void Mod(string substr, string data)
	{
		RoomModifierBase prefab = DewResources.FindOneByTypeSubstring<RoomModifierBase>(substr);
		if (prefab == null)
		{
			Log("Modifier not found with substr: " + substr);
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.AddModifier(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, new ModifierData
		{
			type = prefab.name,
			clientData = data
		});
		Log("Add modifier " + prefab.name + " to this node");
	}

	[DewConsoleMethod("Remove modifier of this node", CommandType.GameServerCheat)]
	public static void Unmod()
	{
		for (int i = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers.Count - 1; i >= 0; i--)
		{
			NetworkedManagerBase<ZoneManager>.instance.RemoveModifier(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers[i].id);
		}
		Log("Removed all modifiers from this node");
	}

	[DewConsoleMethod("Add modifier to all nodes", CommandType.GameServerCheat)]
	public static void ModAll(string substr, string data)
	{
		RoomModifierBase prefab = DewResources.FindOneByTypeSubstring<RoomModifierBase>(substr);
		if (prefab == null)
		{
			Log("Modifier not found with substr: " + substr);
			return;
		}
		for (int i = 0; i < NetworkedManagerBase<ZoneManager>.instance.nodes.Count; i++)
		{
			NetworkedManagerBase<ZoneManager>.instance.AddModifier(i, new ModifierData
			{
				type = prefab.name,
				clientData = data
			});
		}
		Log("Add modifier " + prefab.name + " to all nodes");
	}

	[DewConsoleMethod("Remove modifier of all nodes", CommandType.GameServerCheat)]
	public static void UnmodAll()
	{
		for (int i = 0; i < NetworkedManagerBase<ZoneManager>.instance.nodes.Count; i++)
		{
			for (int j = NetworkedManagerBase<ZoneManager>.instance.nodes[i].modifiers.Count - 1; j >= 0; j--)
			{
				NetworkedManagerBase<ZoneManager>.instance.RemoveModifier(i, NetworkedManagerBase<ZoneManager>.instance.nodes[i].modifiers[j].id);
			}
		}
		Log("Removed modifier of all nodes");
	}

	[DewConsoleMethod("Open a sidetrack rift", CommandType.GameServerCheat)]
	public static void SidetrackRift(string substr)
	{
		Rift_Sidetrack[] rifts = DewResources.FindAllByNameSubstring<Rift_Sidetrack>(substr).ToArray();
		if (rifts.Length == 0)
		{
			Log("Rift not found with substring: " + substr);
			return;
		}
		Vector3 from = GetPlayer().hero.agentPosition;
		Vector3 to = Dew.GetValidAgentDestination_Closest(from, GetCursorWorldPos());
		Dew.InstantiateAndSpawn(rifts[0], to, Quaternion.LookRotation(from - to)).Open();
		Log("Opening: " + rifts[0].name);
	}

	[DewConsoleMethod("Force a sidetrack rift to spawn in this room when it's cleared", CommandType.GameServerCheat)]
	public static void SidetrackSpawn(string substr)
	{
		Rift_Sidetrack[] rifts = DewResources.FindAllByNameSubstring<Rift_Sidetrack>(substr).ToArray();
		if (rifts.Length == 0)
		{
			Log("Rift not found with substring: " + substr);
			return;
		}
		SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Add(rifts[0].name);
		Log("This room is now guaranteed to spawn: " + SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.JoinToString(", "));
	}

	[DewConsoleMethod("Clear forced sidetrack rift spawns in this room", CommandType.GameServerCheat)]
	public static void SidetrackSpawnClear()
	{
		if (SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Count > 0)
		{
			Log("Removing guaranteed sidetrack rifts: " + SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.JoinToString(", "));
			SingletonDewNetworkBehaviour<Room>.instance.rifts.openedSidetrackRifts.Clear();
		}
		else
		{
			Log("No forced sidetrack spawns to clear");
		}
	}

	[DewConsoleMethod("Load a room as sidetrack room", CommandType.GameServerCheat)]
	public static void Sidetrack(string roomSubstr)
	{
		if (NetworkedManagerBase<ZoneManager>.instance.isSidetracking)
		{
			Log("Already sidetracking");
			return;
		}
		string sceneName = DewResources.FindRoomNameBySubstring(roomSubstr);
		if (sceneName == null)
		{
			Log("Cannot find scene with substring: " + roomSubstr);
		}
		else
		{
			NetworkedManagerBase<ZoneManager>.instance.LoadSidetrackRoom(sceneName);
		}
	}

	[DewConsoleMethod("Stop sidetracking and return to previous node", CommandType.GameServerCheat)]
	public static void SidetrackStop()
	{
		if (!NetworkedManagerBase<ZoneManager>.instance.isSidetracking)
		{
			Log("Not sidetracking");
		}
		else
		{
			NetworkedManagerBase<ZoneManager>.instance.ReturnFromSidetracking();
		}
	}

	[DewConsoleMethod("Start hosting via LAN mode", CommandType.Anywhere)]
	public static void LanHost()
	{
		if (NetworkServer.active || NetworkClient.active || NetworkClient.isConnecting)
		{
			Log("Already in a networked context");
			return;
		}
		DewNetworkManager.lanMode = true;
		DewNetworkManager.networkMode = DewNetworkManager.Mode.MultiplayerHost;
		SceneManager.LoadScene("PlayLobby");
	}

	[DewConsoleMethod("Connect as a client via LAN mode", CommandType.Anywhere)]
	public static void LanClient(string ip)
	{
		if (NetworkServer.active || NetworkClient.active || NetworkClient.isConnecting)
		{
			Log("Already in a networked context");
			return;
		}
		DewNetworkManager.lanMode = true;
		DewNetworkManager.networkMode = DewNetworkManager.Mode.MultiplayerJoinLobby;
		DewNetworkManager.joinTargetId = ip;
		PlayLobbyManager.isFirstTimePlayFlow = false;
		SceneManager.LoadScene("PlayLobby");
	}

	[DewConsoleMethod("Get information about networked object of given netId.", CommandType.NetworkServer)]
	public static void NetId(uint id)
	{
		if (NetworkServer.spawned.TryGetValue(id, out var netIdentity))
		{
			Actor actor = netIdentity.GetComponent<Actor>();
			if (actor != null)
			{
				Log($"{id}: {actor.GetActorReadableName()}");
			}
			else
			{
				Log($"{id}: {netIdentity.name}");
			}
		}
		else
		{
			Log($"Networked object of netId {id} not found");
		}
	}

	[DewConsoleMethod("Show information about network")]
	public static void NetStat()
	{
		if (NetworkServer.active)
		{
			Log("Server is active");
		}
		else if (NetworkClient.active)
		{
			Log("Client is active");
		}
		else
		{
			Log("No Network");
		}
		Log($"NetworkClient.isConnecting - {NetworkClient.isConnecting}");
		Log($"NetworkClient.isConnected - {NetworkClient.isConnected}");
		Log($"Round-trip Time: {NetworkTime.rtt * 1000.0:#,##0.0}ms");
	}

	[DewConsoleMethod("Set EntityControl sync parameter", CommandType.Game)]
	public static void SyncFixWarpDistance(float value)
	{
		EntityControl.SyncFixWarpDistance = value;
	}

	[DewConsoleMethod("Set EntityControl sync parameter", CommandType.Game)]
	public static void SyncFixSmoothTime(float value)
	{
		EntityControl.SyncFixSmoothTime = value;
	}

	[DewConsoleMethod("Set EntityControl sync parameter", CommandType.Game)]
	public static void SyncFixSnapshotLifetime(float value)
	{
		EntityControl.SyncFixSnapshotLifetime = value;
	}

	[DewConsoleMethod("Set EntityControl sync parameter", CommandType.Game)]
	public static void SyncAgentVelocityLifetime(float value)
	{
		EntityControl.SyncAgentVelocityLifetime = value;
	}

	[DewConsoleMethod("Set EntityControl sync parameter", CommandType.Game)]
	public static void SyncExtrapolateMaxTime(float value)
	{
		EntityControl.SyncExtrapolateMaxTime = value;
	}

	[DewConsoleMethod("Set EntityControl sync parameter", CommandType.Game)]
	public static void SyncExtrapolateStrength(float value)
	{
		EntityControl.SyncExtrapolateStrength = value;
	}

	[DewConsoleMethod("List players", CommandType.NetworkServer)]
	public static void Plist()
	{
		Log($"- {GetIndex(DewPlayer.creep).ToString(),-4}: {DewPlayer.creep.name}");
		Log($"- {GetIndex(DewPlayer.environment).ToString(),-4}: {DewPlayer.environment.name}");
		Log($"- {GetIndex(DewPlayer.local).ToString(),-4}: {DewPlayer.local.name}");
		foreach (DewPlayer h in DewPlayer.humanPlayers)
		{
			if (!(h == DewPlayer.local))
			{
				Log($"- {GetIndex(h).ToString(),-4}: {h.name}");
			}
		}
	}

	[DewConsoleMethod("Make two players enemies", CommandType.GameServerCheat)]
	public static void Penemy(uint a, uint b)
	{
		if (ResolveTwoPlayersAndLog(a, b, out var pa, out var pb))
		{
			ClearRelationship(pa, pb);
			pa.enemies.Add(pb);
			pb.enemies.Add(pa);
			Log(pa.name + "-" + pb.name + " => Enemy");
		}
	}

	[DewConsoleMethod("Make two players allies", CommandType.GameServerCheat)]
	public static void Pally(uint a, uint b)
	{
		if (ResolveTwoPlayersAndLog(a, b, out var pa, out var pb))
		{
			ClearRelationship(pa, pb);
			pa.allies.Add(pb);
			pb.allies.Add(pa);
			Log(pa.name + "-" + pb.name + " => Ally");
		}
	}

	[DewConsoleMethod("Make two players neutrals", CommandType.GameServerCheat)]
	public static void Pneutral(uint a, uint b)
	{
		if (ResolveTwoPlayersAndLog(a, b, out var pa, out var pb))
		{
			ClearRelationship(pa, pb);
			pa.neutrals.Add(pb);
			pb.neutrals.Add(pa);
			Log(pa.name + "-" + pb.name + " => Neutral");
		}
	}

	private static void ClearRelationship(DewPlayer a, DewPlayer b)
	{
		a.enemies.Remove(b);
		a.allies.Remove(b);
		a.neutrals.Remove(b);
		b.enemies.Remove(a);
		b.allies.Remove(a);
		b.neutrals.Remove(a);
	}

	private static bool ResolveTwoPlayersAndLog(uint a, uint b, out DewPlayer pa, out DewPlayer pb)
	{
		pa = ResolvePlayerByIndex(a);
		pb = ResolvePlayerByIndex(b);
		if (pa == null || pb == null)
		{
			Log("Player not found.");
			Plist();
			return false;
		}
		return true;
	}

	private static DewPlayer ResolvePlayerByIndex(uint index)
	{
		if (!NetworkServer.spawned.TryGetValue(index, out var netIdentity))
		{
			return null;
		}
		if (!netIdentity.TryGetComponent<DewPlayer>(out var player))
		{
			return null;
		}
		return player;
	}

	private static uint GetIndex(DewPlayer player)
	{
		return player.netId;
	}

	[DewConsoleMethod("List all achievements.")]
	public static void AchList()
	{
		int numComplete = 0;
		foreach (KeyValuePair<string, DewProfile.AchievementData> achievement in DewSave.profile.achievements)
		{
			if (achievement.Value.isCompleted)
			{
				numComplete++;
			}
		}
		Log($"Achievements({numComplete}/{Dew.allAchievements.Count}):");
		if (ManagerBase<AchievementManager>.instance != null)
		{
			foreach (DewAchievementItem a in ManagerBase<AchievementManager>.instance._trackedAchievements)
			{
				Log($"[ ] {a.GetCurrentProgress()}/{a.GetMaxProgress()} {a.name}");
			}
		}
		else
		{
			foreach (KeyValuePair<string, DewProfile.AchievementData> p in DewSave.profile.achievements)
			{
				if (!p.Value.isCompleted)
				{
					Log($"[ ] {p.Value.currentProgress}/{p.Value.maxProgress} {p.Key} {JSON.Serialize(p.Value.persistentVariables)}");
				}
			}
		}
		foreach (KeyValuePair<string, DewProfile.AchievementData> p2 in DewSave.profile.achievements)
		{
			if (p2.Value.isCompleted)
			{
				Log($"[v] {p2.Value.currentProgress}/{p2.Value.maxProgress} {p2.Key}");
			}
		}
	}

	[DewConsoleMethod("Complete all tracked achievements.")]
	public static void AchCompleteAll()
	{
		int num = 0;
		if (ManagerBase<AchievementManager>.instance != null)
		{
			num = ManagerBase<AchievementManager>.instance._trackedAchievements.Count;
			List<DewAchievementItem> list = ManagerBase<AchievementManager>.instance._trackedAchievements;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				ManagerBase<AchievementManager>.instance.CompleteAchievement(list[i]);
			}
		}
		else
		{
			foreach (KeyValuePair<string, DewProfile.AchievementData> a in DewSave.profile.achievements)
			{
				if (!a.Value.isCompleted)
				{
					num++;
					a.Value.isCompleted = true;
					a.Value.currentProgress = a.Value.maxProgress;
					a.Value.persistentVariables = null;
					DewAchievementItem newItem = (DewAchievementItem)Activator.CreateInstance(Dew.achievementsByName[a.Key]);
					DewSave.profile.stardust += newItem.grantedStardust;
				}
			}
			DewSave.SaveProfile();
			DewSave.profile.Validate();
		}
		Log($"Completed {num} achievements.");
	}

	[DewConsoleMethod("Complete all tracked achievements.")]
	public static void AchCompleteAll(string substr)
	{
		int num = 0;
		if (ManagerBase<AchievementManager>.instance != null)
		{
			num = ManagerBase<AchievementManager>.instance._trackedAchievements.Count;
			List<DewAchievementItem> list = ManagerBase<AchievementManager>.instance._trackedAchievements;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
				{
					ManagerBase<AchievementManager>.instance.CompleteAchievement(list[i]);
				}
			}
		}
		else
		{
			foreach (KeyValuePair<string, DewProfile.AchievementData> a in DewSave.profile.achievements)
			{
				if (a.Key.Contains(substr, StringComparison.InvariantCultureIgnoreCase) && !a.Value.isCompleted)
				{
					num++;
					a.Value.isCompleted = true;
					a.Value.currentProgress = a.Value.maxProgress;
					a.Value.persistentVariables = null;
					DewAchievementItem newItem = (DewAchievementItem)Activator.CreateInstance(Dew.achievementsByName[a.Key]);
					DewSave.profile.stardust += newItem.grantedStardust;
				}
			}
			DewSave.SaveProfile();
			DewSave.profile.Validate();
		}
		Log($"Completed {num} achievements.");
	}

	[DewConsoleMethod("Reset all achievements.")]
	public static void AchReset()
	{
		if (ManagerBase<AchievementManager>.instance != null)
		{
			ManagerBase<AchievementManager>.instance.StopTrackingAchievements();
		}
		foreach (KeyValuePair<string, DewProfile.AchievementData> a in DewSave.profile.achievements)
		{
			a.Value.isCompleted = false;
			a.Value.currentProgress = 0;
			a.Value.persistentVariables = new Dictionary<string, string>();
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> a2 in DewSave.profile.artifacts)
		{
			a2.Value.status = UnlockStatus.NotDiscovered;
			a2.Value.didReadMemory = false;
			a2.Value.isNewHeroOrHeroSkill = false;
		}
		DewSave.SaveProfile();
		DewSave.profile.Validate();
		if (ManagerBase<AchievementManager>.instance != null)
		{
			ManagerBase<AchievementManager>.instance.StartTrackingAchievements();
		}
		Log("Reset all achievements and unlocks.");
	}

	[DewConsoleMethod("Discover all non-discovered items.")]
	public static void DiscoverAll()
	{
		foreach (KeyValuePair<string, DewProfile.UnlockData> a in DewSave.profile.skills)
		{
			if (a.Value.status == UnlockStatus.NotDiscovered)
			{
				DewSave.profile.DiscoverSkill(a.Key);
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> a2 in DewSave.profile.gems)
		{
			if (a2.Value.status == UnlockStatus.NotDiscovered)
			{
				DewSave.profile.DiscoverGem(a2.Key);
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> a3 in DewSave.profile.artifacts)
		{
			if (a3.Value.status == UnlockStatus.NotDiscovered)
			{
				DewSave.profile.DiscoverArtifact(a3.Key);
			}
		}
		DewSave.SaveProfile();
		DewSave.profile.Validate();
		Log("Discovered all items.");
	}

	[DewConsoleMethod("Un-discover all discovered items.")]
	public static void UndiscoverAll()
	{
		foreach (KeyValuePair<string, DewProfile.UnlockData> a in DewSave.profile.skills)
		{
			if (a.Value.status == UnlockStatus.Complete && !(Dew.GetRequiredAchievementOfTarget(a.Key) != null) && !(Dew.allHeroSkills.FirstOrDefault((Type t) => t.Name == a.Key) != null))
			{
				a.Value.status = UnlockStatus.NotDiscovered;
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> a2 in DewSave.profile.gems)
		{
			if (a2.Value.status == UnlockStatus.Complete && !(Dew.GetRequiredAchievementOfTarget(a2.Key) != null))
			{
				a2.Value.status = UnlockStatus.NotDiscovered;
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> a3 in DewSave.profile.artifacts)
		{
			if (a3.Value.status == UnlockStatus.Complete)
			{
				a3.Value.status = UnlockStatus.NotDiscovered;
			}
		}
		DewSave.SaveProfile();
		DewSave.profile.Validate();
		Log("Undiscovered all items.");
	}

	[DewConsoleMethod("List all gems.")]
	public static void ListGems()
	{
		ListGems(null);
	}

	[DewConsoleMethod("List all gems with substring")]
	public static void ListGems(string substring)
	{
		Log("--------------------------------------");
		foreach (KeyValuePair<string, DewProfile.UnlockData> g in DewSave.profile.gems)
		{
			string name = DewLocalization.GetGemName(DewLocalization.GetGemKey(g.Key));
			if (string.IsNullOrEmpty(substring) || g.Key.Contains(substring, StringComparison.InvariantCultureIgnoreCase) || name.Contains(substring, StringComparison.InvariantCultureIgnoreCase))
			{
				Log(g.Key + " : " + name);
			}
		}
		Log("--------------------------------------");
	}

	[DewConsoleMethod("List all skills.")]
	public static void ListSkills()
	{
		ListSkills(null);
	}

	[DewConsoleMethod("List all skills with substring")]
	public static void ListSkills(string substring)
	{
		Log("--------------------------------------");
		foreach (KeyValuePair<string, DewProfile.UnlockData> g in DewSave.profile.skills)
		{
			string name = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(g.Key), 0);
			if (string.IsNullOrEmpty(substring) || g.Key.Contains(substring, StringComparison.InvariantCultureIgnoreCase) || name.Contains(substring, StringComparison.InvariantCultureIgnoreCase))
			{
				Log(g.Key + " : " + name);
			}
		}
		Log("--------------------------------------");
	}

	[DewConsoleMethod("List all lucid dreams.")]
	public static void ListLucidDreams()
	{
		Log($"LucidDreams({DewSave.profile.GetUnlockedLucidDreamsCount()}/{Dew.allLucidDreams.Count}):");
		foreach (KeyValuePair<string, DewProfile.UnlockData> g in DewSave.profile.lucidDreams)
		{
			if (g.Value.status != UnlockStatus.Locked)
			{
				Log(g.Key ?? "");
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> g2 in DewSave.profile.lucidDreams)
		{
			if (g2.Value.status == UnlockStatus.Locked)
			{
				Log("LOCKED " + g2.Key);
			}
		}
	}

	[DewConsoleMethod("Reset all tutorials.")]
	public static void TutReset()
	{
		if (ManagerBase<InGameTutorialManager>.instance != null)
		{
			ManagerBase<InGameTutorialManager>.instance.StopTutorials();
		}
		DewSave.profile.doneTutorials = new List<string>();
		DewSave.SaveProfile();
		if (ManagerBase<InGameTutorialManager>.instance != null)
		{
			ManagerBase<InGameTutorialManager>.instance.StartTutorials();
		}
		Log("Reset all tutorials.");
	}

	[DewConsoleMethod("List all tutorials.")]
	public static void TutList()
	{
		Log($"Tutorials({DewSave.profile.doneTutorials.Count}/{Dew.allTutorialItems.Count}):");
		foreach (string t in DewSave.profile.doneTutorials)
		{
			Log("[v] " + t);
		}
		foreach (Type t2 in Dew.allTutorialItems)
		{
			if (!DewSave.profile.doneTutorials.Contains(t2.Name))
			{
				Log("[ ] " + t2.Name);
			}
		}
	}

	[DewConsoleMethod("Reset all star related progression.")]
	public static void StarReset()
	{
		if (DewBuildProfile.current.buildType == BuildType.DemoLite)
		{
			DewSave.profile.stars.Clear();
		}
		else
		{
			DewSave.profile.newStars.Clear();
			foreach (KeyValuePair<string, DewProfile.HeroStarSlotUnlockData> p in DewSave.profile.heroUnlockedStarSlots)
			{
				p.Value.Set(StarType.Destruction, new List<int>());
				p.Value.Set(StarType.Imagination, new List<int>());
				p.Value.Set(StarType.Life, new List<int>());
				p.Value.Set(StarType.Flexible, new List<int>());
			}
		}
		DewSave.profile.Validate();
		DewSave.SaveProfile();
	}

	[DewConsoleMethod("Complete all star related progression.")]
	public static void StarComplete()
	{
		if (DewBuildProfile.current.buildType == BuildType.DemoLite)
		{
			foreach (KeyValuePair<string, DewProfile.StarData> p in DewSave.profile.stars)
			{
				p.Value.level = Dew.oldStarsByName[p.Key].maxLevel;
			}
		}
		else
		{
			foreach (KeyValuePair<string, DewProfile.StarData> p2 in DewSave.profile.newStars)
			{
				StarEffect star = DewResources.GetByShortTypeName<StarEffect>(p2.Key);
				p2.Value.level = star.maxStarLevel;
			}
		}
		DewSave.SaveProfile();
	}

	[DewConsoleMethod("Randomly set all star related progression.")]
	public static void StarRandom()
	{
		if (DewBuildProfile.current.buildType == BuildType.DemoLite)
		{
			foreach (KeyValuePair<string, DewProfile.StarData> p in DewSave.profile.stars)
			{
				p.Value.level = UnityEngine.Random.Range(0, Dew.oldStarsByName[p.Key].maxLevel + 1);
			}
		}
		else
		{
			foreach (KeyValuePair<string, DewProfile.StarData> p2 in DewSave.profile.newStars)
			{
				StarEffect star = DewResources.GetByShortTypeName<StarEffect>(p2.Key);
				p2.Value.level = UnityEngine.Random.Range(0, star.maxStarLevel + 1);
			}
		}
		DewSave.SaveProfile();
	}

	[DewConsoleMethod("Set stardust amount of this profile to some value.")]
	public static void Stardust(int value)
	{
		DewSave.profile.stardust = value;
		DewSave.SaveProfile();
	}

	[DewConsoleMethod("Add or deduct mastery point from hero.")]
	public static void MasteryAdd(string hero, long points)
	{
		foreach (KeyValuePair<string, DewProfile.HeroMasteryData> pair in DewSave.profile.heroMasteries)
		{
			if (pair.Key.ToLower().Contains(hero.ToLower()))
			{
				Log($"BEFORE {hero} ({pair.Value.currentLevel:#,##0}) {pair.Value.currentPoints:#,##0}/{Dew.GetRequiredMasteryPointsToLevelUp(pair.Value.currentLevel):#,##0} ({pair.Value.totalPoints:#,##0})");
				if (points > 0)
				{
					DewSave.profile.AddMasteryPoints(pair.Key, points);
				}
				else
				{
					DewSave.profile.RemoveMasteryPoints(pair.Key, points);
				}
				Log($"AFTER {hero} ({pair.Value.currentLevel:#,##0}) {pair.Value.currentPoints:#,##0}/{Dew.GetRequiredMasteryPointsToLevelUp(pair.Value.currentLevel):#,##0} ({pair.Value.totalPoints:#,##0})");
				DewSave.SaveProfile();
				return;
			}
		}
		Log("Hero '" + hero + "' not found");
	}

	[DewConsoleMethod("Start a quest.", CommandType.GameServerCheat)]
	public static void Quest(string substr)
	{
		DewQuest q = DewResources.FindOneByTypeSubstring<DewQuest>(substr);
		if (q == null)
		{
			Log("Could not find quest with substring: " + substr);
		}
		else
		{
			NetworkedManagerBase<QuestManager>.instance.StartQuest(q);
		}
	}

	[DewConsoleMethod("Complete an ongoing quest.", CommandType.GameServerCheat)]
	public static void QuestComplete(string substr)
	{
		DewQuest[] array = NetworkedManagerBase<QuestManager>.instance.activeQuests.ToArray();
		foreach (DewQuest q in array)
		{
			if (q.GetType().Name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
			{
				q.CompleteQuest();
			}
		}
	}

	[DewConsoleMethod("Fail an ongoing quest.", CommandType.GameServerCheat)]
	public static void QuestFail(string substr)
	{
		DewQuest[] array = NetworkedManagerBase<QuestManager>.instance.activeQuests.ToArray();
		foreach (DewQuest q in array)
		{
			if (q.GetType().Name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
			{
				q.FailQuest(QuestFailReason.NotSpecified);
			}
		}
	}

	[DewConsoleMethod("Complete all ongoing quests.", CommandType.GameServerCheat)]
	public static void QuestCompleteAll()
	{
		DewQuest[] array = NetworkedManagerBase<QuestManager>.instance.activeQuests.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CompleteQuest();
		}
	}

	[DewConsoleMethod("Fail all ongoing quests.", CommandType.GameServerCheat)]
	public static void QuestFailAll()
	{
		DewQuest[] array = NetworkedManagerBase<QuestManager>.instance.activeQuests.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].FailQuest(QuestFailReason.NotSpecified);
		}
	}

	[DewConsoleMethod("DewResources show loaded", CommandType.Anywhere)]
	public static void Res()
	{
		Debug.Log($"DewResources has {DewResources.loadedGuids.Count} objects");
	}

	[DewConsoleMethod("DewResources show loaded verbose", CommandType.Anywhere)]
	public static void ResVerbose()
	{
		Debug.Log($"DewResources has {DewResources.loadedGuids.Count} objects");
		foreach (KeyValuePair<string, AsyncOperationHandle<UnityEngine.Object>> p in (Dictionary<string, AsyncOperationHandle<UnityEngine.Object>>)typeof(DewResources).GetField("_loadedObjects", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null))
		{
			switch (p.Value.Status)
			{
			case AsyncOperationStatus.None:
			case AsyncOperationStatus.Failed:
				Debug.Log(p.Key + ": " + p.Value.Status);
				break;
			case AsyncOperationStatus.Succeeded:
				Debug.Log(p.Key + ": " + p.Value.Result);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	[DewConsoleMethod("DewResources unload everything", CommandType.Anywhere)]
	public static void ResUnloadUnused()
	{
		DewResources.UnloadUnused();
	}

	[DewConsoleMethod("DewResources load everything", CommandType.Anywhere)]
	public static void ResLoad(string pattern)
	{
		IEnumerable<string> first = from p in DewResources.database.typeToGuid
			where p.Key.Name.EqualsWildcard(pattern)
			select p.Value;
		IEnumerable<string> fromNames = from p in DewResources.database.nameToGuid
			where p.Key.EqualsWildcard(pattern)
			select p.Value;
		string[] selected = Enumerable.Concat(second: DewResources.database.allGuids.Where((string p) => p.EqualsWildcard(pattern)), first: first.Concat(fromNames)).Distinct().ToArray();
		Log($"Selected {selected.Length} objects");
		selected.ForEach(delegate(string guid)
		{
			DewResources.Load(guid);
		});
		Res();
	}

	[DewConsoleMethod("DewResources preload everything", CommandType.Anywhere)]
	public static void ResPreload(string pattern)
	{
		IEnumerable<string> first = from p in DewResources.database.typeToGuid
			where p.Key.Name.EqualsWildcard(pattern)
			select p.Value;
		IEnumerable<string> fromNames = from p in DewResources.database.nameToGuid
			where p.Key.EqualsWildcard(pattern)
			select p.Value;
		string[] selected = Enumerable.Concat(second: DewResources.database.allGuids.Where((string p) => p.EqualsWildcard(pattern)), first: first.Concat(fromNames)).Distinct().ToArray();
		Log($"Selected {selected.Length} objects");
		selected.ForEach(delegate(string guid)
		{
			DewResources.Preload(guid);
		});
		Res();
	}

	[DewConsoleMethod("DewResources toggle verbose logs", CommandType.Anywhere)]
	public static void ResLog()
	{
		DewResources.EnableVerboseLogging = !DewResources.EnableVerboseLogging;
		Log("DewResources verbose logging is now " + (DewResources.EnableVerboseLogging ? "ON" : "OFF"));
	}

	[DewConsoleMethod("Reset reverie save data completely.", CommandType.Anywhere)]
	public static void RevReset()
	{
		DewSave.profile.remainingRerolls = 3;
		DewSave.profile.completedReveries = 0;
		DewSave.profile.lastReverieTypes.Clear();
		DewSave.profile.reverieSlots.Clear();
		DewSave.profile.Validate();
		DewSave.ApplySettings();
		Debug.Log("Reverie has been reset.");
	}

	[DewConsoleMethod("Clear current reveries.", CommandType.Anywhere)]
	public static void RevClear()
	{
		for (int i = 0; i < DewSave.profile.reverieSlots.Count; i++)
		{
			DewSave.profile.reverieSlots[i] = new DewProfile.DailyReverieData
			{
				type = null,
				nextRefillTimestamp = DateTime.UtcNow.AddSeconds(1 + i).ToTimestamp()
			};
		}
		DewReverie.ClearSpecialReverie();
		DewSave.ApplySettings();
		Debug.Log("Reverie has been cleared.");
	}

	[DewConsoleMethod("Complete all reveries.", CommandType.Anywhere)]
	public static void RevComplete()
	{
		for (int i = 0; i < DewSave.profile.reverieSlots.Count; i++)
		{
			if (!DewSave.profile.reverieSlots[i].IsEmpty())
			{
				DewSave.profile.reverieSlots[i].isComplete = true;
			}
		}
		if (!DewSave.profile.specialReverie.IsEmpty())
		{
			DewSave.profile.specialReverie.isComplete = true;
		}
		DewSave.ApplySettings();
		Debug.Log("All reveries has been completed.");
	}

	[DewConsoleMethod("Set special reverie.", CommandType.Anywhere)]
	public static void RevSpecial(string substr)
	{
		foreach (DewReverieItem r in Dew.allReveries)
		{
			if (r is DewSpecialReverieItem && r.GetType().Name.ToLower().Contains(substr.ToLower()))
			{
				DewReverie.SetSpecialReverie(r.GetType());
				DewSave.ApplySettings();
				Debug.Log("Setting special reverie: " + r.GetType().Name);
				break;
			}
		}
	}

	[DewConsoleMethod("Simulate auth expiration callback.", CommandType.Anywhere)]
	public static void EOSSimulateAuthExpire()
	{
		AuthExpirationCallbackInfo info = default(AuthExpirationCallbackInfo);
		EOSSDKComponent.Instance.OnAuthExpiration(ref info);
	}

	[DewConsoleMethod("Reset EOS.", CommandType.Anywhere)]
	public static void EOSReset()
	{
		ManagerBase<EOSManager>.instance.ResetEOS();
	}

	[DewConsoleMethod("Delete persistent auth.", CommandType.Anywhere)]
	public static void EOSDeletePersistentAuth()
	{
		Log("Start delete persistent auth");
		DeleteDeviceIdOptions opt = default(DeleteDeviceIdOptions);
		EOSSDKComponent.GetConnectInterface().DeleteDeviceId(ref opt, null, delegate(ref DeleteDeviceIdCallbackInfo data)
		{
			Log($"Delete device id returned: {data.ResultCode}");
			Log("Trying to create device id");
			CreateDeviceIdOptions options = new CreateDeviceIdOptions
			{
				DeviceModel = EOSSDKComponent.Instance.deviceModel
			};
			EOSSDKComponent.GetConnectInterface().CreateDeviceId(ref options, null, EOSSDKComponent.Instance.OnCreateDeviceId);
		});
		DeletePersistentAuthOptions a = new DeletePersistentAuthOptions
		{
			RefreshToken = null
		};
		EOSSDKComponent.GetAuthInterface().DeletePersistentAuth(ref a, null, delegate(ref DeletePersistentAuthCallbackInfo data)
		{
			Log($"Delete persistent auth returned: {data.ResultCode}");
		});
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetQ(string substring)
	{
		SetS(0, substring, 1);
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetQ(string substring, int level)
	{
		SetS(0, substring, level);
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetW(string substring)
	{
		SetS(1, substring, 1);
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetW(string substring, int level)
	{
		SetS(1, substring, level);
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetE(string substring)
	{
		SetS(2, substring, 1);
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetE(string substring, int level)
	{
		SetS(2, substring, level);
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetR(string substring)
	{
		SetS(3, substring, 1);
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetR(string substring, int level)
	{
		SetS(3, substring, level);
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetD(string substring)
	{
		SetS(4, substring, 1);
	}

	[DewConsoleMethod("Set my skill", CommandType.GameServerCheat)]
	public static void SetD(string substring, int level)
	{
		SetS(4, substring, level);
	}

	[DewConsoleMethod("Set my ability to something else.", CommandType.GameServerCheat)]
	private static void SetS(int index, string substring, int level)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		AbilityTrigger trg = ((!(ent is Hero)) ? DewResources.FindOneByTypeSubstring<AbilityTrigger>(substring) : DewResources.FindOneByTypeSubstring<SkillTrigger>(substring));
		if (trg == null)
		{
			Log("Could not find ability with query: " + substring);
			return;
		}
		AbilityTrigger newTrg = ((!(trg is SkillTrigger strg)) ? Dew.CreateAbilityTrigger(trg) : Dew.CreateSkillTrigger(strg, Vector3.zero, level));
		if (ent is Hero hero)
		{
			if (hero.Skill.TryGetSkill((HeroSkillLocation)index, out var skill))
			{
				hero.Skill.UnequipSkill((HeroSkillLocation)index, hero.position, ignoreCanReplace: true);
				skill.Destroy();
			}
			hero.Skill.EquipSkill((HeroSkillLocation)index, (SkillTrigger)newTrg, ignoreCanReplace: true);
		}
		else
		{
			ent.Ability.SetAbility(index, newTrg);
		}
		Log($"Set entity's ability ({index}) to {newTrg}");
	}

	[DewConsoleMethod("Set selected entity's attack ability to something else.", CommandType.GameServerCheat)]
	public static void SetA(string substring)
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
			return;
		}
		AbilityTrigger trigger = null;
		AttackTrigger atrg = DewResources.FindOneByTypeSubstring<AttackTrigger>(substring);
		if (atrg == null)
		{
			Log("Could not find attack ability with query: " + substring);
			return;
		}
		Dew.CreateAbilityTrigger(atrg);
		ent.Ability.SetAttackAbility(atrg);
		Log($"Set entity's attack ability to {trigger}");
	}

	[DewConsoleMethod("Spawn a skill at cursor position with specified level", CommandType.GameServerCheat)]
	public static void SpawnSkill(string substring, int level)
	{
		SkillTrigger skill = DewResources.FindOneByTypeSubstring<SkillTrigger>(substring);
		if (skill == null)
		{
			Log("Could not find skill with query: " + substring);
			return;
		}
		Dew.CreateSkillTrigger(skill, GetCursorWorldPos(), level);
		Log($"Created {skill} of level {level}");
	}

	[DewConsoleMethod("Spawn a skill at cursor position with level 1", CommandType.GameServerCheat)]
	public static void SpawnSkill(string substring)
	{
		SpawnSkill(substring, 1);
	}

	[DewConsoleMethod("Spawn a gem at cursor position with specified level", CommandType.GameServerCheat)]
	public static void SpawnGem(string substring, int level)
	{
		Gem gem = DewResources.FindOneByTypeSubstring<Gem>(substring);
		if (gem == null)
		{
			Log("Could not find gem with query: " + substring);
			return;
		}
		Dew.CreateGem(gem, GetCursorWorldPos(), level);
		Log($"Created {gem} of level {level}");
	}

	[DewConsoleMethod("Spawn a gem at cursor position with level 1", CommandType.GameServerCheat)]
	public static void SpawnGem(string substring)
	{
		SpawnGem(substring, 100);
	}

	[DewConsoleMethod("Set my skill's level to some value", CommandType.GameServerCheat)]
	public static void LevelSkill(string type, int level)
	{
		SkillTrigger skill = DewPlayer.local.hero.Skill.GetSkill(GetSkillType(type));
		if (skill == null)
		{
			Log("No skill to set level: " + type.ToUpper());
		}
		else
		{
			skill.level = level;
		}
	}

	[DewConsoleMethod("Set my gem's quality to some value", CommandType.GameServerCheat)]
	public static void Quality(string type, int index, int quality)
	{
		Hero hero = DewPlayer.local.hero;
		HeroSkillLocation skillType = GetSkillType(type);
		foreach (KeyValuePair<GemLocation, Gem> p in hero.Skill.GetGemsPairInSkill(skillType))
		{
			if (p.Key.index == index)
			{
				p.Value.quality = quality;
				return;
			}
		}
		Log($"No gem to set quality: {skillType}[{index}]");
	}

	[DewConsoleMethod("Get equipment string of target hero", CommandType.GameServer)]
	public static void GetEquipment()
	{
		if (!TryGetConsoleSelectedEntity(out var entity))
		{
			PrintNoEntitySelected();
		}
		if (!(entity is Hero h))
		{
			Log("Selected entity is not hero");
			return;
		}
		Log("Equipment string of hero " + h.GetActorReadableName() + ":");
		Log(new AnalyticsEquipmentData(h).ToBase64());
	}

	[DewConsoleMethod("Set equipment of my hero", CommandType.GameServerCheat)]
	public static void SetEquipment(string base64)
	{
		if (!TryGetConsoleSelectedEntity(out var entity))
		{
			PrintNoEntitySelected();
		}
		if (!(entity is Hero h))
		{
			Log("Selected entity is not hero");
			return;
		}
		try
		{
			AnalyticsEquipmentData equipment = new AnalyticsEquipmentData(base64);
			Log($"Got equipment with {equipment.skills.Count} skills and {equipment.gems.Count} gems");
			if (h.Skill.Q != null)
			{
				h.Skill.UnequipSkill(HeroSkillLocation.Q, default(Vector3), ignoreCanReplace: true).Destroy();
			}
			if (h.Skill.W != null)
			{
				h.Skill.UnequipSkill(HeroSkillLocation.W, default(Vector3), ignoreCanReplace: true).Destroy();
			}
			if (h.Skill.E != null)
			{
				h.Skill.UnequipSkill(HeroSkillLocation.E, default(Vector3), ignoreCanReplace: true).Destroy();
			}
			if (h.Skill.R != null)
			{
				h.Skill.UnequipSkill(HeroSkillLocation.R, default(Vector3), ignoreCanReplace: true).Destroy();
			}
			if (h.Skill.Identity != null)
			{
				h.Skill.UnequipSkill(HeroSkillLocation.Identity, default(Vector3), ignoreCanReplace: true).Destroy();
			}
			foreach (GemLocation loc in new List<GemLocation>(h.Skill.gems.Keys))
			{
				h.Skill.UnequipGem(loc, default(Vector3)).Destroy();
			}
			foreach (var s in equipment.skills)
			{
				SetS((int)s.Item1, s.Item2, s.Item3);
			}
			foreach (var g in equipment.gems)
			{
				Gem prefab = DewResources.FindOneByTypeSubstring<Gem>(g.Item2);
				if (prefab == null)
				{
					Log("Gem " + g.Item2 + " not found");
					continue;
				}
				Gem gem = Dew.CreateGem(prefab, default(Vector3), g.Item3);
				h.Skill.EquipGem(g.Item1, gem);
			}
		}
		catch (Exception message)
		{
			Log("Set equipment failed due to an exception:");
			Log(message);
		}
	}

	private static HeroSkillLocation GetSkillType(string trgstr)
	{
		switch (trgstr.ToLower())
		{
		case "q":
			return HeroSkillLocation.Q;
		case "w":
			return HeroSkillLocation.W;
		case "e":
			return HeroSkillLocation.E;
		case "r":
			return HeroSkillLocation.R;
		case "d":
		case "trait":
			return HeroSkillLocation.Identity;
		case "m":
			return HeroSkillLocation.Movement;
		default:
			throw new ArgumentOutOfRangeException("trgstr", trgstr);
		}
	}

	private static int GetIndex(string trgstr)
	{
		switch (trgstr.ToLower())
		{
		case "q":
			return 0;
		case "w":
			return 1;
		case "e":
			return 2;
		case "r":
			return 3;
		case "d":
		case "trait":
			return 4;
		default:
		{
			if (int.TryParse(trgstr, NumberStyles.Any, CultureInfo.InvariantCulture, out var index))
			{
				return index;
			}
			Log(trgstr + " isn't a valid ability index");
			return -1;
		}
		}
	}

	[DewConsoleMethod("Adjust UI scale", CommandType.Anywhere)]
	public static void UIScale(float scale)
	{
		DewSave.profile.gameplay.uiScale = scale;
		DewSave.SaveProfile();
		DewSave.ApplySettings();
	}

	[DewConsoleMethod("Toggle game overs", CommandType.GameServerCheat)]
	public static void GameOverToggle()
	{
		NetworkedManagerBase<GameManager>.instance.isGameOverEnabled = !NetworkedManagerBase<GameManager>.instance.isGameOverEnabled;
		if (NetworkedManagerBase<GameManager>.instance.isGameOverEnabled)
		{
			Log("Automatic game over is now enabled");
		}
		else
		{
			Log("Automatic game over is now disabled");
		}
	}

	[DewConsoleMethod("Toggle debug selective visibility", CommandType.Anywhere)]
	public static void EffFailSelectiveVisibility()
	{
		FxSelectiveVisibility.forceFail = !FxSelectiveVisibility.forceFail;
		if (FxSelectiveVisibility.forceFail)
		{
			Log("Selective visibility check will now ALWAYS FAIL");
		}
		else
		{
			Log("Selective visibility check will now be performed normally");
		}
	}

	[DewConsoleMethod("Toggle disabling of effects", CommandType.Anywhere)]
	public static void EffDisablePlay()
	{
		DewEffect.disablePlay = !DewEffect.disablePlay;
		if (DewEffect.disablePlay)
		{
			Log("DewEffect.Play is now disabled");
		}
		else
		{
			Log("DewEffect.Play is now enabled");
		}
	}

	[DewConsoleMethod("Toggle disabling of effects", CommandType.Anywhere)]
	public static void EffDisablePlayNew()
	{
		DewEffect.disablePlayNew = !DewEffect.disablePlayNew;
		if (DewEffect.disablePlayNew)
		{
			Log("DewEffect.PlayNew is now disabled");
		}
		else
		{
			Log("DewEffect.PlayNew is now enabled");
		}
	}

	[DewConsoleMethod("Test music loop", CommandType.Anywhere)]
	public static void TestMusicLoop()
	{
		ManagerBase<MusicManager>.instance._source.time = ManagerBase<MusicManager>.instance._source.clip.length - 4f;
	}

	[DewConsoleMethod("Show a test message", CommandType.Anywhere)]
	public static void Message()
	{
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			rawContent = "* It's the end."
		});
	}

	[DewConsoleMethod("Set volumetric fog parameters", CommandType.Anywhere)]
	public static void FogDownscaling(float value)
	{
		value = Mathf.Clamp(value, 1f, 8f);
		VolumetricFogManager[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFogManager>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].downscaling = value;
		}
		Log("Set downscaling to " + value);
	}

	[DewConsoleMethod("Set volumetric fog parameters", CommandType.Anywhere)]
	public static void FogRaymarchQuality(int value)
	{
		value = Mathf.Clamp(value, 1, 16);
		VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
		foreach (VolumetricFog m in array)
		{
			m.profile.raymarchQuality = value;
			if (m.gameObject.activeSelf)
			{
				m.gameObject.SetActive(value: false);
				m.gameObject.SetActive(value: true);
			}
		}
		Log("Set raymarch quality to " + value);
	}

	[DewConsoleMethod("Set volumetric fog parameters", CommandType.Anywhere)]
	public static void FogJitter(int value)
	{
		VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].profile.jittering = value;
		}
		Log("Set jittering to " + value);
	}

	[DewConsoleMethod("Test TransitionManager", CommandType.Anywhere)]
	public static void FadeIn()
	{
		ManagerBase<TransitionManager>.instance.FadeIn();
	}

	[DewConsoleMethod("Test TransitionManager", CommandType.Anywhere)]
	public static void FadeOut()
	{
		ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
	}

	[DewConsoleMethod("Test Exception", CommandType.Anywhere)]
	public static void TestException()
	{
		throw new Exception("This is a test exception");
	}

	[DewConsoleMethod("Change force vote debug flag", CommandType.GameServerCheat)]
	public static void VoteDebugForceOn()
	{
		NetworkedManagerBase<ZoneManager>.instance.forceVoteForDebug = true;
		Log("Force vote flag is now FORCE ON");
	}

	[DewConsoleMethod("Change force vote debug flag", CommandType.GameServerCheat)]
	public static void VoteDebugForceOff()
	{
		NetworkedManagerBase<ZoneManager>.instance.forceVoteForDebug = false;
		Log("Force vote flag is now FORCE OFF");
	}

	[DewConsoleMethod("Change force vote debug flag", CommandType.GameServerCheat)]
	public static void VoteDebugDefault()
	{
		NetworkedManagerBase<ZoneManager>.instance.forceVoteForDebug = null;
		Log("Force vote flag is cleared");
	}

	[DewConsoleMethod("Show world text", CommandType.Game)]
	public static void Text(string txt)
	{
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = txt,
			worldPos = GetCursorWorldPos()
		});
	}

	[DewConsoleMethod("Get info about current build profile.")]
	public static void Build()
	{
		DewBuildProfile p = DewBuildProfile.current;
		Log("Showing build profile used for '" + Application.version + "'");
		FieldInfo[] fields = typeof(DewBuildProfile).GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo f in fields)
		{
			object val = f.GetValue(p);
			if (val is IList list)
			{
				Log($"- {f.Name}: {f.FieldType} ({list.Count} elements)");
				foreach (object v in list)
				{
					Log($"   * {v}");
				}
			}
			else
			{
				Log($"- {f.Name}: {val}");
			}
		}
		Log("End of build profile info.");
	}

	[DewConsoleMethod("Display population info.", CommandType.GameServer)]
	public static void Pop()
	{
		Log(string.Format("Pop: {0}/{1} {2}", NetworkedManagerBase<GameManager>.instance.spawnedPopulation, NetworkedManagerBase<GameManager>.instance.maxSpawnedPopulation, NetworkedManagerBase<GameManager>.instance.isSpawnOverPopulation ? "(Overpopulation)" : ""));
		Log($"Spawned Population Multiplier: {NetworkedManagerBase<GameManager>.instance.GetAdjustedMonsterSpawnPopulation(1f):0.00}x");
	}

	[DewConsoleMethod("Sets room index in game session.", CommandType.GameServerCheat)]
	public static void RoomIndex(int index)
	{
		NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex = index;
	}

	[DewConsoleMethod("Set time scale to some value.", CommandType.GameServerCheat)]
	public static void TimeScale(float value)
	{
		NetworkedManagerBase<TimescaleManager>.instance.desiredTimescale = value;
	}

	[DewConsoleMethod("Toggle time scale between 1.0 and 0.0.", CommandType.GameServerCheat)]
	public static void Pause()
	{
		NetworkedManagerBase<TimescaleManager>.instance.desiredTimescale = ((NetworkedManagerBase<TimescaleManager>.instance.desiredTimescale == 0f) ? 1f : 0f);
		Log((NetworkedManagerBase<TimescaleManager>.instance.desiredTimescale == 0f) ? "Game paused" : "Game resumed");
	}

	[DewConsoleMethod("Set max frames per second")]
	public static void Fps(int frameCount)
	{
		Application.targetFrameRate = frameCount;
	}

	[DewConsoleMethod("Enable or disable cheats", CommandType.Game)]
	public static void Cheats(bool enable)
	{
		if (!NetworkServer.active)
		{
			LogWarning("Cheats can only be toggled on/off on server");
		}
		else if (enable)
		{
			NetworkedManagerBase<ConsoleManager>.instance.EnableCheats();
		}
		else
		{
			NetworkedManagerBase<ConsoleManager>.instance.DisableCheats();
		}
	}

	[DewConsoleMethod("Enable or disable actor name change every logic update (Performance will suffer)")]
	public static void UsefulActorName(bool enable)
	{
		ActorManager.enableUsefulActorName = enable;
	}

	[DewConsoleMethod("Show FPS")]
	public static void ShowFPS(bool enable)
	{
		ManagerBase<GlobalLogicPackage>.instance.showFps = enable;
	}

	[DewConsoleMethod("Show FPS")]
	public static void ShowFPS()
	{
		ManagerBase<GlobalLogicPackage>.instance.showFps = !ManagerBase<GlobalLogicPackage>.instance.showFps;
	}

	[DewConsoleMethod("Show current stats about loot", CommandType.GameServer)]
	public static void LootStat()
	{
		Log($"{NetworkedManagerBase<LootManager>.instance.lootInstances.Count} loots active:");
		foreach (KeyValuePair<Type, Loot> l in NetworkedManagerBase<LootManager>.instance.lootInstances)
		{
			Log($" - {l.Key.Name}: {l.Value.currentChance:0.000}");
		}
	}

	[DewConsoleMethod("Set ambient level to some value.", CommandType.GameServerCheat)]
	public static void Ambient(int value)
	{
		NetworkedManagerBase<GameManager>.instance.SetAmbientLevel(value);
		Log($"Ambient level set to {value}");
	}

	[DewConsoleMethod("Get ambient level.", CommandType.GameServer)]
	public static void Ambient()
	{
		Log($"Ambient level is {NetworkedManagerBase<GameManager>.instance.ambientLevel}");
	}

	[DewConsoleMethod("Set zone index to some value.", CommandType.GameServerCheat)]
	public static void ZoneIndex(int value)
	{
		NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex = value;
		Log($"Zone index set to {value}");
	}

	[DewConsoleMethod("Get zone index.", CommandType.GameServer)]
	public static void ZoneIndex()
	{
		Log($"Zone index is {NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex}");
	}

	[DewConsoleMethod("Set difficulty.", CommandType.GameServerCheat)]
	public static void Difficulty(string substring)
	{
		DewDifficultySettings diff = DewResources.FindOneByIdSubstring<DewDifficultySettings>(substring);
		if (diff == null)
		{
			LogWarning("Difficulty not found via substring: " + substring);
		}
		else
		{
			NetworkedManagerBase<GameManager>.instance.difficulty = diff;
		}
	}

	[DewConsoleMethod("Get current difficulty.", CommandType.Game)]
	public static void Difficulty()
	{
		Log(NetworkedManagerBase<GameManager>.instance.difficulty.name);
	}

	[DewConsoleMethod("Game over.", CommandType.GameServerCheat)]
	public static void ConcludeGameOver()
	{
		NetworkedManagerBase<GameManager>.instance.WrapUpAndShowResult(DewGameResult.ResultType.GameOver);
	}

	[DewConsoleMethod("Demo finish.", CommandType.GameServerCheat)]
	public static void ConcludeDemoFinish()
	{
		NetworkedManagerBase<GameManager>.instance.ConcludeDemo();
	}

	[DewConsoleMethod("Change profile name")]
	public static void ProfileName(string newName)
	{
		DewSave.profile.name = newName;
		DewSave.SaveProfile();
	}

	[DewConsoleMethod("Show profile name")]
	public static void ProfileName()
	{
		Log(DewSave.profile.name);
	}

	[DewConsoleMethod("Save profile")]
	public static void ProfileSave()
	{
		DewSave.SaveProfile();
	}

	[DewConsoleMethod("Toggle Cinematic Helper", CommandType.Game)]
	public static void CinematicHelper()
	{
		CinematicCameraHelper obj = UnityEngine.Object.FindObjectOfType<CinematicCameraHelper>();
		if (obj == null)
		{
			Log("Cinematic helper not present");
			return;
		}
		obj.isCinematicHelperEnabled = !obj.isCinematicHelperEnabled;
		Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			ICinematicCameraHelperStateReceiver[] components = array[i].GetComponents<ICinematicCameraHelperStateReceiver>();
			foreach (ICinematicCameraHelperStateReceiver receiver in components)
			{
				try
				{
					receiver.OnCinematicCameraHelperChanged(obj.isCinematicHelperEnabled);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, receiver as UnityEngine.Object);
				}
			}
		}
		Log("Cinematic helper is now " + (obj.isCinematicHelperEnabled ? "ON" : "OFF"));
	}

	[DewConsoleMethod("Change language")]
	public static void Lang(string substring)
	{
		string[] array = DewLocalization.buildData.dataByLanguage.Keys.ToArray();
		string target = "";
		string[] array2 = array;
		foreach (string l in array2)
		{
			if (l.Contains(substring, StringComparison.OrdinalIgnoreCase))
			{
				target = l;
			}
		}
		if (target == "")
		{
			Log("Requested language not found");
			return;
		}
		Log("Changing language from " + DewSave.profile.language + " to " + target);
		DewSave.profile.language = target;
		DewSave.SaveProfile();
		DewSave.ApplySettings();
	}

	[DewConsoleMethod("Toggle lang test")]
	public static void LangTest()
	{
		GameObject existing = GameObject.Find("LangTester");
		if (existing != null)
		{
			UnityEngine.Object.Destroy(existing);
			Log("Stopped lang test");
		}
		else
		{
			Coroutiner coroutiner = new GameObject("LangTester").AddComponent<Coroutiner>();
			UnityEngine.Object.DontDestroyOnLoad(coroutiner);
			coroutiner.StartCoroutine(Routine());
		}
		static IEnumerator Routine()
		{
			while (true)
			{
				SetLang("en-US");
				yield return new WaitForSeconds(0.3f);
				SetLang("ko-KR");
				yield return new WaitForSeconds(0.3f);
				SetLang("zh-CN");
				yield return new WaitForSeconds(0.3f);
				SetLang("es-MX");
				yield return new WaitForSeconds(0.3f);
				SetLang("ja-JP");
				yield return new WaitForSeconds(0.3f);
				SetLang("ru-RU");
				yield return new WaitForSeconds(0.3f);
			}
		}
		static void SetLang(string lang)
		{
			DewSave.profile.language = lang;
			Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive: true);
			for (int i = 0; i < array.Length; i++)
			{
				ILangaugeChangedCallback[] components = array[i].GetComponents<ILangaugeChangedCallback>();
				foreach (ILangaugeChangedCallback receiver in components)
				{
					try
					{
						receiver.OnLanguageChanged();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception, receiver as UnityEngine.Object);
					}
				}
			}
		}
	}

	[DewConsoleMethod("Show hitstop effect", CommandType.GameServerCheat)]
	public static void HitStop()
	{
		if (!TryGetConsoleSelectedEntity(out var ent))
		{
			PrintNoEntitySelected();
		}
		else
		{
			NetworkedManagerBase<FlavourManager>.instance.FxPlayNewNetworked(NetworkedManagerBase<FlavourManager>.instance.hitStopDealDamage, ent);
		}
	}
}
