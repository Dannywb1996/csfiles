using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	private void LateUpdate()
	{
		if (!(Dew.mainCamera == null))
		{
			base.transform.rotation = Dew.mainCamera.transform.rotation;
		}
	}
}
