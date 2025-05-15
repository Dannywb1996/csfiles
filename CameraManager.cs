using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraManager : ManagerBase<CameraManager>
{
	public Action<bool> onIsSpectatingChanged;

	public Action<Entity, Entity> onFocusedEntityChanged;

	public float followSmoothTime = 0.05f;

	public float startSpectatingTime = 3f;

	public float nextSpectateTargetTime = 2f;

	public Vector3 farZoomBody;

	public Vector3 midZoombody;

	public Vector3 closeZoomBody;

	public int zoomSteps;

	public int defaultZoomLevel = 1;

	public float zoomSmoothTime = 0.2f;

	public CinemachineImpulseSource bigDamageShake;

	public CinemachineImpulseSource smallDamageShake;

	public float smallDamageThreshold = 0.01f;

	public float smallDamageAlpha = 0.2f;

	public float bigDamageThreshold = 0.2f;

	public float bigDamageAlpha = 1f;

	public float minAlphaHealthThreshold = 0.5f;

	public float maxAlphaHealthThreshold = 0.25f;

	public float maxAlpha = 0.6f;

	public float damageOverlayDecaySpeed = 0.5f;

	public CanvasGroup damageOverlay;

	public float occlusionTestRadius;

	public CinemachineVirtualCamera selectedEntityCamera;

	public CanvasGroup cutsceneTransitionFade;

	public float cutsceneFadeTime;

	public float genericFadeTime = 0.5f;

	public Volume fadeOutVolume;

	[NonSerialized]
	public bool disableSeeThrough;

	private float _entityCamAngle;

	[NonSerialized]
	public Quaternion entityCamAngleRotation = Quaternion.identity;

	private float _startSpectateCurrentElapsedTime;

	private float _nextSpectateTargetElapsedTime;

	private float _damageOverlayMinAlpha;

	private Vector3 _followCv;

	private Vector3 _zoomCv;

	private Vector4[] _characterPositionsBuffer;

	private Transform _followTransform;

	private CinemachineTransposer _body;

	private List<CameraModifierBase> _cameraModifiers = new List<CameraModifierBase>();

	private float _fadeOutWeight;

	private static readonly int CharacterPositions = Shader.PropertyToID("_CharacterPositions");

	private static readonly int CharacterPositionsCount = Shader.PropertyToID("_CharacterPositionsCount");

	private static readonly int MainCharacterPosition = Shader.PropertyToID("_MainCharacterPosition");

	public bool isSpectating { get; private set; }

	public float entityCamAngle
	{
		get
		{
			return _entityCamAngle;
		}
		set
		{
			_entityCamAngle = value;
			entityCamAngleRotation = Quaternion.Euler(0f, _entityCamAngle, 0f);
			ShaderManager.UpdateRoomRotation();
			SnapCameraToFocusedEntity();
		}
	}

	public int currentZoomIndex { get; private set; }

	public Entity focusedEntity { get; private set; }

	public bool isPlayingCutscene { get; internal set; }

	public DewCutsceneDirector currentCutsceneDirector { get; internal set; }

	protected override void Awake()
	{
		base.Awake();
		damageOverlay.alpha = 0f;
		_followTransform = new GameObject("Follow Transform").transform;
		_followTransform.parent = base.transform;
		selectedEntityCamera.Follow = _followTransform;
		selectedEntityCamera.LookAt = _followTransform;
		_body = selectedEntityCamera.GetCinemachineComponent<CinemachineTransposer>();
		SetZoomLevel(defaultZoomLevel);
	}

	private void Start()
	{
		DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
		dewNetworkManager.onLoadingStatusChanged = (Action<bool>)Delegate.Combine(dewNetworkManager.onLoadingStatusChanged, new Action<bool>(OnLoadingStatusChanged));
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += new Action<EventInfoDamage>(ClientEntityEventOnTakeDamage);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)delegate
		{
			SnapCameraToFocusedEntity();
		};
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)delegate
		{
			LogicUpdateSpectation(0f);
		};
	}

	private void OnLoadingStatusChanged(bool obj)
	{
		if (!obj)
		{
			SnapCameraToFocusedEntity();
		}
	}

	private void OnDestroy()
	{
		if (DewNetworkManager.instance != null)
		{
			DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
			dewNetworkManager.onLoadingStatusChanged = (Action<bool>)Delegate.Remove(dewNetworkManager.onLoadingStatusChanged, new Action<bool>(OnLoadingStatusChanged));
		}
		Shader.SetGlobalInt(CharacterPositionsCount, 0);
	}

	private void ClientEntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (obj.victim != focusedEntity)
		{
			return;
		}
		float val = (obj.damage.amount + obj.damage.discardedAmount) / obj.victim.maxHealth;
		if (val > bigDamageThreshold)
		{
			if (damageOverlay.alpha < _damageOverlayMinAlpha + bigDamageAlpha)
			{
				damageOverlay.alpha = _damageOverlayMinAlpha + bigDamageAlpha;
			}
			bigDamageShake.GenerateImpulse();
		}
		else if (val > smallDamageThreshold)
		{
			if (damageOverlay.alpha < _damageOverlayMinAlpha + smallDamageAlpha)
			{
				damageOverlay.alpha = _damageOverlayMinAlpha + smallDamageAlpha;
			}
			smallDamageShake.GenerateImpulse();
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!(DewPlayer.local == null))
		{
			LogicUpdateOcclusionTest();
			LogicUpdateSpectation(dt);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		FrameUpdateDamageOverlay();
		FrameUpdateFollow();
		FrameUpdateZoom();
	}

	private void FrameUpdateZoom()
	{
		if (!InGameUIManager.instance.IsState("Playing"))
		{
			return;
		}
		if (!NetworkedManagerBase<ConsoleManager>.instance.isConsoleWindowOpen)
		{
			if ((bool)ManagerBase<ControlManager>.instance.it_zoomInCamera)
			{
				SetZoomLevel(currentZoomIndex + 1);
			}
			if ((bool)ManagerBase<ControlManager>.instance.it_zoomOutCamera)
			{
				SetZoomLevel(currentZoomIndex - 1);
			}
		}
		_body.m_FollowOffset = Vector3.SmoothDamp(_body.m_FollowOffset, GetFollowOffsetTarget(), ref _zoomCv, zoomSmoothTime);
	}

	private Vector3 GetFollowOffsetTarget()
	{
		float zoomIndex = currentZoomIndex;
		if (!isSpectating)
		{
			foreach (CameraModifierBase cameraModifier in _cameraModifiers)
			{
				if (cameraModifier is CameraModifierZoom zoom)
				{
					zoomIndex = zoom.zoomIndex;
				}
			}
		}
		float val = zoomIndex / (float)(zoomSteps - 1);
		Vector3 target = ((val < 0.5f) ? Vector3.LerpUnclamped(farZoomBody, midZoombody, val * 2f) : Vector3.LerpUnclamped(midZoombody, closeZoomBody, val * 2f - 1f));
		return Quaternion.Euler(0f, entityCamAngle, 0f) * target;
	}

	public void ZoomIn()
	{
		SetZoomLevel(currentZoomIndex + 1);
	}

	public void ZoomOut()
	{
		SetZoomLevel(currentZoomIndex - 1);
	}

	public void SetZoomLevel(int level)
	{
		currentZoomIndex = Mathf.Clamp(level, 0, zoomSteps - 1);
	}

	public void SnapCameraToFocusedEntity()
	{
		if (!(focusedEntity == null))
		{
			_followTransform.position = focusedEntity.Visual.GetBasePosition();
			_followCv = default(Vector3);
			_body.m_FollowOffset = GetFollowOffsetTarget();
			selectedEntityCamera.enabled = false;
			selectedEntityCamera.enabled = true;
		}
	}

	public void SetCameraPosition(Vector3 pos)
	{
		_followTransform.position = pos;
		_followCv = default(Vector3);
	}

	private void LogicUpdateSpectation(float dt)
	{
		if (isSpectating)
		{
			if (!DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut())
			{
				SetFocusedEntity(DewPlayer.local.hero);
				isSpectating = false;
				onIsSpectatingChanged?.Invoke(isSpectating);
				_nextSpectateTargetElapsedTime = 0f;
				SnapCameraToFocusedEntity();
			}
			else if (focusedEntity.IsNullInactiveDeadOrKnockedOut())
			{
				_nextSpectateTargetElapsedTime += dt;
				if (!(_nextSpectateTargetElapsedTime > nextSpectateTargetTime))
				{
					return;
				}
				bool hasTargetToSpectate = false;
				for (int i = 0; i < NetworkedManagerBase<ActorManager>.instance.allHeroes.Count; i++)
				{
					if (!NetworkedManagerBase<ActorManager>.instance.allHeroes[i].IsNullInactiveDeadOrKnockedOut())
					{
						hasTargetToSpectate = true;
						break;
					}
				}
				if (hasTargetToSpectate)
				{
					_nextSpectateTargetElapsedTime = 0f;
					ChooseNextSpectationTarget();
				}
			}
			else
			{
				_nextSpectateTargetElapsedTime = 0f;
			}
		}
		else if (DewPlayer.local.hero == null)
		{
			ChooseNextSpectationTarget();
			isSpectating = true;
			onIsSpectatingChanged?.Invoke(isSpectating);
		}
		else if (DewPlayer.local.hero.isKnockedOut && !NetworkedManagerBase<GameManager>.instance.isGameConcluded && DewPlayer.humanPlayers.Count > 1)
		{
			_startSpectateCurrentElapsedTime += dt;
			if (_startSpectateCurrentElapsedTime > startSpectatingTime)
			{
				ChooseNextSpectationTarget();
				isSpectating = true;
				onIsSpectatingChanged?.Invoke(isSpectating);
				_startSpectateCurrentElapsedTime = 0f;
			}
		}
		else
		{
			_startSpectateCurrentElapsedTime = 0f;
		}
	}

	private void LogicUpdateOcclusionTest()
	{
		if (_characterPositionsBuffer == null)
		{
			_characterPositionsBuffer = new Vector4[16];
		}
		if (disableSeeThrough || DewPlayer.local.controllingEntity == null)
		{
			Shader.SetGlobalInt(CharacterPositionsCount, 0);
			return;
		}
		Vector3 center = ((focusedEntity != null) ? focusedEntity.agentPosition : ControlManager.GetWorldPositionOnGroundFromViewportPoint(Vector2.one * 0.5f, forDirectionalAttacks: false));
		ListReturnHandle<Vector3> h0;
		List<Vector3> positions = DewPool.GetList(out h0);
		ArrayReturnHandle<Collider> h1;
		Collider[] colArr = DewPool.GetArray(out h1, 128);
		int count = Physics.OverlapSphereNonAlloc(center, occlusionTestRadius, colArr, LayerMasks.Interactable | LayerMasks.Entity);
		for (int i = 0; i < count; i++)
		{
			IInteractable found = colArr[i].GetComponentInParent<IInteractable>();
			if (found != null && !found.IsUnityNull() && !(found is Actor { isActive: false }) && found.CanInteract(ManagerBase<ControlManager>.softInstance.controllingEntity))
			{
				positions.Add(found.interactPivot.position + Vector3.up);
			}
		}
		ArrayReturnHandle<Entity> h2;
		ReadOnlySpan<Entity> readOnlySpan = DewPhysics.OverlapCircleAllEntities(out h2, center, occlusionTestRadius, new CollisionCheckSettings
		{
			includeUncollidable = true
		});
		for (int j = 0; j < readOnlySpan.Length; j++)
		{
			Entity e = readOnlySpan[j];
			if (!e.IsNullInactiveDeadOrKnockedOut() && !(e is IDisableOcclusionTest))
			{
				positions.Add(e.Visual.GetCenterPosition());
			}
		}
		positions.Sort((Vector3 x, Vector3 y) => Vector3.SqrMagnitude(x - center).CompareTo(Vector3.SqrMagnitude(y - center)));
		for (int i2 = 0; i2 < positions.Count && i2 < _characterPositionsBuffer.Length; i2++)
		{
			_characterPositionsBuffer[i2] = positions[i2];
		}
		int total = Mathf.Min(_characterPositionsBuffer.Length, positions.Count);
		Shader.SetGlobalInt(CharacterPositionsCount, total);
		Shader.SetGlobalVectorArray(CharacterPositions, _characterPositionsBuffer);
		Shader.SetGlobalVector(MainCharacterPosition, center);
		h0.Return();
		h1.Return();
		h2.Return();
	}

	private void FrameUpdateDamageOverlay()
	{
		if (focusedEntity == null)
		{
			damageOverlay.alpha = 0f;
			return;
		}
		EntityStatus status = focusedEntity.Status;
		_damageOverlayMinAlpha = 1f - (status.currentHealth / status.maxHealth - maxAlphaHealthThreshold) / (minAlphaHealthThreshold - maxAlphaHealthThreshold);
		_damageOverlayMinAlpha = Mathf.Clamp01(_damageOverlayMinAlpha);
		_damageOverlayMinAlpha *= maxAlpha;
		damageOverlay.alpha = Mathf.MoveTowards(damageOverlay.alpha, _damageOverlayMinAlpha, damageOverlayDecaySpeed * Time.deltaTime);
	}

	private void FrameUpdateFollow()
	{
		if (isSpectating)
		{
			if (DewInput.GetButtonDown(DewSave.profile.controls.spectatorNextTarget, checkGameAreaForMouse: true))
			{
				ChooseNextSpectationTarget();
			}
		}
		else if (focusedEntity != ManagerBase<ControlManager>.instance.controllingEntity)
		{
			SetFocusedEntity(ManagerBase<ControlManager>.instance.controllingEntity);
			SnapCameraToFocusedEntity();
		}
		if (focusedEntity == null)
		{
			_followCv = default(Vector3);
			return;
		}
		Vector3 pos = default(Vector3);
		if (NetworkedManagerBase<ConversationManager>.instance.hasOngoingLocalConversation)
		{
			int count = 0;
			Entity[] speakers = NetworkedManagerBase<ConversationManager>.instance.ongoingLocalConversation.speakers;
			foreach (Entity e in speakers)
			{
				if (!e.IsNullInactiveDeadOrKnockedOut())
				{
					pos += e.Visual.GetBasePosition();
					count++;
				}
			}
			if (count == 0)
			{
				pos = focusedEntity.Visual.GetBasePosition();
			}
			else
			{
				pos /= (float)count;
			}
		}
		else
		{
			pos = focusedEntity.Visual.GetBasePosition();
		}
		if (!isSpectating)
		{
			foreach (CameraModifierBase cameraModifier in _cameraModifiers)
			{
				if (cameraModifier is CameraModifierOffset offset)
				{
					pos += offset.offset;
				}
			}
		}
		_followTransform.position = Vector3.SmoothDamp(_followTransform.position, pos, ref _followCv, followSmoothTime);
	}

	private void ChooseNextSpectationTarget()
	{
		IReadOnlyList<Hero> heroes = NetworkedManagerBase<ActorManager>.instance.allHeroes;
		int index = 0;
		for (int i = 0; i < heroes.Count; i++)
		{
			if (heroes[i] == focusedEntity)
			{
				index = i;
				break;
			}
		}
		for (int j = 0; j < heroes.Count; j++)
		{
			index = (index + 1) % heroes.Count;
			if (!heroes[index].IsNullInactiveDeadOrKnockedOut())
			{
				SetFocusedEntity(heroes[index]);
				SnapCameraToFocusedEntity();
				break;
			}
		}
	}

	public void SetActiveEntityVCam(bool value)
	{
		selectedEntityCamera.gameObject.SetActive(value);
	}

	public void DoGenericFadeIn(bool immediately = false)
	{
		fadeOutVolume.DOKill();
		if (immediately)
		{
			fadeOutVolume.weight = 0f;
		}
		else
		{
			fadeOutVolume.DOWeight(0f, genericFadeTime);
		}
	}

	public void DoGenericFadeOut(bool immediately = false)
	{
		fadeOutVolume.DOKill();
		if (immediately)
		{
			fadeOutVolume.weight = 1f;
		}
		else
		{
			fadeOutVolume.DOWeight(1f, genericFadeTime);
		}
	}

	public void DoCutsceneFadeOut()
	{
		cutsceneTransitionFade.DOKill();
		cutsceneTransitionFade.DOFade(1f, cutsceneFadeTime);
	}

	public void DoCutsceneFadeIn()
	{
		cutsceneTransitionFade.DOKill();
		cutsceneTransitionFade.DOFade(0f, cutsceneFadeTime);
	}

	internal void AddLocalCameraModifier(CameraModifierBase modifier)
	{
		if (modifier == null)
		{
			throw new ArgumentNullException("modifier");
		}
		_cameraModifiers.Add(modifier);
	}

	internal void RemoveLocalCameraModifier(CameraModifierBase modifier)
	{
		_cameraModifiers.Remove(modifier);
	}

	public void SetFocusedEntity(Entity newValue)
	{
		damageOverlay.alpha = 0f;
		if (newValue == focusedEntity)
		{
			return;
		}
		Entity oldValue = focusedEntity;
		focusedEntity = newValue;
		try
		{
			onFocusedEntityChanged?.Invoke(oldValue, newValue);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void SkipCurrentCutscene()
	{
		if (isPlayingCutscene && !(currentCutsceneDirector == null) && currentCutsceneDirector.enableSkip)
		{
			currentCutsceneDirector.CmdSkip();
		}
	}
}
