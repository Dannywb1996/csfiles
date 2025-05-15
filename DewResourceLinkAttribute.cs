using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DewResourceLinkAttribute : Attribute
{
	public ResourceLinkBy by;

	public DewResourceLinkAttribute(ResourceLinkBy by)
	{
		this.by = by;
	}
}
