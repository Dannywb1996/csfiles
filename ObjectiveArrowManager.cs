using System;
using UnityEngine;

[LogicUpdatePriority(3000)]
public class ObjectiveArrowManager : ManagerBase<ObjectiveArrowManager>
{
	public Vector3? objectivePosition;

	public GameObject arrowPivot;

	private void Start()
	{
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnIsInTransitionChanged += new Action<bool>(ClientEventOnIsInTransitionChanged);
	}

	private void ClientEventOnIsInTransitionChanged(bool obj)
	{
		if (obj)
		{
			objectivePosition = null;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		Entity focused = ManagerBase<CameraManager>.softInstance.focusedEntity;
		bool shouldShow = !focused.IsNullInactiveDeadOrKnockedOut() && objectivePosition.HasValue;
		arrowPivot.SetActive(shouldShow);
		if (shouldShow)
		{
			Quaternion rot = Quaternion.LookRotation(objectivePosition.Value - focused.position).Flattened();
			arrowPivot.transform.SetPositionAndRotation(focused.position, rot);
		}
	}
}
