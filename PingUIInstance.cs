using System;
using DG.Tweening;
using PixelPlay.OffScreenIndicator;
using TMPro;
using UnityEngine;

public class PingUIInstance : MonoBehaviour
{
	public Vector2 screenMargin;

	public Transform rotationTransform;

	public float angleOffset;

	public CanvasGroup arrowCanvasGroup;

	public TextMeshProUGUI playerNameText;

	public Vector3 scalePunch;

	public float punchDuration;

	public bool useAlwaysOnTopUIParent;

	internal PingManager.Ping _ping;

	private Func<Vector3> _worldPosGetter;

	private Func<Vector2> _uiPosGetter;

	private void Start()
	{
		switch (_ping.type)
		{
		case PingManager.PingType.Move:
			_worldPosGetter = () => _ping.position;
			break;
		case PingManager.PingType.Entity:
		case PingManager.PingType.ShopItem:
			_worldPosGetter = () => ((Entity)_ping.target).Visual.GetCenterPosition();
			break;
		case PingManager.PingType.Interactable:
			_worldPosGetter = () => ((IInteractable)_ping.target).interactPivot.position;
			break;
		case PingManager.PingType.EquippedItem:
			UnityEngine.Object.Destroy(base.gameObject);
			break;
		case PingManager.PingType.WorldNode:
			_uiPosGetter = GetUIPosOfPing;
			InGameUIManager.instance.IncrementWorldNodePingCounter();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		base.transform.DOPunchScale(scalePunch, punchDuration).SetUpdate(isIndependentUpdate: true);
		playerNameText.text = _ping.sender.playerName;
		UpdatePosition();
	}

	private Vector2 GetUIPosOfPing()
	{
		return InGameUIManager.instance.GetWorldNodeUIPos(_ping.itemIndex);
	}

	private void LateUpdate()
	{
		if (!_ping.IsValid())
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			UpdatePosition();
		}
	}

	private void OnDestroy()
	{
		if (_ping.type == PingManager.PingType.WorldNode && InGameUIManager.instance != null)
		{
			InGameUIManager.instance.DecrementWorldNodePingCounter();
		}
	}

	private void UpdatePosition()
	{
		if (_worldPosGetter != null)
		{
			Vector3 worldPos = _worldPosGetter();
			Vector3 screenPos = Dew.mainCamera.WorldToScreenPoint(worldPos);
			bool isOnScreen = OffScreenIndicatorCore.IsTargetVisible(screenPos);
			arrowCanvasGroup.alpha = ((!isOnScreen) ? 1 : 0);
			if (isOnScreen)
			{
				base.transform.position = screenPos.Quantitized();
				return;
			}
			float angle = 0f;
			Vector3 screenCentre = new Vector3(Screen.width, Screen.height, 0f) / 2f;
			Vector3 screenBounds = screenCentre - new Vector3(screenMargin.x, screenMargin.y, 0f);
			OffScreenIndicatorCore.GetArrowIndicatorPositionAndAngle(ref screenPos, ref angle, screenCentre, screenBounds);
			base.transform.position = screenPos.Quantitized();
			rotationTransform.rotation = Quaternion.Euler(0f, 0f, angle * 57.29578f + angleOffset);
		}
		else if (_uiPosGetter != null)
		{
			Vector2 screenPos2 = _uiPosGetter();
			base.transform.position = screenPos2.Quantitized();
		}
	}
}
