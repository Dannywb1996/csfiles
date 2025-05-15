using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

[LogicUpdatePriority(-300)]
public class AbilityTrigger : Actor
{
	public class ChangedConfigHandle
	{
		internal bool _isActive;

		internal Action<EventInfoAbilityInstance> _onUse;

		internal Action _onExpire;

		internal AbilityTrigger _trigger;

		public bool isActive
		{
			get
			{
				if (_isActive && _trigger != null)
				{
					return _trigger.currentConfigChangeHandle == this;
				}
				return false;
			}
		}

		public float remainingTime { get; internal set; }

		public float duration { get; internal set; }

		public int configIndex { get; internal set; }

		public int previousConfigIndex { get; internal set; }

		public bool setFillAmount { get; internal set; }

		public void Stop()
		{
			if (!isActive)
			{
				Debug.LogWarning("Tried to stop inactive ChangedConfigHandle.");
			}
			else
			{
				_trigger.RemoveCurrentConfigChange(isExpiration: false);
			}
		}
	}

	private struct ConfigSyncData
	{
		public float manaCost;

		public int maxCharges;

		public int addedCharges;

		public float cooldownTime;

		public float minimumDelay;
	}

	private struct CurrentAbilityInstanceData
	{
		public AbilityInstance instance;

		public int configIndex;

		public bool shouldLockCast;

		public bool shouldLockCooldown;

		public bool setFillAmount;
	}

	[Serializable]
	public struct PredictionSettings
	{
		public enum ModelType
		{
			Auto,
			None,
			Simple,
			SpeedAcceleration
		}

		public static readonly PredictionSettings Default = new PredictionSettings
		{
			type = ModelType.Auto
		};

		public ModelType type;

		public bool useCustomParameters;

		public float delay;

		public float initSpeed;

		public float targetSpeed;

		public float acceleration;

		public float frontDistance;

		private bool _shouldShowCustomParameters
		{
			get
			{
				if (type != ModelType.None)
				{
					return type != ModelType.Auto;
				}
				return false;
			}
		}

		private bool _shouldShowSimpleParameters
		{
			get
			{
				if (useCustomParameters)
				{
					if (type != ModelType.Simple)
					{
						return type == ModelType.SpeedAcceleration;
					}
					return true;
				}
				return false;
			}
		}

		private bool _shouldShowSpeedAccelerationParameters
		{
			get
			{
				if (useCustomParameters)
				{
					return type == ModelType.SpeedAcceleration;
				}
				return false;
			}
		}
	}

	public SafeAction ClientTriggerEvent_OnCurrentConfigCharged;

	public SafeAction ClientTriggerEvent_OnCurrentConfigCooldownReduced;

	public SafeAction<int, float> ClientTriggerEvent_OnCooldownReduced;

	public SafeAction<int, float> ClientTriggerEvent_OnCooldownReducedByRatio;

	[SyncVar(hook = "OnOwnerChanged")]
	private Entity _owner;

	public SafeAction<Entity, Entity> ClientEvent_OnOwnerChanged;

	[SyncVar(hook = "OnConfigChanged")]
	private int _currentConfigIndex;

	[SyncVar]
	private float _fillAmount;

	[NonSerialized]
	public float[] currentUnscaledCooldownTimes;

	[NonSerialized]
	public int[] currentCharges;

	[NonSerialized]
	public float[] currentMinimumDelays;

	public Action<EventInfoCast> TriggerEvent_OnCastStart;

	public Action<EventInfoCast> TriggerEvent_OnCastComplete;

	public Action<EventInfoCast> TriggerEvent_OnCastCompleteBeforePrepare;

	internal bool _isConfigDirty;

	private readonly SyncList<bool> _configCastLocks = new SyncList<bool>();

	private readonly SyncList<bool> _configCooldownLocks = new SyncList<bool>();

	private List<CurrentAbilityInstanceData> _currentInstances = new List<CurrentAbilityInstanceData>();

	private int[] _lockConfigCooldownCounters;

	public TriggerConfig[] configs = new TriggerConfig[1]
	{
		new TriggerConfig()
	};

	[CompilerGenerated]
	[SyncVar]
	private StatusEffect _003CcurrentPassiveEffect_003Ek__BackingField;

	[SyncVar]
	private bool _isCasting;

	[CompilerGenerated]
	[SyncVar]
	private int _003CabilityIndex_003Ek__BackingField;

	private Se_GenericEffectContainer _unstoppable;

	private static bool[] _selfEffectLockBuffer;

	private const float PredictionAngleLimit = 75f;

	private const float SpeedAccelerationModel_TimeStep = 0.1f;

	private const float SpeedAccelerationModel_MaxFutureTime = 2f;

	private const float SpeedAccelerationModel_AccurateSearchRangeStart = -0.15f;

	private const float SpeedAccelerationModel_AccurateSearchRangeEnd = 0.15f;

	private const float SpeedAccelerationModel_AccurateSearchTimeStep = 0.025f;

	protected NetworkBehaviourSyncVar ____ownerNetId;

	protected NetworkBehaviourSyncVar ____003CcurrentPassiveEffect_003Ek__BackingFieldNetId;

	public override bool isDestroyedOnRoomChange => owner == null;

	public Entity owner
	{
		get
		{
			return Network_owner;
		}
		set
		{
			Network_owner = value;
		}
	}

	public int currentConfigIndex
	{
		get
		{
			return _currentConfigIndex;
		}
		set
		{
			if (value >= configs.Length || value < 0)
			{
				throw new Exception($"Tried to set an invalid config index({value}) on {this}");
			}
			Network_currentConfigIndex = value;
		}
	}

	public float fillAmount
	{
		get
		{
			return _fillAmount;
		}
		set
		{
			Network_fillAmount = value;
		}
	}

	public TriggerConfig currentConfig => configs[currentConfigIndex];

	public float currentConfigMaxCooldownTime => GetMaxCooldownTime(currentConfigIndex);

	public float currentConfigUnscaledMaxCooldownTime => GetMaxCooldownTime(currentConfigIndex, scaled: false);

	public float currentConfigCooldownTime => currentUnscaledCooldownTimes[currentConfigIndex] * GetCooldownTimeMultiplier(currentConfigIndex);

	public float currentConfigUnscaledCooldownTime => currentUnscaledCooldownTimes[currentConfigIndex];

	public int currentConfigCurrentCharge => currentCharges[currentConfigIndex];

	public float currentConfigCurrentMinimumDelay => currentMinimumDelays[currentConfigIndex];

	public StatusEffect currentPassiveEffect
	{
		[CompilerGenerated]
		get
		{
			return Network_003CcurrentPassiveEffect_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CcurrentPassiveEffect_003Ek__BackingField = value;
		}
	}

	public ChangedConfigHandle currentConfigChangeHandle { get; private set; }

	public int abilityIndex
	{
		[CompilerGenerated]
		get
		{
			return _003CabilityIndex_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CabilityIndex_003Ek__BackingField = value;
		}
	} = -1;

