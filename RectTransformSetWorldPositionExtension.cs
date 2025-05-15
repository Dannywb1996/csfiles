using System;
using UnityEngine;

public static class RectTransformSetWorldPositionExtension
{
	public static void SetWorldPosition(this RectTransform rt, Vector3 worldPosition)
	{
		Canvas canvas = rt.GetComponentInParent<Canvas>();
		if (canvas == null)
		{
			throw new NullReferenceException("Cannot set the world position of a RectTransform without a parent canvas");
		}
		if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			rt.SetWorldPositionForScreenSpaceOverlay(worldPosition, Camera.main);
			return;
		}
		if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			rt.SetWorldPositionForScreenSpaceCamera(worldPosition, canvas);
			return;
		}
		throw new InvalidOperationException("Cannot set the world position of a RectTransform in a World Space canvas");
	}

	public static void SetWorldPositionForScreenSpaceOverlay(this RectTransform rt, Vector3 position, Camera camera)
	{
		rt.position = camera.WorldToScreenPoint(position);
		rt.rotation = Quaternion.identity;
	}

	public static void SetWorldPositionForScreenSpaceCamera(this RectTransform rt, Vector3 position, Canvas canvas)
	{
		if (canvas.worldCamera == null)
		{
			throw new NullReferenceException("Canvas in Screen Space - Camera doesn't have its world camera assigned");
		}
		RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)canvas.transform, canvas.worldCamera.WorldToScreenPoint(position), canvas.worldCamera, out var worldPoint);
		rt.position = worldPoint;
		rt.rotation = canvas.transform.rotation;
	}
}
