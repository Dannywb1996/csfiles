using System;
using UnityEngine;

[LogicUpdatePriority(1000)]
public class FxCastTelegraph : LogicBehaviour
{
	public enum PositionType
	{
		None = 2,
		Caster = 0,
		Target = 1
	}

	public enum OffsetType
	{
		WorldSpace,
		CastDirection
	}

	public enum RotationType
	{
		None = 3,
		AlwaysIdentity = 0,
		LocalRotationOnWorldSpace = 1,
		LocalRotationOnCastDirection = 2
	}

	public PositionType position = PositionType.None;

	public OffsetType offset;

	public RotationType rotation = RotationType.None;

	public bool adjustSimulationSpeed = true;

	private Vector3 _initPosition;

	private Quaternion _initRotation;

	private bool _isInitPosSet;

	public CastMethodType castMethod { get; private set; }

	public CastInfo castInfo { get; private set; }

	private void Awake()
	{
		CheckInitialPositionData();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		UpdatePosition();
	}

	public void Setup(CastMethodType method, CastInfo info, float duration)
	{
		castInfo = info;
		castMethod = method;
		if (duration > 0f && adjustSimulationSpeed)
		{
			ListReturnHandle<ParticleSystem> h0;
			foreach (ParticleSystem item in ((Component)this).GetComponentsInChildrenNonAlloc(out h0))
			{
				ParticleSystem.MainModule mainModule = item.main;
				mainModule.simulationSpeed = 1f / duration;
			}
			h0.Return();
			ListReturnHandle<BoxTelegraphController> h1;
			foreach (BoxTelegraphController item2 in ((Component)this).GetComponentsInChildrenNonAlloc(out h1))
			{
				item2.duration = duration;
			}
			h1.Return();
		}
		UpdatePosition();
	}

	private void CheckInitialPositionData()
	{
		if (!_isInitPosSet)
		{
			_isInitPosSet = true;
			_initPosition = base.transform.localPosition;
			_initRotation = base.transform.localRotation;
		}
	}

	private void UpdatePosition()
	{
		try
		{
			if (position == PositionType.None && rotation == RotationType.None)
			{
				return;
			}
			CheckInitialPositionData();
			Vector3 referencePos = default(Vector3);
			switch (position)
			{
			case PositionType.Caster:
				if (castInfo.caster == null)
				{
					return;
				}
				referencePos = castInfo.caster.position;
				break;
			case PositionType.Target:
				if (castMethod == CastMethodType.Target)
				{
					referencePos = castInfo.target.position;
					break;
				}
				if (castMethod == CastMethodType.Point)
				{
					referencePos = castInfo.point;
					break;
				}
				return;
			default:
				throw new ArgumentOutOfRangeException();
			case PositionType.None:
				break;
			}
			Quaternion castRotation;
			switch (castMethod)
			{
			case CastMethodType.None:
				if (castInfo.caster == null)
				{
					return;
				}
				castRotation = castInfo.caster.Control.desiredRotation;
				break;
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
				castRotation = castInfo.rotation;
				break;
			case CastMethodType.Target:
				castRotation = ((castInfo.target == null) ? castInfo.rotation : Quaternion.LookRotation(castInfo.target.position - castInfo.caster.position).Flattened());
				break;
			case CastMethodType.Point:
			{
				if (castInfo.caster == null)
				{
					return;
				}
				Vector3 delta = castInfo.point - castInfo.caster.position;
				castRotation = ((!(delta.sqrMagnitude < 0.001f)) ? Quaternion.LookRotation(delta).Flattened() : castInfo.caster.rotation);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			Vector3 finalPos = offset switch
			{
				OffsetType.WorldSpace => referencePos + _initPosition, 
				OffsetType.CastDirection => referencePos + castRotation * _initPosition, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			Quaternion finalRot = default(Quaternion);
			switch (rotation)
			{
			case RotationType.AlwaysIdentity:
				finalRot = Quaternion.identity;
				break;
			case RotationType.LocalRotationOnCastDirection:
				finalRot = castRotation * _initRotation;
				break;
			case RotationType.LocalRotationOnWorldSpace:
				finalRot = _initRotation;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case RotationType.None:
				break;
			}
			if (position != PositionType.None)
			{
				base.transform.position = finalPos;
			}
			if (rotation != RotationType.None)
			{
				base.transform.rotation = finalRot;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
