using System;
using System.Buffers;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

[LogicUpdatePriority(-301)]
public class EntityAI : EntityComponent
{
	public CustomAIBehaviors customBehaviors;

	public static bool DisableAI = false;

	public const float DefaultDetectionRange = 15f;

	public const float BossDetectionRange = 25f;

	public const float AggroPropagationRange = 17f;

	public const float AggroPropagationDelayMin = 0.1f;

	public const float AggroPropagationDelayMax = 0.7f;

	public const float IdleAITickInterval = 1f;

	public const float CombatAITickInterval = 0.5f;

	public const float BossAITickInterval = 0.25f;

	public const float LoseTargetedEnemyTime = 5f;

	public const float RetargetingMinTime = 1f;

	public const float RetargetingMaxTime = 4f;

	public const float RetargetingDistanceFuzziness = 0.3f;

	public const int AITickCongestionThreshold = 5000;

	public const float WanderSpeedMultiplier = 0.6f;

	public const float WanderIntervalMin = 4f;

	public const float WanderIntervalMax = 15f;

	public static int PositionSampleCount = 4;

	public static int PositionSampleLagBehindFrames = 2;

	public static float PositionSampleInterval = 0.1f;

	private static int _numOfAi = 0;

	private static float _congestionSkipChance = 0f;

	private Func<Entity, bool> _targetEnemyValidator;

	private Func<Entity, bool> _allyValidator;

	public bool disableAI;

	public bool excludeFromAutoTargeting;

	[NonSerialized]
	public Func<float> predictionStrengthOverride;

	private bool _isAITicking;

	internal EntityAIContext _aiContext;

	private Vector3[] _positionSamples;

	private int _lastSampleIndex;

	private float _lastSampleTime;

	private float _nextAiUpdateTime;

	private float _lastAiUpdateTime;

	private float _nextWanderTime;

	public bool isAITicking
	{
		get
		{
			return _isAITicking;
		}
		set
		{
			if (_isAITicking != value)
			{
				_isAITicking = value;
				if (value)
				{
					_numOfAi++;
				}
				else
				{
					_numOfAi--;
				}
				if (_numOfAi > 5000)
				{
					_congestionSkipChance = (float)(_numOfAi - 5000) / (float)_numOfAi;
				}
				else
				{
					_congestionSkipChance = 0f;
				}
			}
		}
	}

	public float detectionRange { get; private set; }

	public EntityAIContext context => _aiContext;

	public Vector3 estimatedVelocity { get; private set; }

	public Vector3 estimatedVelocityUnclamped { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_numOfAi = 0;
		_congestionSkipChance = 0f;
	}

