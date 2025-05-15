using UnityEngine;

public class CameraModifierOffset : CameraModifierBase
{
	public Vector3 offset;

	public new CameraModifierOffset Apply()
	{
		return (CameraModifierOffset)base.Apply();
	}
}
