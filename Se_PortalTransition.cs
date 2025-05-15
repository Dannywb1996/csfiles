using UnityEngine;

public class Se_PortalTransition : StatusEffect
{
	public GameObject disappearEffect;

	public bool playDisappearEffect;

	public float healMissingHealthRatio;

	public float healMaxHealthRatio;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			if (playDisappearEffect)
			{
				FxPlayNetworked(disappearEffect, base.victim);
			}
			DoProtected(null);
			DoInvisible();
			DoUncollidable();
			DoRoot();
			DoSilence();
			base.victim.Control.Stop();
			base.victim.Visual.DisableRenderers();
			base.victim.Control.CancelOngoingChannels();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && base.victim != null)
		{
			if (base.victim.isAlive)
			{
				base.victim.Status.SetHealth(base.victim.currentHealth + base.victim.maxHealth * healMaxHealthRatio + base.victim.Status.missingHealth * healMissingHealthRatio);
			}
			base.victim.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
