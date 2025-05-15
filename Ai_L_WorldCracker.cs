using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class Ai_L_WorldCracker : AbilityInstance
{
	public DewAnimationClip beamAnim;

	public float frontDistance;

	public float damageCheckInterval;

	public float damageInterval;

	public float yOffset;

	public ScalingValue damagePerTick;

	public float uncancellableTime;

	public float cameraOffset;

	public int cameraZoomIndex;

	public float cameraRevertDelay = 0.1f;

	public float cameraRevertTime = 0.5f;

	public float maxDistance;

	public float radius = 0.8f;

	public GameObject beamEffect;

	public GameObject beamHitEffect;

	public GameObject destinationEffect;

	public LineRenderer lineRenderer;

	public float beamInterval;

	public GameObject periodicEffect;

	public float periodicEffectInterval;

	private float _lastPeriodicEffectInterval;

	private CameraModifierOffset _offset;

	private CameraModifierZoom _zoom;

	[FormerlySerializedAs("beamRotationSpeed")]
	public float angleSpeed;

	public float procCoefficient;

	private float _lastDamageTime;

	private bool _atTarget;

	private float _lastBeamInterval;

	[SyncVar]
	private float _angle;

	private Dictionary<Entity, float> _hitTimes = new Dictionary<Entity, float>();

	public float Network_angle
	{
		get
		{
			return _angle;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _angle, 32uL, null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_angle = base.info.angle;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		SolvePosition();
		FxPlay(beamEffect);
		FxPlay(destinationEffect);
		lineRenderer.enabled = true;
		if (base.info.caster.isOwned)
		{
			_offset = new CameraModifierOffset
			{
				offset = Vector3.zero
			}.Apply();
			_zoom = new CameraModifierZoom
			{
				zoomIndex = cameraZoomIndex
			}.Apply();
		}
		if (!base.isServer)
		{
			return;
		}
		base.info.caster.EntityEvent_OnCastComplete += new Action<EventInfoCast>(PreventYubarMovementSkill);
		base.info.caster.Animation.PlayAbilityAnimation(beamAnim);
		DestroyOnDeath(base.info.caster);
		base.info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.EverythingCancelable,
			duration = 3600f,
			onCancel = delegate
			{
				if (base.isActive)
				{
					Destroy();
				}
			},
			onComplete = delegate
			{
				if (base.isActive)
				{
					Destroy();
				}
			},
			uncancellableTime = uncancellableTime
		}.AddValidation(AbilitySelfValidator.Default));
	}

	private void PreventYubarMovementSkill(EventInfoCast obj)
	{
		if (obj.instance is Se_M_Flicker)
		{
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(beamEffect);
		FxStop(destinationEffect);
		lineRenderer.enabled = false;
		CameraModifierOffset o;
		if (_offset != null)
		{
			o = _offset;
			_offset = null;
			ManagerBase<CameraManager>.instance.StartCoroutine(Routine());
		}
		CameraModifierZoom z;
		if (_zoom != null)
		{
			z = _zoom;
			_zoom = null;
			ManagerBase<CameraManager>.instance.StartCoroutine(Routine2());
		}
		if (base.isServer && !(base.info.caster == null))
		{
			base.info.caster.EntityEvent_OnCastComplete -= new Action<EventInfoCast>(PreventYubarMovementSkill);
			base.info.caster.Animation.StopAbilityAnimation(beamAnim);
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(cameraRevertDelay);
			Vector3 startOffset = o.offset;
			for (float t = 0f; t < cameraRevertTime; t += Time.deltaTime)
			{
				o.offset = startOffset * ((cameraRevertTime - t) / cameraRevertTime);
				yield return null;
			}
			o.Remove();
		}
		IEnumerator Routine2()
		{
			yield return new WaitForSeconds(cameraRevertDelay);
			z.Remove();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		SolvePosition();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (_offset != null && _angle != 0f)
		{
			_offset.offset = Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * cameraOffset;
		}
		if (Time.time - _lastPeriodicEffectInterval > periodicEffectInterval)
		{
			_lastPeriodicEffectInterval = Time.time;
			FxPlay(periodicEffect);
		}
		if (!base.isServer)
		{
			return;
		}
		if (base.info.caster == null || base.info.caster.owner == null)
		{
			Destroy();
			return;
		}
		float target = Vector3.SignedAngle(Vector3.forward, base.info.caster.owner.cursorWorldPos - base.info.caster.position, Vector3.up);
		Network_angle = Mathf.MoveTowardsAngle(_angle, target, angleSpeed * dt);
		base.info.caster.Control.Rotate(_angle, immediately: true, 0.25f);
		if (Time.time - _lastDamageTime < damageCheckInterval)
		{
			return;
		}
		_lastDamageTime = Time.time;
		float num = maxDistance;
		if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo, maxDistance, LayerMasks.Ground | LayerMasks.IncludeInNavigation))
		{
			num = hitInfo.distance + 2f;
		}
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> readOnlySpan = DewPhysics.SphereCastAllEntities(out handle, base.transform.position, radius, base.transform.forward, num, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Entity entity = readOnlySpan[i];
			if (!(Vector2.Distance(base.info.caster.agentPosition.ToXY(), entity.agentPosition.ToXY()) > num) && (!_hitTimes.ContainsKey(entity) || !(Time.time - _hitTimes[entity] < damageInterval)))
			{
				_hitTimes[entity] = Time.time;
				FxPlayNewNetworked(beamHitEffect, entity);
				Damage(damagePerTick, procCoefficient).SetElemental(ElementalType.Light).SetOriginPosition(base.info.caster.position).Dispatch(entity);
			}
		}
		handle.Return();
	}

	private void SolvePosition()
	{
		base.transform.position = base.info.caster.position + Quaternion.Euler(0f, _angle, 0f) * Vector3.forward * frontDistance + Vector3.up * yOffset;
		base.transform.rotation = Quaternion.Euler(0f, _angle, 0f);
		Vector3 vector = base.transform.position + base.transform.forward * 100f;
		if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo, 100f, LayerMasks.Ground | LayerMasks.IncludeInNavigation))
		{
			vector = hitInfo.point;
		}
		lineRenderer.SetPosition(0, base.transform.position);
		lineRenderer.SetPosition(1, vector);
		destinationEffect.transform.position = vector;
		if (Time.time - _lastBeamInterval > beamInterval)
		{
			_atTarget = !_atTarget;
			beamEffect.transform.position = (_atTarget ? vector : base.transform.position);
			_lastBeamInterval = Time.time;
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(_angle);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteFloat(_angle);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _angle, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _angle, null, reader.ReadFloat());
		}
	}
}
