public class MockAbilityInstance : AbilityInstance
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
