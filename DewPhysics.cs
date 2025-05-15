using System;
using System.Collections.Generic;
using UnityEngine;

public static class DewPhysics
{
	public class EntitySweep
	{
		private List<Entity> _current = new List<Entity>();

		private List<Entity> _collided = new List<Entity>();

		private RaycastHit2D[] _hits = new RaycastHit2D[128];

		public float radius = 1f;

		public bool ignoreDuplicateHits = true;

		public bool includeUncollidable;

		public AbilityTargetValidator validator;

		public Entity validatorSelf;

		public IReadOnlyList<Entity> current => _current;

		public Vector3 lastPosition { get; private set; } = Vector3.positiveInfinity;

		private bool ValidateEntity(Entity ent)
		{
			if (validator != null && !validator.Evaluate(validatorSelf, ent))
			{
				return false;
			}
			if (!includeUncollidable && ent.Status.hasUncollidable)
			{
				return false;
			}
			return true;
		}

		public IReadOnlyList<Entity> Next(Vector3 position)
		{
			if (lastPosition == Vector3.positiveInfinity)
			{
				Collider2D[] array = Physics2D.OverlapCircleAll(position.ToXY(), radius, LayerMasks.Entity);
				for (int i = 0; i < array.Length; i++)
				{
					if (TryGetEntity(array[i], out var ent) && ValidateEntity(ent))
					{
						_current.Add(ent);
						_collided.Add(ent);
					}
				}
			}
			else
			{
				_current.Clear();
				int count = Physics2D.CircleCastNonAlloc(lastPosition.ToXY(), radius, position - lastPosition, _hits, Vector3.Distance(position, lastPosition), LayerMasks.Entity);
				for (int j = 0; j < count; j++)
				{
					if (TryGetEntity(_hits[j].collider, out var ent2) && ValidateEntity(ent2) && (!ignoreDuplicateHits || !_collided.Contains(ent2)))
					{
						_current.Add(ent2);
						_collided.Add(ent2);
					}
				}
			}
			lastPosition = position;
			return current;
		}
	}

	private static Collider2D[] _colliders = new Collider2D[256];

	private static RaycastHit2D[] _hits = new RaycastHit2D[256];

	private static readonly Func<Entity, bool> _alwaysTrue = (Entity _) => true;

	public static ReadOnlySpan<Entity> SphereCastAllEntities(out ArrayReturnHandle<Entity> handle, Vector3 origin, float radius, Vector3 direction, float maxDistance, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return SphereCastAllEntities(out handle, origin, radius, direction, maxDistance, _alwaysTrue, settings);
	}

	public static ReadOnlySpan<Entity> SphereCastAllEntities(out ArrayReturnHandle<Entity> handle, Vector3 origin, float radius, Vector3 direction, float maxDistance, IEntityValidator validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return SphereCastAllEntities(out handle, origin, radius, direction, maxDistance, validator.Evaluate, settings);
	}

	public static ReadOnlySpan<Entity> SphereCastAllEntities(out ArrayReturnHandle<Entity> handle, Vector3 origin, float radius, Vector3 direction, float maxDistance, IBinaryEntityValidator validator, Entity self, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return SphereCastAllEntities(out handle, origin, radius, direction, maxDistance, (Entity e) => validator.Evaluate(self, e), settings);
	}

	public static ReadOnlySpan<Entity> SphereCastAllEntities(out ArrayReturnHandle<Entity> handle, Vector3 origin, float radius, Vector3 direction, float maxDistance, Func<Entity, bool> validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		maxDistance = (direction.normalized * maxDistance).Flattened().magnitude;
		int objCount = Physics2D.CircleCastNonAlloc(origin.ToXY(), radius, direction.ToXY(), _hits, maxDistance, LayerMasks.Entity);
		Entity[] arr = DewPool.GetArray(out handle, 128);
		int entCount = 0;
		for (int i = 0; i < objCount; i++)
		{
			if (entCount == arr.Length)
			{
				break;
			}
			if (TryGetEntity(_hits[i].collider, out var ent) && ent.isActive && validator(ent) && (settings.includeUncollidable || !ent.Status.hasUncollidable))
			{
				arr[entCount] = ent;
				entCount++;
			}
		}
		SortArray(settings, arr, entCount, origin);
		return new ReadOnlySpan<Entity>(arr, 0, entCount);
	}

	public static ReadOnlySpan<Entity> OverlapCircleAllEntities(out ArrayReturnHandle<Entity> handle, Vector3 center, float radius, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return OverlapCircleAllEntities(out handle, center, radius, _alwaysTrue, settings);
	}

	public static ReadOnlySpan<Entity> OverlapCircleAllEntities(out ArrayReturnHandle<Entity> handle, Vector3 center, float radius, IEntityValidator validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return OverlapCircleAllEntities(out handle, center, radius, validator.Evaluate, settings);
	}

	public static ReadOnlySpan<Entity> OverlapCircleAllEntities(out ArrayReturnHandle<Entity> handle, Vector3 center, float radius, IBinaryEntityValidator validator, Entity self, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return OverlapCircleAllEntities(out handle, center, radius, (Entity e) => validator.Evaluate(self, e), settings);
	}

