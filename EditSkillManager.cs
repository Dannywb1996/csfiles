using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditSkillManager : ManagerBase<EditSkillManager>
{
	public enum ModeType
	{
		None = 0,
		Regular = 1,
		EquipGem = 2,
		EquipSkill = 3,
		UpgradeGem = 4,
		UpgradeSkill = 5,
		Upgrade = 8,
		Sell = 9,
		Cleanse = 16
	}

	public int backButtonPriority = 5;

	public RectTransform dropToGroundBlocker;

	public Action<ModeType> onModeChanged;

	public Action<UnityEngine.Object> onDraggingObjectChanged;

	public Action<HeroSkillLocation, SkillTrigger> OnSkillSlotClientState;

	public Action<GemLocation, Gem> OnGemSlotClientState;

	public Action<SkillTrigger, bool> OnSkillVisibleClientState;

	public Action<Gem, bool> OnGemVisibleClientState;

	private DewInputTrigger it_skillQ;

	private DewInputTrigger it_skillW;

	private DewInputTrigger it_skillE;

	private DewInputTrigger it_skillR;

	private Actor _currentProvider;

	private List<RaycastResult> _results = new List<RaycastResult>(64);

	public Action onSelectedSlotChanged;

	private HeroSkillLocation? _selectedSkillSlot;

	private GemLocation? _selectedGemSlot;

	public GameObject skillButtons;

	public ModeType mode { get; private set; }

	public float lastModeSetUnscaledTime { get; private set; }

	public bool isDragging => (object)draggingObject != null;

	public UnityEngine.Object draggingObject { get; private set; }

	public bool shouldEndAfterAction { get; private set; }

	public Actor currentProvider
	{
		get
		{
			return _currentProvider;
		}
		private set
		{
			if (!(value == _currentProvider))
			{
				_currentProvider = value;
				lastCurrentProviderSetUnscaledTime = Time.unscaledTime;
			}
		}
	}

	public float lastCurrentProviderSetUnscaledTime { get; private set; }

	public int currentModeSetFrameCount { get; private set; }

	public HeroSkillLocation? selectedSkillSlot
	{
		get
		{
			return _selectedSkillSlot;
		}
		set
		{
			if (_selectedSkillSlot != value)
			{
				_selectedSkillSlot = value;
				onSelectedSlotChanged?.Invoke();
			}
		}
	}

	public GemLocation? selectedGemSlot
	{
		get
		{
			return _selectedGemSlot;
		}
		set
		{
			if (!(_selectedGemSlot == value))
			{
				_selectedGemSlot = value;
				onSelectedSlotChanged?.Invoke();
			}
		}
	}

	public bool isSelectingGround { get; private set; }

	private void Start()
	{
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, backButtonPriority, delegate
		{
			if (mode == ModeType.None)
			{
				return false;
			}
			if (ManagerBase<ControlManager>.instance.isEditSkillDisabled)
			{
				return false;
			}
			EndEdit();
			return true;
		});
		it_skillQ = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.skillQ,
			priority = -1,
			isValidCheck = () => DewInput.currentMode == InputMode.Gamepad && (mode != ModeType.None || ManagerBase<GlobalUIManager>.instance.focused != null)
		};
		it_skillW = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.skillW,
			priority = -1,
			isValidCheck = () => DewInput.currentMode == InputMode.Gamepad && (mode != ModeType.None || ManagerBase<GlobalUIManager>.instance.focused != null)
		};
		it_skillE = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.skillE,
			priority = -1,
			isValidCheck = () => DewInput.currentMode == InputMode.Gamepad && (mode != ModeType.None || ManagerBase<GlobalUIManager>.instance.focused != null)
		};
		it_skillR = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.skillR,
			priority = -1,
			isValidCheck = () => DewInput.currentMode == InputMode.Gamepad && (mode != ModeType.None || ManagerBase<GlobalUIManager>.instance.focused != null)
		};
		InitGamepadInputs();
		InGameUIManager inGameUIManager = InGameUIManager.instance;
		inGameUIManager.onWorldDisplayedChanged = (Action<WorldDisplayStatus>)Delegate.Combine(inGameUIManager.onWorldDisplayedChanged, (Action<WorldDisplayStatus>)delegate
		{
			EndEdit();
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage dmg)
		{
			if (mode != ModeType.None && (mode != ModeType.Regular || DewInput.currentMode == InputMode.Gamepad) && DewPlayer.local != null && dmg.victim == DewPlayer.local.hero && !dmg.damage.HasAttr(DamageAttribute.DamageOverTime))
			{
				EndEdit();
			}
		};
	}

	public void StartDrag(UnityEngine.Object obj)
	{
		isSelectingGround = false;
		if (obj == null)
		{
			return;
		}
		if (obj is Gem)
		{
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			if (selectedGemSlot.HasValue)
			{
				selectedSkillSlot = selectedGemSlot.Value.skill;
				selectedGemSlot = null;
			}
		}
		else if (obj is SkillTrigger skill)
		{
			HeroSkill comp = DewPlayer.local.hero.Skill;
			if (comp.TryGetSkillLocation(skill, out var type) && !comp.CanReplaceSkill(type))
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, (type == HeroSkillLocation.Identity) ? "InGame_Message_CantReplaceCharacterSkill" : "InGame_Message_CantReplaceLockedSkill");
				return;
			}
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
		}
		draggingObject = obj;
		onDraggingObjectChanged?.Invoke(obj);
	}

	public void EndDrag(bool isCancel)
	{
		if (draggingObject == null)
		{
			return;
		}
		if (isCancel)
		{
			if (draggingObject is Gem)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			}
			else if (draggingObject is SkillTrigger)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
			}
			draggingObject = null;
			onDraggingObjectChanged?.Invoke(null);
			return;
		}
		Hero hero = DewPlayer.local.hero;
		if (hero == null)
		{
			return;
		}
		if (isSelectingGround)
		{
			DropDraggingObjectGamepad();
			return;
		}
		if (DewInput.currentMode == InputMode.KeyboardAndMouse)
		{
			Dew.RaycastAllUIElementsBelowCursor(_results);
			foreach (RaycastResult r in _results)
			{
				IEditSkillDropTargetSkillButton skillBtn = r.gameObject.GetComponentInParent<IEditSkillDropTargetSkillButton>();
				if (skillBtn != null)
				{
					HeroSkillLocation skillType = skillBtn.skillType;
					if (draggingObject is Gem gem0)
					{
						HandleGemToSkill(gem0, skillType);
					}
					else if (draggingObject is SkillTrigger skill0)
					{
						HandleSkillToSkill(skill0, skillType);
					}
					draggingObject = null;
					onDraggingObjectChanged?.Invoke(null);
					return;
				}
				IEditSkillDropTargetGemSlot gemSlot = r.gameObject.GetComponentInParent<IEditSkillDropTargetGemSlot>();
				if (gemSlot != null)
				{
					GemLocation slotLoc = gemSlot.location;
					if (draggingObject is Gem gem1)
					{
						HandleGemToGem(gem1, slotLoc);
					}
					else if (draggingObject is SkillTrigger)
					{
						HandleSkillToGem();
					}
					draggingObject = null;
					onDraggingObjectChanged?.Invoke(null);
					return;
				}
			}
			if (currentModeSetFrameCount == Time.frameCount)
			{
				return;
			}
			if ((ManagerBase<ControlManager>.instance.dropConstraint != null && !ManagerBase<ControlManager>.instance.dropConstraint(draggingObject)) || (DewInput.currentMode == InputMode.KeyboardAndMouse && dropToGroundBlocker.GetScreenSpaceRect().Contains(Input.mousePosition)))
			{
				draggingObject = null;
				onDraggingObjectChanged?.Invoke(null);
				return;
			}
			Vector3 groundPos = ControlManager.GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false);
			if (draggingObject is Gem gem2)
			{
				if (hero.Skill.TryGetGemLocation(gem2, out var loc))
				{
					hero.Skill.CmdUnequipGem(loc, groundPos);
				}
				else
				{
					hero.Skill.CmdMoveGem(gem2, groundPos);
				}
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			}
			else if (draggingObject is SkillTrigger skill1)
			{
				if (hero.Skill.TryGetSkillLocation(skill1, out var type))
				{
					hero.Skill.CmdUnequipSkill(type, groundPos);
				}
				else
				{
					hero.Skill.CmdMoveSkill(skill1, groundPos);
				}
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
			}
			draggingObject = null;
			onDraggingObjectChanged?.Invoke(null);
			return;
		}
		if (selectedGemSlot.HasValue)
		{
			if (draggingObject is SkillTrigger)
			{
				HandleSkillToGem();
			}
			else if (draggingObject is Gem g)
			{
				HandleGemToGem(g, selectedGemSlot.Value);
			}
		}
		if (selectedSkillSlot.HasValue)
		{
			if (draggingObject is SkillTrigger s)
			{
				HandleSkillToSkill(s, selectedSkillSlot.Value);
			}
			else if (draggingObject is Gem g2 && !HandleGemToSkill(g2, selectedSkillSlot.Value))
			{
				return;
			}
		}
		draggingObject = null;
		onDraggingObjectChanged?.Invoke(null);
		void HandleGemToGem(Gem gem3, GemLocation gemLocation)
		{
			if (hero.Skill.TryGetGemLocation(gem3, out var draggingLoc) && gemLocation == draggingLoc)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
			}
			else
			{
				hero.Skill.CmdSwapSlotGem(gemLocation, draggingLoc);
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
			}
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
		}
		bool HandleGemToSkill(Gem gem3, HeroSkillLocation heroSkillLocation)
		{
			GemLocation draggingLoc;
			bool isDraggingEquipped = hero.Skill.TryGetGemLocation(gem3, out draggingLoc);
			int emptySlot = hero.Skill.GetEmptyGemSlot(heroSkillLocation);
			if (hero.Skill.GetMaxGemCount(heroSkillLocation) <= 0)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_NoGemSlotOnSkill");
				return false;
			}
			if (isDraggingEquipped && draggingLoc.skill == heroSkillLocation)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
				if (selectedSkillSlot.HasValue)
				{
					selectedSkillSlot = null;
					selectedGemSlot = draggingLoc;
				}
			}
			else
			{
				if (emptySlot == -1)
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_GemSlotFull");
					return false;
				}
				if (isDraggingEquipped)
				{
					GemLocation newPos = new GemLocation(heroSkillLocation, emptySlot);
					hero.Skill.CmdSwapSlotGem(draggingLoc, newPos);
					if (selectedSkillSlot.HasValue)
					{
						selectedSkillSlot = null;
						selectedGemSlot = newPos;
					}
				}
				else
				{
					GemLocation newPos2 = new GemLocation(heroSkillLocation, emptySlot);
					hero.Skill.CmdEquipGem(newPos2, gem3);
					if (selectedSkillSlot.HasValue)
					{
						selectedSkillSlot = null;
						selectedGemSlot = newPos2;
					}
				}
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
			}
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			return true;
		}
		static void HandleSkillToGem()
		{
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
		}
		void HandleSkillToSkill(SkillTrigger dragging, HeroSkillLocation targetLocation)
		{
			HeroSkillLocation draggingLoc;
			bool isDraggingEquipped = hero.Skill.TryGetSkillLocation(dragging, out draggingLoc);
			if (!hero.Skill.CanReplaceSkill(targetLocation))
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, (targetLocation == HeroSkillLocation.Identity) ? "InGame_Message_CantReplaceCharacterSkill" : "InGame_Message_CantReplaceLockedSkill");
			}
			else if (isDraggingEquipped && targetLocation == draggingLoc)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillEquip");
			}
			else
			{
				hero.Skill.CmdSwapSlotSkill(targetLocation, draggingLoc);
			}
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
		}
	}

	private void SetMode(ModeType newMode)
	{
		lastModeSetUnscaledTime = Time.unscaledTime;
		if (mode == ModeType.None && newMode != ModeType.None)
		{
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_EditStart");
		}
		else if (mode != ModeType.None && newMode == ModeType.None)
		{
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_EditEnd");
		}
		if (ManagerBase<ControlManager>.instance.controllingEntity is Hero h && !h.Skill.holdingObject.IsHoldableObjectNullOrInactive() && newMode != ModeType.EquipGem && newMode != ModeType.EquipSkill)
		{
			h.Skill.CmdStopHoldInHand();
		}
		if (newMode != ModeType.None && ManagerBase<ControlManager>.instance.state.type != ControlManager.ControlStateType.None)
		{
			ManagerBase<ControlManager>.instance.state = default(ControlManager.ControlState);
		}
		mode = newMode;
		currentModeSetFrameCount = Time.frameCount;
		onModeChanged?.Invoke(newMode);
		if (DewInput.currentMode == InputMode.Gamepad && ManagerBase<FloatingWindowManager>.instance.currentTarget == null)
		{
			SelectAnyRelevantSlot();
			return;
		}
		selectedSkillSlot = null;
		selectedGemSlot = null;
	}

	public void SetClientState_SetSkillSlot(HeroSkillLocation type, SkillTrigger skill)
	{
		OnSkillSlotClientState?.Invoke(type, skill);
		if (skill != null)
		{
			OnSkillVisibleClientState?.Invoke(skill, arg2: false);
		}
	}

	public void SetClientState_SetGemSlot(GemLocation loc, Gem gem)
	{
		OnGemSlotClientState?.Invoke(loc, gem);
		if (gem != null)
		{
			OnGemVisibleClientState?.Invoke(gem, arg2: false);
		}
	}

	public void SetClientState_MergeGemVictim(Gem gem)
	{
		OnGemVisibleClientState?.Invoke(gem, arg2: false);
	}

	public void StartRegularEdit(bool endAfterAction)
	{
		shouldEndAfterAction = endAfterAction;
		SetMode(ModeType.Regular);
	}

	public void StartEquipGem(Gem gem)
	{
		shouldEndAfterAction = true;
		SetMode(ModeType.EquipGem);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
	}

	public void StartEquipSkill(SkillTrigger skill)
	{
		shouldEndAfterAction = true;
		SetMode(ModeType.EquipSkill);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
	}

	public void StartUpgradeSkill(Actor provider, bool once)
	{
		shouldEndAfterAction = once;
		currentProvider = provider;
		SetMode(ModeType.UpgradeSkill);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
	}

	public void StartUpgradeGem(Actor provider, bool once)
	{
		shouldEndAfterAction = once;
		currentProvider = provider;
		SetMode(ModeType.UpgradeGem);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
	}

	public void StartUpgrade(Actor provider, bool once)
	{
		shouldEndAfterAction = once;
		currentProvider = provider;
		SetMode(ModeType.Upgrade);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
	}

	public void StartSell(PropEnt_Merchant_Base merchant)
	{
		shouldEndAfterAction = false;
		currentProvider = merchant;
		SetMode(ModeType.Sell);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
	}

	public void StartCleanse(Shrine_AltarOfCleansing altar)
	{
		shouldEndAfterAction = false;
		currentProvider = altar;
		SetMode(ModeType.Cleanse);
		ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_CleanseStart");
	}

	public void EndEdit()
	{
		ModeType modeType = mode;
		if (modeType == ModeType.EquipGem || modeType == ModeType.EquipSkill)
		{
			DewPlayer.local.hero.Skill.CmdStopHoldInHand();
		}
		currentProvider = null;
		if (draggingObject != null)
		{
			EndDrag(isCancel: true);
		}
		SetMode(ModeType.None);
	}

	public void NotifyEndOfAction()
	{
		switch (mode)
		{
		case ModeType.Regular:
			if (shouldEndAfterAction && !DewInput.GetButton(DewSave.profile.controls.editSkillHold, checkGameAreaForMouse: false))
			{
				StartCoroutine(DelayedEndEditRoutine());
			}
			else
			{
				StartRegularEdit(endAfterAction: false);
			}
			break;
		case ModeType.EquipGem:
		case ModeType.EquipSkill:
			EndEdit();
			break;
		case ModeType.UpgradeGem:
		case ModeType.UpgradeSkill:
		case ModeType.Upgrade:
		case ModeType.Sell:
		case ModeType.Cleanse:
			if (shouldEndAfterAction)
			{
				EndEdit();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ModeType.None:
			break;
		}
		IEnumerator DelayedEndEditRoutine()
		{
			yield return new WaitForSecondsRealtime(0.01f);
			EndEdit();
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (isDragging && draggingObject == null)
		{
			EndDrag(isCancel: true);
		}
		ModeType modeType = mode;
		if (modeType == ModeType.UpgradeGem || modeType == ModeType.UpgradeSkill || modeType == ModeType.Upgrade || modeType == ModeType.Sell || modeType == ModeType.Cleanse)
		{
			if (currentProvider == null || !currentProvider.isActive)
			{
				EndEdit();
			}
			else if (Vector3.Distance(currentProvider.position, DewPlayer.local.hero.position) > ManagerBase<FloatingWindowManager>.instance.maxDistance)
			{
				EndEdit();
			}
		}
		if (mode != ModeType.None && ManagerBase<CameraManager>.instance.isSpectating)
		{
			EndEdit();
		}
		modeType = mode;
		if (modeType == ModeType.EquipGem || modeType == ModeType.EquipSkill)
		{
			IItem obj = DewPlayer.local.hero.Skill.holdingObject;
			if (obj.IsHoldableObjectNullOrInactive() || (obj.owner != null && obj.owner != DewPlayer.local.hero) || obj.handOwner != DewPlayer.local.hero)
			{
				EndEdit();
			}
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!InGameUIManager.instance.IsState("Playing") || ManagerBase<CameraManager>.instance.isSpectating || !ManagerBase<ControlManager>.instance.isCharacterControlEnabled)
		{
			if (mode != ModeType.None && !ManagerBase<ControlManager>.instance.isEditSkillDisabled)
			{
				EndEdit();
			}
			return;
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			ModeType modeType = mode;
			if ((modeType == ModeType.EquipGem || modeType == ModeType.EquipSkill) && ManagerBase<GlobalUIManager>.instance.focused == null)
			{
				SetMode(mode);
			}
		}
		if (ManagerBase<ControlManager>.instance.isEditSkillDisabled)
		{
			return;
		}
		if (mode != ModeType.None && DewInput.GetButtonDown(DewSave.profile.controls.interact, checkGameAreaForMouse: true))
		{
			EndEdit();
			return;
		}
		if (ManagerBase<ControlManager>.instance.it_editSkillToggle.down)
		{
			if (mode == ModeType.None)
			{
				StartRegularEdit(endAfterAction: false);
			}
			else
			{
				EndEdit();
			}
		}
		if (DewInput.GetButtonDown(DewSave.profile.controls.editSkillHold, checkGameAreaForMouse: false) && mode == ModeType.None && ManagerBase<ControlManager>.instance.shouldProcessCharacterInput)
		{
			StartRegularEdit(endAfterAction: false);
		}
		if (DewInput.GetButtonUp(DewSave.profile.controls.editSkillHold, checkGameAreaForMouse: false) && mode != ModeType.None && mode != ModeType.Sell)
		{
			EndEdit();
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			DoGamepadInputs();
		}
	}

	public bool IsSlotSelectable(HeroSkillLocation skillLoc)
	{
		if (mode == ModeType.None)
		{
			return false;
		}
		if (DewPlayer.local == null)
		{
			return false;
		}
		Hero hero = DewPlayer.local.hero;
		if (hero == null)
		{
			return false;
		}
		SkillTrigger skill = hero.Skill.GetSkill(skillLoc);
		if (draggingObject is SkillTrigger)
		{
			return DewPlayer.local.hero.Skill.CanReplaceSkill(skillLoc);
		}
		if (mode == ModeType.EquipGem || draggingObject is Gem)
		{
			return hero.Skill.GetMaxGemCount(skillLoc) > 0;
		}
		if (mode == ModeType.EquipSkill)
		{
			if (!(skill == null))
			{
				return DewPlayer.local.hero.Skill.CanReplaceSkill(skillLoc);
			}
			return true;
		}
		return true;
	}

	public bool IsSlotSelectable(GemLocation gemLoc)
	{
		if (draggingObject is SkillTrigger)
		{
			return false;
		}
		if (mode == ModeType.None)
		{
			return false;
		}
		if (DewPlayer.local == null)
		{
			return false;
		}
		Hero hero = DewPlayer.local.hero;
		if (hero == null)
		{
			return false;
		}
		Gem gem = hero.Skill.GetGem(gemLoc);
		if (mode == ModeType.EquipGem || draggingObject is Gem)
		{
			return true;
		}
		if (mode == ModeType.Cleanse)
		{
			if (gem != null)
			{
				return DewPlayer.local.gold >= NetworkedManagerBase<GameManager>.instance.GetCleanseGoldCost(gem);
			}
			return false;
		}
		ModeType modeType = mode;
		if (modeType == ModeType.Upgrade || modeType == ModeType.UpgradeGem)
		{
			if (gem != null && ManagerBase<EditSkillManager>.instance.currentProvider is IUpgradeGemProvider prov)
			{
				return DewPlayer.local.dreamDust >= prov.GetDreamDustCost(gem);
			}
			return false;
		}
		if (mode == ModeType.Sell)
		{
			return gem != null;
		}
		if (mode == ModeType.Regular)
		{
			return gem != null;
		}
		return false;
	}

	public bool IsSlotHighlighted(HeroSkillLocation skillLoc)
	{
		if (DewPlayer.local == null || DewPlayer.local.hero == null)
		{
			return false;
		}
		SkillTrigger skill = DewPlayer.local.hero.Skill.GetSkill(skillLoc);
		if (draggingObject is SkillTrigger)
		{
			return DewPlayer.local.hero.Skill.CanReplaceSkill(skillLoc);
		}
		if (draggingObject is Gem g)
		{
			if (DewPlayer.local.hero.Skill.GetEmptyGemSlot(skillLoc) < 0)
			{
				if (DewPlayer.local.hero.Skill.TryGetGemLocation(g, out var loc))
				{
					return loc.skill == skillLoc;
				}
				return false;
			}
			return true;
		}
		if (mode == ModeType.EquipGem)
		{
			if (!ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue || ManagerBase<ControlManager>.instance.gemLocationConstraint == skillLoc)
			{
				return DewPlayer.local.hero.Skill.GetEmptyGemSlot(skillLoc) >= 0;
			}
			return false;
		}
		if (mode == ModeType.Regular)
		{
			return false;
		}
		if (mode == ModeType.Cleanse)
		{
			if (skill != null)
			{
				return DewPlayer.local.gold >= NetworkedManagerBase<GameManager>.instance.GetCleanseGoldCost(skill);
			}
			return false;
		}
		ModeType modeType = mode;
		if (modeType == ModeType.Upgrade || modeType == ModeType.UpgradeSkill)
		{
			if (skill != null && ManagerBase<EditSkillManager>.instance.currentProvider is IUpgradeSkillProvider prov)
			{
				return DewPlayer.local.dreamDust >= prov.GetDreamDustCost(skill);
			}
			return false;
		}
		if (mode == ModeType.Sell)
		{
			if (skill != null)
			{
				return DewPlayer.local.hero.Skill.CanReplaceSkill(skillLoc);
			}
			return false;
		}
		if (mode == ModeType.UpgradeGem)
		{
			return false;
		}
		if (!IsSlotSelectable(skillLoc))
		{
			return false;
		}
		return true;
	}

	public bool IsSlotHighlighted(GemLocation gemLoc)
	{
		if (!IsSlotSelectable(gemLoc))
		{
			return false;
		}
		if (mode == ModeType.EquipGem || draggingObject is Gem)
		{
			if (ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue)
			{
				return ManagerBase<ControlManager>.instance.gemLocationConstraint == gemLoc.skill;
			}
			return true;
		}
		return true;
	}

	public void DoClickOnSkillButton(HeroSkillLocation skillType)
	{
		SkillTrigger skill = DewPlayer.local.hero.Skill.GetSkill(skillType);
		if (mode == ModeType.None || draggingObject != null)
		{
			return;
		}
		Hero hero = DewPlayer.local.hero;
		if (hero.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		bool canSwapOutSkill = hero.Skill.CanReplaceSkill(skillType);
		if (mode == ModeType.Regular)
		{
			if (skill != null && (ManagerBase<ControlManager>.instance.dropConstraint == null || ManagerBase<ControlManager>.instance.dropConstraint(skill)))
			{
				if (canSwapOutSkill)
				{
					DewPlayer.local.hero.Skill.CmdUnequipSkill(skillType, DewPlayer.local.hero.position + UnityEngine.Random.insideUnitSphere.Flattened() * 3f);
					ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillUnequip");
				}
				else
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, (skillType == HeroSkillLocation.Identity) ? "InGame_Message_CantReplaceCharacterSkill" : "InGame_Message_CantReplaceLockedSkill");
				}
			}
		}
		else if (mode == ModeType.EquipGem)
		{
			if (ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue && skillType != ManagerBase<ControlManager>.instance.gemLocationConstraint.Value)
			{
				return;
			}
			if (hero.Skill.GetMaxGemCount(skillType) <= 0)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_NoGemSlotOnSkill");
				return;
			}
			int emptySlot = hero.Skill.GetEmptyGemSlot(skillType);
			if (emptySlot == -1)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_GemSlotFull");
				return;
			}
			hero.Skill.CmdEquipGem(new GemLocation(skillType, emptySlot), (Gem)hero.Skill.holdingObject);
			NotifyEndOfAction();
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
		}
		else if (mode == ModeType.EquipSkill)
		{
			if (!hero.Skill.CanReplaceSkill(skillType))
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, (skillType == HeroSkillLocation.Identity) ? "InGame_Message_CantReplaceCharacterSkill" : "InGame_Message_CantReplaceLockedSkill");
				return;
			}
			if (hero.Skill.GetSkill(skillType) != null)
			{
				hero.Skill.CmdUnequipSkill(skillType, hero.position + UnityEngine.Random.insideUnitSphere.Flattened() * 3f);
			}
			hero.Skill.CmdEquipSkill(skillType, (SkillTrigger)DewPlayer.local.hero.Skill.holdingObject);
			NotifyEndOfAction();
			ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillEquip");
		}
		else if (mode == ModeType.UpgradeSkill || mode == ModeType.Upgrade)
		{
			SkillTrigger s = skill;
			if (s == null)
			{
				return;
			}
			if (!s.isLevelUpEnabled)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CantUpgradeThisSkill");
				return;
			}
			if (currentProvider is IUpgradeSkillProvider prov)
			{
				int cost = prov.GetDreamDustCost(s);
				if (DewPlayer.local.dreamDust < cost)
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_NotEnoughDreamDust");
					return;
				}
				if (!prov.RequestSkillUpgrade(s))
				{
					return;
				}
			}
			NotifyEndOfAction();
		}
		else if (mode == ModeType.Sell)
		{
			if (!DewPlayer.local.hero.Skill.CanReplaceSkill(skillType))
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CantSellCharacterSkill");
			}
			else if (!(skill == null))
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Shop_Sell");
				((PropEnt_Merchant_Base)currentProvider).CmdSell(skill);
			}
		}
		else
		{
			if (mode != ModeType.Cleanse || !(skill != null))
			{
				return;
			}
			if (skill.level <= NetworkedManagerBase<GameManager>.instance.GetCleanseSkillMinLevel())
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_SkillLevelTooLow");
				return;
			}
			int cost2 = NetworkedManagerBase<GameManager>.instance.GetCleanseGoldCost(skill);
			if (DewPlayer.local.gold < cost2)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_NotEnoughGold");
			}
			else
			{
				((Shrine_AltarOfCleansing)currentProvider).CmdCleanse(skill);
			}
		}
	}

	public void DoClickOnGemSlot(GemLocation location)
	{
		if (draggingObject != null || DewPlayer.local == null)
		{
			return;
		}
		Hero hero = DewPlayer.local.hero;
		if (hero.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		Gem gem = hero.Skill.GetGem(location);
		Gem prevGem = gem;
		switch (mode)
		{
		case ModeType.Regular:
			if (!ManagerBase<ControlManager>.instance.isEditSkillDisabled && prevGem != null && (ManagerBase<ControlManager>.instance.dropConstraint == null || ManagerBase<ControlManager>.instance.dropConstraint(prevGem)))
			{
				DewPlayer.local.hero.Skill.CmdUnequipGem(location, DewPlayer.local.hero.position + UnityEngine.Random.insideUnitSphere.Flattened() * 3f);
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemUnequip");
			}
			break;
		case ModeType.EquipGem:
			if (!ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue || (location.skill == ManagerBase<ControlManager>.instance.gemLocationConstraint.Value && !(prevGem != null)))
			{
				if (prevGem != null)
				{
					DewPlayer.local.hero.Skill.CmdUnequipGem(location, DewPlayer.local.hero.position + UnityEngine.Random.insideUnitSphere.Flattened() * 3f);
				}
				hero.Skill.CmdEquipGem(location, (Gem)hero.Skill.holdingObject);
				NotifyEndOfAction();
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemEquip");
			}
			break;
		case ModeType.UpgradeGem:
		case ModeType.Upgrade:
		{
			Gem g = gem;
			if (g == null)
			{
				break;
			}
			if (currentProvider is IUpgradeGemProvider prov)
			{
				int cost2 = prov.GetDreamDustCost(g);
				if (DewPlayer.local.dreamDust < cost2)
				{
					InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_NotEnoughDreamDust");
					break;
				}
				if (!prov.RequestGemUpgrade(g))
				{
					break;
				}
			}
			NotifyEndOfAction();
			break;
		}
		case ModeType.Sell:
			if (gem != null)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_Shop_Sell");
				((PropEnt_Merchant_Base)currentProvider).CmdSell(gem);
			}
			break;
		case ModeType.Cleanse:
		{
			if (!(gem != null))
			{
				break;
			}
			if (gem.quality <= NetworkedManagerBase<GameManager>.instance.GetCleanseGemMinQuality())
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_GemQualityTooLow");
				break;
			}
			int cost = NetworkedManagerBase<GameManager>.instance.GetCleanseGoldCost(gem);
			if (DewPlayer.local.gold < cost)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_NotEnoughGold");
			}
			else
			{
				((Shrine_AltarOfCleansing)currentProvider).CmdCleanse(gem);
			}
			break;
		}
		}
	}

	private void InitGamepadInputs()
	{
		onSelectedSlotChanged = (Action)Delegate.Combine(onSelectedSlotChanged, (Action)delegate
		{
			isSelectingGround = false;
		});
	}

	public void SelectGround()
	{
		if (ManagerBase<ControlManager>.instance.dropConstraint == null || ManagerBase<ControlManager>.instance.dropConstraint(draggingObject))
		{
			isSelectingGround = true;
		}
	}

	public void UnselectGround()
	{
		isSelectingGround = false;
	}

	public void ClearGamepadSelection()
	{
		selectedSkillSlot = null;
		selectedGemSlot = null;
	}

	public bool DoDpadUp()
	{
		if (selectedGemSlot.HasValue)
		{
			selectedSkillSlot = selectedGemSlot.Value.skill;
			if (TryGetNextRelevantSkillLocation(next: true, skipStart: false, canWrap: true, out var loc))
			{
				selectedSkillSlot = loc;
				selectedGemSlot = null;
				return true;
			}
			selectedSkillSlot = null;
		}
		if (ManagerBase<FloatingWindowManager>.instance.currentTarget != null)
		{
			return false;
		}
		if (DewInput.GetButtonDown(GamepadButtonEx.DpadUp))
		{
			ManagerBase<GlobalUIManager>.instance.SetFocus(null);
		}
		return true;
	}

	public bool DoDpadLeft()
	{
		if (selectedGemSlot.HasValue && TryGetNextRelevantGemSlot(next: false, skipStart: true, draggingObject != null, out var gem))
		{
			selectedGemSlot = gem;
			return true;
		}
		if (selectedSkillSlot.HasValue && TryGetNextRelevantSkillLocation(next: false, skipStart: true, draggingObject != null, out var skill))
		{
			selectedSkillSlot = skill;
			return true;
		}
		if (ManagerBase<ControlManager>.instance.dropConstraint != null || ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue)
		{
			return true;
		}
		return false;
	}

	public bool DoDpadDown()
	{
		if (selectedSkillSlot.HasValue)
		{
			selectedGemSlot = new GemLocation(selectedSkillSlot.Value, 0);
			if (TryGetNextRelevantGemSlot(next: true, skipStart: false, canWrap: true, out var loc))
			{
				selectedGemSlot = loc;
				selectedSkillSlot = null;
			}
			else
			{
				selectedGemSlot = null;
			}
		}
		return true;
	}

	public bool DoDpadRight()
	{
		if (selectedGemSlot.HasValue && TryGetNextRelevantGemSlot(next: true, skipStart: true, draggingObject != null, out var gem))
		{
			selectedGemSlot = gem;
			return true;
		}
		if (selectedSkillSlot.HasValue && TryGetNextRelevantSkillLocation(next: true, skipStart: true, draggingObject != null, out var skill))
		{
			selectedSkillSlot = skill;
			return true;
		}
		if (ManagerBase<ControlManager>.instance.dropConstraint != null || ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue)
		{
			return true;
		}
		return false;
	}

	public bool DoConfirm()
	{
		if (DewPlayer.local == null)
		{
			return true;
		}
		Hero hero = DewPlayer.local.hero;
		if (hero == null)
		{
			return true;
		}
		if (draggingObject != null)
		{
			EndDrag(isCancel: false);
		}
		else if (mode == ModeType.Regular)
		{
			Gem g;
			if (selectedSkillSlot.HasValue && hero.Skill.TryGetSkill(selectedSkillSlot.Value, out var s))
			{
				StartDrag(s);
			}
			else if (selectedGemSlot.HasValue && hero.Skill.TryGetGem(selectedGemSlot.Value, out g))
			{
				StartDrag(g);
			}
		}
		else if (selectedSkillSlot.HasValue)
		{
			DoClickOnSkillButton(selectedSkillSlot.Value);
		}
		else if (selectedGemSlot.HasValue)
		{
			DoClickOnGemSlot(selectedGemSlot.Value);
		}
		return true;
	}

	public bool DoBack()
	{
		if (ManagerBase<ControlManager>.instance.isEditSkillDisabled)
		{
			return true;
		}
		if (ManagerBase<FloatingWindowManager>.instance.currentTarget != null)
		{
			ManagerBase<FloatingWindowManager>.instance.ClearTarget();
		}
		EndEdit();
		return true;
	}

	private void DropDraggingObjectGamepad()
	{
		if (ManagerBase<ControlManager>.instance.dropConstraint == null || ManagerBase<ControlManager>.instance.dropConstraint(draggingObject))
		{
			Vector3 pos = Dew.GetGoodRewardPosition(DewPlayer.local.hero.agentPosition);
			HeroSkillLocation type;
			if (draggingObject is Gem g && DewPlayer.local.hero.Skill.TryGetGemLocation(g, out var loc))
			{
				DewPlayer.local.hero.Skill.CmdUnequipGem(loc, pos);
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_GemTouch");
			}
			else if (draggingObject is SkillTrigger s && DewPlayer.local.hero.Skill.TryGetSkillLocation(s, out type))
			{
				DewPlayer.local.hero.Skill.CmdUnequipSkill(type, pos);
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillTouch");
			}
			draggingObject = null;
			onDraggingObjectChanged?.Invoke(null);
		}
	}

	private void DoGamepadInputs()
	{
		if (!(DewPlayer.local == null) && !(DewPlayer.local.hero == null) && (mode != ModeType.None || (ManagerBase<GlobalUIManager>.instance.focused != null && !InGameUIManager.instance.disablePlayingInput)))
		{
			if (it_skillQ.down)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(skillButtons.GetComponent<IGamepadFocusable>());
				selectedSkillSlot = HeroSkillLocation.Q;
				selectedGemSlot = null;
			}
			if (it_skillW.down)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(skillButtons.GetComponent<IGamepadFocusable>());
				selectedSkillSlot = HeroSkillLocation.W;
				selectedGemSlot = null;
			}
			if (it_skillE.down)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(skillButtons.GetComponent<IGamepadFocusable>());
				selectedSkillSlot = HeroSkillLocation.E;
				selectedGemSlot = null;
			}
			if (it_skillR.down)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(skillButtons.GetComponent<IGamepadFocusable>());
				selectedSkillSlot = HeroSkillLocation.R;
				selectedGemSlot = null;
			}
			if (ManagerBase<ControlManager>.instance.aimDirection.HasValue && Time.unscaledTime - lastModeSetUnscaledTime > 0.3f)
			{
				EndEdit();
			}
		}
	}

	public void SelectAnyRelevantSlot()
	{
		selectedSkillSlot = null;
		selectedGemSlot = null;
		HeroSkillLocation slot;
		GemLocation gem;
		if (ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue && IsSlotSelectable(ManagerBase<ControlManager>.instance.gemLocationConstraint.Value))
		{
			selectedSkillSlot = ManagerBase<ControlManager>.instance.gemLocationConstraint.Value;
		}
		else if (TryGetNextRelevantSkillLocation(next: true, skipStart: false, canWrap: false, out slot))
		{
			selectedSkillSlot = slot;
		}
		else if (TryGetNextRelevantGemSlot(next: true, skipStart: false, canWrap: false, out gem))
		{
			selectedGemSlot = gem;
		}
	}

	private bool TryGetNextRelevantSkillLocation(bool next, bool skipStart, bool canWrap, out HeroSkillLocation loc)
	{
		loc = HeroSkillLocation.Q;
		if (DewPlayer.local == null)
		{
			return false;
		}
		if (DewPlayer.local.hero == null)
		{
			return false;
		}
		HeroSkillLocation start = selectedSkillSlot.GetValueOrDefault();
		HeroSkillLocation cursor = start;
		bool isStart = true;
		while (isStart || cursor != start)
		{
			if ((!isStart || !skipStart) && IsSlotSelectable(cursor))
			{
				loc = cursor;
				return true;
			}
			isStart = false;
			if (next)
			{
				if (cursor == HeroSkillLocation.Identity)
				{
					if (!canWrap)
					{
						return false;
					}
					cursor = HeroSkillLocation.Q;
				}
				else
				{
					cursor++;
				}
			}
			else if (cursor == HeroSkillLocation.Q)
			{
				if (!canWrap)
				{
					return false;
				}
				cursor = HeroSkillLocation.Identity;
			}
			else
			{
				cursor--;
			}
		}
		if (IsSlotSelectable(start))
		{
			loc = start;
			return true;
		}
		return false;
	}

	private bool TryGetNextRelevantGemSlot(bool next, bool skipStart, bool canWrap, out GemLocation loc)
	{
		loc = default(GemLocation);
		if (DewPlayer.local == null)
		{
			return false;
		}
		Hero hero = DewPlayer.local.hero;
		if (hero == null)
		{
			return false;
		}
		GemLocation start = selectedGemSlot ?? new GemLocation(HeroSkillLocation.Q, 0);
		GemLocation cursor = start;
		bool isStart = true;
		while (isStart || cursor != start)
		{
			if ((!isStart || !skipStart) && IsSlotSelectable(cursor))
			{
				loc = cursor;
				return true;
			}
			isStart = false;
			if (next)
			{
				cursor.index++;
				if (cursor.index < hero.Skill.GetMaxGemCount(cursor.skill))
				{
					continue;
				}
				cursor.index = 0;
				if (cursor.skill == HeroSkillLocation.Identity)
				{
					if (!canWrap)
					{
						return false;
					}
					cursor.skill = HeroSkillLocation.Q;
				}
				else
				{
					cursor.skill++;
				}
				continue;
			}
			cursor.index--;
			if (cursor.index >= 0)
			{
				continue;
			}
			if (cursor.skill == HeroSkillLocation.Q)
			{
				if (!canWrap)
				{
					return false;
				}
				cursor.skill = HeroSkillLocation.Identity;
			}
			else
			{
				cursor.skill--;
			}
			cursor.index = hero.Skill.GetMaxGemCount(cursor.skill) - 1;
		}
		if (IsSlotSelectable(start))
		{
			loc = start;
			return true;
		}
		return false;
	}
}
