using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class DewReverie
{
	public static void CheckSpecialReveries()
	{
		if (DewSave.profile.specialReverie.IsEmpty() && CheckSpecialReveries_NextFest())
		{
			DewSave.SaveProfile();
		}
	}

	public static async UniTask<bool> ReceiveRewardOfReverie(DewProfile.ReverieDataBase data)
	{
		try
		{
			if (data == null)
			{
				return false;
			}
			if (data.grantedItems != null && !(await DewItem.GenerateItems(data.grantedItems.ToList())))
			{
				return false;
			}
			DewSave.profile.completedReveries++;
			DewSave.profile.stardust += data.grantedStardust;
			return true;
		}
		catch (Exception e)
		{
			DewSessionError.ShowError(e, isFatal: false, isGame: false);
			return false;
		}
		finally
		{
			if (ManagerBase<TransitionManager>.instance != null)
			{
				ManagerBase<TransitionManager>.instance.SetBusy(value: false);
			}
		}
	}

	public static void ClearDailyReverie(int index)
	{
		DewSave.profile.reverieSlots[index] = new DewProfile.DailyReverieData
		{
			type = null,
			nextRefillTimestamp = ((DateTimeOffset)DateTime.Now.AddHours(6.0)).ToUnixTimeSeconds()
		};
		DewSave.SaveProfile();
	}

	public static void StartNextSpecialReverieOrClear()
	{
		if (Dew.reveriesByName.TryGetValue(DewSave.profile.specialReverie.type, out var rev) && rev is DewSpecialReverieItem s && s.nextReverie != null)
		{
			SetSpecialReverie(s.nextReverie);
		}
		else
		{
			ClearSpecialReverie();
		}
	}

	public static void ClearSpecialReverie()
	{
		DewSave.profile.specialReverie = new DewProfile.SpecialReverieData
		{
			type = null
		};
		DewSave.SaveProfile();
	}

	public static void SetSpecialReverie(Type type)
	{
		DewSpecialReverieItem ins = (DewSpecialReverieItem)Activator.CreateInstance(type);
		ins.OnSetupReverie();
		DewProfile.SpecialReverieData data = new DewProfile.SpecialReverieData
		{
			type = type.Name,
			persistentVariables = new Dictionary<string, string>(),
			grantedStardust = ins.grantedStardust,
			grantedItems = ins.grantedItems,
			isComplete = false,
			timeLimitTimestamp = DateTime.Now.Add(ins.timeLimit).ToTimestamp()
		};
		ins.SaveReverieStateToData(data);
		DewSave.profile.specialReverie = data;
		DewSave.SaveProfile();
	}

	public static void SetDailyReverie<T>(int index)
	{
		SetDailyReverie(index, typeof(T));
	}

	public static void SetDailyReverie(int index, Type type)
	{
		DewProfile.DailyReverieData newData = GetNewDailyReverieData(type);
		DewSave.profile.lastReverieTypes.Add(type.Name);
		DewSave.profile.reverieSlots[index] = newData;
		DewSave.SaveProfile();
	}

	public static void SetRandomDailyReverie(int index)
	{
		SetDailyReverie(index, GetRandomDailyReverieType());
	}

	private static DewProfile.DailyReverieData GetNewDailyReverieData(Type type)
	{
		DewReverieItem ins = (DewReverieItem)Activator.CreateInstance(type);
		ins.OnSetupReverie();
		DewProfile.DailyReverieData data = new DewProfile.DailyReverieData
		{
			type = type.Name,
			persistentVariables = new Dictionary<string, string>(),
			grantedStardust = ins.grantedStardust,
			grantedItems = ins.grantedItems,
			isComplete = false
		};
		ins.SaveReverieStateToData(data);
		return data;
	}

	private static Type GetRandomDailyReverieType()
	{
		List<Type> list = new List<Type>();
		list.AddRange(from r in Dew.allReveries
			where !r.excludeFromPool
			select r.GetType() into t
			where !DewSave.profile.lastReverieTypes.Contains(t.Name)
			select t);
		if (list.Count == 0)
		{
			while (DewSave.profile.lastReverieTypes.Count > 3)
			{
				DewSave.profile.lastReverieTypes.RemoveAt(0);
			}
			list.AddRange(from r in Dew.allReveries
				where !r.excludeFromPool
				select r.GetType() into t
				where !DewSave.profile.lastReverieTypes.Contains(t.Name)
				select t);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private static bool CheckSpecialReveries_NextFest()
	{
		return false;
	}
}
