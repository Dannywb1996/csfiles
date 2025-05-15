using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_Chaos : Shrine, IUpgradeGemProvider, IUpgradeSkillProvider
{
	public struct Ad_ChaosStats
	{
		public StatBonus bonus;
	}

	public int numOfChoice = 3;

	public ParticleSystem[] tintedPses;

	public Light[] tintedLights;

	public PerRarityData<float> chance;

	public PerRarityData<float> highChance;

	public PerRarityData<ChaosReward[]> poolByRarity;

	public GameObject destroyEffect;

	public GameObject useEffectOnHero;

	public GameObject useEffectOnLocalHero;

	private readonly SyncDictionary<DewPlayer, ChaosReward[]> _rewards = new SyncDictionary<DewPlayer, ChaosReward[]>();

	[NonSerialized]
	[SyncVar]
	public Rarity rarity;

	[NonSerialized]
	public DewPlayer[] playersOverride;

	public override bool canInteractWithMouse => true;

	public IReadOnlyDictionary<DewPlayer, ChaosReward[]> rewards => _rewards;

	public Rarity Networkrarity
	{
		get
		{
			return rarity;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref rarity, 256uL, null);
		}
	}

	public override void OnStartClient()
	{
		Color col = Dew.GetRarityColor(rarity);
		col = col.WithV(col.GetV() - 0.1f);
		ParticleSystem[] array = tintedPses;
		for (int i = 0; i < array.Length; i++)
		{
			DewEffect.TintObject(array[i], col);
		}
		Light[] array2 = tintedLights;
		for (int i = 0; i < array2.Length; i++)
		{
			DewEffect.TintObject(array2[i], col);
		}
		base.OnStartClient();
	}

	[Server]
	private void RemovePlayerAndCheckEmpty(DewPlayer player)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Shrine_Chaos::RemovePlayerAndCheckEmpty(DewPlayer)' called when server was not active");
			return;
		}
		_rewards.Remove(player);
		ListReturnHandle<Shrine_HeroSoul> handle;
		List<Shrine_HeroSoul> heroSouls = Dew.FindAllActorsOfType(out handle);
		DewPlayer[] array = _rewards.Keys.ToArray();
		foreach (DewPlayer p in array)
		{
			if (p.hero.IsNullInactiveDeadOrKnockedOut() && heroSouls.Find((Shrine_HeroSoul s) => s.targetHero == p.hero) == null)
			{
				_rewards.Remove(p);
			}
		}
		handle.Return();
		if (_rewards.Count <= 0)
		{
			FxPlayNetworked(destroyEffect);
			Destroy();
		}
	}

	[Server]
	private StatBonus GetBonusStat(Hero hero)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'StatBonus Shrine_Chaos::GetBonusStat(Hero)' called when server was not active");
			return null;
		}
		if (hero.TryGetData<Ad_ChaosStats>(out var data))
		{
			return data.bonus;
		}
		StatBonus bonus = new StatBonus();
		hero.AddData(new Ad_ChaosStats
		{
			bonus = bonus
		});
		hero.Status.AddStatBonus(bonus);
		return bonus;
	}

	public void SetRandomRarity(bool isHighQuality)
	{
		float val = UnityEngine.Random.value;
		PerRarityData<float> c = (isHighQuality ? highChance : chance);
		if (val < c.common)
		{
			Networkrarity = Rarity.Common;
		}
		else if (val < c.common + c.rare)
		{
			Networkrarity = Rarity.Rare;
		}
		else if (val < c.common + c.rare + c.epic)
		{
			Networkrarity = Rarity.Epic;
		}
		else
		{
			Networkrarity = Rarity.Legendary;
		}
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		_rewards.Clear();
		ChaosReward[] pool = poolByRarity.Get(rarity);
		foreach (DewPlayer h in DewPlayer.humanPlayers)
		{
			if (playersOverride == null || playersOverride.Contains(h))
			{
				List<int> numbers = new List<int>();
				for (int i = 0; i < pool.Length; i++)
				{
					numbers.Add(i);
				}
				numbers.Shuffle();
				ChaosReward[] availableRewards = new ChaosReward[numOfChoice];
				for (int j = 0; j < numOfChoice; j++)
				{
					availableRewards[j] = pool[numbers[j]];
					availableRewards[j].rarity = rarity;
				}
				_rewards.Add(h, availableRewards);
			}
		}
	}

	protected override bool OnUse(Entity entity)
	{
		TpcOpenChaos(entity.owner.connectionToClient);
		return false;
	}

	[TargetRpc]
	private void TpcOpenChaos(NetworkConnectionToClient target)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(target, "System.Void Shrine_Chaos::TpcOpenChaos(Mirror.NetworkConnectionToClient)", -1695430507, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdChoose(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(index);
		SendCommandInternal("System.Void Shrine_Chaos::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", -1112443512, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcShowCenterMessage(DewPlayer player, string message, string formatArg)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(player);
		writer.WriteString(message);
		writer.WriteString(formatArg);
		SendRPCInternal("System.Void Shrine_Chaos::RpcShowCenterMessage(DewPlayer,System.String,System.String)", 255448538, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcPlayLocalEffect(DewPlayer player)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(player);
		SendRPCInternal("System.Void Shrine_Chaos::RpcPlayLocalEffect(DewPlayer)", 1762722735, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	private void CmdUpgrade(NetworkBehaviour target, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(target);
		SendCommandInternal("System.Void Shrine_Chaos::CmdUpgrade(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", -1649447942, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	public override bool CanInteract(Entity entity)
	{
		if (entity.owner == null)
		{
			return false;
		}
		if (base.CanInteract(entity))
		{
			return _rewards.ContainsKey(entity.owner);
		}
		return false;
	}

	public override bool ShouldBeSaved()
	{
		return false;
	}

	public int GetDreamDustCost(Gem target)
	{
		return 0;
	}

	public int GetAddedQuality()
	{
		if (!_rewards.ContainsKey(DewPlayer.local))
		{
			return 0;
		}
		ChaosReward[] array = _rewards[DewPlayer.local];
		for (int i = 0; i < array.Length; i++)
		{
			ChaosReward r = array[i];
			if (r.type == ChaosRewardType.UpgradeGem)
			{
				return Mathf.RoundToInt(r.quantity);
			}
		}
		return 0;
	}

	public int GetDreamDustCost(SkillTrigger target)
	{
		return 0;
	}

	public int GetAddedLevel()
	{
		if (!_rewards.ContainsKey(DewPlayer.local))
		{
			return 0;
		}
		ChaosReward[] array = _rewards[DewPlayer.local];
		for (int i = 0; i < array.Length; i++)
		{
			ChaosReward r = array[i];
			if (r.type == ChaosRewardType.UpgradeSkill)
			{
				return Mathf.RoundToInt(r.quantity);
			}
		}
		return 0;
	}

	public bool RequestGemUpgrade(Gem target)
	{
		CmdUpgrade(target);
		return true;
	}

	public bool RequestSkillUpgrade(SkillTrigger target)
	{
		CmdUpgrade(target);
		return true;
	}

	public Shrine_Chaos()
	{
		InitSyncObject(_rewards);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcOpenChaos__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<FloatingWindowManager>.instance.SetTarget(this);
	}

	protected static void InvokeUserCode_TpcOpenChaos__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcOpenChaos called on server.");
		}
		else
		{
			((Shrine_Chaos)obj).UserCode_TpcOpenChaos__NetworkConnectionToClient((NetworkConnectionToClient)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdChoose__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		try
		{
			DewPlayer player = sender.GetPlayer();
			if (_rewards.ContainsKey(player))
			{
				ChaosReward r = _rewards[player][index];
				switch (r.type)
				{
				case ChaosRewardType.MaxHealth:
					GetBonusStat(player.hero).maxHealthFlat += r.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_MaxHealth", r.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.AttackDamage:
					GetBonusStat(player.hero).attackDamageFlat += r.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_AttackDamage", r.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.AttackSpeed:
					GetBonusStat(player.hero).attackSpeedPercentage += r.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_AttackSpeed", r.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.AbilityPower:
					GetBonusStat(player.hero).abilityPowerFlat += r.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_AbilityPower", r.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.AbilityHaste:
					GetBonusStat(player.hero).abilityHasteFlat += r.quantity;
					RpcShowCenterMessage(player, "InGame_Message_Chaos_AbilityHaste", r.quantity.ToString("#,##0"));
					break;
				case ChaosRewardType.UpgradeGem:
				case ChaosRewardType.UpgradeSkill:
					return;
				default:
					throw new ArgumentOutOfRangeException();
				}
				FxPlayNewNetworked(useEffectOnHero, player.hero);
				RpcPlayLocalEffect(player);
				RemovePlayerAndCheckEmpty(player);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	protected static void InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChoose called on client.");
		}
		else
		{
			((Shrine_Chaos)obj).UserCode_CmdChoose__Int32__NetworkConnectionToClient(reader.ReadInt(), senderConnection);
		}
	}

	protected void UserCode_RpcShowCenterMessage__DewPlayer__String__String(DewPlayer player, string message, string formatArg)
	{
		if (player.isLocalPlayer)
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.General, message, new object[1] { formatArg });
		}
	}

	protected static void InvokeUserCode_RpcShowCenterMessage__DewPlayer__String__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowCenterMessage called on server.");
		}
		else
		{
			((Shrine_Chaos)obj).UserCode_RpcShowCenterMessage__DewPlayer__String__String(reader.ReadNetworkBehaviour<DewPlayer>(), reader.ReadString(), reader.ReadString());
		}
	}

	protected void UserCode_RpcPlayLocalEffect__DewPlayer(DewPlayer player)
	{
		if (player.isLocalPlayer)
		{
			FxPlayNew(useEffectOnLocalHero, player.hero);
		}
	}

	protected static void InvokeUserCode_RpcPlayLocalEffect__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayLocalEffect called on server.");
		}
		else
		{
			((Shrine_Chaos)obj).UserCode_RpcPlayLocalEffect__DewPlayer(reader.ReadNetworkBehaviour<DewPlayer>());
		}
	}

	protected void UserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour target, NetworkConnectionToClient sender)
	{
		try
		{
			DewPlayer player = sender.GetPlayer();
			if (!_rewards.ContainsKey(player))
			{
				return;
			}
			ChaosReward[] playerReward = _rewards[player];
			for (int i = 0; i < playerReward.Length; i++)
			{
				ChaosReward r = playerReward[i];
				if (r.type == ChaosRewardType.UpgradeSkill && target is SkillTrigger skill)
				{
					if (!(skill.owner != player.hero))
					{
						skill.level += Mathf.RoundToInt(r.quantity);
						FxPlayNewNetworked(useEffectOnHero, player.hero);
						RpcPlayLocalEffect(player);
						RemovePlayerAndCheckEmpty(player);
						NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(player.hero, target);
					}
					break;
				}
				if (r.type == ChaosRewardType.UpgradeGem && target is Gem gem)
				{
					if (!(gem.owner != player.hero))
					{
						gem.quality += Mathf.RoundToInt(r.quantity);
						FxPlayNewNetworked(useEffectOnHero, player.hero);
						RpcPlayLocalEffect(player);
						RemovePlayerAndCheckEmpty(player);
						NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(player.hero, target);
					}
					break;
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	protected static void InvokeUserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpgrade called on client.");
		}
		else
		{
			((Shrine_Chaos)obj).UserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient(reader.ReadNetworkBehaviour(), senderConnection);
		}
	}

	static Shrine_Chaos()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_Chaos), "System.Void Shrine_Chaos::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_Chaos), "System.Void Shrine_Chaos::CmdUpgrade(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Chaos), "System.Void Shrine_Chaos::RpcShowCenterMessage(DewPlayer,System.String,System.String)", InvokeUserCode_RpcShowCenterMessage__DewPlayer__String__String);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Chaos), "System.Void Shrine_Chaos::RpcPlayLocalEffect(DewPlayer)", InvokeUserCode_RpcPlayLocalEffect__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_Chaos), "System.Void Shrine_Chaos::TpcOpenChaos(Mirror.NetworkConnectionToClient)", InvokeUserCode_TpcOpenChaos__NetworkConnectionToClient);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_Rarity(writer, rarity);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			GeneratedNetworkCode._Write_Rarity(writer, rarity);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref rarity, null, GeneratedNetworkCode._Read_Rarity(reader));
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref rarity, null, GeneratedNetworkCode._Read_Rarity(reader));
		}
	}
}
