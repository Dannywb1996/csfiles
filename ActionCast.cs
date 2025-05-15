using UnityEngine;

public class ActionCast : ActionBase
{
	public AbilityTrigger trigger;

	public int configIndex;

	public bool isPredictedOnCast;

	public CastInfo info;

	public Entity predictTarget;

	public bool skipRangeCheck;

	private Vector3? _lastReportedDestination;

	private float _lastReportedRequiredDistance;

	private float _lastAttackDestinationUpdateTime = float.NegativeInfinity;

	private float _nextAttackDestinationUpdateInterval;

	private TriggerConfig _config => trigger.currentConfig;

	private bool _isMonsterChase
	{
		get
		{
			if (_entity is Monster)
			{
				return trigger == _entity.Ability.attackAbility;
			}
			return false;
		}
	}

	public override bool Tick()
	{
		if (trigger.IsNullOrInactive() || (isPredictedOnCast && predictTarget.IsNullInactiveDeadOrKnockedOut()) || (configIndex >= 0 && trigger.currentConfigIndex != configIndex) || (_entity.Ability.attackAbility != trigger && !trigger.CanBeReserved()) || ((object)info.target != null && info.target.IsNullInactiveDeadOrKnockedOut()))
		{
			return true;
		}
		EntityControl.BlockableAction blockable = ((!(trigger is AttackTrigger)) ? EntityControl.BlockableAction.Ability : EntityControl.BlockableAction.Attack);
		if (base.isFirstTick && !_config.ignoreBlock && _entity.Control.IsActionBlocked(blockable) == EntityControl.BlockStatus.BlockedCancelable)
		{
			_entity.Control.DisobeyBlock(blockable);
		}
		bool canBeCast = trigger.CanBeCast();
		bool isBlocked = !_config.ignoreBlock && _entity.Control.IsActionBlocked(blockable) != EntityControl.BlockStatus.Allowed;
		if (trigger is AttackTrigger)
		{
			foreach (Channel ongoingChannel in _entity.Control.ongoingChannels)
			{
				if (ongoingChannel.isAttack)
				{
					isBlocked = true;
				}
			}
		}
		if (!canBeCast || isBlocked)
		{
			return false;
		}
		if (predictTarget != null && !_config.targetValidator.Evaluate(_entity, predictTarget))
		{
			return true;
		}
		bool isInRange = ((predictTarget != null) ? _config.CheckRange(_entity, predictTarget) : _config.CheckRange(info));
		if (skipRangeCheck)
		{
			isInRange = true;
		}
		if (!isInRange)
		{
			return false;
		}
		CastInfo castInfo = ((predictTarget != null) ? trigger.GetPredictedCastInfoToTarget(predictTarget) : info);
		if (_config.castMethod.type == CastMethodType.Point && _config.castMethod.pointData.isClamping)
		{
			float y = castInfo.point.y;
			Vector2 flatPoint = _entity.agentPosition.ToXY() + Vector2.ClampMagnitude(castInfo.point.ToXY() - _entity.agentPosition.ToXY(), _config.castMethod.pointData.range);
			castInfo.point = new Vector3(flatPoint.x, y, flatPoint.y);
		}
		if (_config.castMethod.type == CastMethodType.Point)
		{
			Vector2 xy = castInfo.point.ToXY();
			if (xy == _entity.agentPosition.ToXY() || xy == _entity.position.ToXY())
			{
				castInfo.point = _entity.agentPosition + _entity.transform.forward * 0.001f;
			}
		}
		trigger.OnCastStart(trigger.currentConfigIndex, castInfo);
		return true;
	}

	public override Vector3? GetMoveDestination()
	{
		Vector3 offset = default(Vector3);
		bool isMonsterChase = _isMonsterChase;
		if (isMonsterChase)
		{
			Monster m = (Monster)_entity;
			if (Time.time - _lastAttackDestinationUpdateTime < _nextAttackDestinationUpdateInterval && _lastReportedDestination.HasValue)
			{
				if (Vector2.Distance(_lastReportedDestination.Value.ToXY(), _entity.agentPosition.ToXY()) < _lastReportedRequiredDistance)
				{
					return isPredictedOnCast ? predictTarget.agentPosition : _config.GetMoveToCastDestination(info);
				}
				return _lastReportedDestination;
			}
			_lastAttackDestinationUpdateTime = Time.time;
			_nextAttackDestinationUpdateInterval = Random.Range(0.5f, 1.5f);
			offset += Random.onUnitSphere.Flattened() * 4.5f * m.chaseRandomness;
			if (isPredictedOnCast)
			{
				float dist = Vector2.Distance(predictTarget.agentPosition.ToXY(), _entity.agentPosition.ToXY());
				offset += predictTarget.AI.estimatedVelocity * (dist / m.Control.currentMaxAgentSpeed * Random.value * m.chasePredictiveness * 1.5f);
			}
		}
		Vector3 dest = (isPredictedOnCast ? (predictTarget.agentPosition + offset) : (_config.GetMoveToCastDestination(info) + offset));
		if (isMonsterChase)
		{
			dest = Dew.GetValidAgentDestination_LinearSweep(_entity.agentPosition, dest);
			_lastReportedDestination = dest;
		}
		return dest;
	}

	public override float GetMoveDestinationRequiredDistance()
	{
		return _lastReportedRequiredDistance = (isPredictedOnCast ? (_config.castMethod.GetEffectiveRange() + predictTarget.Control.outerRadius) : _config.GetMoveToCastRequiredDistance(info));
	}

	public override string ToString()
	{
		if (!isPredictedOnCast)
		{
			return string.Format("{0}({1})", "ActionCast", trigger.GetType());
		}
		return string.Format("{0}({1}, Predict:{2})", "ActionCast", trigger.GetType(), predictTarget.GetActorReadableName());
	}
}
