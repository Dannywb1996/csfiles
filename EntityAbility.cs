using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class EntityAbility : EntityComponent, ICleanup
{
	public const int AttackAbilityIndex = 63;

	[SyncVar]
	private AttackTrigger _originalAttackAbility;

	[SyncVar]
	private AbilityTrigger _overridenAttackAbility;

	internal readonly List<AbilityTrigger> _attackAbilityOverrides = new List<AbilityTrigger>();

	private NetworkBehaviourDictionaryWrapper<int, AbilityTrigger> _abilities;

	private readonly SyncDictionary<int, SyncedNetworkBehaviour> _synedAbilities = new SyncDictionary<int, SyncedNetworkBehaviour>();

	[FormerlySerializedAs("_attackAbilityPreset")]
	public AttackTrigger attackAbilityPreset;

	[SerializeField]
	private AbilityTrigger[] _abilityPreset;

	private List<AbilityLockHandle> _handles = new List<AbilityLockHandle>();

	[SyncVar]
	private ulong _abilityCastLockBitmap;

	[SyncVar]
	private ulong _abilityEditLockBitmap;

	[SyncVar]
	private ulong _showLockIconBitmap;

	protected NetworkBehaviourSyncVar ____originalAttackAbilityNetId;

	protected NetworkBehaviourSyncVar ____overridenAttackAbilityNetId;

	public AbilityTrigger attackAbility
	{
		get
		{
			if (!(overridenAttackAbility != null))
			{
				return originalAttackAbility;
			}
			return overridenAttackAbility;
		}
	}

	public AttackTrigger originalAttackAbility
	{
		get
		{
			if (!abilities.ContainsKey(63))
			{
				return null;
			}
			return abilities[63] as AttackTrigger;
		}
	}

	public AbilityTrigger overridenAttackAbility
	{
		get
		{
			return Network_overridenAttackAbility;
		}
		private set
		{
			Network_overridenAttackAbility = value;
		}
	}

	public IReadOnlyDictionary<int, AbilityTrigger> abilities => _abilities;

	public IReadOnlyList<AbilityTrigger> abilityPresets => _abilityPreset;

	bool ICleanup.canDestroy => true;

	public AttackTrigger Network_originalAttackAbility
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____originalAttackAbilityNetId, ref _originalAttackAbility);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _originalAttackAbility, 1uL, null, ref ____originalAttackAbilityNetId);
		}
	}

	public AbilityTrigger Network_overridenAttackAbility
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____overridenAttackAbilityNetId, ref _overridenAttackAbility);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _overridenAttackAbility, 2uL, null, ref ____overridenAttackAbilityNetId);
		}
	}

	public ulong Network_abilityCastLockBitmap
	{
		get
		{
			return _abilityCastLockBitmap;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _abilityCastLockBitmap, 4uL, null);
		}
	}

	public ulong Network_abilityEditLockBitmap
	{
		get
		{
			return _abilityEditLockBitmap;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _abilityEditLockBitmap, 8uL, null);
		}
	}

	public ulong Network_showLockIconBitmap
	{
		get
		{
			return _showLockIconBitmap;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _showLockIconBitmap, 16uL, null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_abilities = new NetworkBehaviourDictionaryWrapper<int, AbilityTrigger>(_synedAbilities);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (attackAbilityPreset != null)
		{
			SetAttackAbility(Dew.CreateAbilityTrigger(attackAbilityPreset));
		}
		if (_abilityPreset == null)
		{
			return;
		}
		for (int i = 0; i < _abilityPreset.Length; i++)
		{
			AbilityTrigger preset = _abilityPreset[i];
			if (!(preset == null))
			{
				AbilityTrigger trig = ((!(preset is SkillTrigger)) ? Dew.CreateAbilityTrigger(preset) : Dew.CreateSkillTrigger((SkillTrigger)preset, base.transform.position, 1));
				SetAbility(i, trig);
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		_synedAbilities.Callback += OnAbilityChanged;
		foreach (KeyValuePair<int, SyncedNetworkBehaviour> pair in _synedAbilities)
		{
			OnAbilityChanged(SyncIDictionary<int, SyncedNetworkBehaviour>.Operation.OP_ADD, pair.Key, pair.Value);
		}
	}

	private void OnAbilityChanged(SyncIDictionary<int, SyncedNetworkBehaviour>.Operation op, int key, SyncedNetworkBehaviour item)
	{
		HeroSkillLocation type;
		switch (key)
		{
		default:
			return;
		case 0:
			type = HeroSkillLocation.Q;
			break;
		case 1:
			type = HeroSkillLocation.W;
			break;
		case 2:
			type = HeroSkillLocation.E;
			break;
		case 3:
			type = HeroSkillLocation.R;
			break;
		case 4:
			type = HeroSkillLocation.Identity;
			break;
		case 5:
			type = HeroSkillLocation.Movement;
			break;
		}
		if (DewPlayer.local != null && base.entity == DewPlayer.local.hero)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnLocalHeroAbilityChanged?.Invoke((Hero)base.entity, type);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.entity.isSleeping && base.isServer)
		{
			if (_attackAbilityOverrides.Count == 0 && overridenAttackAbility != null)
			{
				overridenAttackAbility = null;
			}
			else if (_attackAbilityOverrides.Count > 0 && overridenAttackAbility != _attackAbilityOverrides[_attackAbilityOverrides.Count - 1])
			{
				overridenAttackAbility = _attackAbilityOverrides[_attackAbilityOverrides.Count - 1];
			}
		}
	}

	[Server]
	public void AddAbility(AbilityTrigger trigger)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::AddAbility(AbilityTrigger)' called when server was not active");
			return;
		}
		if (trigger == null)
		{
			throw new ArgumentNullException("trigger");
		}
		for (int i = 0; i < 128; i++)
		{
			if (!_abilities.ContainsKey(i))
			{
				SetAbility(i, trigger);
				return;
			}
		}
		throw new InvalidOperationException("Abilities full: " + trigger.GetActorReadableName() + " -> " + base.entity.GetActorReadableName());
	}

	[Server]
	public T AddAbility<T>() where T : AbilityTrigger
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T EntityAbility::AddAbility()' called when server was not active");
			return null;
		}
		T trigger = Dew.CreateAbilityTrigger<T>();
		if (trigger == null)
		{
			throw new ArgumentNullException("trigger");
		}
		for (int i = 0; i < 128; i++)
		{
			if (!_abilities.ContainsKey(i))
			{
				SetAbility(i, trigger);
				return trigger;
			}
		}
		throw new InvalidOperationException("Abilities full: " + trigger.GetActorReadableName() + " -> " + base.entity.GetActorReadableName());
	}

	[Server]
	public void SetAbility(int index, AbilityTrigger trigger)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::SetAbility(System.Int32,AbilityTrigger)' called when server was not active");
			return;
		}
		if (trigger == null)
		{
			throw new ArgumentNullException("trigger");
		}
		if (_abilities.ContainsKey(index))
		{
			RemoveAbility(index);
		}
		_abilities.Add(index, trigger);
		trigger.owner = base.entity;
		trigger.parentActor = base.entity;
		trigger.abilityIndex = index;
	}

	[Server]
	public AbilityTrigger RemoveAbility(int index)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityTrigger EntityAbility::RemoveAbility(System.Int32)' called when server was not active");
			return null;
		}
		if (!_abilities.TryGetValue(index, out var trg))
		{
			throw new Exception($"This entity({this}) has no ability at the given index {index}");
		}
		trg.owner = null;
		trg.parentActor = null;
		_abilities.Remove(index);
		return trg;
	}

	public T GetAbility<T>() where T : AbilityTrigger
	{
		foreach (KeyValuePair<int, AbilityTrigger> ability in _abilities)
		{
			AbilityTrigger trg = ability.Value;
			if (trg is T)
			{
				return (T)trg;
			}
		}
		return null;
	}

	public bool TryGetAbility<T>(out T trigger) where T : AbilityTrigger
	{
		foreach (KeyValuePair<int, AbilityTrigger> ability in _abilities)
		{
			AbilityTrigger trg = ability.Value;
			if (trg is T)
			{
				trigger = (T)trg;
				return true;
			}
		}
		trigger = null;
		return false;
	}

	[Server]
	public void SetAttackAbility(AttackTrigger trigger)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::SetAttackAbility(AttackTrigger)' called when server was not active");
		}
		else
		{
			SetAbility(63, trigger);
		}
	}

	[Server]
	public AbilityTrigger RemoveAttackAbility()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityTrigger EntityAbility::RemoveAttackAbility()' called when server was not active");
			return null;
		}
		return RemoveAbility(63);
	}

	void ICleanup.OnCleanup()
	{
		foreach (AbilityTrigger item in new List<AbilityTrigger>(_abilities.Values))
		{
			Dew.Destroy(item.gameObject);
		}
	}

	public bool IsAbilityCastLocked(int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (index > 64)
		{
			throw new InvalidOperationException("Ability index out of bounds");
		}
		if ((_abilityCastLockBitmap & (ulong)(1L << index)) != 0L)
		{
			if (abilities.ContainsKey(index))
			{
				return !abilities[index].currentConfig.ignoreAbilityLock;
			}
			return true;
		}
		return false;
	}

	public bool IsAbilityEditLocked(int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (index > 64)
		{
			throw new InvalidOperationException("Ability index out of bounds");
		}
		return (_abilityEditLockBitmap & (ulong)(1L << index)) != 0;
	}

	public bool ShouldShowAbilityLockIcon(int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (index > 64)
		{
			throw new InvalidOperationException("Ability index out of bounds");
		}
		return (_showLockIconBitmap & (ulong)(1L << index)) != 0;
	}

	public bool IsAttackAbilityLocked(int index)
	{
		return IsAbilityCastLocked(63);
	}

	[Server]
	public AbilityLockHandle GetNewAbilityLockHandle(bool shouldShowLockIcon = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityLockHandle EntityAbility::GetNewAbilityLockHandle(System.Boolean)' called when server was not active");
			return null;
		}
		AbilityLockHandle handle = new AbilityLockHandle();
		_handles.Add(handle);
		handle._parent = this;
		handle.shouldShowLockEffect = shouldShowLockIcon;
		return handle;
	}

	[Server]
	internal void UnlockAbility(AbilityLockHandle handle)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::UnlockAbility(AbilityLockHandle)' called when server was not active");
			return;
		}
		_handles.Remove(handle);
		CalculateAbilityLockBitmap();
	}

	[Server]
	internal void CalculateAbilityLockBitmap()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAbility::CalculateAbilityLockBitmap()' called when server was not active");
			return;
		}
		Network_abilityCastLockBitmap = 0uL;
		Network_abilityEditLockBitmap = 0uL;
		Network_showLockIconBitmap = 0uL;
		foreach (AbilityLockHandle h in _handles)
		{
			Network_abilityCastLockBitmap = _abilityCastLockBitmap | h._castLockBitmap;
			Network_abilityEditLockBitmap = _abilityEditLockBitmap | h._editLockBitmap;
			if (h.shouldShowLockEffect)
			{
				Network_showLockIconBitmap = _showLockIconBitmap | h._castLockBitmap;
			}
		}
	}

	public EntityAbility()
	{
		InitSyncObject(_synedAbilities);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(Network_originalAttackAbility);
			writer.WriteNetworkBehaviour(Network_overridenAttackAbility);
			writer.WriteULong(_abilityCastLockBitmap);
			writer.WriteULong(_abilityEditLockBitmap);
			writer.WriteULong(_showLockIconBitmap);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_originalAttackAbility);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_overridenAttackAbility);
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteULong(_abilityCastLockBitmap);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteULong(_abilityEditLockBitmap);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteULong(_showLockIconBitmap);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _originalAttackAbility, null, reader, ref ____originalAttackAbilityNetId);
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _overridenAttackAbility, null, reader, ref ____overridenAttackAbilityNetId);
			GeneratedSyncVarDeserialize(ref _abilityCastLockBitmap, null, reader.ReadULong());
			GeneratedSyncVarDeserialize(ref _abilityEditLockBitmap, null, reader.ReadULong());
			GeneratedSyncVarDeserialize(ref _showLockIconBitmap, null, reader.ReadULong());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _originalAttackAbility, null, reader, ref ____originalAttackAbilityNetId);
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _overridenAttackAbility, null, reader, ref ____overridenAttackAbilityNetId);
		}
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _abilityCastLockBitmap, null, reader.ReadULong());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _abilityEditLockBitmap, null, reader.ReadULong());
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _showLockIconBitmap, null, reader.ReadULong());
		}
	}
}
