public interface IEntityValidator
{
	private class AlwaysTrueValidator : IEntityValidator
	{
		public bool Evaluate(Entity entity)
		{
			return true;
		}
	}

	static IEntityValidator AlwaysTrue;

	bool Evaluate(Entity entity);

	static IEntityValidator()
	{
		AlwaysTrue = new AlwaysTrueValidator();
	}
}
