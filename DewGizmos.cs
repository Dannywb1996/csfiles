using System;
using UnityEngine;

public static class DewGizmos
{
	private const float RiftGizmosStarEdgeLength = 1.5f;

	private const float RiftGizmosDirectionLineLength = 2f;

	private const float RiftGizmosPositionY = 2.35f;

	public static void DrawLine(Vector3 start, Vector3 end, Color color)
	{
	}

	public static void DrawArrow(Vector3 start, Vector3 end, Color color, float headSize)
	{
	}

	public static void DrawText(string text, Vector3 pos, Color color, int fontSize)
	{
	}

	public static void DrawRift(Color color, Vector3 position, Quaternion rotation)
	{
		Color c = Gizmos.color;
		Gizmos.color = color;
		Vector3 center = position + new Vector3(0f, 2.35f, 0f);
		int numPoints = 10;
		Vector3[] starPoints = new Vector3[numPoints];
		for (int i = 0; i < numPoints; i++)
		{
			float radius = ((i % 2 == 0) ? 1.5f : 0.75f);
			float angle = (float)i * MathF.PI / (float)(numPoints / 2) + MathF.PI / 2f;
			starPoints[i] = center + rotation * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
		}
		for (int j = 0; j < numPoints; j++)
		{
			int nextIndex = (j + 1) % numPoints;
			Gizmos.DrawLine(starPoints[j], starPoints[nextIndex]);
		}
		Vector3 direction = rotation * Vector3.forward;
		for (int k = 0; k < numPoints; k += 2)
		{
			Gizmos.DrawLine(starPoints[k], starPoints[k] + direction * 2f);
		}
		Gizmos.color = c;
	}
}
