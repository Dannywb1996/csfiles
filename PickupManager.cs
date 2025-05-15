using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class PickupManager : NetworkedManagerBase<PickupManager>
{
	public int largeGoldOrbAmount = 100;

	public int mediumGoldOrbAmount = 40;

	public int smallGoldOrbAmount = 10;

	public int goldOrbMaxCount = 7;

	public int largeExpOrbAmount = 50;

	public int mediumExpOrbAmount = 10;

	public int smallExpOrbAmount = 3;

	public int expOrbMaxCount = 7;

	public int dreamDustMaxCount = 5;

	public int dreamDustMinAmount = 10;

	public bool isStardustDropDisabled;

	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkedManagerBase<ActorManager>.instance.onEntityAdd += new Action<Entity>(HandleEntityAdd);
	}

	private void HandleEntityAdd(Entity obj)
	{
		if (obj is Monster)
		{
			obj.EntityEvent_OnDeath += new Action<EventInfoKill>(HandleDeathDrop);
		}
	}

	[Server]
	public void DropStarDust(int amount, Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupManager::DropStarDust(System.Int32,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		amount = DewMath.RandomRoundToInt((float)amount * DewBuildProfile.current.stardustGainMultiplier);
		if (amount > 0)
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			int remainingAmount = amount;
			while (remainingAmount > 0 && !NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
			{
				int amountToDrop = Mathf.Max(UnityEngine.Random.Range(2, 4), amount / 6);
				amountToDrop = Mathf.Min(remainingAmount, amountToDrop);
				remainingAmount -= amountToDrop;
				Spawn(amountToDrop);
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.15f));
			}
		}
		void Spawn(int val)
		{
			Dew.CreateActor(Dew.GetGoodRewardPosition(position), null, null, delegate(Shrine_Stardust stardust)
			{
				stardust.amount = val;
			});
		}
	}

	[Server]
	public void DropDreamDust(bool isGivenByOtherPlayer, int amount, Vector3 position, Hero target = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupManager::DropDreamDust(System.Boolean,System.Int32,UnityEngine.Vector3,Hero)' called when server was not active");
			return;
		}
		int perSpawnAmount = Mathf.Max(amount / dreamDustMaxCount, dreamDustMinAmount);
		while (amount > 0)
		{
			if (amount >= perSpawnAmount)
			{
				Spawn(perSpawnAmount);
				amount -= perSpawnAmount;
			}
			else
			{
				Spawn(amount);
				amount = 0;
			}
		}
		void Spawn(int val)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_DreamDust p)
			{
				p.amount = val;
				p.target = target;
				p.isGivenByOtherPlayer = isGivenByOtherPlayer;
			});
		}
	}

	[Server]
	public void DropGold(bool isKillGold, bool isGivenByOtherPlayer, int amount, Vector3 position, Hero target = null)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupManager::DropGold(System.Boolean,System.Boolean,System.Int32,UnityEngine.Vector3,Hero)' called when server was not active");
			return;
		}
		if (amount > largeGoldOrbAmount * goldOrbMaxCount)
		{
			int i;
			for (i = 0; i < goldOrbMaxCount; i++)
			{
				NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_LargeGoldOrb p)
				{
					p.amount = ((i == 0) ? (largeGoldOrbAmount + amount - largeGoldOrbAmount * goldOrbMaxCount) : largeGoldOrbAmount);
					p.target = target;
					p.isKillGold = isKillGold;
					p.isGivenByOtherPlayer = isGivenByOtherPlayer;
				});
			}
			return;
		}
		int largeGoldOrbs = amount / largeGoldOrbAmount;
		amount -= largeGoldOrbs * largeGoldOrbAmount;
		int mediumGoldOrbs = amount / mediumGoldOrbAmount;
		amount -= mediumGoldOrbs * mediumGoldOrbAmount;
		int smallGoldOrbs = amount / smallGoldOrbAmount;
		amount -= smallGoldOrbs * smallGoldOrbAmount;
		for (int i2 = 0; i2 < largeGoldOrbs; i2++)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_LargeGoldOrb p)
			{
				p.amount = largeGoldOrbAmount;
				p.target = target;
				p.isKillGold = isKillGold;
				p.isGivenByOtherPlayer = isGivenByOtherPlayer;
			});
		}
		for (int i3 = 0; i3 < mediumGoldOrbs; i3++)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_MediumGoldOrb p)
			{
				p.amount = mediumGoldOrbAmount;
				p.target = target;
				p.isKillGold = isKillGold;
				p.isGivenByOtherPlayer = isGivenByOtherPlayer;
			});
		}
		for (int i4 = 0; i4 < smallGoldOrbs; i4++)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_SmallGoldOrb p)
			{
				p.amount = smallGoldOrbAmount;
				p.target = target;
				p.isKillGold = isKillGold;
				p.isGivenByOtherPlayer = isGivenByOtherPlayer;
			});
		}
		if (amount > 0)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_SmallGoldOrb p)
			{
				p.amount = amount;
				p.target = target;
				p.isKillGold = isKillGold;
				p.isGivenByOtherPlayer = isGivenByOtherPlayer;
			});
		}
	}

	[Server]
	public void DropExp(int amount, Vector3 position)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PickupManager::DropExp(System.Int32,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		if (amount > largeExpOrbAmount * expOrbMaxCount)
		{
			for (int i = 0; i < expOrbMaxCount; i++)
			{
				NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_LargeExpOrb p)
				{
					p.amount = Mathf.RoundToInt((float)amount / (float)goldOrbMaxCount);
				});
			}
			return;
		}
		int largeExpOrbs = amount / largeExpOrbAmount;
		amount -= largeExpOrbs * largeExpOrbAmount;
		int mediumExpOrbs = amount / mediumExpOrbAmount;
		amount -= mediumExpOrbs * mediumExpOrbAmount;
		int smallExpOrbs = amount / smallExpOrbAmount;
		amount -= smallExpOrbs * smallExpOrbAmount;
		for (int i2 = 0; i2 < largeExpOrbs; i2++)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_LargeExpOrb p)
			{
				p.amount = largeExpOrbAmount;
			});
		}
		for (int i3 = 0; i3 < mediumExpOrbs; i3++)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_MediumExpOrb p)
			{
				p.amount = mediumExpOrbAmount;
			});
		}
		for (int i4 = 0; i4 < smallExpOrbs; i4++)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_SmallExpOrb p)
			{
				p.amount = smallExpOrbAmount;
			});
		}
		if (amount > 0)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance(position, null, default(CastInfo), delegate(Pickup_SmallExpOrb p)
			{
				p.amount = amount;
			});
		}
	}

	private void HandleDeathDrop(EventInfoKill obj)
	{
		if (obj.victim.Status.TryGetStatusEffect<Se_HunterBuff>(out var buff) && !buff.enableGoldAndExpDrops)
		{
			return;
		}
		DewGameSessionSettings gss = NetworkedManagerBase<GameManager>.instance.gss;
		int goldAmount = NetworkedManagerBase<GameManager>.instance.GetGoldDropFromEntity(obj.victim);
		DropGold(isKillGold: true, isGivenByOtherPlayer: false, goldAmount, obj.victim.position);
		for (int i = 0; i < DewPlayer.humanPlayers.Count; i++)
		{
			Hero h = DewPlayer.humanPlayers[i].hero;
			if (!(h == null) && h.level < h.maxLevel)
			{
				int expAmount = (int)NetworkedManagerBase<GameManager>.instance.GetExpDropFromEntity(obj.victim);
				DropExp(expAmount, obj.victim.position);
				break;
			}
		}
		if (!(obj.victim is Monster m))
		{
			return;
		}
		float hpRatioAvg = 0f;
		int hpRatioCount = 0;
		foreach (DewPlayer h2 in DewPlayer.humanPlayers)
		{
			if (!h2.hero.IsNullOrInactive() && !h2.hero.isKnockedOut)
			{
				hpRatioAvg += h2.hero.currentHealth / h2.hero.maxHealth;
				hpRatioCount++;
			}
		}
		float chance = Mathf.Lerp(t: (hpRatioCount != 0) ? (hpRatioAvg / (float)hpRatioCount) : 1f, a: gss.regenOrbDropChanceOnLowHealth.Get(m.type), b: gss.regenOrbDropChanceOnMaxHealth.Get(m.type)) * NetworkedManagerBase<GameManager>.instance.difficulty.regenOrbChanceMultiplier;
		Hero fe = obj.actor.FindFirstOfType<Hero>();
		if (fe != null && fe.owner != null && fe.owner.isHumanPlayer)
		{
			chance *= fe.owner.potionDropChanceMultiplier;
		}
		if (UnityEngine.Random.value < chance)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateAbilityInstance<Pickup_RegenOrb>(m.position, null, default(CastInfo));
		}
		if (!isStardustDropDisabled && UnityEngine.Random.value < gss.stardustDeathDropChance.Get(m.type))
		{
			int stardust = UnityEngine.Random.Range(gss.stardustDeathDropAmount.x, gss.stardustDeathDropAmount.y + 1);
			stardust += NetworkedManagerBase<GameManager>.instance.difficulty.gainedStardustAmountOffset;
			DropStarDust(stardust, obj.victim.position);
		}
	}

	private void MirrorProcessed()
	{
	}
}
