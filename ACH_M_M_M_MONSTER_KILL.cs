using System;
using System.Collections.Generic;

[AchUnlockOnComplete(typeof(LucidDream_Overpopulation))]
public class ACH_M_M_M_MONSTER_KILL : DewAchievementItem
{
	private class Ad_KillCountTracker
	{
		public bool isRoot;

		public int killCount;
	}

	private const int RequiredKillCount = 16;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ClientEventManager>.instance.OnCastComplete += new Action<EventInfoCast>(OnCastComplete);
		AchOnKillLastHit(delegate(EventInfoKill kill)
		{
			if (!TryFindTracker(kill.actor, out var tracker))
			{
				tracker = new Ad_KillCountTracker
				{
					isRoot = false
				};
				kill.actor.AddData(tracker);
			}
			tracker.killCount++;
			if (tracker.killCount >= 16 && tracker.isRoot)
			{
				Complete();
			}
		});
	}

	private bool TryFindTracker(Actor actor, out Ad_KillCountTracker tracker)
	{
		Actor actor2 = actor;
		tracker = null;
		while (tracker == null && actor2 != null)
		{
			actor2.TryGetData<Ad_KillCountTracker>(out tracker);
			actor2 = actor2.parentActor;
		}
		return tracker != null;
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnCastComplete -= new Action<EventInfoCast>(OnCastComplete);
		}
	}

	private void OnCastComplete(EventInfoCast obj)
	{
		if (obj.instance == null)
		{
			return;
		}
		int num = 0;
		ListReturnHandle<Actor> handle;
		List<Actor> list = DewPool.GetList(out handle);
		list.Add(obj.instance);
		while (list.Count > 0)
		{
			Actor actor = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			if (actor.TryGetData<Ad_KillCountTracker>(out var data))
			{
				num += data.killCount;
				actor.RemoveData(data);
			}
			list.AddRange(actor.children);
		}
		handle.Return();
		if (num >= 16)
		{
			Complete();
			return;
		}
		obj.instance.AddData(new Ad_KillCountTracker
		{
			isRoot = true,
			killCount = num
		});
	}
}
