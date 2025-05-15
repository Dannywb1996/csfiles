using System;
using UnityEngine;

public abstract class DewSpecialReverieItem : DewReverieItem
{
	public virtual Type nextReverie { get; }

	public abstract TimeSpan timeLimit { get; }

	public override void OnStartLocalClient()
	{
		try
		{
			LoadState(DewSave.profile.specialReverie.persistentVariables);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void OnStopLocalClient()
	{
		try
		{
			SaveReverieStateToData(DewSave.profile.specialReverie);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
