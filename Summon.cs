using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Summon : Entity
{
	private enum SummonCommandState
	{
		None,
		Move,
		Attack
	}

	private const float HealthScaling_RequiredUpgradesPerZone = 2.5f;

	public float maxDuration = 10f;

	public bool enableOutOfCombatHealthRegen;

	[SyncVar]
	private float _addedDuration;

	[SyncVar]
	private CastInfo _info;

	[SyncVar]
	internal int _skillLevel = -1;

	protected AbilityTargetValidatorWrapper tvDefaultHarmfulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultUsefulEffectTargets;

	private StatBonus _statSyncBonus;

	private bool _doOrphanCheck;

	private AbilityTrigger _orphanCheckParentTrigger;

	private float _lastStuckCheckTime;

	private float _lastOutOfCombatHealthRegenTime;

	private List<Summon> _parentsSummons;

	private SummonCommandState _scState;

	private Entity _scTarget;

	private Vector3 _scDestination;

	private float _scExpireTime;

	public override bool isDestroyedOnRoomChange => hero == null;

	public float normalizedRemainingDuration => Mathf.Clamp01(1f - (Time.time - base.creationTime - _addedDuration) / maxDuration);

	public float summonHealth
	{
		get
		{
			EntityStatus es = base.Status;
			if (es == null)
			{
				es = GetComponent<EntityStatus>();
			}
			if (NetworkedManagerBase<GameManager>.softInstance == null)
			{
				return es.baseStats.maxHealth;
			}
			return es.baseStats.maxHealth * NetworkedManagerBase<GameManager>.instance.GetRegularMonsterDamageMultiplierByScaling(NetworkedManagerBase<GameManager>.instance.difficulty.scalingFactor, (float)(skillLevel - 1) / 2.5f);
		}
	}

	public CastInfo info
	{
		get
		{
			return _info;
		}
		set
		{
			Network_info = value;
		}
	}

	public int skillLevel
	{
		get
		{
			return ScalingValue.levelOverride ?? _skillLevel;
		}
		set
		{
			Network_skillLevel = value;
		}
	}

	public Hero hero { get; private set; }

	public int effectiveLevel => skillLevel;

	public Entity statEntity => this;

	public float Network_addedDuration
	{
		get
		{
			return _addedDuration;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _addedDuration, 16uL, null);
		}
	}

	public CastInfo Network_info
	{
		get
		{
			return _info;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _info, 32uL, null);
		}
	}

	public int Network_skillLevel
	{
		get
		{
			return _skillLevel;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _skillLevel, 64uL, null);
		}
	}

	protected override void OnPrepare()
	{
		tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Neutral | EntityRelation.Enemy);
		tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Self | EntityRelation.Ally);
		base.OnPrepare();
		if (skillLevel == -1)
		{
			if (base.parentActor is AbilityInstance { skillLevel: not -1 } ai)
			{
				Network_skillLevel = ai.skillLevel;
			}
			else if (base.parentActor is SkillTrigger { level: not -1 } st)
			{
				Network_skillLevel = st.level;
			}
			else if (base.parentActor is Gem { effectiveLevel: not -1 } gm)
			{
				Network_skillLevel = gm.effectiveLevel;
			}
		}
		info.caster.Status.ClientEvent_OnStatCalculated += new Action(SyncStats);
		_statSyncBonus = base.Status.AddStatBonus(new StatBonus());
		SyncStats();
	}

	[Server]
	private void SyncStats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Summon::SyncStats()' called when server was not active");
			return;
		}
		_statSyncBonus.attackDamageFlat = info.caster.Status.attackDamage;
		_statSyncBonus._abilityPowerFlat = info.caster.Status.abilityPower;
		_statSyncBonus.maxHealthFlat = summonHealth - base.Status.baseStats.maxHealth;
	}

	protected override void OnCreate()
	{
		hero = FindFirstOfType<Hero>();
		base.OnCreate();
		OnCreate_Command();
		if (base.isServer)
		{
			_orphanCheckParentTrigger = base.firstTrigger;
			_doOrphanCheck = _orphanCheckParentTrigger != null;
			SyncStats();
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> list = DewPhysics.OverlapCircleAllEntities(out handle, base.position, 5f, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (list.Length > 0)
			{
				base.AI.Aggro(list[0]);
			}
			handle.Return();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_parentsSummons != null)
		{
			_parentsSummons.Remove(this);
		}
		if (base.isServer && info.caster != null)
		{
			info.caster.Status.ClientEvent_OnStatCalculated -= new Action(SyncStats);
		}
	}

	public override void OnStart()
	{
		Actor cursor = base.parentActor;
		while (cursor != null)
		{
			if (cursor is Hero h)
			{
				_parentsSummons = h.summons;
				_parentsSummons.Add(this);
				break;
			}
			cursor = cursor.parentActor;
		}
		if (!base.isServer)
		{
			tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Neutral | EntityRelation.Enemy);
			tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Self | EntityRelation.Ally);
		}
		base.OnStart();
	}

	protected override StaggerSettings GetStaggerSettings()
	{
		return StaggerSettings.SummonDefault;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer)
		{
			return;
		}
		if (_doOrphanCheck && (_orphanCheckParentTrigger.IsNullOrInactive() || _orphanCheckParentTrigger.owner.IsNullOrInactive()))
		{
			Kill();
			return;
		}
		if (normalizedRemainingDuration <= 0.0001f && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			Kill();
			return;
		}
		if ((object)hero != null && hero.IsNullInactiveDeadOrKnockedOut())
		{
			Kill();
			return;
		}
		if (hero != null && base.Control._agent.enabled && !base.Control.isDisplacing && Time.time - _lastStuckCheckTime > 1f)
		{
			_lastStuckCheckTime = Time.time;
			if (Dew.GetNavMeshPathStatus(hero.agentPosition, base.agentPosition) != NavMeshPathStatus.PathComplete)
			{
				Vector3 curr = base.agentPosition;
				Vector3 dest = Dew.GetValidAgentDestination_Closest(hero.agentPosition, curr);
				Dew.FilterNonOkayValues(ref curr);
				base.Control.StartDisplacement(new DispByDestination
				{
					destination = dest,
					duration = 0.2f,
					ease = DewEase.EaseOutQuad,
					canGoOverTerrain = true,
					isFriendly = true,
					rotateForward = false,
					rotateSmoothly = false,
					affectedByMovementSpeed = false,
					isCanceledByCC = false
				});
			}
		}
		if (enableOutOfCombatHealthRegen && hero != null && !hero.isInCombat && Time.time - _lastOutOfCombatHealthRegenTime > 0.15f && base.normalizedHealth < 0.9999f)
		{
			_lastOutOfCombatHealthRegenTime = Time.time;
			NetworkedManagerBase<ActorManager>.instance.serverActor.Heal(base.maxHealth * 0.1f).SetCanMerge().Dispatch(this);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (context.targetEnemy == null)
		{
			if (Vector2.Distance(info.caster.agentPosition.ToXY(), base.agentPosition.ToXY()) > 5f || info.caster.Control.isWalking)
			{
				FollowCaster();
			}
			else if (base.Control._desiredAgentDestination.HasValue && Vector2.Distance(base.Control._desiredAgentDestination.Value.ToXY(), base.agentPosition.ToXY()) < 2f)
			{
				base.Control.ClearMovement();
			}
		}
		else
		{
			base.AI.Helper_ChaseTarget();
		}
	}

	private void FollowCaster()
	{
		Vector3 dest = AbilityTrigger.PredictPoint_Simple(1f, info.caster, 1f);
		base.Control.MoveToDestination(dest, immediately: false, Mathf.Lerp(0.3f, 1f, Vector2.Distance(dest.ToXY(), base.agentPosition.ToXY()) / 8f));
	}

	[Server]
	public void AddDuration(float duration, bool clampToMaxDuration = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Summon::AddDuration(System.Single,System.Boolean)' called when server was not active");
		}
		else if (!float.IsPositiveInfinity(maxDuration))
		{
			if (clampToMaxDuration)
			{
				float elapsedTime = Time.time - base.creationTime - _addedDuration;
				float maxAdd = Mathf.Max(0f, maxDuration - elapsedTime);
				duration = Mathf.Min(duration, maxAdd);
			}
			Network_addedDuration = _addedDuration + duration;
		}
	}

	public DamageData DefaultDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Default, value, procCoefficient);
	}

	public DamageData PhysicalDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical, value, procCoefficient);
	}

	public DamageData MagicDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Magic, value, procCoefficient);
	}

	public DamageData PhysicalMagicDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical | DamageData.SourceType.Magic, value, procCoefficient);
	}

	public DamageData PureDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Pure, value, procCoefficient);
	}

	public DamageData Damage(ScalingValue value, float procCoefficient = 1f)
	{
		DamageData.SourceType type = DamageData.SourceType.Default;
		if (value.adFactor > 0f)
		{
			type |= DamageData.SourceType.Physical;
		}
		else if (value.apFactor > 0f)
		{
			type |= DamageData.SourceType.Magic;
		}
		return CreateDamage(type, value, procCoefficient);
	}

	public DamageData CreateDamage(DamageData.SourceType type, ScalingValue value, float procCoefficient = 1f)
	{
		return new DamageData(type, value, statEntity, effectiveLevel, procCoefficient).SetActor(this);
	}

	public HealData Heal(ScalingValue amount)
	{
		return new HealData(GetValue(amount)).SetActor(this);
	}

	public float GetValue(ScalingValue val)
	{
		return val.GetValue(effectiveLevel, statEntity);
	}

	private void OnCreate_Command()
	{
		if (!base.isServer)
		{
			return;
		}
		base.AI.customBehaviors.Add(delegate(ref EntityAIContext context)
		{
			if (_scState != SummonCommandState.None && Time.time > _scExpireTime)
			{
				if (_scState == SummonCommandState.Move)
				{
					base.Control.ClearMovement();
				}
				_scState = SummonCommandState.None;
			}
			if (_scState == SummonCommandState.Attack && _scTarget.IsNullInactiveDeadOrKnockedOut())
			{
				_scState = SummonCommandState.None;
			}
			if (_scState == SummonCommandState.Move)
			{
				if (Vector3.Distance(base.agentPosition, _scDestination) < 2f)
				{
					_scState = SummonCommandState.None;
					base.Control.ClearMovement();
				}
				base.Control.MoveToDestination(_scDestination, immediately: false);
				return true;
			}
			if (_scState == SummonCommandState.Attack)
			{
				if (context.targetEnemy != _scTarget)
				{
					base.AI.Aggro(_scTarget);
				}
				return false;
			}
			return false;
		}, 100);
	}

	[Server]
	public void SummonCommand_MoveToDestination(Vector3 destination)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Summon::SummonCommand_MoveToDestination(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		destination = Dew.GetValidAgentDestination_Closest(base.agentPosition, destination);
		_scState = SummonCommandState.Move;
		_scExpireTime = Time.time + 2f + Vector3.Distance(base.agentPosition, destination) / base.Control.baseAgentSpeed * 1.25f;
		_scDestination = destination;
		base.AI.CallAIUpdateImmediately();
	}

	[Server]
	public void SummonCommand_AttackTarget(Entity target)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Summon::SummonCommand_AttackTarget(Entity)' called when server was not active");
		}
		else if (!target.IsNullInactiveDeadOrKnockedOut())
		{
			_scState = SummonCommandState.Attack;
			_scExpireTime = Time.time + 4f + Vector3.Distance(base.agentPosition, target.agentPosition) / base.Control.baseAgentSpeed * 1.25f;
			_scTarget = target;
			base.AI.CallAIUpdateImmediately();
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(_addedDuration);
			GeneratedNetworkCode._Write_CastInfo(writer, _info);
			writer.WriteInt(_skillLevel);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteFloat(_addedDuration);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			GeneratedNetworkCode._Write_CastInfo(writer, _info);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteInt(_skillLevel);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _addedDuration, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _info, null, GeneratedNetworkCode._Read_CastInfo(reader));
			GeneratedSyncVarDeserialize(ref _skillLevel, null, reader.ReadInt());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _addedDuration, null, reader.ReadFloat());
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _info, null, GeneratedNetworkCode._Read_CastInfo(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _skillLevel, null, reader.ReadInt());
		}
	}
}
