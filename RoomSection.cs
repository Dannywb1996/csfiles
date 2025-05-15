using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Section_Monsters))]
[RequireComponent(typeof(Section_Props))]
public class RoomSection : LogicBehaviour, IPlayerPathableArea
{
	public Vector2[] vectors = new Vector2[6]
	{
		new Vector2(1f, 0f) * 5f,
		new Vector2(0.5f, 0.87f) * 5f,
		new Vector2(-0.5f, 0.87f) * 5f,
		new Vector2(-1f, 0f) * 5f,
		new Vector2(-0.5f, -0.87f) * 5f,
		new Vector2(0.5f, -0.87f) * 5f
	};

	[SerializeField]
	[HideInInspector]
	private Vector2[] _points;

	private PointsToTriangleVertices _wrapper;

	[SerializeField]
	[HideInInspector]
	private float[] _triAreas;

	[SerializeField]
	[HideInInspector]
	private RoomSectionFloatDictionary _distanceToSections;

	private readonly List<Entity> _entities = new List<Entity>();

	private readonly List<Entity> _entitiesWithSectionTriggeringDisabled = new List<Entity>();

	public UnityEvent onEntitiesChanged = new UnityEvent();

	public UnityEvent<Entity> onEntityEnter = new UnityEvent<Entity>();

	public UnityEvent<Entity> onEntityExit = new UnityEvent<Entity>();

	public UnityEvent onEnterFirstTime = new UnityEvent();

	public UnityEvent onEveryonePresent = new UnityEvent();

	public bool clearRoomOnEnterFirstTime;

	public bool waitForAggroedEnemiesToDie = true;

	private bool _didInvokeOnEnterFirstTime;

	private bool _didInvokeOnEveryonePresent;

	private DewCollider _dewCollider;

	public const float BanSpotsDistance = 3.25f;

	private const float NodeWallMinDistance = 1f;

	private const int NodeRetryLimit = 100;

	private const float RandomNodeMinSqrDistance = 6.25f;

	private const float RandomNodeBestSqrDistance = 16f;

	[HideInInspector]
	[SerializeField]
	private List<Vector3> _nodes = new List<Vector3>();

	private List<int> _unusedNodeIndices = new List<int>();

	private List<Vector2> _usedNodePositions = new List<Vector2>();

	public Section_Monsters monsters { get; private set; }

	public Section_Props props { get; private set; }

	[field: SerializeField]
	public float area { get; private set; }

	public IReadOnlyList<Entity> entities => _entities;

	public Vector3 pathablePivot { get; private set; }

	public int numOfHeroes { get; private set; }

	Vector3 IPlayerPathableArea.pathablePosition => pathablePivot;

