using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Game Content Settings", menuName = "Game Content Settings")]
public class DewGameContentSettings : ScriptableObject
{
	public DewGameContentSettings parent;

	[NonSerialized]
	public List<string> includedZones;

	[NonSerialized]
	public List<int> zoneCountByTier;

	[NonSerialized]
	public List<string> availableSkills;

	[NonSerialized]
	public List<string> availableGems;

	[NonSerialized]
	public List<string> availableArtifacts;

	[NonSerialized]
	public List<string> availableHeroes;

	[NonSerialized]
	public List<string> availableGameModifiers;

	[NonSerialized]
	public List<string> availableRoomModifiers;

	[NonSerialized]
	public List<string> availableCurses;

	[NonSerialized]
	public List<string> availableLucidDreams;

	[NonSerialized]
	public List<string> availableTreasures;

	[NonSerialized]
	public List<string> lockedStars;

	[NonSerialized]
	public List<string> excludedMonsters;

	[FormerlySerializedAs("includedZones")]
	[SerializeField]
	private string[] _includedZones = new string[0];

	[FormerlySerializedAs("zoneCountByTier")]
	[SerializeField]
	private int[] _zoneCountByTier = new int[1] { 1 };

	[FormerlySerializedAs("availableSkills")]
	[SerializeField]
	private string[] _availableSkills = new string[0];

	[FormerlySerializedAs("availableGems")]
	[SerializeField]
	private string[] _availableGems = new string[0];

	[FormerlySerializedAs("availableArtifacts")]
	[SerializeField]
	private string[] _availableArtifacts = new string[0];

	[FormerlySerializedAs("availableHeroes")]
	[SerializeField]
	private string[] _availableHeroes = new string[0];

	[FormerlySerializedAs("availableGameModifiers")]
	[SerializeField]
	private string[] _availableGameModifiers = new string[0];

	[FormerlySerializedAs("availableRoomModifiers")]
	[SerializeField]
	private string[] _availableRoomModifiers = new string[0];

	[FormerlySerializedAs("availableCurses")]
	[SerializeField]
	private string[] _availableCurses = new string[0];

	[FormerlySerializedAs("availableLucidDreams")]
	[SerializeField]
	private string[] _availableLucidDreams = new string[0];

	[SerializeField]
	private string[] _availableTreasures = new string[0];

	[FormerlySerializedAs("lockedStars")]
	[SerializeField]
	private string[] _lockedStars = new string[0];

	[SerializeField]
	private string[] _excludedMonsters = new string[0];

	public string[] includedRooms = new string[0];

	public string[] includedMonsters = new string[0];

	public void Init()
	{
		List<DewGameContentSettings> profiles = new List<DewGameContentSettings>();
		DewGameContentSettings cursor = this;
		while (cursor != null)
		{
			profiles.Insert(0, cursor);
			cursor = cursor.parent;
			if (profiles.Count > 100)
			{
				Debug.LogError("Cyclic parenting detected", this);
				return;
			}
		}
		foreach (DewGameContentSettings item in profiles)
		{
			item.Validate();
		}
		includedZones = new List<string>();
		zoneCountByTier = new List<int>();
		availableSkills = new List<string>();
		availableGems = new List<string>();
		availableArtifacts = new List<string>();
		availableHeroes = new List<string>();
		availableGameModifiers = new List<string>();
		availableRoomModifiers = new List<string>();
		availableCurses = new List<string>();
		availableLucidDreams = new List<string>();
		availableTreasures = new List<string>();
		lockedStars = new List<string>();
		excludedMonsters = new List<string>();
		foreach (DewGameContentSettings p in profiles)
		{
			Add(p._includedZones, includedZones);
			Add(p._availableSkills, availableSkills);
			Add(p._availableGems, availableGems);
			Add(p._availableArtifacts, availableArtifacts);
			Add(p._availableHeroes, availableHeroes);
			Add(p._availableGameModifiers, availableGameModifiers);
			Add(p._availableRoomModifiers, availableRoomModifiers);
			Add(p._availableCurses, availableCurses);
			Add(p._availableLucidDreams, availableLucidDreams);
			Add(p._availableTreasures, availableTreasures);
			Add(p._lockedStars, lockedStars);
			Add(p._excludedMonsters, excludedMonsters);
			if (p._zoneCountByTier.Length != 0)
			{
				zoneCountByTier.Clear();
				zoneCountByTier.AddRange(p._zoneCountByTier);
			}
		}
		static void Add(string[] from, List<string> to)
		{
			foreach (string f in from)
			{
				if (f == "Clear")
				{
					to.Clear();
				}
				else if (f.StartsWith("!"))
				{
					to.Remove(f.Substring(1));
				}
				else if (!to.Contains(f))
				{
					to.Add(f);
				}
			}
		}
	}

	public string[] FilterZones(string[] zones)
	{
		List<string> list = new List<string>(zones);
		if (includedZones != null && includedZones.Count > 0)
		{
			List<string> z = includedZones;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (!z.Contains(list[i]))
				{
					list.RemoveAt(i);
				}
			}
		}
		return list.ToArray();
	}

	public Zone[] FilterZones(Zone[] zones)
	{
		List<Zone> list = new List<Zone>(zones);
		if (includedZones != null && includedZones.Count > 0)
		{
			List<string> z = includedZones;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (!z.Contains(list[i].name))
				{
					list.RemoveAt(i);
				}
			}
		}
		return list.ToArray();
	}

	private void PrintValidationResult()
	{
		try
		{
			Validate();
			Debug.Log("No problem has been found in content settings: " + base.name);
		}
		catch (Exception exception)
		{
			Debug.LogError("Problem has been found in content settings: " + base.name);
			Debug.LogException(exception);
		}
	}

	public void Validate()
	{
	}

	private void OnValidate()
	{
		Validate();
	}
}
