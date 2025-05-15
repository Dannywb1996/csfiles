using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EntityTransformSync))]
public class EntityControl : EntityComponent, ICleanup
{
	internal struct OngoingDisplacementInternalData
	{
		public DewEase knownEase;

		public EaseFunction easeFunction;

		public Vector3 start;

		public Vector3 last;
	}

	private struct PositionSyncData
	{
		public double timestamp;

		public Vector3 position;

		public Vector3 velocity;

		public float desiredAngle;
	}

	public enum BlockableAction
	{
		Move,
		Ability,
		Attack
	}

	public enum BlockStatus
	{
		Allowed,
		Blocked,
		BlockedCancelable
	}

	public SafeAction<float, float> ClientEvent_OnOuterRadiusChanged;

	public SafeAction<float, float> ClientEvent_OnInnerRadiusChanged;

	public SafeAction<Vector3, Vector3> ClientEvent_OnTeleport;

	public List<Channel> ongoingChannels = new List<Channel>();

	[SyncVar(hook = "OuterRadiusChanged")]
	[SerializeField]
	private float _outerRadius = 0.45f;

	[SyncVar(hook = "InnerRadiusChanged")]
	[SerializeField]
	private float _innerRadius = 0.25f;

	[SyncVar]
	[SerializeField]
	private bool _freeMovement;

	public bool obstacleAvoidance = true;

	public float baseAgentSpeed = 5f;

	public float rotationSmoothTime = 0.05f;

	public float normalizedAcceleration = 4f;

	public float rotateSpeed = 720f;

	internal NavMeshAgent _agent;

	[SyncVar]
	private float? _overridenDesiredAngle;

	private float _overrideAngleLifeTime;

	private float _overrideAngle;

	private Entity _overrideAngleEntity;

	private Vector3? _overrideAnglePosition;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisControlReversed_003Ek__BackingField;

	[SyncVar]
	private int _gamepadRotationLockCounter;

	public const float QueuedActionExpireTime = 0.2f;

	private List<ActionBase> _queuedActions = new List<ActionBase>();

	private ActionBase _moveToActionOwner;

	[SyncVar]
	private Vector3 _moveToActionDestination;

	[SyncVar]
	private float _moveToActionRequiredDistance;

	[NonSerialized]
	public Entity attackTarget;

	private bool _isMoveToAttackActive;

	private float _lastAttackMoveCheckTime;

	private float _lastAttackStartTime;

	private float _lastAttackMovSpdMultiplier;

	private float _lastAttackMovSpdDuration;

	[SyncVar]
	private byte _blockMove;

	[SyncVar]
	private byte _blockAbility;

	[SyncVar]
	private byte _blockAttack;

	[SyncVar]
	private byte _blockMoveCancelable;

	[SyncVar]
	private byte _blockAbilityCancelable;

	[SyncVar]
	private byte _blockAttackCancelable;

	internal const float DispByTargetGoalDistance = 0.05f;

	private const int MaxCommandsPerTick = 100;

	private const float SetDestinationMinInterval = 0.1f;

	private const float SetDestinationMinSqrMagnitude = 0.2f;

	private const float CommandBufferLifetime = 0.15f;

	public const float CommandStallTimeout = 0.15f;

	public SafeAction<Displacement> ClientEvent_OnDisplacementStarted;

	public SafeAction<Displacement> ClientEvent_OnDisplacementFinished;

	public SafeAction<Displacement> ClientEvent_OnDisplacementCanceled;

	private OngoingDisplacementInternalData _dispData;

	private float _lastStardustInteractTime;

	private const float AgentGoalSqrDistance = 0.1f;

	private const float SetAgentDestinationInterval = 0.25f;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CforceWalking_003Ek__BackingField;

	private float _localWalkStrength;

	[SyncVar]
	private float _syncedWalkStrength;

	private Vector3 _localVelocity;

	private float _lastAgentDestinationTime;

	internal Vector3? _desiredAgentDestination;

	private float _localDesiredAngle;

	private float _desiredAngleCv;

	public static float SyncFixWarpDistance;

	public static float SyncFixSmoothTime;

	public static float SyncFixSnapshotLifetime;

	public static float SyncAgentVelocityLifetime;

	public static float SyncExtrapolateMaxTime;

	public static float SyncExtrapolateStrength;

	private Vector3? _movementVector;

	private bool _isMovementVectorDirection;

	private float _destinationMovementSpeedMultiplier;

	[SyncVar(hook = "OnMovementSyncDataReceived")]
	private PositionSyncData _positionSyncData;

	private float _positionSyncDataUnscaledTime;

	private float _positionSyncDataTime;

	private double _positionSyncDataTimestampLowerBound;

	private Vector3 _fixCv;

	private const float PositionSyncDataSendRateMinInterval = 1f / 60f;

	private float _lastPositionSyncDataSendTime;

	public float currentMaxAgentSpeed => baseAgentSpeed * base.entity.Status.movementSpeedMultiplier * GetMovementSpeedMultiplierByAttack();

	public float outerRadius
	{
		get
		{
			return _outerRadius;
		}
		set
		{
			Network_outerRadius = value;
		}
	}

	public float innerRadius
	{
		get
		{
			return _innerRadius;
		}
		set
		{
			Network_innerRadius = value;
		}
	}

	public bool freeMovement
	{
		get
		{
			return _freeMovement;
		}
		set
		{
			Network_freeMovement = value;
		}
	}

	public Vector3 agentPosition
	{
		get
		{
			if (_agent != null && _agent.enabled)
			{
				return _agent.nextPosition;
			}
			return Dew.GetPositionOnGround(base.transform.position);
		}
	}

	public Quaternion desiredRotation => Quaternion.Euler(0f, desiredAngle, 0f);

	public float desiredAngle
	{
		get
		{
			if (!isLocalMovementProcessor)
			{
				return _positionSyncData.desiredAngle;
			}
			return _localDesiredAngle;
		}
	}

	public ProxyCollider proxyCollider { get; private set; }

	public bool isControlReversed
	{
		[CompilerGenerated]
		get
		{
			return _003CisControlReversed_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisControlReversed_003Ek__BackingField = value;
		}
	}

	public bool isGamepadRotationLocked => _gamepadRotationLockCounter > 0;

	public bool canDestroy => true;

	public IReadOnlyList<ActionBase> queuedActions => _queuedActions;

	public bool isDoingMoveToAction { get; private set; }

	public bool isAirborne
	{
		get
		{
			if (isDisplacing)
			{
				return !ongoingDisplacement.isFriendly;
			}
			return false;
		}
	}

	public bool isDashing
	{
		get
		{
			if (isDisplacing)
			{
				return ongoingDisplacement.isFriendly;
			}
			return false;
		}
	}

	public bool isDisplacing => ongoingDisplacement != null;

	public Displacement ongoingDisplacement { get; private set; }

	public bool isClientSideMovement
	{
		get
		{
			if (base.entity.owner != null)
			{
				return base.entity.owner.isHumanPlayer;
			}
			return false;
		}
	}

	public bool isLocalMovementProcessor
	{
		get
		{
			if (!isClientSideMovement || !base.isOwned)
			{
				if (!isClientSideMovement)
				{
					return base.isServer;
				}
				return false;
			}
			return true;
		}
	}

	public float walkStrength => FilterWalkStrength(isLocalMovementProcessor ? _localWalkStrength : _syncedWalkStrength);

	public bool forceWalking
	{
		[CompilerGenerated]
		get
		{
			return _003CforceWalking_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CforceWalking_003Ek__BackingField = value;
		}
	}

	public bool isWalking => walkStrength > 0.01f;

	public Vector3 agentVelocity
	{
		get
		{
			if (!isLocalMovementProcessor)
			{
				return _positionSyncData.velocity;
			}
			return _localVelocity;
		}
	}

	private float _positionSyncDataUnscaledElapsedTime => Time.unscaledTime - _positionSyncDataUnscaledTime;

	private float _positionSyncDataElapsedTime => Time.time - _positionSyncDataTime;

	private float _positionSyncDataExtrapolateTime => Mathf.Clamp((float)(NetworkTime.time - _positionSyncData.timestamp), 0f, SyncExtrapolateMaxTime);

