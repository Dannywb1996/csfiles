using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DewPersistentGameObserver
{
	private List<Action<DewGameResult>> _gameResultCallbacks;

	internal List<Action> _unregisterCallbacks;

	protected List<Coroutine> _coroutines = new List<Coroutine>();

	public string name => GetType().Name;

	public virtual void OnStartLocalClient()
	{
	}

	public virtual void OnStopLocalClient()
	{
		StopCoroutines();
		UnregisterListeners();
		_gameResultCallbacks = null;
	}

	private void StopCoroutines()
	{
		if (ManagerBase<AchievementManager>.instance != null)
		{
			foreach (Coroutine c in _coroutines)
			{
				ManagerBase<AchievementManager>.instance.StopCoroutine(c);
			}
		}
		_coroutines.Clear();
	}

	private void UnregisterListeners()
	{
		if (_unregisterCallbacks == null)
		{
			return;
		}
		foreach (Action c in _unregisterCallbacks)
		{
			try
			{
				c();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_unregisterCallbacks = null;
	}

	public void LoadState(Dictionary<string, string> storage)
	{
		if (storage == null)
		{
			return;
		}
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo f in fields)
		{
			if (f.GetCustomAttributes(typeof(AchPersistentVarAttribute), inherit: false).Length == 0)
			{
				continue;
			}
			try
			{
				if (storage.TryGetValue(f.Name, out var jsonText))
				{
					object savedValue = JsonUtilityEx.FromJson(jsonText, f.FieldType);
					f.SetValue(this, savedValue);
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while loading persistent var: " + f.FieldType.Name + " " + GetType().Name + "::" + f.Name);
				Debug.LogException(exception);
			}
		}
	}

	public void SaveState(Dictionary<string, string> storage)
	{
		try
		{
			FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo f in fields)
			{
				if (f.GetCustomAttributes(typeof(AchPersistentVarAttribute), inherit: false).Length != 0)
				{
					try
					{
						object currentValue = f.GetValue(this);
						storage[f.Name] = JsonUtilityEx.ToJson(currentValue);
					}
					catch (Exception exception)
					{
						Debug.LogError("Exception occured while saving achievement persistent var: " + f.FieldType.Name + " " + GetType().Name + "::" + f.Name);
						Debug.LogException(exception);
					}
				}
			}
		}
		catch (Exception exception2)
		{
			Debug.LogError("Exception occured while saving persistent observer status: " + GetType().Name);
			Debug.LogException(exception2);
		}
	}

	public void FeedGameResult(DewGameResult obj)
	{
		if (_gameResultCallbacks == null)
		{
			return;
		}
		foreach (Action<DewGameResult> c in _gameResultCallbacks)
		{
			try
			{
				c(obj);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchStartCoroutine(IEnumerator routine)
	{
		Coroutine newRoutine = ManagerBase<AchievementManager>.instance.StartCoroutine(routine);
		_coroutines.Add(newRoutine);
	}

	protected void AchSetInterval(Action func, float interval)
	{
		Coroutine newRoutine = ManagerBase<AchievementManager>.instance.StartCoroutine(Routine());
		_coroutines.Add(newRoutine);
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(interval * UnityEngine.Random.value);
			while (true)
			{
				if (DewPlayer.local != null)
				{
					try
					{
						func();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				yield return new WaitForSeconds(interval);
			}
		}
	}

	protected void AchOnKillLastHit(Action<EventInfoKill> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnDeath += new Action<EventInfoKill>(KillListener);
		_unregisterCallbacks.Add(delegate
		{
			if (NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnDeath -= new Action<EventInfoKill>(KillListener);
			}
		});
		void KillListener(EventInfoKill obj)
		{
			try
			{
				if (!(obj.actor == null) && !(DewPlayer.local == null) && !(DewPlayer.local.hero == null) && obj.actor.IsDescendantOf(DewPlayer.local.hero))
				{
					func(obj);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchOnKillOrAssist(Action<EventInfoKill> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		Hero hero = DewPlayer.local.hero;
		hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(KillListener);
		_unregisterCallbacks.Add(delegate
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(KillListener);
		});
		void KillListener(EventInfoKill obj)
		{
			try
			{
				func(obj);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchOnDealDamage(Action<EventInfoDamage> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += new Action<EventInfoDamage>(DamageListener);
		_unregisterCallbacks.Add(delegate
		{
			if (NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage -= new Action<EventInfoDamage>(DamageListener);
			}
		});
		void DamageListener(EventInfoDamage obj)
		{
			try
			{
				if (!(obj.actor == null) && !(DewPlayer.local == null) && !(DewPlayer.local.hero == null) && obj.actor.IsDescendantOf(DewPlayer.local.hero))
				{
					func(obj);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchOnDoHeal(Action<EventInfoHeal> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeHeal += new Action<EventInfoHeal>(OnTakeHeal);
		_unregisterCallbacks.Add(delegate
		{
			if (NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnTakeHeal -= new Action<EventInfoHeal>(OnTakeHeal);
			}
		});
		void OnTakeHeal(EventInfoHeal obj)
		{
			try
			{
				if (!(obj.actor == null) && !(DewPlayer.local == null) && !(DewPlayer.local.hero == null) && obj.actor.IsDescendantOf(DewPlayer.local.hero))
				{
					func(obj);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchOnTakeDamage(Action<EventInfoDamage> func)
	{
		if (_unregisterCallbacks == null)
		{
			_unregisterCallbacks = new List<Action>();
		}
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += new Action<EventInfoDamage>(DamageListener);
		_unregisterCallbacks.Add(delegate
		{
			if (NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage -= new Action<EventInfoDamage>(DamageListener);
			}
		});
		void DamageListener(EventInfoDamage obj)
		{
			try
			{
				if (!(obj.victim == null) && !(DewPlayer.local == null) && !(DewPlayer.local.hero == null) && !(obj.victim != DewPlayer.local.hero))
				{
					func(obj);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected void AchWithGameResult(Action<DewGameResult> func)
	{
		if (_gameResultCallbacks == null)
		{
			_gameResultCallbacks = new List<Action<DewGameResult>>();
		}
		_gameResultCallbacks.Add(func);
	}
}
