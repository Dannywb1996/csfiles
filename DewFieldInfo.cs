using System;

namespace DewInternal;

public struct DewFieldInfo
{
	public bool requiresTarget;

	public Func<object, object>[] valueGetters;

	public bool isProperty;
}
