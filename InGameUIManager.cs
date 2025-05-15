using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUIManager : UIManager
{
	private class Ad_SharedMessage
	{
		public float lastShowTime;
	}

	public Action<WorldMessageSetting> onShowWorldMessage;

	public GameObject cmsgGeneralPrefab;

	public GameObject cmsgErrorPrefab;

	public Transform cmsgContainer;

	public Transform cmsgStartPivot;

	private int _disablePlayingInputCounter;

	private bool _disablePlayingInputByView;

	private byte _worldNodePingCounter;

	public Action<bool> onHasWorldNodePingChanged;

	public Action<bool> onScoreboardDisplayedChanged;

	private bool _isScoreboardDisplayed;

	public Action<WorldDisplayStatus> onWorldDisplayedChanged;

	public Rift_MockExit currentMockExit;

	private WorldDisplayStatus _isWorldDisplayed;

	[NonSerialized]
	public List<RectTransform> fullWorldMapNodeItems = new List<RectTransform>();

	[NonSerialized]
	public List<RectTransform> miniWorldMapNodeItems = new List<RectTransform>();

	public new static InGameUIManager instance => ManagerBase<UIManager>.instance as InGameUIManager;

	public new static InGameUIManager softInstance => ManagerBase<UIManager>.softInstance as InGameUIManager;

	public bool disablePlayingInput
	{
		get
		{
			if (_disablePlayingInputCounter <= 0)
			{
				return _disablePlayingInputByView;
			}
			return true;
		}
	}

	public bool hasWorldNodePing { get; private set; }

	public bool isScoreboardDisplayed
	{
		get
		{
			return _isScoreboardDisplayed;
		}
		set
		{
			if (_isScoreboardDisplayed == value)
			{
				return;
			}
			_isScoreboardDisplayed = value;
			try
			{
				onScoreboardDisplayedChanged?.Invoke(value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public WorldDisplayStatus isWorldDisplayed
	{
		get
		{
			return _isWorldDisplayed;
		}
		set
		{
			if (_isWorldDisplayed == value)
			{
				return;
			}
			_isWorldDisplayed = value;
			if (_isWorldDisplayed != WorldDisplayStatus.None)
			{
				lastWorldDisplayUnscaledTime = Time.unscaledTime;
			}
			try
			{
				onWorldDisplayedChanged?.Invoke(value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public float lastWorldDisplayUnscaledTime { get; private set; }

	private void Start()
	{
		AddSharedItemHandler();
		InitWorld();
		SetupGamepadBack();
		if (SteamManager.instance != null)
		{
			SteamManager.instance.onGameOverlayShownChanged += new Action<bool>(OnGameOverlayShownChanged);
		}
	}

	private void OnDestroy()
	{
		if (SteamManager.instance != null)
		{
			SteamManager.instance.onGameOverlayShownChanged -= new Action<bool>(OnGameOverlayShownChanged);
		}
	}

	private void OnGameOverlayShownChanged(bool obj)
	{
		if (obj && ManagerBase<GlobalUIManager>.instance != null && ManagerBase<GlobalUIManager>.instance.currentMenuView is UnityEngine.Object o && o != null && !ManagerBase<GlobalUIManager>.instance.isTutorialHighlighting && ManagerBase<GlobalUIManager>.instance.currentMenuView.CanShowMenu() && !ManagerBase<GlobalUIManager>.instance.currentMenuView.IsShowing())
		{
			ManagerBase<GlobalUIManager>.instance.currentMenuView.ShowMenu();
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (isWorldDisplayed != WorldDisplayStatus.None && ManagerBase<ControlManager>.instance.it_interact.down)
		{
			isWorldDisplayed = WorldDisplayStatus.None;
		}
	}

	public void ShowCenterMessage(CenterMessageType type, string key, object[] formatArgs = null)
	{
		string val = DewLocalization.GetUIValue(key);
		if (formatArgs != null)
		{
			val = string.Format(val, formatArgs);
		}
		ShowCenterMessageRaw(type, val);
	}

	public void ShowCenterMessageRaw(CenterMessageType type, string raw)
	{
		UnityEngine.Object.Instantiate(type switch
		{
			CenterMessageType.General => cmsgGeneralPrefab, 
			CenterMessageType.Error => cmsgErrorPrefab, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		}, cmsgStartPivot.position, Quaternion.identity, cmsgContainer).GetComponentInChildren<TextMeshProUGUI>().text = raw;
	}

	protected override void OnStateChanged(string oldState, string newState)
	{
		base.OnStateChanged(oldState, newState);
		ManagerBase<FPSManager>.instance.UpdateFPSLimit();
	}

	public override bool ShouldDoAutoFocus()
	{
		if (!ManagerBase<MessageManager>.instance.isShowingMessage && !disablePlayingInput)
		{
			if ((NetworkedManagerBase<ZoneManager>.softInstance == null || !NetworkedManagerBase<ZoneManager>.softInstance.isInAnyTransition) && (ManagerBase<ControlManager>.softInstance == null || !ManagerBase<ControlManager>.softInstance.shouldProcessCharacterInput))
			{
				return base.state != "Playing";
			}
			return false;
		}
		return true;
	}

	public void EnablePlayingInput()
	{
		_disablePlayingInputCounter--;
	}

	public void DisablePlayingInput()
	{
		_disablePlayingInputCounter++;
	}

	internal void UpdateDisablePlayingInputByView()
	{
		_disablePlayingInputByView = false;
		for (int i = View.instances.Count - 1; i >= 0; i--)
		{
			if (View.instances[i] == null)
			{
				View.instances.RemoveAt(i);
			}
			else if (View.instances[i].isShowing && View.instances[i].disablesInGamePlayingInput)
			{
				_disablePlayingInputByView = true;
				break;
			}
		}
	}

	private void SetupGamepadBack()
	{
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 2000, delegate
		{
			if (isScoreboardDisplayed)
			{
				isScoreboardDisplayed = false;
				return true;
			}
			if (DewInput.currentMode == InputMode.Gamepad && ManagerBase<GlobalUIManager>.instance.focused != null && ManagerBase<ControlManager>.instance.shouldProcessCharacterInput)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(null);
				return false;
			}
			return false;
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage dmg)
		{
			if (DewPlayer.local != null && dmg.victim == DewPlayer.local.hero && !dmg.damage.HasAttr(DamageAttribute.DamageOverTime) && DewInput.currentMode == InputMode.Gamepad && ManagerBase<GlobalUIManager>.instance.focused != null && ManagerBase<ControlManager>.instance.shouldProcessCharacterInput)
			{
				ManagerBase<GlobalUIManager>.instance.SetFocus(null);
			}
		};
	}

	public static bool ValidateInGameActionMessage(bool requireAliveHero = true)
	{
		if (NetworkedManagerBase<GameManager>.softInstance != null && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			if (requireAliveHero)
			{
				return !DewPlayer.local.hero.IsNullInactiveDeadOrKnockedOut();
			}
			return true;
		}
		return false;
	}

	public void ShowWorldPopMessage(WorldMessageSetting message)
	{
		try
		{
			onShowWorldMessage?.Invoke(message);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void AddSharedItemHandler()
	{
		NetworkedManagerBase<ActorManager>.instance.onActorAdd += (Action<Actor>)delegate(Actor a)
		{
			SkillTrigger st = a as SkillTrigger;
			if ((object)st != null)
			{
				st.ClientEvent_OnTempOwnerChanged += (Action<DewPlayer, DewPlayer>)delegate
				{
					CheckSharedMessage(st);
				};
				st.ClientEvent_OnHandOwnerChanged += (Action<Hero, Hero>)delegate
				{
					CheckSharedMessage(st);
				};
				st.ClientEvent_OnOwnerChanged += (Action<Entity, Entity>)delegate
				{
					CheckSharedMessage(st);
				};
			}
			else
			{
				Gem g = a as Gem;
				if ((object)g != null)
				{
					g.ClientEvent_OnTempOwnerChanged += (Action<DewPlayer, DewPlayer>)delegate
					{
						CheckSharedMessage(g);
					};
					g.ClientEvent_OnHandOwnerChanged += (Action<Hero, Hero>)delegate
					{
						CheckSharedMessage(g);
					};
					g.ClientEvent_OnOwnerChanged += (Action<Hero, Hero>)delegate
					{
						CheckSharedMessage(g);
					};
				}
			}
		};
	}

	private void CheckSharedMessage(IItem item)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return null;
			yield return null;
			if (DewPlayer.humanPlayers.Count > 1 && item != null)
			{
				Actor actor = item as Actor;
				if (!actor.IsNullOrInactive() && !(item.tempOwner != null) && !(item.owner != null) && !(item.handOwner != null))
				{
					if (!actor.TryGetData<Ad_SharedMessage>(out var msg))
					{
						msg = new Ad_SharedMessage
						{
							lastShowTime = float.NegativeInfinity
						};
						actor.AddData(msg);
					}
					if (!(Time.time - msg.lastShowTime < 3f))
					{
						msg.lastShowTime = Time.time;
						ShowSharedMessage(item);
					}
				}
			}
		}
	}

	private void ShowSharedMessage(IItem item)
	{
		ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = "<size=130%><color=#91faff>" + DewLocalization.GetUIValue("InGame_Shared") + "</color></size>",
			worldPosGetter = () => (item.worldModel == null) ? (Vector3.down * 10000f) : item.worldModel.iconQuad.transform.position
		});
	}

	private void InitWorld()
	{
		for (int i = 0; i < 100; i++)
		{
			fullWorldMapNodeItems.Add(null);
		}
		for (int j = 0; j < 100; j++)
		{
			miniWorldMapNodeItems.Add(null);
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnNodesChanged += new Action(ClientEventOnNodesChanged);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += (Action<EventInfoLoadRoom>)delegate
		{
			isWorldDisplayed = WorldDisplayStatus.None;
		};
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnIsInTransitionChanged += (Action<bool>)delegate
		{
			isWorldDisplayed = WorldDisplayStatus.None;
		};
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnVoteStarted += (Action<DewPlayer>)delegate
		{
			isWorldDisplayed = WorldDisplayStatus.None;
		};
		ClientEventOnNodesChanged();
		ManagerBase<GlobalUIManager>.instance.AddBackHandler(this, 20, delegate
		{
			if (isWorldDisplayed == WorldDisplayStatus.None)
			{
				return false;
			}
			isWorldDisplayed = WorldDisplayStatus.None;
			return true;
		});
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage dmg)
		{
			if (isWorldDisplayed != WorldDisplayStatus.None && DewPlayer.local != null && dmg.victim == DewPlayer.local.hero && !dmg.damage.HasAttr(DamageAttribute.DamageOverTime))
			{
				isWorldDisplayed = WorldDisplayStatus.None;
			}
		};
	}

	public void IncrementWorldNodePingCounter()
	{
		checked
		{
			_worldNodePingCounter = (byte)(unchecked((uint)_worldNodePingCounter) + 1u);
			UpdateHasWorldNodePing();
		}
	}

	public void DecrementWorldNodePingCounter()
	{
		checked
		{
			_worldNodePingCounter = (byte)(unchecked((uint)_worldNodePingCounter) - 1u);
			UpdateHasWorldNodePing();
		}
	}

	private void UpdateHasWorldNodePing()
	{
		bool newVal = _worldNodePingCounter > 0;
		if (newVal != hasWorldNodePing)
		{
			hasWorldNodePing = newVal;
			try
			{
				onHasWorldNodePingChanged?.Invoke(hasWorldNodePing);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void ClientEventOnNodesChanged()
	{
		int count = NetworkedManagerBase<ZoneManager>.instance.nodes.Count;
		while (fullWorldMapNodeItems.Count < count)
		{
			fullWorldMapNodeItems.Add(null);
		}
		while (miniWorldMapNodeItems.Count < count)
		{
			miniWorldMapNodeItems.Add(null);
		}
	}

	public Vector2 GetWorldNodeUIPos(int index)
	{
		if (index < 0 || index >= fullWorldMapNodeItems.Count || fullWorldMapNodeItems[index] == null)
		{
			return new Vector2(-1000f, -1000f);
		}
		return fullWorldMapNodeItems[index].transform.position;
	}
}
