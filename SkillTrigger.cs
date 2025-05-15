using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class SkillTrigger : AbilityTrigger, IInteractable, IItem, IExcludeFromPool
{
	public const float Ult_GlobalChargeMultiplier = 0.85f;

	public const float Ult_OnKillLesser = 0.12750001f;

	public const float Ult_OnKillNormal = 0.63750005f;

	public const float Ult_OnKillMiniBoss = 12.75f;

	public const float Ult_OnKillBoss = 42.5f;

	public const float Ult_PerDealtNonElementalDamage = 0.2125f;

	public const float Ult_PerDealtElementalDamage = 0.0425f;

	public const float Ult_PerDealtAttack = 0.2125f;

	public const float Ult_PerDealtDamageLesserEnemyHealthRatio = 0.12750001f;

	public const float Ult_PerDealtDamageNormalEnemyHealthRatio = 0.25500003f;

	public const float Ult_PerDealtDamageMiniBossEnemyHealthRatio = 12.75f;

	public const float Ult_PerDealtDamageBossEnemyHealthRatio = 25.5f;

	public const float Ult_PerTakenNonElementalDamage = 0.2125f;

	public const float Ult_PerTakenElementalDamage = 0.0425f;

	public const float Ult_PerTakenDamageMyHealthRatio = 25.5f;

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
	[SyncVar]
	private HeroSkillLocation _003CskillType_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private uint _003CcharacterSkillOwner_003Ek__BackingField;

	public SafeAction<int, int> ClientSkillEvent_OnLevelChange;

	[SyncVar(hook = "OnLevelChange")]
	private int _level;

	public Rarity rarity;

	public SkillType type;

	[SyncVar]
	public DescriptionTags tags;

	public bool isLevelUpEnabled = true;

	public bool useCustomSkillHastePerLevel;

	public float customSkillHastePerLevel = 10f;

	public bool showFillAmountOnScreen = true;

	public GameObject startEffect;

	public GameObject endEffect;

	public bool excludeFromPool;

	[NonSerialized]
	public StatBonus statBonus = new StatBonus();

	private SkillWorldModel _worldModel;

	private SkillBonus _skillHastePerLevelBonus;

	[SyncVar]
	private float _skillHasteGrantedByLevelMultiplier = 1f;

	[CompilerGenerated]
	[SyncVar]
	private Color _003CspecialOverlayColor_003Ek__BackingField;

	public const float DismantleTapMinInterval = 0.075f;

	public const float DismantleDecayStartTime = 1f;

	public const float DismantleTapStrength = 0.4f;

	public SafeAction<float, float> ClientSkillEvent_OnDismantleProgressChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnDismantleProgressChanged")]
	private float _003CdismantleProgress_003Ek__BackingField;

	private float _lastDismantleTapTime;

	internal Hero _lastDismantler;

	[CompilerGenerated]
	[SyncVar]
	private int _003CmaxSellGold_003Ek__BackingField;

	private List<SkillBonus> _grantedSkillBonus;

	internal bool _isSkillBonusDirty;

	[CompilerGenerated]
	[SyncVar]
	private float _003CsbCooldownMultiplier_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CsbCooldownMultiplierIgnoreReceiveCooldownReductionFlag_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CsbCooldownOffset_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CsbCooldownOffsetIgnoreReceiveCooldownReductionFlag_003Ek__BackingField;

	private int? _mainConfigOriginalCharge;

	protected NetworkBehaviourSyncVar ____003ChandOwner_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003CtempOwner_003Ek__BackingFieldNetId;

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

	public HeroSkillLocation skillType
	{
		[CompilerGenerated]
		get
		{
			return _003CskillType_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CskillType_003Ek__BackingField = value;
		}
	}

	public uint characterSkillOwner
	{
		[CompilerGenerated]
		get
		{
			return _003CcharacterSkillOwner_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcharacterSkillOwner_003Ek__BackingField = value;
		}
	}

	public int level
	{
		get
		{
			return ScalingValue.levelOverride ?? _level;
		}
		set
		{
			Network_level = value;
		}
	}

	public float skillHastePerLevel
	{
		get
		{
			if (!useCustomSkillHastePerLevel)
			{
				return NetworkedManagerBase<GameManager>.instance.GetGainedSkillHastePerSkillLevel(this);
			}
			return customSkillHastePerLevel;
		}
	}

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public bool isCharacterSkill => rarity == Rarity.Character;

	public Color specialOverlayColor
	{
		[CompilerGenerated]
		get
		{
			return _003CspecialOverlayColor_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CspecialOverlayColor_003Ek__BackingField = value;
		}
	} = Color.clear;

	Rarity IItem.rarity => rarity;

	public new Hero owner => base.owner as Hero;

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

	public int effectiveLevel => level;

	public Entity statEntity => owner;

	public float sbCooldownMultiplier
	{
		[CompilerGenerated]
		get
		{
			return _003CsbCooldownMultiplier_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CsbCooldownMultiplier_003Ek__BackingField = value;
		}
	} = 1f;

	public float sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag
	{
		[CompilerGenerated]
		get
		{
			return _003CsbCooldownMultiplierIgnoreReceiveCooldownReductionFlag_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CsbCooldownMultiplierIgnoreReceiveCooldownReductionFlag_003Ek__BackingField = value;
		}
	} = 1f;

	public float sbCooldownOffset
	{
		[CompilerGenerated]
		get
		{
			return _003CsbCooldownOffset_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CsbCooldownOffset_003Ek__BackingField = value;
		}
	} = 1f;

	public float sbCooldownOffsetIgnoreReceiveCooldownReductionFlag
	{
		[CompilerGenerated]
		get
		{
			return _003CsbCooldownOffsetIgnoreReceiveCooldownReductionFlag_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CsbCooldownOffsetIgnoreReceiveCooldownReductionFlag_003Ek__BackingField = value;
		}
	} = 1f;

	public bool Network_003CskipStartAnimation_003Ek__BackingField
	{
		get
		{
			return skipStartAnimation;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref skipStartAnimation, 256uL, null);
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
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref handOwner, 512uL, OnHandOwnerChanged, ref ____003ChandOwner_003Ek__BackingFieldNetId);
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
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref tempOwner, 1024uL, OnTempOwnerChanged, ref ____003CtempOwner_003Ek__BackingFieldNetId);
		}
	}

	public HeroSkillLocation Network_003CskillType_003Ek__BackingField
	{
		get
		{
			return skillType;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref skillType, 2048uL, null);
		}
	}

	public uint Network_003CcharacterSkillOwner_003Ek__BackingField
	{
		get
		{
			return characterSkillOwner;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref characterSkillOwner, 4096uL, null);
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
			GeneratedSyncVarSetter(value, ref _level, 8192uL, OnLevelChange);
		}
	}

	public DescriptionTags Networktags
	{
		get
		{
			return tags;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref tags, 16384uL, null);
		}
	}

	public float Network_skillHasteGrantedByLevelMultiplier
	{
		get
		{
			return _skillHasteGrantedByLevelMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _skillHasteGrantedByLevelMultiplier, 32768uL, null);
		}
	}

	public Color Network_003CspecialOverlayColor_003Ek__BackingField
	{
		get
		{
			return specialOverlayColor;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref specialOverlayColor, 65536uL, null);
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
			GeneratedSyncVarSetter(value, ref dismantleProgress, 131072uL, OnDismantleProgressChanged);
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
			GeneratedSyncVarSetter(value, ref maxSellGold, 262144uL, null);
		}
	}

	public float Network_003CsbCooldownMultiplier_003Ek__BackingField
	{
		get
		{
			return sbCooldownMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref sbCooldownMultiplier, 524288uL, null);
		}
	}

	public float Network_003CsbCooldownMultiplierIgnoreReceiveCooldownReductionFlag_003Ek__BackingField
	{
		get
		{
			return sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag, 1048576uL, null);
		}
	}

	public float Network_003CsbCooldownOffset_003Ek__BackingField
	{
		get
		{
			return sbCooldownOffset;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref sbCooldownOffset, 2097152uL, null);
		}
	}

	public float Network_003CsbCooldownOffsetIgnoreReceiveCooldownReductionFlag_003Ek__BackingField
	{
		get
		{
			return sbCooldownOffsetIgnoreReceiveCooldownReductionFlag;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref sbCooldownOffsetIgnoreReceiveCooldownReductionFlag, 4194304uL, null);
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

	protected override void Awake()
	{
		base.Awake();
		ActorEvent_OnAbilityInstanceCreated += (Action<EventInfoAbilityInstance>)delegate(EventInfoAbilityInstance info)
		{
			if (!(owner == null))
			{
				HeroSkillLocation heroSkillLocation;
				if (owner.Skill.Q == this)
				{
					heroSkillLocation = HeroSkillLocation.Q;
				}
				else if (owner.Skill.W == this)
				{
					heroSkillLocation = HeroSkillLocation.W;
				}
				else if (owner.Skill.E == this)
				{
					heroSkillLocation = HeroSkillLocation.E;
				}
				else if (owner.Skill.R == this)
				{
					heroSkillLocation = HeroSkillLocation.R;
				}
				else if (owner.Skill.Identity == this)
				{
					heroSkillLocation = HeroSkillLocation.Identity;
				}
				else
				{
					if (!(owner.Skill.Movement == this))
					{
						return;
					}
					heroSkillLocation = HeroSkillLocation.Movement;
				}
				owner.HeroEvent_OnAbilityInstanceCreatedFromSkill?.Invoke(new EventInfoSkillAbilityInstance
				{
					type = heroSkillLocation,
					instance = info.instance,
					skill = this
				});
			}
		};
		ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)delegate(EventInfoAbilityInstance info)
		{
			if (!(owner == null))
			{
				HeroSkillLocation heroSkillLocation;
				if (owner.Skill.Q == this)
				{
					heroSkillLocation = HeroSkillLocation.Q;
				}
				else if (owner.Skill.W == this)
				{
					heroSkillLocation = HeroSkillLocation.W;
				}
				else if (owner.Skill.E == this)
				{
					heroSkillLocation = HeroSkillLocation.E;
				}
				else if (owner.Skill.R == this)
				{
					heroSkillLocation = HeroSkillLocation.R;
				}
				else if (owner.Skill.Identity == this)
				{
					heroSkillLocation = HeroSkillLocation.Identity;
				}
				else
				{
					if (!(owner.Skill.Movement == this))
					{
						return;
					}
					heroSkillLocation = HeroSkillLocation.Movement;
				}
				owner.HeroEvent_OnAbilityInstanceBeforePrepareFromSkill?.Invoke(new EventInfoSkillAbilityInstance
				{
					type = heroSkillLocation,
					instance = info.instance,
					skill = this
				});
			}
		};
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_skillHastePerLevelBonus = new SkillBonus();
		UpdateSkillHastePerLevelBonus();
		AddSkillBonus(_skillHastePerLevelBonus);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		GameObject prefab = Resources.Load<GameObject>("WorldModels/Skill World Model");
		_worldModel = UnityEngine.Object.Instantiate(prefab, base.position, base.rotation, base.transform).GetComponent<SkillWorldModel>();
	}

	private void UpdateSkillHastePerLevelBonus()
	{
		_skillHastePerLevelBonus.cooldownMultiplier = 1f / (1f + skillHastePerLevel * (float)Mathf.Clamp(level - 1, 0, int.MaxValue) * 0.01f);
	}

	public override float GetCooldownTimeMultiplier(int configIndex)
	{
		if (type == SkillType.Ultimate && configIndex == 0)
		{
			return 100f / configs[configIndex].cooldownTime;
		}
		float time = base.GetCooldownTimeMultiplier(configIndex);
		if (configs[configIndex].canReceiveCooldownReduction)
		{
			return time * sbCooldownMultiplier;
		}
		return time * sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag;
	}

	public override float GetCooldownTimeOffset(int configIndex)
	{
		float offset = base.GetCooldownTimeOffset(configIndex);
		if (configs[configIndex].canReceiveCooldownReduction)
		{
			return offset + sbCooldownOffset;
		}
		return offset + sbCooldownOffsetIgnoreReceiveCooldownReductionFlag;
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (base.isServer)
		{
			DoDismantleLogicUpdate();
			DoSkillBonusLogicUpdate();
		}
		if (showFillAmountOnScreen)
		{
			_ = base.fillAmount;
			_ = 0f;
		}
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance ins = base.OnCastComplete(configIndex, info);
		HeroSkillLocation type;
		if (owner.Skill.GetSkill(HeroSkillLocation.Q) == this)
		{
			type = HeroSkillLocation.Q;
		}
		else if (owner.Skill.GetSkill(HeroSkillLocation.W) == this)
		{
			type = HeroSkillLocation.W;
		}
		else if (owner.Skill.GetSkill(HeroSkillLocation.E) == this)
		{
			type = HeroSkillLocation.E;
		}
		else if (owner.Skill.GetSkill(HeroSkillLocation.R) == this)
		{
			type = HeroSkillLocation.R;
		}
		else if (owner.Skill.GetSkill(HeroSkillLocation.Identity) == this)
		{
			type = HeroSkillLocation.Identity;
		}
		else
		{
			if (!(owner.Skill.GetSkill(HeroSkillLocation.Movement) == this))
			{
				return ins;
			}
			type = HeroSkillLocation.Movement;
		}
		InvokeOnSkillUse(new EventInfoSkillUse
		{
			skill = this,
			type = type
		});
		return ins;
	}

	[ClientRpc]
	private void InvokeOnSkillUse(EventInfoSkillUse info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoSkillUse(writer, info);
		SendRPCInternal("System.Void SkillTrigger::InvokeOnSkillUse(EventInfoSkillUse)", 2077614875, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	protected virtual void OnLevelChange(int oldLevel, int newLevel)
	{
		if (newLevel < 1)
		{
			return;
		}
		if (base.isServer)
		{
			Network_003CmaxSellGold_003Ek__BackingField = int.MaxValue;
			UpdatePassiveEffectIfNeccessary();
			if (_skillHastePerLevelBonus != null)
			{
				UpdateSkillHastePerLevelBonus();
			}
			UpdateSkillBonus();
		}
		ClientSkillEvent_OnLevelChange?.Invoke(oldLevel, newLevel);
		if (owner != null)
		{
			owner.Skill.ClientHeroEvent_OnSkillLevelChanged?.Invoke(this, oldLevel, newLevel);
		}
	}

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (base.isServer)
		{
			Network_003CtempOwner_003Ek__BackingField = null;
			Network_003CskipStartAnimation_003Ek__BackingField = true;
			newOwner.Status.AddStatBonus(statBonus);
			if (isCharacterSkill)
			{
				Network_003CcharacterSkillOwner_003Ek__BackingField = newOwner.netId;
			}
			if (type == SkillType.Ultimate)
			{
				newOwner.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
				newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
				newOwner.ActorEvent_OnKill += new Action<EventInfoKill>(ActorEventOnKill);
				newOwner.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
			}
		}
		FxPlay(startEffect, newOwner);
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (base.isServer && formerOwner != null)
		{
			formerOwner.Status.RemoveStatBonus(statBonus);
			if (type == SkillType.Ultimate)
			{
				formerOwner.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
				formerOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
				formerOwner.ActorEvent_OnKill -= new Action<EventInfoKill>(ActorEventOnKill);
				formerOwner.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
			}
		}
		FxPlay(endEffect, formerOwner);
		FxStop(startEffect);
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		if (obj.victim is Monster && type == SkillType.Ultimate && !obj.victim.Status.hasDamageImmunity)
		{
			ChargeUltimate(0.2125f * obj.strength);
		}
	}

	private void ActorEventOnKill(EventInfoKill obj)
	{
		if (type == SkillType.Ultimate && obj.victim is Monster m)
		{
			switch (m.type)
			{
			case Monster.MonsterType.Lesser:
				ChargeUltimate(0.12750001f);
				break;
			case Monster.MonsterType.Normal:
				ChargeUltimate(0.63750005f);
				break;
			case Monster.MonsterType.MiniBoss:
				ChargeUltimate(12.75f);
				break;
			case Monster.MonsterType.Boss:
				ChargeUltimate(42.5f);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (type == SkillType.Ultimate && obj.victim is Monster m && !obj.victim.Status.hasDamageImmunity)
		{
			float percentage = ((obj.actor is ElementalStatusEffect) ? 0.0425f : 0.2125f);
			float ratio = obj.damage.amount / m.maxHealth;
			ChargeUltimate(m.type switch
			{
				Monster.MonsterType.Lesser => percentage + ratio * 0.12750001f, 
				Monster.MonsterType.Normal => percentage + ratio * 0.25500003f, 
				Monster.MonsterType.MiniBoss => percentage + ratio * 12.75f, 
				Monster.MonsterType.Boss => percentage + ratio * 25.5f, 
				_ => throw new ArgumentOutOfRangeException(), 
			});
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (type == SkillType.Ultimate)
		{
			float percentage = ((obj.actor is ElementalStatusEffect) ? 0.0425f : 0.2125f);
			percentage += 25.5f * obj.damage.amount / owner.maxHealth;
			ChargeUltimate(percentage);
		}
	}

	[Server]
	private void ChargeUltimate(float percentage)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::ChargeUltimate(System.Single)' called when server was not active");
		}
		else
		{
			if (type != SkillType.Ultimate)
			{
				return;
			}
			percentage /= sbCooldownMultiplier;
			percentage /= 1f / (1f + owner.Status.abilityHaste * 0.01f);
			int i = 0;
			if (!configs[i].canReceiveCooldownReduction || IsConfigLocked(i))
			{
				return;
			}
			float currentAmount = percentage * (configs[i].cooldownTime + sbCooldownOffset) / 100f;
			while (currentAmount > 0f && currentCharges[i] < configs[i].maxCharges)
			{
				if (currentCharges[i] == 0)
				{
					currentUnscaledCooldownTimes[i] -= currentAmount;
					if (currentUnscaledCooldownTimes[i] < 0f)
					{
						currentUnscaledCooldownTimes[i] = 0f;
					}
					break;
				}
				currentUnscaledCooldownTimes[i] -= currentAmount;
				currentAmount = 0f;
				if (currentUnscaledCooldownTimes[i] < 0f)
				{
					currentAmount = 0f - currentUnscaledCooldownTimes[i];
					currentCharges[i] = Mathf.Min(currentCharges[i] + configs[i].addedCharges, configs[i].maxCharges);
					if (i == base.currentConfigIndex)
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
			RpcSetCooldownTime(i, currentUnscaledCooldownTimes[i]);
			RpcSetCharge(i, currentCharges[i]);
		}
	}

	public override string GetActorReadableName()
	{
		return $"{base.GetActorReadableName()} ({level})";
	}

	[Server]
	public void RpcSetPositionAndRotation(Vector3 pos, Quaternion rot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::RpcSetPositionAndRotation(UnityEngine.Vector3,UnityEngine.Quaternion)' called when server was not active");
			return;
		}
		base.transform.SetPositionAndRotation(pos, rot);
		RpcSetPositionAndRotation_Imp(pos, rot);
	}

	[ClientRpc]
	private void RpcSetPositionAndRotation_Imp(Vector3 pos, Quaternion rot)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(pos);
		writer.WriteQuaternion(rot);
		SendRPCInternal("System.Void SkillTrigger::RpcSetPositionAndRotation_Imp(UnityEngine.Vector3,UnityEngine.Quaternion)", 1362066538, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public bool CanInteract(Entity entity)
	{
		if (owner == null && Network_003ChandOwner_003Ek__BackingField == null && !_worldModel.isAnimating && Time.time - base.creationTime > 0.5f)
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
			return;
		}
		if (isCharacterSkill && characterSkillOwner != 0 && characterSkillOwner != hero.netId)
		{
			if (entity.isOwned)
			{
				InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_CanOnlyEquipYourOwnCharacterSkill");
			}
			return;
		}
		if (rarity == Rarity.Identity)
		{
			if (!entity.isOwned || !(entity is Hero h))
			{
				return;
			}
			string msg = string.Format(DewLocalization.GetUIValue("InGame_Message_ConfirmEmbraceNewIdentity"), h.Skill.Identity.GetFormattedSkillTitle(), GetFormattedSkillTitle());
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				owner = this,
				rawContent = msg,
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
				defaultButton = DewMessageSettings.ButtonType.No,
				destructiveConfirm = true,
				onClose = delegate(DewMessageSettings.ButtonType b)
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						CmdEquipIdentitySkill();
					}
				},
				validator = () => !this.IsNullOrInactive() && InGameUIManager.ValidateInGameActionMessage()
			});
			return;
		}
		HeroSkillLocation? emptySkill = null;
		if (isCharacterSkill && GetType().Name.StartsWith("St_R_") && hero.Skill.R == null)
		{
			emptySkill = HeroSkillLocation.R;
		}
		else if (hero.Skill.Q == null)
		{
			emptySkill = HeroSkillLocation.Q;
		}
		else if (hero.Skill.W == null)
		{
			emptySkill = HeroSkillLocation.W;
		}
		else if (hero.Skill.E == null)
		{
			emptySkill = HeroSkillLocation.E;
		}
		else if (hero.Skill.R == null)
		{
			emptySkill = HeroSkillLocation.R;
		}
		if (emptySkill.HasValue)
		{
			if (base.isServer)
			{
				hero.Skill.EquipSkill(emptySkill.Value, this);
				hero.Skill.RpcInvokeOnSkillPickup(this);
			}
			if (entity.isOwned)
			{
				ManagerBase<FeedbackManager>.instance.PlayFeedbackEffect("UI_EditSkill_SkillEquip");
			}
		}
		else if (base.isServer)
		{
			hero.Skill.HoldInHand(this);
		}
	}

	private void OnDismantleProgressChanged(float oldVal, float newVal)
	{
		ClientSkillEvent_OnDismantleProgressChanged?.Invoke(oldVal, newVal);
		if (base.isServer && newVal >= 1f && owner == null && Network_003ChandOwner_003Ek__BackingField == null && base.isActive)
		{
			DismantleSkill();
		}
	}

	[Server]
	public void DismantleSkill()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::DismantleSkill()' called when server was not active");
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
		float floatAmount = NetworkedManagerBase<GameManager>.instance.gss.skillDismantleDreamDustByLevel.Evaluate(level);
		floatAmount *= NetworkedManagerBase<GameManager>.instance.gss.skillDismantleDreamDustMultiplier.Get(rarity);
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
		SendCommandInternal("System.Void SkillTrigger::CmdDoDismantleTap(Mirror.NetworkConnectionToClient)", -275473734, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	private void CmdEquipIdentitySkill(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void SkillTrigger::CmdEquipIdentitySkill(Mirror.NetworkConnectionToClient)", -289398032, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	private void DoDismantleLogicUpdate()
	{
		if ((dismantleProgress > 0f && Time.time - _lastDismantleTapTime > 1f) || owner != null || Network_003ChandOwner_003Ek__BackingField != null)
		{
			Network_003CdismantleProgress_003Ek__BackingField = 0f;
		}
	}

	public float GetCooldownTimeOnLevel(int lvl)
	{
		if (!configs[0].canReceiveCooldownReduction)
		{
			return Mathf.Max(0f, GetMaxCooldownTime(0, scaled: false));
		}
		Hero hero = null;
		hero = ((!(base.netIdentity != null) || !(owner != null)) ? DewPlayer.local.hero : owner);
		float abilHasteMult = ((hero != null) ? (1f / (1f + hero.Status.abilityHaste * 0.01f)) : 1f);
		float levelMult = 1f / (1f + skillHastePerLevel * (float)Mathf.Clamp(lvl - 1, 0, int.MaxValue) * 0.01f);
		float result = (configs[0].cooldownTime + sbCooldownOffset) * abilHasteMult * levelMult * sbCooldownMultiplier;
		result /= _skillHasteGrantedByLevelMultiplier;
		return Mathf.Max(0f, result);
	}

	public int GetBuyGold(DewPlayer player)
	{
		return GetBuyGold(player, rarity, level);
	}

	public int GetSellGold(DewPlayer player)
	{
		return Mathf.Min(GetSellGold(player, rarity, level), maxSellGold);
	}

	public static int GetBuyGold(DewPlayer player, Rarity rarity, int level)
	{
		float floatVal = GetPrice(rarity, level);
		if (player != null)
		{
			floatVal *= player.buyPriceMultiplier;
		}
		return Mathf.Max(Mathf.RoundToInt(floatVal), 1);
	}

	public static int GetSellGold(DewPlayer player, Rarity rarity, int level)
	{
		float floatVal = GetPrice(rarity, level);
		if (player != null)
		{
			floatVal *= player.sellPriceMultiplier;
		}
		floatVal *= NetworkedManagerBase<GameManager>.instance.gss.sellValueMultiplier;
		return Mathf.Max(Mathf.RoundToInt(floatVal), 1);
	}

	public static int GetPrice(Rarity rarity, int level)
	{
		DewGameSessionSettings gss = NetworkedManagerBase<GameManager>.instance.gss;
		return Mathf.Max(Mathf.RoundToInt((float)gss.skillValue.Get(rarity) * gss.valueGlobalMultiplier * (1f + (gss.valueMultiplierPerSkillLevel - 1f) * (float)(level - 1))), 1);
	}

	protected override bool ShouldTickCooldown(int configIndex)
	{
		if (type != SkillType.Ultimate || configIndex != 0)
		{
			return base.ShouldTickCooldown(configIndex);
		}
		return false;
	}

	public override bool ShouldBeSaved()
	{
		return owner == null;
	}

	public override void OnSaveActor(Dictionary<string, object> data)
	{
		base.OnSaveActor(data);
		data["level"] = level;
		data["characterSkillOwner"] = characterSkillOwner;
	}

	public override Actor OnLoadCreateActor(Dictionary<string, object> data, Actor parent)
	{
		return Dew.CreateSkillTrigger(this, (Vector3)data["pos"], (int)data["level"], null, delegate(SkillTrigger s)
		{
			s.Network_003CcharacterSkillOwner_003Ek__BackingField = (uint)data["characterSkillOwner"];
			s.Network_003CskipStartAnimation_003Ek__BackingField = true;
		});
	}

	public string GetFormattedSkillTitle()
	{
		string color = Dew.GetRarityColorHex(rarity);
		string skillName = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(GetType()), 0);
		return "<color=" + color + ">" + string.Format(DewLocalization.GetSkillLevelTemplate(level), skillName) + "</color>";
	}

	public DamageData DefaultDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Default, value, procCoefficient);
	}

	public DamageData PhysicalDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical, value, procCoefficient);
	}

	public DamageData MagicDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Magic, value, procCoefficient);
	}

	public DamageData PhysicalMagicDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical | DamageData.SourceType.Magic, value, procCoefficient);
	}

	public DamageData PureDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Pure, value, procCoefficient);
	}

	public DamageData Damage(ScalingValue value, float procCoefficient = 1f)
	{
		DamageData.SourceType type = DamageData.SourceType.Default;
		if (value.adFactor > 0f)
		{
			type |= DamageData.SourceType.Physical;
		}
		else if (value.apFactor > 0f)
		{
			type |= DamageData.SourceType.Magic;
		}
		return CreateDamage(type, value, procCoefficient);
	}

	public DamageData CreateDamage(DamageData.SourceType type, ScalingValue value, float procCoefficient = 1f)
	{
		return new DamageData(type, value, statEntity, effectiveLevel, procCoefficient).SetActor(this);
	}

	public HealData Heal(ScalingValue amount)
	{
		return new HealData(GetValue(amount)).SetActor(this);
	}

	public float GetValue(ScalingValue val)
	{
		return val.GetValue(effectiveLevel, statEntity);
	}

	[Server]
	public SkillBonus AddSkillBonus(SkillBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SkillBonus SkillTrigger::AddSkillBonus(SkillBonus)' called when server was not active");
			return null;
		}
		if (_grantedSkillBonus == null)
		{
			_grantedSkillBonus = new List<SkillBonus>();
		}
		if (bonus._parent != null)
		{
			Debug.LogWarning($"SkillBonus for {this} already has a parent");
			return bonus;
		}
		bonus._parent = this;
		_grantedSkillBonus.Add(bonus);
		UpdateSkillBonus();
		return bonus;
	}

	[Server]
	public void RemoveSkillBonus(SkillBonus bonus)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::RemoveSkillBonus(SkillBonus)' called when server was not active");
			return;
		}
		bonus._parent = null;
		_grantedSkillBonus.Remove(bonus);
		UpdateSkillBonus();
	}

	[Server]
	public void UpdateSkillBonus()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void SkillTrigger::UpdateSkillBonus()' called when server was not active");
			return;
		}
		Network_003CsbCooldownMultiplier_003Ek__BackingField = 1f;
		Network_003CsbCooldownMultiplierIgnoreReceiveCooldownReductionFlag_003Ek__BackingField = 1f;
		Network_003CsbCooldownOffset_003Ek__BackingField = 0f;
		Network_003CsbCooldownOffsetIgnoreReceiveCooldownReductionFlag_003Ek__BackingField = 0f;
		_isSkillBonusDirty = false;
		if (_grantedSkillBonus == null)
		{
			return;
		}
		if (!_mainConfigOriginalCharge.HasValue)
		{
			_mainConfigOriginalCharge = configs[0].maxCharges;
		}
		int maxCharges = _mainConfigOriginalCharge.Value;
		for (int i = 0; i < _grantedSkillBonus.Count; i++)
		{
			sbCooldownMultiplier *= _grantedSkillBonus[i].cooldownMultiplier;
			sbCooldownOffset += _grantedSkillBonus[i].cooldownOffset;
			if (_grantedSkillBonus[i].ignoreReceiveCooldownReductionFlag)
			{
				sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag *= _grantedSkillBonus[i].cooldownMultiplier;
				sbCooldownOffsetIgnoreReceiveCooldownReductionFlag += _grantedSkillBonus[i].cooldownOffset;
			}
			maxCharges += _grantedSkillBonus[i].addedCharge;
		}
		if (maxCharges != configs[0].maxCharges)
		{
			if (maxCharges > configs[0].maxCharges && base.currentConfigUnscaledCooldownTime == 0f)
			{
				SetCooldownTime(0, GetMaxCooldownTime(0, scaled: false), scaled: false);
			}
			configs[0].maxCharges = maxCharges;
		}
		Network_skillHasteGrantedByLevelMultiplier = ((_skillHastePerLevelBonus != null) ? _skillHastePerLevelBonus.cooldownMultiplier : 1f);
	}

	private void DoSkillBonusLogicUpdate()
	{
		if (_isSkillBonusDirty)
		{
			UpdateSkillBonus();
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_InvokeOnSkillUse__EventInfoSkillUse(EventInfoSkillUse info)
	{
		owner.ClientHeroEvent_OnSkillUse?.Invoke(info);
	}

	protected static void InvokeUserCode_InvokeOnSkillUse__EventInfoSkillUse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnSkillUse called on server.");
		}
		else
		{
			((SkillTrigger)obj).UserCode_InvokeOnSkillUse__EventInfoSkillUse(GeneratedNetworkCode._Read_EventInfoSkillUse(reader));
		}
	}

	protected void UserCode_RpcSetPositionAndRotation_Imp__Vector3__Quaternion(Vector3 pos, Quaternion rot)
	{
		if (!base.isServer)
		{
			base.transform.SetPositionAndRotation(pos, rot);
		}
	}

	protected static void InvokeUserCode_RpcSetPositionAndRotation_Imp__Vector3__Quaternion(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetPositionAndRotation_Imp called on server.");
		}
		else
		{
			((SkillTrigger)obj).UserCode_RpcSetPositionAndRotation_Imp__Vector3__Quaternion(reader.ReadVector3(), reader.ReadQuaternion());
		}
	}

	protected void UserCode_CmdDoDismantleTap__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if (!(owner != null) && !(Network_003ChandOwner_003Ek__BackingField != null) && !(Time.time - _lastDismantleTapTime < 0.075f) && !(sender.GetHero() == null))
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
			((SkillTrigger)obj).UserCode_CmdDoDismantleTap__NetworkConnectionToClient(senderConnection);
		}
	}

	protected void UserCode_CmdEquipIdentitySkill__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		if (owner != null || Network_003ChandOwner_003Ek__BackingField != null || isLocked)
		{
			return;
		}
		Hero hero = sender.GetHero();
		if (!hero.IsNullInactiveDeadOrKnockedOut())
		{
			hero.CreateAbilityInstance(base.position, null, new CastInfo(hero), delegate(Ai_EmbraceNewIdentity ai)
			{
				ai.NetworktargetSkill = this;
			});
		}
	}

	protected static void InvokeUserCode_CmdEquipIdentitySkill__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipIdentitySkill called on client.");
		}
		else
		{
			((SkillTrigger)obj).UserCode_CmdEquipIdentitySkill__NetworkConnectionToClient(senderConnection);
		}
	}

	static SkillTrigger()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(SkillTrigger), "System.Void SkillTrigger::CmdDoDismantleTap(Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdDoDismantleTap__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterCommand(typeof(SkillTrigger), "System.Void SkillTrigger::CmdEquipIdentitySkill(Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdEquipIdentitySkill__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(SkillTrigger), "System.Void SkillTrigger::InvokeOnSkillUse(EventInfoSkillUse)", InvokeUserCode_InvokeOnSkillUse__EventInfoSkillUse);
		RemoteProcedureCalls.RegisterRpc(typeof(SkillTrigger), "System.Void SkillTrigger::RpcSetPositionAndRotation_Imp(UnityEngine.Vector3,UnityEngine.Quaternion)", InvokeUserCode_RpcSetPositionAndRotation_Imp__Vector3__Quaternion);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(skipStartAnimation);
			writer.WriteNetworkBehaviour(Network_003ChandOwner_003Ek__BackingField);
			writer.WriteNetworkBehaviour(Network_003CtempOwner_003Ek__BackingField);
			GeneratedNetworkCode._Write_HeroSkillLocation(writer, skillType);
			writer.WriteUInt(characterSkillOwner);
			writer.WriteInt(_level);
			GeneratedNetworkCode._Write_DescriptionTags(writer, tags);
			writer.WriteFloat(_skillHasteGrantedByLevelMultiplier);
			writer.WriteColor(specialOverlayColor);
			writer.WriteFloat(dismantleProgress);
			writer.WriteInt(maxSellGold);
			writer.WriteFloat(sbCooldownMultiplier);
			writer.WriteFloat(sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag);
			writer.WriteFloat(sbCooldownOffset);
			writer.WriteFloat(sbCooldownOffsetIgnoreReceiveCooldownReductionFlag);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteBool(skipStartAnimation);
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003ChandOwner_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003CtempOwner_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			GeneratedNetworkCode._Write_HeroSkillLocation(writer, skillType);
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			writer.WriteUInt(characterSkillOwner);
		}
		if ((base.syncVarDirtyBits & 0x2000L) != 0L)
		{
			writer.WriteInt(_level);
		}
		if ((base.syncVarDirtyBits & 0x4000L) != 0L)
		{
			GeneratedNetworkCode._Write_DescriptionTags(writer, tags);
		}
		if ((base.syncVarDirtyBits & 0x8000L) != 0L)
		{
			writer.WriteFloat(_skillHasteGrantedByLevelMultiplier);
		}
		if ((base.syncVarDirtyBits & 0x10000L) != 0L)
		{
			writer.WriteColor(specialOverlayColor);
		}
		if ((base.syncVarDirtyBits & 0x20000L) != 0L)
		{
			writer.WriteFloat(dismantleProgress);
		}
		if ((base.syncVarDirtyBits & 0x40000L) != 0L)
		{
			writer.WriteInt(maxSellGold);
		}
		if ((base.syncVarDirtyBits & 0x80000L) != 0L)
		{
			writer.WriteFloat(sbCooldownMultiplier);
		}
		if ((base.syncVarDirtyBits & 0x100000L) != 0L)
		{
			writer.WriteFloat(sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag);
		}
		if ((base.syncVarDirtyBits & 0x200000L) != 0L)
		{
			writer.WriteFloat(sbCooldownOffset);
		}
		if ((base.syncVarDirtyBits & 0x400000L) != 0L)
		{
			writer.WriteFloat(sbCooldownOffsetIgnoreReceiveCooldownReductionFlag);
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
			GeneratedSyncVarDeserialize(ref skillType, null, GeneratedNetworkCode._Read_HeroSkillLocation(reader));
			GeneratedSyncVarDeserialize(ref characterSkillOwner, null, reader.ReadUInt());
			GeneratedSyncVarDeserialize(ref _level, OnLevelChange, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref tags, null, GeneratedNetworkCode._Read_DescriptionTags(reader));
			GeneratedSyncVarDeserialize(ref _skillHasteGrantedByLevelMultiplier, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref specialOverlayColor, null, reader.ReadColor());
			GeneratedSyncVarDeserialize(ref dismantleProgress, OnDismantleProgressChanged, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref maxSellGold, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref sbCooldownMultiplier, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref sbCooldownOffset, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref sbCooldownOffsetIgnoreReceiveCooldownReductionFlag, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref skipStartAnimation, null, reader.ReadBool());
		}
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref handOwner, OnHandOwnerChanged, reader, ref ____003ChandOwner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x400L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref tempOwner, OnTempOwnerChanged, reader, ref ____003CtempOwner_003Ek__BackingFieldNetId);
		}
		if ((num & 0x800L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref skillType, null, GeneratedNetworkCode._Read_HeroSkillLocation(reader));
		}
		if ((num & 0x1000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref characterSkillOwner, null, reader.ReadUInt());
		}
		if ((num & 0x2000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _level, OnLevelChange, reader.ReadInt());
		}
		if ((num & 0x4000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref tags, null, GeneratedNetworkCode._Read_DescriptionTags(reader));
		}
		if ((num & 0x8000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _skillHasteGrantedByLevelMultiplier, null, reader.ReadFloat());
		}
		if ((num & 0x10000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref specialOverlayColor, null, reader.ReadColor());
		}
		if ((num & 0x20000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref dismantleProgress, OnDismantleProgressChanged, reader.ReadFloat());
		}
		if ((num & 0x40000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref maxSellGold, null, reader.ReadInt());
		}
		if ((num & 0x80000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref sbCooldownMultiplier, null, reader.ReadFloat());
		}
		if ((num & 0x100000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref sbCooldownMultiplierIgnoreReceiveCooldownReductionFlag, null, reader.ReadFloat());
		}
		if ((num & 0x200000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref sbCooldownOffset, null, reader.ReadFloat());
		}
		if ((num & 0x400000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref sbCooldownOffsetIgnoreReceiveCooldownReductionFlag, null, reader.ReadFloat());
		}
	}
}
