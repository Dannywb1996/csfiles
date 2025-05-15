using System;

public class Se_HeroInCombatIcon : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			if (!(base.victim is Hero h))
			{
				Destroy();
				return;
			}
			base.NetworkshowIcon = h.isInCombat;
			h.ClientHeroEvent_OnIsInCombatChanged += new Action<bool>(ClientHeroEventOnIsInCombatChanged);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && base.victim != null && base.victim is Hero h)
		{
			h.ClientHeroEvent_OnIsInCombatChanged -= new Action<bool>(ClientHeroEventOnIsInCombatChanged);
		}
	}

	private void ClientHeroEventOnIsInCombatChanged(bool obj)
	{
		base.NetworkshowIcon = obj;
	}

	private void MirrorProcessed()
	{
	}
}
