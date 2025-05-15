using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[LogicUpdatePriority(400)]
public class DewCanvas : LogicBehaviour, ISettingsChangedCallback
{
	private CanvasScaler _scaler;

	private Canvas _canvas;

	private float _originalScale;

	private Vector2 _lastScreenSize;

	private void Awake()
	{
		if (!(_scaler != null))
		{
			DoInit();
		}
	}

	private void DoInit()
	{
		_scaler = GetComponent<CanvasScaler>();
		_canvas = GetComponent<Canvas>();
		_originalScale = _scaler.scaleFactor;
	}

	private void Start()
	{
		Vector2 currentSize = new Vector2(Screen.width, Screen.height);
		_lastScreenSize = currentSize;
		UpdateScale();
		_canvas.worldCamera = ManagerBase<DewCamera>.instance.uiCamera;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (_canvas.worldCamera == null && ManagerBase<DewCamera>.softInstance != null)
		{
			_canvas.worldCamera = ManagerBase<DewCamera>.softInstance.uiCamera;
		}
		Vector2 currentSize = new Vector2(Screen.width, Screen.height);
		if (_lastScreenSize != currentSize)
		{
			_lastScreenSize = currentSize;
			UpdateScaleMultiple();
		}
	}

	private void UpdateScaleMultiple()
	{
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
				UpdateScale();
			}
		}
	}

	private void UpdateScale()
	{
		if (_scaler == null)
		{
			DoInit();
		}
		float factor = Mathf.Clamp((float)Screen.height / 1440f, 0.1f, float.PositiveInfinity);
		_scaler.scaleFactor = _originalScale * DewSave.profile.gameplay.uiScale * factor;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateScaleMultiple();
	}

	public void OnSettingsChanged()
	{
		UpdateScaleMultiple();
	}
}
