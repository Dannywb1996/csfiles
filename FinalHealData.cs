public struct FinalHealData
{
	public float amount;

	public float discardedAmount;

	public bool isCrit;

	internal ActorFlags _flags;

	public FinalHealData(HealData data, float maximumAmount)
	{
		amount = data.currentAmount;
		if (amount > maximumAmount)
		{
			discardedAmount = amount - maximumAmount;
			amount = maximumAmount;
		}
		else
		{
			discardedAmount = 0f;
		}
		isCrit = data.isCrit;
		_flags = data._flags.ShallowCopy();
	}
}
