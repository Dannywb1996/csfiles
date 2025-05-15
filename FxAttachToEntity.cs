using System;
using UnityEngine;

[LogicUpdatePriority(1000)]
public class FxAttachToEntity : LogicBehaviour, IAttachableToEntity
{
	public enum PositionType
	{
		Base,
		Center,
		Above,
		Head,
		LeftHand,
		RightHand,
		LeftFoot,
		RightFoot,
		Weapon,
		Muzzle
	}

	public enum OffsetType
	{
		None,
		WorldSpace,
		LocalSpace
	}

	public enum RotationType
	{
		DoNothing,
		AlwaysIdentity,
		PreserveRotation,
		PreserveLocalRotation,
		CameraRotation
	}

	public enum ScaleType
	{
		DoNothing,
		EntityRadiusHeight,
		EntityXYZ,
		EntityRadius,
		EntityOuterRadius
	}

	public PositionType position = PositionType.Center;

	public OffsetType offset;

	public RotationType rotation = RotationType.AlwaysIdentity;

	public bool useFlatRotation;

	public ScaleType scale;

	private Vector3 _localPosition;

	private Quaternion _localRotation;

	private Vector3 _localScale;

	private bool _isLocalPositionSet;

	public Entity targetEntity { get; private set; }

	private void Awake()
	{
		_localPosition = base.transform.localPosition;
		_localRotation = base.transform.localRotation;
		_localScale = base.transform.localScale;
		_isLocalPositionSet = true;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		UpdatePosition();
	}

	public void OnAttachToEntity(Entity target)
	{
		if (targetEntity != null && targetEntity.Visual != null && targetEntity.Visual._attachedEffects != null)
		{
			targetEntity.Visual._attachedEffects.Remove(this);
		}
		targetEntity = target;
		if (targetEntity != null && targetEntity.Visual != null && targetEntity.Visual._attachedEffects != null)
		{
			targetEntity.Visual._attachedEffects.Add(this);
		}
		UpdatePosition();
	}

	private void OnDestroy()
	{
		if (targetEntity != null && targetEntity.Visual != null && targetEntity.Visual._attachedEffects != null)
		{
			targetEntity.Visual._attachedEffects.Remove(this);
		}
		targetEntity = null;
	}

	private void UpdatePosition()
	{
		if (ManagerBase<CameraManager>.softInstance == null)
		{
			return;
		}
		if (!_isLocalPositionSet)
		{
			_isLocalPositionSet = true;
			_localPosition = base.transform.localPosition;
			_localRotation = base.transform.localRotation;
			_localScale = base.transform.localScale;
		}
		if (targetEntity == null)
		{
			return;
		}
		Vector3 pos = default(Vector3);
		Quaternion rot = default(Quaternion);
		Quaternion parentRot = targetEntity.transform.rotation;
		if (position == PositionType.Base)
		{
			pos = targetEntity.Visual.GetBasePosition();
			parentRot = targetEntity.transform.rotation;
		}
		else if (position == PositionType.Center)
		{
			pos = targetEntity.Visual.GetCenterPosition();
			parentRot = targetEntity.transform.rotation;
		}
		else if (position == PositionType.Above)
		{
			pos = targetEntity.Visual.GetAbovePosition();
			parentRot = targetEntity.transform.rotation;
		}
		else if (position == PositionType.Muzzle)
		{
			pos = targetEntity.Visual.GetMuzzlePosition();
			parentRot = targetEntity.Visual.GetMuzzleRotation();
		}
		else if (position == PositionType.Weapon)
		{
			pos = targetEntity.Visual.GetWeaponPosition();
			parentRot = targetEntity.Visual.GetWeaponRotation();
		}
		else
		{
			HumanBodyBones bone = HumanBodyBones.Head;
			switch (position)
			{
			case PositionType.Head:
				bone = HumanBodyBones.Head;
				break;
			case PositionType.LeftHand:
				bone = HumanBodyBones.LeftHand;
				break;
			case PositionType.RightHand:
				bone = HumanBodyBones.RightHand;
				break;
			case PositionType.LeftFoot:
				bone = HumanBodyBones.LeftFoot;
				break;
			case PositionType.RightFoot:
				bone = HumanBodyBones.RightFoot;
				break;
			}
			pos = targetEntity.Visual.GetBonePosition(bone);
			parentRot = targetEntity.Visual.GetBoneRotation(bone);
		}
		if (offset == OffsetType.LocalSpace)
		{
			pos += parentRot * _localPosition;
		}
		else if (offset == OffsetType.WorldSpace)
		{
			pos += ManagerBase<CameraManager>.softInstance.entityCamAngleRotation * _localPosition;
		}
		switch (rotation)
		{
		case RotationType.AlwaysIdentity:
			rot = ManagerBase<CameraManager>.softInstance.entityCamAngleRotation;
			break;
		case RotationType.PreserveRotation:
			rot = _localRotation;
			break;
		case RotationType.PreserveLocalRotation:
			rot = parentRot * _localRotation;
			break;
		case RotationType.DoNothing:
			rot = base.transform.rotation;
			break;
		case RotationType.CameraRotation:
			rot = Dew.mainCamera.transform.rotation;
			break;
		}
		if (useFlatRotation)
		{
			rot = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);
		}
		switch (scale)
		{
		case ScaleType.EntityRadiusHeight:
		{
			Bounds bounds5 = targetEntity.Visual.GetBodyBounds();
			float radius6 = Mathf.Min(bounds5.size.x, bounds5.size.z);
			float height1 = bounds5.size.y;
			Vector3 scale7 = new Vector3(_localScale.x * radius6, _localScale.y * height1, _localScale.z * radius6);
			base.transform.localScale = scale7;
			break;
		}
		case ScaleType.EntityXYZ:
		{
			Bounds bounds4 = targetEntity.Visual.GetBodyBounds();
			Vector3 scale6 = new Vector3(_localScale.x * bounds4.size.x, _localScale.y * bounds4.size.y, _localScale.z * bounds4.size.z);
			base.transform.localScale = scale6;
			break;
		}
		case ScaleType.EntityRadius:
		{
			Bounds bounds3 = targetEntity.Visual.GetBodyBounds();
			float radius5 = Mathf.Min(bounds3.size.x, bounds3.size.z);
			Vector3 scale5 = new Vector3(_localScale.x * radius5, _localScale.y * radius5, _localScale.z * radius5);
			base.transform.localScale = scale5;
			break;
		}
		case ScaleType.EntityOuterRadius:
		{
			float radius4 = targetEntity.Control.outerRadius;
			Vector3 scale4 = new Vector3(_localScale.x * radius4, _localScale.y * radius4, _localScale.z * radius4);
			base.transform.localScale = scale4;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case ScaleType.DoNothing:
			break;
		}
		base.transform.SetPositionAndRotation(pos, rot);
	}
}
