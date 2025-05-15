using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameReverieManager : ManagerBase<InGameReverieManager>
{
	public const int RerollMaxCount = 3;

	public const int SpecialReverieIndex = -99;

	private List<DewReverieItem> _trackedReveries = new List<DewReverieItem>();

	public bool disableTrackingReveries;

	public bool isTrackingReveries { get; private set; }

	private void Start()
	{
		if (disableTrackingReveries)
		{
			return;
		}
		NetworkedManagerBase<GameResultManager>.instance.ClientEvent_OnGameResult += (Action<DewGameResult>)delegate(DewGameResult obj)
		{
			if (isTrackingReveries)
			{
				for (int num = _trackedReveries.Count - 1; num >= 0; num--)
				{
					_trackedReveries[num].FeedGameResult(obj);
				}
				StopTrackingReveries();
			}
		};
		DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
		dewNetworkManager.onSessionEnd = (Action)Delegate.Combine(dewNetworkManager.onSessionEnd, (Action)delegate
		{
			if (isTrackingReveries)
			{
				StopTrackingReveries();
			}
		});
		GameManager.CallOnReady(delegate
		{
			StartTrackingReveries();
			DewPlayer.local.ClientEvent_OnHeroChanged += (Action<Hero, Hero>)delegate
			{
				if (isTrackingReveries)
				{
					StopTrackingReveries();
					StartTrackingReveries();
				}
			};
		});
	}

	private void OnDestroy()
	{
		if (isTrackingReveries)
		{
			StopTrackingReveries();
		}
	}

	internal void StartTrackingReveries()
	{
		if (isTrackingReveries)
		{
			return;
		}
		isTrackingReveries = true;
		for (int i = 0; i < DewSave.profile.reverieSlots.Count; i++)
		{
			try
			{
				DewProfile.DailyReverieData data = DewSave.profile.reverieSlots[i];
				if (!data.IsEmpty() && !data.isComplete)
				{
					DewReverieItem newItem = (DewReverieItem)Activator.CreateInstance(Dew.reveriesByName[data.type].GetType());
					newItem.index = i;
					_trackedReveries.Add(newItem);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		try
		{
			if (!DewSave.profile.specialReverie.IsEmpty() && !DewSave.profile.specialReverie.isComplete)
			{
				DewSpecialReverieItem newItem2 = (DewSpecialReverieItem)Activator.CreateInstance(Dew.reveriesByName[DewSave.profile.specialReverie.type].GetType());
				newItem2.index = -99;
				_trackedReveries.Add(newItem2);
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		for (int i2 = _trackedReveries.Count - 1; i2 >= 0; i2--)
		{
			DewReverieItem r = _trackedReveries[i2];
			try
			{
				r.OnStartLocalClient();
			}
			catch (Exception exception3)
			{
				Debug.LogError("Exception occured while starting reverie: " + r.name);
				Debug.LogException(exception3, this);
				_trackedReveries.RemoveAt(i2);
			}
		}
		Debug.Log($"Started tracking {_trackedReveries.Count} reveries");
	}

	internal void StopTrackingReveries()
	{
		if (!isTrackingReveries)
		{
			return;
		}
		isTrackingReveries = false;
		for (int i = _trackedReveries.Count - 1; i >= 0; i--)
		{
			DewReverieItem r = _trackedReveries[i];
			try
			{
				r.OnStopLocalClient();
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while stopping reverie: " + r.name);
				Debug.LogException(exception, this);
			}
		}
		Debug.Log($"Stopped tracking {_trackedReveries.Count} reveries");
		_trackedReveries.Clear();
	}

	public void CompleteReverie(DewReverieItem item)
	{
		if (item == null)
		{
			return;
		}
		DewProfile.ReverieDataBase data = ((item.index == -99) ? ((DewProfile.ReverieDataBase)DewSave.profile.specialReverie) : ((DewProfile.ReverieDataBase)DewSave.profile.reverieSlots[item.index]));
		if (data.isComplete)
		{
			Debug.Log(item.name + " already complete");
			return;
		}
		Debug.Log(item.name + " completed");
		data.isComplete = true;
		data.maxProgress = item.GetMaxProgress();
		data.currentProgress = data.maxProgress;
		data.persistentVariables = null;
		try
		{
			item.OnStopLocalClient();
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception occured while stopping reverie: " + item.name);
			Debug.LogException(exception, this);
		}
		DewSave.SaveProfile();
		_trackedReveries.Remove(item);
	}
}
