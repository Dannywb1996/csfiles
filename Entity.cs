using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[LogicUpdatePriority(-400)]
[RequireComponent(typeof(EntityAI))]
[RequireComponent(typeof(EntityAnimation))]
[RequireComponent(typeof(EntityControl))]
[RequireComponent(typeof(EntityStatus))]
[RequireComponent(typeof(EntityVisual))]
[RequireComponent(typeof(EntityAbility))]
[RequireComponent(typeof(EntitySound))]
public class Entity : Actor
{
	public struct StaggerSettings
	{
		public static readonly StaggerSettings HeroDefault = new StaggerSettings
		{
			enabled = false
		};

		public static readonly StaggerSettings LesserMonsterDefault = new StaggerSettings
		{
			enabled = true,
			canStaggerWhileChanneling = true,
			highDamageStaggerThreshold = 0.3f,
			accumulatedDamageStaggerThreshold = 0.5f,
			knockbackDistance = 0.45f,
			knockbackDuration = 0.25f,
			knockbackEase = DewEase.EaseOutQuart,
			stunDuration = 0.5f,
			chance = 0.75f,
			chanceInChannel = 0.8f,
			staggerImmunityTime = 0f
		};

		public static readonly StaggerSettings NormalMonsterDefault = new StaggerSettings
		{
			enabled = true,
			canStaggerWhileChanneling = true,
			highDamageStaggerThreshold = 0.4f,
			accumulatedDamageStaggerThreshold = 0.7f,
			knockbackDistance = 0.45f,
			knockbackDuration = 0.25f,
			knockbackEase = DewEase.EaseOutQuart,
			stunDuration = 0.3f,
			chance = 0.75f,
			chanceInChannel = 0.5f,
			staggerImmunityTime = 0f
		};

		public static readonly StaggerSettings MiniBossMonsterDefault = new StaggerSettings
		{
			enabled = true,
			canStaggerWhileChanneling = false,
			highDamageStaggerThreshold = 0.4f,
			accumulatedDamageStaggerThreshold = 0.7f,
			knockbackDistance = 0.45f,
			knockbackDuration = 0.25f,
			knockbackEase = DewEase.EaseOutQuart,
			stunDuration = 0.3f,
			chance = 0.75f,
			chanceInChannel = 0f,
			staggerImmunityTime = 3f
		};

		public static readonly StaggerSettings BossMonsterDefault = new StaggerSettings
		{
			enabled = false
		};

		public static readonly StaggerSettings SummonDefault = new StaggerSettings
		{
			enabled = true,
			canStaggerWhileChanneling = true,
			highDamageStaggerThreshold = 0.3f,
			accumulatedDamageStaggerThreshold = 0.5f,
			knockbackDistance = 0.45f,
			knockbackDuration = 0.25f,
			knockbackEase = DewEase.EaseOutQuart,
			stunDuration = 0.3f,
			chance = 0.75f,
			chanceInChannel = 0.5f,
			staggerImmunityTime = 0f
		};

		public bool enabled;

		public bool canStaggerWhileChanneling;

		public float highDamageStaggerThreshold;

		public float accumulatedDamageStaggerThreshold;

		public float knockbackDistance;

		public float knockbackDuration;

		public DewEase knockbackEase;

		public float stunDuration;

		public float chance;

		public float chanceInChannel;

		public float staggerImmunityTime;
	}

	private EntityAI _AI;

	private EntityAnimation _Animation;

	private EntityAbility _Ability;

	private EntityControl _Control;

	private EntityStatus _Status;

	private EntityVisual _Visual;

	private EntitySound _Sound;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsSleepingChanged")]
	private bool _003CisSleeping_003Ek__BackingField;

	public SafeAction<bool> ClientEntityEvent_OnIsSleepingChanged;

	internal float _accumulatedSleepTime;

	public SafeAction<EventInfoHeal> EntityEvent_OnTakeManaHeal;

	public SafeAction<EventInfoHeal> EntityEvent_OnTakeHeal;

	public SafeAction<EventInfoDamage> EntityEvent_OnTakeDamage;

	public SafeAction<EventInfoKill> EntityEvent_OnDeath;

	public SafeAction<EventInfoSpentMana> EntityEvent_OnGetManaSpent;

	public SafeAction<EventInfoAttackFired> EntityEvent_OnAttackFired;

