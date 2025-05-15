public static class ActorCheck
{
	public static bool IsNullOrInactive(this Actor a)
	{
		if (!(a == null))
		{
			return !a.isActive;
		}
		return true;
	}
}
