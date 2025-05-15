using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ClientEventManager : NetworkedManagerBase<ClientEventManager>
{
	public SafeAction<StatusEffect> OnShowStatusEffectIcon;

	public SafeAction<StatusEffect> OnHideStatusEffectIcon;

	public SafeAction<OnScreenTimerHandle> OnShowOnScreenTimer;

	public SafeAction<OnScreenTimerHandle> OnHideOnScreenTimer;

	public SafeAction<EventInfoHeal> OnTakeManaHeal;

	public SafeAction<EventInfoHeal> OnTakeHeal;

	public SafeAction<EventInfoDamage> OnTakeDamage;

	public SafeAction<EventInfoKill> OnDeath;

	public SafeAction<EventInfoSpentMana> OnGetManaSpent;

	public SafeAction<EventInfoAttackMissed> OnAttackMissed;

	public SafeAction<EventInfoDamageNegatedByImmunity> OnDamageNegated;

	public SafeAction<EventInfoApplyElemental> OnApplyElemental;

	public SafeAction<EventInfoCast> OnCastComplete;

	public SafeAction<EventInfoAttackHit> OnAttackHit;

	public SafeAction<Hero, NetworkBehaviour> OnItemSold;

	public SafeAction<Hero, NetworkBehaviour> OnItemBought;

	public SafeAction<Hero, Gem> OnGemMergeUpgraded;

	public SafeAction<Hero, NetworkBehaviour> OnItemUpgraded;

	public SafeAction<Hero, HeroSkillLocation> OnLocalHeroAbilityChanged;

	public SafeAction<Hero, GemLocation> OnLocalHeroGemChanged;

	public SafeAction<Hero, NetworkBehaviour> OnDismantled;

	public SafeAction<Hero, NetworkBehaviour> OnItemCleansed;

	public SafeAction<Hero> OnHeroKnockedOut;

	public SafeAction<Hero> OnHeroRevive;

	public SafeAction<DewPlayer, DewPlayer, int, int> OnGiveCurrency;

	public SafeAction<Entity> OnIgnoreCC;

	public SafeAction<Entity> OnRefreshEntityHealthbar;

	public override void OnStart()
	{
		base.OnStart();
		NetworkedManagerBase<ActorManager>.instance.onActorAdd += new Action<Actor>(AddEvents);
		NetworkedManagerBase<ActorManager>.instance.onActorRemove += new Action<Actor>(RemoveEvents);
		foreach (Actor a in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			AddEvents(a);
		}
		DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
		dewNetworkManager.onHumanPlayerAdd = (Action<DewPlayer>)Delegate.Combine(dewNetworkManager.onHumanPlayerAdd, new Action<DewPlayer>(OnHumanPlayerAdd));
		DewNetworkManager dewNetworkManager2 = DewNetworkManager.instance;
		dewNetworkManager2.onHumanPlayerRemove = (Action<DewPlayer>)Delegate.Combine(dewNetworkManager2.onHumanPlayerRemove, new Action<DewPlayer>(OnHumanPlayerRemove));
		foreach (DewPlayer p in DewPlayer.humanPlayers)
		{
			OnHumanPlayerAdd(p);
		}
		OnGiveCurrency += (Action<DewPlayer, DewPlayer, int, int>)delegate(DewPlayer from, DewPlayer to, int gold, int dreamDust)
		{
			string content = ((gold > 0 && dreamDust > 0) ? "Chat_Notice_GiveCurrency_GoldAndDreamDust" : ((gold <= 0) ? "Chat_Notice_GiveCurrency_DreamDust" : "Chat_Notice_GiveCurrency_Gold"));
			NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
			{
				type = ChatManager.MessageType.Notice,
				content = content,
				args = new string[4]
				{
					ChatManager.GetColoredDescribedPlayerName(from),
					ChatManager.GetColoredDescribedPlayerName(to),
					gold.ToString("#,##0"),
					dreamDust.ToString("#,##0")
				}
			});
		};
	}

	private void OnHumanPlayerAdd(DewPlayer obj)
	{
		DewPlayer from = obj;
		obj.ClientEvent_OnGiveCurrency += (Action<int, int, DewPlayer>)delegate(int gold, int dreamDust, DewPlayer to)
		{
			OnGiveCurrency?.Invoke(from, to, gold, dreamDust);
		};
	}

	private void OnHumanPlayerRemove(DewPlayer obj)
	{
	}

	public virtual void AddEvents(Actor actor)
	{
		if (base.isServer)
		{
			if (actor is Entity ent)
			{
				ent.EntityEvent_OnTakeManaHeal += new Action<EventInfoHeal>(InvokeOnTakeManaHeal);
				ent.EntityEvent_OnTakeHeal += new Action<EventInfoHeal>(InvokeOnTakeHeal);
				ent.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(InvokeOnTakeDamage);
				ent.EntityEvent_OnDeath += new Action<EventInfoKill>(InvokeOnDeath);
				ent.EntityEvent_OnGetManaSpent += new Action<EventInfoSpentMana>(InvokeOnGetManaSpent);
				ent.EntityEvent_OnAttackMissed += new Action<EventInfoAttackMissed>(InvokeOnAttackMissed);
				ent.EntityEvent_OnDamageNegated += new Action<EventInfoDamageNegatedByImmunity>(InvokeOnDamageNegated);
				ent.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(InvokeOnAttackHit);
			}
			actor.ActorEvent_OnApplyElemental += new Action<EventInfoApplyElemental>(InvokeOnApplyElemental);
		}
	}

	public virtual void RemoveEvents(Actor actor)
	{
		if (base.isServer && !(actor == null))
		{
			if (actor is Entity ent)
			{
				ent.EntityEvent_OnTakeManaHeal -= new Action<EventInfoHeal>(InvokeOnTakeManaHeal);
				ent.EntityEvent_OnTakeHeal -= new Action<EventInfoHeal>(InvokeOnTakeHeal);
				ent.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(InvokeOnTakeDamage);
				ent.EntityEvent_OnDeath -= new Action<EventInfoKill>(InvokeOnDeath);
				ent.EntityEvent_OnGetManaSpent -= new Action<EventInfoSpentMana>(InvokeOnGetManaSpent);
				ent.EntityEvent_OnAttackMissed -= new Action<EventInfoAttackMissed>(InvokeOnAttackMissed);
				ent.EntityEvent_OnDamageNegated -= new Action<EventInfoDamageNegatedByImmunity>(InvokeOnDamageNegated);
				ent.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(InvokeOnAttackHit);
			}
			actor.ActorEvent_OnApplyElemental -= new Action<EventInfoApplyElemental>(InvokeOnApplyElemental);
		}
	}

	[ClientRpc]
	private void InvokeOnTakeManaHeal(EventInfoHeal info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoHeal(writer, info);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnTakeManaHeal(EventInfoHeal)", -1327707822, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnTakeHeal(EventInfoHeal info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoHeal(writer, info);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnTakeHeal(EventInfoHeal)", 977163033, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnTakeDamage(EventInfoDamage info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoDamage(writer, info);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnTakeDamage(EventInfoDamage)", 292245963, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnDeath(EventInfoKill info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoKill(writer, info);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnDeath(EventInfoKill)", 1130950576, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnGetManaSpent(EventInfoSpentMana info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoSpentMana(writer, info);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnGetManaSpent(EventInfoSpentMana)", -19905218, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnAttackMissed(EventInfoAttackMissed info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoAttackMissed(writer, info);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnAttackMissed(EventInfoAttackMissed)", -1821054102, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnDamageNegated(EventInfoDamageNegatedByImmunity info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoDamageNegatedByImmunity(writer, info);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnDamageNegated(EventInfoDamageNegatedByImmunity)", 337155917, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void InvokeOnAttackHit(EventInfoAttackHit info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoAttackHit(writer, info);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnAttackHit(EventInfoAttackHit)", 1032775184, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void InvokeOnItemSold(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(h);
		writer.WriteNetworkBehaviour(nb);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnItemSold(Hero,Mirror.NetworkBehaviour)", -1877842699, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void InvokeOnItemCleansed(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(h);
		writer.WriteNetworkBehaviour(nb);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnItemCleansed(Hero,Mirror.NetworkBehaviour)", -62337846, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void InvokeOnItemBought(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(h);
		writer.WriteNetworkBehaviour(nb);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnItemBought(Hero,Mirror.NetworkBehaviour)", 1103697868, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void InvokeOnGemMergeUpgraded(Hero h, Gem g)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(h);
		writer.WriteNetworkBehaviour(g);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnGemMergeUpgraded(Hero,Gem)", 1730456686, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void InvokeOnItemUpgraded(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(h);
		writer.WriteNetworkBehaviour(nb);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnItemUpgraded(Hero,Mirror.NetworkBehaviour)", 1700352233, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void InvokeOnDismantled(Hero h, NetworkBehaviour nb)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(h);
		writer.WriteNetworkBehaviour(nb);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnDismantled(Hero,Mirror.NetworkBehaviour)", 997592145, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void InvokeOnIgnoreCC(Entity ent)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(ent);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnIgnoreCC(Entity)", -997650711, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	internal void InvokeOnApplyElemental(EventInfoApplyElemental info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoApplyElemental(writer, info);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnApplyElemental(EventInfoApplyElemental)", 2129906046, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	public void InvokeOnRefreshEntityHealthbar(Entity e)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(e);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnRefreshEntityHealthbar(Entity)", -1092666960, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	public void InvokeOnCastComplete(EventInfoCast eventInfo)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EventInfoCast(writer, eventInfo);
		SendRPCInternal("System.Void ClientEventManager::InvokeOnCastComplete(EventInfoCast)", -2136773767, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_InvokeOnTakeManaHeal__EventInfoHeal(EventInfoHeal info)
	{
		if (!(info.target == null))
		{
			OnTakeManaHeal?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnTakeManaHeal__EventInfoHeal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnTakeManaHeal called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnTakeManaHeal__EventInfoHeal(GeneratedNetworkCode._Read_EventInfoHeal(reader));
		}
	}

	protected void UserCode_InvokeOnTakeHeal__EventInfoHeal(EventInfoHeal info)
	{
		if (!(info.target == null))
		{
			OnTakeHeal?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnTakeHeal__EventInfoHeal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnTakeHeal called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnTakeHeal__EventInfoHeal(GeneratedNetworkCode._Read_EventInfoHeal(reader));
		}
	}

	protected void UserCode_InvokeOnTakeDamage__EventInfoDamage(EventInfoDamage info)
	{
		if (!(info.victim == null))
		{
			OnTakeDamage?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnTakeDamage__EventInfoDamage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnTakeDamage called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnTakeDamage__EventInfoDamage(GeneratedNetworkCode._Read_EventInfoDamage(reader));
		}
	}

	protected void UserCode_InvokeOnDeath__EventInfoKill(EventInfoKill info)
	{
		if (!(info.victim == null))
		{
			OnDeath?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnDeath__EventInfoKill(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnDeath called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnDeath__EventInfoKill(GeneratedNetworkCode._Read_EventInfoKill(reader));
		}
	}

	protected void UserCode_InvokeOnGetManaSpent__EventInfoSpentMana(EventInfoSpentMana info)
	{
		if (!(info.entity == null))
		{
			OnGetManaSpent?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnGetManaSpent__EventInfoSpentMana(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnGetManaSpent called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnGetManaSpent__EventInfoSpentMana(GeneratedNetworkCode._Read_EventInfoSpentMana(reader));
		}
	}

	protected void UserCode_InvokeOnAttackMissed__EventInfoAttackMissed(EventInfoAttackMissed info)
	{
		if (!(info.victim == null))
		{
			OnAttackMissed?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnAttackMissed__EventInfoAttackMissed(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnAttackMissed called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnAttackMissed__EventInfoAttackMissed(GeneratedNetworkCode._Read_EventInfoAttackMissed(reader));
		}
	}

	protected void UserCode_InvokeOnDamageNegated__EventInfoDamageNegatedByImmunity(EventInfoDamageNegatedByImmunity info)
	{
		if (!(info.victim == null))
		{
			OnDamageNegated?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnDamageNegated__EventInfoDamageNegatedByImmunity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnDamageNegated called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnDamageNegated__EventInfoDamageNegatedByImmunity(GeneratedNetworkCode._Read_EventInfoDamageNegatedByImmunity(reader));
		}
	}

	protected void UserCode_InvokeOnAttackHit__EventInfoAttackHit(EventInfoAttackHit info)
	{
		if (!(info.victim == null))
		{
			OnAttackHit?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnAttackHit__EventInfoAttackHit(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnAttackHit called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnAttackHit__EventInfoAttackHit(GeneratedNetworkCode._Read_EventInfoAttackHit(reader));
		}
	}

	protected void UserCode_InvokeOnItemSold__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!(h == null) && !(nb == null))
		{
			OnItemSold?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnItemSold__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnItemSold called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnItemSold__Hero__NetworkBehaviour(reader.ReadNetworkBehaviour<Hero>(), reader.ReadNetworkBehaviour());
		}
	}

	protected void UserCode_InvokeOnItemCleansed__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!(h == null) && !(nb == null))
		{
			OnItemCleansed?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnItemCleansed__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnItemCleansed called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnItemCleansed__Hero__NetworkBehaviour(reader.ReadNetworkBehaviour<Hero>(), reader.ReadNetworkBehaviour());
		}
	}

	protected void UserCode_InvokeOnItemBought__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!(h == null) && !(nb == null))
		{
			OnItemBought?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnItemBought__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnItemBought called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnItemBought__Hero__NetworkBehaviour(reader.ReadNetworkBehaviour<Hero>(), reader.ReadNetworkBehaviour());
		}
	}

	protected void UserCode_InvokeOnGemMergeUpgraded__Hero__Gem(Hero h, Gem g)
	{
		if (!(h == null) && !(g == null))
		{
			OnGemMergeUpgraded?.Invoke(h, g);
		}
	}

	protected static void InvokeUserCode_InvokeOnGemMergeUpgraded__Hero__Gem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnGemMergeUpgraded called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnGemMergeUpgraded__Hero__Gem(reader.ReadNetworkBehaviour<Hero>(), reader.ReadNetworkBehaviour<Gem>());
		}
	}

	protected void UserCode_InvokeOnItemUpgraded__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!(h == null) && !(nb == null))
		{
			OnItemUpgraded?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnItemUpgraded__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnItemUpgraded called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnItemUpgraded__Hero__NetworkBehaviour(reader.ReadNetworkBehaviour<Hero>(), reader.ReadNetworkBehaviour());
		}
	}

	protected void UserCode_InvokeOnDismantled__Hero__NetworkBehaviour(Hero h, NetworkBehaviour nb)
	{
		if (!(h == null) && !(nb == null))
		{
			OnDismantled?.Invoke(h, nb);
		}
	}

	protected static void InvokeUserCode_InvokeOnDismantled__Hero__NetworkBehaviour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnDismantled called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnDismantled__Hero__NetworkBehaviour(reader.ReadNetworkBehaviour<Hero>(), reader.ReadNetworkBehaviour());
		}
	}

	protected void UserCode_InvokeOnIgnoreCC__Entity(Entity ent)
	{
		if (!(ent == null))
		{
			OnIgnoreCC?.Invoke(ent);
		}
	}

	protected static void InvokeUserCode_InvokeOnIgnoreCC__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnIgnoreCC called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnIgnoreCC__Entity(reader.ReadNetworkBehaviour<Entity>());
		}
	}

	protected void UserCode_InvokeOnApplyElemental__EventInfoApplyElemental(EventInfoApplyElemental info)
	{
		if (!(info.victim == null) && !(info.actor == null))
		{
			OnApplyElemental?.Invoke(info);
		}
	}

	protected static void InvokeUserCode_InvokeOnApplyElemental__EventInfoApplyElemental(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnApplyElemental called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnApplyElemental__EventInfoApplyElemental(GeneratedNetworkCode._Read_EventInfoApplyElemental(reader));
		}
	}

	protected void UserCode_InvokeOnRefreshEntityHealthbar__Entity(Entity e)
	{
		if (!(e == null))
		{
			OnRefreshEntityHealthbar?.Invoke(e);
		}
	}

	protected static void InvokeUserCode_InvokeOnRefreshEntityHealthbar__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnRefreshEntityHealthbar called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnRefreshEntityHealthbar__Entity(reader.ReadNetworkBehaviour<Entity>());
		}
	}

	protected void UserCode_InvokeOnCastComplete__EventInfoCast(EventInfoCast eventInfo)
	{
		if (!(eventInfo.instance == null) && !(eventInfo.trigger == null))
		{
			OnCastComplete?.Invoke(eventInfo);
		}
	}

	protected static void InvokeUserCode_InvokeOnCastComplete__EventInfoCast(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC InvokeOnCastComplete called on server.");
		}
		else
		{
			((ClientEventManager)obj).UserCode_InvokeOnCastComplete__EventInfoCast(GeneratedNetworkCode._Read_EventInfoCast(reader));
		}
	}

	static ClientEventManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnTakeManaHeal(EventInfoHeal)", InvokeUserCode_InvokeOnTakeManaHeal__EventInfoHeal);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnTakeHeal(EventInfoHeal)", InvokeUserCode_InvokeOnTakeHeal__EventInfoHeal);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnTakeDamage(EventInfoDamage)", InvokeUserCode_InvokeOnTakeDamage__EventInfoDamage);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnDeath(EventInfoKill)", InvokeUserCode_InvokeOnDeath__EventInfoKill);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnGetManaSpent(EventInfoSpentMana)", InvokeUserCode_InvokeOnGetManaSpent__EventInfoSpentMana);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnAttackMissed(EventInfoAttackMissed)", InvokeUserCode_InvokeOnAttackMissed__EventInfoAttackMissed);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnDamageNegated(EventInfoDamageNegatedByImmunity)", InvokeUserCode_InvokeOnDamageNegated__EventInfoDamageNegatedByImmunity);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnAttackHit(EventInfoAttackHit)", InvokeUserCode_InvokeOnAttackHit__EventInfoAttackHit);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnItemSold(Hero,Mirror.NetworkBehaviour)", InvokeUserCode_InvokeOnItemSold__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnItemCleansed(Hero,Mirror.NetworkBehaviour)", InvokeUserCode_InvokeOnItemCleansed__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnItemBought(Hero,Mirror.NetworkBehaviour)", InvokeUserCode_InvokeOnItemBought__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnGemMergeUpgraded(Hero,Gem)", InvokeUserCode_InvokeOnGemMergeUpgraded__Hero__Gem);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnItemUpgraded(Hero,Mirror.NetworkBehaviour)", InvokeUserCode_InvokeOnItemUpgraded__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnDismantled(Hero,Mirror.NetworkBehaviour)", InvokeUserCode_InvokeOnDismantled__Hero__NetworkBehaviour);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnIgnoreCC(Entity)", InvokeUserCode_InvokeOnIgnoreCC__Entity);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnApplyElemental(EventInfoApplyElemental)", InvokeUserCode_InvokeOnApplyElemental__EventInfoApplyElemental);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnRefreshEntityHealthbar(Entity)", InvokeUserCode_InvokeOnRefreshEntityHealthbar__Entity);
		RemoteProcedureCalls.RegisterRpc(typeof(ClientEventManager), "System.Void ClientEventManager::InvokeOnCastComplete(EventInfoCast)", InvokeUserCode_InvokeOnCastComplete__EventInfoCast);
	}
}
