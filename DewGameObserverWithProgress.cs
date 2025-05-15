using System;
using System.Collections;
using UnityEngine;

public class DewGameObserverWithProgress : DewPersistentGameObserver
{
	public Hero hero
	{
		get
		{
			if (!(DewPlayer.local != null))
			{
				return null;
			}
			return DewPlayer.local.hero;
		}
	}

	public virtual int GetMaxProgress()
	{
		return 1;
	}

	public virtual int GetCurrentProgress()
	{
		return 0;
	}

	public virtual void OnComplete()
	{
	}

	public void Complete()
	{
		try
		{
			OnComplete();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected void AchCompleteWhen(Func<bool> condition, float checkInterval = 2f)
	{
		Coroutine newRoutine = ManagerBase<AchievementManager>.instance.StartCoroutine(Routine());
		_coroutines.Add(newRoutine);
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(checkInterval * UnityEngine.Random.value);
			while (true)
			{
				try
				{
					if (DewPlayer.local != null && condition())
					{
						Complete();
						break;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				yield return new WaitForSeconds(checkInterval);
			}
		}
	}
}
