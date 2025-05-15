using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class StatusEffect : AbilityInstance
{
	[SyncVar]
	private Entity _victim;

	[FormerlySerializedAs("isBeneficial")]
	public bool isBeneficialBuff;

	public bool isKilledByCrowdControlImmunity;

	public bool scaleDurationByTenacity;

	[SyncVar(hook = "OnShowIconChanged")]
	public bool showIcon;

	public bool hideOnWorldHealthBar;

	public Sprite icon;

	public int iconOrder;

	public GameObject startEffectVictim;

	public GameObject endEffectVictim;

	public Material[] addedMaterials;

	[SyncVar]
	private float? _maxDuration;

	[SyncVar]
	private float? _remainingDuration;

	private const bool DoOrphanCheckOnlyWithSkillTriggers = true;

	private bool _doOrphanCheck;

	private AbilityTrigger _orphanCheckParentTrigger;

	private OnScreenTimerHandle _locallyShownTimer;

	private bool _isIconShown;

	private List<BasicEffect> _basicEffects = new List<BasicEffect>();

	private List<StatBonus> _statBonuses = new List<StatBonus>();

	protected NetworkBehaviourSyncVar ____victimNetId;

	public override bool isDestroyedOnRoomChange => false;

	public Entity victim
	{
		get
		{
			return Network_victim;
		}
		internal set
		{
			Network_victim = value;
		}
	}

	public float? maxDuration => _maxDuration;

	public float? remainingDuration => _remainingDuration;

	public float? normalizedDuration
	{
		get
		{
			if (!_remainingDuration.HasValue || !_maxDuration.HasValue)
			{
				return null;
			}
			return _remainingDuration.Value / _maxDuration.Value;
		}
	}

	public IReadOnlyList<BasicEffect> basicEffects => _basicEffects;

	public Entity Network_victim
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____victimNetId, ref _victim);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _victim, 32uL, null, ref ____victimNetId);
		}
	}

	public bool NetworkshowIcon
	{
		get
		{
			return showIcon;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref showIcon, 64uL, OnShowIconChanged);
		}
	}

	public float? Network_maxDuration
	{
		get
		{
			return _maxDuration;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _maxDuration, 128uL, null);
		}
	}

	public float? Network_remainingDuration
	{
		get
		{
			return _remainingDuration;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _remainingDuration, 256uL, null);
		}
	}

	[Server]
	protected StatBonus DoStatBonus()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StatBonus StatusEffect::DoStatBonus()' called when server was not active");
			return null;
		}
		if (!base.isActive)
		{
			return null;
		}
		StatBonus newBonus = new StatBonus();
		_statBonuses.Add(newBonus);
		victim.Status.AddStatBonus(newBonus);
		return newBonus;
	}

	[Server]
	protected StatBonus DoStatBonus(StatBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StatBonus StatusEffect::DoStatBonus(StatBonus)' called when server was not active");
			return null;
		}
		if (!base.isActive)
		{
			return null;
		}
		_statBonuses.Add(bonus);
		victim.Status.AddStatBonus(bonus);
		return bonus;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnShowIconChanged(oldVal: false, showIcon);
		if (base.isActive && startEffectVictim != null)
		{
			FxPlay(startEffectVictim, victim);
		}
		if (base.isActive && addedMaterials != null)
		{
			Material[] array = addedMaterials;
			foreach (Material mat in array)
			{
				victim.Visual.AddSharedMaterialLocal(mat);
			}
		}
		if (base.firstTrigger != null && base.firstTrigger is SkillTrigger)
		{
			_doOrphanCheck = true;
			_orphanCheckParentTrigger = base.firstTrigger;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (victim != null)
		{
			base.transform.position = victim.transform.position;
		}
		if (!base.isActive || !base.isServer)
		{
			return;
		}
		if (victim == null || !victim.isActive || (isKilledByCrowdControlImmunity && victim.Status.hasCrowdControlImmunity))
		{
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnIgnoreCC(victim);
			Destroy();
		}
		else if (_remainingDuration.HasValue)
		{
			float deltaTime = Time.deltaTime;
			if (scaleDurationByTenacity)
			{
				deltaTime *= 1f + victim.Status.tenacity / 100f;
			}
			Network_remainingDuration = Mathf.MoveTowards(_remainingDuration.Value, 0f, deltaTime);
			if (_remainingDuration.Value <= 0f)
			{
				StopTimer();
				Destroy();
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && _doOrphanCheck && (_orphanCheckParentTrigger.IsNullOrInactive() || _orphanCheckParentTrigger.owner.IsNullOrInactive()))
		{
			Debug.Log("Destroying orphaned status effect: " + GetActorReadableName());
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_isIconShown)
		{
			_isIconShown = false;
			try
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnHideStatusEffectIcon?.Invoke(this);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (_locallyShownTimer != null)
		{
			try
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnHideOnScreenTimer?.Invoke(_locallyShownTimer);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			_locallyShownTimer = null;
		}
		if (endEffectVictim != null)
		{
			FxPlay(endEffectVictim, victim);
		}
		if (startEffectVictim != null)
		{
			FxStop(startEffectVictim);
		}
		if (!(victim != null))
		{
			return;
		}
		if (base.isServer)
		{
			victim.Status.RemoveStatusEffect(this);
			foreach (BasicEffect beff in _basicEffects)
			{
				victim.Status.RemoveBasicEffect(beff);
			}
			_basicEffects.Clear();
			foreach (StatBonus sb in _statBonuses)
			{
				victim.Status.RemoveStatBonus(sb);
			}
			_statBonuses.Clear();
		}
		if (addedMaterials != null)
		{
			Material[] array = addedMaterials;
			foreach (Material mat in array)
			{
				victim.Visual.RemoveMaterialLocal(mat);
			}
		}
	}

	[Server]
	public void SetTimer(float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::SetTimer(System.Single)' called when server was not active");
			return;
		}
		Network_maxDuration = duration;
		Network_remainingDuration = duration;
	}

	[Server]
	public void ShowOnScreenTimer(string customNameKey = null, Color color = default(Color))
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::ShowOnScreenTimer(System.String,UnityEngine.Color)' called when server was not active");
		}
		else if (!(victim == null) && !(victim.owner == null) && victim.owner.connectionToClient != null)
		{
			if (victim.isOwned)
			{
				SetOnScreenTimerLocal(value: true, customNameKey, color);
			}
			else
			{
				TpcSetOnScreenTimer(victim.owner.connectionToClient, value: true, customNameKey, color);
			}
		}
	}

	[Server]
	public void HideOnScreenTimer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::HideOnScreenTimer()' called when server was not active");
		}
		else if (!(victim == null) && !(victim.owner == null) && victim.owner.connectionToClient != null)
		{
			if (victim.isOwned)
			{
				SetOnScreenTimerLocal(value: false, null, default(Color));
			}
			else
			{
				TpcSetOnScreenTimer(victim.owner.connectionToClient, value: false, null, default(Color));
			}
		}
	}

	[TargetRpc]
	private void TpcSetOnScreenTimer(NetworkConnectionToClient target, bool value, string customNameKey, Color color)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(value);
		writer.WriteString(customNameKey);
		writer.WriteColor(color);
		SendTargetRPCInternal(target, "System.Void StatusEffect::TpcSetOnScreenTimer(Mirror.NetworkConnectionToClient,System.Boolean,System.String,UnityEngine.Color)", 1746918851, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void SetOnScreenTimerLocal(bool value, string customNameKey, Color color)
	{
		if (!base.isActive)
		{
			return;
		}
		if (_locallyShownTimer != null)
		{
			try
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnHideOnScreenTimer?.Invoke(_locallyShownTimer);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			_locallyShownTimer = null;
		}
		if (!value)
		{
			return;
		}
		string timerText;
		if (!string.IsNullOrEmpty(customNameKey))
		{
			timerText = (customNameKey.StartsWith("St_") ? DewLocalization.GetSkillName(DewLocalization.GetSkillKey(customNameKey), 0) : ((!customNameKey.StartsWith("Gem_")) ? DewLocalization.GetUIValue(customNameKey) : DewLocalization.GetGemName(DewLocalization.GetGemKey(customNameKey))));
		}
		else if (!DewLocalization.TryGetUIValue(GetType().Name + "_Name", out timerText))
		{
			AbilityTrigger trg = base.firstTrigger;
			if (trg != null)
			{
				timerText = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(trg.GetType()), 0);
			}
		}
		try
		{
			OnScreenTimerHandle newTimer = (_locallyShownTimer = new OnScreenTimerHandle
			{
				rawText = timerText,
				fillAmountGetter = delegate
				{
					if (normalizedDuration.HasValue)
					{
						return normalizedDuration.Value;
					}
					return (this is StackedStatusEffect { autoDecay: not false } stackedStatusEffect) ? (stackedStatusEffect.remainingDecayTime / stackedStatusEffect.decayTime) : 1f;
				},
				color = color
			});
			NetworkedManagerBase<ClientEventManager>.instance.OnShowOnScreenTimer?.Invoke(newTimer);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	[Server]
	public void SetTimer(float maxDuration, float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::SetTimer(System.Single,System.Single)' called when server was not active");
			return;
		}
		Network_maxDuration = maxDuration;
		Network_remainingDuration = duration;
	}

	[Server]
	public void ResetTimer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::ResetTimer()' called when server was not active");
		}
		else
		{
			Network_remainingDuration = _maxDuration;
		}
	}

	[Server]
	public void StopTimer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::StopTimer()' called when server was not active");
			return;
		}
		if (!_maxDuration.HasValue)
		{
			Debug.LogWarning($"Tried to stop a timer when maxDuration was not set: {this}");
		}
		if (!_remainingDuration.HasValue)
		{
			Debug.LogWarning($"Tried to stop a timer when maxDuration was not set: {this}");
		}
		Network_maxDuration = null;
		Network_remainingDuration = null;
	}

	public override string GetActorReadableName()
	{
		return base.GetActorReadableName() + " " + ((victim != null) ? $" on [{victim.persistentNetId}]" : "");
	}

	private void OnShowIconChanged(bool oldVal, bool newVal)
	{
		if (victim == null)
		{
			return;
		}
		try
		{
			if (newVal && !_isIconShown)
			{
				_isIconShown = true;
				NetworkedManagerBase<ClientEventManager>.instance.OnShowStatusEffectIcon?.Invoke(this);
			}
			else if (!newVal && _isIconShown)
			{
				_isIconShown = false;
				NetworkedManagerBase<ClientEventManager>.instance.OnHideStatusEffectIcon?.Invoke(this);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void DoBasicEffect(BasicEffect eff)
	{
		if (!(victim == null))
		{
			eff.victim = victim;
			eff.parent = this;
			victim.Status.AddBasicEffect(eff);
			_basicEffects.Add(eff);
		}
	}

	private T DoBasicEffect<T>() where T : BasicEffect, new()
	{
		T eff = new T
		{
			victim = victim,
			parent = this
		};
		victim.Status.AddBasicEffect(eff);
		_basicEffects.Add(eff);
		return eff;
	}

	private T DoBasicEffectNoRegister<T>() where T : BasicEffect, new()
	{
		return new T
		{
			victim = victim,
			parent = this
		};
	}

	private void RegisterBasicEffect<T>(T eff) where T : BasicEffect, new()
	{
		victim.Status.AddBasicEffect(eff);
		_basicEffects.Add(eff);
	}

	private T DoBasicEffectWithStrength<T>(float strength) where T : BasicEffectWithStrength, new()
	{
		T eff = DoBasicEffectNoRegister<T>();
		eff.strength = strength;
		RegisterBasicEffect(eff);
		return eff;
	}

	[Server]
	protected SlowEffect DoSlow(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SlowEffect StatusEffect::DoSlow(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<SlowEffect>(strength);
	}

	[Server]
	protected SpeedEffect DoSpeed(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SpeedEffect StatusEffect::DoSpeed(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<SpeedEffect>(strength);
	}

	[Server]
	protected HasteEffect DoHaste(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'HasteEffect StatusEffect::DoHaste(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<HasteEffect>(strength);
	}

	[Server]
	protected CrippleEffect DoCripple(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'CrippleEffect StatusEffect::DoCripple(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<CrippleEffect>(strength);
	}

	[Server]
	protected RootEffect DoRoot()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'RootEffect StatusEffect::DoRoot()' called when server was not active");
			return null;
		}
		return DoBasicEffect<RootEffect>();
	}

	[Server]
	protected SilenceEffect DoSilence()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SilenceEffect StatusEffect::DoSilence()' called when server was not active");
			return null;
		}
		return DoBasicEffect<SilenceEffect>();
	}

	[Server]
	protected StunEffect DoStun()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StunEffect StatusEffect::DoStun()' called when server was not active");
			return null;
		}
		return DoBasicEffect<StunEffect>();
	}

	[Server]
	protected BlindEffect DoBlind()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'BlindEffect StatusEffect::DoBlind()' called when server was not active");
			return null;
		}
		return DoBasicEffect<BlindEffect>();
	}

	[Server]
	protected AttackCriticalEffect DoAttackCritical(Action onUse)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AttackCriticalEffect StatusEffect::DoAttackCritical(System.Action)' called when server was not active");
			return null;
		}
		AttackCriticalEffect eff = DoBasicEffectNoRegister<AttackCriticalEffect>();
		eff.onUse = onUse;
		RegisterBasicEffect(eff);
		return eff;
	}

	[Server]
	protected AttackOverrideEffect DoAttackOverride(AbilityTrigger trigger, Action onUse)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AttackOverrideEffect StatusEffect::DoAttackOverride(AbilityTrigger,System.Action)' called when server was not active");
			return null;
		}
		AttackOverrideEffect eff = DoBasicEffectNoRegister<AttackOverrideEffect>();
		eff.trigger = trigger;
		eff.onUse = onUse;
		eff._shouldTriggerBeDisposed = false;
		RegisterBasicEffect(eff);
		return eff;
	}

	[Server]
	protected AttackOverrideEffect DoAttackOverride<T>(Action onUse) where T : AbilityTrigger
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AttackOverrideEffect StatusEffect::DoAttackOverride(System.Action)' called when server was not active");
			return null;
		}
		AttackOverrideEffect eff = DoBasicEffectNoRegister<AttackOverrideEffect>();
		eff.trigger = Dew.CreateAbilityTrigger<T>();
		eff.onUse = onUse;
		eff._shouldTriggerBeDisposed = true;
		RegisterBasicEffect(eff);
		return eff;
	}

	[Server]
	protected AttackEmpowerEffect DoAttackEmpower(Action<EventInfoAttackEffect, int> onAttackEffect, int maxTriggerCount = int.MaxValue, Action onDepleted = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AttackEmpowerEffect StatusEffect::DoAttackEmpower(System.Action`2<EventInfoAttackEffect,System.Int32>,System.Int32,System.Action)' called when server was not active");
			return null;
		}
		AttackEmpowerEffect eff = DoBasicEffectNoRegister<AttackEmpowerEffect>();
		eff.onAttackEffect = onAttackEffect;
		eff.onDepleted = onDepleted;
		eff.maxTriggerCount = maxTriggerCount;
		RegisterBasicEffect(eff);
		return eff;
	}

	[Server]
	protected InvulnerableEffect DoInvulnerable(Action<EventInfoDamageNegatedByImmunity> onDamageNegated = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'InvulnerableEffect StatusEffect::DoInvulnerable(System.Action`1<EventInfoDamageNegatedByImmunity>)' called when server was not active");
			return null;
		}
		InvulnerableEffect eff = DoBasicEffectNoRegister<InvulnerableEffect>();
		eff.onDamageNegated = onDamageNegated;
		RegisterBasicEffect(eff);
		return eff;
	}

	[Server]
	protected UnstoppableEffect DoUnstoppable()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UnstoppableEffect StatusEffect::DoUnstoppable()' called when server was not active");
			return null;
		}
		return DoBasicEffect<UnstoppableEffect>();
	}

	[Server]
	protected InvisibleEffect DoInvisible()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'InvisibleEffect StatusEffect::DoInvisible()' called when server was not active");
			return null;
		}
		return DoBasicEffect<InvisibleEffect>();
	}

	[Server]
	protected ProtectedEffect DoProtected(Action<EventInfoDamageNegatedByImmunity> onDamageNegated)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ProtectedEffect StatusEffect::DoProtected(System.Action`1<EventInfoDamageNegatedByImmunity>)' called when server was not active");
			return null;
		}
		ProtectedEffect eff = DoBasicEffectNoRegister<ProtectedEffect>();
		eff.onDamageNegated = onDamageNegated;
		RegisterBasicEffect(eff);
		return eff;
	}

	[Server]
	protected UncollidableEffect DoUncollidable()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'UncollidableEffect StatusEffect::DoUncollidable()' called when server was not active");
			return null;
		}
		return DoBasicEffect<UncollidableEffect>();
	}

	[Server]
	protected ArmorBoostEffect DoArmorBoost(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ArmorBoostEffect StatusEffect::DoArmorBoost(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<ArmorBoostEffect>(strength);
	}

	[Server]
	protected ArmorReductionEffect DoArmorReduction(float strength)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ArmorReductionEffect StatusEffect::DoArmorReduction(System.Single)' called when server was not active");
			return null;
		}
		return DoBasicEffectWithStrength<ArmorReductionEffect>(strength);
	}

	[Server]
	protected DeathInterruptEffect DoDeathInterrupt(Action<EventInfoKill> onInterrupt, int priority)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'DeathInterruptEffect StatusEffect::DoDeathInterrupt(System.Action`1<EventInfoKill>,System.Int32)' called when server was not active");
			return null;
		}
		DeathInterruptEffect eff = DoBasicEffectNoRegister<DeathInterruptEffect>();
		eff.onInterrupt = onInterrupt;
		eff.priority = priority;
		RegisterBasicEffect(eff);
		return eff;
	}

	[Server]
	protected ShieldEffect DoShield(float amount, Action<EventInfoDamageNegatedByShield> onDamageNegated = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ShieldEffect StatusEffect::DoShield(System.Single,System.Action`1<EventInfoDamageNegatedByShield>)' called when server was not active");
			return null;
		}
		amount = ProcessShieldAmount(amount, victim);
		ShieldEffect eff = DoBasicEffectNoRegister<ShieldEffect>();
		eff.amount = amount;
		eff.onDamageNegated = onDamageNegated;
		RegisterBasicEffect(eff);
		return eff;
	}

	[Server]
	protected void StopAllBasicEffects()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::StopAllBasicEffects()' called when server was not active");
			return;
		}
		foreach (BasicEffect eff in _basicEffects)
		{
			victim.Status.RemoveBasicEffect(eff);
		}
		_basicEffects.Clear();
	}

	[Server]
	protected void StopBasicEffect(BasicEffect eff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void StatusEffect::StopBasicEffect(BasicEffect)' called when server was not active");
			return;
		}
		if (victim != null)
		{
			victim.Status.RemoveBasicEffect(eff);
		}
		_basicEffects.Remove(eff);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcSetOnScreenTimer__NetworkConnectionToClient__Boolean__String__Color(NetworkConnectionToClient target, bool value, string customNameKey, Color color)
	{
		SetOnScreenTimerLocal(value, customNameKey, color);
	}

	protected static void InvokeUserCode_TpcSetOnScreenTimer__NetworkConnectionToClient__Boolean__String__Color(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetOnScreenTimer called on server.");
		}
		else
		{
			((StatusEffect)obj).UserCode_TpcSetOnScreenTimer__NetworkConnectionToClient__Boolean__String__Color((NetworkConnectionToClient)NetworkClient.connection, reader.ReadBool(), reader.ReadString(), reader.ReadColor());
		}
	}

	static StatusEffect()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(StatusEffect), "System.Void StatusEffect::TpcSetOnScreenTimer(Mirror.NetworkConnectionToClient,System.Boolean,System.String,UnityEngine.Color)", InvokeUserCode_TpcSetOnScreenTimer__NetworkConnectionToClient__Boolean__String__Color);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(Network_victim);
			writer.WriteBool(showIcon);
			writer.WriteNullableFloat(_maxDuration);
			writer.WriteNullableFloat(_remainingDuration);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_victim);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteBool(showIcon);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteNullableFloat(_maxDuration);
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteNullableFloat(_remainingDuration);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _victim, null, reader, ref ____victimNetId);
			GeneratedSyncVarDeserialize(ref showIcon, OnShowIconChanged, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _maxDuration, null, reader.ReadNullableFloat());
			GeneratedSyncVarDeserialize(ref _remainingDuration, null, reader.ReadNullableFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _victim, null, reader, ref ____victimNetId);
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref showIcon, OnShowIconChanged, reader.ReadBool());
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _maxDuration, null, reader.ReadNullableFloat());
		}
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _remainingDuration, null, reader.ReadNullableFloat());
		}
	}
}
