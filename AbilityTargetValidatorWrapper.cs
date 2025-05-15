public class AbilityTargetValidatorWrapper : IEntityValidator
{
	public Entity self;

	private AbilityTargetValidator _validator;

	public AbilityTargetValidatorWrapper(Entity self, EntityRelation targets)
	{
		this.self = self;
		_validator = new AbilityTargetValidator
		{
			targets = targets
		};
	}

	public bool Evaluate(Entity entity)
	{
		return _validator.Evaluate(self, entity);
	}
}
