public static class IHoldableInHandExtension
{
	public static bool IsHoldableObjectNullOrInactive(this IItem ito)
	{
		if (ito != null)
		{
			if (ito is Actor a)
			{
				if (!(a == null))
				{
					return !a.isActive;
				}
				return true;
			}
			return false;
		}
		return true;
	}
}
