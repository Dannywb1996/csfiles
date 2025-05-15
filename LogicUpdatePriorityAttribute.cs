using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class LogicUpdatePriorityAttribute : Attribute
{
	public readonly int priority;

	public LogicUpdatePriorityAttribute(int priority = 0)
	{
		this.priority = priority;
	}
}
