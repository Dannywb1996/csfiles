public class CameraModifierZoom : CameraModifierBase
{
	public float zoomIndex;

	public new CameraModifierZoom Apply()
	{
		return (CameraModifierZoom)base.Apply();
	}
}
