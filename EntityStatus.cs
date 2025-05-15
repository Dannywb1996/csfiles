using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class EntityStatus : EntityComponent, ICleanup
{
	public const float MinimumArmorValue = -300f;

	public const float MaximumArmorValue = 100000f;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisInConversation_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisHealthHidden_003Ek__BackingField;

	[SyncVar(hook = "OnLevelChanged")]
	[SerializeField]
	private int _level;

	public string manaTypeKey = "";

	[SyncVar(hook = "OnBaseStatsChanged")]
	public BaseStats baseStats = BaseStats.Default;

	[SyncVar(hook = "OnScalingStatsChanged")]
	public BonusStats scalingStats = BonusStats.Default;

	[CompilerGenerated]
	[SyncVar]
	private Vector2 _003CspecialFill_003Ek__BackingField;

	[NonSerialized]
	[SyncVar(hook = "OnBonusStatsChanged")]
	public BonusStats bonusStats = BonusStats.Default;

	[NonSerialized]
	[SyncVar]
	private float _currentHealth;

	[NonSerialized]
	[SyncVar]
	private float _currentMana;

	[CompilerGenerated]
	[SyncVar]
	private CounterBool _003CisSectionTriggeringDisabled_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CmirageSkinInitAmount_003Ek__BackingField;

	[NonSerialized]
	[SyncVar]
	private float _currentShield;

	public SafeAction ClientEvent_OnStatCalculated;

	public SafeAction ClientEvent_OnStatusInfoUpdated;

	[NonSerialized]
	public float attackDamage;

	[NonSerialized]
	public float abilityPower;

	[NonSerialized]
	public float maxHealth;

	[NonSerialized]
	public float maxMana;

	[NonSerialized]
	public float healthRegen;

	[NonSerialized]
	public float manaRegen;

	[NonSerialized]
	public float attackSpeedPercentage = 100f;

	[NonSerialized]
	public float critAmp = 1f;

	[NonSerialized]
	public float critChance;

	[NonSerialized]
	public float tenacity;

	[NonSerialized]
	public float abilityHaste;

	[NonSerialized]
	public float movementSpeedPercentage = 100f;

	[NonSerialized]
	public float fireEffectAmp;

	[NonSerialized]
	public float coldEffectAmp;

	[NonSerialized]
	public float lightEffectAmp;

	[NonSerialized]
	public float darkEffectAmp;

	[NonSerialized]
	public float armorFromStats;

	private readonly List<StatBonus> _grantedStatBonuses = new List<StatBonus>();

	private const BasicEffectMask SuppressedByCrowdControlImmunity = BasicEffectMask.Slow | BasicEffectMask.Cripple | BasicEffectMask.Root | BasicEffectMask.Silence | BasicEffectMask.Stun | BasicEffectMask.Blind;

	private readonly SyncList<StatusEffect> _statusEffects = new SyncList<StatusEffect>();

	internal List<BasicEffect> _basicEffects = new List<BasicEffect>();

	[CompilerGenerated]
	[SyncVar]
	private BasicEffectMask _003CbasicEffectMask_003Ek__BackingField;

	[NonSerialized]
	[SyncVar]
	internal float _totalSlow;

	[NonSerialized]
	[SyncVar]
	internal float _totalSpeed;

	[NonSerialized]
	[SyncVar]
	internal float _totalHaste;

	[NonSerialized]
	[SyncVar]
	internal float _totalCripple;

	[NonSerialized]
	[SyncVar]
	internal AbilityTrigger _overridenAttack;

	[NonSerialized]
	[SyncVar]
	internal float _totalArmor;

	private bool _isStatusInfoDirty = true;

	private float _lastShieldTick;

	[CompilerGenerated]
	[SyncVar]
	private bool _003ChasCold_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003CfireStack_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003ClightStack_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003CdarkStack_003Ek__BackingField;

	private Dictionary<BasicEffect, object> _registeredHandlers = new Dictionary<BasicEffect, object>();

	internal BasicEffect _lastDamagePreventer;

	protected NetworkBehaviourSyncVar ____overridenAttackNetId;

	public bool isInConversation
	{
		[CompilerGenerated]
		get
		{
			return _003CisInConversation_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CisInConversation_003Ek__BackingField = value;
		}
	}

	public bool isHealthHidden
	{
		[CompilerGenerated]
		get
		{
			return _003CisHealthHidden_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisHealthHidden_003Ek__BackingField = value;
		}
	}

	public float missingHealth => maxHealth - currentHealth;

	public float missingMana => maxMana - currentMana;

	public float normalizedHealth => currentHealth / maxHealth;

	public float normalizedMana => currentMana / maxMana;

	public int level
	{
		get
		{
			return _level;
		}
		set
		{
			if (!NetworkServer.active)
			{
				throw new Exception("Only server can change entity's level.");
			}
			Network_level = value;
		}
	}

	public Vector2 specialFill
	{
		[CompilerGenerated]
		get
		{
			return _003CspecialFill_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CspecialFill_003Ek__BackingField = value;
		}
	}

	public float currentHealth
	{
		get
		{
			return _currentHealth;
		}
		internal set
		{
			Network_currentHealth = value;
		}
	}

	public float currentMana
	{
		get
		{
			return _currentMana;
		}
		internal set
		{
			Network_currentMana = value;
		}
	}

	public bool isAlive
	{
		get
		{
			if (base.entity.isActive)
			{
				return this != null;
			}
			return false;
		}
	}

	public bool isDead
	{
		get
		{
			if (base.entity.isActive)
			{
				return this == null;
			}
			return true;
		}
	}

	bool ICleanup.canDestroy => true;

	public CounterBool isSectionTriggeringDisabled
	{
		[CompilerGenerated]
		get
		{
			return _003CisSectionTriggeringDisabled_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CisSectionTriggeringDisabled_003Ek__BackingField = value;
		}
	}

	public float currentShield => _currentShield;

	public float mirageSkinInitAmount
	{
		[CompilerGenerated]
		get
		{
			return _003CmirageSkinInitAmount_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CmirageSkinInitAmount_003Ek__BackingField = value;
		}
	}

	public bool hasMirageSkin => mirageSkinInitAmount > 0f;

	public IReadOnlyList<StatusEffect> statusEffects => _statusEffects;

	public float attackSpeedMultiplier
	{
		get
		{
			float v = Mathf.Clamp(attackSpeedPercentage / 100f * (1f + (_totalHaste - _totalCripple) / 100f), 0f, float.PositiveInfinity);
			Dew.FilterNonOkayValues(ref v, 1f);
			return v;
		}
	}

	public float movementSpeedMultiplier
	{
		get
		{
			if (hasRoot)
			{
				return 0f;
			}
			float v = Mathf.Clamp(movementSpeedPercentage / 100f * (1f + (_totalSpeed - _totalSlow) / 100f), 0f, float.PositiveInfinity);
			Dew.FilterNonOkayValues(ref v, 1f);
			return v;
		}
	}

	public float totalSlow => _totalSlow;

	public float totalSpeed => _totalSpeed;

	public float totalHaste => _totalHaste;

	public float totalCripple => _totalCripple;

	public AbilityTrigger overridenAttack => Network_overridenAttack;

	public float totalArmor => _totalArmor + armorFromStats;

	public bool hasSlow => basicEffectMask.HasFlag(BasicEffectMask.Slow);

	public bool hasSpeed => basicEffectMask.HasFlag(BasicEffectMask.Speed);

	public bool hasHaste => basicEffectMask.HasFlag(BasicEffectMask.Haste);

	public bool hasCripple => basicEffectMask.HasFlag(BasicEffectMask.Cripple);

	public bool hasRoot => basicEffectMask.HasFlag(BasicEffectMask.Root);

	public bool hasSilence => basicEffectMask.HasFlag(BasicEffectMask.Silence);

	public bool hasStun => basicEffectMask.HasFlag(BasicEffectMask.Stun);

	public bool hasBlind => basicEffectMask.HasFlag(BasicEffectMask.Blind);

	public bool hasAttackCritical => basicEffectMask.HasFlag(BasicEffectMask.AttackCritical);

	public bool hasAttackOverride => basicEffectMask.HasFlag(BasicEffectMask.AttackOverride);

	public bool hasInvulnerable => basicEffectMask.HasFlag(BasicEffectMask.Invulnerable);

	public bool hasUnstoppable => basicEffectMask.HasFlag(BasicEffectMask.Unstoppable);

	public bool hasProtected => basicEffectMask.HasFlag(BasicEffectMask.Protected);

	public bool hasUncollidable => basicEffectMask.HasFlag(BasicEffectMask.Uncollidable);

	public bool hasInvisible => basicEffectMask.HasFlag(BasicEffectMask.Invisible);

	public bool hasArmorBoost => basicEffectMask.HasFlag(BasicEffectMask.ArmorBoost);

	public bool hasArmorReduction => basicEffectMask.HasFlag(BasicEffectMask.ArmorReduction);

	public bool hasDeathInterrupt => basicEffectMask.HasFlag(BasicEffectMask.DeathInterrupt);

	public bool hasDamageImmunity
	{
		get
		{
			if (!hasInvulnerable)
			{
				return hasProtected;
			}
			return true;
		}
	}

	public bool hasCrowdControlImmunity
	{
		get
		{
			if (!hasUnstoppable)
			{
				return hasInvulnerable;
			}
			return true;
		}
	}

	public bool hasImmobility
	{
		get
		{
			if (!hasRoot)
			{
				return hasStun;
			}
			return true;
		}
	}

	public BasicEffectMask basicEffectMask
	{
		[CompilerGenerated]
		get
		{
			return _003CbasicEffectMask_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CbasicEffectMask_003Ek__BackingField = value;
		}
	}

	public bool hasCold
	{
		[CompilerGenerated]
		get
		{
			return _003ChasCold_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003ChasCold_003Ek__BackingField = value;
		}
	}

	public int fireStack
	{
		[CompilerGenerated]
		get
		{
			return _003CfireStack_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CfireStack_003Ek__BackingField = value;
		}
	}

	public int lightStack
	{
		[CompilerGenerated]
		get
		{
			return _003ClightStack_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003ClightStack_003Ek__BackingField = value;
		}
	}

	public int darkStack
	{
		[CompilerGenerated]
		get
		{
			return _003CdarkStack_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CdarkStack_003Ek__BackingField = value;
		}
	}

	public bool Network_003CisInConversation_003Ek__BackingField
	{
		get
		{
			return isInConversation;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isInConversation, 1uL, null);
		}
	}

	public bool Network_003CisHealthHidden_003Ek__BackingField
	{
		get
		{
			return isHealthHidden;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isHealthHidden, 2uL, null);
		}
	}

	public int Network_level
	{
		get
		{
			return _level;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _level, 4uL, OnLevelChanged);
		}
	}

	public BaseStats NetworkbaseStats
	{
		get
		{
			return baseStats;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref baseStats, 8uL, OnBaseStatsChanged);
		}
	}

	public BonusStats NetworkscalingStats
	{
		get
		{
			return scalingStats;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref scalingStats, 16uL, OnScalingStatsChanged);
		}
	}

	public Vector2 Network_003CspecialFill_003Ek__BackingField
	{
		get
		{
			return specialFill;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref specialFill, 32uL, null);
		}
	}

	public BonusStats NetworkbonusStats
	{
		get
		{
			return bonusStats;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref bonusStats, 64uL, OnBonusStatsChanged);
		}
	}

	public float Network_currentHealth
	{
		get
		{
			return _currentHealth;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _currentHealth, 128uL, null);
		}
	}

	public float Network_currentMana
	{
		get
		{
			return _currentMana;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _currentMana, 256uL, null);
		}
	}

	public CounterBool Network_003CisSectionTriggeringDisabled_003Ek__BackingField
	{
		get
		{
			return isSectionTriggeringDisabled;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isSectionTriggeringDisabled, 512uL, null);
		}
	}

	public float Network_003CmirageSkinInitAmount_003Ek__BackingField
	{
		get
		{
			return mirageSkinInitAmount;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref mirageSkinInitAmount, 1024uL, null);
		}
	}

	public float Network_currentShield
	{
		get
		{
			return _currentShield;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _currentShield, 2048uL, null);
		}
	}

	public BasicEffectMask Network_003CbasicEffectMask_003Ek__BackingField
	{
		get
		{
			return basicEffectMask;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref basicEffectMask, 4096uL, null);
		}
	}

	public float Network_totalSlow
	{
		get
		{
			return _totalSlow;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _totalSlow, 8192uL, null);
		}
	}

	public float Network_totalSpeed
	{
		get
		{
			return _totalSpeed;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _totalSpeed, 16384uL, null);
		}
	}

	public float Network_totalHaste
	{
		get
		{
			return _totalHaste;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _totalHaste, 32768uL, null);
		}
	}

	public float Network_totalCripple
	{
		get
		{
			return _totalCripple;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _totalCripple, 65536uL, null);
		}
	}

	public AbilityTrigger Network_overridenAttack
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____overridenAttackNetId, ref _overridenAttack);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _overridenAttack, 131072uL, null, ref ____overridenAttackNetId);
		}
	}

	public float Network_totalArmor
	{
		get
		{
			return _totalArmor;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _totalArmor, 262144uL, null);
		}
	}

	public bool Network_003ChasCold_003Ek__BackingField
	{
		get
		{
			return hasCold;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref hasCold, 524288uL, null);
		}
	}

	public int Network_003CfireStack_003Ek__BackingField
	{
		get
		{
			return fireStack;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref fireStack, 1048576uL, null);
		}
	}

	public int Network_003ClightStack_003Ek__BackingField
	{
		get
		{
			return lightStack;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref lightStack, 2097152uL, null);
		}
	}

	public int Network_003CdarkStack_003Ek__BackingField
	{
		get
		{
			return darkStack;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref darkStack, 4194304uL, null);
		}
	}

	[Server]
	public void SetMana(float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::SetMana(System.Single)' called when server was not active");
		}
		else
		{
			Network_currentMana = Mathf.Clamp(amount, 0f, maxMana);
		}
	}

	[Server]
	public void SetHealth(float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::SetHealth(System.Single)' called when server was not active");
		}
		else
		{
			Network_currentHealth = Mathf.Clamp(amount, 0f, maxHealth);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.isServer || base.entity.isSleeping)
		{
			return;
		}
		if (_isStatusInfoDirty)
		{
			UpdateStatusInfo();
		}
		if (isAlive)
		{
			if (healthRegen > float.Epsilon || healthRegen < -1E-45f)
			{
				currentHealth = Mathf.Clamp(currentHealth + healthRegen * dt, 0f, maxHealth);
			}
			if (manaRegen > float.Epsilon || manaRegen < -1E-45f)
			{
				currentMana = Mathf.Clamp(currentMana + manaRegen * dt, 0f, maxMana);
			}
		}
		for (int i = 0; i < _grantedStatBonuses.Count; i++)
		{
			if (_grantedStatBonuses[i]._isDirty)
			{
				UpdateBonusStats();
				break;
			}
		}
		if (isAlive && currentHealth <= 0f)
		{
			base.entity.Kill();
		}
	}

	private void OnBaseStatsChanged(BaseStats _, BaseStats __)
	{
		CalculateStats();
	}

	private void OnScalingStatsChanged(BonusStats _, BonusStats __)
	{
		CalculateStats();
	}

	public override void OnStart()
	{
		base.OnStart();
		CalculateStats();
		if (base.isServer)
		{
			currentHealth = maxHealth;
			currentMana = maxMana;
		}
	}

	void ICleanup.OnCleanup()
	{
		foreach (StatusEffect eff in new List<StatusEffect>(_statusEffects))
		{
			if (eff != null && eff.isActive)
			{
				eff.Destroy();
			}
		}
	}

	private void OnLevelChanged(int oldLevel, int newLevel)
	{
		if (!base.isServer)
		{
			CalculateStats();
			return;
		}
		float prevMaxHealth = maxHealth;
		CalculateStats();
		float delta = maxHealth - prevMaxHealth;
		if (delta > 0f)
		{
			base.entity.Status.SetHealth(currentHealth + delta);
		}
		if (base.entity is Hero hero)
		{
			hero.ClientHeroEvent_OnLevelChanged?.Invoke(new EventInfoHeroLevelUp
			{
				oldLevel = oldLevel,
				newLevel = newLevel
			});
		}
	}

	[Server]
	public void DisableSectionTriggering()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::DisableSectionTriggering()' called when server was not active");
		}
		else
		{
			isSectionTriggeringDisabled += 1;
		}
	}

	[Server]
	public void EnableSectionTriggering()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::EnableSectionTriggering()' called when server was not active");
		}
		else
		{
			isSectionTriggeringDisabled -= 1;
		}
	}

	[Server]
	internal void UpdateShieldAmount()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::UpdateShieldAmount()' called when server was not active");
			return;
		}
		float totalShield = 0f;
		for (int i = 0; i < _basicEffects.Count; i++)
		{
			if (_basicEffects[i] is ShieldEffect shield)
			{
				totalShield += shield.amount;
			}
		}
		Network_currentShield = totalShield;
	}

	[Server]
	public StatBonus AddStatBonus(StatBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StatBonus EntityStatus::AddStatBonus(StatBonus)' called when server was not active");
			return null;
		}
		if (bonus == null)
		{
			throw new ArgumentNullException("bonus");
		}
		_grantedStatBonuses.Add(bonus);
		UpdateBonusStats();
		return bonus;
	}

	[Server]
	public void RemoveStatBonus(StatBonus bonus, bool noThrow = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::RemoveStatBonus(StatBonus,System.Boolean)' called when server was not active");
		}
		else if (!_grantedStatBonuses.Remove(bonus))
		{
			if (!noThrow)
			{
				throw new Exception($"Couldn't find the specified instance of BonusStats in this entity ({this}).");
			}
		}
		else
		{
			UpdateBonusStats();
		}
	}

	public void CalculateStats()
	{
		float normalizedHealth = currentHealth / maxHealth;
		float normalizedMana = currentMana / maxMana;
		if (float.IsNaN(normalizedHealth))
		{
			normalizedHealth = 0f;
		}
		if (float.IsNaN(normalizedMana))
		{
			normalizedMana = 0f;
		}
		attackDamage = baseStats.attackDamage;
		abilityPower = baseStats.abilityPower;
		maxHealth = baseStats.maxHealth;
		maxMana = baseStats.maxMana;
		healthRegen = baseStats.healthRegen;
		manaRegen = baseStats.manaRegen;
		attackSpeedPercentage = 100f;
		critAmp = baseStats.critAmp;
		critChance = baseStats.critChance;
		tenacity = baseStats.tenacity;
		abilityHaste = baseStats.abilityHaste;
		movementSpeedPercentage = 100f;
		fireEffectAmp = baseStats.fireEffectAmp;
		coldEffectAmp = baseStats.coldEffectAmp;
		lightEffectAmp = baseStats.lightEffectAmp;
		darkEffectAmp = baseStats.darkEffectAmp;
		armorFromStats = baseStats.armor;
		attackDamage += scalingStats.attackDamageFlat * (float)(_level - 1);
		attackDamage *= 1f + scalingStats.attackDamagePercentage / 100f * (float)(_level - 1);
		abilityPower += scalingStats.abilityPowerFlat * (float)(_level - 1);
		abilityPower *= 1f + scalingStats.abilityPowerPercentage / 100f * (float)(_level - 1);
		maxHealth += scalingStats.maxHealthFlat * (float)(_level - 1);
		maxHealth *= 1f + scalingStats.maxHealthPercentage / 100f * (float)(_level - 1);
		maxMana += scalingStats.maxManaFlat * (float)(_level - 1);
		maxMana *= 1f + scalingStats.maxManaPercentage / 100f * (float)(_level - 1);
		healthRegen += scalingStats.healthRegenFlat * (float)(_level - 1);
		healthRegen *= 1f + scalingStats.healthRegenPercentage / 100f * (float)(_level - 1);
		manaRegen += scalingStats.manaRegenFlat * (float)(_level - 1);
		manaRegen *= 1f + scalingStats.manaRegenPercentage / 100f * (float)(_level - 1);
		attackSpeedPercentage += scalingStats.attackSpeedPercentage * (float)(_level - 1);
		critAmp += scalingStats.critAmpFlat * (float)(_level - 1);
		critAmp *= 1f + scalingStats.critAmpPercentage / 100f * (float)(_level - 1);
		critChance += scalingStats.critChanceFlat * (float)(_level - 1);
		critChance *= 1f + scalingStats.critChancePercentage / 100f * (float)(_level - 1);
		tenacity += scalingStats.tenacityFlat * (float)(_level - 1);
		tenacity *= 1f + scalingStats.tenacityPercentage / 100f * (float)(_level - 1);
		abilityHaste += scalingStats.abilityHasteFlat * (float)(_level - 1);
		abilityHaste *= 1f + scalingStats.abilityHastePercentage / 100f * (float)(_level - 1);
		movementSpeedPercentage += scalingStats.movementSpeedPercentage * (float)(_level - 1);
		fireEffectAmp += scalingStats.fireEffectAmpFlat * (float)(_level - 1);
		coldEffectAmp += scalingStats.coldEffectAmpFlat * (float)(_level - 1);
		lightEffectAmp += scalingStats.lightEffectAmpFlat * (float)(_level - 1);
		darkEffectAmp += scalingStats.darkEffectAmpFlat * (float)(_level - 1);
		armorFromStats += scalingStats.armorFlat * (float)(_level - 1);
		armorFromStats *= 1f + scalingStats.armorPercentage / 100f * (float)(_level - 1);
		attackDamage += bonusStats.attackDamageFlat;
		attackDamage *= 1f + bonusStats.attackDamagePercentage / 100f;
		abilityPower += bonusStats.abilityPowerFlat;
		abilityPower *= 1f + bonusStats.abilityPowerPercentage / 100f;
		maxHealth += bonusStats.maxHealthFlat;
		maxHealth *= 1f + bonusStats.maxHealthPercentage / 100f;
		maxMana += bonusStats.maxManaFlat;
		maxMana *= 1f + bonusStats.maxManaPercentage / 100f;
		healthRegen += bonusStats.healthRegenFlat;
		healthRegen *= 1f + bonusStats.healthRegenPercentage / 100f;
		manaRegen += bonusStats.manaRegenFlat;
		manaRegen *= 1f + bonusStats.manaRegenPercentage / 100f;
		attackSpeedPercentage += bonusStats.attackSpeedPercentage;
		critAmp += bonusStats.critAmpFlat;
		critAmp *= 1f + bonusStats.critAmpPercentage / 100f;
		critChance += bonusStats.critChanceFlat;
		critChance *= 1f + bonusStats.critChancePercentage / 100f;
		abilityHaste += bonusStats.abilityHasteFlat;
		abilityHaste *= 1f + bonusStats.abilityHastePercentage / 100f;
		tenacity += bonusStats.tenacityFlat;
		tenacity *= 1f + bonusStats.tenacityPercentage / 100f;
		movementSpeedPercentage += bonusStats.movementSpeedPercentage;
		fireEffectAmp += bonusStats.fireEffectAmpFlat;
		coldEffectAmp += bonusStats.coldEffectAmpFlat;
		lightEffectAmp += bonusStats.lightEffectAmpFlat;
		darkEffectAmp += bonusStats.darkEffectAmpFlat;
		armorFromStats += bonusStats.armorFlat;
		armorFromStats *= 1f + bonusStats.armorPercentage / 100f;
		abilityPower = Mathf.Max(abilityPower, 1f);
		attackDamage = Mathf.Max(attackDamage, 1f);
		maxHealth = Mathf.Max(maxHealth, 1f);
		healthRegen = Mathf.Max(healthRegen, 0f);
		if (base.isServer)
		{
			currentHealth = normalizedHealth * maxHealth;
			currentMana = normalizedMana * maxMana;
		}
		ClientEvent_OnStatCalculated?.Invoke();
	}

	private void OnBonusStatsChanged(BonusStats oldStats, BonusStats newStats)
	{
		CalculateStats();
	}

	[Server]
	private void UpdateBonusStats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::UpdateBonusStats()' called when server was not active");
			return;
		}
		BonusStats stats = BonusStats.Default;
		for (int i = 0; i < _grantedStatBonuses.Count; i++)
		{
			StatBonus bonus = _grantedStatBonuses[i];
			stats += bonus;
			bonus._isDirty = false;
		}
		NetworkbonusStats = stats;
	}

	[Server]
	internal void DirtyStatusInfo()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::DirtyStatusInfo()' called when server was not active");
		}
		else
		{
			_isStatusInfoDirty = true;
		}
	}

	[Server]
	internal void AddBasicEffect(BasicEffect eff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::AddBasicEffect(BasicEffect)' called when server was not active");
			return;
		}
		eff.isAlive = true;
		if (base.entity.Control.isDisplacing && base.entity.Control.ongoingDisplacement.isCanceledByCC && !base.entity.Status.hasCrowdControlImmunity && (eff is RootEffect || eff is StunEffect))
		{
			base.entity.Control.CancelOngoingDisplacement();
		}
		AttackCriticalEffect ac = eff as AttackCriticalEffect;
		if (ac != null)
		{
			Action<EventInfoAttackFired> handler = delegate(EventInfoAttackFired info)
			{
				if (info.isCrit)
				{
					ac.onUse?.Invoke();
				}
			};
			base.entity.EntityEvent_OnAttackFired += handler;
			_registeredHandlers.Add(ac, handler);
		}
		else
		{
			AttackOverrideEffect ao = eff as AttackOverrideEffect;
			if (ao != null)
			{
				Action<EventInfoAbilityInstance> handler2 = delegate(EventInfoAbilityInstance info)
				{
					if (info.actor == ao.trigger)
					{
						ao.onUse?.Invoke();
					}
				};
				base.entity.ActorEvent_OnAbilityInstanceCreated += handler2;
				_registeredHandlers.Add(ao, handler2);
				base.entity.Ability._attackAbilityOverrides.Add(ao.trigger);
				ao.trigger.owner = base.entity;
			}
			else
			{
				AttackEmpowerEffect emp = eff as AttackEmpowerEffect;
				if (emp != null)
				{
					float startTime = Time.time;
					List<Actor> banned = new List<Actor>();
					Action<EventInfoAttackEffect> attackEffectHandler = delegate(EventInfoAttackEffect info)
					{
						if (!banned.Contains(info.actor) && (info.actor is Entity || !(info.actor.creationTime < startTime)) && (emp.maxTriggerCount == int.MaxValue || emp._nextIndex < emp.maxTriggerCount))
						{
							emp.onAttackEffect?.Invoke(info, emp._nextIndex);
							emp._nextIndex++;
							if (emp.maxTriggerCount != int.MaxValue && emp._nextIndex >= emp.maxTriggerCount)
							{
								try
								{
									emp.onDepleted?.Invoke();
								}
								catch (Exception exception)
								{
									Debug.LogException(exception);
								}
							}
						}
					};
					Action<EventInfoAttackFired> attackFiredHandler = delegate(EventInfoAttackFired info)
					{
						if (emp.maxTriggerCount == int.MaxValue || emp._nextIndex < emp.maxTriggerCount)
						{
							eff.parent.LockDestroy();
							int index = emp._nextIndex;
							emp._nextIndex++;
							if (emp.maxTriggerCount != int.MaxValue && emp._nextIndex >= emp.maxTriggerCount)
							{
								try
								{
									emp.onDepleted?.Invoke();
								}
								catch (Exception exception)
								{
									Debug.LogException(exception);
								}
							}
							banned.Add(info.instance);
							info.instance.ActorEvent_OnAttackEffectTriggered += (Action<EventInfoAttackEffect>)delegate(EventInfoAttackEffect attack)
							{
								emp.onAttackEffect?.Invoke(attack, index);
							};
							info.instance.ClientActorEvent_OnDestroyed += (Action<Actor>)delegate
							{
								eff.parent.UnlockDestroy();
							};
						}
					};
					base.entity.EntityEvent_OnAttackEffectTriggered += attackEffectHandler;
					base.entity.EntityEvent_OnAttackFiredBeforePrepare += attackFiredHandler;
					Tuple<Action<EventInfoAttackEffect>, Action<EventInfoAttackFired>> tuple = new Tuple<Action<EventInfoAttackEffect>, Action<EventInfoAttackFired>>(attackEffectHandler, attackFiredHandler);
					_registeredHandlers.Add(emp, tuple);
				}
				else
				{
					ProtectedEffect pt = eff as ProtectedEffect;
					if (pt != null)
					{
						Action<EventInfoDamageNegatedByImmunity> handler3 = delegate(EventInfoDamageNegatedByImmunity info)
						{
							if (info.effect == pt)
							{
								pt.onDamageNegated?.Invoke(info);
							}
						};
						base.entity.EntityEvent_OnDamageNegated += handler3;
						_registeredHandlers.Add(pt, handler3);
					}
					else
					{
						InvulnerableEffect iv = eff as InvulnerableEffect;
						if (iv != null)
						{
							Action<EventInfoDamageNegatedByImmunity> handler4 = delegate(EventInfoDamageNegatedByImmunity info)
							{
								if (info.effect == iv)
								{
									iv.onDamageNegated?.Invoke(info);
								}
							};
							base.entity.EntityEvent_OnDamageNegated += handler4;
							_registeredHandlers.Add(iv, handler4);
						}
					}
				}
			}
		}
		_basicEffects.Add(eff);
		DirtyStatusInfo();
		UpdateStatusInfo();
	}

	[Server]
	internal void RemoveBasicEffect(BasicEffect eff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::RemoveBasicEffect(BasicEffect)' called when server was not active");
			return;
		}
		eff.isAlive = false;
		if (eff is AttackCriticalEffect ac)
		{
			base.entity.EntityEvent_OnAttackFired -= (Action<EventInfoAttackFired>)_registeredHandlers[ac];
			_registeredHandlers.Remove(ac);
		}
		else if (eff is AttackOverrideEffect ao)
		{
			base.entity.ActorEvent_OnAbilityInstanceCreated -= (Action<EventInfoAbilityInstance>)_registeredHandlers[ao];
			_registeredHandlers.Remove(ao);
			base.entity.Ability._attackAbilityOverrides.Remove(ao.trigger);
			ao.trigger.owner = null;
			if (ao.trigger != null && ao.trigger.isServer && ao._shouldTriggerBeDisposed)
			{
				ao.trigger.Destroy();
			}
		}
		else if (eff is AttackEmpowerEffect emp)
		{
			Tuple<Action<EventInfoAttackEffect>, Action<EventInfoAttackFired>> tuple = (Tuple<Action<EventInfoAttackEffect>, Action<EventInfoAttackFired>>)_registeredHandlers[emp];
			base.entity.EntityEvent_OnAttackEffectTriggered -= tuple.Item1;
			base.entity.EntityEvent_OnAttackFiredBeforePrepare -= tuple.Item2;
			_registeredHandlers.Remove(emp);
		}
		else if (eff is ProtectedEffect pt)
		{
			base.entity.EntityEvent_OnDamageNegated -= (Action<EventInfoDamageNegatedByImmunity>)_registeredHandlers[pt];
			_registeredHandlers.Remove(pt);
		}
		else if (eff is InvulnerableEffect iv)
		{
			base.entity.EntityEvent_OnDamageNegated -= (Action<EventInfoDamageNegatedByImmunity>)_registeredHandlers[iv];
			_registeredHandlers.Remove(iv);
		}
		_basicEffects.Remove(eff);
		DirtyStatusInfo();
	}

	[Server]
	public void UpdateStatusInfo(bool skipIfNotDirty = true)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::UpdateStatusInfo(System.Boolean)' called when server was not active");
		}
		else
		{
			if (skipIfNotDirty && !_isStatusInfoDirty)
			{
				return;
			}
			_isStatusInfoDirty = false;
			_lastDamagePreventer = null;
			BasicEffectMask calcMask = (BasicEffectMask)0u;
			float calcTotalSlow = 0f;
			float calcTotalSpeed = 0f;
			float calcTotalHaste = 0f;
			float calcTotalCripple = 0f;
			AbilityTrigger calcOverridenAttack = null;
			float calcTotalArmor = 0f;
			for (int i = 0; i < _basicEffects.Count; i++)
			{
				BasicEffect se = _basicEffects[i];
				if (!(se is BasicEffectWithStrength { strength: 0 }))
				{
					calcMask |= se._mask;
				}
				if (se is BasicEffectWithStrength { strength: var strength } bws)
				{
					if (bws.decay && bws.parent.normalizedDuration.HasValue)
					{
						strength *= bws.parent.normalizedDuration.Value;
						_isStatusInfoDirty = true;
					}
					if (se is SlowEffect)
					{
						calcTotalSlow = Mathf.Max(calcTotalSlow, strength);
					}
					else if (se is SpeedEffect)
					{
						calcTotalSpeed += strength;
					}
					else if (se is HasteEffect)
					{
						calcTotalHaste += strength;
					}
					else if (se is CrippleEffect)
					{
						calcTotalCripple = Mathf.Max(_totalCripple, strength);
					}
					else if (se is ArmorBoostEffect)
					{
						calcTotalArmor += strength;
					}
					else if (se is ArmorReductionEffect)
					{
						calcTotalArmor -= strength;
					}
				}
				else if (se is AttackOverrideEffect attackOverride)
				{
					calcOverridenAttack = attackOverride.trigger;
				}
				else if (se is ProtectedEffect)
				{
					if (_lastDamagePreventer == null || !(_lastDamagePreventer is InvulnerableEffect))
					{
						_lastDamagePreventer = se;
					}
				}
				else if (se is InvulnerableEffect)
				{
					_lastDamagePreventer = se;
				}
			}
			if ((calcMask & (BasicEffectMask.Invulnerable | BasicEffectMask.Unstoppable)) != 0)
			{
				calcMask = (BasicEffectMask)((uint)calcMask & 0xFFFFFF06u);
				calcTotalSlow = 0f;
				calcTotalCripple = 0f;
			}
			calcTotalSlow *= 1f - tenacity / 100f;
			calcTotalCripple *= 1f - tenacity / 100f;
			calcTotalArmor = Mathf.Clamp(calcTotalArmor, -300f, 100000f);
			Network_003CbasicEffectMask_003Ek__BackingField = calcMask;
			Network_totalSlow = calcTotalSlow;
			Network_totalSpeed = calcTotalSpeed;
			Network_totalHaste = calcTotalHaste;
			Network_totalCripple = calcTotalCripple;
			Network_overridenAttack = calcOverridenAttack;
			Network_totalArmor = calcTotalArmor;
			UpdateShieldAmount();
			RpcInvokeStatusInfoUpdated();
		}
	}

	[ClientRpc]
	private void RpcInvokeStatusInfoUpdated()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityStatus::RpcInvokeStatusInfoUpdated()", -718389064, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public bool HasStatusEffect<T>() where T : StatusEffect
	{
		return HasStatusEffect(typeof(T));
	}

	public bool HasStatusEffect(Type statusEffectType)
	{
		for (int i = 0; i < _statusEffects.Count; i++)
		{
			StatusEffect ele = _statusEffects[i];
			if (statusEffectType.IsInstanceOfType(ele))
			{
				return true;
			}
		}
		return false;
	}

	public T GetStatusEffect<T>() where T : StatusEffect
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (statusEffects[i] is T found)
			{
				return found;
			}
		}
		return null;
	}

	public StatusEffect GetStatusEffect(Type type)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			StatusEffect eff = statusEffects[i];
			if (type.IsInstanceOfType(eff))
			{
				return eff;
			}
		}
		return null;
	}

	public T FindStatusEffect<T>(Func<T, bool> predicate) where T : StatusEffect
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (statusEffects[i] is T found && predicate(found))
			{
				return found;
			}
		}
		return null;
	}

	public StatusEffect FindStatusEffect(Type type, Func<StatusEffect, bool> predicate)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			StatusEffect eff = statusEffects[i];
			if (type.IsInstanceOfType(eff) && predicate(eff))
			{
				return eff;
			}
		}
		return null;
	}

	public IEnumerable<T> GetStatusEffects<T>() where T : StatusEffect
	{
		for (int i = statusEffects.Count - 1; i >= 0; i--)
		{
			if (statusEffects[i] is T found)
			{
				yield return found;
			}
		}
	}

	public IEnumerable<StatusEffect> GetStatusEffects(Type type)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			StatusEffect eff = statusEffects[i];
			if (type.IsInstanceOfType(eff))
			{
				yield return eff;
			}
		}
	}

	public bool TryGetStatusEffect<T>(out T effect) where T : StatusEffect
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			if (statusEffects[i] is T found)
			{
				effect = found;
				return true;
			}
		}
		effect = null;
		return false;
	}

	public bool TryGetStatusEffect(Type type, out StatusEffect effect)
	{
		for (int i = 0; i < statusEffects.Count; i++)
		{
			StatusEffect eff = statusEffects[i];
			if (type.IsInstanceOfType(eff))
			{
				effect = eff;
				return true;
			}
		}
		effect = null;
		return false;
	}

	[Server]
	internal void AddStatusEffect(StatusEffect eff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::AddStatusEffect(StatusEffect)' called when server was not active");
			return;
		}
		_statusEffects.Add(eff);
		InvokeAddStatusEffect(new EventInfoStatusEffect
		{
			victim = base.entity,
			effect = eff
		});
	}

	[Server]
	internal void RemoveStatusEffect(StatusEffect eff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityStatus::RemoveStatusEffect(StatusEffect)' called when server was not active");
			return;
		}
		_statusEffects.Remove(eff);
		InvokeRemoveStatusEffect(new EventInfoStatusEffect
		{
			victim = base.entity,
			effect = eff
		});
	}

	[ClientRpc]
	private void InvokeAddStatusEffect(EventInfoStatusEffect info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoStatusEffect(writer, info);
		SendRPCInternal("System.Void EntityStatus::InvokeAddStatusEffect(EventInfoStatusEffect)", 1999761779, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeRemoveStatusEffect(EventInfoStatusEffect info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoStatusEffect(writer, info);
		SendRPCInternal("System.Void EntityStatus::InvokeRemoveStatusEffect(EventInfoStatusEffect)", -797915986, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public bool HasElemental(ElementalType type)
	{
		return GetElementalStack(type) > 0;
	}

	public int GetElementalStack(ElementalType type)
	{
		switch (type)
		{
		case ElementalType.Fire:
			return fireStack;
		case ElementalType.Cold:
			if (!hasCold)
			{
				return 0;
			}
			return 1;
		case ElementalType.Light:
			return lightStack;
		case ElementalType.Dark:
			return darkStack;
		default:
			return 0;
		}
	}

	public float GetElementalAmp(ElementalType type)
	{
		return type switch
		{
			ElementalType.Fire => fireEffectAmp, 
			ElementalType.Cold => coldEffectAmp, 
			ElementalType.Light => lightEffectAmp, 
			ElementalType.Dark => darkEffectAmp, 
			_ => 0f, 
		};
	}

	public EntityStatus()
	{
		InitSyncObject(_statusEffects);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeStatusInfoUpdated()
	{
		ClientEvent_OnStatusInfoUpdated?.Invoke();
	}

	protected static void InvokeUserCode_RpcInvokeStatusInfoUpdated(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeStatusInfoUpdated called on server.");
		}
		else
		{
			((EntityStatus)obj).UserCode_RpcInvokeStatusInfoUpdated();
		}
	}

	protected void UserCode_InvokeAddStatusEffect__EventInfoStatusEffect(EventInfoStatusEffect info)
	{
		base.entity.ClientEntityEvent_OnStatusEffectAdded?.Invoke(info);
	}

	protected static void InvokeUserCode_InvokeAddStatusEffect__EventInfoStatusEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeAddStatusEffect called on server.");
		}
		else
		{
			((EntityStatus)obj).UserCode_InvokeAddStatusEffect__EventInfoStatusEffect(GeneratedNetworkCode._Read_EventInfoStatusEffect(reader));
		}
	}

	protected void UserCode_InvokeRemoveStatusEffect__EventInfoStatusEffect(EventInfoStatusEffect info)
	{
		base.entity.ClientEntityEvent_OnStatusEffectRemoved?.Invoke(info);
	}

	protected static void InvokeUserCode_InvokeRemoveStatusEffect__EventInfoStatusEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeRemoveStatusEffect called on server.");
		}
		else
		{
			((EntityStatus)obj).UserCode_InvokeRemoveStatusEffect__EventInfoStatusEffect(GeneratedNetworkCode._Read_EventInfoStatusEffect(reader));
		}
	}

	static EntityStatus()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(EntityStatus), "System.Void EntityStatus::RpcInvokeStatusInfoUpdated()", InvokeUserCode_RpcInvokeStatusInfoUpdated);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityStatus), "System.Void EntityStatus::InvokeAddStatusEffect(EventInfoStatusEffect)", InvokeUserCode_InvokeAddStatusEffect__EventInfoStatusEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityStatus), "System.Void EntityStatus::InvokeRemoveStatusEffect(EventInfoStatusEffect)", InvokeUserCode_InvokeRemoveStatusEffect__EventInfoStatusEffect);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(isInConversation);
			writer.WriteBool(isHealthHidden);
			writer.WriteInt(_level);
			GeneratedNetworkCode._Write_BaseStats(writer, baseStats);
			GeneratedNetworkCode._Write_BonusStats(writer, scalingStats);
			writer.WriteVector2(specialFill);
			GeneratedNetworkCode._Write_BonusStats(writer, bonusStats);
			writer.WriteFloat(_currentHealth);
			writer.WriteFloat(_currentMana);
			writer.WriteCounterBool(isSectionTriggeringDisabled);
			writer.WriteFloat(mirageSkinInitAmount);
			writer.WriteFloat(_currentShield);
			GeneratedNetworkCode._Write_BasicEffectMask(writer, basicEffectMask);
			writer.WriteFloat(_totalSlow);
			writer.WriteFloat(_totalSpeed);
			writer.WriteFloat(_totalHaste);
			writer.WriteFloat(_totalCripple);
			writer.WriteNetworkBehaviour(Network_overridenAttack);
			writer.WriteFloat(_totalArmor);
			writer.WriteBool(hasCold);
			writer.WriteInt(fireStack);
			writer.WriteInt(lightStack);
			writer.WriteInt(darkStack);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(isInConversation);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(isHealthHidden);
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteInt(_level);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			GeneratedNetworkCode._Write_BaseStats(writer, baseStats);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			GeneratedNetworkCode._Write_BonusStats(writer, scalingStats);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteVector2(specialFill);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			GeneratedNetworkCode._Write_BonusStats(writer, bonusStats);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteFloat(_currentHealth);
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteFloat(_currentMana);
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteCounterBool(isSectionTriggeringDisabled);
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteFloat(mirageSkinInitAmount);
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteFloat(_currentShield);
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			GeneratedNetworkCode._Write_BasicEffectMask(writer, basicEffectMask);
		}
		if ((base.syncVarDirtyBits & 0x2000L) != 0L)
		{
			writer.WriteFloat(_totalSlow);
		}
		if ((base.syncVarDirtyBits & 0x4000L) != 0L)
		{
			writer.WriteFloat(_totalSpeed);
		}
		if ((base.syncVarDirtyBits & 0x8000L) != 0L)
		{
			writer.WriteFloat(_totalHaste);
		}
		if ((base.syncVarDirtyBits & 0x10000L) != 0L)
		{
			writer.WriteFloat(_totalCripple);
		}
		if ((base.syncVarDirtyBits & 0x20000L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_overridenAttack);
		}
		if ((base.syncVarDirtyBits & 0x40000L) != 0L)
		{
			writer.WriteFloat(_totalArmor);
		}
		if ((base.syncVarDirtyBits & 0x80000L) != 0L)
		{
			writer.WriteBool(hasCold);
		}
		if ((base.syncVarDirtyBits & 0x100000L) != 0L)
		{
			writer.WriteInt(fireStack);
		}
		if ((base.syncVarDirtyBits & 0x200000L) != 0L)
		{
			writer.WriteInt(lightStack);
		}
		if ((base.syncVarDirtyBits & 0x400000L) != 0L)
		{
			writer.WriteInt(darkStack);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref isInConversation, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref isHealthHidden, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _level, OnLevelChanged, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref baseStats, OnBaseStatsChanged, GeneratedNetworkCode._Read_BaseStats(reader));
			GeneratedSyncVarDeserialize(ref scalingStats, OnScalingStatsChanged, GeneratedNetworkCode._Read_BonusStats(reader));
			GeneratedSyncVarDeserialize(ref specialFill, null, reader.ReadVector2());
			GeneratedSyncVarDeserialize(ref bonusStats, OnBonusStatsChanged, GeneratedNetworkCode._Read_BonusStats(reader));
			GeneratedSyncVarDeserialize(ref _currentHealth, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _currentMana, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref isSectionTriggeringDisabled, null, reader.ReadCounterBool());
			GeneratedSyncVarDeserialize(ref mirageSkinInitAmount, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _currentShield, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref basicEffectMask, null, GeneratedNetworkCode._Read_BasicEffectMask(reader));
			GeneratedSyncVarDeserialize(ref _totalSlow, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _totalSpeed, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _totalHaste, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _totalCripple, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _overridenAttack, null, reader, ref ____overridenAttackNetId);
			GeneratedSyncVarDeserialize(ref _totalArmor, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref hasCold, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref fireStack, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref lightStack, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref darkStack, null, reader.ReadInt());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isInConversation, null, reader.ReadBool());
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isHealthHidden, null, reader.ReadBool());
		}
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _level, OnLevelChanged, reader.ReadInt());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref baseStats, OnBaseStatsChanged, GeneratedNetworkCode._Read_BaseStats(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref scalingStats, OnScalingStatsChanged, GeneratedNetworkCode._Read_BonusStats(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref specialFill, null, reader.ReadVector2());
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref bonusStats, OnBonusStatsChanged, GeneratedNetworkCode._Read_BonusStats(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _currentHealth, null, reader.ReadFloat());
		}
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _currentMana, null, reader.ReadFloat());
		}
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isSectionTriggeringDisabled, null, reader.ReadCounterBool());
		}
		if ((num & 0x400L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref mirageSkinInitAmount, null, reader.ReadFloat());
		}
		if ((num & 0x800L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _currentShield, null, reader.ReadFloat());
		}
		if ((num & 0x1000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref basicEffectMask, null, GeneratedNetworkCode._Read_BasicEffectMask(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _totalSlow, null, reader.ReadFloat());
		}
		if ((num & 0x4000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _totalSpeed, null, reader.ReadFloat());
		}
		if ((num & 0x8000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _totalHaste, null, reader.ReadFloat());
		}
		if ((num & 0x10000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _totalCripple, null, reader.ReadFloat());
		}
		if ((num & 0x20000L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _overridenAttack, null, reader, ref ____overridenAttackNetId);
		}
		if ((num & 0x40000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _totalArmor, null, reader.ReadFloat());
		}
		if ((num & 0x80000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref hasCold, null, reader.ReadBool());
		}
		if ((num & 0x100000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref fireStack, null, reader.ReadInt());
		}
		if ((num & 0x200000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref lightStack, null, reader.ReadInt());
		}
		if ((num & 0x400000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref darkStack, null, reader.ReadInt());
		}
	}
}
