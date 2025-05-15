using System;

public class Ai_ReviveHero : AbilityInstance
{
	[NonSerialized]
	public float reviveHealthMultiplier = -1f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		if (base.info.target != null)
		{
			if (base.info.target.Status.TryGetStatusEffect<Se_HeroKnockedOut>(out var knock))
			{
				knock.Revive(reviveHealthMultiplier);
			}
			CreateBasicEffect(base.info.target, new InvulnerableEffect(), 4f, "reviveInvul");
			CreateBasicEffect(base.info.target, new InvisibleEffect(), 2f, "reviveInvis");
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
