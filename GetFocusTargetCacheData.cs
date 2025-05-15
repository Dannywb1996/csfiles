using System.Collections.Generic;
using UnityEngine;

public struct GetFocusTargetCacheData
{
	public List<(IGamepadFocusable, Rect)> candidateFocusables { get; internal set; }

	public List<(UI_GamepadFocusableGroup, Rect)> candidateGroups { get; internal set; }
}
