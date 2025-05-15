using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroLoadoutData
{
	public const int HeroLoadoutCount = 5;

	public int skillQ;

	public int skillR;

	public int skillTrait;

	public int skillMovement;

	public List<LoadoutStarItem> cDestruction = new List<LoadoutStarItem>();

	public List<LoadoutStarItem> cLife = new List<LoadoutStarItem>();

	public List<LoadoutStarItem> cImagination = new List<LoadoutStarItem>();

	public List<LoadoutStarItem> cFlexible = new List<LoadoutStarItem>();

	public HeroLoadoutData()
	{
	}

	public HeroLoadoutData(HeroLoadoutData source)
	{
		skillQ = source.skillQ;
		skillR = source.skillR;
		skillTrait = source.skillTrait;
		skillMovement = source.skillMovement;
		cDestruction = new List<LoadoutStarItem>(source.cDestruction);
		cLife = new List<LoadoutStarItem>(source.cLife);
		cImagination = new List<LoadoutStarItem>(source.cImagination);
		cFlexible = new List<LoadoutStarItem>(source.cFlexible);
	}

	public List<LoadoutStarItem> GetStarList(StarType type)
	{
		return type switch
		{
			StarType.Destruction => cDestruction, 
			StarType.Life => cLife, 
			StarType.Imagination => cImagination, 
			StarType.Flexible => cFlexible, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public int GetSkill(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => skillQ, 
			HeroSkillLocation.R => skillR, 
			HeroSkillLocation.Identity => skillTrait, 
			HeroSkillLocation.Movement => skillMovement, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public void SetSkill(HeroSkillLocation type, int value)
	{
		switch (type)
		{
		case HeroSkillLocation.Q:
			skillQ = value;
			break;
		case HeroSkillLocation.R:
			skillR = value;
			break;
		case HeroSkillLocation.Identity:
			skillTrait = value;
			break;
		case HeroSkillLocation.Movement:
			skillMovement = value;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	public void PopulateLevelsByLocalSaveData()
	{
		PopulateLevels(StarType.Destruction);
		PopulateLevels(StarType.Life);
		PopulateLevels(StarType.Imagination);
		PopulateLevels(StarType.Flexible);
		void PopulateLevels(StarType type)
		{
			List<LoadoutStarItem> list = GetStarList(type);
			for (int i = 0; i < list.Count; i++)
			{
				if (!string.IsNullOrEmpty(list[i].name))
				{
					LoadoutStarItem temp = list[i];
					temp.level = DewSave.profile.newStars[temp.name].level;
					list[i] = temp;
				}
			}
		}
	}

	public bool IsValidFor(string heroType)
	{
		return Validate_Imp(heroType, isRepair: false, checkStarLevels: true, null);
	}

	public bool Validate_Imp(string heroType, bool isRepair, bool checkStarLevels, DewProfile.HeroStarSlotUnlockData unlockedSlots)
	{
		bool isValid = true;
		Hero hero = DewResources.GetByShortTypeName<Hero>(heroType);
		if (hero == null)
		{
			return false;
		}
		HeroSkill skill = hero.GetComponent<HeroSkill>();
		CheckSkill(ref skillQ, skill.loadoutQ);
		CheckSkill(ref skillR, skill.loadoutR);
		CheckSkill(ref skillTrait, skill.loadoutTrait);
		CheckSkill(ref skillMovement, skill.loadoutMovement);
		if (!isValid && !isRepair)
		{
			return false;
		}
		ValidateConstellation(StarType.Destruction, ref cDestruction);
		ValidateConstellation(StarType.Life, ref cLife);
		ValidateConstellation(StarType.Imagination, ref cImagination);
		ValidateConstellation(StarType.Flexible, ref cFlexible);
		if (!isValid && !isRepair)
		{
			return false;
		}
		return isValid;
		void CheckSkill(ref int current, SkillTrigger[] skills)
		{
			if (current < 0 || current >= skills.Length)
			{
				if (isRepair)
				{
					current = Mathf.Clamp(current, 0, skills.Length - 1);
				}
				isValid = false;
			}
		}
		void ValidateConstellation(StarType type, ref List<LoadoutStarItem> stars)
		{
			if (stars == null)
			{
				isValid = false;
				if (!isRepair)
				{
					return;
				}
				stars = new List<LoadoutStarItem>();
			}
			HeroConstellationSettings s = hero.GetConstellationSettings(type);
			if (isRepair)
			{
				while (stars.Count < s.maxCount)
				{
					stars.Add(default(LoadoutStarItem));
					isValid = false;
				}
				while (stars.Count > s.maxCount)
				{
					stars.RemoveAt(stars.Count - 1);
					isValid = false;
				}
			}
			else if (stars.Count != s.maxCount)
			{
				isValid = false;
				return;
			}
			HashSet<string> dupChecker = new HashSet<string>();
			for (int i = 0; i < stars.Count; i++)
			{
				if (!string.IsNullOrEmpty(stars[i].name))
				{
					if (!dupChecker.Add(stars[i].name))
					{
						isValid = false;
						if (!isRepair)
						{
							break;
						}
						stars[i] = default(LoadoutStarItem);
					}
					else
					{
						StarEffect starPrefab = DewResources.GetByShortTypeName<StarEffect>(stars[i].name);
						if (starPrefab == null || (starPrefab.heroType != null && starPrefab.heroType.Name != heroType))
						{
							isValid = false;
							if (!isRepair)
							{
								break;
							}
							stars[i] = default(LoadoutStarItem);
						}
						else
						{
							if (unlockedSlots != null)
							{
								List<int> slots = unlockedSlots.Get(type);
								if (i >= s.defaultCount && !slots.Contains(i))
								{
									isValid = false;
									if (!isRepair)
									{
										break;
									}
									stars[i] = default(LoadoutStarItem);
									continue;
								}
							}
							if (checkStarLevels && starPrefab.type != StarType.Flexible && (stars[i].level < 1 || stars[i].level > starPrefab.maxStarLevel))
							{
								isValid = false;
								if (!isRepair)
								{
									break;
								}
								LoadoutStarItem temp = stars[i];
								temp.level = Mathf.Clamp(temp.level, 1, starPrefab.maxStarLevel);
								stars[i] = temp;
							}
						}
					}
				}
			}
		}
	}
}
