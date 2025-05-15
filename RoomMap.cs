using Unity.Collections;
using UnityEngine;

public class RoomMap : RoomComponent
{
	private const int MaxMapSteps = 512;

	private const float FogOfWarRevealRadius = 24f;

	private const float FogOfWarTickInterval = 0.25f;

	private const float FogOfWarRevealDecaySpeed = 0.4f;

	private const float FogOfWarVisitedAreaVisibility = 0.25f;

	private const float FogOfWarCellSize = 3f;

	public Texture2D mapTexture;

	public float density = 0.5f;

	[HideInInspector]
	public MapData mapData;

	private NativeArray<float> _fowRaw;

	private int _radius;

	private int _fowWidth;

	private int _fowHeight;

	private float _nextFowTickTime;

	public Texture2D fowTexture { get; private set; }

	public override void OnRoomStart(bool isRevisit)
	{
		base.OnRoomStart(isRevisit);
		_fowWidth = Mathf.RoundToInt(mapData.cells.size.x / 3f);
		_fowHeight = Mathf.RoundToInt(mapData.cells.size.y / 3f);
		fowTexture = new Texture2D(_fowWidth, _fowHeight, TextureFormat.RFloat, mipChain: false);
		Color[] cols = new Color[_fowWidth * _fowHeight];
		for (int i = 0; i < cols.Length; i++)
		{
			cols[i] = (isRevisit ? Color.red : Color.black);
		}
		fowTexture.SetPixels(cols);
		fowTexture.Apply();
		_fowRaw = fowTexture.GetRawTextureData<float>();
		_radius = Mathf.RoundToInt(8f);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!(Time.time > _nextFowTickTime))
		{
			return;
		}
		_nextFowTickTime = Time.time + 0.25f;
		for (int i = 0; i < _fowRaw.Length; i++)
		{
			if (_fowRaw[i] > 0.25f)
			{
				_fowRaw[i] = Mathf.MoveTowards(_fowRaw[i], 0.25f, 0.1f);
			}
		}
		foreach (Hero h in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			Vector2 pos = GetFoWPosFromWorldPos(h.position.ToXY());
			RevealFog(pos.x, pos.y);
		}
		fowTexture.Apply();
	}

	public bool IsWorldPosVisible(Vector3 worldPos)
	{
		if (!_fowRaw.IsCreated)
		{
			return false;
		}
		Vector2Int fowPos = GetFoWPosIntFromWorldPos(worldPos.ToXY());
		int i = GetFoWIndex(fowPos.x, fowPos.y);
		return _fowRaw[i] > 0.3f;
	}

	public bool IsWorldPosVisited(Vector3 worldPos)
	{
		if (!_fowRaw.IsCreated)
		{
			return false;
		}
		Vector2Int fowPos = GetFoWPosIntFromWorldPos(worldPos.ToXY());
		int i = GetFoWIndex(fowPos.x, fowPos.y);
		return _fowRaw[i] > 0.2f;
	}

	private Vector2 GetFoWPosFromWorldPos(Vector2 worldPos)
	{
		Vector2 pos = mapData.cells.GetNormalizedPos(worldPos);
		return new Vector2(pos.x * (float)_fowWidth, pos.y * (float)_fowHeight);
	}

	private Vector2Int GetFoWPosIntFromWorldPos(Vector2 worldPos)
	{
		Vector2 pos = mapData.cells.GetNormalizedPos(worldPos);
		return new Vector2Int(Mathf.RoundToInt(pos.x * (float)_fowWidth), Mathf.RoundToInt(pos.y * (float)_fowHeight));
	}

	private void RevealFog(float cenX, float cenY)
	{
		Vector2Int processCenter = new Vector2Int(Mathf.RoundToInt(cenX), Mathf.RoundToInt(cenY));
		int radSqrFrom = (_radius - 2) * (_radius - 2);
		int radSqrTo = _radius * _radius;
		int radSqrDiff = radSqrTo - radSqrFrom;
		int processRadius = _radius + 1;
		for (int y = processCenter.y - processRadius; y <= processCenter.y + processRadius; y++)
		{
			for (int x = processCenter.x - processRadius; x <= processCenter.x + processRadius; x++)
			{
				float sqrDist = Vector2.SqrMagnitude(new Vector2((float)x - cenX, (float)y - cenY));
				if (!(sqrDist > (float)radSqrTo))
				{
					int i = GetFoWIndex(x, y);
					if (sqrDist > (float)radSqrFrom)
					{
						float target = 1f - (sqrDist - (float)radSqrFrom) / (float)radSqrDiff;
						_fowRaw[i] = Mathf.Max(_fowRaw[i], target);
					}
					else
					{
						_fowRaw[i] = 1f;
					}
				}
			}
		}
	}

	private int GetFoWIndex(int x, int y)
	{
		x = Mathf.Clamp(x, 0, _fowWidth - 1);
		y = Mathf.Clamp(y, 0, _fowHeight - 1);
		return x + y * _fowWidth;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (fowTexture != null)
		{
			Object.Destroy(fowTexture);
			fowTexture = null;
		}
		if (_fowRaw.IsCreated)
		{
			_fowRaw.Dispose();
		}
	}

	private void MirrorProcessed()
	{
	}
}
