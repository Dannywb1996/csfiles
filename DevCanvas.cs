using System;
using System.Collections.Generic;
using System.IO;
using IngameDebugConsole;
using UnityEngine;

public class DevCanvas : SingletonBehaviour<DevCanvas>
{
	public enum DevCanvasState
	{
		Hidden,
		Shown,
		ShownWithoutPopup
	}

	public DevCanvasState state;

	public GameObject consoleObject;

	private bool _isDevModeActivated;

	private int _currentPhraseIndex;

	private float _stateShowTime;

	private void Start()
	{
		string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		_isDevModeActivated = File.Exists(Path.Join(docs, "ShapeOfDream-EnableCheats.txt")) || File.Exists(Path.Join(docs, "ShapeOfDreams-EnableCheats.txt"));
		if (_isDevModeActivated)
		{
			state = DevCanvasState.Shown;
		}
		RefreshVisibility();
	}

	private void Update()
	{
		if (!_isDevModeActivated && Input.anyKeyDown)
		{
			List<KeyCode> fullPhrase = new List<KeyCode>
			{
				KeyCode.L,
				KeyCode.S,
				KeyCode.D,
				KeyCode.Escape,
				KeyCode.L,
				KeyCode.S,
				KeyCode.D,
				KeyCode.Escape,
				KeyCode.L,
				KeyCode.S,
				KeyCode.D,
				KeyCode.Escape,
				KeyCode.L,
				KeyCode.S,
				KeyCode.D,
				KeyCode.Escape
			};
			if (Input.GetKeyDown(fullPhrase[_currentPhraseIndex]))
			{
				_currentPhraseIndex++;
				if (_currentPhraseIndex == fullPhrase.Count)
				{
					_isDevModeActivated = true;
					SwitchState();
				}
			}
			else if (Input.GetKeyDown(fullPhrase[0]))
			{
				_currentPhraseIndex = 1;
			}
			else
			{
				_currentPhraseIndex = 0;
			}
		}
		if (_isDevModeActivated)
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				SwitchState();
			}
			_stateShowTime = Mathf.MoveTowards(_stateShowTime, 0f, Time.unscaledDeltaTime);
		}
	}

	private void SwitchState()
	{
		state = (DevCanvasState)((int)(state + 1) % 3);
		_stateShowTime = 2f;
		RefreshVisibility();
	}

	private void OnGUI()
	{
		if (!(_stateShowTime <= 0.001f))
		{
			GUILayout.Label(state.ToString(), new GUIStyle
			{
				fontSize = 28,
				normal = new GUIStyleState
				{
					textColor = Color.green
				}
			});
		}
	}

	private void RefreshVisibility()
	{
		consoleObject.SetActive(state != DevCanvasState.Hidden);
		UnityEngine.Object.FindObjectOfType<DebugLogPopup>(includeInactive: true).gameObject.SetActive(state == DevCanvasState.Shown);
	}
}
