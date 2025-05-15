using System.Collections.Generic;
using DewInternal;

public static class NodeListExtensions
{
	public static string ToText(this List<LocaleNode> list, CurseStatusEffect curse)
	{
		DewLocalization.DescriptionSettings settings = new DewLocalization.DescriptionSettings
		{
			currentLevel = curse.currentStrength.GetValueIndex() + 1
		};
		return DewLocalization.ConvertDescriptionNodesToText(list, settings);
	}
}
