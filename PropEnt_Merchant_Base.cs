using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public abstract class PropEnt_Merchant_Base : PropEntity, IProp, IInteractable
{
	public bool isSellEnabled = true;

	protected readonly SyncDictionary<uint, MerchandiseData[]> _merchandises = new SyncDictionary<uint, MerchandiseData[]>();

	public readonly SyncDictionary<uint, int> remainingRefreshes = new SyncDictionary<uint, int>();

	public GameObject fxSell;

	public GameObject fxPurchase;

	public GameObject fxPurchaseSkill;

	public GameObject fxPurchaseGem;

	public GameObject fxPurchaseSouvenir;

	public GameObject fxPurchaseTreasure;

	public GameObject[] props;

	public IReadOnlyDictionary<uint, MerchandiseData[]> merchandises => _merchandises;

	int IInteractable.priority => 100;

	bool IProp.isSingleton => true;

	public float focusDistance => 4.5f;

	public virtual Transform interactPivot => base.transform;

	public virtual bool canInteractWithMouse => true;

	protected override void Awake()
	{
		base.Awake();
		if (props != null)
		{
			GameObject[] array = props;
			foreach (GameObject prop in array)
			{
				prop.transform.parent = null;
				prop.SetLayerRecursive(0);
				FxPlay(prop);
			}
		}
	}

	public bool CanRefresh(DewPlayer player)
	{
		return remainingRefreshes[player.netId] > 0;
	}

	public int GetRefreshGoldCost(DewPlayer player)
	{
		if (remainingRefreshes[player.netId] <= 0)
		{
			return 99999;
		}
		float sum = 0f;
		MerchandiseData[] array = _merchandises[player.netId];
		for (int i = 0; i < array.Length; i++)
		{
			MerchandiseData i2 = array[i];
			sum += (float)i2.price.gold;
		}
		return Mathf.RoundToInt(sum / (float)_merchandises[player.netId].Length * 0.5f);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		GameManager.CallOnReady(delegate
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomChanged);
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomChanged);
		}
	}

	private void OnRoomChanged(EventInfoLoadRoom _)
	{
		if (this != null && base.isActive)
		{
			return;
		}
		for (int i = 0; i < props.Length; i++)
		{
			GameObject prop = props[i];
			if (prop != null)
			{
				UnityEngine.Object.Destroy(prop);
				props[i] = null;
			}
		}
		if (NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomChanged);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (props == null)
		{
			return;
		}
		GameObject[] array = props;
		foreach (GameObject prop in array)
		{
			if (prop != null)
			{
				prop.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
			}
		}
	}

	public virtual bool CanInteract(Entity entity)
	{
		if (base.isActive)
		{
			return base.Status.isAlive;
		}
		return false;
	}

	public void OnInteract(Entity entity, bool alt)
	{
		if (entity.isOwned && !alt)
		{
			ManagerBase<FloatingWindowManager>.instance.SetTarget(this);
			if (isSellEnabled)
			{
				ManagerBase<EditSkillManager>.instance.StartSell(this);
			}
		}
	}

	protected virtual void OnRefresh(DewPlayer player)
	{
	}

	protected void SpawnMerchandise(MerchandiseData data, DewPlayer player, Cost price)
	{
		Vector3 pos = base.agentPosition + (player.hero.agentPosition - base.agentPosition).normalized * 3f + UnityEngine.Random.insideUnitSphere.Flattened();
		pos = Dew.GetValidAgentDestination_LinearSweep(player.hero.agentPosition, pos);
		FxPlayNewNetworked(fxPurchase, player.hero);
		if (data.type == MerchandiseType.Skill)
		{
			SkillTrigger target = Dew.CreateSkillTrigger(DewResources.GetByShortTypeName<SkillTrigger>(data.itemName), pos, data.level, player);
			if (price.gold > 0)
			{
				target.maxSellGold = price.gold;
			}
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemBought(player.hero, target);
			FxPlayNewNetworked(fxPurchaseSkill, player.hero);
		}
		else if (data.type == MerchandiseType.Gem)
		{
			Gem target2 = Dew.CreateGem(DewResources.GetByShortTypeName<Gem>(data.itemName), pos, data.level, player);
			if (price.gold > 0)
			{
				target2.maxSellGold = price.gold;
			}
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemBought(player.hero, target2);
			FxPlayNewNetworked(fxPurchaseGem, player.hero);
		}
		else if (data.type == MerchandiseType.Treasure)
		{
			Treasure target3 = Dew.InstantiateAndSpawn(DewResources.GetByShortTypeName<Treasure>(data.itemName), pos, null, delegate(Treasure t)
			{
				t.price = price.gold;
				t.merchant = this;
				t.player = player;
				t.hero = player.hero;
				t.customData = data.customData;
			});
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemBought(player.hero, target3);
			FxPlayNewNetworked(fxPurchaseTreasure, player.hero);
		}
		else if (data.type == MerchandiseType.Souvenir)
		{
			FxPlayNewNetworked(fxPurchaseSouvenir, player.hero);
		}
	}

	protected void OnSell(DewPlayer activator, NetworkBehaviour target)
	{
		FxPlayNewNetworked(fxSell, activator.hero);
		if (target is SkillTrigger skill)
		{
			if (!(skill.owner.owner != activator) && !(activator.hero == null))
			{
				Hero hero = activator.hero;
				if (hero.Skill.TryGetSkillLocation(skill, out var loc) && hero.Skill.CanReplaceSkill(loc))
				{
					hero.Skill.UnequipSkill(loc, hero.position);
					int sellGold = skill.GetSellGold(activator);
					skill.Destroy();
					hero.owner.EarnGold(sellGold);
					NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemSold(activator.hero, target);
				}
			}
		}
		else if (target is Gem gem && !(gem.owner.owner != activator) && !(activator.hero == null))
		{
			Hero hero2 = activator.hero;
			if (hero2.Skill.TryGetGemLocation(gem, out var loc2))
			{
				hero2.Skill.UnequipGem(loc2, hero2.position);
				int sellGold2 = gem.GetSellGold(activator);
				gem.Destroy();
				hero2.owner.EarnGold(sellGold2);
				NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemSold(activator.hero, target);
			}
		}
	}

	public override void OnSaveActor(Dictionary<string, object> data)
	{
		base.OnSaveActor(data);
		data["_merchandises"] = _merchandises.ToArray();
		data["remainingRefreshes"] = remainingRefreshes.ToArray();
	}

	public override void OnLoadActor(Dictionary<string, object> data)
	{
		base.OnLoadActor(data);
		_merchandises.Clear();
		KeyValuePair<uint, MerchandiseData[]>[] array = (KeyValuePair<uint, MerchandiseData[]>[])data["_merchandises"];
		foreach (KeyValuePair<uint, MerchandiseData[]> p in array)
		{
			_merchandises.Add(p);
		}
		remainingRefreshes.Clear();
		KeyValuePair<uint, int>[] array2 = (KeyValuePair<uint, int>[])data["remainingRefreshes"];
		foreach (KeyValuePair<uint, int> p2 in array2)
		{
			remainingRefreshes.Add(p2);
		}
	}

	[Command(requiresAuthority = false)]
	public void CmdSell(NetworkBehaviour target, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(target);
		SendCommandInternal("System.Void PropEnt_Merchant_Base::CmdSell(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", -1388222545, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdPurchase(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(index);
		SendCommandInternal("System.Void PropEnt_Merchant_Base::CmdPurchase(System.Int32,Mirror.NetworkConnectionToClient)", -151629917, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdRefresh(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void PropEnt_Merchant_Base::CmdRefresh(Mirror.NetworkConnectionToClient)", -700024020, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	protected PropEnt_Merchant_Base()
	{
		InitSyncObject(_merchandises);
		InitSyncObject(remainingRefreshes);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSell__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour target, NetworkConnectionToClient sender)
	{
		if (!isSellEnabled)
		{
			return;
		}
		try
		{
			OnSell(sender.GetPlayer(), target);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_CmdSell__NetworkBehaviour__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSell called on client.");
		}
		else
		{
			((PropEnt_Merchant_Base)obj).UserCode_CmdSell__NetworkBehaviour__NetworkConnectionToClient(reader.ReadNetworkBehaviour(), senderConnection);
		}
	}

	protected void UserCode_CmdPurchase__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		try
		{
			DewPlayer player = sender.GetPlayer();
			if (!merchandises.TryGetValue(player.netId, out var arr) || index < 0 || index >= arr.Length)
			{
				return;
			}
			MerchandiseData item = arr[index];
			if (item.count <= 0)
			{
				return;
			}
			Cost price = (Cost)(item.price * player.buyPriceMultiplier);
			if (price.CanAfford(player.hero) != AffordType.Yes)
			{
				return;
			}
			if (item.type == MerchandiseType.Treasure)
			{
				Treasure byShortTypeName = DewResources.GetByShortTypeName<Treasure>(item.itemName);
				byShortTypeName.player = player;
				byShortTypeName.hero = player.hero;
				byShortTypeName.merchant = this;
				byShortTypeName.customData = item.customData;
				if (!byShortTypeName.CanBePurchased())
				{
					return;
				}
			}
			MerchandiseData[] newArr = arr.ToArray();
			newArr[index].count--;
			_merchandises[player.netId] = newArr;
			if (price.gold > 0)
			{
				player.SpendGold(price.gold);
			}
			if (price.dreamDust > 0)
			{
				player.SpendDreamDust(price.dreamDust);
			}
			if (price.stardust > 0)
			{
				throw new NotImplementedException();
			}
			SpawnMerchandise(item, player, price);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	protected static void InvokeUserCode_CmdPurchase__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPurchase called on client.");
		}
		else
		{
			((PropEnt_Merchant_Base)obj).UserCode_CmdPurchase__Int32__NetworkConnectionToClient(reader.ReadInt(), senderConnection);
		}
	}

	protected void UserCode_CmdRefresh__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		if (!(player == null) && CanRefresh(player))
		{
			int cost = GetRefreshGoldCost(player);
			if (cost <= player.gold)
			{
				player.SpendGold(cost);
				remainingRefreshes[player.netId]--;
				OnRefresh(player);
			}
		}
	}

	protected static void InvokeUserCode_CmdRefresh__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRefresh called on client.");
		}
		else
		{
			((PropEnt_Merchant_Base)obj).UserCode_CmdRefresh__NetworkConnectionToClient(senderConnection);
		}
	}

	static PropEnt_Merchant_Base()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PropEnt_Merchant_Base), "System.Void PropEnt_Merchant_Base::CmdSell(Mirror.NetworkBehaviour,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdSell__NetworkBehaviour__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterCommand(typeof(PropEnt_Merchant_Base), "System.Void PropEnt_Merchant_Base::CmdPurchase(System.Int32,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdPurchase__Int32__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterCommand(typeof(PropEnt_Merchant_Base), "System.Void PropEnt_Merchant_Base::CmdRefresh(Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdRefresh__NetworkConnectionToClient, requiresAuthority: false);
	}
}
