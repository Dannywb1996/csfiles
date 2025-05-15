using UnityEngine;

public class AttachToHero : MonoBehaviour
{
	private void LateUpdate()
	{
		if (!(ManagerBase<CameraManager>.softInstance == null) && !ManagerBase<CameraManager>.softInstance.focusedEntity.IsNullOrInactive())
		{
			base.transform.SetPositionAndRotation(Dew.GetPositionOnGround(ManagerBase<CameraManager>.softInstance.focusedEntity.position), ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		}
	}
}
