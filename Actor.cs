using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[SelectionBase]
[DewResourceLink(ResourceLinkBy.Type)]
public class Actor : DewNetworkBehaviour, ICleanup, ICustomDestroyRoutine
{
	private class Ad_ExcludedFromRoomSave
	{
	}

	private float _creationTime;

	private byte _destroyLock;

	private float _destroyLockUpdateTime;

	private uint _cachedNetId;

	private const int AncestorDepthLimit = 100;

	private ReactionChain _chain;

	[SyncVar]
	private Actor _parentActor;

	private List<Actor> _children = new List<Actor>();

	private const float DestroyLockTimeout = 60f;

	[SyncVar(hook = "OnIsActiveChanged")]
	private bool _isActive;

	private bool _didCallCreate;

	private bool _didCallDestroy;

	public static Type[] KilledByUnstoppableEffects = new Type[6]
	{
		typeof(SlowEffect),
		typeof(CrippleEffect),
		typeof(RootEffect),
		typeof(SilenceEffect),
		typeof(StunEffect),
		typeof(BlindEffect)
	};

	public static Type[] ScaleDurationByTenacityEffects = new Type[4]
	{
		typeof(RootEffect),
		typeof(SilenceEffect),
		typeof(StunEffect),
		typeof(BlindEffect)
	};

	private Dictionary<Type, object> _dataByType = new Dictionary<Type, object>();

	private List<object> _data = new List<object>();

	public SafeAction<EventInfoAbilityInstance> ActorEvent_OnAbilityInstanceCreated;

	public SafeAction<EventInfoAbilityInstance> ActorEvent_OnAbilityInstanceBeforePrepare;

	public SafeAction<EventInfoDamage> ActorEvent_OnDealDamage;

	public SafeAction<EventInfoHeal> ActorEvent_OnDoHeal;

	public SafeAction<EventInfoHeal> ActorEvent_OnDoManaHeal;

	public SafeAction<EventInfoSpentMana> ActorEvent_OnSpendMana;

	public SafeAction<EventInfoKill> ActorEvent_OnKill;

	public SafeAction<EventInfoAttackHit> ActorEvent_OnAttackHit;

	public SafeAction<EventInfoAttackEffect> ActorEvent_OnAttackEffectTriggered;

	public SafeAction<EventInfoSummon> ActorEvent_OnSummon;

	public SafeAction<EventInfoApplyElemental> ActorEvent_OnApplyElemental;

	public SafeAction<Actor> ClientActorEvent_OnDestroyed;

	public SafeAction<Actor> ClientActorEvent_OnCreate;

	private bool _actorParentEventRegistered;

	public DataProcessors<DamageData> dealtDamageProcessor = new DataProcessors<DamageData>();

	public DataProcessors<HealData> dealtHealProcessor = new DataProcessors<HealData>();

	public DataProcessors<HealData> dealtManaHealProcessor = new DataProcessors<HealData>();

	public DataProcessors<HealData> dealtShieldProcessor = new DataProcessors<HealData>();

	protected NetworkBehaviourSyncVar ____parentActorNetId;

	public virtual bool isDestroyedOnRoomChange => true;

	public float creationTime => _creationTime;

	public uint persistentNetId
	{
		get
		{
			if (base.netId == 0)
			{
				return _cachedNetId;
			}
			_cachedNetId = base.netId;
			return _cachedNetId;
		}
	}

	public bool isDestroyLocked => _destroyLock > 0;

	public Actor parentActor
	{
		get
		{
			return Network_parentActor;
		}
		set
		{
			if ((object)Network_parentActor != null)
			{
				Network_parentActor._children.Remove(this);
			}
			Network_parentActor = value;
			if (value != null)
			{
				value._children.Add(this);
			}
		}
	}

	public Vector3 position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public Quaternion rotation
	{
		get
		{
			return base.transform.rotation;
		}
		set
		{
			base.transform.rotation = value;
		}
	}

	public ReactionChain chainTempName => _chain;

	public IReadOnlyList<Actor> children => _children;

	bool ICleanup.canDestroy
	{
		get
		{
			if (_children.Count > 0)
			{
				return false;
			}
			if (_destroyLock <= 0)
			{
				return true;
			}
			if (Time.time - _destroyLockUpdateTime <= 60f)
			{
				return false;
			}
			Debug.LogWarning("'" + base.name + "' destroy lock timed out! Ignoring the lock...");
			return true;
		}
	}

	public bool isActive => _isActive;

	public bool isDestroyed => !_isActive;

	public IEnumerable<Actor> ancestors => GetEnumerableAncestors();

	public Entity firstEntity => FindFirstOfType<Entity>();

	public AbilityTrigger firstTrigger => FindFirstOfType<AbilityTrigger>();

