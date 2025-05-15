using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class DewCollider : MonoBehaviour
{
	public enum ColliderShape
	{
		Circle,
		Box,
		Polygon
	}

	public ColliderShape shape;

	public float radius = 1f;

	public Vector2 size = Vector2.one;

	public Vector2 offset;

	public Vector2[] points;

	public int maxDecimals = 2;

	public bool receiveEntityCallbacks;

	public bool invokeEventsOnClients;

	public UnityEvent<Entity> onEntityEnter = new UnityEvent<Entity>();

	public UnityEvent<Entity> onEntityExit = new UnityEvent<Entity>();

	internal Collider2D _proxy;

	private void OnEnable()
	{
		if (!(_proxy != null))
		{
			UpdateProxyCollider();
		}
	}

	private void OnDestroy()
	{
		if (_proxy != null)
		{
			UnityEngine.Object.Destroy(_proxy.gameObject);
		}
	}

	public void UpdateProxyCollider()
	{
		UpdateProxyCollider_Imp();
	}

	private void UpdateProxyCollider_Imp()
	{
		if (_proxy != null)
		{
			UnityEngine.Object.Destroy(_proxy.gameObject);
		}
		GameObject proxyObject = DewPhysics.AddEmptyObject(base.name);
		switch (shape)
		{
		case ColliderShape.Circle:
		{
			CircleCollider2D newCol3 = proxyObject.AddComponent<CircleCollider2D>();
			newCol3.radius = radius;
			newCol3.offset = offset;
			newCol3.isTrigger = true;
			_proxy = newCol3;
			break;
		}
		case ColliderShape.Box:
		{
			BoxCollider2D newCol2 = proxyObject.AddComponent<BoxCollider2D>();
			newCol2.size = size;
			newCol2.offset = offset;
			newCol2.isTrigger = true;
			_proxy = newCol2;
			break;
		}
		case ColliderShape.Polygon:
		{
			PolygonCollider2D newCol = proxyObject.AddComponent<PolygonCollider2D>();
			newCol.points = points;
			newCol.isTrigger = true;
			_proxy = newCol;
			break;
		}
		default:
			UnityEngine.Object.Destroy(proxyObject);
			throw new ArgumentException($"Unknown Collider Shape: {shape}");
		}
		if (receiveEntityCallbacks)
		{
			EntityCallbackTrigger entityCallbackTrigger = _proxy.gameObject.AddComponent<EntityCallbackTrigger>();
			entityCallbackTrigger.onEntityEnter = (Action<Entity>)Delegate.Combine(entityCallbackTrigger.onEntityEnter, (Action<Entity>)delegate(Entity e)
			{
				if (!e.IsNullInactiveDeadOrKnockedOut() && (invokeEventsOnClients || NetworkServer.active))
				{
					onEntityEnter?.Invoke(e);
				}
			});
			entityCallbackTrigger.onEntityExit = (Action<Entity>)Delegate.Combine(entityCallbackTrigger.onEntityExit, (Action<Entity>)delegate(Entity e)
			{
				if (invokeEventsOnClients || NetworkServer.active)
				{
					onEntityExit?.Invoke(e);
				}
			});
		}
		PositionColliders();
	}

	private void PositionColliders()
	{
		Vector3 newPos = new Vector3(base.transform.position.x, base.transform.position.z, 0f);
		Quaternion newRot = Quaternion.Euler(0f, 0f, 0f - base.transform.rotation.eulerAngles.y);
		Vector3 newSca = new Vector3(base.transform.lossyScale.x, base.transform.lossyScale.z, 0f);
		Transform t = _proxy.transform;
		if (!(t.position == newPos) || !(t.rotation == newRot) || !(t.localScale == newSca))
		{
			t.SetPositionAndRotation(newPos, newRot);
			t.localScale = newSca;
			Physics2D.autoSyncTransforms = false;
			Physics2D.SyncTransforms();
			Physics2D.autoSyncTransforms = true;
		}
	}

	public bool OverlapPoint(Vector2 point)
	{
		PositionColliders();
		if (_proxy.OverlapPoint(point))
		{
			return true;
		}
		return false;
	}

	private static bool ArrayContains<T>(IReadOnlyList<T> arr, T item, int index, int length)
	{
		for (int i = index; i < index + length; i++)
		{
			if (arr[i].Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	public ReadOnlySpan<Entity> GetEntities(out ArrayReturnHandle<Entity> handle, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return GetEntities(out handle, (Entity _) => true, settings);
	}

	public ReadOnlySpan<Entity> GetEntities(out ArrayReturnHandle<Entity> handle, IEntityValidator validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return GetEntities(out handle, validator.Evaluate, settings);
	}

	public ReadOnlySpan<Entity> GetEntities(out ArrayReturnHandle<Entity> handle, IBinaryEntityValidator validator, Entity self, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		return GetEntities(out handle, (Entity _) => validator.Evaluate(self, _), settings);
	}

	public ReadOnlySpan<Entity> GetEntities(out ArrayReturnHandle<Entity> handle, Func<Entity, bool> validator, CollisionCheckSettings settings = default(CollisionCheckSettings))
	{
		if (this == null)
		{
			Debug.Log("Tried to GetEntities on destroyed DewCollider");
			return new ReadOnlySpan<Entity>(DewPool.GetArray(out handle, 128), 0, 0);
		}
		PositionColliders();
		List<Collider2D> overlapList = new List<Collider2D>();
		Entity[] arr = DewPool.GetArray(out handle, 128);
		Physics2D.autoSyncTransforms = false;
		Physics2D.SyncTransforms();
		Physics2D.autoSyncTransforms = true;
		int entCount = 0;
		overlapList.Clear();
		_proxy.OverlapCollider(new ContactFilter2D
		{
			useLayerMask = true,
			layerMask = LayerMasks.Entity
		}, overlapList);
		for (int j = 0; j < overlapList.Count; j++)
		{
			if (entCount == arr.Length)
			{
				break;
			}
			if (DewPhysics.TryGetEntity(overlapList[j], out var ent) && validator(ent) && (settings.includeUncollidable || !ent.Status.hasUncollidable) && ent.isActive && !ArrayContains(arr, ent, 0, entCount))
			{
				arr[entCount] = ent;
				entCount++;
			}
		}
		DewPhysics.SortArray(settings, arr, entCount, base.transform.position);
		return new ReadOnlySpan<Entity>(arr, 0, entCount);
	}

	public IEnumerable<List<Entity>> SweepEntitiesFromOrigin(int iterations, bool includeUncollidable = false)
	{
		return SweepEntitiesFromOrigin(iterations, (Entity _) => true, includeUncollidable);
	}

	public IEnumerable<List<Entity>> SweepEntitiesFromOrigin(int iterations, IEntityValidator validator, bool includeUncollidable = false)
	{
		return SweepEntitiesFromOrigin(iterations, (Entity _) => validator.Evaluate(_), includeUncollidable);
	}

	public IEnumerable<List<Entity>> SweepEntitiesFromOrigin(int iterations, IBinaryEntityValidator validator, Entity self, bool includeUncollidable = false)
	{
		return SweepEntitiesFromOrigin(iterations, (Entity _) => validator.Evaluate(self, _), includeUncollidable);
	}

	public IEnumerable<List<Entity>> SweepEntitiesFromOrigin(int iterations, Func<Entity, bool> validator, bool includeUncollidable = false)
	{
		DewCollider[] colliders = GetComponentsInChildren<DewCollider>(includeInactive: true);
		EdgeCollider2D helperCol = DewPhysics.AddEmptyObject("SweepHelper of " + base.name).AddComponent<EdgeCollider2D>();
		helperCol.isTrigger = true;
		List<Collider2D> colsTemp = new List<Collider2D>();
		List<Collider2D> colsMain = new List<Collider2D>();
		List<Collider2D> colsHelper = new List<Collider2D>();
		List<Entity> ents = new List<Entity>();
		List<Entity> affectedEnts = new List<Entity>();
		float maxDistance = -1f;
		foreach (DewCollider dc in colliders)
		{
			if (dc._proxy is CircleCollider2D cc2d)
			{
				maxDistance = Mathf.Max(maxDistance, Vector3.Distance(dc.transform.position, base.transform.position) + cc2d.radius * dc.transform.lossyScale.x);
			}
			else if (dc._proxy is PolygonCollider2D pc2d)
			{
				for (int j = 0; j < pc2d.points.Length; j++)
				{
					float dist = Vector3.Distance(base.transform.position, dc.transform.TransformPoint(pc2d.points[j].ToXZ()));
					maxDistance = Mathf.Max(maxDistance, dist);
				}
			}
			else if (dc._proxy is BoxCollider2D box2d)
			{
				Vector2 halfSize = box2d.size * 0.5f;
				Vector2 offset = box2d.offset;
				float dist2 = Vector3.Distance(base.transform.position, dc.transform.TransformPoint((new Vector2(0f - halfSize.x, 0f - halfSize.y) + offset).ToXZ()));
				maxDistance = Mathf.Max(maxDistance, dist2);
				dist2 = Vector3.Distance(base.transform.position, dc.transform.TransformPoint((new Vector2(halfSize.x, 0f - halfSize.y) + offset).ToXZ()));
				maxDistance = Mathf.Max(maxDistance, dist2);
				dist2 = Vector3.Distance(base.transform.position, dc.transform.TransformPoint((new Vector2(halfSize.x, halfSize.y) + offset).ToXZ()));
				maxDistance = Mathf.Max(maxDistance, dist2);
				dist2 = Vector3.Distance(base.transform.position, dc.transform.TransformPoint((new Vector2(0f - halfSize.x, halfSize.y) + offset).ToXZ()));
				maxDistance = Mathf.Max(maxDistance, dist2);
			}
		}
		if (maxDistance < 0f)
		{
			throw new Exception("Calculated max distance of DewCollider is invalid");
		}
		for (int iter = 0; iter < iterations; iter++)
		{
			PositionColliders();
			Vector2[] points = new Vector2[13];
			GeneratePointsInCircle(12, maxDistance / (float)iterations * (float)(iter + 1), 360f, points);
			points[12] = points[0];
			helperCol.points = points;
			helperCol.edgeRadius = maxDistance / (float)iterations;
			helperCol.transform.position = new Vector3(base.transform.position.x, base.transform.position.z, 0f);
			helperCol.transform.rotation = Quaternion.Euler(0f, 0f, 0f - base.transform.rotation.eulerAngles.y);
			Physics2D.autoSyncTransforms = false;
			Physics2D.SyncTransforms();
			Physics2D.autoSyncTransforms = true;
			colsMain.Clear();
			for (int k = 0; k < colliders.Length; k++)
			{
				colsTemp.Clear();
				Physics2D.OverlapCollider(colliders[k]._proxy, new ContactFilter2D
				{
					useLayerMask = true,
					layerMask = LayerMasks.Entity
				}, colsTemp);
				for (int l = 0; l < colsTemp.Count; l++)
				{
					if (!colsMain.Contains(colsTemp[l]))
					{
						colsMain.Add(colsTemp[l]);
					}
				}
			}
			colsHelper.Clear();
			Physics2D.OverlapCollider(helperCol, new ContactFilter2D
			{
				useLayerMask = true,
				layerMask = LayerMasks.Entity
			}, colsHelper);
			ents.Clear();
			for (int m = 0; m < colsMain.Count; m++)
			{
				Collider2D col = colsMain[m];
				if (colsHelper.Contains(col) && DewPhysics.TryGetEntity(col, out var ent) && !affectedEnts.Contains(ent) && validator(ent) && (includeUncollidable || !ent.Status.hasUncollidable))
				{
					affectedEnts.Add(ent);
					ents.Add(ent);
				}
			}
			yield return ents;
		}
		UnityEngine.Object.Destroy(helperCol.gameObject);
	}

	public void GeneratePolygonPoints_Circle(float circleRadius, int accuracy = 20)
	{
		GeneratePolygonPoints_ArcDonut(0f, circleRadius, 360f, accuracy);
	}

	public void GeneratePolygonPoints_Arc(float arcRadius, float arcAngle, int accuracy = 20)
	{
		GeneratePolygonPoints_ArcDonut(0f, arcRadius, arcAngle, accuracy);
	}

	public void GeneratePolygonPoints_Donut(float innerRadius, float outerRadius, int accuracy = 20)
	{
		GeneratePolygonPoints_ArcDonut(innerRadius, outerRadius, 360f, accuracy);
	}

	public void GeneratePolygonPoints_ArcDonut(float innerRadius, float outerRadius, float arcAngle = 360f, int accuracy = 20)
	{
		if (shape != ColliderShape.Polygon)
		{
			throw new InvalidOperationException();
		}
		if (innerRadius > 0.01f && arcAngle < 359f)
		{
			int num = Mathf.CeilToInt((float)accuracy / 360f * arcAngle);
			Vector2[] p = new Vector2[num * 2];
			Vector2[] a = GeneratePointsInCircle(num, outerRadius, arcAngle);
			Vector2[] sourceArray = GeneratePointsInCircle(num, innerRadius, arcAngle).Reverse().ToArray();
			Array.Copy(a, 0, p, 0, num);
			Array.Copy(sourceArray, 0, p, num, num);
			points = p;
		}
		else if (innerRadius > 0.01f)
		{
			Vector2[] p2 = new Vector2[accuracy * 2 + 2];
			int num2 = Mathf.CeilToInt((float)accuracy / 360f * arcAngle);
			Vector2[] a2 = GeneratePointsInCircle(num2, outerRadius, arcAngle);
			Vector2[] sourceArray2 = GeneratePointsInCircle(num2, innerRadius, arcAngle).Reverse().ToArray();
			Array.Copy(a2, 0, p2, 0, num2);
			Array.Copy(sourceArray2, 0, p2, num2 + 1, num2);
			p2[num2] = p2[0];
			p2[num2 * 2 + 1] = p2[num2 + 1];
			points = p2;
		}
		else if (arcAngle < 359f)
		{
			int num3 = Mathf.CeilToInt((float)accuracy / 360f * arcAngle);
			Vector2[] p3 = new Vector2[num3 + 1];
			Array.Copy(GeneratePointsInCircle(num3, outerRadius, arcAngle), 0, p3, 0, num3);
			p3[^1] = Vector2.zero;
			points = p3;
		}
		else
		{
			int num4 = Mathf.CeilToInt((float)accuracy / 360f * arcAngle);
			points = GeneratePointsInCircle(num4, outerRadius, arcAngle);
		}
	}

	public static Vector2[] GeneratePointsInCircle(int count, float radius, float angle)
	{
		Vector2[] points = new Vector2[count];
		GeneratePointsInCircle(count, radius, angle, points);
		return points;
	}

	public static void GeneratePointsInCircle(int count, float radius, float angle, Vector2[] points)
	{
		float start = MathF.PI * -2f * angle / 360f * 0.5f;
		float end = 0f - start;
		for (int i = 0; i < count; i++)
		{
			float rad = Mathf.Lerp(start, end, (float)i / (float)(points.Length - 1));
			points[i] = new Vector2(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius);
		}
	}
}
