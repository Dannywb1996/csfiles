using Mirror;
using UnityEngine;

public class ActionInteract : ActionBase
{
	private const float InteractionRange = 3f;

	public IInteractable interactable;

	public bool isAlt;

	public bool noActivation;

	private Collider _collider;

	public override bool Tick()
	{
		if (interactable == null || interactable.IsUnityNull() || interactable is Actor { isActive: false } || !interactable.CanInteract(_entity))
		{
			return true;
		}
		if (_entity.Status.hasStun)
		{
			return true;
		}
		if (base.isFirstTick)
		{
			ListReturnHandle<Collider> handle;
			foreach (Collider c in ((Component)interactable).GetComponentsInChildrenNonAlloc(includeInactive: true, out handle))
			{
				if (c.gameObject.layer == 14)
				{
					_collider = c;
					break;
				}
			}
			handle.Return();
		}
		Vector3 pos = ((_collider != null) ? _collider.ClosestPoint(_entity.agentPosition) : interactable.interactPivot.position);
		if (Vector2.Distance(_entity.agentPosition.ToXY(), pos.ToXY()) < 3f)
		{
			_entity.Control.ClearActionQueue();
			_entity.Control.RotateTowards(interactable.interactPivot.position, immediately: false);
			_entity.Control.ClearMovement();
			if (!noActivation)
			{
				_entity.RpcInvokeInteract(((NetworkBehaviour)interactable).netIdentity, isAlt);
			}
			return true;
		}
		return false;
	}

	public override Vector3? GetMoveDestination()
	{
		return interactable.interactPivot.position;
	}

	public override float GetMoveDestinationRequiredDistance()
	{
		return 2.5500002f;
	}

	public override string ToString()
	{
		string iname = ((interactable is Actor a) ? a.GetActorReadableName() : interactable.GetType().Name);
		return "ActionInteract(" + iname + ")";
	}
}
