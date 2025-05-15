using Cinemachine;
using UnityEngine;

[ExecuteAlways]
public class MapFakeCamera : MonoBehaviour
{
	public Transform followTransform;

	public CinemachineVirtualCamera vcam;

	public Vector3 originalOffset;

	private CinemachineTransposer _body;

	public static int currentCamAngleIndex;

	private int _needsGameViewRepaintFrame;
}
