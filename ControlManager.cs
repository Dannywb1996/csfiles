using System;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[LogicUpdatePriority(-500)]
public class ControlManager : ManagerBase<ControlManager>
{
	private enum AttackMoveType
	{
		UseUserSettings,
		UseDistanceFromSelf,
		UseDistanceFromDestination
	}

	public enum ControlStateType
	{
		None = 0,
		AttackMove = 1,
		Cast = 4
	}

	public struct ControlState
	{
		public ControlStateType type;

		public AbilityTrigger trigger;

		public int configIndex;

		public DewBinding castKey;

		public CastConfirmType castType;

		public bool isCastInDirectionOfMovement;
	}

	public const float DirectionalControlStandStillActionGracePeriod = 0.3f;

	private const float RaycastMaxDistance = 200f;

	private const float ContinuousMoveDispatchInterval = 0.0625f;

	private const float DirectionalMoveDispatchInterval = 1f / 60f;

	private const float AttackInPlaceDispatchInterval = 1f / 60f;

	private const bool NonChanneledAttackInPlaceStopsMovement = true;

	private const bool AttackInPlaceAutoTarget = true;

	private const float MovementDirectionStaleTime = 0.4f;

	private const float MinWalkStrengthForGamepads = 0.4f;

	private const bool EnableContinuousAttackMove = true;

	public Action<AbilityTrigger> onCastFailed;

	public Action<Entity, Entity> onSelectedEntityChanged;

	public ControlState state;

	private byte _disableCharacterControlCounter;

	[NonSerialized]
	public bool isMainSkillDisabled;

	[NonSerialized]
	public bool isDodgeDisabled;

	[NonSerialized]
	public bool isEditSkillDisabled;

	[NonSerialized]
	public bool isDismantleDisabled;

	[NonSerialized]
	public bool isWorldMapDisabled;

	[NonSerialized]
	public HeroSkillLocation? gemLocationConstraint;

	[NonSerialized]
	public Func<UnityEngine.Object, bool> dismantleConstraint;

	[NonSerialized]
	public Func<UnityEngine.Object, bool> dropConstraint;

	public CommandMarker attackMarker;

	public CommandMarker moveMarker;

	public CommandMarker interactMarker;

	public GameObject noTargetIndicator;

	public Transform noTargetIndicatorParent;

	public GameObject targetEnemyIndicatorWorld;

	public GameObject targetEnemyIndicatorUi;

	public GameObject aimPointUi;

	private readonly Collider[] _interactCheckColliders = new Collider[16];

	private IInteractable _interactable;

	private float _lastInteractableCheck;

	private bool _isContinuousMove;

	private List<RaycastResult> _raycastResults;

	private float _lastLookSendUnscaledTime;

	private float _lastTargetEnemyUpdateTime;

	private float _lastStopIssueTime;

	private bool _isDoingDirectionalMovement;

	private float _lastMovementDispatchUnscaledTime;

	private float _lastMovementDirectionUnscaledTime = float.NegativeInfinity;

	private bool _isDoingContinuousAttack;

	private float _lastAttackInPlaceDispatchTime = float.NegativeInfinity;

	private float _lastAttackMoveDispatchTime = float.NegativeInfinity;

	private static bool _isInputFieldFocused;

	private static int _isInputFieldFocusedCachedFrame;

	private float _lastNoTargetShowTime;

	private float _lastAttackRangeShowTime;

	public static readonly HeroSkillLocation[] HeroSkillTypes = new HeroSkillLocation[6]
	{
		HeroSkillLocation.Q,
		HeroSkillLocation.W,
		HeroSkillLocation.E,
		HeroSkillLocation.R,
		HeroSkillLocation.Identity,
		HeroSkillLocation.Movement
	};

	private const float DirectionalAttackYOffset = 0.75f;

	private static Vector3 _cachedPosition;

	private static int _cachedFrame = -1;

	private const float NearbyInteractableCheckInterval = 0.1f;

	private const float InteractionCheckRange = 6f;

	public Action<IInteractable> onFocusedInteractableChanged;

	private float _lastInteractAltUnscaledTime;

	private const float CastByKeyPressStaleTime = 1f;

	internal SampleCastInfoContext? _localSampleContext;

	internal CastInfo _currentCastInfo;

	private const float SampleUpdateInterval = 0.05f;

	private float _lastSampleUpdateTime = float.NegativeInfinity;

	internal Dictionary<HeroSkillLocation, (DewInputTrigger, float)> _castByKeyInfo = new Dictionary<HeroSkillLocation, (DewInputTrigger, float)>();

	public DewInputTrigger it_confirmCast;

	public DewInputTrigger it_move;

	public DewInputTrigger it_attackMoveNormal;

	public DewInputTrigger it_attackMoveImmediately;

	public DewInputTrigger it_attackMoveOnRelease;

	public DewInputTrigger it_attackInPlace;

	public DewInputTrigger it_scoreboard;

	public DewInputTrigger it_worldMap;

	public DewInputTrigger it_stop;

	public DewInputTrigger it_moveUp;

	public DewInputTrigger it_moveLeft;

	public DewInputTrigger it_moveDown;

	public DewInputTrigger it_moveRight;

	public DewInputTrigger it_skillQ;

	public DewInputTrigger it_skillW;

	public DewInputTrigger it_skillE;

	public DewInputTrigger it_skillR;

	public DewInputTrigger it_skillMovement;

	public DewInputTrigger it_skillQNormal;

	public DewInputTrigger it_skillWNormal;

	public DewInputTrigger it_skillENormal;

	public DewInputTrigger it_skillRNormal;

	public DewInputTrigger it_skillMovementNormal;

	public DewInputTrigger it_skillQImmediately;

	public DewInputTrigger it_skillWImmediately;

	public DewInputTrigger it_skillEImmediately;

	public DewInputTrigger it_skillRImmediately;

	public DewInputTrigger it_skillMovementImmediately;

	public DewInputTrigger it_skillQOnRelease;

	public DewInputTrigger it_skillWOnRelease;

	public DewInputTrigger it_skillEOnRelease;

	public DewInputTrigger it_skillROnRelease;

	public DewInputTrigger it_skillMovementOnRelease;

	public DewInputTrigger it_skillQSelf;

	public DewInputTrigger it_skillWSelf;

	public DewInputTrigger it_skillESelf;

	public DewInputTrigger it_skillRSelf;

	public DewInputTrigger it_skillQEdit;

	public DewInputTrigger it_skillWEdit;

	public DewInputTrigger it_skillEEdit;

	public DewInputTrigger it_skillREdit;

	public DewInputTrigger it_interact;

	public DewInputTrigger it_interactAlt;

	public DewInputTrigger it_editSkillHold;

	public DewInputTrigger it_editSkillToggle;

	public DewInputTrigger it_showDetails;

	public DewInputTrigger it_zoomOutCamera;

	public DewInputTrigger it_zoomInCamera;

	public DewInputTrigger it_chat;

	public DewInputTrigger it_ping;

	public DewInputTrigger it_travelVote;

	public DewInputTrigger it_travelVoteCancel;

	public DewInputTrigger it_spectatorNextTarget;

	public Entity controllingEntity
	{
		get
		{
			if (DewPlayer.local == null)
			{
				return null;
			}
			return DewPlayer.local.controllingEntity;
		}
	}

	public bool isCharacterControlEnabled => _disableCharacterControlCounter == 0;

	public bool shouldProcessCharacterInput { get; private set; }

	public bool shouldProcessCharacterInputAllowKnockedOut { get; private set; }

	public Vector3? aimDirection { get; private set; }

	public Vector3? aimPoint { get; private set; }

	public Entity targetEnemy { get; private set; }

	public bool isMovementSchemeDirectional { get; private set; }

	public bool isLastMovementDirectionFresh => Time.unscaledTime - _lastMovementDirectionUnscaledTime < 0.4f;

	public Vector3 lastMovementDirection { get; private set; }

	public float lastMovementStrength { get; private set; }

	public IInteractable focusedInteractable { get; private set; }

	public bool isFocusedInteractableAtCursor { get; private set; }

	private bool canSendSampleUpdate => Time.unscaledTime - _lastSampleUpdateTime > 0.05f;

	protected override void Awake()
	{
		base.Awake();
		InitializeTriggers();
	}

