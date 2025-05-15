public struct EventInfoHeal
{
	public Actor actor;

	public Entity target;

	public FinalHealData heal;

	public float amount;

	public float discardedAmount;

	public bool isCrit;

	public bool canMerge;

	public ReactionChain chain;
}
