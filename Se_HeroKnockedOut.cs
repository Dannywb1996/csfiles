using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_HeroKnockedOut : StatusEffect
{
	public float reviveHealthMultiplier = 0.5f;

	public GameObject goldExplosion;

	public GameObject blueExplosion;

	private bool _didAddQuest;

	protected override void OnCreate()
	{
		base.OnCreate();
		Hero hero = (Hero)base.victim;
		if (!base.isServer)
		{
			return;
		}
		DoStun();
		DoSilence();
		DoInvulnerable();
		DoInvisible();
		DoUncollidable();
		hero.isKnockedOut = true;
		EventInfoKill knockedOutInfo = new EventInfoKill
		{
			actor = hero._lastAttacker,
			victim = hero
		};
		RpcInvokeKnockedOut(knockedOutInfo);
		hero.Control.freeMovement = true;
		FxPlayNetworked(hero.Visual.hasGoldDissolve ? goldExplosion : blueExplosion, hero);
		if (hero.Visual.deathEffect != null)
		{
			FxGibs[] componentsInChildren = hero.Visual.deathEffect.GetComponentsInChildren<FxGibs>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].info = new GibInfo
				{
					normalizedCurrentDamage = Vector3.up * 0.25f,
					velocity = hero.AI.estimatedVelocityUnclamped,
					yVelocity = hero.Visual.currentYVelocity
				};
			}
			FxPlayNew(hero.Visual.deathEffect, hero.Visual.entity);
		}
	}

	[ClientRpc]
	private void RpcInvokeKnockedOut(EventInfoKill kill)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoKill(writer, kill);
		SendRPCInternal("System.Void Se_HeroKnockedOut::RpcInvokeKnockedOut(EventInfoKill)", -1360231535, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeRevive()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Se_HeroKnockedOut::RpcInvokeRevive()", 194098549, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			Hero hero = base.victim as Hero;
			if (hero != null && hero.isActive)
			{
				hero.isKnockedOut = false;
				hero.Control.freeMovement = false;
				RpcInvokeRevive();
			}
		}
	}

	public void Revive(float reviveHealthMult = -1f)
	{
		if (reviveHealthMult < 0f)
		{
			reviveHealthMult = reviveHealthMultiplier;
		}
		base.victim.Status.SetHealth(base.victim.maxHealth * reviveHealthMult);
		Destroy();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer)
		{
			return;
		}
		if (!_didAddQuest && Time.time - base.creationTime > 1f && Dew.SelectRandomAliveHero(fallbackToDead: false) != null && (AttrCustomizeResources.Config.enableBossRoomGenerateLostSoul || NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss))
		{
			_didAddQuest = true;
			NetworkedManagerBase<QuestManager>.instance.StartQuest(delegate(Quest_LostSoul s)
			{
				s.NetworktargetHero = (Hero)base.victim;
			});
		}
		base.victim.Status.SetHealth(0.01f);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcInvokeKnockedOut__EventInfoKill(EventInfoKill kill)
	{
		try
		{
			((Hero)base.victim).ClientHeroEvent_OnKnockedOut?.Invoke(kill);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnHeroKnockedOut?.Invoke((Hero)base.victim);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	protected static void InvokeUserCode_RpcInvokeKnockedOut__EventInfoKill(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeKnockedOut called on server.");
		}
		else
		{
			((Se_HeroKnockedOut)obj).UserCode_RpcInvokeKnockedOut__EventInfoKill(GeneratedNetworkCode._Read_EventInfoKill(reader));
		}
	}

	protected void UserCode_RpcInvokeRevive()
	{
		try
		{
			((Hero)base.victim).ClientHeroEvent_OnRevive?.Invoke((Hero)base.victim);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnHeroRevive?.Invoke((Hero)base.victim);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	protected static void InvokeUserCode_RpcInvokeRevive(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeRevive called on server.");
		}
		else
		{
			((Se_HeroKnockedOut)obj).UserCode_RpcInvokeRevive();
		}
	}

	static Se_HeroKnockedOut()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Se_HeroKnockedOut), "System.Void Se_HeroKnockedOut::RpcInvokeKnockedOut(EventInfoKill)", InvokeUserCode_RpcInvokeKnockedOut__EventInfoKill);
		RemoteProcedureCalls.RegisterRpc(typeof(Se_HeroKnockedOut), "System.Void Se_HeroKnockedOut::RpcInvokeRevive()", InvokeUserCode_RpcInvokeRevive);
	}
}