	public static ReadOnlySpan<Entity> OverlapCircleAllEntities(out ArrayReturnHandle<Entity> handle, Vector3 center, float radius, Func<Entity, bool> validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		int objCount = Physics2D.OverlapCircleNonAlloc(center.ToXY(), radius, _colliders, LayerMasks.Entity);
		Entity[] arr = DewPool.GetArray(out handle, 128);
		int entCount = 0;
		for (int i = 0; i < objCount; i++)
		{
			if (entCount == arr.Length)
			{
				break;
			}
			if (TryGetEntity(_colliders[i], out var ent) && ent.isActive && validator(ent) && (settings.includeUncollidable || !ent.Status.hasUncollidable))
			{
				arr[entCount] = ent;
				entCount++;
			}
		}
		SortArray(settings, arr, entCount, center);
		return new ReadOnlySpan<Entity>(arr, 0, entCount);
	}

	internal static void SortArray(CollisionCheckSettings settings, Entity[] arr, int entCount, Vector3 distanceCheckPivot)
	{
		if (settings.sortComparer == null)
		{
			return;
		}
		if (settings.sortComparer == CollisionCheckSettings.DistanceFromCenter)
		{
			CollisionCheckSettings._distanceCheckPivot = distanceCheckPivot;
			Array.Sort(arr, 0, entCount, CollisionCheckSettings.DistanceFromCenter);
		}
		else if (settings.sortComparer == CollisionCheckSettings.Random)
		{
			int n = entCount;
			while (n > 1)
			{
				int k = UnityEngine.Random.Range(0, n--);
				int num = n;
				int num2 = k;
				Entity entity = arr[k];
				Entity entity2 = arr[n];
				arr[num] = entity;
				arr[num2] = entity2;
			}
		}
		else
		{
			Array.Sort(arr, 0, entCount, settings.sortComparer);
		}
	}

	public static bool TryGetProxy(Collider2D collider, out ProxyCollider proxy)
	{
		return collider.TryGetComponent<ProxyCollider>(out proxy);
	}

	public static bool TryGetEntity(Collider2D collider, out Entity entity)
	{
		if (!collider.TryGetComponent<ProxyCollider>(out var proxy) || proxy.entity == null)
		{
			entity = null;
			return false;
		}
		entity = proxy.entity;
		return true;
	}

	public static bool TryGetInteractable(Collider2D collider, out IInteractable interactable)
	{
		if (!collider.TryGetComponent<ProxyCollider>(out var proxy) || proxy.interactable == null)
		{
			interactable = null;
			return false;
		}
		interactable = proxy.interactable;
		return true;
	}

	public static bool TryGetCollidableWithProjectile(Collider2D collider, out ICollidableWithProjectile collidable)
	{
		if (!collider.TryGetComponent<ProxyCollider>(out var proxy) || proxy.collidableWithProjectile == null)
		{
			collidable = null;
			return false;
		}
		collidable = proxy.collidableWithProjectile;
		return true;
	}

	internal static GameObject AddEmptyObject(string name)
	{
		Transform parent = ManagerBase<DewPhysicsManager>.instance.transform;
		GameObject gameObject = new GameObject(name);
		gameObject.transform.parent = parent.transform;
		return gameObject;
	}

	internal static ProxyCollider AddProxyOfEntity(Entity entity)
	{
		Transform parent = ManagerBase<DewPhysicsManager>.instance.transform;
		ProxyCollider proxyCollider = new GameObject($"Entity Proxy - {entity.GetType().Name} [{entity.netId}]").AddComponent<ProxyCollider>();
		proxyCollider.transform.parent = parent.transform;
		proxyCollider.gameObject.layer = 8;
		CircleCollider2D newCol = proxyCollider.gameObject.AddComponent<CircleCollider2D>();
		newCol.radius = entity.Control.outerRadius;
		proxyCollider.collider = newCol;
		proxyCollider.entity = entity;
		proxyCollider.gameObject.AddComponent<Rigidbody2D>().isKinematic = true;
		return proxyCollider;
	}

	internal static ProxyCollider AddProxyOfCollidableWithProjectile(ICollidableWithProjectile collidable, float radius = 0.25f)
	{
		Transform parent = ManagerBase<DewPhysicsManager>.instance.transform;
		string name = "ICollidableWithProjectile";
		if (collidable is Component comp)
		{
			name = comp.name;
		}
		ProxyCollider proxyCollider = new GameObject("Collidable With Projectile Proxy - " + name).AddComponent<ProxyCollider>();
		proxyCollider.transform.parent = parent.transform;
		proxyCollider.gameObject.layer = 14;
		CircleCollider2D newCol = proxyCollider.gameObject.AddComponent<CircleCollider2D>();
		newCol.radius = radius;
		proxyCollider.collider = newCol;
		proxyCollider.collidableWithProjectile = collidable;
		return proxyCollider;
	}

	internal static void UpdateProxyPosition(ProxyCollider proxy, Vector3 position)
	{
		proxy.transform.position = new Vector3(position.x, position.z, 0f);
	}

	internal static void RemoveProxy(ProxyCollider proxy)
	{
		if (proxy != null && proxy.gameObject != null)
		{
			UnityEngine.Object.Destroy(proxy.gameObject);
		}
	}
}
