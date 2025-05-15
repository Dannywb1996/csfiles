using UnityEngine;

public class RotateTransform : MonoBehaviour
{
	public Vector3 speed;

	public bool useUnscaledTime;

	private void Update()
	{
		base.transform.Rotate(speed * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));
	}
}