	private float GetAIInterval()
	{
		if (base.entity is Summon s && s.info.caster is Hero)
		{
			return 0.25f;
		}
		if (base.entity is Monster { type: var type } && (type == Monster.MonsterType.MiniBoss || type == Monster.MonsterType.Boss))
		{
			return 0.25f;
		}
		if (!(_aiContext.targetEnemy == null))
		{
			return 0.5f;
		}
		return 1f;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		customBehaviors = new CustomAIBehaviors();
		float interval = GetAIInterval();
		_nextAiUpdateTime = Time.time + UnityEngine.Random.value * interval * 0.5f;
		_lastAiUpdateTime = _nextAiUpdateTime - interval * 0.5f;
		_targetEnemyValidator = delegate(Entity e)
		{
			if (e.Status.hasInvisible)
			{
				return false;
			}
			return (base.entity.GetRelation(e) == EntityRelation.Enemy && e.isActive && e.Status.isAlive && !(e is Hero { isKnockedOut: not false }) && (_aiContext.targetEnemy == e || Dew.GetNavMeshPathStatus(base.entity.agentPosition, e.agentPosition) == NavMeshPathStatus.PathComplete)) ? true : false;
		};
		_allyValidator = (Entity e) => (base.entity.GetRelation(e) == EntityRelation.Ally && e.isActive && e.Status.isAlive) ? true : false;
		_positionSamples = ArrayPool<Vector3>.Shared.Rent(PositionSampleCount);
		for (int i = 0; i < PositionSampleCount; i++)
		{
			_positionSamples[i] = base.transform.position;
		}
		_lastSampleTime = Time.time;
		detectionRange = ((base.entity is Monster { type: Monster.MonsterType.Boss }) ? 25f : 15f);
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> readOnlySpan = DewPhysics.OverlapCircleAllEntities(out handle, base.transform.position, 17f, _allyValidator, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		for (int num = 0; num < readOnlySpan.Length; num++)
		{
			Entity a = readOnlySpan[num];
			Entity te = a.AI._aiContext.targetEnemy;
			if (!te.IsNullInactiveDeadOrKnockedOut() && base.entity.GetRelation(te) == EntityRelation.Enemy && Dew.GetNavMeshPathStatus(base.entity.agentPosition, a.agentPosition) == NavMeshPathStatus.PathComplete)
			{
				Aggro(te);
				break;
			}
		}
		handle.Return();
		base.entity.EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage dmg)
		{
			if (!(_aiContext.targetEnemy != null))
			{
				Entity firstEntity = dmg.actor.firstEntity;
				if (!(firstEntity == null) && base.entity.GetRelation(firstEntity) == EntityRelation.Enemy)
				{
					Aggro(firstEntity, doPropagation: true);
				}
			}
		};
	}

