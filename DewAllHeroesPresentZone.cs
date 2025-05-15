using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DewCollider))]
public class DewAllHeroesPresentZone : DewNetworkBehaviour
{
	public bool once = true;

	public UnityEvent onActivate;

	private List<Hero> _currentMembers = new List<Hero>();

	private float _lastCheckTime;

	private int _lastNumOfHeroes;

	private DewCollider _collider;

	public IReadOnlyList<Hero> currentMembers => _currentMembers;

	protected override void Awake()
	{
		base.Awake();
		_collider = GetComponent<DewCollider>();
	}

	private void Update()
	{
		if (!base.isServer || !(Time.time - _lastCheckTime > 1f))
		{
			return;
		}
		_lastCheckTime = Time.time;
		int required = 0;
		int current = 0;
		_currentMembers.Clear();
		foreach (Hero h in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (!h.isKnockedOut)
			{
				required++;
				if (_collider.OverlapPoint(h.position.ToXY()))
				{
					current++;
					_currentMembers.Add(h);
				}
			}
		}
		if (current > 0 && current != _lastNumOfHeroes && current < required)
		{
			ShowNeedPresenceMessage(current, required);
		}
		_lastNumOfHeroes = current;
		if (required > 0 && required <= current)
		{
			try
			{
				onActivate?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			if (once)
			{
				base.enabled = false;
			}
		}
	}

	[ClientRpc]
	private void ShowNeedPresenceMessage(int curr, int max)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(curr);
		writer.WriteInt(max);
		SendRPCInternal("System.Void DewAllHeroesPresentZone::ShowNeedPresenceMessage(System.Int32,System.Int32)", 590532284, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_ShowNeedPresenceMessage__Int32__Int32(int curr, int max)
	{
		InGameUIManager.instance.ShowCenterMessage(CenterMessageType.General, "InGame_Message_NeedAllPlayersPresence", new object[2] { curr, max });
	}

	protected static void InvokeUserCode_ShowNeedPresenceMessage__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC ShowNeedPresenceMessage called on server.");
		}
		else
		{
			((DewAllHeroesPresentZone)obj).UserCode_ShowNeedPresenceMessage__Int32__Int32(reader.ReadInt(), reader.ReadInt());
		}
	}

	static DewAllHeroesPresentZone()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(DewAllHeroesPresentZone), "System.Void DewAllHeroesPresentZone::ShowNeedPresenceMessage(System.Int32,System.Int32)", InvokeUserCode_ShowNeedPresenceMessage__Int32__Int32);
	}
}
