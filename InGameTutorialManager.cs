using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameTutorialManager : ManagerBase<InGameTutorialManager>
{
	public bool isTutorialDisabled;

	public TutorialArrow arrowPrefab;

	public Transform uiParent;

	public Transform refSkillButtonQ;

	public Transform refSkillButtonW;

	public Transform refSkillButtonE;

	public Transform refSkillButtonR;

	public Transform refSkillButtonTrait;

	public Transform refSkillButtonMovement;

	public Transform refHuntProgress;

	public Transform refHuntImminentProgress;

	public Transform refSkillNotification;

	public Transform refGeneralSkills;

	public Transform refEditKeyDisplay;

	internal List<Action> _logicUpdates = new List<Action>();

	internal List<Action> _frameUpdates = new List<Action>();

	private List<DewInGameTutorialItem> _activeTutorials = new List<DewInGameTutorialItem>();

	public bool isTutorialActive { get; private set; }

	public Transform GetRefSkill(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => refSkillButtonQ, 
			HeroSkillLocation.W => refSkillButtonW, 
			HeroSkillLocation.E => refSkillButtonE, 
			HeroSkillLocation.R => refSkillButtonR, 
			HeroSkillLocation.Identity => refSkillButtonTrait, 
			HeroSkillLocation.Movement => refSkillButtonMovement, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	private void Start()
	{
		if (isTutorialDisabled)
		{
			return;
		}
		NetworkedManagerBase<GameManager>.instance.ClientEvent_OnGameConcluded += (Action)delegate
		{
			if (isTutorialActive)
			{
				StopTutorials();
			}
		};
		GameManager.CallOnReady(delegate
		{
			StartTutorials();
			DewPlayer.local.ClientEvent_OnHeroChanged += (Action<Hero, Hero>)delegate
			{
				if (isTutorialActive)
				{
					StopTutorials();
					StartTutorials();
				}
			};
		});
	}

	public void StartTutorials()
	{
		if (isTutorialActive)
		{
			Debug.LogWarning("Tutorial already has started");
		}
		else
		{
			if (DewSave.profile.gameplay.disableTutorial)
			{
				return;
			}
			isTutorialActive = true;
			foreach (Type tutType in Dew.allTutorialItems)
			{
				if (!DewSave.profile.doneTutorials.Contains(tutType.Name) || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
				{
					DewInGameTutorialItem newItem = (DewInGameTutorialItem)Activator.CreateInstance(tutType);
					_activeTutorials.Add(newItem);
				}
			}
			for (int i = _activeTutorials.Count - 1; i >= 0; i--)
			{
				DewInGameTutorialItem tutItem = _activeTutorials[i];
				try
				{
					tutItem.mgr = this;
					tutItem.OnStart();
				}
				catch (Exception exception)
				{
					Debug.LogError("Exception occured while starting " + tutItem.GetType().Name);
					Debug.LogException(exception, this);
					_activeTutorials.RemoveAt(i);
				}
			}
			if (_activeTutorials.Count > 0)
			{
				Debug.Log($"Started {_activeTutorials.Count} tutorials");
				return;
			}
			isTutorialActive = false;
			_logicUpdates.Clear();
			_frameUpdates.Clear();
		}
	}

	public void StopTutorials()
	{
		if (!isTutorialActive)
		{
			Debug.LogWarning("Tutorial has not started");
			return;
		}
		isTutorialActive = false;
		for (int i = _activeTutorials.Count - 1; i >= 0; i--)
		{
			DewInGameTutorialItem item = _activeTutorials[i];
			string tutKey = item.GetType().Name;
			try
			{
				item.OnStop();
			}
			catch (Exception exception)
			{
				Debug.LogError(tutKey + ".OnStop() failed with exception below");
				Debug.LogException(exception);
			}
		}
		Debug.Log($"Stopped {_activeTutorials.Count} tutorials");
		_activeTutorials.Clear();
		_logicUpdates.Clear();
		_frameUpdates.Clear();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!isTutorialActive)
		{
			return;
		}
		for (int i = 0; i < _frameUpdates.Count; i++)
		{
			try
			{
				_frameUpdates[i]();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!isTutorialActive)
		{
			return;
		}
		for (int i = 0; i < _logicUpdates.Count; i++)
		{
			try
			{
				_logicUpdates[i]();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	internal void CompleteTutorialItem(DewInGameTutorialItem item)
	{
		if (!isTutorialActive)
		{
			Debug.LogWarning("Tutorial has not started");
			return;
		}
		string tutKey = item.GetType().Name;
		int index = _activeTutorials.IndexOf(item);
		if (index < 0)
		{
			Debug.LogWarning(tutKey + " not found in active tutorials");
			return;
		}
		_activeTutorials.RemoveAt(index);
		Debug.Log("Tutorial " + tutKey + " completed.");
		if (DewSave.profile.doneTutorials.Contains(tutKey))
		{
			Debug.LogWarning(tutKey + " already in done tutorials");
		}
		else
		{
			DewSave.profile.doneTutorials.Add(tutKey);
		}
		try
		{
			item.OnStop();
		}
		catch (Exception exception)
		{
			Debug.LogError(tutKey + ".OnStop() failed with exception below");
			Debug.LogException(exception);
		}
		if (_activeTutorials.Count == 0)
		{
			StopTutorials();
		}
	}

	public TutorialArrow CreateArrow()
	{
		return UnityEngine.Object.Instantiate(arrowPrefab, uiParent);
	}
}
