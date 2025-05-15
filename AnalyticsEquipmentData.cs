using System;
using System.Collections.Generic;
using System.IO;

public struct AnalyticsEquipmentData
{
	private const ushort CurrentVersion = 0;

	public List<(HeroSkillLocation, string, int)> skills;

	public List<(GemLocation, string, int)> gems;

	public AnalyticsEquipmentData(Hero hero)
	{
		List<(HeroSkillLocation, string, int)> newSkills = new List<(HeroSkillLocation, string, int)>();
		List<(GemLocation, string, int)> newGems = new List<(GemLocation, string, int)>();
		ProcessSkill(HeroSkillLocation.Q);
		ProcessSkill(HeroSkillLocation.W);
		ProcessSkill(HeroSkillLocation.E);
		ProcessSkill(HeroSkillLocation.R);
		ProcessSkill(HeroSkillLocation.Identity);
		foreach (KeyValuePair<GemLocation, Gem> g in hero.Skill.gems)
		{
			if (ItemNumericIdDatabase.instance.TryGetHash(g.Value.GetType().Name, out var _))
			{
				newGems.Add((g.Key, g.Value.GetType().Name, g.Value.quality));
			}
		}
		skills = newSkills;
		gems = newGems;
		void ProcessSkill(HeroSkillLocation type)
		{
			if (hero.Skill.TryGetSkill(type, out var skill) && ItemNumericIdDatabase.instance.TryGetHash(skill.GetType().Name, out var _))
			{
				newSkills.Add((type, skill.GetType().Name, skill.level));
			}
		}
	}

	public AnalyticsEquipmentData(string base64)
	{
		skills = new List<(HeroSkillLocation, string, int)>();
		gems = new List<(GemLocation, string, int)>();
		using MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64));
		using BinaryReader reader = new BinaryReader(stream);
		if (reader.ReadUInt16() == 0)
		{
			int skillsCount = reader.ReadInt32();
			for (int i = 0; i < skillsCount; i++)
			{
				HeroSkillLocation loc = (HeroSkillLocation)reader.ReadByte();
				uint id = reader.ReadUInt32();
				int level = reader.ReadInt32();
				if (ItemNumericIdDatabase.instance.TryGetItem(id, out var item))
				{
					skills.Add((loc, item, level));
				}
			}
			int gemsCount = reader.ReadInt32();
			for (int j = 0; j < gemsCount; j++)
			{
				GemLocation loc2 = new GemLocation((HeroSkillLocation)reader.ReadByte(), reader.ReadByte());
				uint id2 = reader.ReadUInt32();
				int quality = reader.ReadInt32();
				if (ItemNumericIdDatabase.instance.TryGetItem(id2, out var item2))
				{
					gems.Add((loc2, item2, quality));
				}
			}
			return;
		}
		throw new InvalidOperationException("Data format not supported");
	}

	public string ToBase64()
	{
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(stream);
		writer.Write((ushort)0);
		writer.Write(skills.Count);
		foreach (var s in skills)
		{
			writer.Write((byte)s.Item1);
			ItemNumericIdDatabase.instance.TryGetHash(s.Item2, out var id);
			writer.Write(id);
			writer.Write(s.Item3);
		}
		writer.Write(gems.Count);
		foreach (var g in gems)
		{
			writer.Write((byte)g.Item1.skill);
			writer.Write((byte)g.Item1.index);
			ItemNumericIdDatabase.instance.TryGetHash(g.Item2, out var id2);
			writer.Write(id2);
			writer.Write(g.Item3);
		}
		return Convert.ToBase64String(stream.ToArray());
	}
}
