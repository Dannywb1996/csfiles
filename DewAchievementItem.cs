using System;
using System.Collections.Generic;
using UnityEngine;

public class DewAchievementItem : DewGameObserverWithProgress
{
	public virtual int grantedStardust => 15;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth) || !DewSave.profile.achievements.TryGetValue(GetType().Name, out var data))
		{
			return;
		}
		try
		{
			LoadState(data.persistentVariables);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth) || !DewSave.profile.achievements.TryGetValue(GetType().Name, out var data))
		{
			return;
		}
		if (data.persistentVariables == null)
		{
			data.persistentVariables = new Dictionary<string, string>();
		}
		try
		{
			string achKey = base.name;
			DewProfile.AchievementData achievementData = DewSave.profile.achievements[achKey];
			achievementData.currentProgress = GetCurrentProgress();
			achievementData.maxProgress = GetMaxProgress();
			SaveState(data.persistentVariables);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void OnComplete()
	{
		base.OnComplete();
		ManagerBase<AchievementManager>.instance.CompleteAchievement(this);
	}
}
