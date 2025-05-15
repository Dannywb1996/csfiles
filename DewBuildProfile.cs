using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Build Profile", menuName = "Build Profile")]
public class DewBuildProfile : ScriptableObject
{
	public static string CurrentBuildProfileAssetPath = "Assets/Resources/CurrentBuild.asset";

	public static string EditorBuildProfileDefault = "Assets/Res/BuildProfiles/Profile - Release.asset";

	public BuildType buildType;

	public PlatformType platform;

	public DewGameContentSettings content;

	public int defaultStardustAmount;

	public float killGoldMultiplier = 1f;

	public float dismantleDreamDustMultiplier = 1f;

	public float stardustGainMultiplier = 1f;

	public int worldNodeCountOffset;

	public RoomRewardFlowItemType[] customRewardFlowFirstTime;

	public RoomRewardFlowItemType[] customRewardFlow;

	public float bonusMemoryHaste;

	public int startGold;

	public int startDreamDust;

	public string savePrefix;

	public string codenameSuffix;

	public BuildFeatureTag[] featureTags = new BuildFeatureTag[0];

	public bool isDevelopmentBuild;

	public bool disableAnalytics;

	public bool useSteamLobbyAndRelay;

	public static DewBuildProfile current
	{
		get
		{
			DewBuildProfile profile = Resources.Load<DewBuildProfile>("CurrentBuild");
			if (profile.content.zoneCountByTier == null || profile.content.zoneCountByTier.Count == 0)
			{
				profile.Init();
			}
			return profile;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void PrepareBuildProfile()
	{
		current.Init();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void LogCurrentBuildProfile()
	{
		Debug.Log("Using Build Profile: " + current.name);
	}

	public string GetBundleVersion(string codename)
	{
		if (!string.IsNullOrEmpty(codenameSuffix))
		{
			codename += codenameSuffix;
		}
		DateTime now = DateTime.Now;
		string btype = buildType.ToString().ToLower();
		if (string.IsNullOrEmpty(codename))
		{
			return $"{btype}.{now:yyMMddHH}_{platform.ToString().ToLower()}";
		}
		return $"{btype}.{codename}_{platform.ToString().ToLower()}.{now:yyMMddHH}";
	}

	public string GetFolderName(string codename)
	{
		if (!string.IsNullOrEmpty(codenameSuffix))
		{
			codename += codenameSuffix;
		}
		DateTime now = DateTime.Now;
		string shortBType = buildType switch
		{
			BuildType.Indev => "i", 
			BuildType.DemoLite => "dl", 
			BuildType.DemoPrivate => "dp", 
			BuildType.Release => "r", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		string plat = platform.ToString().ToLower();
		if (string.IsNullOrEmpty(codename))
		{
			return $"ShapeOfDreams-{shortBType}-{now:yyMMddHH}_{plat}";
		}
		return $"ShapeOfDreams-{shortBType}-{now:yyMMddHH}-{codename}_{plat}";
	}

	public string[] GetExtraScriptingDefines()
	{
		List<string> list = new List<string>();
		switch (buildType)
		{
		case BuildType.Indev:
			list.Add("DEW_INDEV");
			break;
		case BuildType.DemoLite:
			list.Add("DEW_DEMOLITE");
			break;
		case BuildType.DemoPrivate:
			list.Add("DEW_DEMOPRIVATE");
			break;
		case BuildType.Release:
			list.Add("DEW_RELEASE");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		switch (platform)
		{
		case PlatformType.DRMFREE:
			list.Add("DEW_DRMFREE");
			break;
		case PlatformType.STEAM:
			list.Add("DEW_STEAM");
			break;
		case PlatformType.STOVE:
			list.Add("DEW_STOVE");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return list.ToArray();
	}

	public bool HasFeature(BuildFeatureTag tag)
	{
		if (featureTags == null)
		{
			return false;
		}
		return featureTags.Contains(tag);
	}

	public void Validate()
	{
		if (!Application.isPlaying)
		{
			if (content == null)
			{
				throw new Exception("Game content not set");
			}
			content.Validate();
		}
	}

	private void PrintValidationResult()
	{
		try
		{
			Validate();
			Debug.Log("No problem has been found in profile: " + base.name);
		}
		catch (Exception exception)
		{
			Debug.LogError("Problem has been found in profile: " + base.name);
			Debug.LogException(exception);
		}
	}

	private void OnValidate()
	{
		Validate();
	}

	public void Init()
	{
		content.Init();
	}
}
