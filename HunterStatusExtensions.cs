public static class HunterStatusExtensions
{
	public static int ToLevel(this HunterStatus status)
	{
		if (status < HunterStatus.Level1)
		{
			return 0;
		}
		return (int)(status - 99);
	}
}
