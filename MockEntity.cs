public class MockEntity : Entity
{
	public string conversationNameUIKey;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			CreateBasicEffect(this, new InvulnerableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new InvisibleEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new UncollidableEffect(), float.PositiveInfinity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
