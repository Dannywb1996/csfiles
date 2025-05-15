public class Se_HunterBuff_ShadowWalk_Disappear : StatusEffect
{
	public float duration;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			base.info.caster.Visual.DisableRenderers();
			DoUnstoppable();
			SetTimer(duration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			base.info.caster.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
