using System;

[AttributeUsage(AttributeTargets.Class)]
public class AchUnlockOnComplete : Attribute
{
	public Type targetType { get; private set; }

	public AchUnlockOnComplete(Type targetType)
	{
		this.targetType = targetType;
	}
}
