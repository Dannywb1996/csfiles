using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Monster : Entity
{
	public enum MonsterType : byte
	{
		Lesser,
		Normal,
		MiniBoss,
		Boss
	}

	[SyncVar]
	public MonsterType type = MonsterType.Normal;

	public float chaseRandomness = 0.5f;

	public float chasePredictiveness = 0.5f;

	[CompilerGenerated]
	[SyncVar(hook = "OnPopulationChanged")]
	[SerializeField]
	private float _003CpopulationCost_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisHunter_003Ek__BackingField;

	protected override DewPlayer defaultOwner => DewPlayer.creep;

	public virtual Vector3? spawnPosOverride => null;

	public virtual Quaternion? spawnRotOverride => null;

	public float populationCost
	{
		[CompilerGenerated]
		get
		{
			return _003CpopulationCost_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CpopulationCost_003Ek__BackingField = value;
		}
	} = 1f;

	public bool isHunter
	{
		[CompilerGenerated]
		get
		{
			return _003CisHunter_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisHunter_003Ek__BackingField = value;
		}
	}

	public Vector3? campPosition { get; internal set; }

	public MonsterType Networktype
	{
		get
		{
			return type;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref type, 16uL, null);
		}
	}

	public float Network_003CpopulationCost_003Ek__BackingField
	{
		get
		{
			return populationCost;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref populationCost, 32uL, OnPopulationChanged);
		}
	}

	public bool Network_003CisHunter_003Ek__BackingField
	{
		get
		{
			return isHunter;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isHunter, 64uL, null);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		float healthP = NetworkedManagerBase<GameManager>.instance.GetMonsterBonusHealthPercentageByMultiplayer(this);
		float statP = NetworkedManagerBase<GameManager>.instance.GetMonsterBonusPowerPercentage(this);
		base.Status.AddStatBonus(new StatBonus
		{
			maxHealthPercentage = healthP,
			attackDamagePercentage = statP,
			abilityPowerPercentage = statP
		});
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer && !campPosition.HasValue)
		{
			NetworkedManagerBase<GameManager>.instance.UpdateSpawnedPopulation();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && !(NetworkedManagerBase<GameManager>.instance == null) && !campPosition.HasValue)
		{
			NetworkedManagerBase<GameManager>.instance.UpdateSpawnedPopulation();
		}
	}

	private void OnPopulationChanged(float oldValue, float newValue)
	{
		if (base.isServer && base.isActive && !campPosition.HasValue)
		{
			NetworkedManagerBase<GameManager>.instance.UpdateSpawnedPopulation();
		}
	}

	protected override StaggerSettings GetStaggerSettings()
	{
		return type switch
		{
			MonsterType.Lesser => StaggerSettings.LesserMonsterDefault, 
			MonsterType.Normal => StaggerSettings.NormalMonsterDefault, 
			MonsterType.MiniBoss => StaggerSettings.MiniBossMonsterDefault, 
			MonsterType.Boss => StaggerSettings.BossMonsterDefault, 
			_ => new StaggerSettings
			{
				enabled = false
			}, 
		};
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
	}

	public override void OnSaveActor(Dictionary<string, object> data)
	{
		base.OnSaveActor(data);
		data["camp"] = campPosition;
	}

	public override void OnLoadActor(Dictionary<string, object> data)
	{
		base.OnLoadActor(data);
		campPosition = (Vector3?)data["camp"];
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_Monster_002FMonsterType(writer, type);
			writer.WriteFloat(populationCost);
			writer.WriteBool(isHunter);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			GeneratedNetworkCode._Write_Monster_002FMonsterType(writer, type);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteFloat(populationCost);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteBool(isHunter);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref type, null, GeneratedNetworkCode._Read_Monster_002FMonsterType(reader));
			GeneratedSyncVarDeserialize(ref populationCost, OnPopulationChanged, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref isHunter, null, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref type, null, GeneratedNetworkCode._Read_Monster_002FMonsterType(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref populationCost, OnPopulationChanged, reader.ReadFloat());
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isHunter, null, reader.ReadBool());
		}
	}
}
