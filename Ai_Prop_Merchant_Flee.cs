public class Ai_Prop_Merchant_Flee : AbilityInstance
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			base.info.caster.Destroy();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