	public SafeAction<EventInfoAttackFired> EntityEvent_OnAttackFiredBeforePrepare;

	public SafeAction<EventInfoAttackHit> EntityEvent_OnAttackHit;

	public SafeAction<EventInfoAttackHit> EntityEvent_OnAttackTaken;

	public SafeAction<EventInfoAttackMissed> EntityEvent_OnAttackMissed;

	public SafeAction<EventInfoAttackMissed> EntityEvent_OnAttackDodged;

	public SafeAction<EventInfoDamageNegatedByImmunity> EntityEvent_OnDamageNegated;

	public SafeAction<EventInfoStatusEffect> ClientEntityEvent_OnStatusEffectAdded;

	public SafeAction<EventInfoStatusEffect> ClientEntityEvent_OnStatusEffectRemoved;

	public SafeAction<EventInfoAttackEffect> EntityEvent_OnAttackEffectTriggered;

	public SafeAction<EventInfoCast> EntityEvent_OnCastComplete;

	public SafeAction<EventInfoCast> EntityEvent_OnCastCompleteBeforePrepare;

	public DataProcessors<DamageData> takenDamageProcessor = new DataProcessors<DamageData>();

	public DataProcessors<HealData> takenHealProcessor = new DataProcessors<HealData>();

	public DataProcessors<HealData> takenManaHealProcessor = new DataProcessors<HealData>();

	public DataProcessors<HealData> takenShieldProcessor = new DataProcessors<HealData>();

	[SyncVar]
	private DewPlayer _owner;

	private float _lastAttackerTime;

	internal StaggerSettings _staggerSettings;

	private float _accDamage;

	private float _lastStaggerTime;

	internal int _wasStuckCounter;

	internal int _stuckCheckVersion;

	protected NetworkBehaviourSyncVar ____ownerNetId;

	public EntityAI AI => _AI;

	public EntityAnimation Animation => _Animation;

	public EntityAbility Ability => _Ability;

	public EntityControl Control => _Control;

	public EntityStatus Status => _Status;

	public EntityVisual Visual => _Visual;

	public EntitySound Sound => _Sound;

	public bool isSleeping
	{
		[CompilerGenerated]
		get
		{
			return _003CisSleeping_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CisSleeping_003Ek__BackingField = value;
		}
	}

	public DewPlayer owner
	{
		get
		{
			if (!(Network_owner == null))
			{
				return Network_owner;
			}
			return defaultOwner;
		}
		set
		{
			Network_owner = value;
			if (!(base.netIdentity == null) && base.netIdentity.isServer)
			{
				base.netIdentity.RemoveClientAuthority();
				if (value != null && value.isHumanPlayer)
				{
					base.netIdentity.AssignClientAuthority(value.connectionToClient);
				}
			}
		}
	}

	protected virtual DewPlayer defaultOwner => DewPlayer.environment;

	internal Actor _lastAttacker { get; private set; }

	public float currentHealth => Status.currentHealth;

	public float currentMana => Status.currentMana;

	public float maxHealth => Status.maxHealth;

	public float maxMana => Status.maxMana;

	public float normalizedHealth => Status.normalizedHealth;

	public float normalizedMana => Status.normalizedMana;

	public bool isAlive => Status.isAlive;

	public bool isDead => Status.isDead;

	public int level => Status.level;

	public Vector3 agentPosition => Control.agentPosition;

	public RoomSection section { get; internal set; }

	public RoomSection lastSection { get; internal set; }

