using UnityEngine;

[ExecuteAlways]
public class AttachToCamera : MonoBehaviour
{
	public bool preserveLocalPosition;

	public bool preserveLocalRotation;

	private Vector3 _localPos;

	private Quaternion _localRot = Quaternion.identity;

	private void Awake()
	{
		if (preserveLocalPosition)
		{
			_localPos = base.transform.localPosition;
		}
		if (preserveLocalRotation)
		{
			_localRot = base.transform.localRotation;
		}
	}

	private void LateUpdate()
	{
		if (!(Dew.mainCamera == null))
		{
			Transform t = Dew.mainCamera.transform;
			base.transform.SetPositionAndRotation(t.TransformPoint(_localPos), _localRot * t.rotation);
		}
	}
}
