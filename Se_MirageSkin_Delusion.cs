using System;

public class Se_MirageSkin_Delusion : Se_MirageSkin
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			base.victim.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.victim is Hero h)
		{
			int addedStack = (h.IsMeleeHero() ? 30 : 90);
			if (obj.victim.Status.TryGetStatusEffect<Se_MirageSkin_Delusion_Delusional>(out var delusional))
			{
				delusional.AddStack(addedStack);
				delusional.ResetTimer();
				delusional.FxPlayNetworked(delusional.fxStackUpdated, base.victim);
			}
			else
			{
				obj.victim.CreateStatusEffect<Se_MirageSkin_Delusion_Delusional>(obj.victim, new CastInfo(obj.victim)).SetStack(addedStack);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && !(base.victim == null))
		{
			base.victim.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void MirrorProcessed()
	{
	}
}
