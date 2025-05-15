using System;

public class Pickup_BaseGoldOrb : PickupInstance
{
	[NonSerialized]
	public int amount;

	[NonSerialized]
	public Hero target;

	[NonSerialized]
	public bool isKillGold;

	[NonSerialized]
	public bool isGivenByOtherPlayer;

	public override bool isDestroyedOnRoomChange => false;

	protected override bool CanBeUsedBy(Hero hero)
	{
		if (target != null)
		{
			if (base.CanBeUsedBy(hero))
			{
				return hero == target;
			}
			return false;
		}
		return base.CanBeUsedBy(hero);
	}

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		if (target != null)
		{
			if (!(target.owner == null))
			{
				float fa = amount;
				if (isKillGold)
				{
					fa *= DewBuildProfile.current.killGoldMultiplier;
					fa *= target.owner.monsterKillGoldMultiplier;
				}
				int a = DewMath.RandomRoundToInt(fa);
				if (isGivenByOtherPlayer)
				{
					target.owner.gold += a;
				}
				else
				{
					target.owner.EarnGold(a);
				}
			}
			return;
		}
		float floatAmount = (float)amount / (float)DewPlayer.humanPlayers.Count;
		foreach (DewPlayer player in DewPlayer.humanPlayers)
		{
			float a2 = floatAmount;
			if (isKillGold)
			{
				a2 *= DewBuildProfile.current.killGoldMultiplier;
				a2 *= player.monsterKillGoldMultiplier;
			}
			int ia = DewMath.RandomRoundToInt(a2);
			if (isGivenByOtherPlayer)
			{
				player.gold += ia;
			}
			else
			{
				player.EarnGold(ia);
			}
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += new Action<EventInfoLoadRoom>(DoPickupImmediately);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted -= new Action<EventInfoLoadRoom>(DoPickupImmediately);
		}
	}

	private void DoPickupImmediately(EventInfoLoadRoom obj)
	{
		if (target != null && CanBeUsedBy(target))
		{
			OnPickup(target);
			Destroy();
			return;
		}
		Hero hero = Dew.SelectRandomAliveHero();
		if (hero != null && CanBeUsedBy(hero))
		{
			OnPickup(hero);
			Destroy();
		}
		else
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
