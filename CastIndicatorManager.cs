using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DTT.AreaOfEffectRegions;
using UnityEngine;

public class CastIndicatorManager : ManagerBase<CastIndicatorManager>
{
	public Transform playerOrigin;

	public LineRegion lineRegion;

	public CircleRegion circleRegion;

	public CircleRegion secondaryCircleRegion;

	public ArcRegion arcRegion;

	private List<Material> _instantiatedMaterials = new List<Material>();

	private bool _isAnimatingFailure;

	protected override void Awake()
	{
		base.Awake();
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		foreach (MeshRenderer ren in componentsInChildren)
		{
			Material[] mats = ren.sharedMaterials;
			for (int j = 0; j < mats.Length; j++)
			{
				mats[j] = UnityEngine.Object.Instantiate(mats[j]);
				_instantiatedMaterials.Add(mats[j]);
			}
			ren.sharedMaterials = mats;
		}
	}

	private void Start()
	{
		HideAll();
	}

	private void OnDestroy()
	{
		foreach (Material instantiatedMaterial in _instantiatedMaterials)
		{
			UnityEngine.Object.Destroy(instantiatedMaterial);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		Entity entity = ManagerBase<ControlManager>.instance.controllingEntity;
		if (entity == null || !ManagerBase<ControlManager>.instance.isCharacterControlEnabled)
		{
			HideAll();
			return;
		}
		if (ManagerBase<ControlManager>.instance._localSampleContext.HasValue && ManagerBase<ControlManager>.instance._localSampleContext.Value.showCastIndicator)
		{
			SampleCastInfoContext context = ManagerBase<ControlManager>.instance._localSampleContext.Value;
			CastMethodData method = context.castMethod;
			DrawIndicator(method, context.currentInfo);
			return;
		}
		if (_isAnimatingFailure)
		{
			playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
			return;
		}
		ControlManager.ControlStateType type = ManagerBase<ControlManager>.instance.state.type;
		if (type == ControlManager.ControlStateType.AttackMove || type == ControlManager.ControlStateType.Cast)
		{
			TriggerConfig config = ((type == ControlManager.ControlStateType.AttackMove) ? entity.Ability.attackAbility : ManagerBase<ControlManager>.instance.state.trigger).currentConfig;
			CastMethodData method2 = config.castMethod;
			DrawIndicator(method2, ManagerBase<ControlManager>.instance.GetCastInfo(method2, config.targetValidator));
		}
		else
		{
			HideAll();
		}
	}

	public void IndicateRangeFailure(float range)
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_isAnimatingFailure = true;
			lineRegion.gameObject.SetActive(value: false);
			circleRegion.gameObject.SetActive(value: true);
			arcRegion.gameObject.SetActive(value: false);
			secondaryCircleRegion.gameObject.SetActive(value: false);
			playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
			circleRegion.transform.DOKill(complete: true);
			circleRegion.transform.localScale = Vector3.one * 0.85f;
			circleRegion.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBounce);
			circleRegion.Radius = range;
			circleRegion.UpdateProperties();
			yield return new WaitForSeconds(0.25f);
			circleRegion.transform.DOScale(Vector3.zero, 0.15f);
			yield return new WaitForSeconds(0.25f);
			_isAnimatingFailure = false;
		}
	}

	private void DrawIndicator(CastMethodData method, CastInfo info)
	{
		Entity entity = ManagerBase<ControlManager>.instance.controllingEntity;
		switch (method.type)
		{
		case CastMethodType.None:
		{
			float rad = method.noneData.radius;
			if (rad > 0.1f)
			{
				DrawCircleRegion(rad);
			}
			else
			{
				HideAll();
			}
			break;
		}
		case CastMethodType.Cone:
			DrawArcRegion(method.coneData.angle, method.coneData.radius, info.angle);
			break;
		case CastMethodType.Arrow:
			DrawLineRegion(method.arrowData.length, method.arrowData.width, info.angle);
			break;
		case CastMethodType.Target:
		{
			Entity target = info.target;
			if (method.targetData.radius > 0.1f && target != null)
			{
				DrawDoubleCircleRegion(method.targetData.range, method.targetData.radius, target.position);
			}
			else
			{
				DrawCircleRegion(method.targetData.range);
			}
			break;
		}
		case CastMethodType.Point:
			if (method.targetData.radius > 0.1f)
			{
				Vector3 position = info.point;
				if (method.pointData.isClamping)
				{
					position = Vector3.ClampMagnitude(position - entity.position, method.pointData.range) + entity.position;
				}
				DrawDoubleCircleRegion(method.pointData.range, method.pointData.radius, position);
			}
			else
			{
				DrawCircleRegion(method.pointData.range);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void HideAll()
	{
		lineRegion.gameObject.SetActive(value: false);
		circleRegion.gameObject.SetActive(value: false);
		arcRegion.gameObject.SetActive(value: false);
		secondaryCircleRegion.gameObject.SetActive(value: false);
	}

	private void DrawCircleRegion(float range)
	{
		lineRegion.gameObject.SetActive(value: false);
		circleRegion.gameObject.SetActive(value: true);
		arcRegion.gameObject.SetActive(value: false);
		secondaryCircleRegion.gameObject.SetActive(value: false);
		playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
		circleRegion.FillProgress = 1f;
		circleRegion.Radius = range;
		circleRegion.UpdateProperties();
	}

	private void DrawArcRegion(float arc, float radius, float angle)
	{
		lineRegion.gameObject.SetActive(value: false);
		circleRegion.gameObject.SetActive(value: false);
		arcRegion.gameObject.SetActive(value: true);
		secondaryCircleRegion.gameObject.SetActive(value: false);
		playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
		arcRegion.Radius = radius;
		arcRegion.Arc = arc;
		arcRegion.Angle = angle;
		arcRegion.UpdateProperties();
	}

	private void DrawLineRegion(float length, float width, float angle)
	{
		lineRegion.gameObject.SetActive(value: true);
		circleRegion.gameObject.SetActive(value: false);
		arcRegion.gameObject.SetActive(value: false);
		secondaryCircleRegion.gameObject.SetActive(value: false);
		playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
		lineRegion.Length = length;
		lineRegion.Width = width;
		lineRegion.Angle = angle;
		lineRegion.UpdateProperties();
	}

	private void DrawDoubleCircleRegion(float range, float radius, Vector3 position)
	{
		lineRegion.gameObject.SetActive(value: false);
		circleRegion.gameObject.SetActive(value: true);
		arcRegion.gameObject.SetActive(value: false);
		secondaryCircleRegion.gameObject.SetActive(value: true);
		playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
		circleRegion.Radius = range;
		circleRegion.UpdateProperties();
		secondaryCircleRegion.transform.position = position;
		secondaryCircleRegion.Radius = radius;
		secondaryCircleRegion.UpdateProperties();
	}
}
