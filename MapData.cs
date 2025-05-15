using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData
{
	private const float MapMarginDistance = 5f;

	private const float NodeDistanceFromWall = 2.5f;

	public Cells2D<MapCellType> cells;

	private FlatTupleListWrapper<int> _ipnWrapper;

	[SerializeField]
	private List<int> _ipnFlat;

	private FlatTupleListWrapper<int> _opnWrapper;

	[SerializeField]
	private List<int> _opnFlat;

	public float area;

	public IReadOnlyList<(int, int)> innerPropNodeIndices
	{
		get
		{
			if (_ipnWrapper == null)
			{
				_ipnWrapper = new FlatTupleListWrapper<int>();
			}
			if (_ipnWrapper.list != _ipnFlat)
			{
				_ipnWrapper.list = _ipnFlat;
			}
			return _ipnWrapper;
		}
	}

	public IReadOnlyList<(int, int)> outerPropNodeIndices
	{
		get
		{
			if (_opnWrapper == null)
			{
				_opnWrapper = new FlatTupleListWrapper<int>();
			}
			if (_opnWrapper.list != _opnFlat)
			{
				_opnWrapper.list = _opnFlat;
			}
			return _opnWrapper;
		}
	}

	public MapData()
	{
	}

	public MapData(Cells2D<MapCellType> raw, int minX, int maxX, int minY, int maxY)
	{
		int margin = Mathf.RoundToInt(5f / raw.cellSize);
		cells = raw.GetCropped(minX - margin, maxX + margin, minY - margin, maxY + margin);
		_ipnFlat = new List<int>();
		_opnFlat = new List<int>();
		float cellArea = cells.cellSize * cells.cellSize;
		int nodeDistFromWall = Mathf.RoundToInt(2.5f / cells.cellSize);
		nodeDistFromWall = Mathf.Max(nodeDistFromWall, 1);
		for (int y = 0; y < cells.dataHeight; y++)
		{
			for (int x = 0; x < cells.dataWidth; x++)
			{
				if (cells.Get((x, y)) != MapCellType.Playable)
				{
					continue;
				}
				area += cellArea;
				bool tooCloseToWall = false;
				for (int yy = y - nodeDistFromWall; yy <= y + nodeDistFromWall; yy++)
				{
					for (int xx = x - nodeDistFromWall; xx <= x + nodeDistFromWall; xx++)
					{
						if (cells.Get((xx, yy)) != MapCellType.Playable)
						{
							tooCloseToWall = true;
							break;
						}
					}
					if (tooCloseToWall)
					{
						break;
					}
				}
				if (!tooCloseToWall)
				{
					_ipnFlat.Add(x);
					_ipnFlat.Add(y);
				}
				else
				{
					_opnFlat.Add(x);
					_opnFlat.Add(y);
				}
			}
		}
	}
}