	private void Start()
	{
		onCastFailed = (Action<AbilityTrigger>)Delegate.Combine(onCastFailed, (Action<AbilityTrigger>)delegate(AbilityTrigger skill)
		{
			if (DewPlayer.local.hero.Skill.Movement == skill)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Control_DodgeUnavailable");
			}
			else
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Control_CastUnavailable");
			}
		});
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		shouldProcessCharacterInputAllowKnockedOut = GetShouldProcessCharacterInputAllowKnockedOut();
		shouldProcessCharacterInput = shouldProcessCharacterInputAllowKnockedOut && (!(controllingEntity is Hero h) || !h.isKnockedOut);
		if (shouldProcessCharacterInput)
		{
			GetCharInputOfVote();
		}
		GetScoreboardAndMapInput();
		if (!shouldProcessCharacterInput || InGameUIManager.instance.isWorldDisplayed != WorldDisplayStatus.None)
		{
			targetEnemy = null;
		}
		if (!shouldProcessCharacterInput)
		{
			if (ShouldProcessPingInput())
			{
				GetCharInputOfPing();
			}
			SetFocusedInteractable(null);
			state = default(ControlState);
			_isDoingContinuousAttack = false;
			aimDirection = null;
			aimPoint = null;
			if (_isDoingDirectionalMovement)
			{
				_isDoingDirectionalMovement = false;
				controllingEntity.Control.CmdClearMovement();
			}
		}
		else
		{
			Input.imeCompositionMode = IMECompositionMode.Off;
			if (Time.time - _lastInteractableCheck > 0.1f)
			{
				_lastInteractableCheck = Time.time;
				UpdateInteractableFocus(alsoCheckNearby: true);
			}
			else
			{
				UpdateInteractableFocus(alsoCheckNearby: false);
			}
			GetCharacterInputs();
		}
	}

	private void LateUpdate()
	{
		if (!(ManagerBase<DewCamera>.instance == null))
		{
			if (DewInput.currentMode != InputMode.Gamepad || targetEnemy == null)
			{
				targetEnemyIndicatorWorld.SetActive(value: false);
				targetEnemyIndicatorUi.SetActive(value: false);
			}
			else
			{
				targetEnemyIndicatorWorld.SetActive(value: true);
				targetEnemyIndicatorUi.SetActive(value: true);
				targetEnemyIndicatorWorld.transform.position = targetEnemy.position;
				targetEnemyIndicatorUi.transform.position = ManagerBase<DewCamera>.softInstance.mainCamera.WorldToScreenPoint(targetEnemy.Visual.GetCenterPosition());
			}
			if (aimPoint.HasValue)
			{
				aimPointUi.transform.position = ManagerBase<DewCamera>.softInstance.mainCamera.WorldToScreenPoint(aimPoint.Value + Vector3.up * 0.75f);
				aimPointUi.SetActive(value: true);
			}
			else
			{
				aimPointUi.SetActive(value: false);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (ManagerBase<CameraManager>.instance.isPlayingCutscene && controllingEntity != null && Time.time - _lastStopIssueTime > 0.25f)
		{
			_lastStopIssueTime = Time.time;
			controllingEntity.Control.CmdStop();
		}
	}

	public void EnableCharacterControls()
	{
		checked
		{
			_disableCharacterControlCounter = (byte)(unchecked((uint)_disableCharacterControlCounter) - 1u);
		}
	}

	public void DisableCharacterControls()
	{
		if (DewPlayer.local != null && DewPlayer.local.hero != null)
		{
			DewPlayer.local.hero.Control.CmdCancelOngoingChannels();
			DewPlayer.local.hero.Control.CmdStop();
		}
		checked
		{
			_disableCharacterControlCounter = (byte)(unchecked((uint)_disableCharacterControlCounter) + 1u);
		}
	}

	private void GetCharacterInputs()
	{
		if (isEditSkillDisabled && ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None)
		{
			if (_isDoingDirectionalMovement)
			{
				_isDoingDirectionalMovement = false;
				controllingEntity.Control.CmdClearMovement();
			}
			return;
		}
		GetCharInputOfAimDirection();
		UpdateTargetEnemy();
		GetCharInputOfInteractByButtonPress();
		GetCharInputOfMove();
		GetCharInputOfCast();
		GetCharInputOfAttack();
		GetCharInputOfStop();
		GetCharInputOfPing();
		GetCharInputOfConfirmCast();
	}

	private void GetCharInputOfAimDirection()
	{
		Vector2 axisVec = DewInput.GetRightJoystick();
		if (DewInput.currentMode == InputMode.Gamepad && (ManagerBase<GlobalUIManager>.instance.focused != null || (SingletonBehaviour<UI_EmoteWheel>.instance._gpeIsHolding && SingletonBehaviour<UI_EmoteWheel>.instance._gpeIsEmoteMode)))
		{
			axisVec = Vector2.zero;
		}
		if (axisVec == Vector2.zero)
		{
			aimDirection = null;
			aimPoint = null;
			return;
		}
		aimDirection = ManagerBase<CameraManager>.instance.entityCamAngleRotation * new Vector3(axisVec.x, 0f, axisVec.y);
		Vector3 agentPosition = controllingEntity.agentPosition;
		Vector3? vector = aimDirection * 8f;
		aimPoint = agentPosition + vector;
		if (Time.unscaledTime - _lastLookSendUnscaledTime > 0.05f && !controllingEntity.Control.isGamepadRotationLocked)
		{
			controllingEntity.Control.CmdLookInDirection(aimDirection.Value);
			_lastLookSendUnscaledTime = Time.unscaledTime;
		}
	}

	private void UpdateTargetEnemy()
	{
		if (targetEnemy != null && (!targetEnemy.isActive || targetEnemy.Status.hasInvisible))
		{
			targetEnemy = null;
		}
		if (InGameUIManager.instance.isWorldDisplayed != WorldDisplayStatus.None || Time.time - _lastTargetEnemyUpdateTime < 0.05f)
		{
			return;
		}
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> ents = DewPhysics.OverlapCircleAllEntities(out handle, aimPoint ?? controllingEntity.agentPosition, 12f, (Entity e) => controllingEntity.CheckEnemyOrNeutral(e) && !e.Status.hasInvisible && !(e is IDisableGamepadTargeting), new CollisionCheckSettings
		{
			includeUncollidable = true,
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (ents.Length == 0)
		{
			targetEnemy = null;
		}
		else
		{
			Entity nextTarget = ents[0];
			for (int i = 0; i < ents.Length; i++)
			{
				if (controllingEntity.GetRelation(ents[i]) == EntityRelation.Enemy)
				{
					nextTarget = ents[i];
					break;
				}
			}
			if (targetEnemy != nextTarget)
			{
				targetEnemy = nextTarget;
				targetEnemyIndicatorUi.transform.localScale = Vector3.one;
				targetEnemyIndicatorUi.transform.DOKill(complete: true);
				targetEnemyIndicatorUi.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.5f);
			}
		}
		handle.Return();
	}

	private void GetCharInputOfVote()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.isVoting)
		{
			if (it_travelVote.down)
			{
				DewPlayer.local.CmdSetIsReady(!DewPlayer.local.isReady);
			}
			if (it_travelVoteCancel.down && !DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut())
			{
				NetworkedManagerBase<ZoneManager>.instance.CmdCancelVote();
			}
		}
	}

	private void GetCharInputOfConfirmCast()
	{
		if (state.type == ControlStateType.AttackMove && it_confirmCast.down)
		{
			DoAttackMoveAtCursor();
			state = default(ControlState);
		}
		if (state.type != ControlStateType.Cast || !it_confirmCast.down)
		{
			return;
		}
		if (!ShouldInvalidateCurrentCast())
		{
			if (state.isCastInDirectionOfMovement ? CastAbilityInDirectionOfMovement(state.trigger) : CastAbilityAtCursor(state.trigger))
			{
				state.type = ControlStateType.None;
			}
		}
		else
		{
			state.type = ControlStateType.None;
		}
	}

	private void GetCharInputOfPing()
	{
		if (it_ping.down)
		{
			SendPingPC();
		}
	}

	private void SendPingPC()
	{
		PingManager.Ping ping = default(PingManager.Ping);
		_raycastResults = new List<RaycastResult>();
		Dew.RaycastAllUIElementsBelowCursor(_raycastResults);
		foreach (RaycastResult hit in _raycastResults)
		{
			IPingableWorldNode worldNode = hit.gameObject.GetComponentInParent<IPingableWorldNode>();
			if (worldNode != null)
			{
				ping.type = PingManager.PingType.WorldNode;
				ping.itemIndex = worldNode.nodeIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
			IPingableUIElement pingable = hit.gameObject.GetComponentInParent<IPingableUIElement>();
			if (pingable != null && pingable.pingTarget != null)
			{
				if (pingable.pingTarget is SkillTrigger skill)
				{
					ping.target = skill;
				}
				else if (pingable.pingTarget is Gem gem)
				{
					ping.target = gem;
				}
				ping.type = PingManager.PingType.EquippedItem;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
			IPingableShopItem shopItem = hit.gameObject.GetComponentInParent<IPingableShopItem>();
			if (shopItem != null && shopItem.shop != null)
			{
				ping.type = PingManager.PingType.ShopItem;
				ping.target = (NetworkBehaviour)shopItem.shop;
				ping.itemIndex = shopItem.merchandiseIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
			IPingableChoiceItem choiceItem = hit.gameObject.GetComponentInParent<IPingableChoiceItem>();
			if (choiceItem != null && choiceItem.shrine != null)
			{
				ping.type = PingManager.PingType.ChoiceItem;
				ping.target = (NetworkBehaviour)choiceItem.shrine;
				ping.itemIndex = choiceItem.choiceIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
		}
		IInteractable interactable = GetInteractableOnCursor();
		if (interactable != null)
		{
			ping.type = PingManager.PingType.Interactable;
			ping.target = (NetworkBehaviour)interactable;
			NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
		}
		else if (GetEntityOnCursor() != null)
		{
			Entity e = GetEntityOnCursor();
			ping.target = e;
			ping.type = PingManager.PingType.Entity;
			NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
		}
		else
		{
			ping.type = PingManager.PingType.Move;
			ping.position = GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false);
			NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
		}
	}

	public void SendPingGamepad()
	{
		PingManager.Ping ping = default(PingManager.Ping);
		if (ManagerBase<GlobalUIManager>.instance.focused != null)
		{
			MonoBehaviour f = (MonoBehaviour)ManagerBase<GlobalUIManager>.instance.focused;
			if (Check(f) || (f.TryGetComponent<IGamepadPingProxyParent>(out var proxy) && Check(proxy.pingableTarget)))
			{
				return;
			}
		}
		IInteractable interactable = focusedInteractable;
		Entity entity = targetEnemy;
		if (aimPoint.HasValue)
		{
			Vector3 screenPoint = ManagerBase<DewCamera>.instance.mainCamera.WorldToScreenPoint(aimPoint.Value);
			interactable = GetInteractableFromScreenPoint(screenPoint);
			if (interactable == null)
			{
				interactable = GetInteractableFromScreenPoint(screenPoint, 1f);
			}
			if (interactable == null)
			{
				interactable = GetInteractableFromScreenPoint(screenPoint, 2.5f);
			}
			if (interactable != null)
			{
				ping.type = PingManager.PingType.Interactable;
				ping.target = (NetworkBehaviour)interactable;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return;
			}
			entity = GetEntityFromScreenPoint(screenPoint);
			if (entity == null)
			{
				entity = GetEntityFromScreenPoint(screenPoint, 1f);
			}
			if (entity == null)
			{
				entity = GetEntityFromScreenPoint(screenPoint, 2.5f);
			}
			if (entity != null)
			{
				ping.target = entity;
				ping.type = PingManager.PingType.Entity;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
			}
			else
			{
				ping.type = PingManager.PingType.Move;
				ping.position = aimPoint.Value;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
			}
		}
		else if (interactable != null)
		{
			ping.type = PingManager.PingType.Interactable;
			ping.target = (NetworkBehaviour)interactable;
			NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
		}
		else
		{
			if (entity == null)
			{
				entity = ManagerBase<CameraManager>.instance.focusedEntity;
			}
			if (entity != null)
			{
				ping.target = entity;
				ping.type = PingManager.PingType.Entity;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
			}
		}
		bool Check(MonoBehaviour obj)
		{
			IPingableWorldNode worldNode = obj.GetComponentInParent<IPingableWorldNode>();
			if (worldNode != null)
			{
				ping.type = PingManager.PingType.WorldNode;
				ping.itemIndex = worldNode.nodeIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return true;
			}
			IPingableUIElement pingable = obj.GetComponentInParent<IPingableUIElement>();
			if (pingable != null && pingable.pingTarget != null)
			{
				if (pingable.pingTarget is SkillTrigger skill)
				{
					ping.target = skill;
				}
				else if (pingable.pingTarget is Gem gem)
				{
					ping.target = gem;
				}
				ping.type = PingManager.PingType.EquippedItem;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return true;
			}
			IPingableShopItem shopItem = obj.GetComponentInParent<IPingableShopItem>();
			if (shopItem != null && shopItem.shop != null)
			{
				ping.type = PingManager.PingType.ShopItem;
				ping.target = (NetworkBehaviour)shopItem.shop;
				ping.itemIndex = shopItem.merchandiseIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return true;
			}
			IPingableChoiceItem choiceItem = obj.GetComponentInParent<IPingableChoiceItem>();
			if (choiceItem != null && choiceItem.shrine != null)
			{
				ping.type = PingManager.PingType.ChoiceItem;
				ping.target = (NetworkBehaviour)choiceItem.shrine;
				ping.itemIndex = choiceItem.choiceIndex;
				NetworkedManagerBase<PingManager>.instance.CmdSendPing(ping);
				return true;
			}
			return false;
		}
	}

	private void GetScoreboardAndMapInput()
	{
		if (InGameUIManager.instance.IsState("Playing"))
		{
			if (it_scoreboard.down)
			{
				InGameUIManager.instance.isScoreboardDisplayed = true;
			}
			if (it_scoreboard.up && DewInput.currentMode == InputMode.KeyboardAndMouse)
			{
				InGameUIManager.instance.isScoreboardDisplayed = false;
			}
			if (isWorldMapDisabled || IsInputFieldFocused() || !it_worldMap.down)
			{
				return;
			}
			if (InGameUIManager.instance.isWorldDisplayed == WorldDisplayStatus.None)
			{
				if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss && Rift_RoomExit.instance != null && Rift_RoomExit.instance.isOpen && !Rift_RoomExit.instance.isLocked && !controllingEntity.IsNullInactiveDeadOrKnockedOut() && Vector2.Distance(Rift_RoomExit.instance.transform.position.ToXY(), controllingEntity.transform.position.ToXY()) < 8f)
				{
					InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.Shown;
				}
				else
				{
					InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.ShownNoMove;
				}
				if (controllingEntity != null)
				{
					controllingEntity.Control.CmdStop();
				}
			}
			else
			{
				InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.None;
			}
		}
		else if (InGameUIManager.instance.IsState("Menu"))
		{
			InGameUIManager.instance.isScoreboardDisplayed = false;
			InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.None;
		}
	}

	private void GetCharInputOfStop()
	{
		if (it_stop.down && Time.unscaledTime - _lastStopIssueTime > 0.1f)
		{
			_lastStopIssueTime = Time.unscaledTime;
			controllingEntity.Control.CmdStop();
		}
	}

	private bool InteractWithInteractableOnCursor()
	{
		IInteractable interactable = GetInteractableOnCursor();
		if (interactable != null && interactable.canInteractWithMouse)
		{
			controllingEntity.Control.CmdInteract(interactable, isAlt: false, isMouse: true);
			Component comp = (Component)interactable;
			UnityEngine.Object.Instantiate(interactMarker, comp.transform.position, Quaternion.identity).followTransform = comp.transform;
			HighlightProvider hl = comp.GetComponentInChildren<HighlightProvider>();
			if (hl != null)
			{
				hl.ShowClick();
			}
			return true;
		}
		return false;
	}

	private void GetCharInputOfMove()
	{
		if (isEditSkillDisabled && ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None)
		{
			return;
		}
		if (it_move.down)
		{
			if (state.type != ControlStateType.None)
			{
				state = default(ControlState);
			}
			if (NetworkedManagerBase<ConversationManager>.instance.hasOngoingLocalConversation || !InteractWithInteractableOnCursor())
			{
				if (DewSave.profile.controls.attackByIssuingMoveOnEnemy && controllingEntity.Ability.attackAbility != null)
				{
					TriggerConfig config = controllingEntity.Ability.attackAbility.currentConfig;
					if (GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), (Entity candidate) => config.targetValidator.Evaluate(controllingEntity, candidate), 0.75f) != null)
					{
						DoAttackMoveAtCursor(AttackMoveType.UseDistanceFromDestination);
						goto IL_00dd;
					}
				}
				MoveToCursor();
				_isContinuousMove = true;
			}
		}
		goto IL_00dd;
		IL_00dd:
		if (_isContinuousMove)
		{
			if (!it_move)
			{
				_isContinuousMove = false;
			}
			else if (Time.unscaledTime - _lastMovementDispatchUnscaledTime > 0.0625f)
			{
				Vector3 dest = GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false);
				dest = Dew.GetValidAgentDestination_Closest(controllingEntity.agentPosition, dest);
				controllingEntity.Control.CmdMoveToDestination(dest, immediately: true);
				_lastMovementDispatchUnscaledTime = Time.unscaledTime;
			}
		}
		Vector2 axisVec = DewInput.GetLeftJoystick();
		if (DewInput.currentMode == InputMode.Gamepad && (ManagerBase<GlobalUIManager>.instance.focused != null || (SingletonBehaviour<UI_EmoteWheel>.instance._gpeIsHolding && !SingletonBehaviour<UI_EmoteWheel>.instance._gpeIsRightStick)))
		{
			axisVec = Vector2.zero;
		}
		if (DewSave.profile.controls.enableDirMoveKeys)
		{
			DewInputTrigger keyUp = it_moveUp;
			DewInputTrigger keyLeft = it_moveLeft;
			DewInputTrigger keyDown = it_moveDown;
			DewInputTrigger keyRight = it_moveRight;
			if ((bool)keyUp && !keyDown)
			{
				axisVec.y = 1f;
			}
			else if (!keyUp && (bool)keyDown)
			{
				axisVec.y = -1f;
			}
			if ((bool)keyLeft && !keyRight)
			{
				axisVec.x = -1f;
			}
			else if (!keyLeft && (bool)keyRight)
			{
				axisVec.x = 1f;
			}
			if (AreControlsInverted())
			{
				axisVec *= -1f;
			}
		}
		float mag = axisVec.magnitude;
		if (mag > 1f)
		{
			axisVec = axisVec.normalized;
			mag = 1f;
		}
		if (mag > 0.0001f)
		{
			if (Time.unscaledTime - ManagerBase<FloatingWindowManager>.instance.lastSetTargetUnscaledTime > 0.3f)
			{
				ManagerBase<FloatingWindowManager>.instance.ClearTarget();
			}
			if (Time.unscaledTime - InGameUIManager.instance.lastWorldDisplayUnscaledTime > 0.3f)
			{
				InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.None;
			}
			if (ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None && ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.Regular && Time.unscaledTime - ManagerBase<EditSkillManager>.instance.lastModeSetUnscaledTime > 0.3f)
			{
				ManagerBase<EditSkillManager>.instance.EndEdit();
			}
			_isDoingDirectionalMovement = true;
			isMovementSchemeDirectional = true;
			lastMovementDirection = (ManagerBase<CameraManager>.instance.entityCamAngleRotation * axisVec.ToXZ()).normalized;
			lastMovementStrength = mag;
			_lastMovementDirectionUnscaledTime = Time.unscaledTime;
			if (Time.unscaledTime - _lastMovementDispatchUnscaledTime > 1f / 60f)
			{
				controllingEntity.Control.CmdMoveWithDirection(lastMovementDirection * (0.4f + 0.6f * mag));
				_lastMovementDispatchUnscaledTime = Time.unscaledTime;
			}
		}
		else if (_isDoingDirectionalMovement)
		{
			_isDoingDirectionalMovement = false;
			controllingEntity.Control.CmdClearMovement();
		}
	}

	private void GetCharInputOfAttack()
	{
		if (controllingEntity.Ability.attackAbility == null)
		{
			return;
		}
		if (DewSave.profile.controls.gamepadAttackByAiming && aimDirection.HasValue && aimDirection.Value.magnitude > 0.99f && (!SingletonBehaviour<UI_EmoteWheel>.instance._gpeIsHolding || !SingletonBehaviour<UI_EmoteWheel>.instance._gpeIsRightStick) && Time.unscaledTime - SingletonBehaviour<UI_EmoteWheel>.instance._gpeEndUnscaledTime > 0.4f && controllingEntity.Control.IsActionBlocked(EntityControl.BlockableAction.Attack) == EntityControl.BlockStatus.Allowed)
		{
			DoAttackInPlaceWithAim();
		}
		if (it_attackMoveImmediately.down)
		{
			_lastAttackMoveDispatchTime = Time.time;
			DoAttackMoveAtCursor();
		}
		else if ((bool)it_attackMoveImmediately && Time.time - _lastAttackMoveDispatchTime > 1f / 60f)
		{
			_lastAttackMoveDispatchTime = Time.time;
			DoAttackMoveAtCursor(AttackMoveType.UseUserSettings, hideIndicator: true);
		}
		if (it_attackMoveNormal.down)
		{
			state.type = ControlStateType.AttackMove;
			state.castType = CastConfirmType.Normal;
		}
		if (it_attackMoveOnRelease.down)
		{
			state.type = ControlStateType.AttackMove;
			state.castType = CastConfirmType.OnRelease;
		}
		if (state.type == ControlStateType.AttackMove && state.castType == CastConfirmType.OnRelease && it_attackMoveOnRelease.up)
		{
			DoAttackMoveAtCursor();
			state = default(ControlState);
		}
		if (it_attackInPlace.down)
		{
			if (GetInteractableOnCursor() is Entity || !InteractWithInteractableOnCursor())
			{
				_lastNoTargetShowTime = float.NegativeInfinity;
				_isDoingContinuousAttack = true;
			}
		}
		else if (_isDoingContinuousAttack && !it_attackInPlace)
		{
			_isDoingContinuousAttack = false;
		}
		if (!_isDoingContinuousAttack || !(Time.time - _lastAttackInPlaceDispatchTime > 1f / 60f))
		{
			return;
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			if (aimDirection.HasValue)
			{
				DoAttackInPlaceWithAim();
			}
			else
			{
				DoAttackInPlaceAuto();
			}
		}
		else
		{
			DoAttackInPlaceAtCursor();
		}
		_lastAttackInPlaceDispatchTime = Time.time;
	}

	private void GetCharInputOfCast()
	{
		if ((bool)it_editSkillHold)
		{
			return;
		}
		bool isSamplingCastInfo = ProcessCastInfoSampling();
		if (controllingEntity is Hero)
		{
			CheckSkillCast(HeroSkillLocation.Q, it_skillQ, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.W, it_skillW, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.E, it_skillE, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.R, it_skillR, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.Movement, it_skillMovement, isSelfCast: false, null);
			CheckSkillCast(HeroSkillLocation.Q, it_skillQSelf, isSelfCast: true, null);
			CheckSkillCast(HeroSkillLocation.W, it_skillWSelf, isSelfCast: true, null);
			CheckSkillCast(HeroSkillLocation.E, it_skillESelf, isSelfCast: true, null);
			CheckSkillCast(HeroSkillLocation.R, it_skillRSelf, isSelfCast: true, null);
			CheckSkillCast(HeroSkillLocation.Q, it_skillQImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.W, it_skillWImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.E, it_skillEImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.R, it_skillRImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.Movement, it_skillMovementImmediately, isSelfCast: false, CastConfirmType.Immediately);
			CheckSkillCast(HeroSkillLocation.Q, it_skillQOnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.W, it_skillWOnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.E, it_skillEOnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.R, it_skillROnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.Movement, it_skillMovementOnRelease, isSelfCast: false, CastConfirmType.OnRelease);
			CheckSkillCast(HeroSkillLocation.Q, it_skillQNormal, isSelfCast: false, CastConfirmType.Normal);
			CheckSkillCast(HeroSkillLocation.W, it_skillWNormal, isSelfCast: false, CastConfirmType.Normal);
			CheckSkillCast(HeroSkillLocation.E, it_skillENormal, isSelfCast: false, CastConfirmType.Normal);
			CheckSkillCast(HeroSkillLocation.R, it_skillRNormal, isSelfCast: false, CastConfirmType.Normal);
			CheckSkillCast(HeroSkillLocation.Movement, it_skillMovementNormal, isSelfCast: false, CastConfirmType.Normal);
		}
		if (state.type != ControlStateType.Cast || state.castType != CastConfirmType.OnRelease || !DewInput.GetButtonUp(state.castKey, checkGameAreaForMouse: false))
		{
			return;
		}
		if (!ShouldInvalidateCurrentCast())
		{
			if (state.isCastInDirectionOfMovement)
			{
				CastAbilityInDirectionOfMovement(state.trigger);
			}
			else
			{
				CastAbilityAtCursor(state.trigger);
			}
		}
		state = default(ControlState);
		void CheckSkillCast(HeroSkillLocation type, DewInputTrigger it, bool isSelfCast, CastConfirmType? confirmType)
		{
			Hero hero = (Hero)controllingEntity;
			if ((!isMainSkillDisabled || !type.IsMainSkill()) && (!isDodgeDisabled || type != HeroSkillLocation.Movement) && it.down && hero.Skill.TryGetSkill(type, out var skill))
			{
				if (!skill.currentConfig.isActive)
				{
					onCastFailed?.Invoke(skill);
				}
				else if (!skill.CanBeReserved())
				{
					onCastFailed?.Invoke(skill);
				}
				else if (isSamplingCastInfo && !skill.currentConfig.ignoreBlock)
				{
					onCastFailed?.Invoke(skill);
				}
				else
				{
					ManagerBase<FloatingWindowManager>.instance.ClearTarget();
					InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.None;
					if (ManagerBase<EditSkillManager>.instance.currentProvider != null)
					{
						ManagerBase<EditSkillManager>.instance.EndEdit();
					}
					if (isSelfCast)
					{
						bool isSelfCastValid;
						switch (skill.currentConfig.castMethod.type)
						{
						case CastMethodType.None:
						case CastMethodType.Cone:
						case CastMethodType.Arrow:
						case CastMethodType.Point:
							isSelfCastValid = true;
							break;
						case CastMethodType.Target:
							isSelfCastValid = skill.currentConfig.targetValidator.Evaluate(controllingEntity, controllingEntity);
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
						if (isSelfCastValid)
						{
							CastInfo info;
							switch (skill.currentConfig.castMethod.type)
							{
							case CastMethodType.None:
								info = new CastInfo(controllingEntity);
								break;
							case CastMethodType.Cone:
							case CastMethodType.Arrow:
								info = new CastInfo(controllingEntity, CastInfo.GetAngle(controllingEntity.rotation));
								break;
							case CastMethodType.Target:
								controllingEntity.Visual.highlight.ShowClick();
								info = new CastInfo(controllingEntity, controllingEntity);
								break;
							case CastMethodType.Point:
								info = new CastInfo(controllingEntity, controllingEntity.agentPosition);
								break;
							default:
								throw new ArgumentOutOfRangeException();
							}
							bool shouldMoveToCast = !_isDoingDirectionalMovement && !_isContinuousMove;
							CastAbility(skill, info, shouldMoveToCast);
							SetCastByKeyFlag(skill, value: true, it);
							return;
						}
					}
					bool castInDirectionOfMovement = ShouldCastInDirectionOfMovement(skill);
					if (DewInput.currentMode == InputMode.Gamepad)
					{
						bool didSuccess = ((castInDirectionOfMovement && (skill.currentConfig.ignoreAimDirectionGamepad || !aimDirection.HasValue)) ? CastAbilityInDirectionOfMovement(skill) : CastAbilityGamepad(skill));
						SetCastByKeyFlag(skill, didSuccess, it);
					}
					else
					{
						CastConfirmType castType = (confirmType.HasValue ? confirmType.Value : GetAbilityCastType(skill));
						if (castType == CastConfirmType.Immediately || skill.currentConfig.castMethod.type == CastMethodType.None || skill.currentConfig.alwaysCastImmediately)
						{
							bool didSuccess2 = (castInDirectionOfMovement ? CastAbilityInDirectionOfMovement(skill) : CastAbilityAtCursor(skill));
							SetCastByKeyFlag(skill, didSuccess2, it);
						}
						else if (castType == CastConfirmType.Normal || castType == CastConfirmType.OnRelease)
						{
							state = new ControlState
							{
								type = ControlStateType.Cast,
								trigger = skill,
								configIndex = skill.currentConfigIndex,
								castKey = it._binding,
								castType = castType,
								isCastInDirectionOfMovement = castInDirectionOfMovement
							};
						}
					}
				}
			}
		}
	}

	private bool GetShouldProcessCharacterInputAllowKnockedOut()
	{
		if (!NetworkClient.active || controllingEntity.IsNullOrInactive() || !InGameUIManager.instance.IsState("Playing") || !isCharacterControlEnabled || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || ManagerBase<MessageManager>.instance.isShowingMessage || InGameUIManager.instance.disablePlayingInput)
		{
			return false;
		}
		if (IsInputFieldFocused())
		{
			return false;
		}
		return true;
	}

	public bool ShouldProcessPingInput()
	{
		if (!NetworkClient.active || !InGameUIManager.instance.IsState("Playing") || !isCharacterControlEnabled || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			return false;
		}
		if (IsInputFieldFocused())
		{
			return false;
		}
		return true;
	}

	public static bool IsInputFieldFocused()
	{
		if (_isInputFieldFocusedCachedFrame != Time.frameCount)
		{
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
			{
				GameObject gobj = EventSystem.current.currentSelectedGameObject;
				_isInputFieldFocused = (gobj.TryGetComponent<InputField>(out var ipf) && ipf.isActiveAndEnabled && ipf.interactable) || (gobj.TryGetComponent<TMP_InputField>(out var tpf) && tpf.isActiveAndEnabled && tpf.interactable);
			}
			else
			{
				_isInputFieldFocused = false;
			}
			_isInputFieldFocusedCachedFrame = Time.frameCount;
		}
		return _isInputFieldFocused;
	}

	public bool CastAbilityInDirectionOfMovement(AbilityTrigger trigger)
	{
		Vector3 dir = (isLastMovementDirectionFresh ? (lastMovementDirection * lastMovementStrength) : controllingEntity.transform.forward.Flattened().normalized);
		CastInfo info;
		switch (trigger.currentConfig.castMethod.type)
		{
		case CastMethodType.None:
			info = new CastInfo(controllingEntity);
			break;
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			info = new CastInfo(controllingEntity, CastInfo.GetAngle(dir));
			break;
		case CastMethodType.Target:
			return false;
		case CastMethodType.Point:
		{
			Vector3 offset = dir * trigger.currentConfig.castMethod.GetEffectiveRange();
			Vector3 pos = controllingEntity.agentPosition + offset;
			info = new CastInfo(controllingEntity, pos);
			break;
		}
		default:
			return false;
		}
		CastAbility(trigger, info, shouldMoveToCast: true);
		return true;
	}

	public bool CastAbilityAtCursor(AbilityTrigger trigger)
	{
		CastInfo info;
		switch (trigger.currentConfig.castMethod.type)
		{
		case CastMethodType.None:
			info = new CastInfo(controllingEntity);
			break;
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			info = new CastInfo(controllingEntity, CastInfo.GetAngle(GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: true) - controllingEntity.transform.position));
			break;
		case CastMethodType.Target:
		{
			Entity target = GetEntityOnCursor(controllingEntity, trigger.currentConfig.targetValidator, GetAimAssistSphereCastRadius());
			if (target == null && DewSave.profile.controls.autoTargetSelfIfPossible && trigger.currentConfig.targetValidator.Evaluate(controllingEntity, controllingEntity))
			{
				target = controllingEntity;
			}
			if (target == null)
			{
				ShowNoTargetIndicatorAtCursor();
				return false;
			}
			target.Visual.highlight.ShowClick();
			info = new CastInfo(controllingEntity, target);
			break;
		}
		case CastMethodType.Point:
			info = new CastInfo(controllingEntity, GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false));
			break;
		default:
			return false;
		}
		bool shouldMoveToCast = !_isDoingDirectionalMovement && !_isContinuousMove;
		CastAbility(trigger, info, shouldMoveToCast);
		return true;
	}

	public void CastAbility(AbilityTrigger trigger, CastInfo info, bool shouldMoveToCast)
	{
		if ((!isMainSkillDisabled || !(trigger is SkillTrigger st) || !(controllingEntity is Hero h) || !h.Skill.TryGetSkillLocation(st, out var type) || !type.IsMainSkill()) && (!isDodgeDisabled || !(trigger is SkillTrigger st2) || !(controllingEntity is Hero h2) || !h2.Skill.TryGetSkillLocation(st2, out var type2) || type2 != HeroSkillLocation.Movement))
		{
			controllingEntity.Control.CmdCast(trigger, trigger.currentConfigIndex, info, shouldMoveToCast, skipRangeCheck: false);
			if (!trigger.currentConfig.postponeBasicCommand)
			{
				controllingEntity.Control.CmdAttack(null, doChase: false);
			}
		}
	}

	private void DoAttackInPlaceAtWorldPosition(Vector3 pos)
	{
		AttackTrigger attack = controllingEntity.Ability.attackAbility as AttackTrigger;
		if (attack.IsNullOrInactive())
		{
			return;
		}
		if (!isMovementSchemeDirectional)
		{
			controllingEntity.Control.CmdClearMovement();
		}
		ReadOnlySpan<Entity> readOnlySpan2;
		if (attack.allowNonTargetedCast)
		{
			Entity bestTarget = null;
			if (!DewSave.profile.controls.turnOffAimAssistMeleeDirectionalAttack || !(controllingEntity is Hero h) || !Dew.IsMeleeHero(h.classType))
			{
				ArrayReturnHandle<Entity> handle;
				ReadOnlySpan<Entity> readOnlySpan = DewPhysics.OverlapCircleAllEntities(out handle, pos, GetAimAssistAttackTargetRange(), attack.currentConfig.targetValidator, controllingEntity);
				float bestScore = float.NegativeInfinity;
				readOnlySpan2 = readOnlySpan;
				for (int i = 0; i < readOnlySpan2.Length; i++)
				{
					Entity e = readOnlySpan2[i];
					float score = 0f - Vector3.Distance(pos, e.agentPosition);
					if (attack.currentConfig.CheckRange(controllingEntity, e))
					{
						score += 1000f;
					}
					if (bestScore < score)
					{
						bestScore = score;
						bestTarget = e;
					}
				}
				handle.Return();
				if (bestTarget != null)
				{
					pos = bestTarget.position;
				}
			}
			float angle = CastInfo.GetAngle(pos.Flattened() - controllingEntity.agentPosition.Flattened());
			Vector3 delta = pos - controllingEntity.agentPosition;
			Vector3 point = controllingEntity.agentPosition + Vector3.ClampMagnitude(delta, attack.currentConfig.effectiveRange);
			if (bestTarget != null && attack.currentConfig.CheckRange(controllingEntity, bestTarget))
			{
				controllingEntity.Control.CmdCast(attack, attack.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					target = bestTarget
				}, allowMoveToCast: false, skipRangeCheck: true);
			}
			else
			{
				controllingEntity.Control.CmdCast(attack, attack.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					angle = angle,
					point = point
				}, allowMoveToCast: false, skipRangeCheck: false);
			}
			return;
		}
		ArrayReturnHandle<Entity> handle2;
		ReadOnlySpan<Entity> readOnlySpan3 = DewPhysics.OverlapCircleAllEntities(out handle2, pos, 6f, attack.currentConfig.targetValidator, controllingEntity);
		float bestScore2 = float.NegativeInfinity;
		Entity bestTarget2 = null;
		readOnlySpan2 = readOnlySpan3;
		for (int i = 0; i < readOnlySpan2.Length; i++)
		{
			Entity e2 = readOnlySpan2[i];
			float score2 = 0f - Vector3.Distance(pos, e2.agentPosition);
			if (attack.currentConfig.CheckRange(controllingEntity, e2))
			{
				score2 += 1000f;
			}
			if (bestScore2 < score2)
			{
				bestScore2 = score2;
				bestTarget2 = e2;
			}
		}
		handle2.Return();
		if (bestTarget2 == null)
		{
			if (Time.time - _lastNoTargetShowTime > 2.5f)
			{
				_lastNoTargetShowTime = Time.time;
				ShowNoTargetIndicatorAtCursor();
			}
		}
		else
		{
			controllingEntity.Control.CmdAttack(bestTarget2, doChase: false);
		}
	}

	private void DoAttackInPlaceWithAim()
	{
		Vector3 dir = aimDirection.Value;
		AttackTrigger attack = controllingEntity.Ability.attackAbility as AttackTrigger;
		if (attack.IsNullOrInactive())
		{
			return;
		}
		float effectiveRange = attack.currentConfig.effectiveRange;
		ReadOnlySpan<Entity> readOnlySpan2;
		if (attack.allowNonTargetedCast)
		{
			Entity bestTarget = null;
			if (!DewSave.profile.controls.turnOffAimAssistMeleeDirectionalAttack || !(controllingEntity is Hero h) || !Dew.IsMeleeHero(h.classType))
			{
				ArrayReturnHandle<Entity> handle;
				ReadOnlySpan<Entity> readOnlySpan = DewPhysics.OverlapCircleAllEntities(out handle, controllingEntity.agentPosition, effectiveRange + 5f, attack.currentConfig.targetValidator, controllingEntity);
				float bestScore = float.NegativeInfinity;
				readOnlySpan2 = readOnlySpan;
				for (int i = 0; i < readOnlySpan2.Length; i++)
				{
					Entity e = readOnlySpan2[i];
					float num = Vector2.Angle(dir.ToXY(), (e.agentPosition - controllingEntity.agentPosition).ToXY());
					float score = 0f - num;
					if (attack.currentConfig.CheckRange(controllingEntity, e))
					{
						score += 30f;
					}
					if (!(num > GetAimAssistAttackTargetAngleGamepad()) && bestScore < score)
					{
						bestScore = score;
						bestTarget = e;
					}
				}
				handle.Return();
				if (bestTarget != null)
				{
					dir = (bestTarget.agentPosition - controllingEntity.agentPosition).Flattened().normalized;
				}
			}
			if (bestTarget != null && attack.currentConfig.CheckRange(controllingEntity, bestTarget))
			{
				controllingEntity.Control.CmdCast(attack, attack.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					target = bestTarget
				}, allowMoveToCast: false, skipRangeCheck: true);
				return;
			}
			float angle = CastInfo.GetAngle(dir);
			Vector3 point = controllingEntity.agentPosition + dir * attack.currentConfig.effectiveRange;
			controllingEntity.Control.CmdCast(attack, attack.currentConfigIndex, new CastInfo
			{
				caster = controllingEntity,
				angle = angle,
				point = point
			}, allowMoveToCast: false, skipRangeCheck: false);
			return;
		}
		ArrayReturnHandle<Entity> handle2;
		ReadOnlySpan<Entity> readOnlySpan3 = DewPhysics.OverlapCircleAllEntities(out handle2, controllingEntity.agentPosition, effectiveRange + 5f, attack.currentConfig.targetValidator, controllingEntity);
		float bestScore2 = float.NegativeInfinity;
		Entity bestTarget2 = null;
		readOnlySpan2 = readOnlySpan3;
		for (int i = 0; i < readOnlySpan2.Length; i++)
		{
			Entity e2 = readOnlySpan2[i];
			float score2 = 0f - Vector2.Angle(dir.ToXY(), (e2.agentPosition - controllingEntity.agentPosition).ToXY());
			if (attack.currentConfig.CheckRange(controllingEntity, e2))
			{
				score2 += 1000f;
			}
			if (bestScore2 < score2)
			{
				bestScore2 = score2;
				bestTarget2 = e2;
			}
		}
		handle2.Return();
		if (bestTarget2 == null)
		{
			if (Time.time - _lastNoTargetShowTime > 2.5f)
			{
				_lastNoTargetShowTime = Time.time;
				ShowNoTargetIndicatorWithAim();
				ManagerBase<CastIndicatorManager>.instance.IndicateRangeFailure(effectiveRange);
			}
		}
		else
		{
			controllingEntity.Control.CmdAttack(bestTarget2, doChase: false);
		}
	}

	private void DoAttackInPlaceAtCursor()
	{
		DoAttackInPlaceAtWorldPosition(GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: true));
	}

	private void DoAttackInPlaceAuto()
	{
		AttackTrigger attack = controllingEntity.Ability.attackAbility as AttackTrigger;
		if (attack.IsNullOrInactive())
		{
			return;
		}
		if (attack.allowNonTargetedCast)
		{
			Vector3 dir = (isLastMovementDirectionFresh ? (lastMovementDirection * lastMovementStrength) : controllingEntity.transform.forward.Flattened().normalized);
			float angle = ((targetEnemy != null) ? CastInfo.GetAngle(targetEnemy.agentPosition.Flattened() - controllingEntity.agentPosition.Flattened()) : CastInfo.GetAngle(dir));
			Vector3 point = ((!(targetEnemy != null)) ? (controllingEntity.agentPosition + dir * attack.currentConfig.effectiveRange) : (controllingEntity.agentPosition + Vector3.ClampMagnitude(targetEnemy.agentPosition - controllingEntity.agentPosition, attack.currentConfig.effectiveRange)));
			if (targetEnemy != null && attack.currentConfig.CheckRange(controllingEntity, targetEnemy))
			{
				controllingEntity.Control.CmdCast(attack, attack.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					target = targetEnemy
				}, allowMoveToCast: false, skipRangeCheck: true);
			}
			else
			{
				controllingEntity.Control.CmdCast(attack, attack.currentConfigIndex, new CastInfo
				{
					caster = controllingEntity,
					angle = angle,
					point = point
				}, allowMoveToCast: false, skipRangeCheck: false);
			}
		}
		else if (targetEnemy == null || !attack.currentConfig.CheckRange(controllingEntity, targetEnemy))
		{
			if (Time.time - _lastNoTargetShowTime > 2.5f)
			{
				_lastNoTargetShowTime = Time.time;
				ShowNoTargetIndicatorWithAim();
				if (Time.time - _lastAttackRangeShowTime > 3f)
				{
					ManagerBase<CastIndicatorManager>.instance.IndicateRangeFailure(attack.currentConfig.effectiveRange);
					_lastAttackRangeShowTime = Time.time;
				}
			}
		}
		else
		{
			controllingEntity.Control.CmdAttack(targetEnemy, doChase: false);
		}
	}

	private void DoAttackMoveAtCursor(AttackMoveType type = AttackMoveType.UseUserSettings, bool hideIndicator = false)
	{
		if (controllingEntity.Ability.attackAbility != null)
		{
			Entity entUnderCursor = GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), controllingEntity, controllingEntity.Ability.attackAbility.currentConfig.targetValidator, GetAimAssistSphereCastRadius());
			if (entUnderCursor != null)
			{
				if (!_isContinuousMove)
				{
					controllingEntity.Control.CmdClearMovement();
				}
				controllingEntity.Control.CmdAttack(entUnderCursor, !_isContinuousMove);
				if (!hideIndicator)
				{
					entUnderCursor.Visual.highlight.ShowClick();
				}
				if (!hideIndicator)
				{
					UnityEngine.Object.Instantiate(attackMarker, entUnderCursor.Visual.GetBasePosition(), Quaternion.identity).followTransform = entUnderCursor.transform;
				}
				return;
			}
			Vector2 pivot = ((type == AttackMoveType.UseDistanceFromDestination || (type == AttackMoveType.UseUserSettings && DewSave.profile.controls.attackMoveUseDistanceFromDestination)) ? GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false).ToXY() : controllingEntity.position.ToXY());
			Entity ent = ActionAttackMove.FindAttackMoveTarget(controllingEntity, pivot);
			if (ent != null)
			{
				if (!_isContinuousMove)
				{
					controllingEntity.Control.CmdClearMovement();
				}
				controllingEntity.Control.CmdAttack(ent, !_isContinuousMove);
				if (!hideIndicator)
				{
					UnityEngine.Object.Instantiate(attackMarker, GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false), Quaternion.identity);
				}
			}
			else
			{
				Vector3 position = GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false);
				if (!hideIndicator)
				{
					UnityEngine.Object.Instantiate(attackMarker, position, Quaternion.identity);
				}
				if (!_isContinuousMove)
				{
					controllingEntity.Control.CmdAttackMove(position, DewSave.profile.controls.attackMoveUseDistanceFromDestination);
				}
			}
		}
		else
		{
			MoveToCursor();
		}
	}

	private void MoveToCursor()
	{
		isMovementSchemeDirectional = false;
		Vector3 position = GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false);
		controllingEntity.Control.CmdMoveToDestination(position, immediately: true);
		UnityEngine.Object.Instantiate(moveMarker, position, Quaternion.identity);
	}

	private void ShowNoTargetIndicatorWithAim()
	{
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Control_CastUnavailable");
		if (UnityEngine.Object.Instantiate(noTargetIndicator, Vector3.left * 1000f, Quaternion.identity, noTargetIndicatorParent).TryGetComponent<IWorldPositionReceiver>(out var c))
		{
			c.SetWorldPosition(aimPoint ?? controllingEntity.agentPosition);
		}
	}

	private void ShowNoTargetIndicatorAtCursor()
	{
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Control_CastUnavailable");
		if (UnityEngine.Object.Instantiate(noTargetIndicator, GetMousePositionWithInversionInMind(), Quaternion.identity, noTargetIndicatorParent).TryGetComponent<IWorldPositionReceiver>(out var c))
		{
			c.SetWorldPosition(GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false));
		}
	}

	public bool CastAbilityGamepad(AbilityTrigger trigger)
	{
		if (!aimDirection.HasValue)
		{
			return CastAbilityAuto(trigger);
		}
		return CastAbilityWithAim(trigger);
	}

	public bool CastAbilityWithAim(AbilityTrigger trigger)
	{
		CastInfo info;
		switch (trigger.currentConfig.castMethod.type)
		{
		case CastMethodType.None:
			info = new CastInfo(controllingEntity);
			break;
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			info = new CastInfo(controllingEntity, CastInfo.GetAngle(aimDirection.Value));
			break;
		case CastMethodType.Target:
		{
			Entity target = targetEnemy;
			if (target.IsNullInactiveDeadOrKnockedOut() || !trigger.currentConfig.targetValidator.Evaluate(controllingEntity, target) || !trigger.currentConfig.CheckRange(controllingEntity, target))
			{
				target = null;
				ArrayReturnHandle<Entity> handle;
				ReadOnlySpan<Entity> ents = DewPhysics.OverlapCircleAllEntities(out handle, aimPoint.Value, 12f, (Entity e) => trigger.currentConfig.targetValidator.Evaluate(controllingEntity, e) && trigger.currentConfig.CheckRange(controllingEntity, e) && Vector3.Angle(aimDirection.Value, e.agentPosition - controllingEntity.agentPosition) < 80f, new CollisionCheckSettings
				{
					sortComparer = CollisionCheckSettings.DistanceFromCenter,
					includeUncollidable = true
				});
				if (ents.Length > 0)
				{
					target = ents[0];
				}
				handle.Return();
			}
			if (target.IsNullInactiveDeadOrKnockedOut())
			{
				ManagerBase<CastIndicatorManager>.instance.IndicateRangeFailure(trigger.currentConfig.effectiveRange);
				ShowNoTargetIndicatorWithAim();
				return false;
			}
			info = new CastInfo(controllingEntity, target);
			break;
		}
		case CastMethodType.Point:
			info = new CastInfo(controllingEntity, aimPoint.Value);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		CastAbility(trigger, info, shouldMoveToCast: true);
		return true;
	}

	public bool CastAbilityAuto(AbilityTrigger trigger)
	{
		Entity target = targetEnemy;
		if (target.IsNullInactiveDeadOrKnockedOut() || !trigger.currentConfig.targetValidator.Evaluate(controllingEntity, target) || (trigger.currentConfig.castMethod.type == CastMethodType.Target && !trigger.currentConfig.CheckRange(controllingEntity, target)))
		{
			if (trigger.currentConfig.castMethod.type == CastMethodType.Target)
			{
				target = null;
			}
			float effRange = trigger.currentConfig.effectiveRange;
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> ents = DewPhysics.OverlapCircleAllEntities(out handle, controllingEntity.agentPosition, effRange, (Entity e) => trigger.currentConfig.targetValidator.Evaluate(controllingEntity, e), new CollisionCheckSettings
			{
				sortComparer = CollisionCheckSettings.DistanceFromCenter
			});
			if (ents.Length > 0)
			{
				target = ents[0];
			}
			handle.Return();
		}
		CastInfo info;
		if (!target.IsNullInactiveDeadOrKnockedOut())
		{
			info = trigger.GetPredictedCastInfoToTarget(target, 0.5f);
		}
		else
		{
			Vector3 dir = (isLastMovementDirectionFresh ? (lastMovementDirection * lastMovementStrength) : controllingEntity.transform.forward.Flattened().normalized);
			switch (trigger.currentConfig.castMethod.type)
			{
			case CastMethodType.None:
				info = new CastInfo(controllingEntity);
				break;
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
				info = new CastInfo(controllingEntity, CastInfo.GetAngle(dir));
				break;
			case CastMethodType.Target:
				ShowNoTargetIndicatorWithAim();
				ManagerBase<CastIndicatorManager>.instance.IndicateRangeFailure(trigger.currentConfig.effectiveRange);
				return false;
			case CastMethodType.Point:
				info = new CastInfo(controllingEntity, controllingEntity.agentPosition + dir * trigger.currentConfig.castMethod.pointData.range);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		CastAbility(trigger, info, shouldMoveToCast: true);
		return true;
	}

	public bool ShouldCastInDirectionOfMovement(AbilityTrigger trigger)
	{
		if (DewInput.currentMode == InputMode.KeyboardAndMouse && isMovementSchemeDirectional && trigger.currentConfig.castByMoveDirectionByDefault)
		{
			switch (DewSave.profile.controls.dashDirectionWhenDirectionalMovement)
			{
			case DashDirection.CurrentMovement:
				return true;
			case DashDirection.CurrentMovementOnlyDodge:
				return trigger.GetType().Name.Contains("_M_");
			case DashDirection.AllTowardsCursor:
				return false;
			}
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			return trigger.currentConfig.castByMoveDirectionGamepad;
		}
		return false;
	}

	public DewBinding GetSkillBinding(HeroSkillLocation type)
	{
		return DewSave.profile.controls.GetSkillBinding(type);
	}

	public DewInputTrigger GetSkillInputTrigger(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => it_skillQ, 
			HeroSkillLocation.W => it_skillW, 
			HeroSkillLocation.E => it_skillE, 
			HeroSkillLocation.R => it_skillR, 
			HeroSkillLocation.Movement => it_skillMovement, 
			_ => DewInputTrigger.MockTrigger, 
		};
	}

	public static Entity GetEntityFromScreenPoint(Vector2 screenPoint, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(screenPoint, (Entity _) => true, sphereCastRadius);
	}

	public static Entity GetEntityFromScreenPoint(Vector2 screenPoint, Entity self, IBinaryEntityValidator validator, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(screenPoint, (Entity entity) => validator.Evaluate(self, entity), sphereCastRadius);
	}

	public static Entity GetEntityFromScreenPoint(Vector2 screenPoint, IEntityValidator validator, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(screenPoint, (Entity entity) => validator.Evaluate(entity), sphereCastRadius);
	}

	public static T GetFromScreenPoint<T>(Vector2 screenPoint, int layerMask, Func<T, bool> validator = null, float sphereCastRadius = -1f)
	{
		Ray ray = Dew.mainCamera.ScreenPointToRay(screenPoint);
		RaycastHit[] hits = Physics.RaycastAll(ray, 200f, layerMask, QueryTriggerInteraction.Collide);
		Array.Sort(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
		for (int i = 0; i < hits.Length; i++)
		{
			T t = hits[i].collider.GetComponentInParent<T>();
			if (t == null || t is Actor { isActive: false } || t is Hero { isKnockedOut: not false })
			{
				continue;
			}
			if (t is Component c)
			{
				Actor a2 = c.GetComponentInParent<Actor>();
				if ((a2 != null && !a2.isActive) || a2 is Hero { isKnockedOut: not false })
				{
					continue;
				}
			}
			if (validator == null || validator(t))
			{
				return t;
			}
		}
		if (sphereCastRadius <= 0f)
		{
			return default(T);
		}
		hits = Physics.SphereCastAll(Dew.mainCamera.ScreenPointToRay(screenPoint), sphereCastRadius, 200f, layerMask, QueryTriggerInteraction.Collide);
		Array.Sort(hits, (RaycastHit x, RaycastHit y) => GetDistanceToRay(x).CompareTo(GetDistanceToRay(y)));
		for (int i2 = 0; i2 < hits.Length; i2++)
		{
			T t2 = hits[i2].collider.GetComponentInParent<T>();
			if (t2 == null || (t2 is Actor a3 && (a3 == null || !a3.isActive)) || t2 is Hero { isKnockedOut: not false })
			{
				continue;
			}
			if (t2 is Component c2)
			{
				Actor a4 = c2.GetComponentInParent<Actor>();
				if ((a4 != null && !a4.isActive) || a4 is Hero { isKnockedOut: not false })
				{
					continue;
				}
			}
			if (validator == null || validator(t2))
			{
				return t2;
			}
		}
		return default(T);
		float GetDistanceToRay(RaycastHit hit)
		{
			return Vector3.Cross(ray.direction, hit.point - ray.origin).magnitude;
		}
	}

	public static Entity GetEntityFromScreenPoint(Vector2 screenPoint, Func<Entity, bool> validator, float sphereCastRadius = -1f)
	{
		return GetFromScreenPoint(screenPoint, LayerMasks.Entity, validator, sphereCastRadius);
	}

	public static IInteractable GetInteractableFromScreenPoint(Vector2 screenPoint, float sphereCastRadius = -1f)
	{
		return GetFromScreenPoint(screenPoint, LayerMasks.Entity | LayerMasks.Interactable, (IInteractable i) => i.CanInteract(ManagerBase<ControlManager>.instance.controllingEntity), sphereCastRadius);
	}

	public static HighlightProvider GetHighlightableFromScreenPoint(Vector2 screenPoint)
	{
		return GetFromScreenPoint<HighlightProvider>(screenPoint, LayerMasks.Entity | LayerMasks.Interactable);
	}

	public static Entity GetEntityOnCursor(float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), sphereCastRadius);
	}

	public static IInteractable GetInteractableOnCursor()
	{
		return GetInteractableFromScreenPoint(GetMousePositionWithInversionInMind());
	}

	public static HighlightProvider GetHighlightableOnCursor()
	{
		return GetHighlightableFromScreenPoint(GetMousePositionWithInversionInMind());
	}

	public static Entity GetEntityOnCursor(Entity self, IBinaryEntityValidator validator, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), self, validator, sphereCastRadius);
	}

	public static Entity GetEntityOnCursor(IEntityValidator validator, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), validator, sphereCastRadius);
	}

	public static Entity GetEntityOnCursor(Func<Entity, bool> validator, float sphereCastRadius = -1f)
	{
		return GetEntityFromScreenPoint(GetMousePositionWithInversionInMind(), validator, sphereCastRadius);
	}

	private bool ShouldInvalidateCurrentCast()
	{
		if (!(state.trigger == null) && !(state.trigger.owner != controllingEntity))
		{
			return state.configIndex != state.trigger.currentConfigIndex;
		}
		return true;
	}

	private CastConfirmType GetAbilityCastType(SkillTrigger skill)
	{
		if (skill.GetType().Name.Contains("_M_"))
		{
			return DewSave.profile.controls.movementHeroAbilityCastType;
		}
		return DewSave.profile.controls.defaultHeroAbilityCastType;
	}

	public static Vector3 GetWorldPositionOnGroundFromViewportPoint(Vector2 viewportPoint, bool forDirectionalAttacks)
	{
		Ray ray = Dew.mainCamera.ViewportPointToRay(viewportPoint);
		Vector3 offset = (forDirectionalAttacks ? new Vector3(0f, 0.75f, 0f) : Vector3.zero);
		if (ManagerBase<CameraManager>.softInstance != null && ManagerBase<CameraManager>.softInstance.focusedEntity != null && new Plane(Vector3.up, ManagerBase<CameraManager>.softInstance.focusedEntity.agentPosition + offset).Raycast(ray, out var distance))
		{
			return ray.origin + ray.direction * distance;
		}
		if (new Plane(Vector3.up, offset).Raycast(ray, out var distance2))
		{
			return ray.origin + ray.direction * distance2;
		}
		return ray.origin + ray.direction * 100f;
	}

	public static Vector3 GetWorldPositionOnGroundFromScreenPoint(Vector2 screenPoint, bool forDirectionalAttacks)
	{
		return GetWorldPositionOnGroundFromViewportPoint(new Vector2(screenPoint.x / (float)Screen.width, screenPoint.y / (float)Screen.height), forDirectionalAttacks);
	}

	public static bool AreControlsInverted()
	{
		if (ManagerBase<ControlManager>.softInstance != null && ManagerBase<ControlManager>.softInstance.controllingEntity != null)
		{
			return ManagerBase<ControlManager>.softInstance.controllingEntity.Control.isControlReversed;
		}
		return false;
	}

	public static Vector2 GetMousePositionWithInversionInMind()
	{
		Vector3 mousePos = Input.mousePosition;
		if (AreControlsInverted())
		{
			Vector3 center = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
			Vector3 delta = mousePos - center;
			mousePos = center - delta;
		}
		return mousePos;
	}

	public static Vector3 GetWorldPositionOnGroundOnCursor(bool forDirectionalAttacks)
	{
		if (Time.frameCount != _cachedFrame)
		{
			_cachedPosition = GetWorldPositionOnGroundFromScreenPoint(GetMousePositionWithInversionInMind(), forDirectionalAttacks);
		}
		return _cachedPosition;
	}

	public static float GetAimAssistSphereCastRadius()
	{
		return DewSave.profile.controls.targetAssist switch
		{
			AimAssistType.None => 0f, 
			AimAssistType.Medium => 0.75f, 
			AimAssistType.High => 1.5f, 
			AimAssistType.VeryHigh => 2.5f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static float GetAimAssistAttackTargetRange()
	{
		return DewSave.profile.controls.targetAssist switch
		{
			AimAssistType.None => 0f, 
			AimAssistType.Medium => 2.5f, 
			AimAssistType.High => 3.5f, 
			AimAssistType.VeryHigh => 5f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static float GetAimAssistAttackTargetAngleGamepad()
	{
		return DewSave.profile.controls.attackTargetAssistGamepad switch
		{
			AimAssistType.None => 0f, 
			AimAssistType.Medium => 30f, 
			AimAssistType.High => 45f, 
			AimAssistType.VeryHigh => 90f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void SetFocusedInteractable(IInteractable i, bool isAtCursor = false)
	{
		if (i == focusedInteractable && (i == null || isAtCursor == isFocusedInteractableAtCursor))
		{
			return;
		}
		focusedInteractable = i;
		isFocusedInteractableAtCursor = isAtCursor;
		try
		{
			onFocusedInteractableChanged?.Invoke(i);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private IInteractable GetNearbyInteractable()
	{
		int count = Physics.OverlapSphereNonAlloc(controllingEntity.position, 6f, _interactCheckColliders, LayerMasks.Interactable | LayerMasks.Entity);
		IInteractable best = null;
		float bestDist = float.PositiveInfinity;
		int bestPriority = int.MaxValue;
		for (int i = 0; i < count; i++)
		{
			IInteractable found = _interactCheckColliders[i].GetComponentInParent<IInteractable>();
			if (found != null && !found.IsUnityNull() && !(found is Actor { isActive: false }) && found.CanInteract(controllingEntity) && bestPriority >= found.priority)
			{
				float dist = Vector2.Distance(DewPlayer.local.hero.position.ToXY(), ((Component)found).transform.position.ToXY());
				if (!(dist > found.focusDistance) && (bestPriority != found.priority || !(dist > bestDist)))
				{
					bestDist = dist;
					bestPriority = found.priority;
					best = found;
				}
			}
		}
		return best;
	}

	private void GetCharInputOfInteractByButtonPress()
	{
		if (focusedInteractable == null || focusedInteractable.IsUnityNull() || !focusedInteractable.CanInteract(controllingEntity))
		{
			return;
		}
		bool interactAltDown = DewInput.GetButtonDown(DewSave.profile.controls.interactAlt, checkGameAreaForMouse: true);
		bool interactAltHoldRepeat = Time.unscaledTime - _lastInteractAltUnscaledTime > 0.3f && DewInput.GetButton(DewSave.profile.controls.interactAlt, checkGameAreaForMouse: true);
		if (DewInput.GetButtonDown(DewSave.profile.controls.interact, checkGameAreaForMouse: true) || interactAltDown || interactAltHoldRepeat)
		{
			if (interactAltDown || interactAltHoldRepeat)
			{
				_lastInteractAltUnscaledTime = Time.unscaledTime;
			}
			controllingEntity.Control.CmdInteract(focusedInteractable, interactAltDown || interactAltHoldRepeat, isMouse: false);
			Component comp = (Component)focusedInteractable;
			UnityEngine.Object.Instantiate(interactMarker, comp.transform.position, Quaternion.identity).followTransform = comp.transform;
			if (state.type != ControlStateType.None)
			{
				state = default(ControlState);
			}
			HighlightProvider hl = comp.GetComponentInChildren<HighlightProvider>();
			if (hl != null)
			{
				hl.ShowClick();
			}
		}
	}

	internal void UpdateInteractableFocus(bool alsoCheckNearby)
	{
		if (ManagerBase<FloatingWindowManager>.instance.currentTarget != null || NetworkedManagerBase<ConversationManager>.instance.hasOngoingLocalConversation || InGameUIManager.instance.isWorldDisplayed != WorldDisplayStatus.None)
		{
			SetFocusedInteractable(null);
			return;
		}
		switch (ManagerBase<EditSkillManager>.instance.mode)
		{
		case EditSkillManager.ModeType.UpgradeGem:
		case EditSkillManager.ModeType.UpgradeSkill:
		case EditSkillManager.ModeType.Upgrade:
		case EditSkillManager.ModeType.Sell:
		case EditSkillManager.ModeType.Cleanse:
			SetFocusedInteractable(null);
			break;
		default:
			SetFocusedInteractable(null);
			break;
		case EditSkillManager.ModeType.None:
		{
			IInteractable onCursor = GetInteractableOnCursor();
			if (onCursor != null)
			{
				SetFocusedInteractable(onCursor, isAtCursor: true);
			}
			else if (alsoCheckNearby)
			{
				SetFocusedInteractable(GetNearbyInteractable());
			}
			break;
		}
		}
	}

	public void SetCastByKeyFlag(AbilityTrigger trigger, bool value, DewInputTrigger it)
	{
		if (trigger is SkillTrigger skill && DewPlayer.local.hero.Skill.TryGetSkillLocation(skill, out var type))
		{
			if (!value)
			{
				_castByKeyInfo.Remove(type);
			}
			else
			{
				_castByKeyInfo[type] = (it, Time.unscaledTime);
			}
		}
	}

	private bool ProcessCastInfoSampling()
	{
		if (DewPlayer.local == null || !_localSampleContext.HasValue)
		{
			_localSampleContext = null;
			return false;
		}
		SampleCastInfoContext context = _localSampleContext.Value;
		CastMethodData method = context.castMethod;
		CastInfo info = GetCastInfo(method, context.targetValidator, context.currentInfo);
		if (context.angleSpeedLimit > 0.0001f)
		{
			info.angle = Mathf.MoveTowardsAngle(context.currentInfo.angle, info.angle, context.angleSpeedLimit * Time.deltaTime);
		}
		context.currentInfo = info;
		_localSampleContext = context;
		if (canSendSampleUpdate)
		{
			DewPlayer.local.DispatchSample_Update(info);
			_lastSampleUpdateTime = Time.unscaledTime;
		}
		if (DewInput.GetButtonDown(DewSave.profile.controls.attackInPlace, checkGameAreaForMouse: false) || DewInput.GetButtonDown(MouseButton.Left, checkGameArea: false))
		{
			DewPlayer.local.DispatchSample_Cast(info);
		}
		else if (context.castOnButton != SampleCastInfoContext.CastOnButtonType.None && context.castKey != null)
		{
			if (context.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButton)
			{
				if (context.trigger is SkillTrigger skill && DewPlayer.local.hero.Skill.TryGetSkillLocation(skill, out var type))
				{
					if (_castByKeyInfo.ContainsKey(type) && Time.unscaledTime - _castByKeyInfo[type].Item2 < 1f)
					{
						context.castOnButton = SampleCastInfoContext.CastOnButtonType.ByButtonRelease;
						_castByKeyInfo.Remove(type);
					}
					else
					{
						context.castOnButton = SampleCastInfoContext.CastOnButtonType.ByButtonPress;
					}
				}
				else
				{
					Debug.LogWarning($"Cast on release sampling started from unknown trigger: {context.trigger}");
					context.castOnButton = SampleCastInfoContext.CastOnButtonType.ByButtonPress;
				}
				_localSampleContext = context;
			}
			if (context.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButtonPress && context.castKey.down)
			{
				DewPlayer.local.DispatchSample_Cast(info);
			}
			if (context.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButtonRelease && !context.castKey)
			{
				DewPlayer.local.DispatchSample_Cast(info);
			}
		}
		return true;
	}

	public CastInfo GetCastInfo(CastMethodData method, AbilityTargetValidator targetValidator, CastInfo prev = default(CastInfo))
	{
		CastInfo info;
		switch (method.type)
		{
		case CastMethodType.None:
			info = new CastInfo(controllingEntity);
			break;
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			info = ((DewInput.currentMode != InputMode.KeyboardAndMouse) ? ((!aimDirection.HasValue) ? ((!(Time.unscaledTime - _lastMovementDirectionUnscaledTime < 0.05f)) ? new CastInfo(controllingEntity, prev.angle) : new CastInfo(controllingEntity, CastInfo.GetAngle(lastMovementDirection))) : new CastInfo(controllingEntity, CastInfo.GetAngle(aimDirection.Value))) : new CastInfo(controllingEntity, CastInfo.GetAngle(GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: true) - controllingEntity.transform.position)));
			break;
		case CastMethodType.Target:
		{
			Entity target = GetEntityOnCursor(controllingEntity, targetValidator);
			if (target == null)
			{
				return default(CastInfo);
			}
			info = new CastInfo(controllingEntity, target);
			break;
		}
		case CastMethodType.Point:
			info = new CastInfo(controllingEntity, GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false));
			break;
		default:
			return default(CastInfo);
		}
		return info;
	}

	private void InitializeTriggers()
	{
		it_confirmCast = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.confirmCast,
			isValidCheck = () => shouldProcessCharacterInput && state.type != ControlStateType.None,
			checkGameAreaForMouse = false,
			priority = -5
		};
		it_move = CharacterControl(() => DewSave.profile.controls.move, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_attackMoveNormal = CharacterControl(() => DewSave.profile.controls.attackMoveNormal, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_attackMoveImmediately = CharacterControl(() => DewSave.profile.controls.attackMoveImmediately, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_attackMoveOnRelease = CharacterControl(() => DewSave.profile.controls.attackMoveOnRelease, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_attackInPlace = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.attackInPlace,
			isValidCheck = () => shouldProcessCharacterInput && ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None,
			checkGameAreaForMouse = true,
			priority = -1
		};
		it_scoreboard = CharacterControl(() => DewSave.profile.controls.scoreboard, checkGameAreaForMouse: false, 1, allowKnockOut: true, invalidIfHasFocus: false, null);
		it_worldMap = CharacterControl(() => DewSave.profile.controls.worldMap, checkGameAreaForMouse: false, 1, allowKnockOut: true, invalidIfHasFocus: false, null);
		it_stop = CharacterControl(() => DewSave.profile.controls.stop, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_moveUp = DirectionalMovementControl(() => DewSave.profile.controls.moveUp);
		it_moveLeft = DirectionalMovementControl(() => DewSave.profile.controls.moveLeft);
		it_moveDown = DirectionalMovementControl(() => DewSave.profile.controls.moveDown);
		it_moveRight = DirectionalMovementControl(() => DewSave.profile.controls.moveRight);
		it_skillQ = CharacterControl(() => DewSave.profile.controls.skillQ, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillW = CharacterControl(() => DewSave.profile.controls.skillW, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillE = CharacterControl(() => DewSave.profile.controls.skillE, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillR = CharacterControl(() => DewSave.profile.controls.skillR, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillMovement = CharacterControl(delegate
		{
			DewBinding dewBinding = DewSave.profile.controls.skillMovement;
			if (DewSave.profile.controls.leftJoystickClickAction == JoystickClickAction.Dodge)
			{
				dewBinding = dewBinding.CloneWith(GamepadButtonEx.LeftStick);
			}
			if (DewSave.profile.controls.rightJoystickClickAction == JoystickClickAction.Dodge)
			{
				dewBinding = dewBinding.CloneWith(GamepadButtonEx.RightStick);
			}
			return dewBinding;
		}, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillQNormal = CharacterControl(() => DewSave.profile.controls.skillQNormal, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillWNormal = CharacterControl(() => DewSave.profile.controls.skillWNormal, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillENormal = CharacterControl(() => DewSave.profile.controls.skillENormal, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillRNormal = CharacterControl(() => DewSave.profile.controls.skillRNormal, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillMovementNormal = CharacterControl(() => DewSave.profile.controls.skillMovementNormal, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillQImmediately = CharacterControl(() => DewSave.profile.controls.skillQImmediately, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillWImmediately = CharacterControl(() => DewSave.profile.controls.skillWImmediately, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillEImmediately = CharacterControl(() => DewSave.profile.controls.skillEImmediately, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillRImmediately = CharacterControl(() => DewSave.profile.controls.skillRImmediately, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillMovementImmediately = CharacterControl(() => DewSave.profile.controls.skillMovementImmediately, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillQOnRelease = CharacterControl(() => DewSave.profile.controls.skillQOnRelease, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillWOnRelease = CharacterControl(() => DewSave.profile.controls.skillWOnRelease, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillEOnRelease = CharacterControl(() => DewSave.profile.controls.skillEOnRelease, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillROnRelease = CharacterControl(() => DewSave.profile.controls.skillROnRelease, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillMovementOnRelease = CharacterControl(() => DewSave.profile.controls.skillMovementOnRelease, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillQSelf = CharacterControl(() => DewSave.profile.controls.skillQSelf, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillWSelf = CharacterControl(() => DewSave.profile.controls.skillWSelf, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillESelf = CharacterControl(() => DewSave.profile.controls.skillESelf, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillRSelf = CharacterControl(() => DewSave.profile.controls.skillRSelf, checkGameAreaForMouse: true, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_skillQEdit = EditSkillControl(() => DewSave.profile.controls.skillQEdit);
		it_skillWEdit = EditSkillControl(() => DewSave.profile.controls.skillWEdit);
		it_skillEEdit = EditSkillControl(() => DewSave.profile.controls.skillEEdit);
		it_skillREdit = EditSkillControl(() => DewSave.profile.controls.skillREdit);
		it_interact = CharacterControl(() => DewSave.profile.controls.interact, checkGameAreaForMouse: true, -3, allowKnockOut: false, invalidIfHasFocus: false, () => focusedInteractable != null);
		it_interactAlt = CharacterControl(() => DewSave.profile.controls.interactAlt, checkGameAreaForMouse: true, -3, allowKnockOut: false, invalidIfHasFocus: true, () => focusedInteractable != null);
		it_editSkillHold = CharacterControl(() => DewSave.profile.controls.editSkillHold, checkGameAreaForMouse: false, 0, allowKnockOut: false, invalidIfHasFocus: false, null);
		it_editSkillToggle = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.editSkillToggle,
			isValidCheck = () => ManagerBase<GlobalUIManager>.instance.focused == null && shouldProcessCharacterInput,
			checkGameAreaForMouse = false,
			priority = -1
		};
		it_showDetails = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.showDetails,
			isValidCheck = () => shouldProcessCharacterInput,
			canConsume = false,
			checkGameAreaForMouse = false,
			priority = 0
		};
		it_zoomOutCamera = CharacterControl(() => DewSave.profile.controls.zoomOut, checkGameAreaForMouse: true, 1, allowKnockOut: true, invalidIfHasFocus: false, null);
		it_zoomInCamera = CharacterControl(() => DewSave.profile.controls.zoomIn, checkGameAreaForMouse: true, 1, allowKnockOut: true, invalidIfHasFocus: false, null);
		it_chat = CharacterControl(() => DewSave.profile.controls.chat, checkGameAreaForMouse: true, 0, allowKnockOut: true, invalidIfHasFocus: false, null);
		it_ping = CharacterControl(() => DewSave.profile.controls.ping, checkGameAreaForMouse: false, 0, allowKnockOut: true, invalidIfHasFocus: false, null);
		it_travelVote = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.travelVote,
			isValidCheck = () => shouldProcessCharacterInput && NetworkedManagerBase<ZoneManager>.instance.isVoting,
			checkGameAreaForMouse = false,
			priority = -10
		};
		it_travelVoteCancel = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.travelVoteCancel,
			isValidCheck = () => shouldProcessCharacterInput && NetworkedManagerBase<ZoneManager>.instance.isVoting,
			checkGameAreaForMouse = false,
			priority = -10
		};
		it_spectatorNextTarget = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.spectatorNextTarget,
			isValidCheck = () => ManagerBase<CameraManager>.instance.isSpectating,
			checkGameAreaForMouse = true,
			priority = -5
		};
		DewInputTrigger CharacterControl(Func<DewBinding> binding, bool checkGameAreaForMouse, int priority, bool allowKnockOut, bool invalidIfHasFocus, Func<bool> customValidCheck)
		{
			return new DewInputTrigger
			{
				owner = this,
				binding = binding,
				isValidCheck = () => (allowKnockOut ? shouldProcessCharacterInputAllowKnockedOut : shouldProcessCharacterInput) && (!invalidIfHasFocus || ManagerBase<GlobalUIManager>.instance.focused == null) && (customValidCheck == null || customValidCheck()),
				checkGameAreaForMouse = checkGameAreaForMouse,
				priority = priority
			};
		}
		DewInputTrigger DirectionalMovementControl(Func<DewBinding> binding)
		{
			return new DewInputTrigger
			{
				owner = this,
				binding = binding,
				isValidCheck = () => DewSave.profile.controls.enableDirMoveKeys && shouldProcessCharacterInput
			};
		}
		DewInputTrigger EditSkillControl(Func<DewBinding> binding)
		{
			return new DewInputTrigger
			{
				owner = this,
				binding = binding,
				isValidCheck = delegate
				{
					if (shouldProcessCharacterInput)
					{
						EditSkillManager.ModeType mode = ManagerBase<EditSkillManager>.instance.mode;
						return mode == EditSkillManager.ModeType.EquipGem || mode == EditSkillManager.ModeType.EquipSkill;
					}
					return false;
				}
			};
		}
	}
}
