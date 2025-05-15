using UnityEngine;

internal static class ILogicUpdateNullCheckExtension
{
	internal static bool IsNull(this ILogicUpdate logic)
	{
		if (logic is Object unityObj)
		{
			return unityObj == null;
		}
		return logic == null;
	}
}
