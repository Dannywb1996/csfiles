using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DewReverieItem : DewGameObserverWithProgress, IExcludeFromPool
{
	public const int ReverieSlotCount = 3;

	public int index;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public abstract int grantedStardust { get; }

	public virtual string[] grantedItems { get; }

	public virtual bool excludeFromPool => false;

	public virtual string reverieListButtonText => null;

	public virtual void OnReverieListButtonClick()
	{
	}

	public virtual void OnSetupReverie()
	{
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		try
		{
			LoadState(DewSave.profile.reverieSlots[index].persistentVariables);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		try
		{
			SaveReverieStateToData(DewSave.profile.reverieSlots[index]);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void SaveReverieStateToData(DewProfile.ReverieDataBase data)
	{
		try
		{
			if (data.persistentVariables == null)
			{
				data.persistentVariables = new Dictionary<string, string>();
			}
			data.currentProgress = GetCurrentProgress();
			data.maxProgress = GetMaxProgress();
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
		if (ManagerBase<InGameReverieManager>.instance != null)
		{
			ManagerBase<InGameReverieManager>.instance.CompleteReverie(this);
			return;
		}
		DewProfile.DailyReverieData data = DewSave.profile.reverieSlots[index];
		if (data.isComplete)
		{
			Debug.Log(base.name + " already complete");
			return;
		}
		Debug.Log(base.name + " completed");
		data.isComplete = true;
		data.maxProgress = GetMaxProgress();
		data.currentProgress = data.maxProgress;
		data.persistentVariables = null;
		try
		{
			OnStopLocalClient();
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception occured while stopping reverie: " + base.name);
			Debug.LogException(exception);
		}
		DewSave.SaveProfile();
	}
}
