using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Gem : Actor, IInteractable, IItem, IExcludeFromPool
{
	public enum QualityType : byte
	{
		Chipped,
		Flawed,
		Regular,
		Flawless,
		Perfect,
		Otherworldly,
		Transcendent
	}

	public const float GemMergeExponent = 1f;

	public SafeAction ClientGemEvent_OnCooldownStarted;

	public SafeAction<float> ClientGemEvent_OnCooldownReduced;

	public SafeAction<float> ClientGemEvent_OnCooldownReducedByRatio;

	public SafeAction ClientGemEvent_OnCooldownReady;

	public SafeAction ClientGemEvent_OnFlash;

	public SafeAction<int, int> ClientGemEvent_OnQualityChanged;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CskipStartAnimation_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnHandOwnerChanged")]
	private Hero _003ChandOwner_003Ek__BackingField;

	public SafeAction<Hero, Hero> ClientEvent_OnHandOwnerChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnTempOwnerChanged")]
	private DewPlayer _003CtempOwner_003Ek__BackingField;

	public SafeAction<DewPlayer, DewPlayer> ClientEvent_OnTempOwnerChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnOwnerChanged")]
	private Hero _003Cowner_003Ek__BackingField;

	public SafeAction<Hero, Hero> ClientEvent_OnOwnerChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnSkillChanged")]
	private SkillTrigger _003Cskill_003Ek__BackingField;

	[SyncVar(hook = "OnQualityChange")]
	private int _quality = -1;

	public Sprite icon;

	public Rarity rarity;

	public bool isCooldownEnabled;

	public ScalingValue cooldownTime;

	public bool isRateLimited;

	public ScalingValue rateLimitTime;

	public ScalingValue rateLimitCount;

	public bool setNumberDisplay = true;

	public GameObject readyEffect;

	public bool enableStatBonus;

	public StatBonus statBonus;

	public bool excludeFromPool;

	protected AbilityTargetValidatorWrapper tvDefaultHarmfulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultUsefulEffectTargets;

	private GemWorldModel _worldModel;

	[SyncVar]
	private float _currentCooldown;

	[SyncVar]
	private int? _numberDisplay;

	private float _rateLimitRemainingUse;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisNotReadyByRateLimit_003Ek__BackingField;

	internal Action<float, float> onDismantleProgressChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnDismantleProgressChanged")]
	private float _003CdismantleProgress_003Ek__BackingField;

	private float _lastDismantleTapTime;

	private Hero _lastDismantler;

	[CompilerGenerated]
	[SyncVar]
	private int _003CmaxSellGold_003Ek__BackingField;

	protected NetworkBehaviourSyncVar ____003ChandOwner_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003CtempOwner_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003Cowner_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003Cskill_003Ek__BackingFieldNetId;

	int IInteractable.priority
	{
		get
		{
			if (!(Network_003CtempOwner_003Ek__BackingField != null) || !(Network_003CtempOwner_003Ek__BackingField != DewPlayer.local))
			{
				return 0;
			}
			return 50;
		}
	}

	public ItemWorldModel worldModel => _worldModel;

	public bool skipStartAnimation
	{
		[CompilerGenerated]
		get
		{
			return _003CskipStartAnimation_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CskipStartAnimation_003Ek__BackingField = value;
		}
	}

	public Hero handOwner
	{
		[CompilerGenerated]
		get
		{
			return Network_003ChandOwner_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003ChandOwner_003Ek__BackingField = value;
		}
	}

	public DewPlayer tempOwner
	{
		[CompilerGenerated]
		get
		{
			return Network_003CtempOwner_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CtempOwner_003Ek__BackingField = value;
		}
	}

	public bool isLocked => IsLockedFor(DewPlayer.local);

	public Hero owner
	{
		[CompilerGenerated]
		get
		{
			return Network_003Cowner_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cowner_003Ek__BackingField = value;
		}
	}

	Entity IItem.owner
	{
		get
		{
			return Network_003Cowner_003Ek__BackingField;
		}
		set
		{
			Network_003Cowner_003Ek__BackingField = value as Hero;
		}
	}

	public SkillTrigger skill
	{
		[CompilerGenerated]
		get
		{
			return Network_003Cskill_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cskill_003Ek__BackingField = value;
		}
	}

	public int quality
	{
		get
		{
			return ScalingValue.levelOverride ?? _quality;
		}
		set
		{
			Network_quality = value;
		}
	}

	public QualityType qualityType => GetQualityType(quality);

	public int effectiveLevel => Mathf.Clamp(quality, 1, int.MaxValue) + 1;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	Rarity IItem.rarity => rarity;

	public bool isValid
	{
		get
		{
			if (this != null && base.isActive)
			{
				return Network_003Cowner_003Ek__BackingField != null;
			}
			return false;
		}
	}

	public float maxCooldown
	{
		get
		{
			if (!isCooldownEnabled)
			{
				return 0f;
			}
			return GetValue(cooldownTime);
		}
	}

	public float currentCooldown => _currentCooldown;

	public int? numberDisplay
	{
		get
		{
			return _numberDisplay;
		}
		set
		{
			Network_numberDisplay = value;
		}
	}

	public bool isNotReadyByRateLimit
	{
		[CompilerGenerated]
		get
		{
			return _003CisNotReadyByRateLimit_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = value;
		}
	}

	public Transform interactPivot
	{
		get
		{
			if (!(_worldModel != null))
			{
				return base.transform;
			}
			return _worldModel.iconQuad.transform;
		}
	}

	public bool canInteractWithMouse => true;

	public float focusDistance => 3f;

	public override bool isDestroyedOnRoomChange
	{
		get
		{
			if (Network_003Cowner_003Ek__BackingField == null)
			{
				return Network_003ChandOwner_003Ek__BackingField == null;
			}
			return false;
		}
	}

	public float dismantleProgress
	{
		[CompilerGenerated]
		get
		{
			return _003CdismantleProgress_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CdismantleProgress_003Ek__BackingField = value;
		}
	}

	public int maxSellGold
	{
		[CompilerGenerated]
		get
		{
			return _003CmaxSellGold_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CmaxSellGold_003Ek__BackingField = value;
		}
	} = int.MaxValue;

	public bool Network_003CskipStartAnimation_003Ek__BackingField
	{
		get
		{
			return skipStartAnimation;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref skipStartAnimation, 4uL, null);
		}
	}

	public Hero Network_003ChandOwner_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003ChandOwner_003Ek__BackingFieldNetId, ref handOwner);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref handOwner, 8uL, OnHandOwnerChanged, ref ____003ChandOwner_003Ek__BackingFieldNetId);
		}
	}

	public DewPlayer Network_003CtempOwner_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003CtempOwner_003Ek__BackingFieldNetId, ref tempOwner);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref tempOwner, 16uL, OnTempOwnerChanged, ref ____003CtempOwner_003Ek__BackingFieldNetId);
		}
	}

	public Hero Network_003Cowner_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003Cowner_003Ek__BackingFieldNetId, ref owner);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref owner, 32uL, OnOwnerChanged, ref ____003Cowner_003Ek__BackingFieldNetId);
		}
	}

	public SkillTrigger Network_003Cskill_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003Cskill_003Ek__BackingFieldNetId, ref skill);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref skill, 64uL, OnSkillChanged, ref ____003Cskill_003Ek__BackingFieldNetId);
		}
	}

	public int Network_quality
	{
		get
		{
			return _quality;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _quality, 128uL, OnQualityChange);
		}
	}

	public float Network_currentCooldown
	{
		get
		{
			return _currentCooldown;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _currentCooldown, 256uL, null);
		}
	}

	public int? Network_numberDisplay
	{
		get
		{
			return _numberDisplay;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _numberDisplay, 512uL, null);
		}
	}

	public bool Network_003CisNotReadyByRateLimit_003Ek__BackingField
	{
		get
		{
			return isNotReadyByRateLimit;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isNotReadyByRateLimit, 1024uL, null);
		}
	}

	public float Network_003CdismantleProgress_003Ek__BackingField
	{
		get
		{
			return dismantleProgress;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref dismantleProgress, 2048uL, OnDismantleProgressChanged);
		}
	}

	public int Network_003CmaxSellGold_003Ek__BackingField
	{
		get
		{
			return maxSellGold;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref maxSellGold, 4096uL, null);
		}
	}

	private void OnTempOwnerChanged(DewPlayer _, DewPlayer __)
	{
		ClientEvent_OnTempOwnerChanged?.Invoke(_, __);
	}

	public bool IsLockedFor(DewPlayer player)
	{
		if (Network_003CtempOwner_003Ek__BackingField != null && Network_003CtempOwner_003Ek__BackingField != player)
		{
			return !Network_003CtempOwner_003Ek__BackingField.hero.IsNullInactiveDeadOrKnockedOut();
		}
		return false;
	}

	private void OnHandOwnerChanged(Hero oldOwner, Hero newOwner)
	{
		ClientEvent_OnHandOwnerChanged?.Invoke(oldOwner, newOwner);
	}

	private void OnOwnerChanged(Hero oldOwner, Hero newOwner)
	{
		if (oldOwner != null)
		{
			OnUnequipGem(oldOwner);
		}
		if (newOwner != null)
		{
			OnEquipGem(newOwner);
		}
		ClientEvent_OnOwnerChanged?.Invoke(oldOwner, newOwner);
	}

	private void OnSkillChanged(SkillTrigger oldSkill, SkillTrigger newSkill)
	{
		if (oldSkill != null)
		{
			OnUnequipSkill(oldSkill);
		}
		if (newSkill != null)
		{
			OnEquipSkill(newSkill);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(null, EntityRelation.Neutral | EntityRelation.Enemy);
		tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(null, EntityRelation.Self | EntityRelation.Ally);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (quality < 0)
		{
			quality = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>().SelectQuality(rarity);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		GameObject prefab = Resources.Load<GameObject>("WorldModels/Gem World Model");
		_worldModel = UnityEngine.Object.Instantiate(prefab, base.position, base.rotation, base.transform).GetComponent<GemWorldModel>();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (base.isServer && isCooldownEnabled && Network_003Cowner_003Ek__BackingField != null && _currentCooldown > 0f)
		{
			Network_currentCooldown = Mathf.MoveTowards(_currentCooldown, 0f, dt);
			if (_currentCooldown == 0f)
			{
				InvokeOnCooldownReady();
			}
		}
		if (base.isServer)
		{
			DoDismantleLogicUpdate();
		}
		if (base.isServer && isRateLimited)
		{
			int useCount = Mathf.RoundToInt(GetValue(rateLimitCount));
			float interval = GetValue(rateLimitTime);
			_rateLimitRemainingUse = Mathf.MoveTowards(_rateLimitRemainingUse, useCount, dt * (float)useCount / interval);
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
			if (setNumberDisplay)
			{
				numberDisplay = Mathf.FloorToInt(_rateLimitRemainingUse);
			}
		}
	}

	public virtual void OnEquipGem(Hero newOwner)
	{
		if (base.isServer)
		{
			Network_003CtempOwner_003Ek__BackingField = null;
			Network_003CskipStartAnimation_003Ek__BackingField = true;
		}
		base.transform.localPosition = Vector3.zero;
		if (base.isServer && enableStatBonus)
		{
			newOwner.Status.AddStatBonus(statBonus);
		}
		tvDefaultUsefulEffectTargets.self = newOwner;
		tvDefaultHarmfulEffectTargets.self = newOwner;
	}

	public virtual void OnUnequipGem(Hero oldOwner)
	{
		if (base.isServer && oldOwner != null && enableStatBonus)
		{
			oldOwner.Status.RemoveStatBonus(statBonus);
		}
		tvDefaultUsefulEffectTargets.self = null;
		tvDefaultHarmfulEffectTargets.self = null;
	}

	public virtual void OnEquipSkill(SkillTrigger newSkill)
	{
		if (base.isServer)
		{
			newSkill.TriggerEvent_OnCastComplete = (Action<EventInfoCast>)Delegate.Combine(newSkill.TriggerEvent_OnCastComplete, new Action<EventInfoCast>(OnCastComplete));
			newSkill.TriggerEvent_OnCastCompleteBeforePrepare = (Action<EventInfoCast>)Delegate.Combine(newSkill.TriggerEvent_OnCastCompleteBeforePrepare, new Action<EventInfoCast>(OnCastCompleteBeforePrepare));
			newSkill.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(OnDealDamage);
			newSkill.ActorEvent_OnDoHeal += new Action<EventInfoHeal>(OnDoHeal);
		}
	}

	public virtual void OnUnequipSkill(SkillTrigger oldSkill)
	{
		if (base.isServer && !(oldSkill == null))
		{
			oldSkill.TriggerEvent_OnCastComplete = (Action<EventInfoCast>)Delegate.Remove(oldSkill.TriggerEvent_OnCastComplete, new Action<EventInfoCast>(OnCastComplete));
			oldSkill.TriggerEvent_OnCastCompleteBeforePrepare = (Action<EventInfoCast>)Delegate.Remove(oldSkill.TriggerEvent_OnCastCompleteBeforePrepare, new Action<EventInfoCast>(OnCastCompleteBeforePrepare));
			oldSkill.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(OnDealDamage);
			oldSkill.ActorEvent_OnDoHeal -= new Action<EventInfoHeal>(OnDoHeal);
		}
	}

	protected virtual void OnQualityChange(int oldQuality, int newQuality)
	{
		if (base.isServer)
		{
			Network_003CmaxSellGold_003Ek__BackingField = int.MaxValue;
		}
		if (Network_003Cowner_003Ek__BackingField != null)
		{
			Network_003Cowner_003Ek__BackingField.Skill.ClientHeroEvent_OnGemQualityChanged?.Invoke(this, oldQuality, newQuality);
		}
		ClientGemEvent_OnQualityChanged?.Invoke(oldQuality, newQuality);
	}

	[ClientRpc]
	internal void RpcSetPositionAndRotation(Vector3 pos, Quaternion rot)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(pos);
		writer.WriteQuaternion(rot);
		SendRPCInternal("System.Void Gem::RpcSetPositionAndRotation(UnityEngine.Vector3,UnityEngine.Quaternion)", 1135075037, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public float GetValue(ScalingValue val)
	{
		if (base.netIdentity == null)
		{
			return val.GetValue(effectiveLevel, null);
		}
		return val.GetValue(effectiveLevel, Network_003Cowner_003Ek__BackingField);
	}

	protected virtual void OnDealDamage(EventInfoDamage info)
	{
	}

	protected virtual void OnDoHeal(EventInfoHeal obj)
	{
	}

	protected virtual void OnCastComplete(EventInfoCast info)
	{
	}

	protected virtual void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
	}

	public bool CanInteract(Entity entity)
	{
		if (Network_003Cowner_003Ek__BackingField == null && Network_003ChandOwner_003Ek__BackingField == null && !_worldModel.isAnimating && Time.time - base.creationTime > 0.5f)
		{
			return !IsLockedFor(entity.owner);
		}
		return false;
	}

	void IInteractable.OnInteract(Entity entity, bool alt)
	{
		if (!(entity is Hero hero) || IsLockedFor(entity.owner))
		{
			return;
		}
		if (alt)
		{
			if (hero.isOwned && !ManagerBase<ControlManager>.instance.isDismantleDisabled && (ManagerBase<ControlManager>.instance.dismantleConstraint == null || ManagerBase<ControlManager>.instance.dismantleConstraint(this)))
			{
				CmdDoDismantleTap();
			}
		}
		else if (base.isServer)
		{
			if (hero.Skill.TryGetEquippedGemOfSameType(GetType(), out var _, out var gem))
			{
				hero.Skill.MergeGem(this, gem);
			}
			else
			{
				hero.Skill.HoldInHand(this);
			}
		}
	}

	public static int GetMergedQuality(int a, int b)
	{
		return Mathf.Clamp(Mathf.RoundToInt(Mathf.Pow(Mathf.Pow(a, 1f) + Mathf.Pow(b, 1f), 1f)), Mathf.Max(a, b) + 1, int.MaxValue);
	}

	[Server]
	public void StartCooldown()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::StartCooldown()' called when server was not active");
			return;
		}
		if (!isCooldownEnabled)
		{
			throw new InvalidOperationException("Cooldown is disabled");
		}
		Network_currentCooldown = maxCooldown;
		InvokeOnCooldownStarted();
	}

	[Server]
	public void ApplyCooldownReduction(float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::ApplyCooldownReduction(System.Single)' called when server was not active");
			return;
		}
		InvokeOnCooldownReduced(amount);
		if (isCooldownEnabled)
		{
			Network_currentCooldown = Mathf.MoveTowards(_currentCooldown, 0f, amount);
			if (_currentCooldown == 0f)
			{
				InvokeOnCooldownReady();
			}
		}
		if (isRateLimited)
		{
			int useCount = Mathf.RoundToInt(GetValue(rateLimitCount));
			float interval = GetValue(rateLimitTime);
			_rateLimitRemainingUse = Mathf.MoveTowards(_rateLimitRemainingUse, useCount, amount * (float)useCount / interval);
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
			if (setNumberDisplay)
			{
				numberDisplay = Mathf.FloorToInt(_rateLimitRemainingUse);
			}
		}
	}

	[Server]
	public void ApplyCooldownReductionByRatio(float ratio)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::ApplyCooldownReductionByRatio(System.Single)' called when server was not active");
			return;
		}
		InvokeOnCooldownReducedByRatio(ratio);
		if (isCooldownEnabled)
		{
			Network_currentCooldown = Mathf.MoveTowards(_currentCooldown, 0f, GetValue(cooldownTime) * ratio);
			if (_currentCooldown == 0f)
			{
				InvokeOnCooldownReady();
			}
		}
		if (isRateLimited)
		{
			int useCount = Mathf.RoundToInt(GetValue(rateLimitCount));
			_rateLimitRemainingUse = Mathf.MoveTowards(_rateLimitRemainingUse, useCount, ratio * (float)useCount);
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
			if (setNumberDisplay)
			{
				numberDisplay = Mathf.FloorToInt(_rateLimitRemainingUse);
			}
		}
	}

	[Server]
	public void ResetCooldown()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::ResetCooldown()' called when server was not active");
			return;
		}
		if (!isCooldownEnabled && !isRateLimited)
		{
			throw new InvalidOperationException("Cooldown is disabled");
		}
		if (isCooldownEnabled && _currentCooldown > 0f)
		{
			Network_currentCooldown = 0f;
			InvokeOnCooldownReady();
		}
		if (!isRateLimited)
		{
			return;
		}
		int useCount = Mathf.RoundToInt(GetValue(rateLimitCount));
		if (_rateLimitRemainingUse < (float)useCount)
		{
			_rateLimitRemainingUse = useCount;
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
			if (setNumberDisplay)
			{
				numberDisplay = Mathf.FloorToInt(_rateLimitRemainingUse);
			}
			InvokeOnCooldownReady();
		}
	}

	[Server]
	public void NotifyUse()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::NotifyUse()' called when server was not active");
			return;
		}
		InvokeFlash();
		if (isRateLimited)
		{
			_rateLimitRemainingUse -= 1f;
			Network_003CisNotReadyByRateLimit_003Ek__BackingField = _rateLimitRemainingUse < 1f;
		}
	}

	[ClientRpc]
	private void InvokeOnCooldownReady()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Gem::InvokeOnCooldownReady()", -1566986505, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnCooldownStarted()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Gem::InvokeOnCooldownStarted()", 622452117, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnCooldownReducedByRatio(float ratio)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(ratio);
		SendRPCInternal("System.Void Gem::InvokeOnCooldownReducedByRatio(System.Single)", 1296628743, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnCooldownReduced(float amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(amount);
		SendRPCInternal("System.Void Gem::InvokeOnCooldownReduced(System.Single)", 717694297, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeFlash()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Gem::InvokeFlash()", -1601952754, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public virtual bool IsReady()
	{
		if (!isCooldownEnabled || _currentCooldown <= 0f)
		{
			if (isRateLimited)
			{
				return !isNotReadyByRateLimit;
			}
			return true;
		}
		return false;
	}

	private void OnDismantleProgressChanged(float oldVal, float newVal)
	{
		onDismantleProgressChanged?.Invoke(oldVal, newVal);
		if (base.isServer && newVal >= 1f && Network_003Cowner_003Ek__BackingField == null && Network_003ChandOwner_003Ek__BackingField == null && base.isActive)
		{
			DismantleGem();
		}
	}

	[Server]
	public void DismantleGem()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Gem::DismantleGem()' called when server was not active");
		}
		else if (!(_lastDismantler == null) && !(_lastDismantler.owner == null))
		{
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, GetDismantleAmount(_lastDismantler.owner), base.position, _lastDismantler);
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnDismantled(_lastDismantler, this);
			Destroy();
		}
	}

	public int GetDismantleAmount(DewPlayer player)
	{
		if (player == null)
		{
			player = _lastDismantler.owner;
		}
		float floatAmount = NetworkedManagerBase<GameManager>.instance.gss.gemDismantleDreamDustByQuality.Evaluate(quality);
		floatAmount *= NetworkedManagerBase<GameManager>.instance.gss.gemDismantleDreamDustMultiplier.Get(rarity);
		if (player != null)
		{
			floatAmount *= player.dismantleDreamDustMultiplier;
		}
		floatAmount *= DewBuildProfile.current.dismantleDreamDustMultiplier;
		return Mathf.Max(1, Mathf.RoundToInt(floatAmount));
	}

	[Command(requiresAuthority = false)]
	private void CmdDoDismantleTap(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void Gem::CmdDoDismantleTap(Mirror.NetworkConnectionToClient)", 1150960282, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	private void DoDismantleLogicUpdate()
	{
		if ((dismantleProgress > 0f && Time.time - _lastDismantleTapTime > 1f) || Network_003Cowner_003Ek__BackingField != null || Network_003ChandOwner_003Ek__BackingField != null)
		{
			Network_003CdismantleProgress_003Ek__BackingField = 0f;
		}
	}

	public static QualityType GetQualityType(float quality)
	{
		if (quality < 50f)
		{
			return QualityType.Chipped;
		}
		if (quality < 100f)
		{
			return QualityType.Flawed;
		}
		if (quality < 200f)
		{
			return QualityType.Regular;
		}
		if (quality < 400f)
		{
			return QualityType.Flawless;
		}
		if (quality < 600f)
		{
			return QualityType.Perfect;
		}
		if (quality < 1000f)
		{
			return QualityType.Otherworldly;
		}
		return QualityType.Transcendent;
	}

	public int GetBuyGold(DewPlayer player)
	{
		return GetBuyGold(player, rarity, quality);
	}

	public int GetSellGold(DewPlayer player)
	{
		return Mathf.Min(GetSellGold(player, rarity, quality), maxSellGold);
	}

	public static int GetBuyGold(DewPlayer player, Rarity rarity, int quality)
	{
		float floatVal = GetPrice(rarity, quality);
		if (player != null)
		{
			floatVal *= player.buyPriceMultiplier;
		}
		return Mathf.Max(Mathf.RoundToInt(floatVal), 1);
	}

	public static int GetSellGold(DewPlayer player, Rarity rarity, int quality)
	{
		float floatVal = GetPrice(rarity, quality);
		if (player != null)
		{
			floatVal *= player.sellPriceMultiplier;
		}
		floatVal *= NetworkedManagerBase<GameManager>.instance.gss.sellValueMultiplier;
		return Mathf.Max(Mathf.RoundToInt(floatVal), 1);
	}

	public static int GetPrice(Rarity rarity, int quality)
	{
		DewGameSessionSettings gss = NetworkedManagerBase<GameManager>.instance.gss;
		return Mathf.Max(Mathf.RoundToInt((float)gss.gemValue.Get(rarity) * gss.valueGlobalMultiplier * (1f + (gss.valueMultiplierPerGemQuality - 1f) * (float)quality)), 1);
	}

	public override bool ShouldBeSaved()
	{
		return Network_003Cowner_003Ek__BackingField == null;
	}

	public override void OnSaveActor(Dictionary<string, object> data)
	{
		base.OnSaveActor(data);
		data["quality"] = quality;
	}

	public override Actor OnLoadCreateActor(Dictionary<string, object> data, Actor parent)
	{
		return Dew.CreateGem(this, (Vector3)data["pos"], (int)data["quality"], null, delegate(Gem g)
		{
			g.Network_003CskipStartAnimation_003Ek__BackingField = true;
		});
	}

	[Server]
	public T CreateAbilityInstanceWithSource<T>(Actor source, Vector3 position, Quaternion? rotation, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T Gem::CreateAbilityInstanceWithSource(Actor,UnityEngine.Vector3,System.Nullable`1<UnityEngine.Quaternion>,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return source.CreateAbilityInstance(position, rotation, info, delegate(T t)
		{
			t.skillLevel = effectiveLevel;
			t.gem = this;
			beforePrepare?.Invoke(t);
		});
	}

	[Server]
	public T CreateStatusEffectWithSource<T>(Actor source, Entity victim, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T Gem::CreateStatusEffectWithSource(Actor,Entity,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return source.CreateStatusEffect(victim, info, delegate(T t)
		{
			t.skillLevel = effectiveLevel;
			t.gem = this;
			beforePrepare?.Invoke(t);
		});
	}

	public Se_GenericEffectContainer CreateBasicEffectWithSource(Actor source, Entity victim, BasicEffect eff, float duration, string id, DuplicateEffectBehavior onDuplicate = DuplicateEffectBehavior.ReplacePrevious)
	{
		return source.CreateBasicEffect(victim, eff, duration, id, onDuplicate);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSetPositionAndRotation__Vector3__Quaternion(Vector3 pos, Quaternion rot)
	{
		base.transform.SetPositionAndRotation(pos, rot);
	}

	protected static void InvokeUserCode_RpcSetPositionAndRotation__Vector3__Quaternion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetPositionAndRotation called on server.");
		}
		else
		{
			((Gem)obj).UserCode_RpcSetPositionAndRotation__Vector3__Quaternion(reader.ReadVector3(), reader.ReadQuaternion());
		}
	}

	protected void UserCode_InvokeOnCooldownReady()
	{
		ClientGemEvent_OnCooldownReady?.Invoke();
	}

	protected static void InvokeUserCode_InvokeOnCooldownReady(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCooldownReady called on server.");
		}
		else
		{
			((Gem)obj).UserCode_InvokeOnCooldownReady();
		}
	}

	protected void UserCode_InvokeOnCooldownStarted()
	{
		ClientGemEvent_OnCooldownStarted?.Invoke();
	}

	protected static void InvokeUserCode_InvokeOnCooldownStarted(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCooldownStarted called on server.");
		}
		else
		{
			((Gem)obj).UserCode_InvokeOnCooldownStarted();
		}
	}

	protected void UserCode_InvokeOnCooldownReducedByRatio__Single(float ratio)
	{
		ClientGemEvent_OnCooldownReducedByRatio?.Invoke(ratio);
	}

	protected static void InvokeUserCode_InvokeOnCooldownReducedByRatio__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCooldownReducedByRatio called on server.");
		}
		else
		{
			((Gem)obj).UserCode_InvokeOnCooldownReducedByRatio__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_InvokeOnCooldownReduced__Single(float amount)
	{
		ClientGemEvent_OnCooldownReduced?.Invoke(amount);
	}

	protected static void InvokeUserCode_InvokeOnCooldownReduced__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCooldownReduced called on server.");
		}
		else
		{
			((Gem)obj).UserCode_InvokeOnCooldownReduced__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_InvokeFlash()
	{
		ClientGemEvent_OnFlash?.Invoke();
	}

	protected static void InvokeUserCode_InvokeFlash(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeFlash called on server.");
		}
		else
		{
			((Gem)obj).UserCode_InvokeFlash();
		}
	}

	protected void UserCode_CmdDoDismantleTap__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if (!(Network_003Cowner_003Ek__BackingField != null) && !(Network_003ChandOwner_003Ek__BackingField != null) && !(Time.time - _lastDismantleTapTime < 0.075f) && !(sender.GetHero() == null))
		{
			_lastDismantleTapTime = Time.time;
			dismantleProgress += 0.4f;
			_lastDismantler = sender.GetHero();
		}
	}

	protected static void InvokeUserCode_CmdDoDismantleTap__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoDismantleTap called on client.");
		}
		else
		{
			((Gem)obj).UserCode_CmdDoDismantleTap__NetworkConnectionToClient(senderConnection);
		}
	}

	static Gem()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Gem), "System.Void Gem::CmdDoDismantleTap(Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdDoDismantleTap__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::RpcSetPositionAndRotation(UnityEngine.Vector3,UnityEngine.Quaternion)", InvokeUserCode_RpcSetPositionAndRotation__Vector3__Quaternion);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeOnCooldownReady()", InvokeUserCode_InvokeOnCooldownReady);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeOnCooldownStarted()", InvokeUserCode_InvokeOnCooldownStarted);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeOnCooldownReducedByRatio(System.Single)", InvokeUserCode_InvokeOnCooldownReducedByRatio__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeOnCooldownReduced(System.Single)", InvokeUserCode_InvokeOnCooldownReduced__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Gem), "System.Void Gem::InvokeFlash()", InvokeUserCode_InvokeFlash);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(skipStartAnimation);
			writer.WriteNetworkBehaviour(Network_003ChandOwner_003Ek__BackingField);
			writer.WriteNetworkBehaviour(Network_003CtempOwner_003Ek__BackingField);
			writer.WriteNetworkBehaviour(Network_003Cowner_003Ek__BackingField);
			writer.WriteNetworkBehaviour(Network_003Cskill_003Ek__BackingField);
			writer.WriteInt(_quality);
			writer.WriteFloat(_currentCooldown);
			writer.WriteIntNullable(_numberDisplay);
			writer.WriteBool(isNotReadyByRateLimit);
			writer.WriteFloat(dismantleProgress);
			writer.WriteInt(maxSellGold);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(skipStartAnimation);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003ChandOwner_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003CtempOwner_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003Cowner_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003Cskill_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteInt(_quality);
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteFloat(_currentCooldown);
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteIntNullable(_numberDisplay);
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteBool(isNotReadyByRateLimit);
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteFloat(dismantleProgress);
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			writer.WriteInt(maxSellGold);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref skipStartAnimation, null, reader.ReadBool());
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref handOwner, OnHandOwnerChanged, reader, ref ____003ChandOwner_003Ek__BackingFieldNetId);
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref tempOwner, OnTempOwnerChanged, reader, ref ____003CtempOwner_003Ek__BackingFieldNetId);
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref owner, OnOwnerChanged, reader, ref ____003Cowner_003Ek__BackingFieldNetId);
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref skill, OnSkillChanged, reader, ref ____003Cskill_003Ek__BackingFieldNetId);
			GeneratedSyncVarDeserialize(ref _quality, OnQualityChange, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref _currentCooldown, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _numberDisplay, null, reader.ReadIntNullable());
			GeneratedSyncVarDeserialize(ref isNotReadyByRateLimit, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref dismantleProgress, OnDismantleProgressChanged, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref maxSellGold, null, reader.ReadInt());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref skipStartAnimation, null, reader.ReadBool());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref handOwner, OnHandOwnerChanged, reader, ref ____003ChandOwner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref tempOwner, OnTempOwnerChanged, reader, ref ____003CtempOwner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref owner, OnOwnerChanged, reader, ref ____003Cowner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref skill, OnSkillChanged, reader, ref ____003Cskill_003Ek__BackingFieldNetId);
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _quality, OnQualityChange, reader.ReadInt());
		}
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _currentCooldown, null, reader.ReadFloat());
		}
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _numberDisplay, null, reader.ReadIntNullable());
		}
		if ((num & 0x400L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isNotReadyByRateLimit, null, reader.ReadBool());
		}
		if ((num & 0x800L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref dismantleProgress, OnDismantleProgressChanged, reader.ReadFloat());
		}
		if ((num & 0x1000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref maxSellGold, null, reader.ReadInt());
		}
	}
}
