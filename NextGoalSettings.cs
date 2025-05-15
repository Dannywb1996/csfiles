using System;

public class NextGoalSettings
{
	public string localizedTitleKey;

	public string localizedDescriptionKey;

	public GetNodeIndexSettings nodeIndexSettings;

	public string addedModifier;

	public string modifierData;

	public string roomOverride;

	public bool revertModifierOnRemove;

	public bool revertRoomOnRemove;

	public bool failQuestOnFail;

	public bool dontChangeTitle;

	public bool dontChangeDescription;

	public bool ignoreSuboptimalSituation;

	public Action onReachDestination;

	public Action onFail;
}
