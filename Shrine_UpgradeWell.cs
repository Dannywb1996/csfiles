using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_UpgradeWell : Shrine, IUpgradeGemProvider, IUpgradeSkillProvider
{
	public GameObject openEffect;

	public GameObject upgradeEffect;

	protected override bool OnUse(Entity entity)
	{
		RpcStartUpgrade(entity.owner.connectionToClient);
		FxPlayNewNetworked(openEffect);
		return false;
	}

	[TargetRpc]
	private void RpcStartUpgrade(NetworkConnectionToClient target)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(target, "System.Void Shrine_UpgradeWell::RpcStartUpgrade(Mirror.NetworkConnectionToClient)", 2144469099, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	private void CmdUpgrade(NetworkBehaviour target, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(target);
		SendCommandInternal("System.Void Shrine_UpgradeWell::CmdUpgrade(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", -1786943484, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	public int GetDreamDustCost(Gem target)
	{
		return NetworkedManagerBase<GameManager>.instance.GetGemUpgradeDreamDustCost(target);
	}

	public int GetAddedQuality()
	{
		return NetworkedManagerBase<GameManager>.instance.GetGemUpgradeAddedQuality();
	}

	public int GetDreamDustCost(SkillTrigger target)
	{
		return NetworkedManagerBase<GameManager>.instance.GetSkillUpgradeDreamDustCost(target);
	}

	public int GetAddedLevel()
	{
		return 1;
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

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcStartUpgrade__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<EditSkillManager>.instance.StartUpgrade(this, once: false);
	}

	protected static void InvokeUserCode_RpcStartUpgrade__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC RpcStartUpgrade called on server.");
		}
		else
		{
			((Shrine_UpgradeWell)obj).UserCode_RpcStartUpgrade__NetworkConnectionToClient((NetworkConnectionToClient)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour target, NetworkConnectionToClient sender)
	{
		DewPlayer activator = sender.GetPlayer();
		try
		{
			if (target is SkillTrigger skill)
			{
				if (skill.owner.owner != activator || !skill.isLevelUpEnabled)
				{
					return;
				}
				int cost = GetDreamDustCost(skill);
				if (activator.dreamDust < cost)
				{
					return;
				}
				activator.SpendDreamDust(cost);
				skill.level += GetAddedLevel();
				NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(activator.hero, target);
			}
			else if (target is Gem gem)
			{
				if (gem.owner.owner != activator)
				{
					return;
				}
				int cost2 = GetDreamDustCost(gem);
				if (activator.dreamDust < cost2)
				{
					return;
				}
				activator.SpendDreamDust(cost2);
				gem.quality += GetAddedQuality();
				NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(activator.hero, target);
			}
			FxPlayNewNetworked(upgradeEffect);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
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
			((Shrine_UpgradeWell)obj).UserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient(reader.ReadNetworkBehaviour(), senderConnection);
		}
	}

	static Shrine_UpgradeWell()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_UpgradeWell), "System.Void Shrine_UpgradeWell::CmdUpgrade(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdUpgrade__NetworkBehaviour__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_UpgradeWell), "System.Void Shrine_UpgradeWell::RpcStartUpgrade(Mirror.NetworkConnectionToClient)", InvokeUserCode_RpcStartUpgrade__NetworkConnectionToClient);
	}
}
