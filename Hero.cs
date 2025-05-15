using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[RequireComponent(typeof(HeroSkill))]
public class Hero : Entity, IExcludeFromPool
{
	public enum HeroMainStatType
	{
		Strength,
		Intelligence,
		Agility
	}

	public enum HeroClassType
	{
		RangedAttacker,
		MeleeAttacker,
		RangedMage,
		MeleeMage,
		RangedTank,
		MeleeTank,
		RangedSupport,
		MeleeSupport,
		RangedSummoner,
		MeleeSummoner
	}

	public enum HeroDifficulty
	{
		VeryEasy,
		Easy,
		Medium,
		Hard,
		VeryHard
	}

	public const float MarkAsInCombatDuration = 2f;

	public const float InCombatCheckInterval = 0.5f;

	public SafeAction<EventInfoKill> ClientHeroEvent_OnKillOrAssist;

	public SafeAction<EventInfoSkillUse> ClientHeroEvent_OnSkillUse;

	public SafeAction<EventInfoKill> ClientHeroEvent_OnKnockedOut;

	public SafeAction<Hero> ClientHeroEvent_OnRevive;

	public SafeAction<EventInfoSkillAbilityInstance> HeroEvent_OnAbilityInstanceCreatedFromSkill;

	public SafeAction<EventInfoSkillAbilityInstance> HeroEvent_OnAbilityInstanceBeforePrepareFromSkill;

	public SafeAction<EventInfoHeroLevelUp> ClientHeroEvent_OnLevelChanged;

	private HeroSkill _Skill;

	[CompilerGenerated]
	[SyncVar]
	private HeroLoadoutData _003Cloadout_003Ek__BackingField;

	public readonly SyncList<string> accessories = new SyncList<string>();

	private KillTracker _assistTracker;

	[SyncVar]
	private int _exp;

	[SyncVar]
	private bool _isKnockedOut;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsInCombatChanged")]
	private bool _003CisInCombat_003Ek__BackingField;

	public SafeAction<bool> ClientHeroEvent_OnIsInCombatChanged;

	private float _lastIsInCombatMarkTime;

	private float _nextCombatCheckTime;

	public Sprite icon;

	public Color mainColor;

	public HeroClassType classType;

	public HeroDifficulty difficulty;

	public Transform weapon;

	public GameObject decoConstellationPrefab;

	public bool excludeFromPool;

	public float cDisplayBaseAngle;

	public HeroConstellationSettings cDestruction;

	public HeroConstellationSettings cLife;

	public HeroConstellationSettings cImagination;

	public HeroConstellationSettings cFlexible;

	[NonSerialized]
	public List<Summon> summons = new List<Summon>();

	private Vector3 _weaponOriginalScale;

	private Vector3 _weaponCv;

	private GameObject _levelUpEffect;

	public override bool isDestroyedOnRoomChange
	{
		get
		{
			if (!(base.owner == null))
			{
				return !DewPlayer.humanPlayers.Contains(base.owner);
			}
			return true;
		}
	}

	public HeroSkill Skill => _Skill;

	public HeroLoadoutData loadout
	{
		[CompilerGenerated]
		get
		{
			return _003Cloadout_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cloadout_003Ek__BackingField = value;
		}
	}

	public int exp
	{
		get
		{
			return _exp;
		}
		internal set
		{
			if (!base.isServer)
			{
				throw new InvalidOperationException("Can't set Hero.exp on clients");
			}
			if (base.level >= maxLevel)
			{
				Network_exp = 0;
				return;
			}
			Network_exp = value;
			while (_exp >= maxExp && base.level < maxLevel)
			{
				Network_exp = _exp - maxExp;
				base.Status.level++;
			}
		}
	}

	public int maxExp
	{
		get
		{
			if (base.level < NetworkedManagerBase<GameManager>.instance.gss.maxHeroLevel)
			{
				return (int)((float)(50 + 10 * base.level) * Mathf.Pow(1.2f, base.level));
			}
			return 0;
		}
	}

	public int maxLevel => NetworkedManagerBase<GameManager>.instance.gss.maxHeroLevel;

	public bool isKnockedOut
	{
		get
		{
			return _isKnockedOut;
		}
		internal set
		{
			Network_isKnockedOut = value;
		}
	}

