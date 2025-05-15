using System;

public struct EventInfoDamage
{
	public Actor actor;

	public Entity victim;

	public FinalDamageData damage;

	[NonSerialized]
	public ReactionChain chain;
}
