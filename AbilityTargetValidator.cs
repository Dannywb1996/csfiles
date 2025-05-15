using System;

[Serializable]
public class AbilityTargetValidator : IBinaryEntityValidator
{
	public EntityRelation targets = EntityRelation.Neutral | EntityRelation.Enemy;

	public bool Evaluate(Entity self, Entity target)
	{
		if (target == null)
		{
			return false;
		}
		if (self == null)
		{
			return false;
		}
		EntityRelation rel = self.GetRelation(target);
		if (target.isActive && (!target.Status.hasInvisible || rel == EntityRelation.Ally || rel == EntityRelation.Self))
		{
			return (rel & targets) == rel;
		}
		return false;
	}
}
