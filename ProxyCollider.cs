using UnityEngine;

public class ProxyCollider : MonoBehaviour
{
	public Entity entity;

	public IInteractable interactable;

	public ICollidableWithProjectile collidableWithProjectile;

	public Collider2D collider;
}
