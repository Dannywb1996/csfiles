using UnityEngine;

public class BoneOffset : MonoBehaviour
{
	public Vector3 localPosition;

	public Vector3 localRotation;

	public Vector3 localScale = Vector3.one;

	private void LateUpdate()
	{
		base.transform.localScale = localScale;
		base.transform.localPosition += localPosition;
		base.transform.localRotation *= Quaternion.Euler(localRotation);
	}
}
