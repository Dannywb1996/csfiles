public class CameraModifierBase
{
	public CameraModifierBase Apply()
	{
		ManagerBase<CameraManager>.instance.AddLocalCameraModifier(this);
		return this;
	}

	public void Remove()
	{
		if (!(ManagerBase<CameraManager>.instance == null))
		{
			ManagerBase<CameraManager>.instance.RemoveLocalCameraModifier(this);
		}
	}
}
