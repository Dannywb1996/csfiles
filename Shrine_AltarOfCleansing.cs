using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Shrine_AltarOfCleansing : Shrine
{
	public GameObject openEffect;

	public GameObject cleanseEffect;

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
		SendTargetRPCInternal(target, "System.Void Shrine_AltarOfCleansing::RpcStartUpgrade(Mirror.NetworkConnectionToClient)", -650226466, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdCleanse(NetworkBehaviour target, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(target);
		SendCommandInternal("System.Void Shrine_AltarOfCleansing::CmdCleanse(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", -695271376, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcStartUpgrade__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<EditSkillManager>.instance.StartCleanse(this);
	}

	protected static void InvokeUserCode_RpcStartUpgrade__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC RpcStartUpgrade called on server.");
		}
		else
		{
			((Shrine_AltarOfCleansing)obj).UserCode_RpcStartUpgrade__NetworkConnectionToClient((NetworkConnectionToClient)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdCleanse__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour target, NetworkConnectionToClient sender)
	{
		DewPlayer activator = sender.GetPlayer();
		if (activator == null)
		{
			return;
		}
		if (target is SkillTrigger skill)
		{
			if (!(skill.owner.owner != activator) && !(activator.hero == null) && skill.level > NetworkedManagerBase<GameManager>.instance.GetCleanseSkillMinLevel())
			{
				int cost = NetworkedManagerBase<GameManager>.instance.GetCleanseGoldCost(skill);
				if (activator.gold >= cost)
				{
					activator.SpendGold(cost);
					Hero hero = activator.hero;
					int amount = NetworkedManagerBase<GameManager>.instance.GetCleanseReturnedDreamDust(activator, skill);
					NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount, base.position, hero);
					NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemCleansed(activator.hero, target);
					skill.level = NetworkedManagerBase<GameManager>.instance.GetCleanseSkillMinLevel();
					FxPlayNewNetworked(cleanseEffect);
				}
			}
		}
		else if (target is Gem gem && !(gem.owner.owner != activator) && !(activator.hero == null) && gem.effectiveLevel > NetworkedManagerBase<GameManager>.instance.GetCleanseGemMinQuality())
		{
			int cost2 = NetworkedManagerBase<GameManager>.instance.GetCleanseGoldCost(gem);
			if (activator.gold >= cost2)
			{
				activator.SpendGold(cost2);
				Hero hero2 = activator.hero;
				int amount2 = NetworkedManagerBase<GameManager>.instance.GetCleanseReturnedDreamDust(activator, gem);
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount2, base.position, hero2);
				NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemCleansed(activator.hero, target);
				gem.quality = NetworkedManagerBase<GameManager>.instance.GetCleanseGemMinQuality();
				FxPlayNewNetworked(cleanseEffect);
			}
		}
	}

	protected static void InvokeUserCode_CmdCleanse__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCleanse called on client.");
		}
		else
		{
			((Shrine_AltarOfCleansing)obj).UserCode_CmdCleanse__NetworkBehaviour__NetworkConnectionToClient(reader.ReadNetworkBehaviour(), senderConnection);
		}
	}

	static Shrine_AltarOfCleansing()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Shrine_AltarOfCleansing), "System.Void Shrine_AltarOfCleansing::CmdCleanse(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdCleanse__NetworkBehaviour__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(Shrine_AltarOfCleansing), "System.Void Shrine_AltarOfCleansing::RpcStartUpgrade(Mirror.NetworkConnectionToClient)", InvokeUserCode_RpcStartUpgrade__NetworkConnectionToClient);
	}
}