	public float Network_outerRadius
	{
		get
		{
			return _outerRadius;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _outerRadius, 1uL, OuterRadiusChanged);
		}
	}

	public float Network_innerRadius
	{
		get
		{
			return _innerRadius;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _innerRadius, 2uL, InnerRadiusChanged);
		}
	}

	public bool Network_freeMovement
	{
		get
		{
			return _freeMovement;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _freeMovement, 4uL, null);
		}
	}

	public float? Network_overridenDesiredAngle
	{
		get
		{
			return _overridenDesiredAngle;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _overridenDesiredAngle, 8uL, null);
		}
	}

	public bool Network_003CisControlReversed_003Ek__BackingField
	{
		get
		{
			return isControlReversed;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isControlReversed, 16uL, null);
		}
	}

	public int Network_gamepadRotationLockCounter
	{
		get
		{
			return _gamepadRotationLockCounter;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _gamepadRotationLockCounter, 32uL, null);
		}
	}

	public Vector3 Network_moveToActionDestination
	{
		get
		{
			return _moveToActionDestination;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _moveToActionDestination, 64uL, null);
		}
	}

	public float Network_moveToActionRequiredDistance
	{
		get
		{
			return _moveToActionRequiredDistance;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _moveToActionRequiredDistance, 128uL, null);
		}
	}

	public byte Network_blockMove
	{
		get
		{
			return _blockMove;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _blockMove, 256uL, null);
		}
	}

	public byte Network_blockAbility
	{
		get
		{
			return _blockAbility;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _blockAbility, 512uL, null);
		}
	}

	public byte Network_blockAttack
	{
		get
		{
			return _blockAttack;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _blockAttack, 1024uL, null);
		}
	}

	public byte Network_blockMoveCancelable
	{
		get
		{
			return _blockMoveCancelable;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _blockMoveCancelable, 2048uL, null);
		}
	}

	public byte Network_blockAbilityCancelable
	{
		get
		{
			return _blockAbilityCancelable;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _blockAbilityCancelable, 4096uL, null);
		}
	}

	public byte Network_blockAttackCancelable
	{
		get
		{
			return _blockAttackCancelable;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _blockAttackCancelable, 8192uL, null);
		}
	}

	public bool Network_003CforceWalking_003Ek__BackingField
	{
		get
		{
			return forceWalking;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref forceWalking, 16384uL, null);
		}
	}

	public float Network_syncedWalkStrength
	{
		get
		{
			return _syncedWalkStrength;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _syncedWalkStrength, 32768uL, null);
		}
	}

	public PositionSyncData Network_positionSyncData
	{
		get
		{
			return _positionSyncData;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _positionSyncData, 65536uL, OnMovementSyncDataReceived);
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		base.gameObject.SetLayerRecursive(8);
		Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
		rigidbody.useGravity = false;
		rigidbody.isKinematic = true;
		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		proxyCollider = DewPhysics.AddProxyOfEntity(base.entity);
		_agent = base.gameObject.AddComponent<NavMeshAgent>();
		_agent.updateRotation = false;
		_agent.updatePosition = false;
		_agent.autoBraking = false;
		UpdateObstacleAvoidanceAndPriority();
		DoSyncStart();
	}

	private void UpdateObstacleAvoidanceAndPriority()
	{
		bool hasUncollidable = base.entity.Status.hasUncollidable;
		_agent.obstacleAvoidanceType = ((obstacleAvoidance && !hasUncollidable) ? ObstacleAvoidanceType.HighQualityObstacleAvoidance : ObstacleAvoidanceType.NoObstacleAvoidance);
		float preferredRadius = innerRadius;
		if (Math.Abs(_agent.radius - preferredRadius) > 0.01f)
		{
			_agent.radius = preferredRadius;
			if (_agent.enabled)
			{
				_agent.enabled = false;
				_agent.enabled = true;
			}
		}
		if (_agent.enabled && _agent.isOnNavMesh)
		{
			int priority = 50;
			if (base.entity is Hero)
			{
				priority = ((!base.entity.Status.hasCrowdControlImmunity) ? 75 : 0);
			}
			else if (base.entity is Monster m)
			{
				priority = m.type switch
				{
					Monster.MonsterType.Lesser => base.entity.Status.hasCrowdControlImmunity ? 20 : 99, 
					Monster.MonsterType.Normal => base.entity.Status.hasCrowdControlImmunity ? 10 : 50, 
					Monster.MonsterType.MiniBoss => 5, 
					Monster.MonsterType.Boss => 5, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
			}
			else if (base.entity is Summon)
			{
				priority = 80;
			}
			priority = Mathf.Clamp(priority, 0, 99);
			if (priority != _agent.avoidancePriority)
			{
				_agent.avoidancePriority = priority;
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		_localDesiredAngle = CastInfo.GetAngle(base.transform.forward);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		base.entity.EntityEvent_OnDeath += new Action<EventInfoKill>(EntityEventOnDeath);
	}

	private bool HasAuthorityWithLog()
	{
		if (!base.isOwned)
		{
			Debug.LogWarning("No authority on " + base.entity.GetActorReadableName());
			return false;
		}
		return true;
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
		RpcOnDeath();
	}

	[ClientRpc]
	private void RpcOnDeath()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityControl::RpcOnDeath()", -69985209, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public override void OnStop()
	{
		base.OnStop();
		UnityEngine.Object.Destroy(GetComponent<CapsuleCollider>());
		UnityEngine.Object.Destroy(GetComponent<Rigidbody>());
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		NavMeshAgent agent = GetComponent<NavMeshAgent>();
		if (agent != null)
		{
			UnityEngine.Object.Destroy(agent);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!base.entity.isSleeping)
		{
			DoAttackFrameUpdate();
			DoActionFrameUpdate();
			DoMovementFrameUpdate();
		}
	}

	private void DoOverrideRotationLogicUpdate(float dt)
	{
		if (!base.isServer)
		{
			return;
		}
		if (_overrideAngleLifeTime <= 0f)
		{
			Network_overridenDesiredAngle = null;
			return;
		}
		_overrideAngleLifeTime = Mathf.MoveTowards(_overrideAngleLifeTime, 0f, dt);
		if (_overrideAngleEntity != null)
		{
			if (!_overrideAngleEntity.isActive || _overrideAngleEntity.Status.isDead)
			{
				_overrideAngleLifeTime = 0f;
				Network_overridenDesiredAngle = null;
			}
			else
			{
				Network_overridenDesiredAngle = CastInfo.GetAngle(_overrideAngleEntity.position - base.entity.position);
			}
		}
		else if (_overrideAnglePosition.HasValue)
		{
			Network_overridenDesiredAngle = CastInfo.GetAngle(_overrideAnglePosition.Value - base.entity.position);
		}
		else
		{
			Network_overridenDesiredAngle = _overrideAngle;
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.entity.isSleeping)
		{
			if (base.isServer)
			{
				DoOverrideRotationLogicUpdate(dt);
			}
			DoMovementLogicUpdate();
			if (base.isServer && ongoingChannels.Count > 0)
			{
				TickChannels(dt);
			}
			bool shouldDisableAgent = freeMovement || base.entity.Visual.isSpawning || base.entity.IsNullInactiveDeadOrKnockedOut() || (isDisplacing && ShouldDisplacementDisableAgent());
			if (shouldDisableAgent && _agent.enabled)
			{
				_agent.enabled = false;
			}
			else if (!shouldDisableAgent && !_agent.enabled)
			{
				Vector3 temp = base.transform.position;
				temp = Dew.GetPositionOnGround(Dew.GetValidAgentPosition(temp));
				base.transform.position = temp;
				_agent.enabled = true;
			}
			if (base.isServer && (base.entity.IsNullInactiveDeadOrKnockedOut() || base.entity.Visual.isSpawning))
			{
				Stop();
			}
			if (proxyCollider != null)
			{
				DewPhysics.UpdateProxyPosition(proxyCollider, base.transform.position);
			}
		}
	}

	[Server]
	public void StartDisplacement(Displacement disp)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::StartDisplacement(Displacement)' called when server was not active");
			return;
		}
		if (!disp.isFriendly && base.entity.Status.hasCrowdControlImmunity)
		{
			CancelImmediately();
			return;
		}
		if (!Dew.IsOkay(disp))
		{
			CancelImmediately();
			return;
		}
		StartDisplacementLocal(disp);
		RpcStartDisplacement(disp);
		void CancelImmediately()
		{
			disp.hasStarted = true;
			disp.isAlive = false;
			disp.onCancel?.Invoke();
			ClientEvent_OnDisplacementCanceled?.Invoke(disp);
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnIgnoreCC(base.entity);
		}
	}

	[ClientRpc]
	private void RpcStartDisplacement(Displacement disp)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteDisplacement(disp);
		SendRPCInternal("System.Void EntityControl::RpcStartDisplacement(Displacement)", -1028026508, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void StartDisplacementLocal(Displacement disp)
	{
		if (disp.hasStarted)
		{
			Debug.LogWarning("Tried to start a displacement that has already started!");
			return;
		}
		if (isDisplacing)
		{
			CancelOngoingDisplacementLocal();
		}
		ongoingDisplacement = disp;
		ongoingDisplacement.hasStarted = true;
		ongoingDisplacement.isAlive = true;
		_dispData = default(OngoingDisplacementInternalData);
		ClientEvent_OnDisplacementStarted?.Invoke(disp);
	}

	[Server]
	public void CancelOngoingDisplacement()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::CancelOngoingDisplacement()' called when server was not active");
			return;
		}
		RpcCancelOngoingDisplacement();
		CancelOngoingDisplacementLocal();
	}

	[ClientRpc]
	private void RpcCancelOngoingDisplacement()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityControl::RpcCancelOngoingDisplacement()", -2097004086, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void CancelOngoingDisplacementLocal()
	{
		if (isDisplacing)
		{
			Displacement disp = ongoingDisplacement;
			ongoingDisplacement = null;
			_dispData = default(OngoingDisplacementInternalData);
			disp.isAlive = false;
			disp.onCancel?.Invoke();
			ClientEvent_OnDisplacementCanceled?.Invoke(disp);
		}
	}

	[Server]
	public void Rotate(Vector3 forward, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Rotate(UnityEngine.Vector3,System.Boolean,System.Single)' called when server was not active");
		}
		else if (!(forward.Flattened().sqrMagnitude < 0.01f))
		{
			Rotate(Quaternion.LookRotation(forward.Flattened()), immediately, overrideDuration);
		}
	}

	[Server]
	public void RotateTowards(Vector3 position, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::RotateTowards(UnityEngine.Vector3,System.Boolean,System.Single)' called when server was not active");
			return;
		}
		if (overrideDuration > 0f)
		{
			_overrideAngleEntity = null;
			_overrideAnglePosition = position;
			_overrideAngleLifeTime = overrideDuration;
			DoOverrideRotationLogicUpdate(0f);
		}
		Rotate(position - base.transform.position, immediately);
	}

	[Server]
	public void RotateTowards(Entity ent, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::RotateTowards(Entity,System.Boolean,System.Single)' called when server was not active");
		}
		else if (!(ent == null))
		{
			if (overrideDuration > 0f)
			{
				_overrideAngleEntity = ent;
				_overrideAnglePosition = null;
				_overrideAngleLifeTime = overrideDuration;
				DoOverrideRotationLogicUpdate(0f);
			}
			Rotate(ent.position - base.transform.position, immediately);
		}
	}

	[Server]
	public void Rotate(Quaternion rotation, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Rotate(UnityEngine.Quaternion,System.Boolean,System.Single)' called when server was not active");
		}
		else
		{
			Rotate(rotation.eulerAngles.y, immediately, overrideDuration);
		}
	}

	[Server]
	public void Rotate(float angle, bool immediately, float overrideDuration = -1f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Rotate(System.Single,System.Boolean,System.Single)' called when server was not active");
			return;
		}
		if (isLocalMovementProcessor)
		{
			_localDesiredAngle = angle;
		}
		else
		{
			TpcSetDesiredAngle(angle);
		}
		if (immediately)
		{
			RpcRotateImmediately(angle);
		}
		if (overrideDuration > 0f)
		{
			_overrideAngleEntity = null;
			_overrideAnglePosition = null;
			_overrideAngle = angle;
			_overrideAngleLifeTime = overrideDuration;
			DoOverrideRotationLogicUpdate(0f);
		}
	}

	[Command]
	public void CmdCancelOngoingChannels()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void EntityControl::CmdCancelOngoingChannels()", -1975460018, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void CancelOngoingChannels()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::CancelOngoingChannels()' called when server was not active");
			return;
		}
		foreach (Channel c in ongoingChannels)
		{
			if (c.isAlive)
			{
				c.Cancel();
			}
		}
	}

	[TargetRpc]
	private void TpcSetDesiredAngle(float angle)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(angle);
		SendTargetRPCInternal(null, "System.Void EntityControl::TpcSetDesiredAngle(System.Single)", 1502117120, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcRotateImmediately(float angle)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(angle);
		SendRPCInternal("System.Void EntityControl::RpcRotateImmediately(System.Single)", 698520438, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void StopOverrideRotation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::StopOverrideRotation()' called when server was not active");
			return;
		}
		_overrideAngleEntity = null;
		_overrideAnglePosition = null;
		_overrideAngleLifeTime = 0f;
		Network_overridenDesiredAngle = null;
	}

	[Server]
	public void LockGamepadRotation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::LockGamepadRotation()' called when server was not active");
		}
		else
		{
			Network_gamepadRotationLockCounter = _gamepadRotationLockCounter + 1;
		}
	}

	[Server]
	public void UnlockGamepadRotation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::UnlockGamepadRotation()' called when server was not active");
		}
		else
		{
			Network_gamepadRotationLockCounter = _gamepadRotationLockCounter - 1;
		}
	}

	[Server]
	public void Teleport(Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Teleport(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		base.entity._wasStuckCounter = 0;
		TeleportLocal(position);
		RpcTeleport(position);
	}

	[ClientRpc]
	private void RpcTeleport(Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(position);
		SendRPCInternal("System.Void EntityControl::RpcTeleport(UnityEngine.Vector3)", -720064226, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void TeleportLocal(Vector3 position)
	{
		Vector3 prev = _agent.nextPosition;
		base.entity.Visual.DoActionBeforeTeleport();
		_agent.Warp(position);
		base.entity.Visual.DoActionAfterTeleport();
		ClientEvent_OnTeleport?.Invoke(prev, position);
		if (base.entity.isOwned && ManagerBase<ControlManager>.instance.controllingEntity == base.entity && Vector3.Distance(prev, position) > 13f)
		{
			ManagerBase<CameraManager>.instance.SnapCameraToFocusedEntity();
		}
	}

	[Command]
	public void CmdLookInDirection(Vector3 dir)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(dir);
		SendCommandInternal("System.Void EntityControl::CmdLookInDirection(UnityEngine.Vector3)", -1864142367, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public void SetAgentPosition(Vector3 position)
	{
		if (isLocalMovementProcessor)
		{
			_agent.nextPosition = position;
		}
	}

	private void OuterRadiusChanged(float oldValue, float newValue)
	{
		ClientEvent_OnOuterRadiusChanged?.Invoke(oldValue, newValue);
		if (!(proxyCollider == null))
		{
			((CircleCollider2D)proxyCollider.collider).radius = newValue;
		}
	}

	private void InnerRadiusChanged(float oldValue, float newValue)
	{
		ClientEvent_OnInnerRadiusChanged?.Invoke(oldValue, newValue);
	}

	public void OnCleanup()
	{
		Stop();
		CancelOngoingDisplacement();
		DewPhysics.RemoveProxy(proxyCollider);
	}

	[Server]
	private void SetMoveToActionState(bool state)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::SetMoveToActionState(System.Boolean)' called when server was not active");
			return;
		}
		if (isLocalMovementProcessor)
		{
			SetMoveToActionStateLocal(state);
			return;
		}
		TpcSetMoveToActionState(state);
		isDoingMoveToAction = state;
	}

	[TargetRpc]
	private void TpcSetMoveToActionState(bool state)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(state);
		SendTargetRPCInternal(null, "System.Void EntityControl::TpcSetMoveToActionState(System.Boolean)", -437273642, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void SetMoveToActionStateLocal(bool state)
	{
		if (isDoingMoveToAction && !state)
		{
			_desiredAgentDestination = null;
		}
		isDoingMoveToAction = state;
		if (state)
		{
			_destinationMovementSpeedMultiplier = 1f;
			SetAgentDestinationLocal(_moveToActionDestination, important: true);
		}
	}

	[Server]
	public void Cast(AbilityTrigger trigger, int configIndex, CastInfo info, bool allowMoveToCast = true, bool skipRangeCheck = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Cast(AbilityTrigger,System.Int32,CastInfo,System.Boolean,System.Boolean)' called when server was not active");
		}
		else if (!trigger.IsNullOrInactive() && configIndex < trigger.configs.Length)
		{
			AddAction(new ActionCast
			{
				trigger = trigger,
				configIndex = configIndex,
				isAllowedToMove = allowMoveToCast,
				info = info,
				isPredictedOnCast = false,
				skipRangeCheck = skipRangeCheck
			});
		}
	}

	[Server]
	public void Cast(AbilityTrigger trigger, int configIndex, Entity predictTarget, bool allowMoveToCast = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Cast(AbilityTrigger,System.Int32,Entity,System.Boolean)' called when server was not active");
		}
		else if (!trigger.IsNullOrInactive() && configIndex < trigger.configs.Length)
		{
			AddAction(new ActionCast
			{
				trigger = trigger,
				configIndex = configIndex,
				isAllowedToMove = allowMoveToCast,
				predictTarget = predictTarget,
				isPredictedOnCast = true
			});
		}
	}

	[Server]
	public void Cast(AbilityTrigger trigger, CastInfo info, bool allowMoveToCast = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Cast(AbilityTrigger,CastInfo,System.Boolean)' called when server was not active");
		}
		else
		{
			Cast(trigger, trigger.currentConfigIndex, info, allowMoveToCast);
		}
	}

	[Server]
	public void Cast(AbilityTrigger trigger, Entity predictTarget, bool allowMoveToCast = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Cast(AbilityTrigger,Entity,System.Boolean)' called when server was not active");
		}
		else
		{
			Cast(trigger, trigger.currentConfigIndex, predictTarget, allowMoveToCast);
		}
	}

	[Command]
	public void CmdCast(AbilityTrigger trigger, int configIndex, CastInfo info, bool allowMoveToCast, bool skipRangeCheck)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(trigger);
		writer.WriteInt(configIndex);
		GeneratedNetworkCode._Write_CastInfo(writer, info);
		writer.WriteBool(allowMoveToCast);
		writer.WriteBool(skipRangeCheck);
		SendCommandInternal("System.Void EntityControl::CmdCast(AbilityTrigger,System.Int32,CastInfo,System.Boolean,System.Boolean)", 1776035736, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	private void AddAction(ActionBase newAction)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::AddAction(ActionBase)' called when server was not active");
			return;
		}
		newAction._entity = base.entity;
		newAction._addedUnscaledTime = Time.unscaledTime;
		newAction._didDoFirstTick = false;
		_queuedActions.Add(newAction);
	}

	private void DoActionFrameUpdate()
	{
		if (!base.isServer)
		{
			return;
		}
		if (!isDoingMoveToAction)
		{
			_moveToActionOwner = null;
		}
		bool didDoMoveToAction = false;
		for (int i = _queuedActions.Count - 1; i >= 0; i--)
		{
			ActionBase a = _queuedActions[i];
			if (!isDoingMoveToAction && a._didDoFirstTick && a.isAllowedToMove)
			{
				if (a.ShouldCancelIfDisallowedToMove())
				{
					RemoveAction(i);
					continue;
				}
				a.isAllowedToMove = false;
			}
			try
			{
				if (a.Tick())
				{
					if (_queuedActions.Count == 0)
					{
						break;
					}
					RemoveAction(i);
					continue;
				}
				if (a.isAllowedToMove && !didDoMoveToAction)
				{
					Vector3? moveDest = a.GetMoveDestination();
					if (moveDest.HasValue)
					{
						Network_moveToActionDestination = moveDest.Value;
						Network_moveToActionRequiredDistance = a.GetMoveDestinationRequiredDistance();
						_moveToActionOwner = a;
						if (a.isFirstTick)
						{
							SetMoveToActionState(state: true);
						}
						a._didDoFirstTick = true;
						didDoMoveToAction = true;
						continue;
					}
				}
				a._didDoFirstTick = true;
			}
			catch (Exception exception)
			{
				Debug.LogWarning($"Action {a} removed from {base.entity.GetActorReadableName()} due to exception below");
				Debug.LogException(exception);
				RemoveAction(i);
				continue;
			}
			if (Time.unscaledTime - a._addedUnscaledTime > 0.2f)
			{
				RemoveAction(i);
			}
		}
		if (_moveToActionOwner == null && isDoingMoveToAction)
		{
			SetMoveToActionState(state: false);
		}
	}

	internal void RemoveAction(int index)
	{
		if (index >= 0 && index < _queuedActions.Count)
		{
			ActionBase a = _queuedActions[index];
			_queuedActions.RemoveAt(index);
			if (_moveToActionOwner == a)
			{
				_moveToActionOwner = null;
			}
		}
	}

	private void StopDoingMoveToAction()
	{
		if (isLocalMovementProcessor && isDoingMoveToAction)
		{
			_desiredAgentDestination = null;
			isDoingMoveToAction = false;
			if (base.isServer)
			{
				_moveToActionOwner = null;
			}
			else
			{
				CmdStopDoingMoveToAction();
			}
		}
	}

	[Command]
	private void CmdStopDoingMoveToAction()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void EntityControl::CmdStopDoingMoveToAction()", -1348140044, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	public void CmdAttack(Entity target, bool doChase)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(target);
		writer.WriteBool(doChase);
		SendCommandInternal("System.Void EntityControl::CmdAttack(Entity,System.Boolean)", -720151533, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void Attack(Entity target, bool doChase)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Attack(Entity,System.Boolean)' called when server was not active");
			return;
		}
		attackTarget = target;
		_isMoveToAttackActive = doChase;
	}

	[Server]
	public void AttackMove(Vector3 destination, bool useDistanceFromDestination = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::AttackMove(UnityEngine.Vector3,System.Boolean)' called when server was not active");
			return;
		}
		AddAction(new ActionAttackMove
		{
			destination = destination,
			isAllowedToMove = true,
			useDistanceFromDestination = useDistanceFromDestination
		});
	}

	[Command]
	public void CmdAttackMove(Vector3 destination, bool useDistanceFromDestination)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(destination);
		writer.WriteBool(useDistanceFromDestination);
		SendCommandInternal("System.Void EntityControl::CmdAttackMove(UnityEngine.Vector3,System.Boolean)", -1264557694, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	private void CmdCancelMoveToAttack()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void EntityControl::CmdCancelMoveToAttack()", -917930445, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void DoAttackFrameUpdate()
	{
		if (isLocalMovementProcessor && !isWalking)
		{
			_lastAttackStartTime -= Time.deltaTime;
		}
		if (!base.isServer || (object)attackTarget == null || (attackTarget.IsNullInactiveDeadOrKnockedOut() && !TryGetOtherTarget()))
		{
			return;
		}
		AbilityTrigger attackTrigger = base.entity.Ability.attackAbility;
		if (attackTrigger.IsNullOrInactive())
		{
			return;
		}
		if (_isMoveToAttackActive)
		{
			for (int i = _queuedActions.Count - 1; i >= 0; i--)
			{
				ActionBase c = _queuedActions[i];
				if (c is ActionCast cast && cast.trigger == attackTrigger)
				{
					if (c.isAllowedToMove)
					{
						return;
					}
					RemoveAction(i);
				}
			}
			Cast(attackTrigger, -1, attackTarget);
		}
		else if (attackTrigger.currentConfig.CheckRange(base.entity, attackTarget))
		{
			if (attackTrigger.CanBeCast())
			{
				Cast(attackTrigger, -1, attackTarget, allowMoveToCast: false);
			}
		}
		else
		{
			TryGetOtherTarget();
		}
	}

	private bool TryGetOtherTarget()
	{
		AbilityTrigger attackTrigger = base.entity.Ability.attackAbility;
		if (attackTrigger.IsNullOrInactive())
		{
			return false;
		}
		CollisionCheckSettings.pivot = ((attackTarget != null) ? attackTarget.transform.position : agentPosition);
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> ents = DewPhysics.OverlapCircleAllEntities(out handle, agentPosition, attackTrigger.currentConfig.effectiveRange, attackTrigger.currentConfig.targetValidator, base.entity, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromPivot
		});
		if (ents.Length > 0)
		{
			attackTarget = ents[0];
			handle.Return();
			return true;
		}
		attackTarget = null;
		handle.Return();
		return false;
	}

	[Server]
	internal void SetAttackMovSpdDisadvantage(float channelDuration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::SetAttackMovSpdDisadvantage(System.Single)' called when server was not active");
		}
		else if (isLocalMovementProcessor)
		{
			SetAttackMovSpdDisadvantage_Imp(channelDuration);
		}
		else if (isClientSideMovement && base.entity.owner.isHumanPlayer)
		{
			TpcSetAttackMovSpdDisadvantage(channelDuration);
		}
	}

	[TargetRpc]
	internal void TpcSetAttackMovSpdDisadvantage(float channelDuration)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(channelDuration);
		SendTargetRPCInternal(null, "System.Void EntityControl::TpcSetAttackMovSpdDisadvantage(System.Single)", 1424288161, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void SetAttackMovSpdDisadvantage_Imp(float channelDuration)
	{
		_lastAttackStartTime = Time.time;
		_lastAttackMovSpdMultiplier = 0.5f;
		_lastAttackMovSpdDuration = channelDuration * 2f;
	}

	private float GetMovementSpeedMultiplierByAttack()
	{
		float elapsed = Time.time - _lastAttackStartTime;
		if (elapsed > _lastAttackMovSpdDuration)
		{
			return 1f;
		}
		return Mathf.Lerp(_lastAttackMovSpdMultiplier, 1f, elapsed / _lastAttackMovSpdDuration);
	}

	private void TickChannel(Channel channel, float dt)
	{
		if (!channel.isAlive)
		{
			ongoingChannels.Remove(channel);
			return;
		}
		if (!base.entity.isActive || base.entity is Hero { isKnockedOut: not false })
		{
			channel.isAlive = false;
			DecrementBlockCounters(channel);
			ongoingChannels.Remove(channel);
			channel.onCancel?.Invoke();
			return;
		}
		if (channel._validators != null)
		{
			bool cancel = false;
			for (int j = 0; j < channel._validators.Count; j++)
			{
				if (!channel._validators[j]())
				{
					channel.isAlive = false;
					DecrementBlockCounters(channel);
					ongoingChannels.Remove(channel);
					channel.onCancel?.Invoke();
					cancel = true;
					break;
				}
			}
			if (cancel)
			{
				return;
			}
		}
		if (channel.uncancellableTime > 0f && channel.elapsedTime < channel.uncancellableTime && channel.elapsedTime + dt >= channel.uncancellableTime && channel.blockedActions.HasFlag(Channel.BlockedAction.Cancelable))
		{
			DecrementBlockCounters(channel);
			IncrementBlockCounters(channel.blockedActions);
		}
		channel.elapsedTime += dt;
		channel.onTick?.Invoke();
		if (channel.elapsedTime >= channel.duration)
		{
			channel.elapsedTime = channel.duration;
			channel.isAlive = false;
			DecrementBlockCounters(channel, channel.duration > 0.0001f);
			ongoingChannels.Remove(channel);
			channel.onComplete?.Invoke();
		}
	}

	private void TickChannels(float dt)
	{
		for (int i = ongoingChannels.Count - 1; i >= 0; i--)
		{
			Channel channel = ongoingChannels[i];
			TickChannel(channel, dt);
		}
	}

	public Channel StartDaze(float duration)
	{
		Channel c = new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = duration
		};
		if (duration <= 0.0001f)
		{
			return c;
		}
		StartChannel(c);
		return c;
	}

	[Server]
	public Channel StartChannel(Channel channel)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'Channel EntityControl::StartChannel(Channel)' called when server was not active");
			return null;
		}
		channel._owner = base.entity;
		channel.isAlive = true;
		if (channel.uncancellableTime > 0f && channel.blockedActions.HasFlag(Channel.BlockedAction.Cancelable))
		{
			IncrementBlockCounters(channel.blockedActions & ~Channel.BlockedAction.Cancelable);
		}
		else
		{
			IncrementBlockCounters(channel.blockedActions);
		}
		ongoingChannels.Add(channel);
		TickChannel(channel, 0.0001f);
		return channel;
	}

	[Server]
	public void IncrementBlockCounters(Channel.BlockedAction action)
	{
		checked
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void EntityControl::IncrementBlockCounters(Channel/BlockedAction)' called when server was not active");
			}
			else if (action.HasFlag(Channel.BlockedAction.Cancelable))
			{
				if (action.HasFlag(Channel.BlockedAction.Move))
				{
					Network_blockMoveCancelable = (byte)(unchecked((uint)_blockMoveCancelable) + 1u);
				}
				if (action.HasFlag(Channel.BlockedAction.Ability))
				{
					Network_blockAbilityCancelable = (byte)(unchecked((uint)_blockAbilityCancelable) + 1u);
				}
				if (action.HasFlag(Channel.BlockedAction.Attack))
				{
					Network_blockAttackCancelable = (byte)(unchecked((uint)_blockAttackCancelable) + 1u);
				}
			}
			else
			{
				if (action.HasFlag(Channel.BlockedAction.Move))
				{
					Network_blockMove = (byte)(unchecked((uint)_blockMove) + 1u);
				}
				if (action.HasFlag(Channel.BlockedAction.Ability))
				{
					Network_blockAbility = (byte)(unchecked((uint)_blockAbility) + 1u);
				}
				if (action.HasFlag(Channel.BlockedAction.Attack))
				{
					Network_blockAttack = (byte)(unchecked((uint)_blockAttack) + 1u);
				}
			}
		}
	}

	[Server]
	private void DecrementBlockCounters(Channel channel, bool delayOneFrame = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::DecrementBlockCounters(Channel,System.Boolean)' called when server was not active");
		}
		else if (channel.uncancellableTime <= 0f || !channel.blockedActions.HasFlag(Channel.BlockedAction.Cancelable))
		{
			DecrementBlockCounters(channel.blockedActions, delayOneFrame);
		}
		else if (channel.elapsedTime < channel.uncancellableTime)
		{
			DecrementBlockCounters(channel.blockedActions & ~Channel.BlockedAction.Cancelable, delayOneFrame);
		}
		else
		{
			DecrementBlockCounters(channel.blockedActions, delayOneFrame);
		}
	}

	[Server]
	public void DecrementBlockCounters(Channel.BlockedAction action, bool delayOneFrame = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::DecrementBlockCounters(Channel/BlockedAction,System.Boolean)' called when server was not active");
		}
		else
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			checked
			{
				if (delayOneFrame)
				{
					if (action.HasFlag(Channel.BlockedAction.Cancelable))
					{
						if (action.HasFlag(Channel.BlockedAction.Move))
						{
							Network_blockMoveCancelable = (byte)(unchecked((uint)_blockMoveCancelable) - 1u);
							Network_blockMove = (byte)(unchecked((uint)_blockMove) + 1u);
						}
						if (action.HasFlag(Channel.BlockedAction.Ability))
						{
							Network_blockAbilityCancelable = (byte)(unchecked((uint)_blockAbilityCancelable) - 1u);
							Network_blockAbility = (byte)(unchecked((uint)_blockAbility) + 1u);
						}
						if (action.HasFlag(Channel.BlockedAction.Attack))
						{
							Network_blockAttackCancelable = (byte)(unchecked((uint)_blockAttackCancelable) - 1u);
							Network_blockAttack = (byte)(unchecked((uint)_blockAttack) + 1u);
						}
						action &= ~Channel.BlockedAction.Cancelable;
					}
					yield return null;
				}
				if (action.HasFlag(Channel.BlockedAction.Cancelable))
				{
					if (action.HasFlag(Channel.BlockedAction.Move))
					{
						Network_blockMoveCancelable = (byte)(unchecked((uint)_blockMoveCancelable) - 1u);
					}
					if (action.HasFlag(Channel.BlockedAction.Ability))
					{
						Network_blockAbilityCancelable = (byte)(unchecked((uint)_blockAbilityCancelable) - 1u);
					}
					if (action.HasFlag(Channel.BlockedAction.Attack))
					{
						Network_blockAttackCancelable = (byte)(unchecked((uint)_blockAttackCancelable) - 1u);
					}
				}
				else
				{
					if (action.HasFlag(Channel.BlockedAction.Move))
					{
						Network_blockMove = (byte)(unchecked((uint)_blockMove) - 1u);
					}
					if (action.HasFlag(Channel.BlockedAction.Ability))
					{
						Network_blockAbility = (byte)(unchecked((uint)_blockAbility) - 1u);
					}
					if (action.HasFlag(Channel.BlockedAction.Attack))
					{
						Network_blockAttack = (byte)(unchecked((uint)_blockAttack) - 1u);
					}
				}
			}
		}
	}

	public BlockStatus IsActionBlocked(BlockableAction action)
	{
		switch (action)
		{
		case BlockableAction.Move:
			if (_blockMove > 0)
			{
				return BlockStatus.Blocked;
			}
			if (_blockMoveCancelable > 0)
			{
				return BlockStatus.BlockedCancelable;
			}
			return BlockStatus.Allowed;
		case BlockableAction.Ability:
			if (_blockAbility > 0)
			{
				return BlockStatus.Blocked;
			}
			if (_blockAbilityCancelable > 0)
			{
				return BlockStatus.BlockedCancelable;
			}
			return BlockStatus.Allowed;
		case BlockableAction.Attack:
			if (_blockAttack > 0)
			{
				return BlockStatus.Blocked;
			}
			if (_blockAttackCancelable > 0)
			{
				return BlockStatus.BlockedCancelable;
			}
			return BlockStatus.Allowed;
		default:
			throw new Exception($"Unknown action type provided for block check: {action}");
		}
	}

	[Command]
	private void CmdDisobeyBlock(BlockableAction action)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EntityControl_002FBlockableAction(writer, action);
		SendCommandInternal("System.Void EntityControl::CmdDisobeyBlock(EntityControl/BlockableAction)", -1524326419, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void DisobeyBlock(BlockableAction action)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::DisobeyBlock(EntityControl/BlockableAction)' called when server was not active");
			return;
		}
		switch (action)
		{
		case BlockableAction.Move:
			if (_blockMove > 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not cancelable!");
			}
			if (_blockMoveCancelable == 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not prohibited!");
			}
			KillChannels(Channel.BlockedAction.Move);
			if (_blockMoveCancelable > 0)
			{
				throw new Exception($"Disobeyed {action} block, but it's not lifted by the channels!");
			}
			break;
		case BlockableAction.Ability:
			if (_blockAbility > 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not cancelable!");
			}
			if (_blockAbilityCancelable == 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not prohibited!");
			}
			KillChannels(Channel.BlockedAction.Ability);
			if (_blockAbilityCancelable > 0)
			{
				throw new Exception($"Disobeyed {action} block, but it's not lifted by the channels!");
			}
			break;
		case BlockableAction.Attack:
			if (_blockAttack > 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not cancelable!");
			}
			if (_blockAttackCancelable == 0)
			{
				throw new Exception($"Cannot disobey {action} block, it's not prohibited!");
			}
			KillChannels(Channel.BlockedAction.Attack);
			if (_blockAttackCancelable > 0)
			{
				throw new Exception($"Disobeyed {action} block, but it's not lifted by the channels!");
			}
			break;
		default:
			throw new Exception($"Unknown action type provided for block disobeying: {action}");
		}
		void KillChannels(Channel.BlockedAction block)
		{
			for (int i = 0; i < ongoingChannels.Count; i++)
			{
				Channel channel = ongoingChannels[i];
				if (channel.isAlive && channel.blockedActions.HasFlag(block))
				{
					channel.isAlive = false;
					channel.onCancel?.Invoke();
					DecrementBlockCounters(channel, delayOneFrame: false);
				}
			}
		}
	}

	private void DoDisplacement()
	{
		if (!base.entity.isActive || (!ongoingDisplacement.isFriendly && (base.entity.Status.hasInvulnerable || base.entity.Status.hasUnstoppable)))
		{
			CancelOngoingDisplacementLocal();
			return;
		}
		if (!Dew.IsOkay(ongoingDisplacement))
		{
			CancelOngoingDisplacementLocal();
			return;
		}
		float multiplier = (ongoingDisplacement.affectedByMovementSpeed ? base.entity.Status.movementSpeedMultiplier : 1f);
		if (ongoingDisplacement is DispByDestination dd)
		{
			if (dd.duration < 0.0001f)
			{
				dd.isAlive = false;
				ongoingDisplacement = null;
				_dispData = default(OngoingDisplacementInternalData);
				try
				{
					dd.onFinish?.Invoke();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				try
				{
					ClientEvent_OnDisplacementFinished?.Invoke(dd);
					return;
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
					return;
				}
			}
			if (_dispData.knownEase != dd.ease || _dispData.easeFunction == null)
			{
				_dispData.easeFunction = EasingFunction.GetEasingFunction(dd.ease);
				_dispData.knownEase = dd.ease;
				_dispData.start = base.transform.position;
				_dispData.last = base.transform.position;
			}
			dd.elapsedTime += Time.deltaTime;
			Vector3 newPos = Vector3.Lerp(_dispData.start, dd.destination, _dispData.easeFunction(0f, 1f, dd.elapsedTime / dd.duration));
			Vector3 delta = newPos - _dispData.last;
			if (Math.Abs(multiplier - 1f) > 0.001f)
			{
				dd.destination += delta * (multiplier - 1f);
				delta *= multiplier;
				newPos = _dispData.last + delta;
			}
			_dispData.last = newPos;
			Dew.FilterNonOkayValues(ref delta);
			if (_agent.isOnNavMesh)
			{
				_agent.Move(delta);
				base.transform.position = _agent.nextPosition;
			}
			else
			{
				base.transform.position += delta;
			}
			if (dd.rotateForward)
			{
				SetRotation(delta, dd.rotateSmoothly);
			}
			else
			{
				DoRotateTowardsDesiredAngleTick(desiredAngle);
			}
			if (dd.elapsedTime > dd.duration)
			{
				dd.isAlive = false;
				ongoingDisplacement = null;
				_dispData = default(OngoingDisplacementInternalData);
				try
				{
					dd.onFinish?.Invoke();
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
				try
				{
					ClientEvent_OnDisplacementFinished?.Invoke(dd);
				}
				catch (Exception exception4)
				{
					Debug.LogException(exception4);
				}
			}
		}
		else
		{
			if (!(ongoingDisplacement is DispByTarget dt))
			{
				return;
			}
			Vector3 targetPos = dt.target.transform.position;
			Vector3 trPos = Dew.GetPositionOnGround(base.transform.position);
			Vector3 dispDest = targetPos + (dt.target.Control.outerRadius + outerRadius + dt.goalDistance) * (trPos - targetPos).normalized;
			Vector3 newPos2 = Vector3.MoveTowards(trPos, dispDest, Time.deltaTime * dt.speed * multiplier);
			Dew.FilterNonOkayValues(ref newPos2);
			base.transform.position = newPos2;
			_agent.nextPosition = newPos2;
			dt.elapsedTime += Time.deltaTime;
			if (dt.rotateForward)
			{
				SetRotation(targetPos - base.transform.position, dt.rotateSmoothly);
			}
			if (Vector2.Distance(base.transform.position.ToXY(), dispDest.ToXY()) < 0.05f)
			{
				if (Dew.GetNavMeshPathStatus(dt.target.position, base.transform.position) != NavMeshPathStatus.PathComplete)
				{
					Teleport(Dew.GetValidAgentDestination_Closest(dt.target.agentPosition, agentPosition));
				}
				dt.isAlive = false;
				ongoingDisplacement = null;
				try
				{
					dt.onFinish?.Invoke();
				}
				catch (Exception exception5)
				{
					Debug.LogException(exception5);
				}
				try
				{
					ClientEvent_OnDisplacementFinished?.Invoke(dt);
					return;
				}
				catch (Exception exception6)
				{
					Debug.LogException(exception6);
					return;
				}
			}
			if (dt.elapsedTime > dt.cancelTime)
			{
				if (base.isServer && Dew.GetNavMeshPathStatus(dt.target.position, base.transform.position) != NavMeshPathStatus.PathComplete)
				{
					Teleport(Dew.GetValidAgentDestination_Closest(dt.target.agentPosition, agentPosition));
				}
				dt.onCancel?.Invoke();
				dt.isAlive = false;
				ongoingDisplacement = null;
				ClientEvent_OnDisplacementFinished?.Invoke(dt);
			}
		}
		void SetRotation(Vector3 forward, bool smooth)
		{
			forward.Flatten();
			if (!(forward.sqrMagnitude < 0.01f))
			{
				if (isLocalMovementProcessor)
				{
					_localDesiredAngle = CastInfo.GetAngle(forward);
				}
				if (smooth)
				{
					DoRotateTowardsDesiredAngleTick(CastInfo.GetAngle(forward));
				}
				else
				{
					base.transform.rotation = Quaternion.LookRotation(forward);
				}
			}
		}
	}

	private bool ShouldDisplacementDisableAgent()
	{
		if (ongoingDisplacement == null)
		{
			return false;
		}
		if (ongoingDisplacement is DispByDestination dest)
		{
			return dest.canGoOverTerrain;
		}
		if (ongoingDisplacement is DispByTarget)
		{
			return true;
		}
		return false;
	}

	[Server]
	public void Stop()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Stop()' called when server was not active");
			return;
		}
		ClearMovement();
		ClearActionQueue();
		if (isLocalMovementProcessor)
		{
			StopMovementProcessor();
		}
		else
		{
			TpcStopMovementProcessor();
		}
	}

	public void CmdStop()
	{
		if (HasAuthorityWithLog())
		{
			CmdClearMovement();
			if (isLocalMovementProcessor)
			{
				StopMovementProcessor();
			}
			else
			{
				CmdStopMovementProcessor();
			}
			CmdClearActionQueue();
		}
	}

	[Command]
	private void CmdClearActionQueue()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void EntityControl::CmdClearActionQueue()", 462041075, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void ClearActionQueue()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::ClearActionQueue()' called when server was not active");
			return;
		}
		_queuedActions.Clear();
		_moveToActionOwner = null;
	}

	[TargetRpc]
	private void TpcStopMovementProcessor()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(null, "System.Void EntityControl::TpcStopMovementProcessor()", -1383202127, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	private void CmdStopMovementProcessor()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void EntityControl::CmdStopMovementProcessor()", -40884866, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void StopMovementProcessor()
	{
		_desiredAgentDestination = null;
		if (_agent.hasPath)
		{
			_agent.ResetPath();
		}
		StopDoingMoveToAction();
		if (base.isServer)
		{
			_isMoveToAttackActive = false;
			Attack(null, doChase: false);
		}
		else
		{
			CmdCancelMoveToAttack();
			CmdAttack(null, doChase: false);
		}
	}

	[Server]
	public void Interact(IInteractable interactable, bool isAlt, bool isMouse)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::Interact(IInteractable,System.Boolean,System.Boolean)' called when server was not active");
			return;
		}
		if (interactable is Shrine_Stardust)
		{
			_lastStardustInteractTime = Time.time;
		}
		else if (Time.time - _lastStardustInteractTime < 0.5f)
		{
			_lastStardustInteractTime = Time.time;
			return;
		}
		AddAction(new ActionInteract
		{
			interactable = interactable,
			isAllowedToMove = true,
			isAlt = isAlt,
			noActivation = (isMouse && !interactable.canInteractWithMouse)
		});
	}

	[Command]
	public void CmdInteract(IInteractable interactable, bool isAlt, bool isMouse)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteIInteractable(interactable);
		writer.WriteBool(isAlt);
		writer.WriteBool(isMouse);
		SendCommandInternal("System.Void EntityControl::CmdInteract(IInteractable,System.Boolean,System.Boolean)", -1209342242, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private float FilterWalkStrength(float original)
	{
		if (forceWalking)
		{
			return 1f;
		}
		if (IsActionBlocked(BlockableAction.Move) != BlockStatus.Allowed || currentMaxAgentSpeed < 0.1f || base.entity.Status.hasRoot || base.entity.Status.hasStun || base.entity.Control.isDisplacing)
		{
			return 0f;
		}
		return original;
	}

	private void DoMovementFrameUpdate()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			CancelOngoingDisplacementLocal();
		}
		else if (isLocalMovementProcessor)
		{
			DoMovementProcessorFrameUpdate();
		}
		else
		{
			DoMovementObserverFrameUpdate();
		}
	}

	private void DoMovementLogicUpdate()
	{
		if (obstacleAvoidance && _agent != null)
		{
			UpdateObstacleAvoidanceAndPriority();
		}
	}

	private void DoMovementProcessorFrameUpdate()
	{
		if (base.entity.Visual.isSpawning)
		{
			return;
		}
		if (isDoingMoveToAction)
		{
			if (Vector2.Distance(_moveToActionDestination.ToXY(), _agent.nextPosition.ToXY()) < _moveToActionRequiredDistance - 0.001f)
			{
				_desiredAgentDestination = null;
			}
			else
			{
				_desiredAgentDestination = _moveToActionDestination;
			}
		}
		bool cannotMove = base.entity.Status.hasRoot || base.entity.Status.hasStun || isDisplacing;
		if (_agent.isOnNavMesh)
		{
			if ((cannotMove || !_desiredAgentDestination.HasValue) && _agent.hasPath)
			{
				_agent.ResetPath();
			}
			if (_desiredAgentDestination.HasValue && Time.time - _lastAgentDestinationTime > 0.25f)
			{
				Vector3 destination = _agent.destination;
				Vector3? desiredAgentDestination = _desiredAgentDestination;
				if (destination != desiredAgentDestination)
				{
					_lastAgentDestinationTime = Time.time;
					_agent.SetDestination(_desiredAgentDestination.Value);
				}
			}
		}
		if (_overridenDesiredAngle.HasValue)
		{
			_localDesiredAngle = _overridenDesiredAngle.Value;
		}
		if (isDisplacing)
		{
			DoDisplacement();
			_localVelocity = default(Vector3);
			UpdatePositionSyncData(new PositionSyncData
			{
				timestamp = NetworkTime.time,
				position = base.transform.position,
				velocity = _localVelocity,
				desiredAngle = _localDesiredAngle
			});
			if (Math.Abs(0f - _localWalkStrength) > 0.001f)
			{
				SetWalkStrength(0f);
			}
			return;
		}
		float newWalkStrength = 0f;
		Vector3 targetVelocity = default(Vector3);
		float speed = currentMaxAgentSpeed;
		if (_movementVector.HasValue && !_isMovementVectorDirection && Vector2.SqrMagnitude(_agent.destination.ToXY() - _agent.nextPosition.ToXY()) < 0.1f)
		{
			_agent.ResetPath();
			_movementVector = null;
			_desiredAgentDestination = null;
		}
		if (IsActionBlocked(BlockableAction.Move) != BlockStatus.Allowed || cannotMove)
		{
			targetVelocity = default(Vector3);
		}
		else if (_agent.hasPath)
		{
			if (_agent.desiredVelocity.sqrMagnitude > 0.01f)
			{
				Vector3 dir = _agent.desiredVelocity.normalized;
				newWalkStrength = _destinationMovementSpeedMultiplier;
				targetVelocity = _destinationMovementSpeedMultiplier * speed * dir;
			}
		}
		else if (_movementVector.HasValue && _isMovementVectorDirection)
		{
			Vector3 vec = _movementVector.Value;
			targetVelocity = speed * vec;
			newWalkStrength = vec.magnitude;
		}
		_agent.velocity = Vector3.zero;
		DoVelocityUpdate(targetVelocity);
		_agent.nextPosition += _localVelocity * Time.deltaTime;
		if (!_overridenDesiredAngle.HasValue && targetVelocity.sqrMagnitude > 0.1f && _localVelocity.sqrMagnitude > 0.1f)
		{
			_localDesiredAngle = CastInfo.GetAngle(_localVelocity);
		}
		DoRotateTowardsDesiredAngleTick(_localDesiredAngle);
		if (Math.Abs(newWalkStrength - _localWalkStrength) > 0.001f)
		{
			if (newWalkStrength == 0f && _localWalkStrength > 0f)
			{
				newWalkStrength = Mathf.MoveTowards(_localWalkStrength, 0f, Time.deltaTime * 20f);
			}
			SetWalkStrength(newWalkStrength);
		}
		Vector3 newPos = _agent.nextPosition;
		if (_agent.isOnNavMesh)
		{
			newPos.y = Dew.GetValidAgentPosition(newPos).y;
		}
		else
		{
			newPos = Dew.GetPositionOnGround(newPos);
		}
		base.transform.position = newPos;
		UpdatePositionSyncData(new PositionSyncData
		{
			timestamp = NetworkTime.time,
			position = newPos,
			velocity = _localVelocity,
			desiredAngle = _localDesiredAngle
		});
	}

	private void DoVelocityUpdate(Vector3 targetVelocity)
	{
		float currMag = _localVelocity.magnitude;
		float currAngle = ((!(currMag > 0.1f)) ? base.transform.rotation.eulerAngles.y : Vector3.SignedAngle(Vector3.forward, _localVelocity, Vector3.up));
		float targetMag = targetVelocity.magnitude;
		float targetAngle = ((!(targetMag > 0.1f)) ? currAngle : Vector3.SignedAngle(Vector3.forward, targetVelocity, Vector3.up));
		float magChangeSpeed = ((base.entity.Status.movementSpeedMultiplier > 0.001f || targetMag > currMag) ? (normalizedAcceleration * currentMaxAgentSpeed) : (Mathf.Clamp(baseAgentSpeed, 0.1f, 1000f) * 5f));
		float nextAngle = Mathf.MoveTowardsAngle(currAngle, targetAngle, rotateSpeed * base.entity.Status.movementSpeedMultiplier * Time.deltaTime);
		float nextRad = Mathf.MoveTowards(currMag, targetMag, magChangeSpeed * Time.deltaTime * ((currMag > targetMag) ? 2f : 1f));
		if (float.IsNaN(nextAngle))
		{
			nextAngle = currAngle;
		}
		if (float.IsNaN(nextRad))
		{
			nextRad = currMag;
		}
		Vector3 nextVelocity = Quaternion.Euler(0f, nextAngle, 0f) * Vector3.forward * nextRad;
		if (nextVelocity.IsNaN())
		{
			nextVelocity = Vector3.zero;
		}
		_localVelocity = nextVelocity;
	}

	private void DoRotateTowardsDesiredAngleTick(float target)
	{
		float y = base.transform.rotation.eulerAngles.y;
		float smoothTime = rotationSmoothTime;
		if (Mathf.Abs(Mathf.DeltaAngle(y, target)) < 60f)
		{
			smoothTime *= 2f;
		}
		float y2 = Mathf.SmoothDampAngle(y, target, ref _desiredAngleCv, smoothTime);
		if (float.IsNaN(y2))
		{
			_desiredAngleCv = 0f;
		}
		else
		{
			base.transform.rotation = Quaternion.Euler(0f, y2, 0f);
		}
	}

	private void SetWalkStrength(float strength)
	{
		if (isLocalMovementProcessor)
		{
			if (base.isServer)
			{
				Network_syncedWalkStrength = strength;
				_localWalkStrength = strength;
			}
			else
			{
				_localWalkStrength = strength;
				CmdSetWalkStrength(strength);
			}
		}
	}

	[Command]
	private void CmdSetWalkStrength(float strength)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(strength);
		SendCommandInternal("System.Void EntityControl::CmdSetWalkStrength(System.Single)", 1641380514, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void MoveToDestination(Vector3 destination, bool immediately, float speedMult = 1f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::MoveToDestination(UnityEngine.Vector3,System.Boolean,System.Single)' called when server was not active");
			return;
		}
		speedMult = Mathf.Clamp01(speedMult);
		if (isLocalMovementProcessor)
		{
			Attack(null, doChase: false);
			MoveToDestination_Imp(destination, immediately, speedMult);
		}
		else
		{
			TpcMoveToDestination(destination, immediately, speedMult);
		}
	}

	[TargetRpc]
	private void TpcMoveToDestination(Vector3 destination, bool immediately, float speedMult)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(destination);
		writer.WriteBool(immediately);
		writer.WriteFloat(speedMult);
		SendTargetRPCInternal(null, "System.Void EntityControl::TpcMoveToDestination(UnityEngine.Vector3,System.Boolean,System.Single)", -95381029, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public void CmdMoveToDestination(Vector3 destination, bool immediately, float speedMult = 1f)
	{
		if (HasAuthorityWithLog())
		{
			MoveToDestination_Imp(destination, immediately, speedMult);
			if (base.entity.Ability.attackAbility != null && base.entity.Ability.attackAbility.currentConfig.channel.duration > 0.0001f)
			{
				CmdAttack(null, doChase: false);
			}
		}
	}

	private void MoveToDestination_Imp(Vector3 destination, bool immediately, float speedMult)
	{
		if (IsActionBlocked(BlockableAction.Move) == BlockStatus.BlockedCancelable)
		{
			if (base.isServer)
			{
				DisobeyBlock(BlockableAction.Move);
			}
			else
			{
				CmdDisobeyBlock(BlockableAction.Move);
			}
		}
		_destinationMovementSpeedMultiplier = speedMult;
		CmdSetMovementState(destination, isDirection: false);
		SetAgentDestination(destination, immediately);
		StopDoingMoveToAction();
		if (base.isServer)
		{
			_isMoveToAttackActive = false;
		}
		else
		{
			CmdCancelMoveToAttack();
		}
	}

	public void CmdMoveWithDirection(Vector3 direction)
	{
		if (HasAuthorityWithLog())
		{
			if (IsActionBlocked(BlockableAction.Move) == BlockStatus.BlockedCancelable)
			{
				CmdDisobeyBlock(BlockableAction.Move);
			}
			CmdSetMovementState(direction, isDirection: true);
			StopDoingMoveToAction();
			CmdCancelMoveToAttack();
		}
	}

	[Server]
	public void ClearMovement()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityControl::ClearMovement()' called when server was not active");
		}
		else if (isLocalMovementProcessor)
		{
			SetMovementStateLocal(null, isDirection: false);
		}
		else
		{
			TpcSetMovementState(null, isDirection: false);
		}
	}

	public void CmdClearMovement()
	{
		if (HasAuthorityWithLog())
		{
			CmdSetMovementState(null, isDirection: false);
		}
	}

	public void SetAgentDestination(Vector3 destination, bool important)
	{
		if (isLocalMovementProcessor)
		{
			SetAgentDestinationLocal(destination, important);
		}
		else
		{
			CmdSetAgentDestination(destination, important);
		}
	}

	private void SetAgentDestinationLocal(Vector3 destination, bool important)
	{
		if (isLocalMovementProcessor && !destination.IsNaN())
		{
			_desiredAgentDestination = destination;
			if (important && _agent.isOnNavMesh)
			{
				_agent.path = Dew.GetNavMeshPath(_agent.nextPosition, destination);
			}
		}
	}

	[Command]
	private void CmdSetAgentDestination(Vector3 destination, bool important)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(destination);
		writer.WriteBool(important);
		SendCommandInternal("System.Void EntityControl::CmdSetAgentDestination(UnityEngine.Vector3,System.Boolean)", -2073988596, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void CmdSetMovementState(Vector3? vector, bool isDirection)
	{
		if (isLocalMovementProcessor)
		{
			SetMovementStateLocal(vector, isDirection);
		}
		else
		{
			CmdSetMovementState_Imp(vector, isDirection);
		}
	}

	[Command]
	private void CmdSetMovementState_Imp(Vector3? vector, bool isDirection)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3Nullable(vector);
		writer.WriteBool(isDirection);
		SendCommandInternal("System.Void EntityControl::CmdSetMovementState_Imp(System.Nullable`1<UnityEngine.Vector3>,System.Boolean)", 477684443, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	private void TpcSetMovementState(Vector3? vector, bool isDirection)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3Nullable(vector);
		writer.WriteBool(isDirection);
		SendTargetRPCInternal(null, "System.Void EntityControl::TpcSetMovementState(System.Nullable`1<UnityEngine.Vector3>,System.Boolean)", 873017755, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void SetMovementStateLocal(Vector3? vector, bool isDirection)
	{
		_movementVector = vector;
		if ((!vector.HasValue || isDirection) && _desiredAgentDestination.HasValue)
		{
			_desiredAgentDestination = null;
		}
		_isMovementVectorDirection = isDirection;
		if (isDirection && _agent.hasPath)
		{
			_agent.ResetPath();
		}
	}

	private void OnMovementSyncDataReceived(PositionSyncData _, PositionSyncData __)
	{
		_positionSyncDataTime = Time.time;
		_positionSyncDataUnscaledTime = Time.unscaledTime;
	}

	private void DoMovementObserverFrameUpdate()
	{
		if (isDisplacing)
		{
			DoDisplacement();
		}
		else
		{
			if (_positionSyncData.timestamp < _positionSyncDataTimestampLowerBound)
			{
				return;
			}
			DoRotateTowardsDesiredAngleTick(_positionSyncData.desiredAngle);
			if (!base.entity.Visual.isSpawning)
			{
				float positionSyncDataUnscaledElapsedTime = _positionSyncDataUnscaledElapsedTime;
				if (positionSyncDataUnscaledElapsedTime > SyncAgentVelocityLifetime)
				{
					_positionSyncData.velocity = default(Vector3);
				}
				Transform t = base.transform;
				Vector3 newClientPos = Dew.GetPositionOnGround(t.position);
				newClientPos += _positionSyncData.velocity * Time.deltaTime;
				if (positionSyncDataUnscaledElapsedTime <= SyncFixSnapshotLifetime)
				{
					Vector3 estimate = _positionSyncData.position + _positionSyncData.velocity * (_positionSyncDataElapsedTime + _positionSyncDataExtrapolateTime * SyncExtrapolateStrength);
					newClientPos = ((!(Vector2.Distance(newClientPos.ToXY(), estimate.ToXY()) > SyncFixWarpDistance)) ? Vector3.SmoothDamp(newClientPos, estimate, ref _fixCv, SyncFixSmoothTime) : estimate);
				}
				if (_agent != null && _agent.isOnNavMesh)
				{
					newClientPos.y = Dew.GetValidAgentPosition(newClientPos).y;
				}
				else
				{
					newClientPos = Dew.GetPositionOnGround(newClientPos);
				}
				t.position = newClientPos;
				if (_agent != null)
				{
					_agent.nextPosition = newClientPos;
				}
			}
		}
	}

	private void UpdatePositionSyncData(PositionSyncData data)
	{
		if (NetworkServer.isLoadingScene || (NetworkClient.active && !NetworkClient.ready))
		{
			return;
		}
		if (isClientSideMovement)
		{
			if (Time.unscaledTime - _lastPositionSyncDataSendTime >= 1f / 60f)
			{
				_lastPositionSyncDataSendTime = Time.unscaledTime;
				CmdPositionSyncData(data);
			}
		}
		else if (!base.isServer)
		{
			Debug.LogWarning("Tried to set sync data without authority");
		}
		else
		{
			Dew.FilterNonOkayValues(ref data.position, base.transform.position);
			Dew.FilterNonOkayValues(ref data.velocity);
			Dew.FilterNonOkayValues(ref data.desiredAngle);
			Network_positionSyncData = data;
		}
	}

	[Command]
	private void CmdPositionSyncData(PositionSyncData data)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EntityControl_002FPositionSyncData(writer, data);
		SendCommandInternal("System.Void EntityControl::CmdPositionSyncData(EntityControl/PositionSyncData)", -1630240464, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void DoSyncStart()
	{
		if (base.isServer)
		{
			Network_positionSyncData = new PositionSyncData
			{
				timestamp = NetworkTime.time,
				desiredAngle = CastInfo.GetAngle(base.transform.forward),
				position = base.entity.position,
				velocity = Vector3.zero
			};
		}
		ClientEvent_OnTeleport += (Action<Vector3, Vector3>)delegate
		{
			_positionSyncDataTimestampLowerBound = NetworkTime.time;
		};
		ClientEvent_OnDisplacementFinished += (Action<Displacement>)delegate
		{
			_positionSyncDataTimestampLowerBound = NetworkTime.time;
		};
		ClientEvent_OnDisplacementCanceled += (Action<Displacement>)delegate
		{
			_positionSyncDataTimestampLowerBound = NetworkTime.time;
		};
	}

	static EntityControl()
	{
		SyncFixWarpDistance = 5f;
		SyncFixSmoothTime = 0.1f;
		SyncFixSnapshotLifetime = 0.5f;
		SyncAgentVelocityLifetime = 1f;
		SyncExtrapolateMaxTime = 0.15f;
		SyncExtrapolateStrength = 1f;
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdCancelOngoingChannels()", InvokeUserCode_CmdCancelOngoingChannels, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdLookInDirection(UnityEngine.Vector3)", InvokeUserCode_CmdLookInDirection__Vector3, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdCast(AbilityTrigger,System.Int32,CastInfo,System.Boolean,System.Boolean)", InvokeUserCode_CmdCast__AbilityTrigger__Int32__CastInfo__Boolean__Boolean, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdStopDoingMoveToAction()", InvokeUserCode_CmdStopDoingMoveToAction, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdAttack(Entity,System.Boolean)", InvokeUserCode_CmdAttack__Entity__Boolean, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdAttackMove(UnityEngine.Vector3,System.Boolean)", InvokeUserCode_CmdAttackMove__Vector3__Boolean, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdCancelMoveToAttack()", InvokeUserCode_CmdCancelMoveToAttack, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdDisobeyBlock(EntityControl/BlockableAction)", InvokeUserCode_CmdDisobeyBlock__BlockableAction, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdClearActionQueue()", InvokeUserCode_CmdClearActionQueue, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdStopMovementProcessor()", InvokeUserCode_CmdStopMovementProcessor, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdInteract(IInteractable,System.Boolean,System.Boolean)", InvokeUserCode_CmdInteract__IInteractable__Boolean__Boolean, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdSetWalkStrength(System.Single)", InvokeUserCode_CmdSetWalkStrength__Single, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdSetAgentDestination(UnityEngine.Vector3,System.Boolean)", InvokeUserCode_CmdSetAgentDestination__Vector3__Boolean, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdSetMovementState_Imp(System.Nullable`1<UnityEngine.Vector3>,System.Boolean)", InvokeUserCode_CmdSetMovementState_Imp__Nullable_00601__Boolean, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(EntityControl), "System.Void EntityControl::CmdPositionSyncData(EntityControl/PositionSyncData)", InvokeUserCode_CmdPositionSyncData__PositionSyncData, requiresAuthority: true);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcOnDeath()", InvokeUserCode_RpcOnDeath);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcStartDisplacement(Displacement)", InvokeUserCode_RpcStartDisplacement__Displacement);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcCancelOngoingDisplacement()", InvokeUserCode_RpcCancelOngoingDisplacement);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcRotateImmediately(System.Single)", InvokeUserCode_RpcRotateImmediately__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::RpcTeleport(UnityEngine.Vector3)", InvokeUserCode_RpcTeleport__Vector3);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcSetDesiredAngle(System.Single)", InvokeUserCode_TpcSetDesiredAngle__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcSetMoveToActionState(System.Boolean)", InvokeUserCode_TpcSetMoveToActionState__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcSetAttackMovSpdDisadvantage(System.Single)", InvokeUserCode_TpcSetAttackMovSpdDisadvantage__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcStopMovementProcessor()", InvokeUserCode_TpcStopMovementProcessor);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcMoveToDestination(UnityEngine.Vector3,System.Boolean,System.Single)", InvokeUserCode_TpcMoveToDestination__Vector3__Boolean__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityControl), "System.Void EntityControl::TpcSetMovementState(System.Nullable`1<UnityEngine.Vector3>,System.Boolean)", InvokeUserCode_TpcSetMovementState__Nullable_00601__Boolean);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcOnDeath()
	{
		Collider col = GetComponent<Collider>();
		if (col != null)
		{
			col.enabled = false;
		}
	}

	protected static void InvokeUserCode_RpcOnDeath(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDeath called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_RpcOnDeath();
		}
	}

	protected void UserCode_RpcStartDisplacement__Displacement(Displacement disp)
	{
		if (!base.isServer)
		{
			StartDisplacementLocal(disp);
		}
	}

	protected static void InvokeUserCode_RpcStartDisplacement__Displacement(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartDisplacement called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_RpcStartDisplacement__Displacement(reader.ReadDisplacement());
		}
	}

	protected void UserCode_RpcCancelOngoingDisplacement()
	{
		if (!base.isServer)
		{
			CancelOngoingDisplacementLocal();
		}
	}

	protected static void InvokeUserCode_RpcCancelOngoingDisplacement(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCancelOngoingDisplacement called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_RpcCancelOngoingDisplacement();
		}
	}

	protected void UserCode_CmdCancelOngoingChannels()
	{
		CancelOngoingChannels();
	}

	protected static void InvokeUserCode_CmdCancelOngoingChannels(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCancelOngoingChannels called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdCancelOngoingChannels();
		}
	}

	protected void UserCode_TpcSetDesiredAngle__Single(float angle)
	{
		_localDesiredAngle = angle;
	}

	protected static void InvokeUserCode_TpcSetDesiredAngle__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetDesiredAngle called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_TpcSetDesiredAngle__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_RpcRotateImmediately__Single(float angle)
	{
		base.transform.rotation = Quaternion.Euler(0f, angle, 0f);
	}

	protected static void InvokeUserCode_RpcRotateImmediately__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRotateImmediately called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_RpcRotateImmediately__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_RpcTeleport__Vector3(Vector3 position)
	{
		if (!base.isServer)
		{
			TeleportLocal(position);
		}
	}

	protected static void InvokeUserCode_RpcTeleport__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTeleport called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_RpcTeleport__Vector3(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdLookInDirection__Vector3(Vector3 dir)
	{
		if (isGamepadRotationLocked || base.entity.IsNullInactiveDeadOrKnockedOut() || base.entity.Control.IsActionBlocked(BlockableAction.Move) != BlockStatus.Allowed || base.entity.Status.hasStun)
		{
			return;
		}
		foreach (Channel ongoingChannel in base.entity.Control.ongoingChannels)
		{
			if (ongoingChannel.isAttack)
			{
				return;
			}
		}
		Rotate(dir, immediately: false, 0.4f);
	}

	protected static void InvokeUserCode_CmdLookInDirection__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdLookInDirection called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdLookInDirection__Vector3(reader.ReadVector3());
		}
	}

	protected void UserCode_TpcSetMoveToActionState__Boolean(bool state)
	{
		SetMoveToActionStateLocal(state);
	}

	protected static void InvokeUserCode_TpcSetMoveToActionState__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetMoveToActionState called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_TpcSetMoveToActionState__Boolean(reader.ReadBool());
		}
	}

	protected void UserCode_CmdCast__AbilityTrigger__Int32__CastInfo__Boolean__Boolean(AbilityTrigger trigger, int configIndex, CastInfo info, bool allowMoveToCast, bool skipRangeCheck)
	{
		Cast(trigger, configIndex, info, allowMoveToCast, skipRangeCheck);
	}

	protected static void InvokeUserCode_CmdCast__AbilityTrigger__Int32__CastInfo__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCast called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdCast__AbilityTrigger__Int32__CastInfo__Boolean__Boolean(reader.ReadNetworkBehaviour<AbilityTrigger>(), reader.ReadInt(), GeneratedNetworkCode._Read_CastInfo(reader), reader.ReadBool(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdStopDoingMoveToAction()
	{
		isDoingMoveToAction = false;
	}

	protected static void InvokeUserCode_CmdStopDoingMoveToAction(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopDoingMoveToAction called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdStopDoingMoveToAction();
		}
	}

	protected void UserCode_CmdAttack__Entity__Boolean(Entity target, bool doChase)
	{
		Attack(target, doChase);
	}

	protected static void InvokeUserCode_CmdAttack__Entity__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAttack called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdAttack__Entity__Boolean(reader.ReadNetworkBehaviour<Entity>(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdAttackMove__Vector3__Boolean(Vector3 destination, bool useDistanceFromDestination)
	{
		AttackMove(destination, useDistanceFromDestination);
	}

	protected static void InvokeUserCode_CmdAttackMove__Vector3__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAttackMove called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdAttackMove__Vector3__Boolean(reader.ReadVector3(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdCancelMoveToAttack()
	{
		_isMoveToAttackActive = false;
	}

	protected static void InvokeUserCode_CmdCancelMoveToAttack(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCancelMoveToAttack called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdCancelMoveToAttack();
		}
	}

	protected void UserCode_TpcSetAttackMovSpdDisadvantage__Single(float channelDuration)
	{
		SetAttackMovSpdDisadvantage_Imp(channelDuration);
	}

	protected static void InvokeUserCode_TpcSetAttackMovSpdDisadvantage__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetAttackMovSpdDisadvantage called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_TpcSetAttackMovSpdDisadvantage__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_CmdDisobeyBlock__BlockableAction(BlockableAction action)
	{
		if (IsActionBlocked(action) == BlockStatus.BlockedCancelable)
		{
			DisobeyBlock(action);
		}
	}

	protected static void InvokeUserCode_CmdDisobeyBlock__BlockableAction(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDisobeyBlock called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdDisobeyBlock__BlockableAction(GeneratedNetworkCode._Read_EntityControl_002FBlockableAction(reader));
		}
	}

	protected void UserCode_CmdClearActionQueue()
	{
		ClearActionQueue();
	}

	protected static void InvokeUserCode_CmdClearActionQueue(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdClearActionQueue called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdClearActionQueue();
		}
	}

	protected void UserCode_TpcStopMovementProcessor()
	{
		StopMovementProcessor();
	}

	protected static void InvokeUserCode_TpcStopMovementProcessor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcStopMovementProcessor called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_TpcStopMovementProcessor();
		}
	}

	protected void UserCode_CmdStopMovementProcessor()
	{
		StopMovementProcessor();
	}

	protected static void InvokeUserCode_CmdStopMovementProcessor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopMovementProcessor called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdStopMovementProcessor();
		}
	}

	protected void UserCode_CmdInteract__IInteractable__Boolean__Boolean(IInteractable interactable, bool isAlt, bool isMouse)
	{
		Interact(interactable, isAlt, isMouse);
	}

	protected static void InvokeUserCode_CmdInteract__IInteractable__Boolean__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdInteract called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdInteract__IInteractable__Boolean__Boolean(reader.ReadIInteractable(), reader.ReadBool(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdSetWalkStrength__Single(float strength)
	{
		Network_syncedWalkStrength = strength;
	}

	protected static void InvokeUserCode_CmdSetWalkStrength__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetWalkStrength called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdSetWalkStrength__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_TpcMoveToDestination__Vector3__Boolean__Single(Vector3 destination, bool immediately, float speedMult)
	{
		CmdMoveToDestination(destination, immediately, speedMult);
	}

	protected static void InvokeUserCode_TpcMoveToDestination__Vector3__Boolean__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcMoveToDestination called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_TpcMoveToDestination__Vector3__Boolean__Single(reader.ReadVector3(), reader.ReadBool(), reader.ReadFloat());
		}
	}

	protected void UserCode_CmdSetAgentDestination__Vector3__Boolean(Vector3 destination, bool important)
	{
		SetAgentDestinationLocal(destination, important);
	}

	protected static void InvokeUserCode_CmdSetAgentDestination__Vector3__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetAgentDestination called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdSetAgentDestination__Vector3__Boolean(reader.ReadVector3(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdSetMovementState_Imp__Nullable_00601__Boolean(Vector3? vector, bool isDirection)
	{
		SetMovementStateLocal(vector, isDirection);
	}

	protected static void InvokeUserCode_CmdSetMovementState_Imp__Nullable_00601__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetMovementState_Imp called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdSetMovementState_Imp__Nullable_00601__Boolean(reader.ReadVector3Nullable(), reader.ReadBool());
		}
	}

	protected void UserCode_TpcSetMovementState__Nullable_00601__Boolean(Vector3? vector, bool isDirection)
	{
		SetMovementStateLocal(vector, isDirection);
	}

	protected static void InvokeUserCode_TpcSetMovementState__Nullable_00601__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetMovementState called on server.");
		}
		else
		{
			((EntityControl)obj).UserCode_TpcSetMovementState__Nullable_00601__Boolean(reader.ReadVector3Nullable(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdPositionSyncData__PositionSyncData(PositionSyncData data)
	{
		if (isClientSideMovement && Dew.IsOkay(data.timestamp))
		{
			Dew.FilterNonOkayValues(ref data.position, base.transform.position);
			Dew.FilterNonOkayValues(ref data.velocity);
			Dew.FilterNonOkayValues(ref data.desiredAngle);
			Network_positionSyncData = data;
		}
	}

	protected static void InvokeUserCode_CmdPositionSyncData__PositionSyncData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPositionSyncData called on client.");
		}
		else
		{
			((EntityControl)obj).UserCode_CmdPositionSyncData__PositionSyncData(GeneratedNetworkCode._Read_EntityControl_002FPositionSyncData(reader));
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(_outerRadius);
			writer.WriteFloat(_innerRadius);
			writer.WriteBool(_freeMovement);
			writer.WriteNullableFloat(_overridenDesiredAngle);
			writer.WriteBool(isControlReversed);
			writer.WriteInt(_gamepadRotationLockCounter);
			writer.WriteVector3(_moveToActionDestination);
			writer.WriteFloat(_moveToActionRequiredDistance);
			NetworkWriterExtensions.WriteByte(writer, _blockMove);
			NetworkWriterExtensions.WriteByte(writer, _blockAbility);
			NetworkWriterExtensions.WriteByte(writer, _blockAttack);
			NetworkWriterExtensions.WriteByte(writer, _blockMoveCancelable);
			NetworkWriterExtensions.WriteByte(writer, _blockAbilityCancelable);
			NetworkWriterExtensions.WriteByte(writer, _blockAttackCancelable);
			writer.WriteBool(forceWalking);
			writer.WriteFloat(_syncedWalkStrength);
			GeneratedNetworkCode._Write_EntityControl_002FPositionSyncData(writer, _positionSyncData);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteFloat(_outerRadius);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteFloat(_innerRadius);
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(_freeMovement);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteNullableFloat(_overridenDesiredAngle);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteBool(isControlReversed);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteInt(_gamepadRotationLockCounter);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteVector3(_moveToActionDestination);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteFloat(_moveToActionRequiredDistance);
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockMove);
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockAbility);
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockAttack);
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockMoveCancelable);
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockAbilityCancelable);
		}
		if ((base.syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, _blockAttackCancelable);
		}
		if ((base.syncVarDirtyBits & 0x4000L) != 0L)
		{
			writer.WriteBool(forceWalking);
		}
		if ((base.syncVarDirtyBits & 0x8000L) != 0L)
		{
			writer.WriteFloat(_syncedWalkStrength);
		}
		if ((base.syncVarDirtyBits & 0x10000L) != 0L)
		{
			GeneratedNetworkCode._Write_EntityControl_002FPositionSyncData(writer, _positionSyncData);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _outerRadius, OuterRadiusChanged, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _innerRadius, InnerRadiusChanged, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _freeMovement, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _overridenDesiredAngle, null, reader.ReadNullableFloat());
			GeneratedSyncVarDeserialize(ref isControlReversed, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _gamepadRotationLockCounter, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref _moveToActionDestination, null, reader.ReadVector3());
			GeneratedSyncVarDeserialize(ref _moveToActionRequiredDistance, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _blockMove, null, NetworkReaderExtensions.ReadByte(reader));
			GeneratedSyncVarDeserialize(ref _blockAbility, null, NetworkReaderExtensions.ReadByte(reader));
			GeneratedSyncVarDeserialize(ref _blockAttack, null, NetworkReaderExtensions.ReadByte(reader));
			GeneratedSyncVarDeserialize(ref _blockMoveCancelable, null, NetworkReaderExtensions.ReadByte(reader));
			GeneratedSyncVarDeserialize(ref _blockAbilityCancelable, null, NetworkReaderExtensions.ReadByte(reader));
			GeneratedSyncVarDeserialize(ref _blockAttackCancelable, null, NetworkReaderExtensions.ReadByte(reader));
			GeneratedSyncVarDeserialize(ref forceWalking, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _syncedWalkStrength, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _positionSyncData, OnMovementSyncDataReceived, GeneratedNetworkCode._Read_EntityControl_002FPositionSyncData(reader));
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _outerRadius, OuterRadiusChanged, reader.ReadFloat());
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _innerRadius, InnerRadiusChanged, reader.ReadFloat());
		}
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _freeMovement, null, reader.ReadBool());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _overridenDesiredAngle, null, reader.ReadNullableFloat());
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isControlReversed, null, reader.ReadBool());
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _gamepadRotationLockCounter, null, reader.ReadInt());
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _moveToActionDestination, null, reader.ReadVector3());
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _moveToActionRequiredDistance, null, reader.ReadFloat());
		}
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _blockMove, null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _blockAbility, null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _blockAttack, null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x800L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _blockMoveCancelable, null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x1000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _blockAbilityCancelable, null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _blockAttackCancelable, null, NetworkReaderExtensions.ReadByte(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref forceWalking, null, reader.ReadBool());
		}
		if ((num & 0x8000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _syncedWalkStrength, null, reader.ReadFloat());
		}
		if ((num & 0x10000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _positionSyncData, OnMovementSyncDataReceived, GeneratedNetworkCode._Read_EntityControl_002FPositionSyncData(reader));
		}
	}
}