	private void Awake()
	{
		UpdatePathablePivot();
		monsters = GetComponent<Section_Monsters>();
		props = GetComponent<Section_Props>();
		onEnterFirstTime.AddListener(delegate
		{
			if (NetworkServer.active && clearRoomOnEnterFirstTime && !SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
			{
				StartCoroutine(Routine());
			}
		});
		ResetUsedNodeIndices();
		IEnumerator Routine()
		{
			if (waitForAggroedEnemiesToDie)
			{
				yield return Dew.WaitForAggroedEnemiesRoutine();
			}
			SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
		}
	}

	private void UpdatePathablePivot()
	{
		pathablePivot = Dew.GetPositionOnGround(base.transform.position);
	}

	private void OnValidate()
	{
		_points = new Vector2[vectors.Length * 3 - 6];
		PolygonTools.GetVerticesArray(vectors, ref _points);
		_wrapper = new PointsToTriangleVertices(_points);
		_triAreas = new float[_wrapper.Count / 3];
		area = 0f;
		for (int i = 0; i < _wrapper.Count / 3; i++)
		{
			Vector2 v = _wrapper[i * 3 + 1] - _wrapper[i * 3];
			Vector2 b = _wrapper[i * 3 + 2] - _wrapper[i * 3];
			float faceArea = Mathf.Abs(v.Cross(b));
			_triAreas[i] = faceArea;
			area += faceArea;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_dewCollider = base.gameObject.AddComponent<DewCollider>();
		_dewCollider.shape = DewCollider.ColliderShape.Polygon;
		_dewCollider.points = vectors;
		_dewCollider.receiveEntityCallbacks = true;
		_dewCollider.UpdateProxyCollider();
		GameManager.CallOnReady(delegate
		{
			if (!(_dewCollider == null))
			{
				_dewCollider.onEntityEnter.AddListener(HandleEntityEnter);
				_dewCollider.onEntityExit.AddListener(HandleEntityExit);
				ArrayReturnHandle<Entity> handle;
				ReadOnlySpan<Entity> readOnlySpan = _dewCollider.GetEntities(out handle, new CollisionCheckSettings
				{
					includeUncollidable = true
				});
				for (int i = 0; i < readOnlySpan.Length; i++)
				{
					Entity e = readOnlySpan[i];
					HandleEntityEnter(e);
				}
				handle.Return();
			}
		});
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!NetworkServer.active)
		{
			return;
		}
		for (int i = _entitiesWithSectionTriggeringDisabled.Count - 1; i >= 0; i--)
		{
			Entity e = _entitiesWithSectionTriggeringDisabled[i];
			if (!e.Status.isSectionTriggeringDisabled)
			{
				_entitiesWithSectionTriggeringDisabled.RemoveAt(i);
				AddEntity(e);
			}
		}
	}

	private void HandleEntityEnter(Entity e)
	{
		if (NetworkServer.active)
		{
			if ((bool)e.Status.isSectionTriggeringDisabled)
			{
				_entitiesWithSectionTriggeringDisabled.Add(e);
			}
			else
			{
				AddEntity(e);
			}
		}
	}

	private void AddEntity(Entity e)
	{
		e.section = this;
		e.lastSection = this;
		_entities.Add(e);
		UpdateResidentMetrics();
		try
		{
			onEntityEnter?.Invoke(e);
			onEntitiesChanged?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		if (!(e is Hero))
		{
			return;
		}
		if (!_didInvokeOnEnterFirstTime)
		{
			_didInvokeOnEnterFirstTime = true;
			try
			{
				onEnterFirstTime?.Invoke();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		if (!_didInvokeOnEveryonePresent)
		{
			CheckEveryonePresent();
		}
	}

	private void HandleEntityExit(Entity e)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		_entitiesWithSectionTriggeringDisabled.Remove(e);
		if (e.section == this)
		{
			e.section = null;
		}
		if (!_entities.Remove(e))
		{
			return;
		}
		UpdateResidentMetrics();
		try
		{
			onEntityExit?.Invoke(e);
			onEntitiesChanged?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void UpdateResidentMetrics()
	{
		numOfHeroes = 0;
		foreach (Entity entity in _entities)
		{
			if (entity is Hero)
			{
				numOfHeroes++;
			}
		}
	}

	private void CheckEveryonePresent()
	{
		foreach (Hero h in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!h.IsNullInactiveDeadOrKnockedOut() && !_entities.Contains(h))
			{
				return;
			}
		}
		_didInvokeOnEveryonePresent = true;
		try
		{
			onEveryonePresent?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_dewCollider != null)
		{
			UnityEngine.Object.Destroy(_dewCollider);
		}
		_entities.Clear();
	}

	public Vector3 GetRandomWorldPosition()
	{
		return base.transform.TransformPoint(GetRandomLocalPosition());
	}

	public Vector3 GetRandomLocalPosition()
	{
		Vector2[] selected = new Vector2[3];
		SelectRandomTriangle(ref selected);
		return PolygonTools.GetRandomPositionInTriangle(selected).ToXZ();
	}

	private int SelectRandomTriangle(ref Vector2[] verts)
	{
		if (_wrapper == null)
		{
			_wrapper = new PointsToTriangleVertices(_points);
		}
		float rng = UnityEngine.Random.Range(0f, area);
		for (int i = 0; i < _wrapper.Count / 3; i++)
		{
			if (rng < _triAreas[i])
			{
				verts[0] = _wrapper[i * 3];
				verts[1] = _wrapper[i * 3 + 1];
				verts[2] = _wrapper[i * 3 + 2];
				return i;
			}
			rng -= _triAreas[i];
		}
		verts[0] = _wrapper[^3];
		verts[1] = _wrapper[^2];
		verts[2] = _wrapper[^1];
		return _wrapper.Count / 3 - 1;
	}

	public float GetNavDistanceTo(RoomSection sec)
	{
		return _distanceToSections[sec];
	}

	public bool OverlapPoint(Vector2 point)
	{
		return _dewCollider.OverlapPoint(point);
	}

	public void ResetUsedNodeIndices()
	{
		_unusedNodeIndices.Clear();
		for (int i = 0; i < _nodes.Count; i++)
		{
			_unusedNodeIndices.Add(i);
		}
		_usedNodePositions.Clear();
	}

	public Vector3 GetGoodWanderPosition(Vector3 from)
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException("This is supposed to be used in runtime");
		}
		if (_nodes.Count == 0)
		{
			Debug.LogWarning("Section " + base.name + " of " + SingletonDewNetworkBehaviour<Room>.instance.name + " does not have node setup");
			return Dew.GetValidAgentDestination_Closest(from, GetRandomWorldPosition());
		}
		return Dew.GetValidAgentDestination_Closest(from, Dew.SelectBestWithScore(_nodes, GetScore, 0.4f));
		float GetScore(Vector3 pos, int index)
		{
			float score = 1f;
			for (int i = 0; i < _usedNodePositions.Count + 1; i++)
			{
				Vector2 current = ((i == _usedNodePositions.Count) ? from.ToXY() : _usedNodePositions[i]);
				float sqrDist = Vector2.SqrMagnitude(pos.ToXY() - current);
				if (!(sqrDist > 16f))
				{
					score = ((!(sqrDist > 6.25f)) ? Mathf.Min(score, -1f - (6.25f - sqrDist) * 3f) : Mathf.Min(score, (sqrDist - 6.25f) / 9.75f));
				}
			}
			return score;
		}
	}

	public bool TryGetGoodNodePosition(out Vector3 position)
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException("This is supposed to be used in runtime");
		}
		if (_nodes.Count == 0)
		{
			throw new InvalidOperationException("Section " + base.name + " of " + SingletonDewNetworkBehaviour<Room>.instance.name + " does not have node setup");
		}
		if (_unusedNodeIndices.Count == 0)
		{
			position = _nodes[UnityEngine.Random.Range(0, _nodes.Count)];
			return false;
		}
		int bestIndex = 0;
		float bestScore = float.NegativeInfinity;
		for (int i = 0; i < _nodes.Count; i++)
		{
			Vector3 item = _nodes[i];
			float score = GetScore(item, i);
			score *= 1f + UnityEngine.Random.Range(-0.1f, 0.1f);
			if (score > bestScore)
			{
				bestScore = score;
				bestIndex = i;
			}
		}
		position = _nodes[bestIndex];
		_unusedNodeIndices.Remove(bestIndex);
		_usedNodePositions.Add(position.ToXY());
		if (bestScore < 0f)
		{
			return false;
		}
		return true;
		float GetScore(Vector3 pos, int index)
		{
			float score2 = 1f;
			for (int j = 0; j < _usedNodePositions.Count; j++)
			{
				float sqrDist = Vector2.SqrMagnitude(pos.ToXY() - _usedNodePositions[j]);
				if (!(sqrDist > 16f))
				{
					score2 = ((!(sqrDist > 6.25f)) ? Mathf.Min(score2, -1f - (6.25f - sqrDist) * 3f) : Mathf.Min(score2, (sqrDist - 6.25f) / 9.75f));
				}
			}
			return score2;
		}
	}

	public Vector3 GetAnyRandomNode()
	{
		return _nodes[UnityEngine.Random.Range(0, _nodes.Count)];
	}
}