	public bool Network_003CisSleeping_003Ek__BackingField
	{
		get
		{
			return isSleeping;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isSleeping, 4uL, OnIsSleepingChanged);
		}
	}

	public DewPlayer Network_owner
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____ownerNetId, ref _owner);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _owner, 8uL, null, ref ____ownerNetId);
		}
	}

	private void OnIsSleepingChanged(bool oldVal, bool newVal)
	{
		try
		{
			ClientEntityEvent_OnIsSleepingChanged?.Invoke(newVal);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			if (newVal)
			{
				NetworkedManagerBase<ActorManager>.instance.onAwakeEntityRemove?.Invoke(this);
			}
			else
			{
				NetworkedManagerBase<ActorManager>.instance.onAwakeEntityAdd?.Invoke(this);
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		AssignComponents();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (owner == null)
		{
			owner = DewPlayer.creep;
		}
		_staggerSettings = GetStaggerSettings();
		EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(HandleDamageForStagger);
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(HandleDamageForStagger);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!isSleeping)
		{
			try
			{
				NetworkedManagerBase<ActorManager>.instance.onAwakeEntityRemove?.Invoke(this);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (base.isServer && _lastAttacker != null)
		{
			_lastAttacker.UnlockDestroy();
			_lastAttacker = null;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && _lastAttacker != null && Time.time - _lastAttackerTime > 30f)
		{
			_lastAttacker.UnlockDestroy();
			_lastAttacker = null;
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if (Application.IsPlaying(this))
		{
			if (!base.isActive)
			{
				Gizmos.color = Color.gray;
				Gizmos.DrawCube(base.position, Vector3.one * 0.5f);
				return;
			}
			Gizmos.color = Color.yellow;
			Gizmos.DrawCube(base.position, Vector3.one * 0.5f);
			Gizmos.color = Color.cyan;
			Gizmos.DrawCube(agentPosition, Vector3.one * 0.35f);
		}
	}

	[Server]
	internal void SetLastAttacker(Actor actor)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::SetLastAttacker(Actor)' called when server was not active");
		}
		else
		{
			if (actor == null || !actor.isServer || !base.isActive)
			{
				return;
			}
			_lastAttackerTime = Time.time;
			if (!(_lastAttacker == actor))
			{
				if (_lastAttacker != null)
				{
					_lastAttacker.UnlockDestroy();
				}
				_lastAttacker = actor;
				actor.LockDestroy();
			}
		}
	}

	private void HandleDamageForStagger(EventInfoDamage info)
	{
		if (Control.isAirborne || !_staggerSettings.enabled || (!_staggerSettings.canStaggerWhileChanneling && Control.ongoingChannels.Count > 0))
		{
			return;
		}
		float chance = ((Control.ongoingChannels.Count > 0) ? _staggerSettings.chanceInChannel : _staggerSettings.chance);
		if (info.damage.amount > maxHealth * _staggerSettings.highDamageStaggerThreshold)
		{
			if (!(Time.time - _lastStaggerTime < _staggerSettings.staggerImmunityTime) && !(UnityEngine.Random.value > chance))
			{
				Stagger(info.damage.direction);
			}
			return;
		}
		_accDamage += info.damage.amount / maxHealth;
		if (_accDamage > _staggerSettings.accumulatedDamageStaggerThreshold && !(Time.time - _lastStaggerTime < _staggerSettings.staggerImmunityTime) && !(UnityEngine.Random.value > chance))
		{
			Stagger(info.damage.direction);
		}
	}

	[Server]
	public void Stagger(Vector3? dir)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::Stagger(System.Nullable`1<UnityEngine.Vector3>)' called when server was not active");
		}
		else if (!Status.hasCrowdControlImmunity)
		{
			_accDamage = 0f;
			_lastStaggerTime = Time.time;
			CreateBasicEffect(this, new StunEffect(), _staggerSettings.stunDuration, "Stagger");
			Animation.PlayStaggerAnimation();
			if (!dir.HasValue)
			{
				Control.Rotate(Quaternion.Euler(0f, UnityEngine.Random.Range(-30, 30), 0f) * base.transform.forward, immediately: true);
				return;
			}
			dir = dir.Value.Flattened().normalized;
			dir = Quaternion.Euler(0f, UnityEngine.Random.Range(-30, 30), 0f) * dir.Value;
			Control.Rotate(-dir.Value, immediately: true);
			Control.StartDisplacement(new DispByDestination
			{
				destination = base.position + dir.Value * _staggerSettings.knockbackDistance,
				duration = _staggerSettings.knockbackDuration,
				ease = _staggerSettings.knockbackEase,
				isFriendly = false,
				onCancel = null,
				onFinish = null,
				rotateForward = false,
				canGoOverTerrain = false,
				isCanceledByCC = false
			});
		}
	}

	protected virtual StaggerSettings GetStaggerSettings()
	{
		return new StaggerSettings
		{
			enabled = false
		};
	}

	protected virtual void AIUpdate(ref EntityAIContext context)
	{
	}

	internal void CallAIUpdate(ref EntityAIContext context)
	{
		if (AI.customBehaviors.Execute(ref context))
		{
			return;
		}
		if (owner == DewPlayer.local && context.targetEnemy != null)
		{
			Debug.DrawLine(base.position, context.targetEnemy.position, Color.red, 0.1f);
		}
		try
		{
			AIUpdate(ref context);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	public void ProcessReceivedDamage(ref DamageData data, Actor actor)
	{
		takenDamageProcessor.Process(ref data, actor, this);
	}

	public void ProcessReceivedHeal(ref HealData data, Actor actor)
	{
		takenHealProcessor.Process(ref data, actor, this);
	}

	public void ProcessReceivedManaHeal(ref HealData data, Actor actor)
	{
		takenManaHealProcessor.Process(ref data, actor, this);
	}

	public void ProcessReceivedShield(ref HealData data, Actor actor)
	{
		takenShieldProcessor.Process(ref data, actor, this);
	}

	private bool AssignComponents()
	{
		_AI = GetComponent<EntityAI>();
		_Ability = GetComponent<EntityAbility>();
		_Control = GetComponent<EntityControl>();
		_Status = GetComponent<EntityStatus>();
		_Animation = GetComponent<EntityAnimation>();
		_Visual = GetComponent<EntityVisual>();
		_Sound = GetComponent<EntitySound>();
		_AI.entity = this;
		_Ability.entity = this;
		_Control.entity = this;
		_Status.entity = this;
		_Animation.entity = this;
		_Visual.entity = this;
		_Sound.entity = this;
		return true;
	}

	public bool CheckEnemyOrNeutral(Entity target)
	{
		EntityRelation rel = GetRelation(target);
		if (rel != EntityRelation.Enemy)
		{
			return rel == EntityRelation.Neutral;
		}
		return true;
	}

	public EntityRelation GetRelation(Entity other)
	{
		if (other == null)
		{
			return EntityRelation.Neutral;
		}
		if (this == other)
		{
			return EntityRelation.Self;
		}
		return GetTeamRelation(other.owner) switch
		{
			TeamRelation.Own => EntityRelation.Ally, 
			TeamRelation.Enemy => EntityRelation.Enemy, 
			TeamRelation.Ally => EntityRelation.Ally, 
			_ => EntityRelation.Neutral, 
		};
	}

	public TeamRelation GetTeamRelation(DewPlayer other)
	{
		if (owner == null)
		{
			return TeamRelation.Neutral;
		}
		return owner.GetTeamRelation(other);
	}

	[Server]
	public void Kill()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::Kill()' called when server was not active");
		}
		else
		{
			Kill(ignoreDeathInterrupts: false);
		}
	}

	[Server]
	public void KillNoInterrupt()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::KillNoInterrupt()' called when server was not active");
		}
		else
		{
			Kill(ignoreDeathInterrupts: true);
		}
	}

	[Server]
	private void Kill(bool ignoreDeathInterrupts)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::Kill(System.Boolean)' called when server was not active");
		}
		else
		{
			if (!base.isActive)
			{
				return;
			}
			EventInfoKill info = new EventInfoKill
			{
				actor = _lastAttacker,
				victim = this
			};
			Status.currentHealth = 0f;
			if (info.actor == null)
			{
				info.actor = this;
			}
			if (Status.hasDeathInterrupt && !ignoreDeathInterrupts)
			{
				ListReturnHandle<DeathInterruptEffect> handle;
				List<DeathInterruptEffect> deathInterrupts = DewPool.GetList(out handle);
				foreach (BasicEffect basicEffect in Status._basicEffects)
				{
					if (basicEffect is DeathInterruptEffect di)
					{
						deathInterrupts.Add(di);
					}
				}
				deathInterrupts.Sort((DeathInterruptEffect x, DeathInterruptEffect y) => x.priority.CompareTo(y.priority));
				foreach (DeathInterruptEffect item in deathInterrupts)
				{
					item.onInterrupt?.Invoke(info);
					if (currentHealth > 0f)
					{
						handle.Return();
						return;
					}
				}
				handle.Return();
			}
			EntityEvent_OnDeath?.Invoke(info);
			if (info.actor != null)
			{
				info.actor.ActorEvent_OnKill?.Invoke(info);
			}
			NotifyEntityDeathToClients(info);
			Destroy();
		}
	}

	protected virtual void OnDeath(EventInfoKill info)
	{
	}

	[ClientRpc]
	internal void NotifyEntityDeathToClients(EventInfoKill info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoKill(writer, info);
		SendRPCInternal("System.Void Entity::NotifyEntityDeathToClients(EventInfoKill)", 1937835807, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public override string GetActorReadableName()
	{
		return $"{base.GetActorReadableName()} of {owner.name} ({Status.level})";
	}

	[ClientRpc]
	internal void RpcInvokeInteract(NetworkIdentity target, bool alt)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkIdentity(target);
		writer.WriteBool(alt);
		SendRPCInternal("System.Void Entity::RpcInvokeInteract(Mirror.NetworkIdentity,System.Boolean)", -1989805683, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void WakeUp()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::WakeUp()' called when server was not active");
		}
		else if (isSleeping)
		{
			Network_003CisSleeping_003Ek__BackingField = false;
			_accumulatedSleepTime = 0f;
		}
	}

	[Server]
	public void Sleep()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Entity::Sleep()' called when server was not active");
		}
		else if (!isSleeping)
		{
			Network_003CisSleeping_003Ek__BackingField = true;
		}
	}

	public override bool ShouldBeSaved()
	{
		return true;
	}

	public override void OnSaveActor(Dictionary<string, object> data)
	{
		base.OnSaveActor(data);
		data["level"] = level;
		data["health"] = currentHealth;
		data["mana"] = currentMana;
		data["owner"] = owner;
	}

	public override Actor OnLoadCreateActor(Dictionary<string, object> data, Actor parent)
	{
		return Dew.SpawnEntity(this, (Vector3)data["pos"], (Quaternion)data["rot"], parent, (DewPlayer)data["owner"], (int)data["level"], delegate(Entity e)
		{
			e.Visual.NetworkskipSpawning = true;
		});
	}

	public override void OnLoadActor(Dictionary<string, object> data)
	{
		base.OnLoadActor(data);
		Status.SetHealth((float)data["health"]);
		Status.SetMana((float)data["mana"]);
	}

	protected override NetworkConnectionToClient GetNetworkAuthorityConnection()
	{
		return owner;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_NotifyEntityDeathToClients__EventInfoKill(EventInfoKill info)
	{
		OnDeath(info);
	}

	protected static void InvokeUserCode_NotifyEntityDeathToClients__EventInfoKill(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC NotifyEntityDeathToClients called on server.");
		}
		else
		{
			((Entity)obj).UserCode_NotifyEntityDeathToClients__EventInfoKill(GeneratedNetworkCode._Read_EventInfoKill(reader));
		}
	}

	protected void UserCode_RpcInvokeInteract__NetworkIdentity__Boolean(NetworkIdentity target, bool alt)
	{
		IInteractable interactable = target.GetComponent<IInteractable>();
		if (interactable != null)
		{
			try
			{
				interactable.OnInteract(this, alt);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, interactable as UnityEngine.Object);
			}
			ManagerBase<ControlManager>.instance.UpdateInteractableFocus(alsoCheckNearby: true);
		}
	}

	protected static void InvokeUserCode_RpcInvokeInteract__NetworkIdentity__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeInteract called on server.");
		}
		else
		{
			((Entity)obj).UserCode_RpcInvokeInteract__NetworkIdentity__Boolean(reader.ReadNetworkIdentity(), reader.ReadBool());
		}
	}

	static Entity()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Entity), "System.Void Entity::NotifyEntityDeathToClients(EventInfoKill)", InvokeUserCode_NotifyEntityDeathToClients__EventInfoKill);
		RemoteProcedureCalls.RegisterRpc(typeof(Entity), "System.Void Entity::RpcInvokeInteract(Mirror.NetworkIdentity,System.Boolean)", InvokeUserCode_RpcInvokeInteract__NetworkIdentity__Boolean);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(isSleeping);
			writer.WriteNetworkBehaviour(Network_owner);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(isSleeping);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_owner);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref isSleeping, OnIsSleepingChanged, reader.ReadBool());
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _owner, null, reader, ref ____ownerNetId);
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isSleeping, OnIsSleepingChanged, reader.ReadBool());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _owner, null, reader, ref ____ownerNetId);
		}
	}
}
