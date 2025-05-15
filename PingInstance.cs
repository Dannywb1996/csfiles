using System;
using UnityEngine;

public class PingInstance : MonoBehaviour
{
	public GameObject effect;

	public float pingLifeTime = 8f;

	internal PingManager.Ping _ping;

	private float _startUnscaledTime;

	internal GameObject _uiInstance;

	private Func<Vector3> _worldPosGetter;

	private void Awake()
	{
		_startUnscaledTime = Time.unscaledTime;
	}

	private void Start()
	{
		switch (_ping.type)
		{
		case PingManager.PingType.Move:
			_worldPosGetter = () => _ping.position;
			break;
		case PingManager.PingType.Entity:
		case PingManager.PingType.ShopItem:
			_worldPosGetter = () => ((Entity)_ping.target).Visual.GetBasePosition();
			break;
		case PingManager.PingType.Interactable:
		{
			Transform interactablePivot = _ping.target.GetComponent<IInteractable>().interactPivot;
			_worldPosGetter = () => interactablePivot.position;
			break;
		}
		case PingManager.PingType.EquippedItem:
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
		base.transform.position = _worldPosGetter();
		DewEffect.Play(effect);
	}

	private void OnDestroy()
	{
		if (_uiInstance != null)
		{
			UnityEngine.Object.DestroyImmediate(_uiInstance);
		}
	}

	private void LateUpdate()
	{
		if (!_ping.IsValid())
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (Time.unscaledTime - _startUnscaledTime > pingLifeTime)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		base.transform.position = _worldPosGetter();
	}
}
