using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_BossSoul : Shrine, IShrineCustomName, IUpgradeSkillProvider, IUpgradeGemProvider
{
	[SyncVar]
	internal string _bossTypeName;

	private string _droppedItemTypeName;

	private float _droppedItemChance;

	public GameObject[] tintedObjects;

	public Color tint;

	public float upgradeStartHeight;

	public float rewardDelay;

	public GameObject fxExplode;

	private readonly SyncList<uint> _usedPlayerIds = new SyncList<uint>();

	private bool _startedExplodeRoutine;

	public string Network_bossTypeName
	{
		get
		{
			return _bossTypeName;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _bossTypeName, 256uL, null);
		}
	}

	protected override void OnCreate()
	{
		GameObject[] array = tintedObjects;
		for (int i = 0; i < array.Length; i++)
		{
			DewEffect.TintRecursively(array[i], tint);
		}
		base.OnCreate();
		FxStop(availableEffect);
		FxPlay(availableEffect);
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = base.position;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer)
		{
			CheckIfAllAlivePlayersUpgraded();
		}
	}

	[ClientRpc]
	private void RpcClearObjective()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Shrine_BossSoul::RpcClearObjective()", 2094961658, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public override bool CanInteract(Entity entity)
	{
		if (base.CanInteract(entity) && entity.owner != null)
		{
			return !_usedPlayerIds.Contains(entity.owner.netId);
		}
		return false;
	}

	protected override bool OnUse(Entity entity)
	{
		if (entity.owner == null || !entity.owner.isHumanPlayer)
		{
			return false;
		}
		if (_usedPlayerIds.Contains(entity.owner.netId))
		{
			return false;
		}
		TpcShowUpgrade(entity.owner);
		return false;
	}

	[TargetRpc]
	private void TpcShowUpgrade(NetworkConnectionToClient target)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(target, "System.Void Shrine_BossSoul::TpcShowUpgrade(Mirror.NetworkConnectionToClient)", 2006915022, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	private void CheckIfAllAlivePlayersUpgraded()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine_BossSoul::CheckIfAllAlivePlayersUpgraded()' called when server was not active");
		}
		else
		{
			if (!base.isActive || _startedExplodeRoutine)
			{
				return;
			}
			foreach (DewPlayer h in DewPlayer.humanPlayers)
			{
				if (!h.hero.IsNullInactiveDeadOrKnockedOut() && !_usedPlayerIds.Contains(h.netId))
				{
					return;
				}
			}
			Explode();
		}
	}

	[Server]
	private void Explode()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine_BossSoul::Explode()' called when server was not active");
			return;
		}
		_startedExplodeRoutine = true;
		RpcClearObjective();
		Vector3 soulPos = base.position;
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (rewardDelay > 0.0001f)
			{
				yield return new WaitForSeconds(rewardDelay);
			}
			FxPlayNetworked(fxExplode);
			DewGameSessionSettings gss = NetworkedManagerBase<GameManager>.instance.gss;
			int zoneIndex = NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex;
			int gold = DewMath.RandomRoundToInt(UnityEngine.Random.Range(gss.bossRewardsGoldMin.Evaluate(zoneIndex), gss.bossRewardsGoldMax.Evaluate(zoneIndex)));
			int dreamDust = DewMath.RandomRoundToInt(UnityEngine.Random.Range(gss.bossRewardsDreamDustMin.Evaluate(zoneIndex), gss.bossRewardsDreamDustMax.Evaluate(zoneIndex)));
			int stardust = UnityEngine.Random.Range(gss.stardustBossSoulAmount.x, gss.stardustBossSoulAmount.y + 1);
			stardust += NetworkedManagerBase<GameManager>.instance.difficulty.gainedStardustAmountOffset;
			foreach (DewPlayer p in DewPlayer.humanPlayers)
			{
				if (!p.hero.IsNullInactiveDeadOrKnockedOut())
				{
					NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: true, isGivenByOtherPlayer: false, gold, soulPos, p.hero);
					NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, dreamDust, soulPos, p.hero);
				}
			}
			yield return new WaitForSeconds(0.5f);
			NetworkedManagerBase<PickupManager>.instance.DropStarDust(stardust, soulPos);
			yield return new WaitForSeconds(0.3f);
			if (!string.IsNullOrEmpty(_droppedItemTypeName) && (Dew.IsSkillIncludedInGame(_droppedItemTypeName) || Dew.IsGemIncludedInGame(_droppedItemTypeName)))
			{
				UnityEngine.Object item = DewResources.GetByShortTypeName(_droppedItemTypeName);
				foreach (DewPlayer h in DewPlayer.humanPlayers)
				{
					if (!h.hero.IsNullInactiveDeadOrKnockedOut() && !(UnityEngine.Random.value > _droppedItemChance))
					{
						Vector3 pos = Dew.GetGoodRewardPosition(soulPos + (h.hero.position - soulPos).normalized * 3f, 1.25f);
						if (item is SkillTrigger st)
						{
							NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Skill>().SelectSkillAndLevel(st.rarity, out var _, out var level);
							Dew.CreateSkillTrigger(st, pos, level, h);
						}
						else if (item is Gem g)
						{
							int quality = NetworkedManagerBase<LootManager>.instance.GetLootInstance<Loot_Gem>().SelectQuality(g.rarity);
							Dew.CreateGem(g, pos, quality, h);
						}
					}
				}
			}
			yield return new WaitForSeconds(0.4f);
			Rift[] allRifts = UnityEngine.Object.FindObjectsOfType<Rift>();
			Rift[] array = allRifts;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].isLocked = false;
				yield return new WaitForSeconds(0.25f);
			}
			Destroy();
		}
	}

	public string GetRawName()
	{
		return string.Format(DewLocalization.GetUIValue("Shrine_BossSoul_Name"), DewLocalization.GetUIValue(_bossTypeName + "_Name"));
	}

	public void SetGemReward<T>(float chance) where T : Gem
	{
		_droppedItemTypeName = typeof(T).Name;
		_droppedItemChance = chance;
	}

	public void SetSkillReward<T>(float chance) where T : SkillTrigger
	{
		_droppedItemTypeName = typeof(T).Name;
		_droppedItemChance = chance;
	}

	public int GetDreamDustCost(SkillTrigger target)
	{
		return 0;
	}

	public int GetAddedLevel()
	{
		return DewResources.GetByType<Ai_RandomGemUpgrade>().addedLevel;
	}

	public int GetDreamDustCost(Gem target)
	{
		return 0;
	}

	public int GetAddedQuality()
	{
		return DewResources.GetByType<Ai_RandomGemUpgrade>().addedQuality;
	}

	public bool RequestSkillUpgrade(SkillTrigger target)
	{
		CmdUpgrade(target);
		return true;
	}

	public bool RequestGemUpgrade(Gem target)
	{
		CmdUpgrade(target);
		return true;
	}

	[Command(requiresAuthority = false)]
	private void CmdUpgrade(NetworkBehaviour target, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(target);
		SendCommandInternal("System.Void Shrine_BossSoul::CmdUpgrade(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", -670062422, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	public Shrine_BossSoul()
	{
		InitSyncObject(_usedPlayerIds);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcClearObjective()
	{
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
	}

	protected static void InvokeUserCode_RpcClearObjective(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearObjective called on server.");
		}
		else
		{
			((Shrine_BossSoul)obj).UserCode_RpcClearObjective();
		}
	}

	protected void UserCode_TpcShowUpgrade__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<FloatingWindowManager>.instance.ClearTarget();
		ManagerBase<EditSkillManager>.instance.StartUpgrade(this, once: true);
	}

	protected static void InvokeUserCode_TpcShowUpgrade__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcShowUpgrade called on server.");
		}
		else
		{
			((Shrine_BossSoul)obj).UserCode_TpcShowUpgrade__NetworkConnectionToClient((NetworkConnectionToClient)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour target, NetworkConnectionToClient sender)
	{
		DewPlayer p = sender.GetPlayer();
		if (p == null || _usedPlayerIds.Contains(p.netId))
		{
			return;
		}
		try
		{
			SkillTrigger skill = target as SkillTrigger;
			if ((object)skill != null)
			{
				if (skill.owner.owner != p || !skill.isLevelUpEnabled)
				{
					return;
				}
				CreateAbilityInstance(base.position + Vector3.up * upgradeStartHeight, null, new CastInfo(null, p.hero), delegate(Ai_RandomGemUpgrade ai)
				{
					ai.upgradeTarget = skill;
				});
				_usedPlayerIds.Add(p.netId);
			}
			else
			{
				Gem gem = target as Gem;
				if ((object)gem != null)
				{
					if (gem.owner.owner != p)
					{
						return;
					}
					CreateAbilityInstance(base.position + Vector3.up * upgradeStartHeight, null, new CastInfo(null, p.hero), delegate(Ai_RandomGemUpgrade ai)
					{
						ai.upgradeTarget = gem;
					});
					_usedPlayerIds.Add(p.netId);
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		CheckIfAllAlivePlayersUpgraded();
	}

	protected static void InvokeUserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpgrade called on client.");
		}
		else
		{
			((Shrine_BossSoul)obj).UserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient(reader.ReadNetworkBehaviour(), senderConnection);
		}
	}

	static Shrine_BossSoul()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_BossSoul), "System.Void Shrine_BossSoul::CmdUpgrade(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_BossSoul), "System.Void Shrine_BossSoul::RpcClearObjective()", InvokeUserCode_RpcClearObjective);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_BossSoul), "System.Void Shrine_BossSoul::TpcShowUpgrade(Mirror.NetworkConnectionToClient)", InvokeUserCode_TpcShowUpgrade__NetworkConnectionToClient);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteString(_bossTypeName);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteString(_bossTypeName);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _bossTypeName, null, reader.ReadString());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _bossTypeName, null, reader.ReadString());
		}
	}
}
