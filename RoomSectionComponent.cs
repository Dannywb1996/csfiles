using UnityEngine;

[LogicUpdatePriority(200)]
[RequireComponent(typeof(RoomSection))]
public class RoomSectionComponent : LogicBehaviour
{
	public RoomSection section { get; private set; }

	protected virtual void Awake()
	{
		section = GetComponent<RoomSection>();
	}
}
