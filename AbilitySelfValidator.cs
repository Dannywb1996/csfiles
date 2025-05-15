using System;

[Serializable]
public class AbilitySelfValidator : IEntityValidator
{
	public static AbilitySelfValidator Default = new AbilitySelfValidator();

	public bool isMovementAbility;

	public bool allowWhileDashing;

	public bool allowWhileDisabled;

	public bool Evaluate(Entity self)
	{
		if (self != null && self.isActive && (allowWhileDisabled || (!self.Control.isAirborne && !self.Status.hasStun && !self.Status.hasSilence)) && (allowWhileDashing || !self.Control.isDashing))
		{
			if (isMovementAbility)
			{
				return !self.Status.hasRoot;
			}
			return true;
		}
		return false;
	}
}
