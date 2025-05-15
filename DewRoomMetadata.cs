using System.Collections.Generic;
using UnityEngine;

public class DewRoomMetadata : ScriptableObject
{
	public List<MonsterSpawnRule> customRules = new List<MonsterSpawnRule>();

	public List<PropSpawnRule> customPropRules = new List<PropSpawnRule>();

	public float area;
}
