using System;
using System.Collections.Generic;

namespace DewInternal;

[Serializable]
public class PerLanguageLocalizationData
{
	public Dictionary<string, string> ui;

	public Dictionary<string, SkillData> skills;

	public Dictionary<string, GemData> gems;

	public Dictionary<string, AchievementData> achievements;

	public Dictionary<string, CurseData> curses;

	public Dictionary<string, string> tips;

	public Dictionary<string, StarData> stars;

	public Dictionary<string, ConversationData> conversations;

	public Dictionary<string, ArtifactData> artifacts;

	public Dictionary<string, TreasureData> treasures;
}
