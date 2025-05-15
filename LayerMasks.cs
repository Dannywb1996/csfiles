using UnityEngine;

public static class LayerMasks
{
	public static readonly int Default = LayerMask.GetMask("Default");

	public static readonly int Entity = LayerMask.GetMask("Entity");

	public static readonly int Interactable = LayerMask.GetMask("Interactable");

	public static readonly int IncludeInNavigation = LayerMask.GetMask("IncludeInNavigation");

	public static readonly int CollidableWithProjectile = LayerMask.GetMask("CollidableWithProjectile");

	public static readonly int Ground = LayerMask.GetMask("Ground");

	public static readonly int Everything = -1;
}