	public bool isInCombat
	{
		[CompilerGenerated]
		get
		{
			return _003CisInCombat_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisInCombat_003Ek__BackingField = value;
		}
	}

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public HeroLoadoutData Network_003Cloadout_003Ek__BackingField
	{
		get
		{
			return loadout;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref loadout, 16uL, null);
		}
	}

	public int Network_exp
	{
		get
		{
			return _exp;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _exp, 32uL, null);
		}
	}

	public bool Network_isKnockedOut
	{
		get
		{
			return _isKnockedOut;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _isKnockedOut, 64uL, null);
		}
	}

	public bool Network_003CisInCombat_003Ek__BackingField
	{
		get
		{
			return isInCombat;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isInCombat, 128uL, OnIsInCombatChanged);
		}
	}

	private void OnIsInCombatChanged(bool oldVal, bool newVal)
	{
		try
		{
			ClientHeroEvent_OnIsInCombatChanged?.Invoke(newVal);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public HeroConstellationSettings GetConstellationSettings(StarType type)
	{
		return type switch
		{
			StarType.Life => cLife, 
			StarType.Destruction => cDestruction, 
			StarType.Imagination => cImagination, 
			StarType.Flexible => cFlexible, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	protected override void Awake()
	{
		base.Awake();
		AssignComponents();
		_levelUpEffect = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Effects/HeroLevelUp"), base.transform);
		_levelUpEffect.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		if (weapon != null)
		{
			_weaponOriginalScale = weapon.localScale;
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!(context.targetEnemy == null) && !TrySkill(Skill.Q) && !TrySkill(Skill.W) && !TrySkill(Skill.E) && !TrySkill(Skill.R))
		{
			base.AI.Helper_ChaseTarget();
		}
		bool TrySkill(SkillTrigger skill)
		{
			if (skill == null)
			{
				return false;
			}
			if (UnityEngine.Random.value < 0.5f)
			{
				return false;
			}
			if (!skill.CanBeCast())
			{
				return false;
			}
			if (!skill.currentConfig.ignoreBlock && base.Control.IsActionBlocked(EntityControl.BlockableAction.Ability) != EntityControl.BlockStatus.Allowed)
			{
				return false;
			}
			return base.AI.Helper_CastAbilityAuto(skill);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!(weapon == null))
		{
			bool shouldHideWeapon = base.Animation.abilityAnimStatus.isPlaying && base.Animation.abilityAnimStatus.currentClip.hideWeaponOnHeroes;
			weapon.localScale = Vector3.SmoothDamp(weapon.localScale, shouldHideWeapon ? Vector3.zero : _weaponOriginalScale, ref _weaponCv, 0.05f);
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		if (loadout == null)
		{
			Network_003Cloadout_003Ek__BackingField = new HeroLoadoutData();
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		ClientHeroEvent_OnLevelChanged += new Action<EventInfoHeroLevelUp>(OnHeroLevelUp);
		_assistTracker = TrackKills(30f, delegate(EventInfoKill obj)
		{
			RpcInvokeOnKillOrAssist(obj);
		});
		EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage dmg)
		{
			Entity entity = dmg.actor.firstEntity;
			if (!(entity == null) && entity.GetRelation(this) == EntityRelation.Enemy)
			{
				MarkAsInCombat();
			}
		};
		ActorEvent_OnDealDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage dmg)
		{
			if (GetRelation(dmg.victim) == EntityRelation.Enemy)
			{
				MarkAsInCombat();
			}
		};
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		ApplyHeroStatusEffects();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && Time.time > _nextCombatCheckTime)
		{
			_nextCombatCheckTime = Time.time + 0.5f;
			bool newCombat = Time.time - _lastIsInCombatMarkTime < 2f || (base.section != null && base.section.monsters.isCombatActive);
			if (newCombat != isInCombat)
			{
				Network_003CisInCombat_003Ek__BackingField = newCombat;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && _assistTracker != null)
		{
			_assistTracker.Stop();
			_assistTracker = null;
		}
	}

	[ClientRpc]
	private void RpcInvokeOnKillOrAssist(EventInfoKill obj)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoKill(writer, obj);
		SendRPCInternal("System.Void Hero::RpcInvokeOnKillOrAssist(EventInfoKill)", 162120419, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority();
		NetworkedManagerBase<ActorManager>.instance.onLocalHeroAdd?.Invoke(this);
	}

	public override void OnStopAuthority()
	{
		base.OnStopAuthority();
		if (NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.onLocalHeroRemove?.Invoke(this);
		}
	}

	private void ApplyHeroStatusEffects()
	{
		CreateStatusEffect<Se_HeroDeathInterrupt>(this, new CastInfo(this));
		CreateStatusEffect<Se_HeroInCombatIcon>(this, new CastInfo(this));
	}

	protected override StaggerSettings GetStaggerSettings()
	{
		return StaggerSettings.HeroDefault;
	}

	private bool AssignComponents()
	{
		_Skill = GetComponent<HeroSkill>();
		_Skill.entity = this;
		return true;
	}

	[Server]
	public void ReceiveExperience(int amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Hero::ReceiveExperience(System.Int32)' called when server was not active");
		}
		else
		{
			exp += amount;
		}
	}

	private void OnHeroLevelUp(EventInfoHeroLevelUp obj)
	{
		FxPlayNewNetworked(_levelUpEffect, this);
		float healRatio = Mathf.Clamp(0.2f - (float)obj.oldLevel * 0.0075f, 0.05f, 1f);
		Heal(base.Status.missingHealth * healRatio).Dispatch(this);
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			color = new Color(1f, 0.65f, 0.96f),
			rawText = DewLocalization.GetUIValue("InGame_Message_LevelUpPopUp"),
			worldPosGetter = () => this.IsNullInactiveDeadOrKnockedOut() ? Vector3.zero : base.Visual.GetCenterPosition()
		});
	}

	[Server]
	public void MarkAsInCombat()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Hero::MarkAsInCombat()' called when server was not active");
			return;
		}
		_lastIsInCombatMarkTime = Time.time;
		if (!isInCombat)
		{
			Network_003CisInCombat_003Ek__BackingField = true;
		}
	}

	public bool IsMeleeHero()
	{
		return Dew.IsMeleeHero(classType);
	}

	public bool IsRangedHero()
	{
		return Dew.IsRangedHero(classType);
	}

	[Command]
	public void CmdTeleportToWaypoint(Room_Waypoint waypoint)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(waypoint);
		SendCommandInternal("System.Void Hero::CmdTeleportToWaypoint(Room_Waypoint)", 1444585225, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public Hero()
	{
		InitSyncObject(accessories);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeOnKillOrAssist__EventInfoKill(EventInfoKill obj)
	{
		try
		{
			ClientHeroEvent_OnKillOrAssist?.Invoke(obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnKillOrAssist__EventInfoKill(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnKillOrAssist called on server.");
		}
		else
		{
			((Hero)obj).UserCode_RpcInvokeOnKillOrAssist__EventInfoKill(GeneratedNetworkCode._Read_EventInfoKill(reader));
		}
	}

	protected void UserCode_CmdTeleportToWaypoint__Room_Waypoint(Room_Waypoint waypoint)
	{
		if (!(waypoint == null) && waypoint.isUnlocked)
		{
			if (isInCombat)
			{
				base.owner.TpcShowCenterMessage(CenterMessageType.Error, "InGame_Message_TeleportUnavailableInCombat");
			}
			else if (!base.Status.HasStatusEffect<Se_WaypointTeleport>())
			{
				CreateStatusEffect<Se_WaypointTeleport>(this, new CastInfo(this, waypoint.transform.position));
			}
		}
	}

	protected static void InvokeUserCode_CmdTeleportToWaypoint__Room_Waypoint(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTeleportToWaypoint called on client.");
		}
		else
		{
			((Hero)obj).UserCode_CmdTeleportToWaypoint__Room_Waypoint(reader.ReadNetworkBehaviour<Room_Waypoint>());
		}
	}

	static Hero()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Hero), "System.Void Hero::CmdTeleportToWaypoint(Room_Waypoint)", InvokeUserCode_CmdTeleportToWaypoint__Room_Waypoint, requiresAuthority: true);
		RemoteProcedureCalls.RegisterRpc(typeof(Hero), "System.Void Hero::RpcInvokeOnKillOrAssist(EventInfoKill)", InvokeUserCode_RpcInvokeOnKillOrAssist__EventInfoKill);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_HeroLoadoutData(writer, loadout);
			writer.WriteInt(_exp);
			writer.WriteBool(_isKnockedOut);
			writer.WriteBool(isInCombat);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			GeneratedNetworkCode._Write_HeroLoadoutData(writer, loadout);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteInt(_exp);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteBool(_isKnockedOut);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteBool(isInCombat);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref loadout, null, GeneratedNetworkCode._Read_HeroLoadoutData(reader));
			GeneratedSyncVarDeserialize(ref _exp, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref _isKnockedOut, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref isInCombat, OnIsInCombatChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref loadout, null, GeneratedNetworkCode._Read_HeroLoadoutData(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _exp, null, reader.ReadInt());
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _isKnockedOut, null, reader.ReadBool());
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isInCombat, OnIsInCombatChanged, reader.ReadBool());
		}
	}
}
