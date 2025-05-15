using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class HeroSkill : HeroComponent, ICleanup
{
	private const float UnequipMaxDistance = 1.5f;

	public SafeAction<SkillTrigger> ClientHeroEvent_OnSkillEquip;

	public SafeAction<SkillTrigger> ClientHeroEvent_OnSkillUnequip;

	public SafeAction<SkillTrigger> ClientHeroEvent_OnSkillPickup;

	public SafeAction<SkillTrigger> ClientHeroEvent_OnSkillDrop;

	public SafeAction<HeroSkillLocation, HeroSkillLocation> ClientHeroEvent_OnSkillSwap;

	public SafeAction<SkillTrigger, int, int> ClientHeroEvent_OnSkillLevelChanged;

	public SkillTrigger[] loadoutQ;

	public SkillTrigger[] loadoutR;

	public SkillTrigger[] loadoutTrait;

	public SkillTrigger[] loadoutMovement;

	[CompilerGenerated]
	[SyncVar(hook = "OnHoldingObjectChanged")]
	private IItem _003CholdingObject_003Ek__BackingField;

	private NetworkBehaviourDictionaryWrapper<GemLocation, Gem> _gems;

	private readonly SyncDictionary<GemLocation, SyncedNetworkBehaviour> _syncedGems = new SyncDictionary<GemLocation, SyncedNetworkBehaviour>();

	public SafeAction<Gem, int, int> ClientHeroEvent_OnGemQualityChanged;

	public SafeAction<Gem> ClientHeroEvent_OnGemEquip;

	public SafeAction<Gem> ClientHeroEvent_OnGemUnequip;

	public SafeAction<Gem> ClientHeroEvent_OnGemPickup;

	public SafeAction<Gem> ClientHeroEvent_OnGemDrop;

	public SafeAction<GemLocation, GemLocation> ClientHeroEvent_OnGemSwap;

	public SkillTrigger Q
	{
		get
		{
			if (!base.hero.Ability.abilities.TryGetValue(0, out var trg))
			{
				return null;
			}
			return trg as SkillTrigger;
		}
		set
		{
			base.hero.Ability.SetAbility(0, value);
		}
	}

	public SkillTrigger W
	{
		get
		{
			if (!base.hero.Ability.abilities.TryGetValue(1, out var trg))
			{
				return null;
			}
			return trg as SkillTrigger;
		}
		set
		{
			base.hero.Ability.SetAbility(1, value);
		}
	}

	public SkillTrigger E
	{
		get
		{
			if (!base.hero.Ability.abilities.TryGetValue(2, out var trg))
			{
				return null;
			}
			return trg as SkillTrigger;
		}
		set
		{
			base.hero.Ability.SetAbility(2, value);
		}
	}

	public SkillTrigger R
	{
		get
		{
			if (!base.hero.Ability.abilities.TryGetValue(3, out var trg))
			{
				return null;
			}
			return trg as SkillTrigger;
		}
		set
		{
			base.hero.Ability.SetAbility(3, value);
		}
	}

	public SkillTrigger Identity
	{
		get
		{
			if (!base.hero.Ability.abilities.TryGetValue(4, out var trg))
			{
				return null;
			}
			return trg as SkillTrigger;
		}
		set
		{
			base.hero.Ability.SetAbility(4, value);
		}
	}

	public SkillTrigger Movement
	{
		get
		{
			if (!base.hero.Ability.abilities.TryGetValue(5, out var trg))
			{
				return null;
			}
			return trg as SkillTrigger;
		}
		set
		{
			base.hero.Ability.SetAbility(5, value);
		}
	}

	public IItem holdingObject
	{
		[CompilerGenerated]
		get
		{
			return _003CholdingObject_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CholdingObject_003Ek__BackingField = value;
		}
	}

	bool ICleanup.canDestroy => true;

	public IReadOnlyDictionary<GemLocation, Gem> gems => _gems;

	public IItem Network_003CholdingObject_003Ek__BackingField
	{
		get
		{
			return holdingObject;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref holdingObject, 1uL, OnHoldingObjectChanged);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_gems = new NetworkBehaviourDictionaryWrapper<GemLocation, Gem>(_syncedGems);
	}

	public SkillTrigger[] GetLoadoutSkills(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => loadoutQ, 
			HeroSkillLocation.R => loadoutR, 
			HeroSkillLocation.Identity => loadoutTrait, 
			HeroSkillLocation.Movement => loadoutMovement, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		HandleLoadout(HeroSkillLocation.Q);
		HandleLoadout(HeroSkillLocation.R);
		HandleLoadout(HeroSkillLocation.Identity);
		HandleLoadout(HeroSkillLocation.Movement);
		void HandleLoadout(HeroSkillLocation type)
		{
			int loadoutIndex = base.hero.loadout.GetSkill(type);
			SkillTrigger[] skills = GetLoadoutSkills(type);
			if (skills != null && skills.Length != 0)
			{
				int i = Mathf.Clamp(loadoutIndex, 0, skills.Length);
				SkillTrigger trig = Dew.CreateSkillTrigger(skills[i], base.transform.position, 1);
				trig.skillType = type;
				base.hero.Ability.SetAbility((int)type, trig);
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		_syncedGems.Callback += OnGemChanged;
		foreach (KeyValuePair<GemLocation, SyncedNetworkBehaviour> pair in _syncedGems)
		{
			OnGemChanged(SyncIDictionary<GemLocation, SyncedNetworkBehaviour>.Operation.OP_ADD, pair.Key, pair.Value);
		}
	}

	public SkillTrigger GetSkill(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => Q, 
			HeroSkillLocation.W => W, 
			HeroSkillLocation.E => E, 
			HeroSkillLocation.R => R, 
			HeroSkillLocation.Identity => Identity, 
			HeroSkillLocation.Movement => Movement, 
			_ => null, 
		};
	}

	public bool TryGetSkill(HeroSkillLocation type, out SkillTrigger skill)
	{
		skill = null;
		switch (type)
		{
		case HeroSkillLocation.Q:
			skill = Q;
			break;
		case HeroSkillLocation.W:
			skill = W;
			break;
		case HeroSkillLocation.E:
			skill = E;
			break;
		case HeroSkillLocation.R:
			skill = R;
			break;
		case HeroSkillLocation.Identity:
			skill = Identity;
			break;
		case HeroSkillLocation.Movement:
			skill = Movement;
			break;
		}
		return skill != null;
	}

	[Server]
	public SkillTrigger UnequipSkill(HeroSkillLocation type, Vector3 position, bool ignoreCanReplace = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'SkillTrigger HeroSkill::UnequipSkill(HeroSkillLocation,UnityEngine.Vector3,System.Boolean)' called when server was not active");
			return null;
		}
		position = base.hero.position + Vector3.ClampMagnitude(position - base.hero.position, 1.5f);
		if (!ignoreCanReplace && !CanReplaceSkill(type))
		{
			return null;
		}
		if (!base.hero.Ability.abilities.ContainsKey((int)type))
		{
			return null;
		}
		foreach (Gem item in GetGemsInSkill(type))
		{
			item.skill = null;
			item.parentActor = base.hero;
		}
		SkillTrigger skill = (SkillTrigger)base.hero.Ability.RemoveAbility((int)type);
		Vector3 pos = Dew.GetValidAgentDestination_LinearSweep(base.hero.agentPosition, position);
		pos = Dew.GetPositionOnGround(pos);
		skill.RpcSetPositionAndRotation(pos, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		RpcInvokeOnSkillUnequip(skill);
		return skill;
	}

	public void CmdUnequipSkill(HeroSkillLocation type, Vector3 position)
	{
		ManagerBase<EditSkillManager>.instance.SetClientState_SetSkillSlot(type, null);
		CmdUnequipSkill_Internal(type, position);
	}

	[Command]
	private void CmdUnequipSkill_Internal(HeroSkillLocation type, Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroSkillLocation(writer, type);
		writer.WriteVector3(position);
		SendCommandInternal("System.Void HeroSkill::CmdUnequipSkill_Internal(HeroSkillLocation,UnityEngine.Vector3)", -1886966275, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void EquipSkill(HeroSkillLocation type, SkillTrigger skill, bool ignoreCanReplace = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::EquipSkill(HeroSkillLocation,SkillTrigger,System.Boolean)' called when server was not active");
		}
		else
		{
			if ((!ignoreCanReplace && !CanReplaceSkill(type)) || skill.IsNullOrInactive() || skill.owner != null || (skill.handOwner != null && skill.handOwner != base.hero) || (skill.isCharacterSkill && skill.characterSkillOwner != 0 && skill.characterSkillOwner != base.hero.netId))
			{
				return;
			}
			if (holdingObject == skill)
			{
				Network_003CholdingObject_003Ek__BackingField = null;
			}
			if (base.hero.Ability.abilities.ContainsKey((int)type))
			{
				UnequipSkill(type, base.hero.position, ignoreCanReplace);
			}
			skill.handOwner = null;
			skill.skillType = type;
			base.hero.Ability.SetAbility((int)type, skill);
			foreach (Gem item in GetGemsInSkill(type))
			{
				item.skill = skill;
				item.parentActor = skill;
			}
			RpcInvokeOnSkillEquip(skill);
		}
	}

	public void CmdEquipSkill(HeroSkillLocation type, SkillTrigger skill)
	{
		ManagerBase<EditSkillManager>.instance.SetClientState_SetSkillSlot(type, skill);
		CmdEquipSkill_Internal(type, skill);
	}

	[Command]
	private void CmdEquipSkill_Internal(HeroSkillLocation type, SkillTrigger skill)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroSkillLocation(writer, type);
		writer.WriteNetworkBehaviour(skill);
		SendCommandInternal("System.Void HeroSkill::CmdEquipSkill_Internal(HeroSkillLocation,SkillTrigger)", -101434386, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public void CmdSwapSlotSkill(HeroSkillLocation a, HeroSkillLocation b)
	{
		SkillTrigger sa = GetSkill(a);
		SkillTrigger sb = GetSkill(b);
		ManagerBase<EditSkillManager>.instance.SetClientState_SetSkillSlot(a, sb);
		ManagerBase<EditSkillManager>.instance.SetClientState_SetSkillSlot(b, sa);
		CmdSwapSlotSkill_Internal(a, b);
	}

	[Command]
	private void CmdSwapSlotSkill_Internal(HeroSkillLocation a, HeroSkillLocation b)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroSkillLocation(writer, a);
		GeneratedNetworkCode._Write_HeroSkillLocation(writer, b);
		SendCommandInternal("System.Void HeroSkill::CmdSwapSlotSkill_Internal(HeroSkillLocation,HeroSkillLocation)", -36094078, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public bool TryGetSkillLocation(SkillTrigger skill, out HeroSkillLocation type)
	{
		if (GetSkill(HeroSkillLocation.Q) == skill)
		{
			type = HeroSkillLocation.Q;
			return true;
		}
		if (GetSkill(HeroSkillLocation.W) == skill)
		{
			type = HeroSkillLocation.W;
			return true;
		}
		if (GetSkill(HeroSkillLocation.E) == skill)
		{
			type = HeroSkillLocation.E;
			return true;
		}
		if (GetSkill(HeroSkillLocation.R) == skill)
		{
			type = HeroSkillLocation.R;
			return true;
		}
		if (GetSkill(HeroSkillLocation.Identity) == skill)
		{
			type = HeroSkillLocation.Identity;
			return true;
		}
		if (GetSkill(HeroSkillLocation.Movement) == skill)
		{
			type = HeroSkillLocation.Movement;
			return true;
		}
		type = HeroSkillLocation.Q;
		return false;
	}

	public void CmdMoveSkill(SkillTrigger skill, Vector3 position)
	{
		position = base.hero.position + Vector3.ClampMagnitude(position - base.hero.agentPosition, 1.5f);
		position = Dew.GetValidAgentDestination_LinearSweep(base.hero.agentPosition, position);
		position = Dew.GetPositionOnGround(position);
		skill.position = position;
		CmdMoveSkill_Internal(skill, position);
	}

	[Command]
	private void CmdMoveSkill_Internal(SkillTrigger skill, Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(skill);
		writer.WriteVector3(position);
		SendCommandInternal("System.Void HeroSkill::CmdMoveSkill_Internal(SkillTrigger,UnityEngine.Vector3)", -1687722992, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public bool CanReplaceSkill(HeroSkillLocation type)
	{
		if (base.entity.Ability.IsAbilityEditLocked((int)type))
		{
			return false;
		}
		if (!(GetSkill(type) == null))
		{
			if (type != HeroSkillLocation.Identity)
			{
				return type != HeroSkillLocation.Movement;
			}
			return false;
		}
		return true;
	}

	[ClientRpc]
	private void RpcInvokeOnSkillEquip(SkillTrigger skill)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(skill);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillEquip(SkillTrigger)", 438388514, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeOnSkillUnequip(SkillTrigger skill)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(skill);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillUnequip(SkillTrigger)", 1228677609, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void RpcInvokeOnSkillPickup(SkillTrigger skill)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(skill);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillPickup(SkillTrigger)", -889243458, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void RpcInvokeOnSkillDrop(SkillTrigger skill)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(skill);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillDrop(SkillTrigger)", -352525839, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void RpcInvokeOnSkillSwap(HeroSkillLocation a, HeroSkillLocation b)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroSkillLocation(writer, a);
		GeneratedNetworkCode._Write_HeroSkillLocation(writer, b);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnSkillSwap(HeroSkillLocation,HeroSkillLocation)", 832442570, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void HoldInHand(IItem holdable)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::HoldInHand(IItem)' called when server was not active");
		}
		else if (!(holdable.handOwner != null) && !(holdable.owner != null) && (!(holdable is SkillTrigger { isCharacterSkill: not false, characterSkillOwner: not 0u } st0) || st0.characterSkillOwner == base.hero.netId))
		{
			if (!holdingObject.IsHoldableObjectNullOrInactive())
			{
				StopHoldInHand();
			}
			Network_003CholdingObject_003Ek__BackingField = holdable;
			if (holdable is SkillTrigger st1)
			{
				RpcInvokeOnSkillPickup(st1);
			}
			if (holdable is Gem g)
			{
				RpcInvokeOnGemPickup(g);
			}
		}
	}

	public void CmdStopHoldInHand()
	{
		CmdStopHoldInHand_Internal();
	}

	[Command]
	private void CmdStopHoldInHand_Internal()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void HeroSkill::CmdStopHoldInHand_Internal()", -792606367, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	private void StopHoldInHand()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::StopHoldInHand()' called when server was not active");
			return;
		}
		if (holdingObject.IsHoldableObjectNullOrInactive())
		{
			Network_003CholdingObject_003Ek__BackingField = null;
			return;
		}
		if ((holdingObject.owner != null && holdingObject.owner != base.hero) || holdingObject.handOwner != base.hero)
		{
			Network_003CholdingObject_003Ek__BackingField = null;
			return;
		}
		Vector3 pos = Dew.GetValidAgentDestination_LinearSweep(base.hero.agentPosition, base.hero.agentPosition + UnityEngine.Random.insideUnitSphere.Flattened() * 1.5f);
		pos = Dew.GetPositionOnGround(pos);
		if (holdingObject is SkillTrigger s)
		{
			if (s.owner == null)
			{
				s.RpcSetPositionAndRotation(pos, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
			}
			s.handOwner = null;
		}
		else if (holdingObject is Gem g)
		{
			if (g.owner == null)
			{
				g.RpcSetPositionAndRotation(pos, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
			}
			g.handOwner = null;
		}
		if (holdingObject is SkillTrigger st)
		{
			RpcInvokeOnSkillDrop(st);
		}
		if (holdingObject is Gem gem)
		{
			RpcInvokeOnGemDrop(gem);
		}
		Network_003CholdingObject_003Ek__BackingField = null;
	}

	private void OnHoldingObjectChanged(IItem oldObject, IItem newObject)
	{
		if (base.isServer)
		{
			if (oldObject != null)
			{
				oldObject.handOwner = null;
				oldObject.tempOwner = null;
			}
			if (newObject != null)
			{
				newObject.handOwner = base.hero;
			}
		}
		if (base.isOwned)
		{
			if (newObject is SkillTrigger s)
			{
				ManagerBase<EditSkillManager>.instance.StartEquipSkill(s);
			}
			if (newObject is Gem g)
			{
				ManagerBase<EditSkillManager>.instance.StartEquipGem(g);
			}
		}
	}

	void ICleanup.OnCleanup()
	{
		foreach (Gem item in new List<Gem>(gems.Values))
		{
			Dew.Destroy(item.gameObject);
		}
	}

	private void OnGemChanged(SyncIDictionary<GemLocation, SyncedNetworkBehaviour>.Operation op, GemLocation key, SyncedNetworkBehaviour item)
	{
		if (!(DewPlayer.local == null) && !(base.hero != DewPlayer.local.hero))
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnLocalHeroGemChanged?.Invoke(base.hero, key);
		}
	}

	[Server]
	public void EquipGem(GemLocation loc, Gem gem)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::EquipGem(GemLocation,Gem)' called when server was not active");
			return;
		}
		if (loc.index >= GetMaxGemCount(loc.skill) || loc.index < 0)
		{
			throw new ArgumentException("Slot index out of bounds");
		}
		if (gem == null)
		{
			throw new ArgumentNullException("gem");
		}
		if (TryGetEquippedGemOfSameType(gem.GetType(), out var _, out var _))
		{
			throw new InvalidOperationException("Tried to equip more than one of same type of gem");
		}
		if (gem.owner != null || gem.skill != null || (gem.handOwner != null && gem.handOwner != base.hero))
		{
			throw new InvalidOperationException("Gem already belongs to someone else");
		}
		if (gems.ContainsKey(loc))
		{
			throw new InvalidOperationException("Another gem already equipped to specified slot");
		}
		if (holdingObject == gem)
		{
			Network_003CholdingObject_003Ek__BackingField = null;
		}
		gem.owner = base.hero;
		if (TryGetSkill(loc.skill, out var skill))
		{
			gem.skill = skill;
			gem.parentActor = skill;
		}
		else
		{
			gem.parentActor = base.hero;
		}
		_gems.Add(loc, gem);
		gem.RpcSetPositionAndRotation(base.hero.position, Quaternion.identity);
		RpcInvokeOnGemEquip(gem);
	}

	public void CmdEquipGem(GemLocation loc, Gem gem)
	{
		if (!ManagerBase<ControlManager>.instance.gemLocationConstraint.HasValue || ManagerBase<ControlManager>.instance.gemLocationConstraint.Value == loc.skill)
		{
			ManagerBase<EditSkillManager>.instance.SetClientState_SetGemSlot(loc, gem);
			CmdEquipGem_Internal(loc, gem);
		}
	}

	[Command]
	private void CmdEquipGem_Internal(GemLocation loc, Gem gem)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GemLocation(writer, loc);
		writer.WriteNetworkBehaviour(gem);
		SendCommandInternal("System.Void HeroSkill::CmdEquipGem_Internal(GemLocation,Gem)", -1094464876, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public Gem UnequipGem(GemLocation loc, Vector3 position)
	{
		position = base.hero.agentPosition + Vector3.ClampMagnitude(position - base.hero.agentPosition, 1.5f);
		Gem gem = gems[loc];
		_gems.Remove(loc);
		gem.skill = null;
		gem.owner = null;
		gem.parentActor = null;
		Vector3 pos = Dew.GetValidAgentDestination_LinearSweep(base.hero.agentPosition, position);
		pos = Dew.GetPositionOnGround(pos);
		gem.RpcSetPositionAndRotation(pos, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		RpcInvokeOnGemUnequip(gem);
		return gem;
	}

	public bool TryGetGemLocation(Gem gem, out GemLocation location)
	{
		foreach (KeyValuePair<GemLocation, Gem> pair in gems)
		{
			if (pair.Value == gem)
			{
				location = pair.Key;
				return true;
			}
		}
		location = default(GemLocation);
		return false;
	}

	[Server]
	public void MergeGem(Gem victim, Gem receivingGem)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HeroSkill::MergeGem(Gem,Gem)' called when server was not active");
			return;
		}
		if (victim == null)
		{
			throw new ArgumentNullException("victim");
		}
		if (receivingGem == null)
		{
			throw new ArgumentNullException("receivingGem");
		}
		if (victim.owner != null)
		{
			throw new InvalidOperationException("Victim gem is equipped");
		}
		if (receivingGem.owner != base.hero)
		{
			throw new InvalidOperationException("Receiving gem is not owned");
		}
		if (victim.GetType() != receivingGem.GetType())
		{
			throw new InvalidOperationException("Merge gem type is different");
		}
		receivingGem.quality = Gem.GetMergedQuality(victim.quality, receivingGem.quality);
		victim.Destroy();
		NetworkedManagerBase<ClientEventManager>.instance.InvokeOnGemMergeUpgraded(base.hero, receivingGem);
	}

	public void CmdUnequipGem(GemLocation loc, Vector3 position)
	{
		ManagerBase<EditSkillManager>.instance.SetClientState_SetGemSlot(loc, null);
		CmdUnequipGem_Internal(loc, position);
	}

	[Command]
	private void CmdUnequipGem_Internal(GemLocation loc, Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GemLocation(writer, loc);
		writer.WriteVector3(position);
		SendCommandInternal("System.Void HeroSkill::CmdUnequipGem_Internal(GemLocation,UnityEngine.Vector3)", 896427363, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public void CmdMoveGem(Gem gem, Vector3 position)
	{
		position = base.hero.agentPosition + Vector3.ClampMagnitude(position - base.hero.agentPosition, 1.5f);
		position = Dew.GetValidAgentDestination_LinearSweep(base.hero.agentPosition, position);
		position = Dew.GetPositionOnGround(position);
		gem.position = position;
		CmdMoveGem_Internal(gem, position);
	}

	[Command]
	private void CmdMoveGem_Internal(Gem gem, Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(gem);
		writer.WriteVector3(position);
		SendCommandInternal("System.Void HeroSkill::CmdMoveGem_Internal(Gem,UnityEngine.Vector3)", -794189872, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public bool TryGetEquippedGemOfSameType(Type type, out GemLocation loc, out Gem gem)
	{
		foreach (KeyValuePair<GemLocation, Gem> p in gems)
		{
			if (p.Value.GetType() == type)
			{
				loc = p.Key;
				gem = p.Value;
				return true;
			}
		}
		loc = default(GemLocation);
		gem = null;
		return false;
	}

	public int GetMaxGemCount(HeroSkillLocation type)
	{
		return AttrCustomizeManager.GetMaxGemCount(type);
	}

	public int GetCurrentGemCount(HeroSkillLocation type)
	{
		int count = 0;
		foreach (Gem item in GetGemsInSkill(type))
		{
			_ = item;
			count++;
		}
		return count;
	}

	public int GetEmptyGemSlot(HeroSkillLocation type)
	{
		int rBound = GetMaxGemCount(type);
		if (rBound <= 0)
		{
			return -1;
		}
		for (int i = 0; i < rBound; i++)
		{
			if (!gems.ContainsKey(new GemLocation
			{
				index = i,
				skill = type
			}))
			{
				return i;
			}
		}
		return -1;
	}

	public Gem GetFirstGem(HeroSkillLocation type)
	{
		if (GetMaxGemCount(type) <= 0)
		{
			return null;
		}
		GemLocation targetLoc = default(GemLocation);
		Gem target = null;
		foreach (KeyValuePair<GemLocation, Gem> p in GetGemsPairInSkill(type))
		{
			if (target == null || targetLoc.index > p.Key.index)
			{
				targetLoc = p.Key;
				target = p.Value;
			}
		}
		return target;
	}

	public Gem GetGem(GemLocation loc)
	{
		gems.TryGetValue(loc, out var gem);
		return gem;
	}

	public bool TryGetGem(GemLocation loc, out Gem gem)
	{
		return gems.TryGetValue(loc, out gem);
	}

	public IEnumerable<Gem> GetGemsInSkill(HeroSkillLocation type)
	{
		if (GetMaxGemCount(type) <= 0)
		{
			yield break;
		}
		foreach (KeyValuePair<GemLocation, Gem> pair in gems)
		{
			if (pair.Key.skill == type)
			{
				yield return pair.Value;
			}
		}
	}

	public IEnumerable<KeyValuePair<GemLocation, Gem>> GetGemsPairInSkill(HeroSkillLocation type)
	{
		foreach (KeyValuePair<GemLocation, Gem> pair in gems)
		{
			if (pair.Key.skill == type)
			{
				yield return pair;
			}
		}
	}

	public void CmdSwapSlotGem(GemLocation a, GemLocation b)
	{
		Gem ga = GetGem(a);
		Gem gb = GetGem(b);
		ManagerBase<EditSkillManager>.instance.SetClientState_SetGemSlot(a, gb);
		ManagerBase<EditSkillManager>.instance.SetClientState_SetGemSlot(b, ga);
		CmdSwapSlotGem_Internal(a, b);
	}

	[Command]
	private void CmdSwapSlotGem_Internal(GemLocation a, GemLocation b)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GemLocation(writer, a);
		GeneratedNetworkCode._Write_GemLocation(writer, b);
		SendCommandInternal("System.Void HeroSkill::CmdSwapSlotGem_Internal(GemLocation,GemLocation)", -330993968, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeOnGemEquip(Gem gem)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(gem);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemEquip(Gem)", 279202744, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeOnGemUnequip(Gem gem)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(gem);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemUnequip(Gem)", 1260293521, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void RpcInvokeOnGemPickup(Gem gem)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(gem);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemPickup(Gem)", -1409720352, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void RpcInvokeOnGemDrop(Gem gem)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(gem);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemDrop(Gem)", 724416973, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void RpcInvokeOnGemSwap(GemLocation a, GemLocation b)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GemLocation(writer, a);
		GeneratedNetworkCode._Write_GemLocation(writer, b);
		SendRPCInternal("System.Void HeroSkill::RpcInvokeOnGemSwap(GemLocation,GemLocation)", -1010508132, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public HeroSkill()
	{
		InitSyncObject(_syncedGems);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdUnequipSkill_Internal__HeroSkillLocation__Vector3(HeroSkillLocation type, Vector3 position)
	{
		try
		{
			SkillTrigger skill = UnequipSkill(type, position);
			if (skill != null)
			{
				RpcInvokeOnSkillDrop(skill);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdUnequipSkill_Internal__HeroSkillLocation__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUnequipSkill_Internal called on client.");
		}
		else
		{
			((HeroSkill)obj).UserCode_CmdUnequipSkill_Internal__HeroSkillLocation__Vector3(GeneratedNetworkCode._Read_HeroSkillLocation(reader), reader.ReadVector3());
		}
	}

	protected void UserCode_CmdEquipSkill_Internal__HeroSkillLocation__SkillTrigger(HeroSkillLocation type, SkillTrigger skill)
	{
		try
		{
			EquipSkill(type, skill);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdEquipSkill_Internal__HeroSkillLocation__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipSkill_Internal called on client.");
		}
		else
		{
			((HeroSkill)obj).UserCode_CmdEquipSkill_Internal__HeroSkillLocation__SkillTrigger(GeneratedNetworkCode._Read_HeroSkillLocation(reader), reader.ReadNetworkBehaviour<SkillTrigger>());
		}
	}

	protected void UserCode_CmdSwapSlotSkill_Internal__HeroSkillLocation__HeroSkillLocation(HeroSkillLocation a, HeroSkillLocation b)
	{
		try
		{
			SkillTrigger sa = GetSkill(a);
			SkillTrigger sb = GetSkill(b);
			if (sa != null)
			{
				UnequipSkill(a, base.hero.position);
			}
			if (sb != null)
			{
				UnequipSkill(b, base.hero.position);
			}
			if (sa != null)
			{
				EquipSkill(b, sa);
			}
			if (sb != null)
			{
				EquipSkill(a, sb);
			}
			RpcInvokeOnSkillSwap(a, b);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdSwapSlotSkill_Internal__HeroSkillLocation__HeroSkillLocation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwapSlotSkill_Internal called on client.");
		}
		else
		{
			((HeroSkill)obj).UserCode_CmdSwapSlotSkill_Internal__HeroSkillLocation__HeroSkillLocation(GeneratedNetworkCode._Read_HeroSkillLocation(reader), GeneratedNetworkCode._Read_HeroSkillLocation(reader));
		}
	}

	protected void UserCode_CmdMoveSkill_Internal__SkillTrigger__Vector3(SkillTrigger skill, Vector3 position)
	{
		if (!(skill.owner != null))
		{
			position = base.hero.agentPosition + Vector3.ClampMagnitude(position - base.hero.agentPosition, 1.5f);
			position = Dew.GetValidAgentDestination_LinearSweep(base.hero.agentPosition, position);
			position = Dew.GetPositionOnGround(position);
			skill.RpcSetPositionAndRotation(position, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		}
	}

	protected static void InvokeUserCode_CmdMoveSkill_Internal__SkillTrigger__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMoveSkill_Internal called on client.");
		}
		else
		{
			((HeroSkill)obj).UserCode_CmdMoveSkill_Internal__SkillTrigger__Vector3(reader.ReadNetworkBehaviour<SkillTrigger>(), reader.ReadVector3());
		}
	}

	protected void UserCode_RpcInvokeOnSkillEquip__SkillTrigger(SkillTrigger skill)
	{
		if (!(skill == null))
		{
			ClientHeroEvent_OnSkillEquip?.Invoke(skill);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillEquip__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillEquip called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnSkillEquip__SkillTrigger(reader.ReadNetworkBehaviour<SkillTrigger>());
		}
	}

	protected void UserCode_RpcInvokeOnSkillUnequip__SkillTrigger(SkillTrigger skill)
	{
		if (!(skill == null))
		{
			ClientHeroEvent_OnSkillUnequip?.Invoke(skill);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillUnequip__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillUnequip called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnSkillUnequip__SkillTrigger(reader.ReadNetworkBehaviour<SkillTrigger>());
		}
	}

	protected void UserCode_RpcInvokeOnSkillPickup__SkillTrigger(SkillTrigger skill)
	{
		if (!(skill == null))
		{
			ClientHeroEvent_OnSkillPickup?.Invoke(skill);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillPickup__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillPickup called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnSkillPickup__SkillTrigger(reader.ReadNetworkBehaviour<SkillTrigger>());
		}
	}

	protected void UserCode_RpcInvokeOnSkillDrop__SkillTrigger(SkillTrigger skill)
	{
		if (!(skill == null))
		{
			ClientHeroEvent_OnSkillDrop?.Invoke(skill);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillDrop__SkillTrigger(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillDrop called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnSkillDrop__SkillTrigger(reader.ReadNetworkBehaviour<SkillTrigger>());
		}
	}

	protected void UserCode_RpcInvokeOnSkillSwap__HeroSkillLocation__HeroSkillLocation(HeroSkillLocation a, HeroSkillLocation b)
	{
		ClientHeroEvent_OnSkillSwap?.Invoke(a, b);
	}

	protected static void InvokeUserCode_RpcInvokeOnSkillSwap__HeroSkillLocation__HeroSkillLocation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSkillSwap called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnSkillSwap__HeroSkillLocation__HeroSkillLocation(GeneratedNetworkCode._Read_HeroSkillLocation(reader), GeneratedNetworkCode._Read_HeroSkillLocation(reader));
		}
	}

	protected void UserCode_CmdStopHoldInHand_Internal()
	{
		try
		{
			StopHoldInHand();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdStopHoldInHand_Internal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopHoldInHand_Internal called on client.");
		}
		else
		{
			((HeroSkill)obj).UserCode_CmdStopHoldInHand_Internal();
		}
	}

	protected void UserCode_CmdEquipGem_Internal__GemLocation__Gem(GemLocation loc, Gem gem)
	{
		try
		{
			EquipGem(loc, gem);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdEquipGem_Internal__GemLocation__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipGem_Internal called on client.");
		}
		else
		{
			((HeroSkill)obj).UserCode_CmdEquipGem_Internal__GemLocation__Gem(GeneratedNetworkCode._Read_GemLocation(reader), reader.ReadNetworkBehaviour<Gem>());
		}
	}

	protected void UserCode_CmdUnequipGem_Internal__GemLocation__Vector3(GemLocation loc, Vector3 position)
	{
		try
		{
			Gem gem = UnequipGem(loc, position);
			RpcInvokeOnGemDrop(gem);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdUnequipGem_Internal__GemLocation__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUnequipGem_Internal called on client.");
		}
		else
		{
			((HeroSkill)obj).UserCode_CmdUnequipGem_Internal__GemLocation__Vector3(GeneratedNetworkCode._Read_GemLocation(reader), reader.ReadVector3());
		}
	}

	protected void UserCode_CmdMoveGem_Internal__Gem__Vector3(Gem gem, Vector3 position)
	{
		if (!(gem.owner != null))
		{
			position = base.hero.agentPosition + Vector3.ClampMagnitude(position - base.hero.agentPosition, 1.5f);
			position = Dew.GetValidAgentDestination_LinearSweep(base.hero.agentPosition, position);
			position = Dew.GetPositionOnGround(position);
			gem.RpcSetPositionAndRotation(position, ManagerBase<CameraManager>.instance.entityCamAngleRotation);
		}
	}

	protected static void InvokeUserCode_CmdMoveGem_Internal__Gem__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMoveGem_Internal called on client.");
		}
		else
		{
			((HeroSkill)obj).UserCode_CmdMoveGem_Internal__Gem__Vector3(reader.ReadNetworkBehaviour<Gem>(), reader.ReadVector3());
		}
	}

	protected void UserCode_CmdSwapSlotGem_Internal__GemLocation__GemLocation(GemLocation a, GemLocation b)
	{
		try
		{
			Gem ga = GetGem(a);
			Gem gb = GetGem(b);
			if (ga != null)
			{
				UnequipGem(a, base.hero.position);
			}
			if (gb != null)
			{
				UnequipGem(b, base.hero.position);
			}
			if (ga != null)
			{
				EquipGem(b, ga);
			}
			if (gb != null)
			{
				EquipGem(a, gb);
			}
			RpcInvokeOnGemSwap(a, b);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdSwapSlotGem_Internal__GemLocation__GemLocation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwapSlotGem_Internal called on client.");
		}
		else
		{
			((HeroSkill)obj).UserCode_CmdSwapSlotGem_Internal__GemLocation__GemLocation(GeneratedNetworkCode._Read_GemLocation(reader), GeneratedNetworkCode._Read_GemLocation(reader));
		}
	}

	protected void UserCode_RpcInvokeOnGemEquip__Gem(Gem gem)
	{
		if (!(gem == null))
		{
			ClientHeroEvent_OnGemEquip?.Invoke(gem);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnGemEquip__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemEquip called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnGemEquip__Gem(reader.ReadNetworkBehaviour<Gem>());
		}
	}

	protected void UserCode_RpcInvokeOnGemUnequip__Gem(Gem gem)
	{
		if (!(gem == null))
		{
			ClientHeroEvent_OnGemUnequip?.Invoke(gem);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnGemUnequip__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemUnequip called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnGemUnequip__Gem(reader.ReadNetworkBehaviour<Gem>());
		}
	}

	protected void UserCode_RpcInvokeOnGemPickup__Gem(Gem gem)
	{
		if (!(gem == null))
		{
			ClientHeroEvent_OnGemPickup?.Invoke(gem);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnGemPickup__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemPickup called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnGemPickup__Gem(reader.ReadNetworkBehaviour<Gem>());
		}
	}

	protected void UserCode_RpcInvokeOnGemDrop__Gem(Gem gem)
	{
		if (!(gem == null))
		{
			ClientHeroEvent_OnGemDrop?.Invoke(gem);
		}
	}

	protected static void InvokeUserCode_RpcInvokeOnGemDrop__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemDrop called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnGemDrop__Gem(reader.ReadNetworkBehaviour<Gem>());
		}
	}

	protected void UserCode_RpcInvokeOnGemSwap__GemLocation__GemLocation(GemLocation a, GemLocation b)
	{
		ClientHeroEvent_OnGemSwap?.Invoke(a, b);
	}

	protected static void InvokeUserCode_RpcInvokeOnGemSwap__GemLocation__GemLocation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGemSwap called on server.");
		}
		else
		{
			((HeroSkill)obj).UserCode_RpcInvokeOnGemSwap__GemLocation__GemLocation(GeneratedNetworkCode._Read_GemLocation(reader), GeneratedNetworkCode._Read_GemLocation(reader));
		}
	}

	static HeroSkill()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdUnequipSkill_Internal(HeroSkillLocation,UnityEngine.Vector3)", InvokeUserCode_CmdUnequipSkill_Internal__HeroSkillLocation__Vector3, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdEquipSkill_Internal(HeroSkillLocation,SkillTrigger)", InvokeUserCode_CmdEquipSkill_Internal__HeroSkillLocation__SkillTrigger, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdSwapSlotSkill_Internal(HeroSkillLocation,HeroSkillLocation)", InvokeUserCode_CmdSwapSlotSkill_Internal__HeroSkillLocation__HeroSkillLocation, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdMoveSkill_Internal(SkillTrigger,UnityEngine.Vector3)", InvokeUserCode_CmdMoveSkill_Internal__SkillTrigger__Vector3, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdStopHoldInHand_Internal()", InvokeUserCode_CmdStopHoldInHand_Internal, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdEquipGem_Internal(GemLocation,Gem)", InvokeUserCode_CmdEquipGem_Internal__GemLocation__Gem, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdUnequipGem_Internal(GemLocation,UnityEngine.Vector3)", InvokeUserCode_CmdUnequipGem_Internal__GemLocation__Vector3, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdMoveGem_Internal(Gem,UnityEngine.Vector3)", InvokeUserCode_CmdMoveGem_Internal__Gem__Vector3, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(HeroSkill), "System.Void HeroSkill::CmdSwapSlotGem_Internal(GemLocation,GemLocation)", InvokeUserCode_CmdSwapSlotGem_Internal__GemLocation__GemLocation, requiresAuthority: true);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillEquip(SkillTrigger)", InvokeUserCode_RpcInvokeOnSkillEquip__SkillTrigger);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillUnequip(SkillTrigger)", InvokeUserCode_RpcInvokeOnSkillUnequip__SkillTrigger);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillPickup(SkillTrigger)", InvokeUserCode_RpcInvokeOnSkillPickup__SkillTrigger);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillDrop(SkillTrigger)", InvokeUserCode_RpcInvokeOnSkillDrop__SkillTrigger);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnSkillSwap(HeroSkillLocation,HeroSkillLocation)", InvokeUserCode_RpcInvokeOnSkillSwap__HeroSkillLocation__HeroSkillLocation);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemEquip(Gem)", InvokeUserCode_RpcInvokeOnGemEquip__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemUnequip(Gem)", InvokeUserCode_RpcInvokeOnGemUnequip__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemPickup(Gem)", InvokeUserCode_RpcInvokeOnGemPickup__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemDrop(Gem)", InvokeUserCode_RpcInvokeOnGemDrop__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(HeroSkill), "System.Void HeroSkill::RpcInvokeOnGemSwap(GemLocation,GemLocation)", InvokeUserCode_RpcInvokeOnGemSwap__GemLocation__GemLocation);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteIHoldableInHand(holdingObject);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteIHoldableInHand(holdingObject);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref holdingObject, OnHoldingObjectChanged, reader.ReadIHoldableInHand());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref holdingObject, OnHoldingObjectChanged, reader.ReadIHoldableInHand());
		}
	}
}
