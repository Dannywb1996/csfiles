using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionAttackMove : ActionBase
{
	private const float AttackMoveCheckInterval = 0.2f;

	private const float AttackMoveAcquisitionBonusRange = 3f;

	private const float AttackMoveAcquisitionRangeMinimum = 6f;

	private static readonly IComparer<Entity> SortComparer = Comparer<Entity>.Create((Entity x, Entity y) => (Vector2.Distance(_sortPivot, x.agentPosition.ToXY()) - x.Control.outerRadius).CompareTo(Vector2.Distance(_sortPivot, y.agentPosition.ToXY()) - y.Control.outerRadius));

	private static Vector2 _sortPivot;

	public Vector3 destination;

	public bool useDistanceFromDestination;

	private float _lastAttackMoveCheckTime = float.NegativeInfinity;

	public override bool Tick()
	{
		if (base.isFirstTick)
		{
			if (_entity.Control.IsActionBlocked(EntityControl.BlockableAction.Move) == EntityControl.BlockStatus.BlockedCancelable)
			{
				_entity.Control.DisobeyBlock(EntityControl.BlockableAction.Move);
			}
			if (_entity.Control.IsActionBlocked(EntityControl.BlockableAction.Attack) == EntityControl.BlockStatus.BlockedCancelable)
			{
				_entity.Control.DisobeyBlock(EntityControl.BlockableAction.Attack);
			}
		}
		if (Vector2.Distance(destination.ToXY(), _entity.agentPosition.ToXY()) < 0.15f)
		{
			return true;
		}
		if (_entity.Status.hasStun)
		{
			return false;
		}
		if (Time.time - _lastAttackMoveCheckTime < 0.2f)
		{
			return false;
		}
		_lastAttackMoveCheckTime = Time.time;
		Entity target = FindAttackMoveTarget(_entity, useDistanceFromDestination ? destination.ToXY() : _entity.agentPosition.ToXY());
		if (target != null)
		{
			_entity.Control.Attack(target, doChase: true);
			return true;
		}
		return false;
	}

	public static Entity FindAttackMoveTarget(Entity ent, Vector3 sortPivot)
	{
		if (ent.Ability.attackAbility == null)
		{
			return null;
		}
		float radius = Mathf.Max(ent.Ability.attackAbility.currentConfig.castMethod.GetEffectiveRange() + 3f, 6f);
		_sortPivot = sortPivot;
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> readOnlySpan = DewPhysics.OverlapCircleAllEntities(out handle, ent.agentPosition, radius, ent.Ability.attackAbility.currentConfig.targetValidator, ent, new CollisionCheckSettings
		{
			sortComparer = SortComparer
		});
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Entity c = readOnlySpan[i];
			if (!c.AI.excludeFromAutoTargeting)
			{
				handle.Return();
				return c;
			}
		}
		handle.Return();
		return null;
	}

	public override Vector3? GetMoveDestination()
	{
		return destination;
	}

	public override float GetMoveDestinationRequiredDistance()
	{
		return 0.15f;
	}

	public override bool ShouldCancelIfDisallowedToMove()
	{
		return true;
	}

	public override string ToString()
	{
		return "ActionAttackMove()";
	}
}
