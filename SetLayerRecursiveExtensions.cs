using UnityEngine;

public static class SetLayerRecursiveExtensions
{
	public static void SetLayerRecursive(this GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform item in obj.transform)
		{
			item.gameObject.SetLayerRecursive(layer);
		}
	}
}