	public void CallAIUpdateImmediately()
	{
		float aiDeltaTime = Time.time - _lastAiUpdateTime;
		UpdateAIContext(aiDeltaTime);
		_aiContext._insideAIUpdate = true;
		base.entity.CallAIUpdate(ref _aiContext);
		_aiContext._insideAIUpdate = false;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.isServer || base.entity.isSleeping)
		{
			return;
		}
		isAITicking = base.entity.isActive && !disableAI && base.entity.owner.controllingEntity != base.entity;
		if (!DisableAI && isAITicking && Time.time > _nextAiUpdateTime && !base.entity.Visual.isSpawning)
		{
			if (base.entity.Control.IsActionBlocked(EntityControl.BlockableAction.Ability) == EntityControl.BlockStatus.Blocked)
			{
				return;
			}
			if (UnityEngine.Random.value < _congestionSkipChance)
			{
				_nextAiUpdateTime = Time.time + UnityEngine.Random.value * GetAIInterval();
				return;
			}
			CallAIUpdateImmediately();
			if (base.entity is Monster m && Time.time > _nextWanderTime && _aiContext.targetEnemy == null && base.entity.Control.queuedActions.Count == 0)
			{
				_nextWanderTime = Time.time + UnityEngine.Random.Range(4f, 15f);
				if (m.campPosition.HasValue)
				{
					Vector3 pos = Dew.GetPositionOnGround(m.campPosition.Value + UnityEngine.Random.insideUnitSphere * 6f);
					pos = Dew.GetValidAgentDestination_LinearSweep(m.campPosition.Value, pos);
					base.entity.Control.MoveToDestination(pos, immediately: false, 0.6f);
				}
				else if (base.entity.section != null)
				{
					_nextWanderTime = Time.time + UnityEngine.Random.Range(4f, 15f);
					Vector3 targetPos = base.entity.section.GetGoodWanderPosition(base.entity.agentPosition);
					base.entity.Control.MoveToDestination(targetPos, immediately: false, 0.6f);
				}
			}
			_lastAiUpdateTime = Time.time;
			_nextAiUpdateTime = Time.time + GetAIInterval();
		}
		if (!(Time.time - _lastSampleTime > PositionSampleInterval))
		{
			return;
		}
		if (_positionSamples.Length < PositionSampleCount)
		{
			_positionSamples = ArrayPool<Vector3>.Shared.Rent(PositionSampleCount);
			for (int i = 0; i < PositionSampleCount; i++)
			{
				_positionSamples[i] = base.transform.position;
			}
		}
		_lastSampleTime = Time.time;
		_lastSampleIndex = (_lastSampleIndex + 1) % PositionSampleCount;
		_positionSamples[_lastSampleIndex] = base.transform.position;
		int oldestSampleIndex = _lastSampleIndex + 1;
		if (oldestSampleIndex >= PositionSampleCount)
		{
			oldestSampleIndex = 0;
		}
		estimatedVelocity = (_positionSamples[(PositionSampleCount + _lastSampleIndex - PositionSampleLagBehindFrames) % PositionSampleCount] - _positionSamples[oldestSampleIndex]) / (PositionSampleInterval * (float)(PositionSampleCount - 1 - PositionSampleLagBehindFrames));
		estimatedVelocityUnclamped = estimatedVelocity;
		estimatedVelocity = Vector3.ClampMagnitude(estimatedVelocity, base.entity.Control.currentMaxAgentSpeed);
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		_aiContext = default(EntityAIContext);
		if (isAITicking)
		{
			isAITicking = false;
		}
		if (_positionSamples != null)
		{
			ArrayPool<Vector3>.Shared.Return(_positionSamples);
		}
	}

	public void Aggro(Entity target, bool doPropagation = false)
	{
		if (!base.entity.CheckEnemyOrNeutral(target))
		{
			Debug.LogWarning($"Cannot aggro to non-enemy entity: {base.entity} => {target}");
			return;
		}
		if (_aiContext.targetEnemy == null)
		{
			_nextAiUpdateTime = Time.time - 0.01f;
		}
		_aiContext.targetEnemy = target;
		_aiContext._targetEnemyStartTime = Time.time;
		_aiContext._targetEnemyLoseElapsedTime = 0f;
		_aiContext._retargetingTime = UnityEngine.Random.Range(1f, 4f);
		if (!doPropagation)
		{
			return;
		}
		ArrayReturnHandle<Entity> handle0;
		ReadOnlySpan<Entity> nearbyAllies = DewPhysics.OverlapCircleAllEntities(out handle0, base.transform.position, 17f, _allyValidator, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		ReadOnlySpan<Entity> readOnlySpan = nearbyAllies;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			readOnlySpan[i].AI.ReceiveAggroPropagation(target);
		}
		if (base.entity.section != null)
		{
			foreach (Entity e in base.entity.section.entities)
			{
				if (!nearbyAllies.Contains(e) && !e.IsNullOrInactive() && base.entity.GetRelation(e) == EntityRelation.Ally)
				{
					e.AI.ReceiveAggroPropagation(target);
				}
			}
		}
		handle0.Return();
	}

	internal void ReceiveAggroPropagation(Entity target)
	{
		if (base.entity.GetRelation(target) == EntityRelation.Enemy)
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.7f));
			if (!(_aiContext.targetEnemy != null) && base.entity.isActive)
			{
				Aggro(target);
			}
		}
	}

	[Server]
	private void UpdateAIContext(float dt)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAI::UpdateAIContext(System.Single)' called when server was not active");
			return;
		}
		_aiContext.deltaTime = dt;
		if (_aiContext.targetEnemy != null)
		{
			if (!_aiContext.targetEnemy.isActive || _aiContext.targetEnemy.Status.isDead || _aiContext.targetEnemy is Hero { isKnockedOut: not false } || _aiContext._targetEnemyLoseElapsedTime > 5f)
			{
				_aiContext.targetEnemy = null;
				if (_aiContext.targetEnemyLastKnownPosition.HasValue && base.entity.Ability.attackAbility != null)
				{
					base.entity.Control.MoveToDestination(_aiContext.targetEnemyLastKnownPosition.Value, immediately: false);
				}
			}
			else
			{
				if (_aiContext.targetEnemy is Hero h2)
				{
					h2.MarkAsInCombat();
				}
				if (_aiContext.targetEnemyElapsedTime > _aiContext._retargetingTime)
				{
					ArrayReturnHandle<Entity> handle;
					ReadOnlySpan<Entity> ents = DewPhysics.OverlapCircleAllEntities(out handle, base.transform.position, detectionRange, _targetEnemyValidator);
					if (ents.Length > 0)
					{
						if (base.entity.IsAnyBoss() && UnityEngine.Random.value < 0.4f)
						{
							Aggro(ents[UnityEngine.Random.Range(0, ents.Length)]);
						}
						else
						{
							Entity target = Dew.SelectBestWithScore(ents, (Entity e, int _) => 0f - Vector2.Distance(base.entity.agentPosition.ToXY(), e.agentPosition.ToXY()), 0.3f);
							Aggro(target);
						}
					}
					else
					{
						Aggro(_aiContext.targetEnemy);
					}
					handle.Return();
				}
			}
		}
		if (_aiContext.targetEnemy == null)
		{
			ArrayReturnHandle<Entity> handle2;
			ReadOnlySpan<Entity> ents2 = DewPhysics.OverlapCircleAllEntities(out handle2, base.transform.position, detectionRange, _targetEnemyValidator, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (ents2.Length > 0)
			{
				Aggro(ents2[0], doPropagation: true);
			}
			handle2.Return();
		}
		if (_aiContext.targetEnemy != null)
		{
			_aiContext.targetEnemyLastKnownPosition = _aiContext.targetEnemy.position;
			if (Vector2.Distance(_aiContext.targetEnemy.position.ToXY(), base.transform.position.ToXY()) - _aiContext.targetEnemy.Control.outerRadius > detectionRange || !_targetEnemyValidator(_aiContext.targetEnemy))
			{
				_aiContext._targetEnemyLoseElapsedTime += dt;
			}
		}
	}

	private void CheckInsideAIUpdate()
	{
		if (!_aiContext._insideAIUpdate)
		{
			throw new InvalidOperationException("AI helper functions can only be called inside AIUpdate");
		}
	}

	public T Helper_GetAbility<T>() where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T abil = ((!(base.entity.Ability.attackAbility is T atk)) ? base.entity.Ability.GetAbility<T>() : atk);
		if (abil == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_GetAbility", base.entity, typeof(T)));
		}
		return abil;
	}

	public void Helper_ChaseTarget()
	{
		CheckInsideAIUpdate();
		if (_aiContext.targetEnemy == null)
		{
			Debug.LogWarning(string.Format("{0}: '{1}' does not have a target right now", "Helper_ChaseTarget", base.entity));
		}
		else
		{
			base.entity.Control.Attack(_aiContext.targetEnemy, doChase: true);
		}
	}

	public void Helper_CastAbility<T>(CastInfo info) where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T abil = Helper_GetAbility<T>();
		if (abil == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_CastAbility", base.entity, typeof(T)));
		}
		else if (!abil.CanBeCast())
		{
			Debug.LogWarning(string.Format("{0}: '{1}' cannot cast '{2}' right now", "Helper_CastAbility", base.entity, typeof(T)));
		}
		else
		{
			base.entity.Control.Cast(abil, info);
		}
	}

	public bool Helper_TryGetCastInfoAuto<T>(out CastInfo info) where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T abil = Helper_GetAbility<T>();
		if (abil == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_CastAbility", base.entity, typeof(T)));
			info = default(CastInfo);
			return false;
		}
		return Helper_TryGetCastInfoAuto(abil, out info);
	}

	public bool Helper_TryGetCastInfoAuto(AbilityTrigger abil, out CastInfo info)
	{
		CheckInsideAIUpdate();
		if (abil == null)
		{
			info = default(CastInfo);
			return false;
		}
		if (abil.currentConfig.castMethod.type == CastMethodType.None)
		{
			info = new CastInfo(base.entity);
			return true;
		}
		Entity target = null;
		if (_aiContext.targetEnemy != null && abil.currentConfig.targetValidator.Evaluate(base.entity, _aiContext.targetEnemy))
		{
			target = _aiContext.targetEnemy;
		}
		else
		{
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> readOnlySpan = DewPhysics.OverlapCircleAllEntities(out handle, base.transform.position, detectionRange, new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				Entity candidate = readOnlySpan[i];
				if (abil.currentConfig.targetValidator.Evaluate(base.entity, candidate))
				{
					target = candidate;
					break;
				}
			}
			handle.Return();
		}
		if (target == null)
		{
			info = default(CastInfo);
			return false;
		}
		info = abil.GetPredictedCastInfoToTarget(target);
		return true;
	}

	public void Helper_CastAbilityAuto<T>() where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T abil = Helper_GetAbility<T>();
		CastInfo info;
		if (abil == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_CastAbilityAuto", base.entity, typeof(T)));
		}
		else if (!abil.CanBeCast())
		{
			Debug.LogWarning(string.Format("{0}: '{1}' cannot cast '{2}' right now", "Helper_CastAbilityAuto", base.entity, typeof(T)));
		}
		else if (!Helper_TryGetCastInfoAuto(abil, out info))
		{
			Debug.Log("Helper_CastAbilityAuto: Helper_TryGetCastInfoAuto failed");
		}
		else
		{
			base.entity.Control.Cast(abil, info);
		}
	}

	public bool Helper_CastAbilityAuto(AbilityTrigger abil)
	{
		CheckInsideAIUpdate();
		if (abil == null)
		{
			return false;
		}
		if (!abil.CanBeCast())
		{
			return false;
		}
		if (!Helper_TryGetCastInfoAuto(abil, out var info))
		{
			return false;
		}
		base.entity.Control.Cast(abil, info);
		return true;
	}

	public bool Helper_CanBeCast<T>() where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T abil = Helper_GetAbility<T>();
		if (abil == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_CanBeCast", base.entity, typeof(T)));
			return false;
		}
		if (abil.CanBeCast())
		{
			if (!abil.currentConfig.ignoreBlock)
			{
				return base.entity.Control.IsActionBlocked((!(abil is AttackTrigger)) ? EntityControl.BlockableAction.Ability : EntityControl.BlockableAction.Attack) != EntityControl.BlockStatus.Blocked;
			}
			return true;
		}
		return false;
	}

	public bool Helper_IsTargetInRange<T>() where T : AbilityTrigger
	{
		CheckInsideAIUpdate();
		T abil = Helper_GetAbility<T>();
		if (abil == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have '{2}'", "Helper_IsTargetInRange", base.entity, typeof(T)));
			return false;
		}
		return Helper_IsTargetInRange(abil);
	}

	public bool Helper_IsTargetInRange(AbilityTrigger abil)
	{
		CheckInsideAIUpdate();
		if (abil == null)
		{
			return false;
		}
		if (_aiContext.targetEnemy == null)
		{
			return false;
		}
		return abil.IsTargetInRange(_aiContext.targetEnemy);
	}

	public bool Helper_IsTargetInRangeOfAttack()
	{
		CheckInsideAIUpdate();
		AbilityTrigger abil = base.entity.Ability.attackAbility;
		if (abil == null)
		{
			Debug.LogError(string.Format("{0}: '{1}' does not have attack ability", "Helper_IsTargetInRangeOfAttack", base.entity));
			return false;
		}
		if (_aiContext.targetEnemy == null)
		{
			return false;
		}
		float effRange = abil.currentConfig.castMethod.GetEffectiveRange();
		return Vector3.Distance(base.entity.position, _aiContext.targetEnemy.position) - _aiContext.targetEnemy.Control.outerRadius < effRange;
	}

	private void MirrorProcessed()
	{
	}
}
