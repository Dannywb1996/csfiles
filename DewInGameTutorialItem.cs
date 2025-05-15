using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DewInGameTutorialItem
{
	private List<Action> _cleanupCallbacks = new List<Action>();

	private Coroutiner _coroutiner;

	public InGameTutorialManager mgr { get; set; }

	public bool isActive { get; internal set; }

	public virtual void OnStart()
	{
		if (!isActive)
		{
			isActive = true;
		}
	}

	public virtual void OnStop()
	{
		if (!isActive)
		{
			return;
		}
		isActive = false;
		foreach (Action c in _cleanupCallbacks)
		{
			try
			{
				c();
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception below occured in cleanup callback of " + GetType().Name);
				Debug.LogException(exception);
			}
		}
		_cleanupCallbacks.Clear();
	}

	protected void TutOnLogicUpdate(Action func)
	{
		if (!isActive)
		{
			Debug.LogWarning(GetType().Name + " is inactive");
			return;
		}
		mgr._logicUpdates.Add(func);
		_cleanupCallbacks.Add(delegate
		{
			mgr._logicUpdates.Remove(func);
		});
	}

	protected void TutOnFrameUpdate(Action func)
	{
		if (!isActive)
		{
			Debug.LogWarning(GetType().Name + " is inactive");
			return;
		}
		mgr._frameUpdates.Add(func);
		_cleanupCallbacks.Add(delegate
		{
			mgr._frameUpdates.Remove(func);
		});
	}

	protected void TutComplete()
	{
		if (isActive)
		{
			mgr.CompleteTutorialItem(this);
		}
	}

	protected TutorialArrow TutCreateArrow()
	{
		TutorialArrow newArrow = mgr.CreateArrow();
		_cleanupCallbacks.Add(delegate
		{
			if (newArrow != null)
			{
				UnityEngine.Object.Destroy(newArrow.gameObject);
			}
		});
		return newArrow;
	}

	protected Coroutiner TutGetCoroutiner()
	{
		if (!isActive)
		{
			return null;
		}
		if (_coroutiner == null)
		{
			GameObject gobj = new GameObject("Coroutiner - " + GetType().Name);
			gobj.transform.parent = mgr.transform;
			_coroutiner = gobj.AddComponent<Coroutiner>();
			_cleanupCallbacks.Add(delegate
			{
				if (_coroutiner != null)
				{
					UnityEngine.Object.Destroy(_coroutiner.gameObject);
				}
			});
		}
		return _coroutiner;
	}

	protected void TutDelayedExecution(Action func, float seconds)
	{
		if (isActive)
		{
			TutGetCoroutiner().StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(seconds);
			func();
		}
	}

	protected void TutOnOutOfCombat(Action func)
	{
		if (!isActive)
		{
			Debug.LogWarning(GetType().Name + " is inactive");
		}
		else
		{
			TutGetCoroutiner().StartCoroutine(Routine());
		}
		static bool IsOutOfCombat()
		{
			return !DewPlayer.local.hero.isInCombat;
		}
		IEnumerator Routine()
		{
			if (!IsOutOfCombat())
			{
				int count = 0;
				while (true)
				{
					yield return new WaitForSeconds(0.5f);
					if (IsOutOfCombat())
					{
						count++;
						if (count >= 3)
						{
							break;
						}
					}
					else
					{
						count = 0;
					}
				}
			}
			try
			{
				func();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
