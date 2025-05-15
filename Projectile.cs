using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Projectile : AbilityInstance
{
	public enum ProjectileMode
	{
		Point,
		Direction,
		Target
	}

	public enum StartPositionType
	{
		Base,
		Center,
		Muzzle,
		Custom
	}

	public struct EntityHit
	{
		public Entity entity;

		public Vector3 point;
	}

	public enum RotationType
	{
		None,
		Velocity,
		Target
	}

	public enum AddEffectTarget
	{
		Fly,
		Entity,
		Complete,
		Dissipate
	}

	private const float GoalDistanceToTargetWorldPosition = 0.1f;

	[SyncVar]
	public ProjectileMode mode;

	[SyncVar]
	public float endDistance = 8f;

	public StartPositionType start = StartPositionType.Center;

	public float startInFrontDistance = 0.5f;

	public bool endsWithSameHeight = true;

	public float customEndHeight;

	public RotationType rotationType = RotationType.Velocity;

	[FormerlySerializedAs("killOnComplete")]
	public bool destroyOnComplete = true;

	public bool killOnDissipate = true;

	public AnimationCurve verticalOffsetOverTrajectory = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0f), new Keyframe(1f, 0f));

	public bool scaleVerticalOffsetOnLength;

	public AnimationCurve horizontalOffsetOverTrajectory = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0f), new Keyframe(1f, 0f));

	public bool scaleHorizontalOffsetOnLength;

	public float collisionRadius = 0.25f;

	public bool canCollideMidFlight;

	public bool dissipateOnTerrain;

	public AbilityTargetValidator collisionTargets;

	public GameObject effectOnFly;

	public GameObject effectOnEntity;

	public GameObject effectOnComplete;

	public GameObject effectOnDissipate;

	[SyncVar]
	private Entity _targetEntity;

	private Collider _targetEntityCollider;

	private HashSet<Entity> _collidedEntities = new HashSet<Entity>();

	private HashSet<ICollidableWithProjectile> _collidedCollidables = new HashSet<ICollidableWithProjectile>();

	private Vector3 _lastCollisionCheckAnythingPosition;

	private Vector3 _lastCollisionCheckTargetEntityPosition;

	[SyncVar]
	private bool _isCompleted;

	[SyncVar]
	private bool _isFlying;

	[SyncVar]
	private bool _entityMode;

	[SyncVar]
	private Vector3 _targetPosition;

	[SyncVar]
	private Vector3 _startPosition;

	private float _trajectoryLength;

	protected Vector3 _estimatedVelocity;

	private Vector3? _lastKnownTargetPosition;

	private Vector3 _ipEndPos;

	private Quaternion _ipEndRot;

	private float _ipRemainingTime;

	private int _startFrame;

	private static readonly RaycastHit2D[] _raycastHit2DResults;

	private static readonly Comparer<RaycastHit2D> _raycastHit2DComparer;

	protected NetworkBehaviourSyncVar ____targetEntityNetId;

	protected Entity targetEntity => Network_targetEntity;

	public bool isCompleted => _isCompleted;

	public bool isFlying => _isFlying;

	protected Vector3 targetPosition
	{
		get
		{
			return _targetPosition;
		}
		set
		{
			Network_targetPosition = value;
		}
	}

	public ProjectileMode Networkmode
	{
		get
		{
			return mode;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref mode, 32uL, null);
		}
	}

	public float NetworkendDistance
	{
		get
		{
			return endDistance;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref endDistance, 64uL, null);
		}
	}

	public Entity Network_targetEntity
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____targetEntityNetId, ref _targetEntity);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _targetEntity, 128uL, null, ref ____targetEntityNetId);
		}
	}

	public bool Network_isCompleted
	{
		get
		{
			return _isCompleted;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _isCompleted, 256uL, null);
		}
	}

	public bool Network_isFlying
	{
		get
		{
			return _isFlying;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _isFlying, 512uL, null);
		}
	}

	public bool Network_entityMode
	{
		get
		{
			return _entityMode;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _entityMode, 1024uL, null);
		}
	}

	public Vector3 Network_targetPosition
	{
		get
		{
			return _targetPosition;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _targetPosition, 2048uL, null);
		}
	}

	public Vector3 Network_startPosition
	{
		get
		{
			return _startPosition;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _startPosition, 4096uL, null);
		}
	}

	protected abstract Vector3 PositionSolver(float dt);

	protected virtual Quaternion RotationSolver(float dt)
	{
		switch (rotationType)
		{
		case RotationType.None:
			return base.transform.rotation;
		case RotationType.Velocity:
			if (_estimatedVelocity.sqrMagnitude > 0.001f)
			{
				return Quaternion.LookRotation(_estimatedVelocity);
			}
			return base.transform.rotation;
		case RotationType.Target:
		{
			Vector3 forward = GetTargetWorldPosition() - base.transform.position;
			if (forward.sqrMagnitude > 0.001f)
			{
				return Quaternion.LookRotation(forward);
			}
			return base.transform.rotation;
		}
		default:
			throw new Exception($"Couldn't solve rotation for projectile ({this}), unknown RotationType: {rotationType}");
		}
	}

	protected Vector3 CalculateOffsetOverTrajectoryInWorldSpace(float normalizedPosition, Quaternion rotation)
	{
		float v = verticalOffsetOverTrajectory.Evaluate(normalizedPosition);
		float h = horizontalOffsetOverTrajectory.Evaluate(normalizedPosition);
		if (scaleVerticalOffsetOnLength)
		{
			v *= _trajectoryLength;
		}
		if (scaleHorizontalOffsetOnLength)
		{
			h *= _trajectoryLength;
		}
		return Vector3.up * v + rotation * Vector3.right * h;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_lastCollisionCheckAnythingPosition = base.transform.position;
		_lastCollisionCheckTargetEntityPosition = base.transform.position;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_isFlying = true;
		switch (start)
		{
		case StartPositionType.Base:
			Network_startPosition = base.info.caster.Visual.GetBasePosition();
			break;
		case StartPositionType.Center:
			Network_startPosition = base.info.caster.Visual.GetCenterPosition();
			break;
		case StartPositionType.Muzzle:
			Network_startPosition = base.info.caster.Visual.GetMuzzlePosition();
			break;
		case StartPositionType.Custom:
			Network_startPosition = base.position;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		switch (mode)
		{
		case ProjectileMode.Point:
			Network_entityMode = false;
			Network_targetPosition = base.info.point;
			Network_startPosition = _startPosition + (_targetPosition - _startPosition).Flattened().normalized * startInFrontDistance;
			break;
		case ProjectileMode.Direction:
			Network_entityMode = false;
			Network_targetPosition = _startPosition + base.info.forward * endDistance;
			_targetPosition.y = _startPosition.y;
			Network_startPosition = _startPosition + base.info.forward * startInFrontDistance;
			break;
		case ProjectileMode.Target:
			if ((object)base.info.target == null)
			{
				Debug.LogError($"'{this}' is targeted projectile but has null info.target!");
				return;
			}
			Network_entityMode = true;
			Network_startPosition = _startPosition + (base.info.target.position - _startPosition).Flattened().normalized * startInFrontDistance;
			Network_targetEntity = base.info.target;
			Network_targetPosition = base.info.target.position;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (mode != ProjectileMode.Target)
		{
			RaycastHit hit;
			RaycastHit hit2;
			if (endsWithSameHeight)
			{
				_targetPosition.y = _startPosition.y;
			}
			else if (Physics.Raycast(_targetPosition + Vector3.up * 5f, Vector3.down, out hit, 10f, LayerMasks.Ground))
			{
				_targetPosition.y = hit.point.y + customEndHeight;
			}
			else if (Physics.Raycast(_startPosition + Vector3.up * 5f, Vector3.down, out hit2, 10f, LayerMasks.Ground))
			{
				_targetPosition.y = hit2.point.y + customEndHeight;
			}
			else
			{
				Debug.LogWarning($"Failed to find ground position and calculate y-coord for projectile '{this}'");
			}
		}
	}

	private void Interpolate(Vector3 pos, Quaternion rot, float dt)
	{
		_ipEndPos = pos;
		_ipEndRot = rot;
		_ipRemainingTime = dt;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!_isFlying || Time.frameCount <= _startFrame + 1)
		{
			return;
		}
		Vector3 lastPos = base.transform.position;
		Vector3 targetPos = PositionSolver(dt);
		_estimatedVelocity = (targetPos - lastPos) / dt;
		Quaternion targetRot = RotationSolver(dt);
		Interpolate(targetPos, targetRot, dt);
		if (!base.isServer)
		{
			return;
		}
		DoAnythingCollisionChecks(lastPos);
		if (!_isFlying)
		{
			return;
		}
		if (targetEntity != null && targetEntity.isActive)
		{
			DoTargetEntityCollisionCheck(lastPos);
		}
		if (_isFlying && CheckForCompletion())
		{
			if (targetEntity != null && targetEntity.isActive && _collidedEntities.Add(targetEntity))
			{
				EntityHit hit = new EntityHit
				{
					entity = targetEntity,
					point = targetEntity.GetComponent<Collider>().ClosestPoint(base.transform.position)
				};
				OnEntity(hit);
			}
			if (_isFlying)
			{
				Network_isCompleted = true;
				Network_isFlying = false;
				OnComplete();
			}
		}
	}

	protected virtual bool CheckForCompletion()
	{
		return Vector3.Distance(base.transform.position, GetTargetWorldPosition()) < 0.1f;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (base.isActive && _isFlying && _ipRemainingTime != 0f)
		{
			float v = Time.deltaTime / _ipRemainingTime;
			_ipRemainingTime -= Time.deltaTime;
			if (v > 1f)
			{
				_ipRemainingTime = 0f;
				v = 1f;
			}
			base.transform.position = Vector3.Lerp(base.transform.position, _ipEndPos, v);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, _ipEndRot, v);
		}
	}

	protected virtual Vector3 GetTargetWorldPosition()
	{
		if (_entityMode)
		{
			if (targetEntity == null)
			{
				if (_lastKnownTargetPosition.HasValue)
				{
					return _lastKnownTargetPosition.Value;
				}
				if ((object)targetEntity != null)
				{
					_lastKnownTargetPosition = targetEntity.position;
					return _lastKnownTargetPosition.Value;
				}
				Debug.LogWarning(GetActorReadableName() + " is entity mode projectile but has an invalid target or does not have one");
				if (base.isServer)
				{
					Destroy();
				}
				return Dew.GetPositionOnGround(base.transform.position);
			}
			_lastKnownTargetPosition = targetEntity.Visual.GetCenterPosition();
			return _lastKnownTargetPosition.Value;
		}
		return _targetPosition;
	}

	protected override void OnCreate()
	{
		_trajectoryLength = Vector2.Distance(_startPosition.ToXY(), _targetPosition.ToXY());
		base.OnCreate();
		base.position = _startPosition;
		base.position = PositionSolver(0.0001f);
		_estimatedVelocity = (base.position - _startPosition).normalized * 0.1f;
		base.rotation = RotationSolver(0.0001f);
		if (effectOnFly != null)
		{
			FxPlay(effectOnFly);
		}
		_startFrame = Time.frameCount;
		if (rotationType == RotationType.Target)
		{
			Vector3 d = GetTargetWorldPosition() - base.transform.position;
			if (d.sqrMagnitude > 0.01f)
			{
				base.transform.rotation = Quaternion.LookRotation(d);
			}
		}
		else if (rotationType == RotationType.Velocity)
		{
			Vector3 d2 = PositionSolver(0.001f) - base.transform.position;
			if (d2.sqrMagnitude > 0.01f)
			{
				base.transform.rotation = Quaternion.LookRotation(d2);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			Network_isFlying = false;
		}
		if (effectOnFly != null)
		{
			FxStop(effectOnFly);
		}
	}

	[Server]
	private void DoAnythingCollisionChecks(Vector3 nextPosition)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::DoAnythingCollisionChecks(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		Vector3 direction = nextPosition - _lastCollisionCheckAnythingPosition;
		int count = Physics2D.CircleCastNonAlloc(_lastCollisionCheckAnythingPosition.ToXY(), collisionRadius, direction.ToXY(), _raycastHit2DResults, direction.magnitude, LayerMasks.Everything);
		Array.Sort(_raycastHit2DResults, 0, count, _raycastHit2DComparer);
		for (int i = 0; i < count; i++)
		{
			RaycastHit2D hit = _raycastHit2DResults[i];
			if (!base.isActive || !isFlying || !canCollideMidFlight)
			{
				break;
			}
			if (hit.collider.gameObject == base.gameObject)
			{
				continue;
			}
			if (DewPhysics.TryGetEntity(hit.collider, out var entity) && ((object)base.info.caster == null || collisionTargets.Evaluate(base.info.caster, entity)) && !entity.Status.hasUncollidable && _collidedEntities.Add(entity))
			{
				EntityHit entityHit;
				if (hit.point == Vector2.zero && hit.distance == 0f)
				{
					Vector2 point = hit.collider.ClosestPoint(_lastCollisionCheckAnythingPosition.ToXY());
					entityHit = new EntityHit
					{
						entity = entity,
						point = point.ToXZ()
					};
				}
				else
				{
					Vector3 point2 = hit.point.ToXZ();
					point2.y = base.position.y;
					entityHit = new EntityHit
					{
						entity = entity,
						point = point2
					};
				}
				OnEntity(entityHit);
			}
			if (DewPhysics.TryGetCollidableWithProjectile(hit.collider, out var collidable) && _collidedCollidables.Add(collidable))
			{
				CollidableHit collidableHit;
				if (hit.point == Vector2.zero && hit.distance == 0f)
				{
					Vector2 point3 = hit.collider.ClosestPoint(_lastCollisionCheckAnythingPosition.ToXY());
					collidableHit = new CollidableHit
					{
						projectile = this,
						point = point3.ToXZ()
					};
				}
				else
				{
					Vector3 point4 = hit.point.ToXZ();
					point4.y = base.position.y;
					collidableHit = new CollidableHit
					{
						projectile = this,
						point = point4
					};
				}
				collidable.OnProjectileCollision(collidableHit);
			}
		}
		if (dissipateOnTerrain)
		{
			ArrayReturnHandle<RaycastHit> handle;
			RaycastHit[] arr = DewPool.GetArray(out handle, 64);
			if (Physics.RaycastNonAlloc(_lastCollisionCheckAnythingPosition, direction.normalized, arr, direction.magnitude, LayerMasks.Ground) > 0)
			{
				Dissipate();
			}
			handle.Return();
		}
		_lastCollisionCheckAnythingPosition = nextPosition;
	}

	[Server]
	private void DoTargetEntityCollisionCheck(Vector3 checkPosition)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::DoTargetEntityCollisionCheck(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (_targetEntityCollider == null)
		{
			_targetEntityCollider = targetEntity.GetComponentInChildren<Collider>();
		}
		if (_targetEntityCollider == null)
		{
			Debug.LogWarning($"This projectile ({this})'s Target Entity ({targetEntity}) does not have a collider.");
			Destroy();
			return;
		}
		Vector3 point = _targetEntityCollider.ClosestPoint(checkPosition);
		if (Vector3.Distance(point, checkPosition) <= collisionRadius && _collidedEntities.Add(targetEntity))
		{
			EntityHit hit = new EntityHit
			{
				entity = targetEntity,
				point = point
			};
			OnEntity(hit);
		}
		_lastCollisionCheckTargetEntityPosition = checkPosition;
	}

	[Server]
	public void Dissipate()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::Dissipate()' called when server was not active");
			return;
		}
		Network_isFlying = false;
		RpcStopOnFlyEffect();
		OnDissipate();
		if (killOnDissipate)
		{
			Destroy();
		}
	}

	[Server]
	protected void StopFlying()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::StopFlying()' called when server was not active");
			return;
		}
		Network_isFlying = false;
		RpcStopOnFlyEffect();
	}

	protected virtual void OnEntity(EntityHit hit)
	{
		RpcPlayOnEntityEffect(hit);
	}

	[ClientRpc]
	private void RpcPlayOnEntityEffect(EntityHit hit)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_Projectile_002FEntityHit(writer, hit);
		SendRPCInternal("System.Void Projectile::RpcPlayOnEntityEffect(Projectile/EntityHit)", 1988428214, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	protected virtual void OnDissipate()
	{
		RpcPlayOnDissipateEffect();
	}

	[ClientRpc]
	private void RpcPlayOnDissipateEffect()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Projectile::RpcPlayOnDissipateEffect()", 825882477, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	protected virtual void OnComplete()
	{
		RpcPlayOnCompleteEffect();
		RpcStopOnFlyEffect();
		if (destroyOnComplete)
		{
			Destroy();
		}
	}

	[ClientRpc]
	private void RpcStopOnFlyEffect()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Projectile::RpcStopOnFlyEffect()", 86461196, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcPlayOnCompleteEffect()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Projectile::RpcPlayOnCompleteEffect()", -490060138, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void AddEffect(AddEffectTarget target, GameObject eff)
	{
		switch (target)
		{
		case AddEffectTarget.Fly:
		{
			GameObject newEff = AddEffectToTarget(eff, ref effectOnFly);
			if (isFlying)
			{
				FxPlay(newEff);
			}
			break;
		}
		case AddEffectTarget.Entity:
			AddEffectToTarget(eff, ref effectOnEntity);
			break;
		case AddEffectTarget.Complete:
			AddEffectToTarget(eff, ref effectOnComplete);
			break;
		case AddEffectTarget.Dissipate:
			AddEffectToTarget(eff, ref effectOnDissipate);
			break;
		}
	}

	private GameObject AddEffectToTarget(GameObject eff, ref GameObject target)
	{
		GameObject newEff = UnityEngine.Object.Instantiate(eff);
		if (target == null)
		{
			target = newEff;
			newEff.transform.parent = base.transform;
		}
		else
		{
			newEff.transform.parent = target.transform;
		}
		newEff.transform.localPosition = Vector3.zero;
		newEff.transform.localRotation = Quaternion.identity;
		return newEff;
	}

	[Server]
	public void SetCustomStartPosition(Vector3 pos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Projectile::SetCustomStartPosition(UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (start != StartPositionType.Custom)
		{
			Debug.LogWarning($"{this} start position is not set to custom");
			return;
		}
		Network_startPosition = pos;
		base.transform.position = pos;
	}

	static Projectile()
	{
		_raycastHit2DResults = new RaycastHit2D[128];
		_raycastHit2DComparer = Comparer<RaycastHit2D>.Create((RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance));
		RemoteProcedureCalls.RegisterRpc(typeof(Projectile), "System.Void Projectile::RpcPlayOnEntityEffect(Projectile/EntityHit)", InvokeUserCode_RpcPlayOnEntityEffect__EntityHit);
		RemoteProcedureCalls.RegisterRpc(typeof(Projectile), "System.Void Projectile::RpcPlayOnDissipateEffect()", InvokeUserCode_RpcPlayOnDissipateEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(Projectile), "System.Void Projectile::RpcStopOnFlyEffect()", InvokeUserCode_RpcStopOnFlyEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(Projectile), "System.Void Projectile::RpcPlayOnCompleteEffect()", InvokeUserCode_RpcPlayOnCompleteEffect);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayOnEntityEffect__EntityHit(EntityHit hit)
	{
		if (effectOnEntity != null)
		{
			effectOnEntity.transform.SetPositionAndRotation(hit.point, base.transform.rotation);
			FxPlayNew(effectOnEntity, hit.entity);
		}
	}

	protected static void InvokeUserCode_RpcPlayOnEntityEffect__EntityHit(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOnEntityEffect called on server.");
		}
		else
		{
			((Projectile)obj).UserCode_RpcPlayOnEntityEffect__EntityHit(GeneratedNetworkCode._Read_Projectile_002FEntityHit(reader));
		}
	}

	protected void UserCode_RpcPlayOnDissipateEffect()
	{
		if (effectOnDissipate != null)
		{
			FxPlayNew(effectOnDissipate, base.transform.position, base.transform.rotation);
		}
	}

	protected static void InvokeUserCode_RpcPlayOnDissipateEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOnDissipateEffect called on server.");
		}
		else
		{
			((Projectile)obj).UserCode_RpcPlayOnDissipateEffect();
		}
	}

	protected void UserCode_RpcStopOnFlyEffect()
	{
		if (effectOnFly != null)
		{
			FxStop(effectOnFly);
		}
	}

	protected static void InvokeUserCode_RpcStopOnFlyEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopOnFlyEffect called on server.");
		}
		else
		{
			((Projectile)obj).UserCode_RpcStopOnFlyEffect();
		}
	}

	protected void UserCode_RpcPlayOnCompleteEffect()
	{
		if (effectOnComplete != null)
		{
			FxPlayNew(effectOnComplete, base.transform.position, base.transform.rotation);
		}
	}

	protected static void InvokeUserCode_RpcPlayOnCompleteEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOnCompleteEffect called on server.");
		}
		else
		{
			((Projectile)obj).UserCode_RpcPlayOnCompleteEffect();
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_Projectile_002FProjectileMode(writer, mode);
			writer.WriteFloat(endDistance);
			writer.WriteNetworkBehaviour(Network_targetEntity);
			writer.WriteBool(_isCompleted);
			writer.WriteBool(_isFlying);
			writer.WriteBool(_entityMode);
			writer.WriteVector3(_targetPosition);
			writer.WriteVector3(_startPosition);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			GeneratedNetworkCode._Write_Projectile_002FProjectileMode(writer, mode);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteFloat(endDistance);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_targetEntity);
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteBool(_isCompleted);
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteBool(_isFlying);
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteBool(_entityMode);
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteVector3(_targetPosition);
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			writer.WriteVector3(_startPosition);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref mode, null, GeneratedNetworkCode._Read_Projectile_002FProjectileMode(reader));
			GeneratedSyncVarDeserialize(ref endDistance, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _targetEntity, null, reader, ref ____targetEntityNetId);
			GeneratedSyncVarDeserialize(ref _isCompleted, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _isFlying, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _entityMode, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _targetPosition, null, reader.ReadVector3());
			GeneratedSyncVarDeserialize(ref _startPosition, null, reader.ReadVector3());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref mode, null, GeneratedNetworkCode._Read_Projectile_002FProjectileMode(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref endDistance, null, reader.ReadFloat());
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _targetEntity, null, reader, ref ____targetEntityNetId);
		}
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _isCompleted, null, reader.ReadBool());
		}
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _isFlying, null, reader.ReadBool());
		}
		if ((num & 0x400L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _entityMode, null, reader.ReadBool());
		}
		if ((num & 0x800L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _targetPosition, null, reader.ReadVector3());
		}
		if ((num & 0x1000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _startPosition, null, reader.ReadVector3());
		}
	}
}
