using UnityEngine;

[ExecuteAlways]
public class DewCamera : ManagerBase<DewCamera>
{
	public Camera mainCamera;

	public Camera uiCamera;

	public override bool shouldRegisterUpdates => false;

	protected override void Awake()
	{
		base.Awake();
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
	}

	private void Update()
	{
		uiCamera.transform.position = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, -100f);
		uiCamera.orthographicSize = (float)Mathf.Min(Screen.width, Screen.height) * 0.5f;
	}
}
