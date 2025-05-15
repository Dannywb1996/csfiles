using Mirror;

public struct SyncedNetworkBehaviour
{
	public uint netId;

	public byte componentIndex;

	public SyncedNetworkBehaviour(uint netId, int componentIndex)
	{
		this = default(SyncedNetworkBehaviour);
		this.netId = netId;
		this.componentIndex = (byte)componentIndex;
	}

	public bool Equals(SyncedNetworkBehaviour other)
	{
		if (other.netId == netId)
		{
			return other.componentIndex == componentIndex;
		}
		return false;
	}

	public bool Equals(uint netId, int componentIndex)
	{
		if (this.netId == netId)
		{
			return this.componentIndex == componentIndex;
		}
		return false;
	}

	public override string ToString()
	{
		return $"[netId:{netId} compIndex:{componentIndex}]";
	}

	public static implicit operator NetworkBehaviour(SyncedNetworkBehaviour snb)
	{
		if (snb.netId == 0)
		{
			return null;
		}
		if (NetworkServer.active && NetworkServer.spawned.TryGetValue(snb.netId, out var sid))
		{
			return sid.NetworkBehaviours[snb.componentIndex];
		}
		if (NetworkClient.spawned.TryGetValue(snb.netId, out var id))
		{
			return id.NetworkBehaviours[snb.componentIndex];
		}
		return null;
	}

	public static implicit operator SyncedNetworkBehaviour(NetworkBehaviour nb)
	{
		return new SyncedNetworkBehaviour(nb.netId, nb.ComponentIndex);
	}
}
