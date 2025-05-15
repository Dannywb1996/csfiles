using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DewGameResult
{
	public struct SkillData
	{
		public uint netId;

		public HeroSkillLocation loc;

		public SkillType type;

		public string name;

		public int level;

		public int maxCharges;

		public float cooldownTime;

		public Dictionary<string, string> capturedTooltipFields;

		public SkillData(HeroSkillLocation loc, SkillTrigger skill)
		{
			netId = skill.netId;
			this.loc = loc;
			name = skill.GetType().Name;
			level = skill.level;
			maxCharges = skill.configs[0].maxCharges;
			cooldownTime = Mathf.Max(0f, skill.GetMaxCooldownTime(0));
			type = skill.type;
			capturedTooltipFields = new Dictionary<string, string>();
		}

		public void CaptureTooltipFields(SkillTrigger skill)
		{
			DewLocalization.CaptureDescriptionExpressions(DewLocalization.GetSkillDescription(DewLocalization.GetSkillKey(skill.GetType()), 0), capturedTooltipFields, new DewLocalization.DescriptionSettings
			{
				contextEntity = skill.owner,
				contextObject = skill
			});
		}

		public SkillTrigger GetSkillTrigger()
		{
			return DewResources.GetByShortTypeName<SkillTrigger>(name);
		}
	}

	public struct GemData
	{
		public uint netId;

		public GemLocation location;

		public string name;

		public int quality;

		public Dictionary<string, string> capturedTooltipFields;

		public GemData(GemLocation loc, Gem gem)
		{
			netId = gem.netId;
			location = loc;
			name = gem.GetType().Name;
			quality = gem.quality;
			capturedTooltipFields = new Dictionary<string, string>();
		}

		public void CaptureTooltipFields(Gem gem)
		{
			DewLocalization.CaptureDescriptionExpressions(DewLocalization.GetGemDescription(DewLocalization.GetGemKey(gem.GetType())), capturedTooltipFields, new DewLocalization.DescriptionSettings
			{
				contextEntity = gem.owner,
				contextObject = gem
			});
		}

		public Gem GetGem()
		{
			return DewResources.GetByShortTypeName<Gem>(name);
		}
	}

	public class PlayerData
	{
		public bool isLocalPlayer;

		public uint playerNetId;

		public string playerProfileName;

		public string heroType;

		public float maxHealth;

		public float attackDamage;

		public float abilityPower;

		public float skillHaste;

		public float attackSpeed;

		public float fireAmp;

		public float critChance;

		public int level;

		public int kills;

		public int heroicBossKills;

		public int miniBossKills;

		public int hunterKills;

		public int totalGoldIncome;

		public int totalDreamDustIncome;

		public int totalStardustIncome;

		public int deaths;

		public int combatTime;

		public float dealtDamageToEnemies;

		public float maxDealtSingleDamageToEnemy;

		public float healToSelf;

		public float healToOthers;

		public float receivedDamage;

		public string causeOfDeathActor = "";

		public string causeOfDeathEntity = "";

		public List<PlayerStarItem> stars = new List<PlayerStarItem>();

		public Dictionary<string, string> capturedStarTooltipFields = new Dictionary<string, string>();

		public List<SkillData> skills = new List<SkillData>();

		public List<GemData> gems = new List<GemData>();

		public List<int> maxGemCounts = new List<int>();

		public bool TryGetSkillData(HeroSkillLocation type, out SkillData data)
		{
			int i = skills.FindIndex((SkillData s) => s.loc == type);
			if (i < 0)
			{
				data = default(SkillData);
				return false;
			}
			data = skills[i];
			return true;
		}

		public bool TryGetGemData(GemLocation location, out GemData data)
		{
			int i = gems.FindIndex((GemData g) => g.location == location);
			if (i < 0)
			{
				data = default(GemData);
				return false;
			}
			data = gems[i];
			return true;
		}

		internal DewPlayer GetPlayer()
		{
			return DewPlayer.humanPlayers.FirstOrDefault((DewPlayer h) => h.netId == playerNetId);
		}
	}

	public enum ResultType
	{
		Conceded,
		GameOver,
		DemoFinish
	}

	public ResultType result;

	public long startTimestamp;

	public int elapsedGameTimeSeconds;

	public int visitedWorlds = 1;

	public int visitedLocations = 1;

	public string difficulty;

	public List<PlayerData> players = new List<PlayerData>();
}
