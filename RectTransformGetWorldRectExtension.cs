using UnityEngine;

public static class RectTransformGetWorldRectExtension
{
	public static Rect GetWorldRect(this RectTransform rt, Vector2 scale)
	{
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);
		Vector3 vector = corners[0];
		Vector2 scaledSize = new Vector2(scale.x * rt.rect.size.x, scale.y * rt.rect.size.y);
		return new Rect(vector, scaledSize);
	}

	public static Rect GetScreenSpaceRect(this RectTransform transform)
	{
		Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
		return new Rect((Vector2)transform.position - size * transform.pivot, size);
	}
}
