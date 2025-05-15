using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class PickupInstance : AbilityInstance
{
	private static Collider2D[] _colliders;

	[Header("Effects")]
	public GameObject mainEffect;

	public GameObject pickupEffect;

	public GameObject pickupEffectOnHero;

	public float variationStrength = 0.2f;

	[Header("Velocity")]
	public float fixDirectionMaxSpeedRad = 8f;

	public float maxInitialVelocity = 6f;

	public float maxVelocity = 8f;

	public float velocityDeceleration = 12f;

	public float velocityAccelerationMax = 30f;

	public float velocityAccelerationMin = 10f;

	[Header("Behavior")]
	public float pickupDelay = 0.5f;

	public float expirationTime = float.PositiveInfinity;

	public float attractionRange = 6f;

	public float pickupRange = 0.5f;

	public float yPosFromGround = 1f;

	[Header("Timeout-Magnet")]
	public bool enableTimeoutMagnet = true;

	public float timeoutMagnetTime = 4f;

	[SyncVar]
	private Vector2 _initialFlatVelocity;

	private Vector2 _currentFlatVelocity;

	private float _currentYVelocity;

	private float _expirationTimer;

	private float _startTime;

	[SyncVar]
	private int _variationSeed;

	[SyncVar]
	private Hero _target;

	protected NetworkBehaviourSyncVar ____targetNetId;

	public Vector2 Network_initialFlatVelocity
	{
		get
		{
			return _initialFlatVelocity;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _initialFlatVelocity, 32uL, null);
		}
	}

	public int Network_variationSeed
	{
		get
		{
			return _variationSeed;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _variationSeed, 64uL, null);
		}
	}

	public Hero Network_target
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____targetNetId, ref _target);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _target, 128uL, null, ref ____targetNetId);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_initialFlatVelocity = UnityEngine.Random.insideUnitCircle * maxInitialVelocity;
		Network_variationSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (mainEffect != null)
		{
			FxPlay(mainEffect);
		}
		_startTime = Time.time;
		_currentFlatVelocity = _initialFlatVelocity;
		System.Random rand = new System.Random(_variationSeed);
		maxVelocity *= 1f + ((float)rand.NextDouble() * 2f - 1f) * variationStrength;
		velocityDeceleration *= 1f + ((float)rand.NextDouble() * 2f - 1f) * variationStrength;
		velocityAccelerationMax *= 1f + ((float)rand.NextDouble() * 2f - 1f) * variationStrength;
		velocityAccelerationMin *= 1f + ((float)rand.NextDouble() * 2f - 1f) * variationStrength;
		attractionRange *= 1f + ((float)rand.NextDouble() * 2f - 1f) * variationStrength;
		pickupRange *= 1f + ((float)rand.NextDouble() * 2f - 1f) * variationStrength;
		yPosFromGround *= 1f + ((float)rand.NextDouble() * 2f - 1f) * variationStrength;
		timeoutMagnetTime *= 1f + ((float)rand.NextDouble() * 2f - 1f) * variationStrength;
		pickupDelay *= 1f + ((float)rand.NextDouble() * 2f - 1f) * variationStrength;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (mainEffect != null)
		{
			FxStop(mainEffect);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (Network_target != null)
		{
			float d = Vector2.Distance(Network_target.position.ToXY(), base.position.ToXY());
			float velocityAcceleration = Mathf.Lerp(velocityAccelerationMin, velocityAccelerationMax, (d - pickupRange) / (attractionRange - pickupRange));
			Vector2 direction = (Network_target.position - base.position).ToXY().normalized;
			Vector2 currDirection = _currentFlatVelocity.normalized;
			float fixDirectionStrength = Mathf.Clamp(1f - (d - pickupRange) / (attractionRange - pickupRange), 0f, 1f);
			_currentFlatVelocity = Vector3.RotateTowards(currDirection, direction, Time.deltaTime * fixDirectionMaxSpeedRad * fixDirectionStrength, 0f) * _currentFlatVelocity.magnitude;
			_currentFlatVelocity = Vector2.MoveTowards(_currentFlatVelocity, direction * maxVelocity, velocityAcceleration * Time.deltaTime);
		}
		else
		{
			_currentFlatVelocity = Vector2.MoveTowards(_currentFlatVelocity, Vector3.zero, velocityDeceleration * Time.deltaTime);
		}
		Vector3 newPos = base.position + _currentFlatVelocity.ToXZ() * Time.deltaTime + _currentYVelocity * Time.deltaTime * Vector3.up;
		if (Physics.Raycast(base.position + Vector3.up * 3f, Vector3.down, out var result, 6f, LayerMasks.Ground))
		{
			float groundPos = result.point.y + yPosFromGround;
			float targetYPos = float.NegativeInfinity;
			if (Network_target != null)
			{
				targetYPos = Network_target.position.y + yPosFromGround;
			}
			if (newPos.y < groundPos)
			{
				newPos.y = groundPos;
				_currentYVelocity = 0f;
			}
			else if (newPos.y < targetYPos)
			{
				newPos.y = Mathf.MoveTowards(newPos.y, targetYPos, Time.deltaTime * maxVelocity);
				_currentYVelocity = 0f;
			}
			else
			{
				_currentYVelocity += -18.8f * Time.deltaTime;
			}
		}
		base.position = newPos;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer)
		{
			return;
		}
		if (enableTimeoutMagnet && Time.time - _startTime > timeoutMagnetTime)
		{
			ProcessTimeoutMagnet();
		}
		else
		{
			Hero newTarget = TryFindTarget();
			if (Network_target != null && newTarget == null)
			{
				SyncPosition();
			}
			Network_target = newTarget;
		}
		if (Network_target == null || !CanBeUsedBy(Network_target))
		{
			Network_target = null;
			_expirationTimer += dt;
			if (_expirationTimer > expirationTime)
			{
				Destroy();
			}
		}
		else if (Vector2.Distance(base.position.ToXY(), Network_target.position.ToXY()) < pickupRange && CanBeUsedBy(Network_target))
		{
			OnPickup(Network_target);
			Destroy();
		}
	}

	private void ProcessTimeoutMagnet()
	{
		if (!(Network_target == null))
		{
			return;
		}
		Hero closest = null;
		float closestDist = float.PositiveInfinity;
		foreach (DewPlayer p in DewPlayer.humanPlayers)
		{
			if (!(p.hero == null) && CanBeUsedBy(p.hero))
			{
				float d = Vector2.Distance(base.position.ToXY(), p.hero.position.ToXY());
				if (!(closestDist < d))
				{
					closestDist = d;
					closest = p.hero;
				}
			}
		}
		if (closest != null)
		{
			SyncPosition();
			Network_target = closest;
		}
	}

	private Hero TryFindTarget()
	{
		int count = Physics2D.OverlapCircleNonAlloc(base.position.ToXY(), attractionRange, _colliders, LayerMasks.Entity);
		Hero newTarget = null;
		float targetDist = float.PositiveInfinity;
		for (int i = 0; i < count; i++)
		{
			if (DewPhysics.TryGetEntity(_colliders[i], out var ent) && ent.isActive && ent is Hero hero && CanBeUsedBy(hero))
			{
				float dist = Vector2.Distance(base.position.ToXY(), hero.position.ToXY());
				if (!(dist > attractionRange) && !(dist > targetDist))
				{
					targetDist = dist;
					newTarget = hero;
				}
			}
		}
		return newTarget;
	}

	protected virtual bool CanBeUsedBy(Hero hero)
	{
		if (Time.time - base.creationTime > pickupDelay)
		{
			return !hero.IsNullInactiveDeadOrKnockedOut();
		}
		return false;
	}

	protected virtual void OnPickup(Hero hero)
	{
		if (pickupEffect != null)
		{
			FxPlayNetworked(pickupEffect);
		}
		if (pickupEffectOnHero != null)
		{
			FxPlayNetworked(pickupEffectOnHero, hero);
		}
	}

	[Server]
	private void SyncPosition()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupInstance::SyncPosition()' called when server was not active");
		}
		else
		{
			RpcSyncPosition(base.position, _currentFlatVelocity);
		}
	}

	[ClientRpc]
	private void RpcSyncPosition(Vector3 pos, Vector2 vel)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(pos);
		writer.WriteVector2(vel);
		SendRPCInternal("System.Void PickupInstance::RpcSyncPosition(UnityEngine.Vector3,UnityEngine.Vector2)", 1612814072, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	static PickupInstance()
	{
		_colliders = new Collider2D[10];
		RemoteProcedureCalls.RegisterRpc(typeof(PickupInstance), "System.Void PickupInstance::RpcSyncPosition(UnityEngine.Vector3,UnityEngine.Vector2)", InvokeUserCode_RpcSyncPosition__Vector3__Vector2);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSyncPosition__Vector3__Vector2(Vector3 pos, Vector2 vel)
	{
		base.position = pos;
		_currentFlatVelocity = vel;
	}

	protected static void InvokeUserCode_RpcSyncPosition__Vector3__Vector2(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncPosition called on server.");
		}
		else
		{
			((PickupInstance)obj).UserCode_RpcSyncPosition__Vector3__Vector2(reader.ReadVector3(), reader.ReadVector2());
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVector2(_initialFlatVelocity);
			writer.WriteInt(_variationSeed);
			writer.WriteNetworkBehaviour(Network_target);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteVector2(_initialFlatVelocity);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteInt(_variationSeed);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_target);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _initialFlatVelocity, null, reader.ReadVector2());
			GeneratedSyncVarDeserialize(ref _variationSeed, null, reader.ReadInt());
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _target, null, reader, ref ____targetNetId);
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _initialFlatVelocity, null, reader.ReadVector2());
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _variationSeed, null, reader.ReadInt());
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _target, null, reader, ref ____targetNetId);
		}
	}
}