	public Actor Network_parentActor
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____parentActorNetId, ref _parentActor);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _parentActor, 1uL, null, ref ____parentActorNetId);
		}
	}

	public bool Network_isActive
	{
		get
		{
			return _isActive;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _isActive, 2uL, OnIsActiveChanged);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		RegisterParentEvent();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		Network_isActive = true;
	}

	public override void OnStart()
	{
		base.OnStart();
		if (!(NetworkedManagerBase<ActorManager>.instance == null))
		{
			_creationTime = Time.time;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		InvokeOnCreateIfDidnt();
		if (!isActive)
		{
			InvokeOnDestroyActorIfDidnt();
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		parentActor = null;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (isActive)
		{
			try
			{
				ActiveLogicUpdate(dt);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
		if (ActorManager.enableUsefulActorName)
		{
			base.name = GetActorReadableName();
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (isActive)
		{
			try
			{
				ActiveFrameUpdate();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
	}

	protected virtual void ActiveFrameUpdate()
	{
	}

	protected virtual void ActiveLogicUpdate(float dt)
	{
	}

	public virtual string GetActorReadableName()
	{
		return string.Format("[{0}{1}] {2}{3}{4}", persistentNetId, (parentActor != null) ? $" from {parentActor.persistentNetId}" : "", base.isClient ? "" : "~", isActive ? "" : "!", GetType());
	}

	public override string ToString()
	{
		if (!(base.netIdentity != null))
		{
			return base.ToString();
		}
		return GetActorReadableName();
	}

	public void CustomDestroyRoutine()
	{
		throw new NotImplementedException();
	}

	void ICleanup.OnCleanup()
	{
		Network_isActive = false;
	}

	private void OnIsActiveChanged(bool oldVal, bool newVal)
	{
		if (!newVal)
		{
			InvokeOnCreateIfDidnt();
			InvokeOnDestroyActorIfDidnt();
		}
	}

	[Server]
	internal void PrepareAndSpawn()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::PrepareAndSpawn()' called when server was not active");
			return;
		}
		try
		{
			OnPrepare();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		NetworkServer.Spawn(base.gameObject, GetNetworkAuthorityConnection());
	}

	protected virtual void OnPrepare()
	{
	}

	protected virtual void OnCreate()
	{
		try
		{
			ClientActorEvent_OnCreate?.Invoke(this);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected virtual void OnDestroyActor()
	{
		try
		{
			ClientActorEvent_OnDestroyed?.Invoke(this);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		if (NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.RemoveActor(this);
			NetworkedManagerBase<ActorManager>.instance.AddActorBeingDestroyed(this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.RemoveActor(this);
			NetworkedManagerBase<ActorManager>.instance.RemoveActorBeingDestroyed(this);
		}
	}

	private void InvokeOnCreateIfDidnt()
	{
		if (_didCallCreate)
		{
			return;
		}
		_didCallCreate = true;
		NetworkedManagerBase<ActorManager>.instance.AddActor(this);
		try
		{
			OnCreate();
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception occured on " + base.transform.GetScenePath() + "::OnCreate");
			Debug.LogException(exception, this);
		}
	}

	private void InvokeOnDestroyActorIfDidnt()
	{
		if (_didCallDestroy)
		{
			return;
		}
		_didCallDestroy = true;
		try
		{
			OnDestroyActor();
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception occured on " + base.transform.GetScenePath() + "::OnDestroyActor");
			Debug.LogException(exception, this);
		}
	}

	public virtual bool ShouldBeSaved()
	{
		return false;
	}

	public virtual void OnSaveActor(Dictionary<string, object> data)
	{
		data["pos"] = position;
		data["rot"] = rotation;
	}

	public virtual Actor OnLoadCreateActor(Dictionary<string, object> data, Actor parent)
	{
		return Dew.CreateActor(this, (Vector3)data["pos"], (Quaternion)data["rot"], parent);
	}

	public virtual void OnLoadActor(Dictionary<string, object> data)
	{
	}

	void ICustomDestroyRoutine.CustomDestroyRoutine()
	{
		base.gameObject.AddComponent<EffectAutoDestroy>();
	}

	protected virtual NetworkConnectionToClient GetNetworkAuthorityConnection()
	{
		return null;
	}

	[Server]
	public void DoHeal(HealData heal, Entity target, ReactionChain chain = default(ReactionChain))
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::DoHeal(HealData,Entity,ReactionChain)' called when server was not active");
			return;
		}
		if (heal.originalAmount < 0f)
		{
			Debug.LogWarning($"Heal by {GetActorReadableName()} original amount is {heal.originalAmount}");
			return;
		}
		ProcessDealtHeal(ref heal, target);
		target.ProcessReceivedHeal(ref heal, this);
		FinalHealData finalHeal = new FinalHealData(heal, target.Status.missingHealth);
		if (!(finalHeal.amount + finalHeal.discardedAmount < 0.5f))
		{
			if (finalHeal.amount + finalHeal.discardedAmount < 0f)
			{
				Debug.LogWarning($"Heal by {GetActorReadableName()} final amount is {finalHeal.amount + finalHeal.discardedAmount}");
				return;
			}
			target.Status.currentHealth += finalHeal.amount;
			EventInfoHeal info = new EventInfoHeal
			{
				actor = this,
				target = target,
				amount = finalHeal.amount,
				discardedAmount = finalHeal.discardedAmount,
				isCrit = finalHeal.isCrit,
				heal = finalHeal,
				chain = chain,
				canMerge = heal.canMerge
			};
			ActorEvent_OnDoHeal?.Invoke(info);
			target.EntityEvent_OnTakeHeal?.Invoke(info);
		}
	}

	[Server]
	public void DoManaHeal(HealData heal, Entity target, ReactionChain chain = default(ReactionChain))
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::DoManaHeal(HealData,Entity,ReactionChain)' called when server was not active");
			return;
		}
		ProcessDealtManaHeal(ref heal, target);
		target.ProcessReceivedManaHeal(ref heal, this);
		FinalHealData finalHeal = new FinalHealData(heal, target.Status.missingMana);
		if (!(finalHeal.amount < 0.01f))
		{
			target.Status.currentMana += finalHeal.amount;
			EventInfoHeal info = new EventInfoHeal
			{
				actor = this,
				target = target,
				amount = finalHeal.amount,
				discardedAmount = finalHeal.discardedAmount,
				heal = finalHeal,
				isCrit = finalHeal.isCrit,
				chain = chain
			};
			ActorEvent_OnDoManaHeal?.Invoke(info);
			target.EntityEvent_OnTakeManaHeal?.Invoke(info);
		}
	}

	public DamageData DefaultDamage(float amount, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Default, amount, procCoefficient);
	}

	public DamageData PhysicalDamage(float amount, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical, amount, procCoefficient);
	}

	public DamageData MagicDamage(float amount, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Magic, amount, procCoefficient);
	}

	public DamageData PureDamage(float amount, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Pure, amount, procCoefficient);
	}

	public DamageData CreateDamage(DamageData.SourceType type, float amount, float procCoefficient = 1f)
	{
		return new DamageData(type, amount, procCoefficient).SetActor(this);
	}

	public HealData Heal(float amount)
	{
		return new HealData(amount).SetActor(this);
	}

	[Server]
	public void DealDamage(DamageData damage, Entity target, ReactionChain chain = default(ReactionChain))
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::DealDamage(DamageData,Entity,ReactionChain)' called when server was not active");
		}
		else
		{
			if (target == null)
			{
				return;
			}
			if (target.IsNullInactiveDeadOrKnockedOut())
			{
				if (damage.attackEffectStrength > 0.001f)
				{
					TriggerAttackEffects(target, damage.attackEffectStrength, damage.attackEffectType, chain);
				}
				return;
			}
			if (damage.originalAmount < 0f)
			{
				Debug.LogWarning($"Damage by {GetActorReadableName()} original amount is {damage.originalAmount}");
				return;
			}
			ProcessDealtDamage(ref damage, target);
			target.ProcessReceivedDamage(ref damage, this);
			float armor = target.Status.totalArmor;
			target.Status.UpdateStatusInfo();
			FinalDamageData finalDamage = new FinalDamageData(damage, armor, target);
			if (finalDamage.amount + finalDamage.discardedAmount < 0f)
			{
				Debug.LogWarning($"Damage by {GetActorReadableName()} final amount is {finalDamage.amount + finalDamage.discardedAmount}");
			}
			else
			{
				if (finalDamage.amount + finalDamage.discardedAmount <= 0.0001f)
				{
					return;
				}
				if (target is PropEntity { takeOneDamageOnHit: not false })
				{
					finalDamage.amount = 1.0001f;
					finalDamage.discardedAmount = 0f;
				}
				if (!damage.HasAttr(DamageAttribute.IgnoreDamageImmunity))
				{
					if (target.Status.hasDamageImmunity)
					{
						EventInfoDamageNegatedByImmunity negation = new EventInfoDamageNegatedByImmunity
						{
							actor = this,
							effect = target.Status._lastDamagePreventer,
							data = finalDamage,
							victim = target
						};
						target.EntityEvent_OnDamageNegated?.Invoke(negation);
						return;
					}
					if (damage.isBlockedByImmunity)
					{
						EventInfoDamageNegatedByImmunity negation2 = new EventInfoDamageNegatedByImmunity
						{
							actor = this,
							effect = null,
							data = finalDamage,
							victim = target
						};
						target.EntityEvent_OnDamageNegated?.Invoke(negation2);
						return;
					}
				}
				target.SetLastAttacker(this);
				if (finalDamage.attackEffectStrength > 0.001f)
				{
					TriggerAttackEffects(target, finalDamage.attackEffectStrength, finalDamage.attackEffectType, chain);
				}
				float healthDmgAmount = finalDamage.amount;
				if (!damage.HasAttr(DamageAttribute.IgnoreShield) && target.Status.currentShield > 0f)
				{
					ListReturnHandle<BasicEffect> handle;
					List<BasicEffect> be = DewPool.GetList(out handle);
					foreach (BasicEffect b in target.Status._basicEffects)
					{
						be.Add(b);
					}
					for (int i = be.Count - 1; i >= 0; i--)
					{
						if (be[i].isAlive && be[i] is ShieldEffect { amount: 0 } shield)
						{
							if (shield.amount > healthDmgAmount)
							{
								shield.amount -= healthDmgAmount;
								try
								{
									shield.onDamageNegated?.Invoke(new EventInfoDamageNegatedByShield
									{
										actor = this,
										negatedAmount = healthDmgAmount,
										shield = shield,
										victim = target,
										damage = finalDamage
									});
								}
								catch (Exception exception)
								{
									Debug.LogException(exception);
								}
								healthDmgAmount = 0f;
								break;
							}
							float negatedAmount = shield.amount;
							healthDmgAmount -= shield.amount;
							shield.amount = 0f;
							try
							{
								shield.onDamageNegated?.Invoke(new EventInfoDamageNegatedByShield
								{
									actor = this,
									negatedAmount = negatedAmount,
									shield = shield,
									victim = target,
									damage = finalDamage
								});
							}
							catch (Exception exception2)
							{
								Debug.LogException(exception2);
							}
						}
					}
					handle.Return();
				}
				target.Status.currentHealth -= Mathf.Max(healthDmgAmount, 0f);
				if (finalDamage.elemental.HasValue)
				{
					if (finalDamage.overrideElementalStacks.HasValue)
					{
						ApplyElemental(finalDamage.elemental.Value, target, finalDamage.overrideElementalStacks.Value);
					}
					else
					{
						switch (finalDamage.elemental.Value)
						{
						case ElementalType.Fire:
						{
							Se_Elm_Fire fire;
							if (target.Status.fireStack == 0 || UnityEngine.Random.value < finalDamage.procCoefficient)
							{
								ApplyElemental(ElementalType.Fire, target);
							}
							else if (target.Status.TryGetStatusEffect<Se_Elm_Fire>(out fire))
							{
								fire.ResetDecayTimer();
							}
							break;
						}
						case ElementalType.Cold:
						case ElementalType.Light:
						case ElementalType.Dark:
							ApplyElemental(finalDamage.elemental.Value, target);
							break;
						}
					}
				}
				EventInfoDamage info = new EventInfoDamage
				{
					actor = this,
					victim = target,
					damage = finalDamage,
					chain = chain
				};
				try
				{
					ActorEvent_OnDealDamage?.Invoke(info);
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
				try
				{
					target.EntityEvent_OnTakeDamage?.Invoke(info);
				}
				catch (Exception exception4)
				{
					Debug.LogException(exception4);
				}
				if (target.Status.currentHealth <= 0f)
				{
					target.Kill();
				}
			}
		}
	}

	[Server]
	public void DoBasicAttackHit(Entity target, bool isCriticalHit, bool isMain, float damage, float attackEffect)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::DoBasicAttackHit(Entity,System.Boolean,System.Boolean,System.Single,System.Single)' called when server was not active");
			return;
		}
		Entity from = firstEntity;
		if (from == null)
		{
			Debug.LogError($"Tried to DoAttackHit with no parent entity: {this}");
			return;
		}
		AttackTrigger atkTrg = from.Ability.originalAttackAbility;
		if (atkTrg == null)
		{
			Debug.LogError($"Tried to DoAttackHit with entity without attack ability: {this}");
			return;
		}
		if (from.Status.hasBlind)
		{
			EventInfoAttackMissed miss = new EventInfoAttackMissed
			{
				actor = this,
				attacker = from,
				victim = target,
				isCrit = isCriticalHit
			};
			from.EntityEvent_OnAttackMissed?.Invoke(miss);
			target.EntityEvent_OnAttackDodged?.Invoke(miss);
			return;
		}
		DamageData dmg = new DamageData(DamageData.SourceType.Physical, atkTrg.factor, from, 0, 1f);
		dmg.SetOriginPosition(from.position);
		if (isCriticalHit)
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
			dmg.ApplyAmplification(from.Status.critAmp);
		}
		EventInfoAttackHit info = new EventInfoAttackHit
		{
			actor = this,
			isCrit = isCriticalHit,
			attacker = from,
			victim = target,
			strength = damage
		};
		ActorEvent_OnAttackHit?.Invoke(info);
		from.EntityEvent_OnAttackHit?.Invoke(info);
		target.EntityEvent_OnAttackTaken?.Invoke(info);
		if (attackEffect > 0.001f)
		{
			dmg.DoAttackEffect((!isMain) ? AttackEffectType.BasicAttackSub : AttackEffectType.BasicAttackMain, attackEffect);
		}
		dmg.ApplyRawMultiplier(damage);
		DealDamage(dmg, target);
	}

	[Server]
	public void TriggerAttackEffects(Entity target, float strength, AttackEffectType type, ReactionChain chain = default(ReactionChain))
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::TriggerAttackEffects(Entity,System.Single,AttackEffectType,ReactionChain)' called when server was not active");
			return;
		}
		Entity from = firstEntity;
		if (from == null)
		{
			Debug.LogError($"Tried to DoAttackHit with no parent entity: {this}");
			return;
		}
		EventInfoAttackEffect info = new EventInfoAttackEffect
		{
			actor = this,
			attacker = from,
			victim = target,
			type = type,
			strength = strength,
			chain = chain
		};
		ActorEvent_OnAttackEffectTriggered?.Invoke(info);
		from.EntityEvent_OnAttackEffectTriggered?.Invoke(info);
	}

	[Server]
	public void RefundMana(float amount, Entity target)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::RefundMana(System.Single,Entity)' called when server was not active");
		}
		else
		{
			target.Status.currentMana = Mathf.MoveTowards(target.currentMana, target.Status.maxMana, amount);
		}
	}

	[Server]
	public void SpendMana(float amount, Entity target)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::SpendMana(System.Single,Entity)' called when server was not active");
			return;
		}
		target.Status.currentMana = Mathf.MoveTowards(target.currentMana, 0f, amount);
		EventInfoSpentMana info = new EventInfoSpentMana
		{
			actor = this,
			amount = amount,
			entity = target
		};
		ActorEvent_OnSpendMana?.Invoke(info);
		target.EntityEvent_OnGetManaSpent?.Invoke(info);
	}

	[Server]
	public T CreateAbilityInstance<T>(T prefab, Vector3 position, Quaternion? rotation, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T Actor::CreateAbilityInstance(T,UnityEngine.Vector3,System.Nullable`1<UnityEngine.Quaternion>,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return Dew.CreateAbilityInstance(prefab, position, rotation, this, info, beforePrepare);
	}

	[Server]
	public T CreateAbilityInstance<T>(Vector3 position, Quaternion? rotation, CastInfo info, Action<T> beforePrepare = null) where T : AbilityInstance
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T Actor::CreateAbilityInstance(UnityEngine.Vector3,System.Nullable`1<UnityEngine.Quaternion>,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return CreateAbilityInstance(DewResources.GetByType<T>(), position, rotation, info, beforePrepare);
	}

	[Server]
	public T CreateStatusEffect<T>(T prefab, Entity victim, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T Actor::CreateStatusEffect(T,Entity,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return Dew.CreateStatusEffect(prefab, victim, this, info, beforePrepare);
	}

	[Server]
	public T CreateStatusEffect<T>(Entity victim, CastInfo info, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T Actor::CreateStatusEffect(Entity,CastInfo,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return CreateStatusEffect(DewResources.GetByType<T>(), victim, info, beforePrepare);
	}

	public Se_GenericShield_OneShot GiveShield(Entity victim, float amount, float duration, bool isDecay = false)
	{
		return CreateStatusEffect(victim, new CastInfo(victim), delegate(Se_GenericShield_OneShot se)
		{
			se.shield.amount = ProcessShieldAmount(amount, victim);
			se.SetTimer(duration);
			se.isDecay = isDecay;
		});
	}

	public T CreateActor<T>(Vector3 pos, Quaternion? rot, Action<T> beforePrepare = null) where T : Actor
	{
		return Dew.CreateActor(pos, rot, this, beforePrepare);
	}

	public T CreateActor<T>(T prefab, Vector3 pos, Quaternion? rot, Action<T> beforePrepare = null) where T : Actor
	{
		return Dew.CreateActor(prefab, pos, rot, this, beforePrepare);
	}

	public T SpawnEntity<T>(Vector3 pos, Quaternion? rot, DewPlayer owner, int level, Action<T> beforeSpawn = null) where T : Entity
	{
		return Dew.SpawnEntity(pos, rot, this, owner, level, beforeSpawn);
	}

	public T SpawnEntity<T>(T entity, Vector3 pos, Quaternion? rot, DewPlayer owner, int level, Action<T> beforeSpawn = null) where T : Entity
	{
		return Dew.SpawnEntity(entity, pos, rot, this, owner, level, beforeSpawn);
	}

	[Server]
	public void Teleport(Entity entity, Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::Teleport(Entity,UnityEngine.Vector3)' called when server was not active");
		}
		else
		{
			entity.Control.Teleport(position);
		}
	}

	public OnScreenTimerHandle ShowOnScreenTimerLocally(OnScreenTimerHandle handle)
	{
		if (handle.rawText == null && handle.rawTextGetter == null && !DewLocalization.TryGetUIValue(GetType().Name + "_Name", out handle.rawText))
		{
			AbilityTrigger trg = firstTrigger;
			if (trg != null)
			{
				handle.rawText = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(trg.GetType()), 0);
			}
		}
		try
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnShowOnScreenTimer?.Invoke(handle);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return handle;
	}

	public void HideOnScreenTimerLocally(OnScreenTimerHandle handle)
	{
		try
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnHideOnScreenTimer?.Invoke(handle);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	[Server]
	public ElementalStatusEffect ApplyElemental(ElementalType type, Entity to, int appliedStacks = 1)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'ElementalStatusEffect Actor::ApplyElemental(ElementalType,Entity,System.Int32)' called when server was not active");
			return null;
		}
		Type seType = DewElementals.GetSeType(type);
		Entity fromEnt = this as Entity;
		if (fromEnt == null)
		{
			fromEnt = firstEntity;
		}
		if (fromEnt is Summon)
		{
			Entity newCand = fromEnt;
			while (newCand != null && newCand is Summon)
			{
				newCand = newCand.firstEntity;
			}
			if (newCand != null)
			{
				fromEnt = newCand;
			}
		}
		float eAmp = ((fromEnt != null) ? fromEnt.Status.GetElementalAmp(type) : 0f);
		if (to.Status.TryGetStatusEffect(seType, out var eff))
		{
			ElementalStatusEffect es = (ElementalStatusEffect)eff;
			if (appliedStacks > 0)
			{
				es.AddStack(appliedStacks);
				ActorEvent_OnApplyElemental?.Invoke(new EventInfoApplyElemental
				{
					actor = this,
					type = type,
					victim = to,
					addedStack = appliedStacks
				});
			}
			else
			{
				es.ResetDecayTimer();
			}
			if (es.ampAmount < eAmp)
			{
				es.NetworkampAmount = eAmp;
				es.parentActor = ((fromEnt != null) ? fromEnt : NetworkedManagerBase<ActorManager>.instance.serverActor);
				es.info = new CastInfo(fromEnt);
			}
			return es;
		}
		ElementalStatusEffect prefab = DewResources.GetByType<ElementalStatusEffect>(seType);
		if (fromEnt != null)
		{
			return fromEnt.CreateStatusEffect(prefab, to, new CastInfo(fromEnt), delegate(ElementalStatusEffect s)
			{
				s.NetworkampAmount = eAmp;
			});
		}
		ElementalStatusEffect result = NetworkedManagerBase<ActorManager>.instance.serverActor.CreateStatusEffect(prefab, to, new CastInfo(fromEnt), delegate(ElementalStatusEffect s)
		{
			s.NetworkampAmount = eAmp;
			s.SetStack(appliedStacks);
		});
		SafeAction<EventInfoApplyElemental> actorEvent_OnApplyElemental = ActorEvent_OnApplyElemental;
		if (actorEvent_OnApplyElemental != null)
		{
			actorEvent_OnApplyElemental.Invoke(new EventInfoApplyElemental
			{
				actor = this,
				type = type,
				victim = to,
				addedStack = appliedStacks
			});
			return result;
		}
		return result;
	}

	public Se_GenericEffectContainer CreateBasicEffect(Entity victim, BasicEffect eff, float duration, string id = null, DuplicateEffectBehavior onDuplicate = DuplicateEffectBehavior.ReplacePrevious)
	{
		if (victim == null)
		{
			Debug.LogWarning("Tried to create " + eff.GetType().Name + " with null victim");
			return null;
		}
		if (id != null && onDuplicate != DuplicateEffectBehavior.DoNothing)
		{
			foreach (StatusEffect se in new List<StatusEffect>(victim.Status.statusEffects))
			{
				if (se is Se_GenericEffectContainer cont && cont._id == id)
				{
					switch (onDuplicate)
					{
					case DuplicateEffectBehavior.ReplacePrevious:
						se.Destroy();
						break;
					case DuplicateEffectBehavior.UsePrevious:
						se.SetTimer(duration, duration);
						return cont;
					}
				}
			}
		}
		return CreateStatusEffect(victim, new CastInfo(firstEntity), delegate(Se_GenericEffectContainer se_GenericEffectContainer)
		{
			se_GenericEffectContainer.effect = eff;
			se_GenericEffectContainer.duration = duration;
			se_GenericEffectContainer.Network_id = id;
			se_GenericEffectContainer.isKilledByCrowdControlImmunity = KilledByUnstoppableEffects.Contains(eff.GetType());
			se_GenericEffectContainer.scaleDurationByTenacity = ScaleDurationByTenacityEffects.Contains(eff.GetType());
		});
	}

	private int GetLevel()
	{
		Actor cursor = this;
		while (cursor != null)
		{
			if (cursor is AbilityInstance ai)
			{
				return ai.effectiveLevel;
			}
			if (cursor is SkillTrigger st)
			{
				return st.level;
			}
			if (cursor is Entity e)
			{
				return e.level;
			}
			cursor = parentActor;
		}
		return 1;
	}

	private DewPlayer GetPlayer()
	{
		Actor cursor = this;
		while (cursor != null)
		{
			if (cursor is AbilityInstance ai && ai.info.caster != null)
			{
				return ai.info.caster.owner;
			}
			if (cursor is SkillTrigger st && st.owner != null)
			{
				return st.owner.owner;
			}
			if (cursor is Entity e)
			{
				return e.owner;
			}
			cursor = parentActor;
		}
		return null;
	}

	private CastInfo GetCastInfo()
	{
		CastInfo fallback = default(CastInfo);
		Actor cursor = this;
		while (cursor != null)
		{
			if (cursor is AbilityInstance ai)
			{
				return ai.info;
			}
			if (cursor is SkillTrigger st)
			{
				fallback.caster = st.owner;
			}
			cursor = cursor.parentActor;
		}
		return fallback;
	}

	public T SpawnSummon<T>(Vector3 position, Quaternion? rotation, Action<T> beforePrepare = null) where T : Summon
	{
		return Dew.SpawnEntity(position, rotation, this, GetPlayer(), 1, delegate(T ent)
		{
			ent.info = GetCastInfo();
			ent.skillLevel = GetLevel();
			try
			{
				beforePrepare?.Invoke(ent);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		});
	}

	public bool HasData<T>()
	{
		return _dataByType.ContainsKey(typeof(T));
	}

	public bool RemoveData<T>()
	{
		if (!_dataByType.Remove(typeof(T), out var obj))
		{
			return false;
		}
		_data.Remove(obj);
		object next = _data.Find((object o) => o.GetType() == typeof(T));
		if (next != null)
		{
			_dataByType.Add(typeof(T), next);
		}
		return true;
	}

	public bool RemoveData(object obj)
	{
		if (!_dataByType.ContainsKey(obj.GetType()))
		{
			return false;
		}
		_data.Remove(obj);
		object next = _data.Find((object o) => o.GetType() == obj.GetType());
		if (next != null)
		{
			_dataByType.Add(obj.GetType(), next);
		}
		return true;
	}

	public void AddData<T>(T data)
	{
		if (!_dataByType.ContainsKey(typeof(T)))
		{
			_dataByType.Add(typeof(T), data);
		}
		_data.Add(data);
	}

	public bool TryGetData<T>(out T data)
	{
		object raw;
		bool num = _dataByType.TryGetValue(typeof(T), out raw);
		if (num)
		{
			data = (T)raw;
			return num;
		}
		data = default(T);
		return num;
	}

	public T GetData<T>()
	{
		if (_dataByType.ContainsKey(typeof(T)))
		{
			return (T)_dataByType[typeof(T)];
		}
		return default(T);
	}

	public T FindData<T>(Predicate<T> predicate)
	{
		if (!_dataByType.TryGetValue(typeof(T), out var d))
		{
			return default(T);
		}
		if (predicate((T)d))
		{
			return (T)d;
		}
		object found = _data.Find((object o) => o is T obj && predicate(obj));
		if (found == null)
		{
			return default(T);
		}
		return (T)found;
	}

	public bool TryFindData<T>(Predicate<T> predicate, out T result)
	{
		if (!_dataByType.TryGetValue(typeof(T), out var d))
		{
			result = default(T);
			return false;
		}
		if (predicate((T)d))
		{
			result = (T)d;
			return true;
		}
		object found = _data.Find((object o) => o is T obj && predicate(obj));
		if (found == null)
		{
			result = default(T);
			return false;
		}
		result = (T)found;
		return true;
	}

	private void RegisterParentEvent()
	{
		if (_actorParentEventRegistered)
		{
			return;
		}
		_actorParentEventRegistered = true;
		ActorEvent_OnAbilityInstanceCreated += (Action<EventInfoAbilityInstance>)delegate(EventInfoAbilityInstance _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnAbilityInstanceCreated?.Invoke(_);
			}
		};
		ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)delegate(EventInfoAbilityInstance _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnAbilityInstanceBeforePrepare?.Invoke(_);
			}
		};
		ActorEvent_OnDealDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnDealDamage?.Invoke(_);
			}
		};
		ActorEvent_OnDoHeal += (Action<EventInfoHeal>)delegate(EventInfoHeal _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnDoHeal?.Invoke(_);
			}
		};
		ActorEvent_OnDoManaHeal += (Action<EventInfoHeal>)delegate(EventInfoHeal _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnDoManaHeal?.Invoke(_);
			}
		};
		ActorEvent_OnSpendMana += (Action<EventInfoSpentMana>)delegate(EventInfoSpentMana _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnSpendMana?.Invoke(_);
			}
		};
		ActorEvent_OnKill += (Action<EventInfoKill>)delegate(EventInfoKill _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnKill?.Invoke(_);
			}
		};
		ActorEvent_OnAttackHit += (Action<EventInfoAttackHit>)delegate(EventInfoAttackHit _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnAttackHit?.Invoke(_);
			}
		};
		ActorEvent_OnAttackEffectTriggered += (Action<EventInfoAttackEffect>)delegate(EventInfoAttackEffect _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnAttackEffectTriggered?.Invoke(_);
			}
		};
		ActorEvent_OnApplyElemental += (Action<EventInfoApplyElemental>)delegate(EventInfoApplyElemental _)
		{
			if (parentActor != null)
			{
				parentActor.ActorEvent_OnApplyElemental?.Invoke(_);
			}
		};
		ClientActorEvent_OnCreate += (Action<Actor>)delegate(Actor _)
		{
			if (parentActor != null)
			{
				parentActor.ClientActorEvent_OnCreate?.Invoke(_);
			}
		};
	}

	private IEnumerable<Actor> GetEnumerableAncestors()
	{
		Actor cursor = parentActor;
		int depth = 1;
		while (cursor != null)
		{
			if (depth >= 100)
			{
				throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
			}
			yield return cursor;
			cursor = cursor.parentActor;
			depth++;
		}
	}

	public bool IsDescendantOf(Actor actor)
	{
		Actor cursor = parentActor;
		int depth = 1;
		while (cursor != null)
		{
			if (depth >= 100)
			{
				throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
			}
			if (cursor == actor)
			{
				return true;
			}
			cursor = cursor.parentActor;
			depth++;
		}
		return false;
	}

	public bool IsDescendantOf<T>() where T : Actor
	{
		Actor cursor = parentActor;
		int depth = 1;
		while (cursor != null)
		{
			if (depth >= 100)
			{
				throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
			}
			if (cursor is T)
			{
				return true;
			}
			cursor = cursor.parentActor;
			depth++;
		}
		return false;
	}

	public T FindFirstAncestorOfType<T>() where T : Actor
	{
		Actor cursor = parentActor;
		int depth = 1;
		while (cursor != null)
		{
			if (depth >= 100)
			{
				throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
			}
			if (cursor is T t)
			{
				return t;
			}
			cursor = cursor.parentActor;
			depth++;
		}
		return null;
	}

	public T FindFirstOfType<T>() where T : Actor
	{
		Actor cursor = this;
		int depth = 1;
		while (cursor != null)
		{
			if (depth >= 100)
			{
				throw new Exception("Ancestor depth reached upper limit, suspecting cyclic parenting and exiting enumeration.");
			}
			if (cursor is T t)
			{
				return t;
			}
			cursor = cursor.parentActor;
			depth++;
		}
		return null;
	}

	[Server]
	public void Destroy()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::Destroy()' called when server was not active");
		}
		else if (this == null)
		{
			Debug.LogWarning("Tried to destroy null actor");
		}
		else
		{
			Dew.Destroy(base.gameObject);
		}
	}

	[Server]
	public void DestroyIfActive()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::DestroyIfActive()' called when server was not active");
		}
		else if (!(this == null) && isActive)
		{
			Dew.Destroy(base.gameObject);
		}
	}

	[Server]
	public void LockDestroy()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::LockDestroy()' called when server was not active");
			return;
		}
		checked
		{
			_destroyLock = (byte)(unchecked((uint)_destroyLock) + 1u);
			_destroyLockUpdateTime = Time.time;
		}
	}

	[Server]
	public void UnlockDestroy()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::UnlockDestroy()' called when server was not active");
			return;
		}
		checked
		{
			_destroyLock = (byte)(unchecked((uint)_destroyLock) - 1u);
			_destroyLockUpdateTime = Time.time;
		}
	}

	public bool IsExcludedFromRoomSave()
	{
		return HasData<Ad_ExcludedFromRoomSave>();
	}

	[Server]
	public void ExcludeFromRoomSave()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::ExcludeFromRoomSave()' called when server was not active");
		}
		else if (!HasData<Ad_ExcludedFromRoomSave>())
		{
			AddData(new Ad_ExcludedFromRoomSave());
		}
	}

	[Server]
	public void ClearExcludeFromRoomSave()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Actor::ClearExcludeFromRoomSave()' called when server was not active");
		}
		else if (HasData<Ad_ExcludedFromRoomSave>())
		{
			RemoveData<Ad_ExcludedFromRoomSave>();
		}
	}

	private void ProcessDealtDamage(ref DamageData data, Entity target)
	{
		dealtDamageProcessor.Process(ref data, this, target);
		foreach (Actor ancestor in ancestors)
		{
			ancestor.dealtDamageProcessor.Process(ref data, this, target);
		}
	}

	private void ProcessDealtHeal(ref HealData data, Entity target)
	{
		dealtHealProcessor.Process(ref data, this, target);
		foreach (Actor ancestor in ancestors)
		{
			ancestor.dealtHealProcessor.Process(ref data, this, target);
		}
	}

	private void ProcessDealtManaHeal(ref HealData data, Entity target)
	{
		dealtManaHealProcessor.Process(ref data, this, target);
		foreach (Actor ancestor in ancestors)
		{
			ancestor.dealtManaHealProcessor.Process(ref data, this, target);
		}
	}

	private void ProcessDealtShield(ref HealData data, Entity target)
	{
		dealtShieldProcessor.Process(ref data, this, target);
		foreach (Actor ancestor in ancestors)
		{
			ancestor.dealtShieldProcessor.Process(ref data, this, target);
		}
	}

	public float ProcessShieldAmount(float amount, Entity target)
	{
		HealData data = new HealData(amount);
		ProcessDealtShield(ref data, target);
		target.ProcessReceivedShield(ref data, this);
		return new FinalHealData(data, float.PositiveInfinity).amount;
	}

	[Server]
	public KillTracker TrackKills(float gracePeriod, Action<EventInfoKill> callback)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'KillTracker Actor::TrackKills(System.Single,System.Action`1<EventInfoKill>)' called when server was not active");
			return null;
		}
		return new KillTracker(this, gracePeriod, callback);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(Network_parentActor);
			writer.WriteBool(_isActive);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_parentActor);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(_isActive);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _parentActor, null, reader, ref ____parentActorNetId);
			GeneratedSyncVarDeserialize(ref _isActive, OnIsActiveChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _parentActor, null, reader, ref ____parentActorNetId);
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _isActive, OnIsActiveChanged, reader.ReadBool());
		}
	}
}
