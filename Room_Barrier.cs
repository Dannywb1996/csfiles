using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class Room_Barrier : DewNetworkBehaviour
{
	private struct Ad_BarrierPassThrough
	{
		public float lastUseTime;
	}

	[SyncVar(hook = "OnOpenChanged")]
	[SerializeField]
	private bool _isOpen;

	public GameObject openEffect;

	public GameObject closedEffect;

	public bool canPassThroughOneWay;

	public DewCollider passThroughCollider;

	private NavMeshObstacle _navObstacle;

	private Collider _collider;

	public bool isOpen => _isOpen;

	public bool Network_isOpen
	{
		get
		{
			return _isOpen;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _isOpen, 1uL, OnOpenChanged);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_navObstacle = GetComponent<NavMeshObstacle>();
		_collider = GetComponent<Collider>();
		if (!canPassThroughOneWay)
		{
			return;
		}
		passThroughCollider.onEntityEnter.AddListener(delegate(Entity e)
		{
			if (base.isServer && !isOpen && (!e.TryGetData<Ad_BarrierPassThrough>(out var data) || !(Time.time - data.lastUseTime < 2f)) && !e.Visual.isSpawning)
			{
				LetEntityPassThrough(e);
			}
		});
		passThroughCollider.receiveEntityCallbacks = true;
		passThroughCollider.UpdateProxyCollider();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		OnOpenChanged(_isOpen, _isOpen);
	}

	private void OnOpenChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			if (openEffect != null)
			{
				FxPlay(openEffect);
			}
			if (closedEffect != null)
			{
				FxStop(closedEffect);
			}
		}
		else
		{
			if (openEffect != null)
			{
				FxStop(openEffect);
			}
			if (closedEffect != null)
			{
				FxPlay(closedEffect);
			}
		}
		_navObstacle.enabled = !newVal;
		_collider.enabled = !newVal;
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Barrier::Open()' called when server was not active");
		}
		else
		{
			Network_isOpen = true;
		}
	}

	[Server]
	public void Close()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Barrier::Close()' called when server was not active");
		}
		else
		{
			if (!_isOpen)
			{
				return;
			}
			Network_isOpen = false;
			if (!canPassThroughOneWay)
			{
				return;
			}
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> entities = passThroughCollider.GetEntities(out handle);
			for (int i = 0; i < entities.Length; i++)
			{
				Entity e = entities[i];
				if (e.Visual.isSpawning)
				{
					return;
				}
				LetEntityPassThrough(e);
			}
			handle.Return();
		}
	}

	[Server]
	public void Toggle()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Barrier::Toggle()' called when server was not active");
		}
		else
		{
			Network_isOpen = !_isOpen;
		}
	}

	private void LetEntityPassThrough(Entity e)
	{
		e.RemoveData<Ad_BarrierPassThrough>();
		e.AddData(new Ad_BarrierPassThrough
		{
			lastUseTime = Time.time
		});
		e.CreateStatusEffect(e, default(CastInfo), delegate(Se_BarrierPassThrough s)
		{
			Vector3 agentPosition = e.agentPosition;
			agentPosition = base.transform.InverseTransformPoint(agentPosition);
			agentPosition.z *= -1f;
			agentPosition = base.transform.TransformPoint(agentPosition);
			agentPosition = Dew.GetPositionOnGround(agentPosition);
			RoomSection componentInParent = GetComponentInParent<RoomSection>();
			if (componentInParent != null)
			{
				agentPosition = Dew.GetValidAgentDestination_Closest(componentInParent.pathablePivot, agentPosition);
			}
			else
			{
				s.destination = agentPosition;
			}
			s.destination = agentPosition;
		});
	}

	private void OnDrawGizmos()
	{
		if (canPassThroughOneWay)
		{
			Vector3 vector = base.transform.position + base.transform.up * 2f;
			DewGizmos.DrawArrow(vector, vector + base.transform.forward * 4f, Color.cyan, 1f);
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(_isOpen);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(_isOpen);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _isOpen, OnOpenChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _isOpen, OnOpenChanged, reader.ReadBool());
		}
	}
}
