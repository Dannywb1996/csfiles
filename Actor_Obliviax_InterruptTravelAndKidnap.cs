using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Actor_Obliviax_InterruptTravelAndKidnap : Actor
{
	public GameObject fxStart;

	public GameObject fxBeforeKidnapEffect;

	public GameObject fxKidnapEffect;

	protected override void OnCreate()
	{
		base.OnCreate();
		DewEffect.Play(fxStart, ManagerBase<CameraManager>.instance.focusedEntity.agentPosition, null);
		if (base.isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.AddTravelToNodeInterrupt(TravelInterrupt);
			NetworkedManagerBase<ChatManager>.instance.BroadcastChatMessage(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = "Chat_Notice_ObliviaxWarning"
			});
		}
	}

	private bool TravelInterrupt(EventInfoTravelToNodeInterrupt arg)
	{
		StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			RpcPlayBeforeKidnapEffect();
			if (Rift_RoomExit.instance != null)
			{
				Rift_RoomExit.instance.isLocked = true;
			}
			foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
			{
				if (!humanPlayer.hero.IsNullInactiveDeadOrKnockedOut())
				{
					Hero target = humanPlayer.hero;
					CreateBasicEffect(target, new UncollidableEffect(), 5f);
					CreateBasicEffect(target, new DeathInterruptEffect
					{
						onInterrupt = delegate
						{
							target.Status.SetHealth(1f);
						},
						priority = -9999
					}, 5f);
				}
			}
			yield return new WaitForSeconds(1.5f);
			bool flag = true;
			foreach (DewPlayer humanPlayer2 in DewPlayer.humanPlayers)
			{
				if (!humanPlayer2.hero.IsNullInactiveDeadOrKnockedOut())
				{
					CreateStatusEffect<Se_ObliviaxKidnap>(humanPlayer2.hero, default(CastInfo));
					flag = false;
				}
			}
			if (flag)
			{
				Destroy();
				FxStopNetworked(fxBeforeKidnapEffect);
				FxStopNetworked(fxKidnapEffect);
			}
			else
			{
				RpcPlayKidnapEffect();
				yield return new WaitForSeconds(3.6f);
				if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
				{
					NetworkedManagerBase<ZoneManager>.instance.LoadSidetrackRoom("Room_Special_Obliviax_Nest");
				}
				Destroy();
				yield return new WaitForSeconds(1f);
				FxStopNetworked(fxBeforeKidnapEffect);
				FxStopNetworked(fxKidnapEffect);
				if (Rift_RoomExit.instance != null)
				{
					Rift_RoomExit.instance.isLocked = false;
				}
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.RemoveTravelToNodeInterrupt(TravelInterrupt);
		}
	}

	[ClientRpc]
	private void RpcPlayBeforeKidnapEffect()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Actor_Obliviax_InterruptTravelAndKidnap::RpcPlayBeforeKidnapEffect()", -1120428135, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcPlayKidnapEffect()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Actor_Obliviax_InterruptTravelAndKidnap::RpcPlayKidnapEffect()", 763090682, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayBeforeKidnapEffect()
	{
		DewEffect.Play(fxBeforeKidnapEffect, ManagerBase<CameraManager>.instance.focusedEntity.agentPosition, null);
	}

	protected static void InvokeUserCode_RpcPlayBeforeKidnapEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayBeforeKidnapEffect called on server.");
		}
		else
		{
			((Actor_Obliviax_InterruptTravelAndKidnap)obj).UserCode_RpcPlayBeforeKidnapEffect();
		}
	}

	protected void UserCode_RpcPlayKidnapEffect()
	{
		DewEffect.Play(fxKidnapEffect, ManagerBase<CameraManager>.instance.focusedEntity.agentPosition, null);
	}

	protected static void InvokeUserCode_RpcPlayKidnapEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayKidnapEffect called on server.");
		}
		else
		{
			((Actor_Obliviax_InterruptTravelAndKidnap)obj).UserCode_RpcPlayKidnapEffect();
		}
	}

	static Actor_Obliviax_InterruptTravelAndKidnap()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Actor_Obliviax_InterruptTravelAndKidnap), "System.Void Actor_Obliviax_InterruptTravelAndKidnap::RpcPlayBeforeKidnapEffect()", InvokeUserCode_RpcPlayBeforeKidnapEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(Actor_Obliviax_InterruptTravelAndKidnap), "System.Void Actor_Obliviax_InterruptTravelAndKidnap::RpcPlayKidnapEffect()", InvokeUserCode_RpcPlayKidnapEffect);
	}
}
