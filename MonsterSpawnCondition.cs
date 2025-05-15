using System;
using UnityEngine;

[Serializable]
public class MonsterSpawnCondition
{
	public MonsterSpawnConditionType type;

	public float conditionValueMin;

	public float conditionValueMax = float.PositiveInfinity;

	public int conditionValueMinInt => Mathf.RoundToInt(conditionValueMin);

	public int conditionValueMaxInt
	{
		get
		{
			if (!(conditionValueMax > 2.1474836E+09f))
			{
				return Mathf.RoundToInt(conditionValueMax);
			}
			return int.MaxValue;
		}
	}

	public bool EvaluateCondition()
	{
		switch (type)
		{
		case MonsterSpawnConditionType.CurrentZoneClearedNodes:
			if (NetworkedManagerBase<ZoneManager>.instance.currentZoneClearedNodes >= conditionValueMinInt)
			{
				return NetworkedManagerBase<ZoneManager>.instance.currentZoneClearedNodes <= conditionValueMaxInt;
			}
			return false;
		case MonsterSpawnConditionType.GlobalRoomIndex:
			if (NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex >= conditionValueMinInt)
			{
				return NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex <= conditionValueMaxInt;
			}
			return false;
		case MonsterSpawnConditionType.ActivatedCombatAreas:
		{
			int v = NetworkedManagerBase<ZoneManager>.instance.currentRoom.numOfActivatedCombatAreas;
			if (v >= conditionValueMinInt)
			{
				return v <= conditionValueMaxInt;
			}
			return false;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
