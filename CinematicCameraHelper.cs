using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[LogicUpdatePriority(500)]
public class CinematicCameraHelper : LogicBehaviour
{
	public CanvasGroup entireUICanvasGroup;

	public GameObject castIndicatorObject;

	public GameObject[] cinematicCameras;

	public CinemachineTargetGroup groupMyHero;

	public CinemachineTargetGroup groupAllHeroes;

	public CinemachineTargetGroup groupHeroesAndMonsters;

	public int maxTargets;

	public float weightGainPerSecond;

	public float radius = 3f;

	public int freeCamIndex;

	public Transform freeCamTargetTransform;

	public Transform freeCamTransform;

	public float freeCamSmoothTime;

	public float freeCamRotSmoothTime;

	public float freeCamFastSpeed;

	public float freeCamSpeed;

	public float freeCamSlowSpeed;

	public float freeCamMouseSensitivity;

	private int _currentCameraIndex = -1;

	private Hero _hiddenHero;

	private bool _didDisableControls;

	private Vector3 _cvPos;

	private float _cvRotX;

	private float _cvRotY;

	private float _cvRotZ;

	private List<Transform> _allEntities = new List<Transform>();

	public bool isCinematicHelperEnabled { get; set; }

	private void Awake()
	{
		CinemachineTargetGroup.Target[] a = new CinemachineTargetGroup.Target[maxTargets];
		for (int i = 0; i < a.Length; i++)
		{
			a[i].radius = radius;
			a[i].weight = 0f;
		}
		groupAllHeroes.m_Targets = a;
		CinemachineTargetGroup.Target[] b = new CinemachineTargetGroup.Target[maxTargets];
		for (int j = 0; j < b.Length; j++)
		{
			b[j].radius = radius;
			b[j].weight = 0f;
		}
		groupHeroesAndMonsters.m_Targets = b;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!isCinematicHelperEnabled || ControlManager.IsInputFieldFocused())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Keypad1))
		{
			ManagerBase<CursorManager>.instance.disableSoftwareCursor = !ManagerBase<CursorManager>.instance.disableSoftwareCursor;
		}
		if (Input.GetKeyDown(KeyCode.Keypad2))
		{
			entireUICanvasGroup.alpha = ((entireUICanvasGroup.alpha > 0.1f) ? 0f : 1f);
			castIndicatorObject.SetActive(entireUICanvasGroup.alpha > 0.1f);
		}
		if (Input.GetKeyDown(KeyCode.Keypad3))
		{
			_currentCameraIndex++;
			if (_currentCameraIndex >= cinematicCameras.Length)
			{
				_currentCameraIndex = -1;
			}
			for (int i = 0; i < cinematicCameras.Length; i++)
			{
				cinematicCameras[i].SetActive(_currentCameraIndex == i);
			}
			if (_currentCameraIndex == freeCamIndex)
			{
				freeCamTargetTransform.transform.SetPositionAndRotation(Dew.mainCamera.transform.position, Dew.mainCamera.transform.rotation);
				freeCamTransform.transform.SetPositionAndRotation(freeCamTargetTransform.position, freeCamTargetTransform.rotation);
				ManagerBase<ControlManager>.instance.DisableCharacterControls();
				_didDisableControls = true;
				ManagerBase<CameraManager>.instance.disableSeeThrough = true;
			}
			else if (_didDisableControls)
			{
				ManagerBase<ControlManager>.instance.EnableCharacterControls();
				_didDisableControls = false;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				ManagerBase<CameraManager>.instance.disableSeeThrough = false;
			}
		}
		if (Input.GetKeyDown(KeyCode.Keypad4))
		{
			if (_hiddenHero != null)
			{
				_hiddenHero.Visual.EnableRenderersLocal();
				_hiddenHero = null;
			}
			else
			{
				DewPlayer.local.hero.Visual.DisableRenderersLocal();
				_hiddenHero = DewPlayer.local.hero;
			}
		}
		if (_currentCameraIndex == freeCamIndex)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			float speed = (Input.GetKey(KeyCode.LeftShift) ? freeCamFastSpeed : (Input.GetKey(KeyCode.LeftControl) ? freeCamSlowSpeed : freeCamSpeed));
			if (Input.GetKey(KeyCode.W))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * speed * freeCamTransform.forward;
			}
			if (Input.GetKey(KeyCode.A))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * speed * -freeCamTransform.right;
			}
			if (Input.GetKey(KeyCode.S))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * speed * -freeCamTransform.forward;
			}
			if (Input.GetKey(KeyCode.D))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * speed * freeCamTransform.right;
			}
			if (Input.GetKey(KeyCode.Space))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * speed * Vector3.up;
			}
			if (Input.GetKey(KeyCode.C))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * speed * Vector3.down;
			}
			Vector3 eulerAngles = freeCamTargetTransform.rotation.eulerAngles;
			float x = eulerAngles.x - Input.GetAxis("Mouse Y") * freeCamMouseSensitivity;
			if (eulerAngles.x < 90f && x > 89f)
			{
				x = 89f;
			}
			if (eulerAngles.x > 90f && x < 271f)
			{
				x = 271f;
			}
			float y = eulerAngles.y + Input.GetAxis("Mouse X") * freeCamMouseSensitivity;
			int z = 0;
			freeCamTargetTransform.rotation = Quaternion.Euler(x, y, z);
			Vector3 currentEuler = freeCamTransform.rotation.eulerAngles;
			freeCamTransform.position = Vector3.SmoothDamp(freeCamTransform.position, freeCamTargetTransform.position, ref _cvPos, freeCamSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime);
			freeCamTransform.rotation = Quaternion.Euler(Mathf.SmoothDampAngle(currentEuler.x, x, ref _cvRotX, freeCamRotSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime), Mathf.SmoothDampAngle(currentEuler.y, y, ref _cvRotY, freeCamRotSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime), Mathf.SmoothDampAngle(currentEuler.z, z, ref _cvRotZ, freeCamRotSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime));
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (_currentCameraIndex < 0)
		{
			return;
		}
		if (DewPlayer.local != null && DewPlayer.local.hero != null)
		{
			groupMyHero.m_Targets[0].target = DewPlayer.local.hero.transform;
		}
		_allEntities.Clear();
		for (int i = 0; i < NetworkedManagerBase<ActorManager>.instance.allEntities.Count; i++)
		{
			Entity e = NetworkedManagerBase<ActorManager>.instance.allEntities[i];
			if (e is Hero || e is Monster)
			{
				_allEntities.Add(e.transform);
			}
		}
		CopyList(_allEntities, groupHeroesAndMonsters);
		CopyList(NetworkedManagerBase<ActorManager>.instance.allHeroes, groupAllHeroes);
	}

	private void CopyList(IReadOnlyList<Component> from, CinemachineTargetGroup to)
	{
		to.m_Targets[0].target = null;
		for (int i = 0; i < to.m_Targets.Length; i++)
		{
			to.m_Targets[i].radius = radius;
			to.m_Targets[i].target = ((i < from.Count) ? from[i].transform : to.m_Targets[0].target);
			if (i >= from.Count)
			{
				to.m_Targets[i].weight = 0f;
			}
			else
			{
				to.m_Targets[i].weight = Mathf.MoveTowards(to.m_Targets[i].weight, 1f, weightGainPerSecond * Time.deltaTime);
			}
		}
	}
}
