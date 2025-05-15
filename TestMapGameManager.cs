using System.Collections.Generic;
using UnityEngine;

public class TestMapGameManager : GameManager
{
	public static bool moveToAppropriateNode = true;

	public static bool startFromRoomIndex99;

	public static bool simulateLastZone;

	public static string difficultyNameToLoad = "diffNormal";

	public static List<string> lucidDreams = new List<string>();

	private int _nextZoneIndex;

	public static Vector3 spawnPos;

	private float _startTime;

	protected override bool IsLazyCallReady()
	{
		if (base.IsLazyCallReady())
		{
			return Time.time - _startTime > 1f;
		}
		return false;
	}

	protected override DewDifficultySettings GetDifficulty()
	{
		return DewResources.GetByName<DewDifficultySettings>(difficultyNameToLoad);
	}

	public override IList<string> GetLucidDreams()
	{
		return lucidDreams;
	}

	private void MirrorProcessed()
	{
	}
}
