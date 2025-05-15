using UnityEngine;

[RequireComponent(typeof(InteractableMeshHighlightProvider))]
public class Item : Actor, IInteractable
{
	public GameObject startEffect;

	public GameObject endEffect;

	public GameObject endEffectOnConsumer;

	int IInteractable.priority => 0;

	public Transform interactPivot => base.transform;

	public bool canInteractWithMouse => true;

	public float focusDistance => 3f;

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (startEffect != null)
		{
			FxPlay(startEffect);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (startEffect != null)
		{
			FxStop(startEffect);
		}
		if (endEffect != null)
		{
			FxPlay(endEffect);
		}
	}

	public virtual bool CanInteract(Entity entity)
	{
		if (base.isActive)
		{
			return entity is Hero;
		}
		return false;
	}

	public virtual void OnInteract(Entity entity, bool alt)
	{
		if (endEffectOnConsumer != null)
		{
			FxPlay(endEffectOnConsumer, entity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
