using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public abstract class StandardProjectile : Projectile
{
	public bool use3DVelocity;

	[SyncVar]
	public float _initialSpeed = 5f;

	[SyncVar]
	public float _targetSpeed = 5f;

	[SyncVar]
	public float _acceleration;

	public bool enableRandomCurve;

	public float randomCurveMagnitude = 3f;

	public bool ensurePositiveY = true;

	private float _ipNormalizedPositionTarget;

	private float _ipNormalizedPositionRemainingTime;

	private bool _isRefPosSet;

	private Vector3 _referencePosition;

	private float _referenceFlatVelocity;

	private Vector3 _referenceVelocity;

	private Vector3 _randomCurveVec;

	public float initialSpeed
	{
		get
		{
			return _initialSpeed;
		}
		set
		{
			Network_initialSpeed = value;
		}
	}

	public float targetSpeed
	{
		get
		{
			return _targetSpeed;
		}
		set
		{
			Network_targetSpeed = value;
		}
	}

	public float acceleration
	{
		get
		{
			return _acceleration;
		}
		set
		{
			Network_acceleration = value;
		}
	}

	public float normalizedPosition { get; private set; }

	public float Network_initialSpeed
	{
		get
		{
			return _initialSpeed;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _initialSpeed, 8192uL, null);
		}
	}

	public float Network_targetSpeed
	{
		get
		{
			return _targetSpeed;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _targetSpeed, 16384uL, null);
		}
	}

	public float Network_acceleration
	{
		get
		{
			return _acceleration;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _acceleration, 32768uL, null);
		}
	}

	protected override void OnCreate()
	{
		if (enableRandomCurve)
		{
			_randomCurveVec = UnityEngine.Random.insideUnitSphere * randomCurveMagnitude;
			if (ensurePositiveY && _randomCurveVec.y < 0f)
			{
				_randomCurveVec.y *= -1f;
			}
		}
		if (use3DVelocity)
		{
			_referenceVelocity = (GetTargetWorldPosition() - _referencePosition).normalized * _initialSpeed;
		}
		else
		{
			_referenceFlatVelocity = _initialSpeed;
		}
		base.OnCreate();
	}

	protected override bool CheckForCompletion()
	{
		return normalizedPosition > 0.99999f;
	}

	protected override Vector3 PositionSolver(float dt)
	{
		Vector3 targetPos = GetTargetWorldPosition();
		if (!_isRefPosSet)
		{
			_isRefPosSet = true;
			_referencePosition = base.transform.position;
		}
		Vector3 result;
		if (use3DVelocity)
		{
			float beforeDistance = Vector3.Distance(_referencePosition, targetPos);
			if (Vector3.Distance(_referencePosition, targetPos) < _referenceVelocity.magnitude * dt && Vector3.Angle(_referenceVelocity, targetPos - _referencePosition) < 60f)
			{
				_referencePosition = targetPos;
			}
			else
			{
				_referencePosition += _referenceVelocity * dt;
			}
			float afterDistance = Vector3.Distance(_referencePosition, targetPos);
			_referenceVelocity = Vector3.MoveTowards(_referenceVelocity, (targetPos - _referencePosition).normalized * _targetSpeed, _acceleration * dt);
			UpdateNormalizedTrajectory(beforeDistance, afterDistance, dt);
			result = _referencePosition + CalculateOffsetOverTrajectoryInWorldSpace(normalizedPosition, Quaternion.LookRotation(_referenceVelocity));
		}
		else
		{
			float beforeDistance = Vector3.Distance(_referencePosition, targetPos);
			_referencePosition = Vector3.MoveTowards(_referencePosition, targetPos, _referenceFlatVelocity * dt);
			float afterDistance = Vector3.Distance(_referencePosition, targetPos);
			_referenceFlatVelocity = Mathf.MoveTowards(_referenceFlatVelocity, _targetSpeed, _acceleration * dt);
			UpdateNormalizedTrajectory(beforeDistance, afterDistance, dt);
			result = ((!(Vector3.SqrMagnitude(targetPos - base.transform.position) < 1E-05f)) ? (_referencePosition + CalculateOffsetOverTrajectoryInWorldSpace(normalizedPosition, Quaternion.LookRotation(targetPos - base.transform.position))) : (_referencePosition + CalculateOffsetOverTrajectoryInWorldSpace(normalizedPosition, base.transform.rotation)));
		}
		if (enableRandomCurve)
		{
			result += Mathf.Sin(normalizedPosition * MathF.PI) * _randomCurveVec;
		}
		return result;
	}

	private void UpdateNormalizedTrajectory(float beforeDistance, float afterDistance, float dt)
	{
		if (beforeDistance != 0f)
		{
			float newPosition = normalizedPosition + (1f - normalizedPosition) * (beforeDistance - afterDistance) / beforeDistance;
			if (!(newPosition < normalizedPosition))
			{
				newPosition = Mathf.Clamp(newPosition, 0f, 1f);
				_ipNormalizedPositionTarget = newPosition;
				_ipNormalizedPositionRemainingTime = dt;
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (_ipNormalizedPositionRemainingTime != 0f)
		{
			float v = Time.deltaTime / _ipNormalizedPositionRemainingTime;
			_ipNormalizedPositionRemainingTime -= Time.deltaTime;
			if (v > 1f)
			{
				_ipNormalizedPositionRemainingTime = 0f;
				v = 1f;
			}
			normalizedPosition = Mathf.Lerp(normalizedPosition, _ipNormalizedPositionTarget, v);
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
			writer.WriteFloat(_initialSpeed);
			writer.WriteFloat(_targetSpeed);
			writer.WriteFloat(_acceleration);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x2000L) != 0L)
		{
			writer.WriteFloat(_initialSpeed);
		}
		if ((base.syncVarDirtyBits & 0x4000L) != 0L)
		{
			writer.WriteFloat(_targetSpeed);
		}
		if ((base.syncVarDirtyBits & 0x8000L) != 0L)
		{
			writer.WriteFloat(_acceleration);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _initialSpeed, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _targetSpeed, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _acceleration, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x2000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _initialSpeed, null, reader.ReadFloat());
		}
		if ((num & 0x4000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _targetSpeed, null, reader.ReadFloat());
		}
		if ((num & 0x8000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _acceleration, null, reader.ReadFloat());
		}
	}
}
