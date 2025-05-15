using UnityEngine;

[RequireComponent(typeof(Room))]
public class RoomComponent : DewNetworkBehaviour
{
	public Room room { get; private set; }

	public bool isRoomActive { get; internal set; }

	protected override void Awake()
	{
		base.Awake();
		room = GetComponent<Room>();
	}

	public virtual void OnRoomStartServer(WorldNodeSaveData save)
	{
	}

	public virtual void OnRoomStart(bool isRevisit)
	{
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (isRoomActive)
		{
			ActiveLogicUpdate(dt);
		}
	}

	protected virtual void ActiveLogicUpdate(float dt)
	{
	}

	public virtual void OnRoomStop()
	{
	}

	public virtual void OnRoomStopServer(WorldNodeSaveData save)
	{
	}

	private void MirrorProcessed()
	{
	}
}
