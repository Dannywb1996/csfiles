using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cells2D<T>
{
	public Vector2 center;

	public Vector2 min;

	public Vector2 max;

	public Vector2 size;

	public float cellSize;

	public T[] data;

	public int dataWidth;

	public int dataHeight;

	public Cells2D()
	{
	}

	public Cells2D(Vector2 worldCenter, float cellSize, int dataWidth, int dataHeight)
	{
		center = worldCenter;
		this.cellSize = cellSize;
		data = new T[dataWidth * dataHeight];
		this.dataWidth = dataWidth;
		this.dataHeight = dataHeight;
		min = center - new Vector2((float)dataWidth / 2f * cellSize, (float)dataHeight / 2f * cellSize);
		max = center + new Vector2((float)dataWidth / 2f * cellSize, (float)dataHeight / 2f * cellSize);
		size = max - min;
	}

	public Cells2D<T> GetCropped(int startX, int endX, int startY, int endY)
	{
		Vector2 worldPos = GetWorldPos((startX, startY));
		Vector2 wMax = GetWorldPos((endX, endY));
		Cells2D<T> cropped = new Cells2D<T>((worldPos + wMax) / 2f, cellSize, endX - startX + 1, endY - startY + 1);
		for (int x = startX; x <= endX; x++)
		{
			for (int y = startY; y <= endY; y++)
			{
				cropped.Set((x - startX, y - startY), Get((x, y)));
			}
		}
		return cropped;
	}

	public Vector2 GetWorldPos((int, int) indices)
	{
		int x = indices.Item1;
		int y = indices.Item2;
		float x2 = center.x + (float)(x - dataWidth / 2) * cellSize;
		float yPos = center.y + (float)(y - dataHeight / 2) * cellSize;
		return new Vector2(x2, yPos);
	}

	public Vector2 GetNormalizedPos(Vector2 worldPos)
	{
		float x = (worldPos.x - min.x) / ((float)dataWidth * cellSize);
		float yPos = (worldPos.y - min.y) / ((float)dataHeight * cellSize);
		return new Vector2(x, yPos);
	}

	public T Get((int, int) indices)
	{
		var (x, y) = indices;
		return data[x + y * dataWidth];
	}

	public void Set((int, int) indices, T value)
	{
		var (x, y) = indices;
		data[x + y * dataWidth] = value;
	}

	public (int, int) GetClosestCell(Vector2 worldPos)
	{
		int value = Mathf.RoundToInt((worldPos.x - center.x + (float)dataWidth * cellSize * 0.5f) / cellSize);
		int yIndex = Mathf.RoundToInt((worldPos.y - center.y + (float)dataHeight * cellSize * 0.5f) / cellSize);
		int item = Mathf.Clamp(value, 0, dataWidth - 1);
		yIndex = Mathf.Clamp(yIndex, 0, dataHeight - 1);
		return (item, yIndex);
	}

	public bool IsInBounds((int, int) indices)
	{
		if (indices.Item1 >= 0 && indices.Item2 >= 0 && indices.Item1 < dataWidth)
		{
			return indices.Item2 < dataHeight;
		}
		return false;
	}

	public void FloodFill((int, int) start, Func<(int, int), bool> func)
	{
		Stack<(int, int)> stack = new Stack<(int, int)>();
		stack.Push(start);
		while (stack.Count > 0)
		{
			(int, int) current = stack.Pop();
			if (IsInBounds(current) && func(current))
			{
				stack.Push((current.Item1 + 1, current.Item2));
				stack.Push((current.Item1 - 1, current.Item2));
				stack.Push((current.Item1, current.Item2 + 1));
				stack.Push((current.Item1, current.Item2 - 1));
			}
		}
	}
}
