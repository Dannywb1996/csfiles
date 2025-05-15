using System;
using UnityEngine;

public class EntityCallbackTrigger : MonoBehaviour
{
	public Action<Entity> onEntityEnter;

	public Action<Entity> onEntityExit;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (DewPhysics.TryGetEntity(other, out var ent))
		{
			onEntityEnter?.Invoke(ent);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (DewPhysics.TryGetEntity(other, out var ent))
		{
			onEntityExit?.Invoke(ent);
		}
	}
}