	public Entity Network_owner
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____ownerNetId, ref _owner);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _owner, 4uL, OnOwnerChanged, ref ____ownerNetId);
		}
	}

	public int Network_currentConfigIndex
	{
		get
		{
			return _currentConfigIndex;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _currentConfigIndex, 8uL, OnConfigChanged);
		}
	}

	public float Network_fillAmount
	{
		get
		{
			return _fillAmount;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _fillAmount, 16uL, null);
		}
	}

	public StatusEffect Network_003CcurrentPassiveEffect_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003CcurrentPassiveEffect_003Ek__BackingFieldNetId, ref currentPassiveEffect);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref currentPassiveEffect, 32uL, null, ref ____003CcurrentPassiveEffect_003Ek__BackingFieldNetId);
		}
	}

	public bool Network_isCasting
	{
		get
		{
			return _isCasting;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _isCasting, 64uL, null);
		}
	}

	public int Network_003CabilityIndex_003Ek__BackingField
	{
		get
		{
			return abilityIndex;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref abilityIndex, 128uL, null);
		}
	}

	public virtual float GetCooldownTimeMultiplier(int configIndex)
	{
		if (owner == null)
		{
			return 1f;
		}
		if (configs[configIndex].canReceiveCooldownReduction)
		{
			return 100f / (100f + owner.Status.abilityHaste);
		}
		return 1f;
	}

	public float GetMaxCooldownTime(int configIndex, bool scaled = true)
	{
		float time = configs[configIndex].cooldownTime + GetCooldownTimeOffset(currentConfigIndex);
		if (scaled)
		{
			time *= GetCooldownTimeMultiplier(currentConfigIndex);
		}
		return Mathf.Max(0f, time);
	}

	public virtual float GetCooldownTimeOffset(int configIndex)
	{
		return 0f;
	}

	public virtual float GetAnimationSpeed()
	{
		return 1f;
	}

	public virtual float GetChannelDurationMultiplier()
	{
		return 1f;
	}

	public virtual float GetPostDelayDurationMultiplier()
	{
		return 1f;
	}

	[Server]
	public void UndoChangeConfig()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::UndoChangeConfig()' called when server was not active");
		}
		else
		{
			RemoveCurrentConfigChange(isExpiration: false);
		}
	}

	[Server]
	private void RemoveCurrentConfigChange(bool isExpiration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::RemoveCurrentConfigChange(System.Boolean)' called when server was not active");
			return;
		}
		if (currentConfigChangeHandle == null || !currentConfigChangeHandle.isActive)
		{
			currentConfigChangeHandle = null;
			Debug.LogWarning("Tried to expire inactive ChangedConfigHandle.", this);
			return;
		}
		ChangedConfigHandle handle = currentConfigChangeHandle;
		currentConfigChangeHandle = null;
		currentConfigIndex = handle.previousConfigIndex;
		if (handle.setFillAmount)
		{
			fillAmount = 0f;
		}
		if (isExpiration)
		{
			handle._onExpire?.Invoke();
		}
	}

	[Server]
	public ChangedConfigHandle ChangeConfigTimed(int index, float duration, Action<EventInfoAbilityInstance> onUse = null, Action onExpire = null, bool setFillAmount = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityTrigger/ChangedConfigHandle AbilityTrigger::ChangeConfigTimed(System.Int32,System.Single,System.Action`1<EventInfoAbilityInstance>,System.Action,System.Boolean)' called when server was not active");
			return null;
		}
		if (currentConfigChangeHandle != null)
		{
			RemoveCurrentConfigChange(isExpiration: true);
		}
		ChangedConfigHandle handle = new ChangedConfigHandle();
		handle._isActive = true;
		handle.remainingTime = duration;
		handle.duration = duration;
		handle._onUse = onUse;
		handle._onExpire = onExpire;
		handle._trigger = this;
		handle.previousConfigIndex = currentConfigIndex;
		handle.configIndex = index;
		handle.setFillAmount = setFillAmount;
		currentConfigIndex = index;
		currentConfigChangeHandle = handle;
		return handle;
	}

	[Server]
	public ChangedConfigHandle ChangeConfigTimedOnce(int index, float duration, Action<EventInfoAbilityInstance> onUse = null, Action onExpire = null, bool setFillAmount = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityTrigger/ChangedConfigHandle AbilityTrigger::ChangeConfigTimedOnce(System.Int32,System.Single,System.Action`1<EventInfoAbilityInstance>,System.Action,System.Boolean)' called when server was not active");
			return null;
		}
		ChangedConfigHandle changedConfigHandle = ChangeConfigTimed(index, duration, onUse, onExpire, setFillAmount);
		changedConfigHandle._onUse = (Action<EventInfoAbilityInstance>)Delegate.Combine(changedConfigHandle._onUse, (Action<EventInfoAbilityInstance>)delegate
		{
			RemoveCurrentConfigChange(isExpiration: false);
		});
		return changedConfigHandle;
	}

	[Server]
	public void ApplyCooldownReduction(float amount, bool scaled = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::ApplyCooldownReduction(System.Single,System.Boolean)' called when server was not active");
		}
		else
		{
			ApplyCooldownReductionWithMinimum(amount, 0f, scaled);
		}
	}

	[Server]
	public void ApplyCooldownReductionWithMinimum(float amount, float minThreshold, bool scaled = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::ApplyCooldownReductionWithMinimum(System.Single,System.Single,System.Boolean)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			if (!configs[i].canReceiveCooldownReduction)
			{
				continue;
			}
			InvokeCooldownReduced(i, amount);
			float currentMinThre = minThreshold;
			float currentAmount = amount;
			if (scaled)
			{
				float mult = GetCooldownTimeMultiplier(i);
				currentMinThre /= mult;
				currentAmount /= mult;
			}
			if ((currentCharges[i] == 0 && currentUnscaledCooldownTimes[i] <= currentMinThre) || currentCharges[i] >= configs[i].maxCharges)
			{
				continue;
			}
			while (currentAmount > 0f && currentCharges[i] < configs[i].maxCharges)
			{
				if (currentCharges[i] >= configs[i].maxCharges - 1)
				{
					currentUnscaledCooldownTimes[i] -= currentAmount;
					if (currentUnscaledCooldownTimes[i] < currentMinThre)
					{
						currentUnscaledCooldownTimes[i] = currentMinThre;
					}
					break;
				}
				currentUnscaledCooldownTimes[i] -= currentAmount;
				currentAmount = 0f;
				if (currentUnscaledCooldownTimes[i] < 0f)
				{
					currentAmount = 0f - currentUnscaledCooldownTimes[i];
					currentCharges[i] = Mathf.Min(currentCharges[i] + configs[i].addedCharges, configs[i].maxCharges);
					if (i == currentConfigIndex)
					{
						InvokeCurrentConfigCharged();
					}
					if (currentCharges[i] >= configs[i].maxCharges)
					{
						currentUnscaledCooldownTimes[i] = 0f;
						break;
					}
					currentUnscaledCooldownTimes[i] = GetMaxCooldownTime(i, scaled: false);
				}
			}
			if (i == currentConfigIndex)
			{
				InvokeCurrentConfigCooldownReduced();
			}
			RpcSetCooldownTime(i, currentUnscaledCooldownTimes[i]);
			RpcSetCharge(i, currentCharges[i]);
		}
	}

	[Server]
	public void ApplyCooldownReductionByRatio(float ratio)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::ApplyCooldownReductionByRatio(System.Single)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			if (!configs[i].canReceiveCooldownReduction)
			{
				continue;
			}
			float currentAmount = GetMaxCooldownTime(i, scaled: false) * ratio;
			while (currentAmount > 0f && currentCharges[i] < configs[i].maxCharges)
			{
				currentUnscaledCooldownTimes[i] -= currentAmount;
				currentAmount = 0f;
				if (currentUnscaledCooldownTimes[i] < 0f)
				{
					currentAmount = 0f - currentUnscaledCooldownTimes[i];
					currentCharges[i] = Mathf.Min(currentCharges[i] + configs[i].addedCharges, configs[i].maxCharges);
					if (i == currentConfigIndex)
					{
						InvokeCurrentConfigCharged();
					}
					if (currentCharges[i] >= configs[i].maxCharges)
					{
						currentUnscaledCooldownTimes[i] = 0f;
						break;
					}
					currentUnscaledCooldownTimes[i] = GetMaxCooldownTime(i, scaled: false);
				}
			}
			InvokeCooldownReducedByRatio(i, ratio);
			if (i == currentConfigIndex)
			{
				InvokeCurrentConfigCooldownReduced();
			}
			RpcSetCooldownTime(i, currentUnscaledCooldownTimes[i]);
			RpcSetCharge(i, currentCharges[i]);
		}
	}

	[Server]
	public void ResetCooldown()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::ResetCooldown()' called when server was not active");
		}
		else
		{
			ApplyCooldownReductionByRatio(1f);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		TriggerConfig[] array = configs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]._parent = this;
		}
		_lockConfigCooldownCounters = new int[configs.Length];
	}

	public virtual void OnCastStart(int configIndex, CastInfo info)
	{
		Network_isCasting = true;
		if (!configs[configIndex].isActive)
		{
			throw new Exception($"Tried to cast a passive ability: {this}");
		}
		TriggerConfig config = configs[configIndex];
		info.animSelectValue = UnityEngine.Random.value;
		if (config.startAnim != null)
		{
			owner.Animation.PlayAbilityAnimation(config.startAnim, GetAnimationSpeed(), info.animSelectValue);
		}
		if (config.castVoice != null)
		{
			owner.Sound.Say(config.castVoice, interruptPrevious: true);
		}
		if (config.effectOnCast != null)
		{
			DewEffect.PlayCastEffectNetworked(base.netIdentity, config.effectOnCast, info, config.castMethod.type, config.channel.duration * GetChannelDurationMultiplier());
		}
		if (config.faceForward)
		{
			OnRotateForward(configIndex, info);
		}
		OnStartChannel(configIndex, info);
		try
		{
			TriggerEvent_OnCastStart?.Invoke(new EventInfoCast
			{
				configIndex = configIndex,
				info = info,
				instance = null,
				trigger = this
			});
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected virtual void OnStartChannel(int configIndex, CastInfo info)
	{
		TriggerConfig config = configs[configIndex];
		Channel channel = ((config.castMethod.type != CastMethodType.Target) ? config.channel.CreateChannel(delegate
		{
			OnCastComplete(configIndex, info);
		}, delegate
		{
			OnCastCancel(configIndex, info);
		}, config.selfValidator) : config.channel.CreateChannel(delegate
		{
			OnCastComplete(configIndex, info);
		}, delegate
		{
			OnCastCancel(configIndex, info);
		}, config.selfValidator, info.target, config.targetValidator));
		channel.duration *= GetChannelDurationMultiplier();
		if (config.unstoppableWhileCasting)
		{
			_unstoppable = CreateBasicEffect(owner, new UnstoppableEffect(), channel.duration);
		}
		owner.Control.StartChannel(channel);
	}

	protected virtual void OnRotateForward(int configIndex, CastInfo info)
	{
		TriggerConfig config = configs[configIndex];
		switch (config.castMethod.type)
		{
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			owner.Control.StopOverrideRotation();
			owner.Control.Rotate(info.rotation, immediately: false, config.overrideRotation ? config.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.Target:
			owner.Control.StopOverrideRotation();
			if (!info.target.IsNullInactiveDeadOrKnockedOut() && info.target != info.caster)
			{
				owner.Control.RotateTowards(info.target, immediately: false, config.overrideRotation ? config.overrideRotationDuration : (-1f));
			}
			break;
		case CastMethodType.Point:
			owner.Control.StopOverrideRotation();
			owner.Control.RotateTowards(info.point, immediately: false, config.overrideRotation ? config.overrideRotationDuration : (-1f));
			break;
		case CastMethodType.None:
			break;
		}
	}

	public override void OnSerialize(NetworkWriter writer, bool initialState)
	{
		base.OnSerialize(writer, initialState);
		if (initialState)
		{
			for (int i = 0; i < configs.Length; i++)
			{
				writer.WriteFloat(currentUnscaledCooldownTimes[i]);
				writer.WriteInt(currentCharges[i]);
				writer.WriteFloat(currentMinimumDelays[i]);
				writer.WriteFloat(configs[i].manaCost);
				writer.WriteInt(configs[i].maxCharges);
				writer.WriteFloat(configs[i].minimumDelay);
			}
		}
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			currentUnscaledCooldownTimes = new float[configs.Length];
			currentCharges = new int[configs.Length];
			currentMinimumDelays = new float[configs.Length];
			for (int i = 0; i < configs.Length; i++)
			{
				currentUnscaledCooldownTimes[i] = reader.ReadFloat();
				currentCharges[i] = reader.ReadInt();
				currentMinimumDelays[i] = reader.ReadFloat();
				configs[i].manaCost = reader.ReadFloat();
				configs[i].maxCharges = reader.ReadInt();
				configs[i].minimumDelay = reader.ReadFloat();
			}
		}
	}

	protected virtual void OnCastCancel(int configIndex, CastInfo info)
	{
		Network_isCasting = false;
		OnCastCancelSetCooldownTime(configIndex, info);
		TriggerConfig config = configs[configIndex];
		if (config.effectOnCast != null)
		{
			FxStopNetworked(config.effectOnCast);
		}
		if (config.startAnim != null && owner != null)
		{
			owner.Animation.StopAbilityAnimation(config.startAnim);
		}
		if (!_unstoppable.IsNullOrInactive())
		{
			_unstoppable.Destroy();
			_unstoppable = null;
		}
	}

	protected virtual void OnCastCancelSetCooldownTime(int configIndex, CastInfo info)
	{
		if (currentUnscaledCooldownTimes[configIndex] < 1f)
		{
			SetCooldownTime(configIndex, 1f, scaled: false);
		}
	}

	protected virtual Vector3 GetInstanceSpawnPosition(int configIndex, CastInfo info)
	{
		return owner.transform.position;
	}

	protected virtual Quaternion? GetInstanceSpawnRotation(int configIndex, CastInfo info)
	{
		CastMethodType castType = configs[configIndex].castMethod.type;
		Quaternion? rot = null;
		return (castType != CastMethodType.Arrow && castType != CastMethodType.Cone) ? new Quaternion?(owner.Control.desiredRotation) : new Quaternion?(info.rotation);
	}

	public virtual AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		Network_isCasting = false;
		if (!configs[configIndex].isActive)
		{
			throw new Exception($"Tried to cast a passive ability: {this}");
		}
		OnCastCompleteSpendMana(configIndex, info);
		OnCastCompleteSetCooldownTime(configIndex, info);
		OnCastCompleteSetCharge(configIndex, info);
		OnCastCompleteSetMinimumDelay(configIndex, info);
		TriggerConfig config = configs[configIndex];
		if (config.castMethod.type == CastMethodType.Point)
		{
			float y = info.point.y;
			Vector2 flatPoint = owner.agentPosition.ToXY() + Vector2.ClampMagnitude(info.point.ToXY() - owner.agentPosition.ToXY(), config.castMethod.pointData.range);
			info.point = new Vector3(flatPoint.x, y, flatPoint.y);
		}
		if (!_unstoppable.IsNullOrInactive())
		{
			_unstoppable.Destroy();
			_unstoppable = null;
		}
		if (config.endAnim != null)
		{
			owner.Animation.PlayAbilityAnimation(config.endAnim, GetAnimationSpeed(), info.animSelectValue);
		}
		if (config.effectOnCast != null)
		{
			FxStopNetworked(config.effectOnCast);
		}
		AbilityInstance newInstance;
		if (config.spawnedInstance == null)
		{
			newInstance = CreateAbilityInstance(GetInstanceSpawnPosition(configIndex, info), GetInstanceSpawnRotation(configIndex, info), info, delegate(MockAbilityInstance ai)
			{
				EventInfoCast eventInfoCast = new EventInfoCast
				{
					configIndex = configIndex,
					info = info,
					trigger = this,
					instance = ai
				};
				OnCastCompleteBeforePrepare(eventInfoCast);
				try
				{
					TriggerEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
				try
				{
					owner.EntityEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast);
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
			});
		}
		else if (config.spawnedInstance is StatusEffect se)
		{
			Entity victim = ((config.castMethod.type != CastMethodType.Target || config.victim != TriggerConfig.StatusEffectVictimType.Target) ? info.caster : info.target);
			if (config.destroyExistingEffect && victim.Status.TryGetStatusEffect(se.GetType(), out var existing))
			{
				existing.Destroy();
			}
			newInstance = CreateStatusEffect(se, victim, info, delegate(StatusEffect ai)
			{
				EventInfoCast eventInfoCast = new EventInfoCast
				{
					configIndex = configIndex,
					info = info,
					trigger = this,
					instance = ai
				};
				OnCastCompleteBeforePrepare(eventInfoCast);
				TriggerEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast);
			});
		}
		else
		{
			newInstance = CreateAbilityInstance(config.spawnedInstance, GetInstanceSpawnPosition(configIndex, info), GetInstanceSpawnRotation(configIndex, info), info, delegate(AbilityInstance ai)
			{
				EventInfoCast eventInfoCast = new EventInfoCast
				{
					configIndex = configIndex,
					info = info,
					trigger = this,
					instance = ai
				};
				OnCastCompleteBeforePrepare(eventInfoCast);
				try
				{
					TriggerEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
				try
				{
					owner.EntityEvent_OnCastCompleteBeforePrepare?.Invoke(eventInfoCast);
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
			});
		}
		_currentInstances.Add(new CurrentAbilityInstanceData
		{
			instance = newInstance,
			configIndex = configIndex,
			shouldLockCast = config.lockCastUntilKilled,
			shouldLockCooldown = config.lockCooldownUntilKilled,
			setFillAmount = config.setFillAmount
		});
		UpdateCastAndCooldownLocks();
		if (currentConfigChangeHandle != null && currentConfigChangeHandle.isActive && currentConfigChangeHandle.configIndex == configIndex)
		{
			currentConfigChangeHandle._onUse?.Invoke(new EventInfoAbilityInstance
			{
				actor = this,
				instance = newInstance
			});
		}
		float pd = config.postDelay * GetPostDelayDurationMultiplier();
		if (pd > 0f)
		{
			owner.Control.StartDaze(pd);
		}
		EventInfoCast eventInfo = new EventInfoCast
		{
			configIndex = configIndex,
			info = info,
			trigger = this,
			instance = newInstance
		};
		try
		{
			TriggerEvent_OnCastComplete?.Invoke(eventInfo);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		owner.EntityEvent_OnCastComplete?.Invoke(eventInfo);
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnCastComplete(eventInfo);
		if (config.moveTowardsCastDirection)
		{
			Vector3? dir = null;
			switch (config.castMethod.type)
			{
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
				dir = info.forward;
				break;
			case CastMethodType.Target:
				dir = (info.target.agentPosition - owner.agentPosition).Flattened().normalized;
				break;
			case CastMethodType.Point:
				dir = (info.point - owner.agentPosition).Flattened().normalized;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case CastMethodType.None:
				break;
			}
			if (dir.HasValue && dir.Value.sqrMagnitude > 0.01f)
			{
				Vector3 agentPosition = owner.agentPosition;
				Vector3? vector = dir * 20f;
				Vector3? dest = agentPosition + vector;
				if (NavMesh.Raycast(owner.agentPosition, dest.Value, out var hit, -1))
				{
					dest = hit.position;
				}
				owner.Control.MoveToDestination(dest.Value, immediately: true);
			}
		}
		return newInstance;
	}

	public virtual void OnCastCompleteSpendMana(int configIndex, CastInfo info)
	{
		if (configs[configIndex].manaCost > 0f)
		{
			SpendMana(configs[configIndex].manaCost, owner);
		}
	}

	public virtual void OnCastCompleteSetCooldownTime(int configIndex, CastInfo info)
	{
		if (currentUnscaledCooldownTimes[configIndex] <= 0f)
		{
			SetCooldownTime(configIndex, GetMaxCooldownTime(configIndex, scaled: false), scaled: false);
		}
	}

	public virtual void OnCastCompleteSetCharge(int configIndex, CastInfo info)
	{
		SetCharge(configIndex, currentCharges[configIndex] - 1);
	}

	public virtual void OnCastCompleteSetMinimumDelay(int configIndex, CastInfo info)
	{
		SetMinimumDelay(configIndex, configs[configIndex].minimumDelay);
	}

	public virtual void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
	}

	protected virtual bool ShouldTickCooldown(int configIndex)
	{
		return !_configCooldownLocks[configIndex];
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (base.isServer && currentConfigChangeHandle != null)
		{
			currentConfigChangeHandle.remainingTime = Mathf.MoveTowards(currentConfigChangeHandle.remainingTime, 0f, dt);
			if (currentConfigChangeHandle.setFillAmount)
			{
				fillAmount = currentConfigChangeHandle.remainingTime / currentConfigChangeHandle.duration;
			}
			if (currentConfigChangeHandle.remainingTime <= 0f)
			{
				RemoveCurrentConfigChange(isExpiration: true);
			}
		}
		if (base.isServer)
		{
			for (int i = _currentInstances.Count - 1; i >= 0; i--)
			{
				if (_currentInstances[i].instance == null || !_currentInstances[i].instance.isActive)
				{
					if (_currentInstances[i].setFillAmount)
					{
						fillAmount = 0f;
					}
					_currentInstances.RemoveAt(i);
					UpdateCastAndCooldownLocks();
				}
				else if (_currentInstances[i].setFillAmount && _currentInstances[i].instance is StatusEffect se)
				{
					fillAmount = se.normalizedDuration ?? 0f;
				}
			}
		}
		if (owner != null)
		{
			for (int j = 0; j < configs.Length; j++)
			{
				if (ShouldTickCooldown(j))
				{
					float delta = dt / GetCooldownTimeMultiplier(j);
					currentUnscaledCooldownTimes[j] = Mathf.MoveTowards(currentUnscaledCooldownTimes[j], 0f, delta);
				}
				currentMinimumDelays[j] = Mathf.MoveTowards(currentMinimumDelays[j], 0f, dt);
				if (!base.isServer)
				{
					continue;
				}
				if (currentUnscaledCooldownTimes[j] == 0f && currentCharges[j] < configs[j].maxCharges)
				{
					if (currentCharges[j] < configs[j].maxCharges - 1)
					{
						SetCooldownTime(j, GetMaxCooldownTime(j, scaled: false), scaled: false);
					}
					SetCharge(j, currentCharges[j] + configs[j].addedCharges);
					if (j == currentConfigIndex)
					{
						InvokeCurrentConfigCharged();
					}
				}
				if (currentCharges[j] > configs[j].maxCharges)
				{
					SetCharge(j, configs[j].maxCharges);
				}
				if (currentCharges[j] >= configs[j].maxCharges && currentUnscaledCooldownTimes[j] > 0f)
				{
					SetCooldownTime(j, 0f, scaled: false);
				}
			}
		}
		if (base.isServer && _isConfigDirty)
		{
			_isConfigDirty = false;
			ConfigSyncData[] data = new ConfigSyncData[configs.Length];
			for (int k = 0; k < configs.Length; k++)
			{
				data[k].cooldownTime = configs[k].cooldownTime;
				data[k].manaCost = configs[k].manaCost;
				data[k].addedCharges = configs[k].addedCharges;
				data[k].maxCharges = configs[k].maxCharges;
				data[k].minimumDelay = configs[k].minimumDelay;
			}
			RpcSyncConfigs(data);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		currentUnscaledCooldownTimes = new float[configs.Length];
		currentCharges = new int[configs.Length];
		currentMinimumDelays = new float[configs.Length];
		for (int i = 0; i < configs.Length; i++)
		{
			_configCastLocks.Add(item: false);
		}
		for (int j = 0; j < configs.Length; j++)
		{
			_configCooldownLocks.Add(item: false);
		}
		for (int k = 0; k < configs.Length; k++)
		{
			if (configs[k].startCharges < 0)
			{
				currentCharges[k] = configs[k].maxCharges;
			}
			else
			{
				currentCharges[k] = Mathf.Clamp(configs[k].startCharges, 0, configs[k].maxCharges);
			}
			if (currentCharges[k] < configs[k].maxCharges)
			{
				currentUnscaledCooldownTimes[k] = GetMaxCooldownTime(k, scaled: false);
			}
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		if (currentConfigChangeHandle != null && currentConfigChangeHandle.isActive)
		{
			RemoveCurrentConfigChange(isExpiration: true);
		}
	}

	protected virtual void OnEquip(Entity newOwner)
	{
		if (base.isServer)
		{
			UpdatePassiveEffectIfNeccessary();
		}
	}

	protected virtual void OnUnequip(Entity formerOwner)
	{
		if (base.isServer && Network_003CcurrentPassiveEffect_003Ek__BackingField != null && Network_003CcurrentPassiveEffect_003Ek__BackingField.isActive)
		{
			Network_003CcurrentPassiveEffect_003Ek__BackingField.Destroy();
			Network_003CcurrentPassiveEffect_003Ek__BackingField = null;
		}
	}

	protected virtual void OnConfigChanged(int oldIndex, int newIndex)
	{
		if (base.isServer)
		{
			UpdatePassiveEffectIfNeccessary();
		}
	}

	[Server]
	internal void UpdatePassiveEffectIfNeccessary()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::UpdatePassiveEffectIfNeccessary()' called when server was not active");
		}
		else if (currentConfig.isActive || currentConfig.appliedStatusEffect == null)
		{
			if (Network_003CcurrentPassiveEffect_003Ek__BackingField != null)
			{
				Network_003CcurrentPassiveEffect_003Ek__BackingField.DestroyIfActive();
				Network_003CcurrentPassiveEffect_003Ek__BackingField = null;
			}
		}
		else if (!(Network_003CcurrentPassiveEffect_003Ek__BackingField != null) || !(currentConfig.appliedStatusEffect.GetType() == Network_003CcurrentPassiveEffect_003Ek__BackingField.GetType()) || !(Network_003CcurrentPassiveEffect_003Ek__BackingField.victim == owner) || (this is SkillTrigger strg && (!(Network_003CcurrentPassiveEffect_003Ek__BackingField != null) || Network_003CcurrentPassiveEffect_003Ek__BackingField.skillLevel != strg.level)))
		{
			if (Network_003CcurrentPassiveEffect_003Ek__BackingField != null)
			{
				Network_003CcurrentPassiveEffect_003Ek__BackingField.DestroyIfActive();
				Network_003CcurrentPassiveEffect_003Ek__BackingField = null;
			}
			Network_003CcurrentPassiveEffect_003Ek__BackingField = CreateStatusEffect(currentConfig.appliedStatusEffect, owner, new CastInfo(owner));
		}
	}

	[Server]
	public void SetCooldownTime(int configIndex, float time, bool scaled = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetCooldownTime(System.Int32,System.Single,System.Boolean)' called when server was not active");
			return;
		}
		if (scaled)
		{
			time /= GetCooldownTimeMultiplier(configIndex);
		}
		currentUnscaledCooldownTimes[configIndex] = time;
		RpcSetCooldownTime(configIndex, time);
	}

	[ClientRpc]
	internal void RpcSetCooldownTime(int configIndex, float time)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(configIndex);
		writer.WriteFloat(time);
		SendRPCInternal("System.Void AbilityTrigger::RpcSetCooldownTime(System.Int32,System.Single)", -840561602, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void SetCooldownTimeAll(float time, bool scaled = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetCooldownTimeAll(System.Single,System.Boolean)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			currentUnscaledCooldownTimes[i] = (scaled ? (time / GetCooldownTimeMultiplier(i)) : time);
		}
		RpcSetCooldownTimeAll(time);
	}

	[ClientRpc]
	private void RpcSetCooldownTimeAll(float time)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(time);
		SendRPCInternal("System.Void AbilityTrigger::RpcSetCooldownTimeAll(System.Single)", 984725360, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void SetCharge(int configIndex, int charge)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetCharge(System.Int32,System.Int32)' called when server was not active");
			return;
		}
		charge = Mathf.Clamp(charge, 0, configs[configIndex].maxCharges);
		currentCharges[configIndex] = charge;
		if (currentCharges[configIndex] < configs[configIndex].maxCharges && currentUnscaledCooldownTimes[configIndex] <= 0f)
		{
			SetCooldownTime(configIndex, GetMaxCooldownTime(configIndex, scaled: false), scaled: false);
		}
		RpcSetCharge(configIndex, charge);
	}

	[ClientRpc]
	internal void RpcSetCharge(int configIndex, int charge)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(configIndex);
		writer.WriteInt(charge);
		SendRPCInternal("System.Void AbilityTrigger::RpcSetCharge(System.Int32,System.Int32)", 802458050, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void SetChargeAll(int charge)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetChargeAll(System.Int32)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			SetCharge(i, charge);
		}
	}

	[Server]
	public void SetMinimumDelay(int configIndex, float delay)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetMinimumDelay(System.Int32,System.Single)' called when server was not active");
			return;
		}
		currentMinimumDelays[configIndex] = delay;
		RpcSetMinimumDelay(configIndex, delay);
	}

	[ClientRpc]
	private void RpcSetMinimumDelay(int configIndex, float delay)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(configIndex);
		writer.WriteFloat(delay);
		SendRPCInternal("System.Void AbilityTrigger::RpcSetMinimumDelay(System.Int32,System.Single)", -712970245, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void SetMinimumDelayAll(float delay)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::SetMinimumDelayAll(System.Single)' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			currentMinimumDelays[i] = delay;
		}
		RpcSetMinimumDelayAll(delay);
	}

	[ClientRpc]
	private void RpcSetMinimumDelayAll(float delay)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(delay);
		SendRPCInternal("System.Void AbilityTrigger::RpcSetMinimumDelayAll(System.Single)", 482718573, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcSyncConfigs(ConfigSyncData[] data)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_AbilityTrigger_002FConfigSyncData_005B_005D(writer, data);
		SendRPCInternal("System.Void AbilityTrigger::RpcSyncConfigs(AbilityTrigger/ConfigSyncData[])", -180556732, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public virtual bool CanBeCast()
	{
		if (!_configCastLocks[currentConfigIndex] && currentConfigCurrentCharge > 0 && currentConfigCurrentMinimumDelay == 0f && currentConfig.selfValidator.Evaluate(owner) && owner.currentMana >= currentConfig.manaCost && !_isCasting)
		{
			return !owner.Ability.IsAbilityCastLocked(abilityIndex);
		}
		return false;
	}

	public virtual bool CanBeReserved()
	{
		if (!_configCastLocks[currentConfigIndex] && owner != null && currentConfigCurrentCharge > 0 && currentConfigCurrentMinimumDelay == 0f && owner.currentMana >= currentConfig.manaCost)
		{
			return !owner.Ability.IsAbilityCastLocked(abilityIndex);
		}
		return false;
	}

	private void OnOwnerChanged(Entity oldOwner, Entity newOwner)
	{
		try
		{
			if (oldOwner != null)
			{
				OnUnequip(oldOwner);
			}
			if (newOwner != null)
			{
				OnEquip(newOwner);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		ClientEvent_OnOwnerChanged?.Invoke(oldOwner, newOwner);
	}

	public override string GetActorReadableName()
	{
		return string.Format("[{0}] {1}{2}{3}{4}", base.persistentNetId, base.isClient ? "" : "~", base.isActive ? "" : "!", GetType(), (owner != null) ? $" on [{owner.persistentNetId}]" : "");
	}

	[ClientRpc]
	internal void InvokeCurrentConfigCharged()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void AbilityTrigger::InvokeCurrentConfigCharged()", 632735808, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeCurrentConfigCooldownReduced()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void AbilityTrigger::InvokeCurrentConfigCooldownReduced()", -305176029, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeCooldownReduced(int configIndex, float amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(configIndex);
		writer.WriteFloat(amount);
		SendRPCInternal("System.Void AbilityTrigger::InvokeCooldownReduced(System.Int32,System.Single)", 840126176, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeCooldownReducedByRatio(int configIndex, float amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(configIndex);
		writer.WriteFloat(amount);
		SendRPCInternal("System.Void AbilityTrigger::InvokeCooldownReducedByRatio(System.Int32,System.Single)", 1447345474, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public bool IsTargetInRange(Entity target)
	{
		if (owner == null)
		{
			return false;
		}
		float effRange = currentConfig.castMethod.GetEffectiveRange();
		return Vector2.Distance(owner.agentPosition.ToXY(), target.agentPosition.ToXY()) - target.Control.outerRadius < effRange;
	}

	[Server]
	private void UpdateCastAndCooldownLocks()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::UpdateCastAndCooldownLocks()' called when server was not active");
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			_selfEffectLockBuffer[i] = false;
		}
		for (int j = 0; j < _currentInstances.Count; j++)
		{
			if (_currentInstances[j].shouldLockCast)
			{
				_selfEffectLockBuffer[_currentInstances[j].configIndex] = true;
			}
		}
		for (int k = 0; k < configs.Length; k++)
		{
			_configCastLocks[k] = _selfEffectLockBuffer[k];
		}
		for (int l = 0; l < configs.Length; l++)
		{
			_selfEffectLockBuffer[l] = _lockConfigCooldownCounters[l] > 0;
		}
		for (int m = 0; m < _currentInstances.Count; m++)
		{
			if (_currentInstances[m].shouldLockCooldown)
			{
				_selfEffectLockBuffer[_currentInstances[m].configIndex] = true;
			}
		}
		for (int n = 0; n < configs.Length; n++)
		{
			_configCooldownLocks[n] = _selfEffectLockBuffer[n];
		}
	}

	public bool IsConfigLocked(int configIndex)
	{
		return _configCastLocks[configIndex];
	}

	public void SyncCastMethodChanges(int configIndex)
	{
		RpcSetCastMethod(configIndex, configs[configIndex].castMethod);
	}

	[ClientRpc]
	private void RpcSetCastMethod(int configIndex, CastMethodData method)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(configIndex);
		writer.WriteCastMethodData(method);
		SendRPCInternal("System.Void AbilityTrigger::RpcSetCastMethod(System.Int32,CastMethodData)", -948141829, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void LockCooldown(int configIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::LockCooldown(System.Int32)' called when server was not active");
			return;
		}
		_lockConfigCooldownCounters[configIndex]++;
		UpdateCastAndCooldownLocks();
	}

	[Server]
	public void UnlockCooldown(int configIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityTrigger::UnlockCooldown(System.Int32)' called when server was not active");
			return;
		}
		_lockConfigCooldownCounters[configIndex]--;
		UpdateCastAndCooldownLocks();
	}

	public CastInfo GetPredictedCastInfoToTarget(Entity target)
	{
		if (owner is Monster { isHunter: not false } m && !m.IsAnyBoss())
		{
			return GetPredictedCastInfoToTarget(target, UnityEngine.Random.value);
		}
		return GetPredictedCastInfoToTarget(target, owner.AI.predictionStrengthOverride?.Invoke() ?? NetworkedManagerBase<GameManager>.instance.GetPredictionStrength());
	}

	public CastInfo GetPredictedCastInfoToTarget(Entity target, float strength)
	{
		PredictionSettings prediction = currentConfig.predictionSettings;
		if (prediction.type == PredictionSettings.ModelType.None)
		{
			return GetCastInfoToTarget(target);
		}
		CastMethodData castMethod = currentConfig.castMethod;
		CastMethodType type = castMethod.type;
		if (type == CastMethodType.None || type == CastMethodType.Target)
		{
			return GetCastInfoToTarget(target);
		}
		PopulatePredictionAutoParameters(ref prediction);
		float naiveAngle = CastInfo.GetAngle(target.position - owner.position);
		switch (prediction.type)
		{
		case PredictionSettings.ModelType.Simple:
			switch (castMethod.type)
			{
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
			{
				float angle2 = PredictAngle_Simple(strength, target, owner.position, prediction.delay);
				if (Mathf.Abs(naiveAngle - angle2) > 75f)
				{
					angle2 = naiveAngle;
				}
				return new CastInfo(owner, angle2);
			}
			case CastMethodType.Point:
			{
				Vector3 point2 = PredictPointClamped_Simple(strength, target, owner.position, castMethod.GetEffectiveRange(), prediction.delay);
				if (Mathf.Abs(naiveAngle - CastInfo.GetAngle(point2 - owner.position)) > 75f)
				{
					point2 = target.position;
				}
				return new CastInfo(owner, point2);
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		case PredictionSettings.ModelType.SpeedAcceleration:
			switch (castMethod.type)
			{
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
			{
				float angle = PredictAngle_SpeedAcceleration(strength, target, owner.position, prediction.delay, prediction.frontDistance, prediction.initSpeed, prediction.targetSpeed, prediction.acceleration);
				if (Mathf.Abs(naiveAngle - angle) > 75f)
				{
					angle = naiveAngle;
				}
				return new CastInfo(owner, angle);
			}
			case CastMethodType.Point:
			{
				Vector3 point = PredictPointClamped_SpeedAcceleration(strength, target, owner.position, castMethod.GetEffectiveRange(), prediction.delay, prediction.frontDistance, prediction.initSpeed, prediction.targetSpeed, prediction.acceleration);
				if (Mathf.Abs(naiveAngle - CastInfo.GetAngle(point - owner.position)) > 75f)
				{
					point = target.position;
				}
				return new CastInfo(owner, point);
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public CastInfo GetCastInfoToTarget(Entity target)
	{
		switch (currentConfig.castMethod.type)
		{
		case CastMethodType.None:
			return new CastInfo(owner);
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			return new CastInfo(owner, CastInfo.GetAngle(target.position - owner.position));
		case CastMethodType.Target:
			return new CastInfo(owner, target);
		case CastMethodType.Point:
			return new CastInfo(owner, target.position);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void PopulatePredictionAutoParameters(ref PredictionSettings prediction)
	{
		if (prediction.type == PredictionSettings.ModelType.Auto)
		{
			prediction.useCustomParameters = false;
			prediction.type = ((currentConfig.spawnedInstance is StandardProjectile) ? PredictionSettings.ModelType.SpeedAcceleration : PredictionSettings.ModelType.Simple);
		}
		if (!prediction.useCustomParameters)
		{
			if (currentConfig.spawnedInstance is StandardProjectile proj)
			{
				prediction.delay = currentConfig.channel.duration;
				prediction.frontDistance = proj.startInFrontDistance;
				prediction.initSpeed = proj._initialSpeed;
				prediction.targetSpeed = proj._targetSpeed;
				prediction.acceleration = proj._acceleration;
			}
			else if (currentConfig.spawnedInstance is InstantDamageInstance inst)
			{
				prediction.delay = currentConfig.channel.duration + inst.damageDelay;
			}
			else
			{
				prediction.delay = currentConfig.channel.duration;
			}
		}
	}

	public static Vector3 PredictPoint_Simple(float strength, Entity target, float delay)
	{
		return Dew.GetPositionOnGround(target.position + target.AI.estimatedVelocity * (delay * strength));
	}

	public static Vector3 PredictPointClamped_Simple(float strength, Entity target, Vector3 startPos, float range, float delay)
	{
		Vector3 delta = PredictPoint_Simple(strength, target, delay) - startPos;
		delta = Vector3.ClampMagnitude(delta, range);
		return startPos + delta;
	}

	public static float PredictAngle_Simple(float strength, Entity target, Vector3 startPos, float delay)
	{
		return CastInfo.GetAngle(PredictPoint_Simple(strength, target, delay) - startPos);
	}

	public static float PredictTime_SpeedAcceleration(Entity target, Vector3 startPos, float delay, float frontDistance, float initSpeed, float targetSpeed, float acceleration)
	{
		Vector2 posTarget = target.position.ToXY();
		Vector2 velTarget = target.AI.estimatedVelocity.ToXY();
		float thresholdPt = ((acceleration == 0f) ? float.PositiveInfinity : ((targetSpeed - initSpeed) / acceleration));
		float bestDiff = float.PositiveInfinity;
		float bestT = -1f;
		for (float t = delay; t < 2f + delay; t += 0.1f)
		{
			float diff = GetDiff(t);
			if (diff < bestDiff)
			{
				bestDiff = diff;
				bestT = t;
			}
		}
		float searchStart = bestT + -0.15f;
		float searchEnd = bestT + 0.15f;
		for (float t2 = searchStart; t2 < searchEnd; t2 += 0.025f)
		{
			float diff2 = GetDiff(t2);
			if (diff2 < bestDiff)
			{
				bestDiff = diff2;
				bestT = t2;
			}
		}
		return bestT;
		float GetDiff(float num)
		{
			float distToTargetSqr = Pow2(velTarget.x * num + posTarget.x - startPos.x) + Pow2(velTarget.y * num + posTarget.y - startPos.y);
			float distToProjSqr = frontDistance;
			float pt = num - delay;
			distToProjSqr = ((!(pt <= thresholdPt)) ? (distToProjSqr + Pow2(targetSpeed * pt + ((0f - targetSpeed) * targetSpeed - initSpeed * initSpeed + 2f * initSpeed * targetSpeed) / (2f * acceleration))) : (distToProjSqr + Pow2(initSpeed * pt + 0.5f * acceleration * pt * pt)));
			return Mathf.Abs(distToProjSqr - distToTargetSqr);
		}
	}

	public static Vector3 PredictPoint_SpeedAcceleration(float strength, Entity target, Vector3 startPos, float delay, float frontDistance, float initSpeed, float targetSpeed, float acceleration)
	{
		float bestT = PredictTime_SpeedAcceleration(target, startPos, delay, frontDistance, initSpeed, targetSpeed, acceleration);
		return PredictPoint_Simple(strength, target, bestT);
	}

	public static Vector3 PredictPointClamped_SpeedAcceleration(float strength, Entity target, Vector3 startPos, float range, float delay, float frontDistance, float initSpeed, float targetSpeed, float acceleration)
	{
		float bestT = PredictTime_SpeedAcceleration(target, startPos, delay, frontDistance, initSpeed, targetSpeed, acceleration);
		return PredictPointClamped_Simple(strength, target, startPos, range, bestT);
	}

	public static float PredictAngle_SpeedAcceleration(float strength, Entity target, Vector3 startPos, float delay, float frontDistance, float initSpeed, float targetSpeed, float acceleration)
	{
		float bestT = PredictTime_SpeedAcceleration(target, startPos, delay, frontDistance, initSpeed, targetSpeed, acceleration);
		return PredictAngle_Simple(strength, target, startPos, bestT);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float Pow2(float a)
	{
		return a * a;
	}

	public AbilityTrigger()
	{
		InitSyncObject(_configCastLocks);
		InitSyncObject(_configCooldownLocks);
	}

	static AbilityTrigger()
	{
		_selfEffectLockBuffer = new bool[32];
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetCooldownTime(System.Int32,System.Single)", InvokeUserCode_RpcSetCooldownTime__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetCooldownTimeAll(System.Single)", InvokeUserCode_RpcSetCooldownTimeAll__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetCharge(System.Int32,System.Int32)", InvokeUserCode_RpcSetCharge__Int32__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetMinimumDelay(System.Int32,System.Single)", InvokeUserCode_RpcSetMinimumDelay__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetMinimumDelayAll(System.Single)", InvokeUserCode_RpcSetMinimumDelayAll__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSyncConfigs(AbilityTrigger/ConfigSyncData[])", InvokeUserCode_RpcSyncConfigs__ConfigSyncData_005B_005D);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::InvokeCurrentConfigCharged()", InvokeUserCode_InvokeCurrentConfigCharged);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::InvokeCurrentConfigCooldownReduced()", InvokeUserCode_InvokeCurrentConfigCooldownReduced);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::InvokeCooldownReduced(System.Int32,System.Single)", InvokeUserCode_InvokeCooldownReduced__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::InvokeCooldownReducedByRatio(System.Int32,System.Single)", InvokeUserCode_InvokeCooldownReducedByRatio__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(AbilityTrigger), "System.Void AbilityTrigger::RpcSetCastMethod(System.Int32,CastMethodData)", InvokeUserCode_RpcSetCastMethod__Int32__CastMethodData);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSetCooldownTime__Int32__Single(int configIndex, float time)
	{
		if (!NetworkServer.active)
		{
			currentUnscaledCooldownTimes[configIndex] = time;
		}
	}

	protected static void InvokeUserCode_RpcSetCooldownTime__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCooldownTime called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_RpcSetCooldownTime__Int32__Single(reader.ReadInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_RpcSetCooldownTimeAll__Single(float time)
	{
		if (!NetworkServer.active)
		{
			for (int i = 0; i < configs.Length; i++)
			{
				currentUnscaledCooldownTimes[i] = time;
			}
		}
	}

	protected static void InvokeUserCode_RpcSetCooldownTimeAll__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCooldownTimeAll called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_RpcSetCooldownTimeAll__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_RpcSetCharge__Int32__Int32(int configIndex, int charge)
	{
		if (!NetworkServer.active)
		{
			currentCharges[configIndex] = charge;
		}
	}

	protected static void InvokeUserCode_RpcSetCharge__Int32__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCharge called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_RpcSetCharge__Int32__Int32(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcSetMinimumDelay__Int32__Single(int configIndex, float delay)
	{
		if (!NetworkServer.active)
		{
			currentMinimumDelays[configIndex] = delay;
		}
	}

	protected static void InvokeUserCode_RpcSetMinimumDelay__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMinimumDelay called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_RpcSetMinimumDelay__Int32__Single(reader.ReadInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_RpcSetMinimumDelayAll__Single(float delay)
	{
		if (!NetworkServer.active)
		{
			for (int i = 0; i < configs.Length; i++)
			{
				currentMinimumDelays[i] = delay;
			}
		}
	}

	protected static void InvokeUserCode_RpcSetMinimumDelayAll__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMinimumDelayAll called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_RpcSetMinimumDelayAll__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_RpcSyncConfigs__ConfigSyncData_005B_005D(ConfigSyncData[] data)
	{
		if (data.Length != configs.Length)
		{
			throw new Exception($"{this} configuration sync failed; data length ({data.Length}) different from expected local config length ({configs.Length}), number of configs should never be edited on runtime");
		}
		for (int i = 0; i < data.Length; i++)
		{
			configs[i]._cooldownTime = data[i].cooldownTime;
			configs[i]._manaCost = data[i].manaCost;
			configs[i]._addedCharges = data[i].addedCharges;
			configs[i]._maxCharges = data[i].maxCharges;
			configs[i]._minimumDelay = data[i].minimumDelay;
		}
	}

	protected static void InvokeUserCode_RpcSyncConfigs__ConfigSyncData_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncConfigs called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_RpcSyncConfigs__ConfigSyncData_005B_005D(GeneratedNetworkCode._Read_AbilityTrigger_002FConfigSyncData_005B_005D(reader));
		}
	}

	protected void UserCode_InvokeCurrentConfigCharged()
	{
		ClientTriggerEvent_OnCurrentConfigCharged?.Invoke();
	}

	protected static void InvokeUserCode_InvokeCurrentConfigCharged(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeCurrentConfigCharged called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_InvokeCurrentConfigCharged();
		}
	}

	protected void UserCode_InvokeCurrentConfigCooldownReduced()
	{
		ClientTriggerEvent_OnCurrentConfigCooldownReduced?.Invoke();
	}

	protected static void InvokeUserCode_InvokeCurrentConfigCooldownReduced(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeCurrentConfigCooldownReduced called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_InvokeCurrentConfigCooldownReduced();
		}
	}

	protected void UserCode_InvokeCooldownReduced__Int32__Single(int configIndex, float amount)
	{
		ClientTriggerEvent_OnCooldownReduced?.Invoke(configIndex, amount);
	}

	protected static void InvokeUserCode_InvokeCooldownReduced__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeCooldownReduced called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_InvokeCooldownReduced__Int32__Single(reader.ReadInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_InvokeCooldownReducedByRatio__Int32__Single(int configIndex, float amount)
	{
		ClientTriggerEvent_OnCooldownReducedByRatio?.Invoke(configIndex, amount);
	}

	protected static void InvokeUserCode_InvokeCooldownReducedByRatio__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeCooldownReducedByRatio called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_InvokeCooldownReducedByRatio__Int32__Single(reader.ReadInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_RpcSetCastMethod__Int32__CastMethodData(int configIndex, CastMethodData method)
	{
		if (!base.isServer)
		{
			configs[configIndex].castMethod = method;
		}
	}

	protected static void InvokeUserCode_RpcSetCastMethod__Int32__CastMethodData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCastMethod called on server.");
		}
		else
		{
			((AbilityTrigger)obj).UserCode_RpcSetCastMethod__Int32__CastMethodData(reader.ReadInt(), reader.ReadWriteCastMethodData());
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(Network_owner);
			writer.WriteInt(_currentConfigIndex);
			writer.WriteFloat(_fillAmount);
			writer.WriteNetworkBehaviour(Network_003CcurrentPassiveEffect_003Ek__BackingField);
			writer.WriteBool(_isCasting);
			writer.WriteInt(abilityIndex);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_owner);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteInt(_currentConfigIndex);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteFloat(_fillAmount);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003CcurrentPassiveEffect_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteBool(_isCasting);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteInt(abilityIndex);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _owner, OnOwnerChanged, reader, ref ____ownerNetId);
			GeneratedSyncVarDeserialize(ref _currentConfigIndex, OnConfigChanged, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref _fillAmount, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref currentPassiveEffect, null, reader, ref ____003CcurrentPassiveEffect_003Ek__BackingFieldNetId);
			GeneratedSyncVarDeserialize(ref _isCasting, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref abilityIndex, null, reader.ReadInt());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _owner, OnOwnerChanged, reader, ref ____ownerNetId);
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _currentConfigIndex, OnConfigChanged, reader.ReadInt());
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _fillAmount, null, reader.ReadFloat());
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref currentPassiveEffect, null, reader, ref ____003CcurrentPassiveEffect_003Ek__BackingFieldNetId);
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _isCasting, null, reader.ReadBool());
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref abilityIndex, null, reader.ReadInt());
		}
	}
}
