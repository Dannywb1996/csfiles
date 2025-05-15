using System;
using UnityEngine;

public class BackHandler
{
	public MonoBehaviour owner;

	public int priority;

	public Func<bool> func;

	public void Remove()
	{
		if (ManagerBase<GlobalUIManager>.instance != null)
		{
			ManagerBase<GlobalUIManager>.instance.RemoveBackHandler(this);
		}
	}
}
