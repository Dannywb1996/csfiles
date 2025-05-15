using System.Collections.Generic;
using UnityEngine;

public static class PolygonTools
{
	public static float[] _areas = new float[128];

	public static void GetVerticesArray(IReadOnlyList<Vector2> points, ref Vector2[] verts)
	{
		for (int i = 0; i < points.Count - 2; i++)
		{
			verts[i * 3] = points[i];
			verts[i * 3 + 1] = points[i + 1];
			verts[i * 3 + 2] = points[i + 2];
		}
	}

	public static float GetArea(IReadOnlyList<Vector2> vertices)
	{
		float areaSum = 0f;
		for (int i = 0; i < vertices.Count / 3; i++)
		{
			Vector2 v = vertices[i * 3 + 1] - vertices[i * 3];
			Vector2 b = vertices[i * 3 + 2] - vertices[i * 3];
			float area = Mathf.Abs(v.Cross(b));
			_areas[i] = area;
			areaSum += area;
		}
		return areaSum;
	}

	public static int PickRandomTriangleWeightedByArea(IReadOnlyList<Vector2> vertices, ref Vector2[] triVerts)
	{
		float areaSum = GetArea(vertices);
		float rng = Random.Range(0f, areaSum);
		for (int i = 0; i < vertices.Count / 3; i++)
		{
			if (rng < _areas[i])
			{
				triVerts[0] = vertices[i * 3];
				triVerts[1] = vertices[i * 3 + 1];
				triVerts[2] = vertices[i * 3 + 2];
				return i;
			}
			rng -= _areas[i];
		}
		triVerts[0] = vertices[vertices.Count - 3];
		triVerts[1] = vertices[vertices.Count - 2];
		triVerts[2] = vertices[vertices.Count - 1];
		return vertices.Count / 3 - 1;
	}

	public static Vector2 GetRandomPositionInTriangle(Vector2[] triVerts)
	{
		float r1 = Mathf.Sqrt(Random.Range(0f, 1f));
		float r2 = Random.Range(0f, 1f);
		float num = 1f - r1;
		float m2 = r1 * (1f - r2);
		float m3 = r2 * r1;
		return num * triVerts[0] + m2 * triVerts[1] + m3 * triVerts[2];
	}
}
