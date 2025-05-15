public class Se_HeroDeathInterrupt : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		DoDeathInterrupt(delegate
		{
			base.victim.Status.SetHealth(0.01f);
			if (NetworkedManagerBase<GameManager>.instance.difficulty.enableBleedOuts)
			{
				if (!base.victim.Status.HasStatusEffect<Se_HeroKnockedOut>() && !base.victim.Status.HasStatusEffect<Se_HeroBleedingOut>())
				{
					CreateStatusEffect<Se_HeroBleedingOut>(base.victim);
				}
			}
			else if (!base.victim.Status.HasStatusEffect<Se_HeroKnockedOut>())
			{
				CreateStatusEffect<Se_HeroKnockedOut>(base.victim);
			}
		}, 1000);
	}

	private void MirrorProcessed()
	{
	}
}
