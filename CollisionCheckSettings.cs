using System.Collections.Generic;
using UnityEngine;

public struct CollisionCheckSettings
{
	public static readonly Comparer<Entity> None = null;

	public static readonly Comparer<Entity> DistanceFromCenter = Comparer<Entity>.Create((Entity a, Entity b) => Vector3.SqrMagnitude(a.position - _distanceCheckPivot).CompareTo(Vector3.SqrMagnitude(b.position - _distanceCheckPivot)));

	public static readonly Comparer<Entity> DistanceFromPivot = Comparer<Entity>.Create((Entity a, Entity b) => Vector3.SqrMagnitude(a.position - pivot).CompareTo(Vector3.SqrMagnitude(b.position - pivot)));

	public static readonly Comparer<Entity> Random = Comparer<Entity>.Create((Entity _, Entity _) => 0);

	internal static Vector3 _distanceCheckPivot;

	public static Vector3 pivot;

	public bool includeUncollidable;

	public IComparer<Entity> sortComparer;
}
