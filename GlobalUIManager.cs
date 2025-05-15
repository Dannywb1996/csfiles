using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUIManager : ManagerBase<GlobalUIManager>
{
	public struct RectResult
	{
		public Rect rect;

		public float distance;
	}

	public RectTransform tooltipBoxTransform;

	public CanvasGroup tooltipBoxCanvasGroup;

	[NonSerialized]
	public bool isBackDisabled;

	[NonSerialized]
	public IMenuView currentMenuView;

	public TMP_FontAsset fontBody;

	public TMP_FontAsset fontHeading;

	public TMP_FontAsset fontHeadingBold;

	public TMP_FontAsset fontHeadingLight;

	private readonly List<BackHandler> _backHandlers = new List<BackHandler>();

	private DewInputTrigger it_back;

	private DewInputTrigger it_menu;

	public GameObject fxTick;

	public GameObject focusDisplayObject;

	public Transform focusDisplayAnimationTarget;

	public RectTransform focusDisplayTransform;

	public GameObject focusDisplayBoxObject;

	public GameObject focusDisplayCircleObject;

	private TMP_InputField _targetField;

	internal List<IGamepadFocusable> _allFocusables = new List<IGamepadFocusable>();

	internal List<UI_GamepadFocusableGroup> _allGroups = new List<UI_GamepadFocusableGroup>();

	private DewInputTrigger it_gamepadConfirm;

	private DewInputTrigger it_gamepadUp;

	private DewInputTrigger it_gamepadLeft;

	private DewInputTrigger it_gamepadDown;

	private DewInputTrigger it_gamepadRight;

	public Action<IGamepadFocusable, IGamepadFocusable> onFocusedChanged;

	private List<IGamepadFocusable> _focusedHistory = new List<IGamepadFocusable> { null };

	private List<IGamepadFocusListener> _focusedListeners = new List<IGamepadFocusListener>();

	private int _framesWithoutFocus;

	public GameObject fxHighlightShow;

	public GameObject fxHighlightHide;

	public GameObject tutHighlightObject;

	public RectTransform tutHighlightBox;

	public RectTransform tutHighlightBackdropLeft;

	public RectTransform tutHighlightBackdropRight;

	public RectTransform tutHighlightBackdropTop;

	public RectTransform tutHighlightBackdropBottom;

	public TextMeshProUGUI tutHighlightText;

	private TutorialHighlightSettings _currentHighlight;

	private int _lastHighlightFrame = int.MinValue;

	private float _highlightStartUnscaledTime;

	private Vector2 _anchoredPosCv;

	private Vector2 _sizeDeltaCv;

	private bool _didAddContinueText;

	public bool isTooltipShown => tooltipBoxCanvasGroup.alpha > 0.8f;

	public Rect tooltipScreenSpaceRect { get; private set; }

	public float lastFocusChangeUnscaledTime { get; private set; }

	public IGamepadFocusable focused { get; private set; }

	public bool isTutorialHighlighting => _currentHighlight != null;

	private void Start()
	{
		InitGamepadInput();
		it_back = new DewInputTrigger
		{
			owner = this,
			priority = 0,
			binding = () => DewSave.profile.controls.back,
			isValidCheck = () => !isBackDisabled
		};
		it_menu = new DewInputTrigger
		{
			owner = this,
			priority = 0,
			binding = () => DewSave.profile.controls.menu,
			isValidCheck = () => currentMenuView is UnityEngine.Object obj && obj != null && (currentMenuView.CanShowMenu() || currentMenuView.IsShowing())
		};
		Start_Tutorial();
	}

	private void OnDestroy()
	{
		OnDestroy_Tutorial();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		FrameUpdate_Tutorial();
		if (ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading)
		{
			focusDisplayObject.SetActive(value: false);
			return;
		}
		if (it_menu.down && currentMenuView is UnityEngine.Object o && o != null && !isTutorialHighlighting && (currentMenuView.CanShowMenu() || currentMenuView.IsShowing()))
		{
			if (currentMenuView.IsShowing())
			{
				GoBack();
			}
			else
			{
				currentMenuView.ShowMenu();
			}
		}
		if (DewInput.currentMode == InputMode.KeyboardAndMouse && !isBackDisabled && it_back.down && _backHandlers.Count > 0)
		{
			GoBack();
		}
		if (DewInput.currentMode == InputMode.Gamepad)
		{
			DoGamepadInputs();
		}
		else
		{
			if (focused == null)
			{
				return;
			}
			IGamepadFocusable last = focused;
			focused = null;
			try
			{
				onFocusedChanged?.Invoke(last, null);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			foreach (IGamepadFocusListener l in _focusedListeners)
			{
				if (l.IsValid())
				{
					l.OnFocusStateChanged(state: false);
				}
			}
			_focusedListeners.Clear();
			focusDisplayObject.SetActive(value: false);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		LogicUpdate_Tutorial();
		if (isTooltipShown)
		{
			tooltipScreenSpaceRect = tooltipBoxTransform.GetScreenSpaceRect();
		}
	}

	public void GoBack()
	{
		BackHandler[] handlers = _backHandlers.ToArray();
		for (int i = handlers.Length - 1; i >= 0; i--)
		{
			BackHandler h = handlers[i];
			if (h.owner == null)
			{
				_backHandlers.Remove(h);
			}
			else
			{
				try
				{
					if (h.func())
					{
						break;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					_backHandlers.Remove(h);
				}
			}
		}
	}

	public BackHandler AddBackHandler(MonoBehaviour owner, int priority, Func<bool> callback)
	{
		BackHandler newItem = new BackHandler
		{
			owner = owner,
			func = callback,
			priority = priority
		};
		int index = -1;
		for (int i = 0; i < _backHandlers.Count && _backHandlers[i].priority <= priority; i++)
		{
			index = i;
		}
		_backHandlers.Insert(index + 1, newItem);
		for (int i2 = _backHandlers.Count - 1; i2 >= 0; i2--)
		{
			if (_backHandlers[i2].owner == null)
			{
				_backHandlers.RemoveAt(i2);
			}
		}
		return newItem;
	}

	public void RemoveBackHandler(BackHandler handle)
	{
		_backHandlers.Remove(handle);
	}

	public bool IsUIElementClickable(RectTransform rectTransform)
	{
		ListReturnHandle<RaycastResult> handle;
		List<RaycastResult> list = Dew.RaycastAllUIElementsBelowScreenPoint(rectTransform.GetScreenSpaceRect().center, out handle);
		bool result = list.Count > 0 && list[0].gameObject.transform.IsChildOf(rectTransform);
		handle.Return();
		return result;
	}

	public void EnforceFontFallbackOrder()
	{
		Enforce(fontBody);
		Enforce(fontHeading);
		Enforce(fontHeadingBold);
		Enforce(fontHeadingLight);
		static void Enforce(TMP_FontAsset fontAsset)
		{
			bool isJapanese = DewSave.profile.language == "ja-JP";
			List<TMP_FontAsset> table = fontAsset.fallbackFontAssetTable.ToList();
			int jpIndex = table.FindIndex((TMP_FontAsset x) => x.name.Contains("JP"));
			int cnIndex = table.FindIndex((TMP_FontAsset x) => x.name.Contains("SC"));
			if (jpIndex != -1 && cnIndex != -1 && ((isJapanese && jpIndex > cnIndex) || (!isJapanese && cnIndex > jpIndex)))
			{
				int index = jpIndex;
				List<TMP_FontAsset> list = table;
				int index2 = cnIndex;
				TMP_FontAsset tMP_FontAsset = table[cnIndex];
				TMP_FontAsset tMP_FontAsset2 = table[jpIndex];
				TMP_FontAsset tMP_FontAsset3 = (table[index] = tMP_FontAsset);
				tMP_FontAsset3 = (list[index2] = tMP_FontAsset2);
				fontAsset.fallbackFontAssetTable = table;
			}
		}
	}

	private void LateUpdate()
	{
		if (ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading || (ManagerBase<UIManager>.softInstance != null && !ManagerBase<UIManager>.softInstance.ShouldDoAutoFocus()))
		{
			return;
		}
		if (focused == null && DewInput.currentMode == InputMode.Gamepad)
		{
			_framesWithoutFocus++;
			if (_framesWithoutFocus <= 4 || TryMoveFocusBack())
			{
				return;
			}
			float bestScore = float.NegativeInfinity;
			IGamepadFocusable best = null;
			for (int i = 0; i < _allFocusables.Count; i++)
			{
				if (_allFocusables[i].IsValid() && _allFocusables[i].CanBeFocused())
				{
					Rect rect = ((RectTransform)((Component)_allFocusables[i]).transform).GetScreenSpaceRect();
					float score = rect.center.y - rect.center.x;
					if (!(score < bestScore))
					{
						bestScore = score;
						best = _allFocusables[i];
					}
				}
			}
			if (best != null)
			{
				SetFocus(best);
			}
		}
		else
		{
			_framesWithoutFocus = 0;
		}
	}

	private void InitGamepadInput()
	{
		it_gamepadConfirm = new DewInputTrigger
		{
			owner = this,
			binding = () => DewSave.profile.controls.confirm,
			priority = -50,
			isValidCheck = () => focused != null
		};
		it_gamepadUp = DpadControl(GamepadButtonEx.DpadUp, GamepadButtonEx.LeftStickUp);
		it_gamepadLeft = DpadControl(GamepadButtonEx.DpadLeft, GamepadButtonEx.LeftStickLeft);
		it_gamepadDown = DpadControl(GamepadButtonEx.DpadDown, GamepadButtonEx.LeftStickDown);
		it_gamepadRight = DpadControl(GamepadButtonEx.DpadRight, GamepadButtonEx.LeftStickRight);
		DewInputTrigger DpadControl(GamepadButtonEx button, GamepadButtonEx button2)
		{
			return new DewInputTrigger
			{
				owner = this,
				priority = -1000,
				binding = () => DewBinding.GamepadOnly(button, button2),
				isValidCheck = ShouldDoDpad
			};
		}
	}

	public void AddGamepadFocusable(IGamepadFocusable focusable)
	{
		if (!focusable.IsValid() || _allFocusables.Contains(focusable))
		{
			return;
		}
		for (int i = _allFocusables.Count - 1; i >= 0; i--)
		{
			if (!_allFocusables[i].IsValid())
			{
				_allFocusables.RemoveAt(i);
			}
		}
		_allFocusables.Add(focusable);
		if (focused == null && focusable.CanBeFocused() && focusable.GetBehavior() == FocusableBehavior.Normal)
		{
			SetFocus(focusable);
		}
	}

	public void RemoveGamepadFocusable(IGamepadFocusable focusable)
	{
		_allFocusables.Remove(focusable);
		if (focused == focusable)
		{
			SetFocus(null);
		}
	}

	public void SetFocus(IGamepadFocusable focusable)
	{
		if (DewInput.currentMode != InputMode.Gamepad)
		{
			return;
		}
		IGamepadFocusable last = focused;
		EventSystem.current.SetSelectedGameObject(null);
		if (focusable != null && !focusable.IsValid())
		{
			focusable = null;
		}
		if (focused != null || _focusedHistory[0] != null)
		{
			_focusedHistory.Insert(0, focused);
			while (_focusedHistory.Count > 10)
			{
				_focusedHistory.RemoveAt(_focusedHistory.Count - 1);
			}
		}
		focused = focusable;
		if (last != focused)
		{
			try
			{
				onFocusedChanged?.Invoke(last, focused);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		lastFocusChangeUnscaledTime = Time.unscaledTime;
		if (focused == null)
		{
			NotifyNewListeners(null);
			return;
		}
		StopAllCoroutines();
		StartCoroutine(Routine());
		Component comp = (Component)focused;
		ListReturnHandle<IGamepadFocusListener> handle;
		List<IGamepadFocusListener> listeners = comp.GetComponentsInParentNonAlloc(out handle);
		NotifyNewListeners(listeners);
		handle.Return();
		ScrollRect scroll = comp.GetComponentInParent<ScrollRect>();
		if (scroll != null)
		{
			Canvas.ForceUpdateCanvases();
			Rect viewportRect = scroll.viewport.GetScreenSpaceRect();
			Vector2 snapPos = (Vector2)scroll.transform.InverseTransformPoint(scroll.content.position) - (Vector2)scroll.transform.InverseTransformPoint(comp.transform.position);
			float maxPos = scroll.content.sizeDelta.y - viewportRect.height / scroll.viewport.lossyScale.y;
			if (!(maxPos < 0f))
			{
				scroll.content.anchoredPosition = scroll.content.anchoredPosition.WithY(Mathf.Clamp(snapPos.y - viewportRect.height * 0.5f, 0f, maxPos));
			}
		}
		IEnumerator Routine()
		{
			focusDisplayAnimationTarget.DOKill(complete: true);
			focusDisplayAnimationTarget.localScale = Vector3.zero;
			yield return null;
			focusDisplayAnimationTarget.localScale = Vector3.one * 1.15f;
			focusDisplayAnimationTarget.DOScale(Vector3.one, 0.17f).SetUpdate(isIndependentUpdate: true);
		}
	}

	public bool MoveFocus(Vector3 direction)
	{
		if (focused == null || !focused.IsValid())
		{
			foreach (IGamepadFocusable f in _allFocusables)
			{
				if (f.IsValid() && f.CanBeFocused())
				{
					SetFocusWithRespectToGroupEnterBehavior(f);
					return true;
				}
			}
			SetFocus(null);
			return false;
		}
		IGamepadNavigationHint hint = ((Component)focused).GetComponentInParent<IGamepadNavigationHint>(includeInactive: true);
		if (hint != null)
		{
			if (direction == Vector3.up && hint.TryGetUp(out var up))
			{
				if (up != null)
				{
					SetFocus(up);
				}
				return true;
			}
			if (direction == Vector3.left && hint.TryGetLeft(out var left))
			{
				if (left != null)
				{
					SetFocus(left);
				}
				return true;
			}
			if (direction == Vector3.down && hint.TryGetDown(out var down))
			{
				if (down != null)
				{
					SetFocus(down);
				}
				return true;
			}
			if (direction == Vector3.right && hint.TryGetRight(out var right))
			{
				if (right != null)
				{
					SetFocus(right);
				}
				return true;
			}
		}
		if (focused.GetBehavior() == FocusableBehavior.BottomBar && direction == (Vector3)Vector2.up && TryMoveFocusBack())
		{
			return true;
		}
		TryGetFocusTargetGeneric(direction, null, out var next);
		if (next != null)
		{
			SetFocus(next);
			return true;
		}
		return false;
		void SetFocusWithRespectToGroupEnterBehavior(IGamepadFocusable nextTarget)
		{
			if (nextTarget == null || !(nextTarget is Component nextComp) || nextComp == null)
			{
				SetFocus(null);
			}
			else
			{
				UI_GamepadFocusableGroup nextGroup = nextComp.GetComponentInParent<UI_GamepadFocusableGroup>();
				if (nextGroup == null)
				{
					SetFocus(nextTarget);
				}
				else
				{
					IGamepadFocusable bestFocusable = nextGroup.GetEnterFocusable(direction);
					if (bestFocusable == null)
					{
						bestFocusable = nextTarget;
					}
					SetFocus(bestFocusable);
				}
			}
		}
	}

	public bool MoveFocus(Vector3 direction, float angleLimit, float normalizedPerpendicularDistLimit, float normalizedDistLimit, Func<IGamepadFocusable, bool> condition = null, Func<IGamepadFocusable, float> customScoreFunc = null)
	{
		return MoveFocus(direction, new Vector2(0f, angleLimit), new Vector2(0f, normalizedPerpendicularDistLimit), new Vector2(0f, normalizedDistLimit), condition, customScoreFunc);
	}

	public bool MoveFocus(Vector3 direction, Vector2 angleLimit, Vector2 normalizedPerpendicularDistLimit, Vector2 normalizedDistLimit, Func<IGamepadFocusable, bool> condition = null, Func<IGamepadFocusable, float> customScoreFunc = null)
	{
		if (focused == null || !focused.IsValid())
		{
			foreach (IGamepadFocusable f in _allFocusables)
			{
				if (f.IsValid() && f.CanBeFocused())
				{
					SetFocus(f);
					return true;
				}
			}
			SetFocus(null);
			return false;
		}
		if (focused.GetBehavior() == FocusableBehavior.BottomBar && direction == (Vector3)Vector2.up && TryMoveFocusBack())
		{
			return true;
		}
		TryGetFocusTarget(new GetFocusTargetSettings
		{
			direction = direction,
			angleLimit = angleLimit,
			normalizedPerpendicularDistLimit = normalizedPerpendicularDistLimit,
			normalizedDistLimit = normalizedDistLimit,
			condition = condition,
			customScoreFunc = customScoreFunc
		}, out var next);
		if (next != null)
		{
			SetFocus(next);
			return true;
		}
		return false;
	}

	public bool TryMoveFocusBack()
	{
		if (ManagerBase<ControlManager>.softInstance != null)
		{
			return false;
		}
		for (int i = 0; i < _focusedHistory.Count; i++)
		{
			if (_focusedHistory[i] != focused && _focusedHistory[i] != null && _focusedHistory[i].IsValid() && _focusedHistory[i].CanBeFocused())
			{
				IGamepadFocusable f = _focusedHistory[i];
				_focusedHistory[i] = null;
				SetFocus(f);
				return true;
			}
		}
		return false;
	}

	private void NotifyNewListeners(List<IGamepadFocusListener> newListeners)
	{
		PointerEventData ev = new PointerEventData(EventSystem.current);
		foreach (IGamepadFocusListener old in _focusedListeners)
		{
			try
			{
				if (old.IsValid() && (newListeners == null || !newListeners.Contains(old)))
				{
					old.OnFocusStateChanged(state: false);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (_focusedHistory[0] is Component last && last != null)
		{
			ListReturnHandle<IPointerExitHandler> handle;
			foreach (IPointerExitHandler item in last.GetComponentsNonAlloc(out handle))
			{
				item.OnPointerExit(ev);
			}
			handle.Return();
		}
		if (newListeners != null)
		{
			foreach (IGamepadFocusListener newListener in newListeners)
			{
				try
				{
					if (newListener.IsValid() && !_focusedListeners.Contains(newListener))
					{
						newListener.OnFocusStateChanged(state: true);
					}
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			}
		}
		if (focused is Component now && now != null)
		{
			ListReturnHandle<IPointerEnterHandler> handle2;
			foreach (IPointerEnterHandler item2 in now.GetComponentsNonAlloc(out handle2))
			{
				item2.OnPointerEnter(ev);
			}
			handle2.Return();
		}
		_focusedListeners.Clear();
		if (newListeners != null)
		{
			_focusedListeners.AddRange(newListeners);
		}
	}

	private bool ShouldDoDpad()
	{
		if (focused == null)
		{
			return ManagerBase<ControlManager>.softInstance == null;
		}
		return true;
	}

	private void DoGamepadInputs()
	{
		if (focused != null && (!focused.IsValid() || !focused.CanBeFocused()))
		{
			SetFocus(null);
		}
		if (focused is IGamepadFocusableOverrideInput oo)
		{
			oo.OnGamepadUpdate();
		}
		bool num = !ShouldDoDpad();
		if (!num && it_gamepadUp.downRepeated)
		{
			if (!(focused is IGamepadFocusableOverrideInput o) || !o.OnGamepadDpadUp())
			{
				MoveFocus(Vector2.up);
			}
			DewEffect.Play(fxTick);
		}
		if (!num && it_gamepadLeft.downRepeated)
		{
			if (!(focused is IGamepadFocusableOverrideInput o2) || !o2.OnGamepadDpadLeft())
			{
				MoveFocus(Vector2.left);
			}
			DewEffect.Play(fxTick);
		}
		if (!num && it_gamepadDown.downRepeated)
		{
			if (!(focused is IGamepadFocusableOverrideInput o3) || !o3.OnGamepadDpadDown())
			{
				MoveFocus(Vector2.down);
			}
			DewEffect.Play(fxTick);
		}
		if (!num && it_gamepadRight.downRepeated)
		{
			if (!(focused is IGamepadFocusableOverrideInput o4) || !o4.OnGamepadDpadRight())
			{
				MoveFocus(Vector2.right);
			}
			DewEffect.Play(fxTick);
		}
		if (((focused != null && focused.CanHoldConfirmToRepeat()) ? it_gamepadConfirm.downRepeated : it_gamepadConfirm.down) && focused != null && (!(focused is IGamepadFocusableOverrideInput ov0) || !ov0.OnGamepadConfirm()))
		{
			Component comp = (Component)focused;
			SimulateClickOnUIElement(comp);
		}
		if (it_back.down && (focused == null || !(focused is IGamepadFocusableOverrideInput ov1) || !ov1.OnGamepadBack()))
		{
			GoBack();
		}
		if (focused == null || focused.GetSelectionDisplayType() == SelectionDisplayType.Dont)
		{
			focusDisplayObject.SetActive(value: false);
			return;
		}
		SelectionDisplayType type = focused.GetSelectionDisplayType();
		focusDisplayObject.SetActive(value: true);
		focusDisplayBoxObject.SetActive(type == SelectionDisplayType.Box);
		focusDisplayCircleObject.SetActive(type == SelectionDisplayType.Circle);
		Transform ogParent = focusDisplayTransform.parent;
		RectTransform rt = (RectTransform)((Component)focused).transform;
		int ogIndex = focusDisplayTransform.GetSiblingIndex();
		focusDisplayTransform.SetParent(rt, worldPositionStays: true);
		focusDisplayTransform.localScale = Vector3.one;
		focusDisplayTransform.localRotation = Quaternion.identity;
		focusDisplayTransform.anchorMin = Vector2.zero;
		focusDisplayTransform.anchorMax = Vector2.one;
		focusDisplayTransform.sizeDelta = Vector2.zero;
		focusDisplayTransform.anchoredPosition = Vector2.zero;
		focusDisplayTransform.localPosition = focusDisplayTransform.localPosition.WithZ(0f);
		focusDisplayTransform.SetParent(ogParent, worldPositionStays: true);
		focusDisplayTransform.SetSiblingIndex(ogIndex);
		focusDisplayTransform.localPosition = focusDisplayTransform.localPosition.WithZ(0f);
	}

	public void SimulateClickOnUIElement(Component comp)
	{
		PointerEventData e = new PointerEventData(EventSystem.current)
		{
			button = PointerEventData.InputButton.Left
		};
		IPointerDownHandler[] components = comp.GetComponents<IPointerDownHandler>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].OnPointerDown(e);
		}
		IPointerUpHandler[] components2 = comp.GetComponents<IPointerUpHandler>();
		for (int i = 0; i < components2.Length; i++)
		{
			components2[i].OnPointerUp(e);
		}
		IPointerClickHandler[] components3 = comp.GetComponents<IPointerClickHandler>();
		for (int i = 0; i < components3.Length; i++)
		{
			components3[i].OnPointerClick(e);
		}
	}

	public void SetFocusOnFirstFocusable(GameObject parent)
	{
		if (parent == null)
		{
			return;
		}
		ListReturnHandle<IGamepadFocusable> handle;
		List<IGamepadFocusable> focusables = parent.GetComponentsInChildrenNonAlloc(out handle);
		if (focusables.Count > 0)
		{
			focusables.Sort((IGamepadFocusable a, IGamepadFocusable b) => ((Component)a).transform.GetSiblingIndex().CompareTo(((Component)b).transform.GetSiblingIndex()));
			foreach (IGamepadFocusable f in focusables)
			{
				if (f.CanBeFocused())
				{
					SetFocus(f);
					break;
				}
			}
		}
		handle.Return();
	}

	public void PopulateCacheData(ref GetFocusTargetSettings settings, out Action handle)
	{
		UI_GamepadFocusableGroup currentGroup = null;
		if (ManagerBase<GlobalUIManager>.instance.focused is Component comp && comp != null)
		{
			currentGroup = comp.GetComponentInParent<UI_GamepadFocusableGroup>();
		}
		ListReturnHandle<(IGamepadFocusable, Rect)> h0;
		List<(IGamepadFocusable, Rect)> candidateFocusables = DewPool.GetList(out h0);
		foreach (IGamepadFocusable f in ManagerBase<GlobalUIManager>.instance._allFocusables)
		{
			if (!f.IsValid() || f == ManagerBase<GlobalUIManager>.instance.focused)
			{
				continue;
			}
			Rect candidateRect = ((RectTransform)((Component)f).transform).GetScreenSpaceRect();
			if (settings.condition == null || settings.condition(f))
			{
				if (!f.CanBeFocused())
				{
					DewDebug.DrawRect(candidateRect, Color.red, 0.5f);
					continue;
				}
				DewDebug.DrawRect(candidateRect, Color.blue, 0.5f);
				candidateFocusables.Add((f, candidateRect));
			}
		}
		ListReturnHandle<(UI_GamepadFocusableGroup, Rect)> h1;
		List<(UI_GamepadFocusableGroup, Rect)> candidateGroups = DewPool.GetList(out h1);
		if (settings.condition == null && settings.customScoreFunc == null)
		{
			foreach (UI_GamepadFocusableGroup g in _allGroups)
			{
				if (!(currentGroup == g) && (!(currentGroup != null) || !currentGroup.transform.IsSelfOrDescendantOf(g.transform)) && g.IsValid())
				{
					Rect candidateRect2 = ((RectTransform)g.transform).GetScreenSpaceRect();
					DewDebug.DrawRect(candidateRect2, Color.gray, 0.5f);
					DewDebug.DrawRect(candidateRect2.Expand(1f), Color.gray, 0.5f);
					DewDebug.DrawRect(candidateRect2.Expand(2f), Color.gray, 0.5f);
					candidateGroups.Add((g, candidateRect2));
				}
			}
		}
		if (candidateGroups.Count > 0)
		{
			for (int i = candidateFocusables.Count - 1; i >= 0; i--)
			{
				if (candidateFocusables[i].Item1 is Component c)
				{
					UI_GamepadFocusableGroup parentGroup = c.GetComponentInParent<UI_GamepadFocusableGroup>();
					if (parentGroup != null && parentGroup != currentGroup)
					{
						candidateFocusables.RemoveAt(i);
					}
				}
			}
		}
		settings.cacheData = new GetFocusTargetCacheData
		{
			candidateFocusables = candidateFocusables,
			candidateGroups = candidateGroups
		};
		handle = delegate
		{
			if (h0.needToReturn)
			{
				h0.Return();
			}
			if (h1.needToReturn)
			{
				h1.Return();
			}
		};
	}

	public bool TryGetFocusTargetGeneric(Vector3 direction, Func<IGamepadFocusable, bool> condition, out IGamepadFocusable next)
	{
		GetFocusTargetSettings s = new GetFocusTargetSettings
		{
			direction = direction,
			condition = condition
		};
		PopulateCacheData(ref s, out var handle);
		s.angleLimit = new Vector2(0f, 10f);
		s.normalizedPerpendicularDistLimit = new Vector2(0f, 0.015f);
		s.normalizedDistLimit = new Vector2(0f, 0.125f);
		bool result = TryGetFocusTarget(s, out next);
		if (!result)
		{
			s.angleLimit = new Vector2(0f, 30f);
			s.normalizedPerpendicularDistLimit = new Vector2(0f, 0.015f);
			s.normalizedDistLimit = new Vector2(0f, 0.3f);
			result = TryGetFocusTarget(s, out next);
		}
		if (!result)
		{
			s.angleLimit = new Vector2(0f, 45f);
			s.normalizedPerpendicularDistLimit = new Vector2(0f, 0.35f);
			s.normalizedDistLimit = new Vector2(0f, float.PositiveInfinity);
			result = TryGetFocusTarget(s, out next);
		}
		if (!result)
		{
			s.angleLimit = new Vector2(0f, 85f);
			s.normalizedPerpendicularDistLimit = new Vector2(0f, float.PositiveInfinity);
			s.normalizedDistLimit = new Vector2(0f, float.PositiveInfinity);
			s.useCenterOfRect = true;
			TryGetFocusTarget(s, out next);
		}
		handle();
		return next != null;
	}

	public bool TryGetFocusTarget(GetFocusTargetSettings s, out IGamepadFocusable next)
	{
		Component obj = (Component)focused;
		Vector2 direction = (obj.transform.rotation * s.direction).WithZ(0f).normalized;
		Rect beforeRect = ((RectTransform)obj.transform).GetScreenSpaceRect();
		Vector2 beforePos = ((direction.x > 0.5f) ? ((direction.y > 0.5f) ? new Vector2(beforeRect.xMax + 1f, beforeRect.yMax + 1f) : ((!(direction.y < -0.5f)) ? new Vector2(beforeRect.xMax, beforeRect.center.y) : new Vector2(beforeRect.xMax + 1f, beforeRect.yMin - 1f))) : ((direction.x < -0.5f) ? ((direction.y > 0.5f) ? new Vector2(beforeRect.xMin - 1f, beforeRect.yMax + 1f) : ((!(direction.y < -0.5f)) ? new Vector2(beforeRect.xMin - 1f, beforeRect.center.y) : new Vector2(beforeRect.xMin - 1f, beforeRect.yMin - 1f))) : ((direction.y > 0.5f) ? new Vector2(beforeRect.center.x, beforeRect.yMax + 1f) : ((!(direction.y < -0.5f)) ? new Vector2(beforeRect.center.x, beforeRect.center.y) : new Vector2(beforeRect.center.x, beforeRect.yMin - 1f)))));
		ListReturnHandle<float> handle1;
		List<float> scores = DewPool.GetList(out handle1);
		Vector2 perpDistLimit = s.normalizedPerpendicularDistLimit;
		Vector2 distLimit = s.normalizedDistLimit;
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
		{
			perpDistLimit *= (float)Screen.height;
			distLimit *= (float)Screen.width;
		}
		else
		{
			perpDistLimit *= (float)Screen.width;
			distLimit *= (float)Screen.height;
		}
		distLimit.y = Mathf.Min(distLimit.y, 100000f);
		float mul = 1f / Mathf.Cos(s.angleLimit.y * (MathF.PI / 180f));
		if (mul > 9000f)
		{
			mul = 9000f;
		}
		if (mul < 0.1f)
		{
			mul = 0.1f;
		}
		Vector2 leftPoint = beforePos + RotateVector(direction, 0f - s.angleLimit.y) * distLimit.y * mul;
		Vector2 rightPoint = beforePos + RotateVector(direction, s.angleLimit.y) * distLimit.y * mul;
		Color debugColor = (s.useCenterOfRect ? Color.cyan : Color.magenta);
		Debug.DrawLine(beforePos, leftPoint, debugColor, 0.5f);
		Debug.DrawLine(beforePos, rightPoint, debugColor, 0.5f);
		Debug.DrawLine(rightPoint, leftPoint, debugColor, 0.5f);
		Action handle2 = null;
		if (s.cacheData.candidateFocusables == null)
		{
			PopulateCacheData(ref s, out handle2);
		}
		foreach (var item in s.cacheData.candidateFocusables)
		{
			Rect candidateRect = item.Item2;
			if (s.useCenterOfRect)
			{
				if (IsPointInTriangle(candidateRect.center, beforePos, leftPoint, rightPoint))
				{
					if (s.customScoreFunc != null)
					{
						scores.Add(s.customScoreFunc(item.Item1));
					}
					else
					{
						scores.Add(1f / Vector2.Distance(candidateRect.center, beforePos));
					}
					DewDebug.DrawRect(candidateRect, Color.green, 0.5f);
				}
				else
				{
					scores.Add(-1f);
				}
			}
			else if (IsRectIntersectingTriangle(candidateRect, beforePos, leftPoint, rightPoint))
			{
				if (s.customScoreFunc != null)
				{
					scores.Add(s.customScoreFunc(item.Item1));
				}
				else
				{
					scores.Add(1f / GetClosestPointDistance(candidateRect, beforePos));
				}
				DewDebug.DrawRect(candidateRect, Color.green, 0.5f);
			}
			else
			{
				scores.Add(-1f);
			}
		}
		foreach (var candidateGroup in s.cacheData.candidateGroups)
		{
			Rect candidateRect2 = candidateGroup.Item2;
			if (s.useCenterOfRect)
			{
				if (IsPointInTriangle(candidateRect2.center, beforePos, leftPoint, rightPoint))
				{
					scores.Add(1f / Vector2.Distance(candidateRect2.center, beforePos));
					DewDebug.DrawRect(candidateRect2, Color.green, 0.5f);
				}
				else
				{
					scores.Add(-1f);
				}
			}
			else if (IsRectIntersectingTriangle(candidateRect2, beforePos, leftPoint, rightPoint))
			{
				scores.Add(1f / GetClosestPointDistance(candidateRect2, beforePos));
				DewDebug.DrawRect(candidateRect2, Color.green, 0.5f);
			}
			else
			{
				scores.Add(-1f);
			}
		}
		next = null;
		float bestScore = float.NegativeInfinity;
		for (int i = 0; i < scores.Count; i++)
		{
			if (scores[i] < 0f || scores[i] < bestScore)
			{
				continue;
			}
			if (i >= s.cacheData.candidateFocusables.Count)
			{
				IGamepadFocusable n = s.cacheData.candidateGroups[i - s.cacheData.candidateFocusables.Count].Item1.GetEnterFocusable(s.direction);
				if (n != null)
				{
					next = n;
					bestScore = scores[i];
				}
			}
			else
			{
				next = s.cacheData.candidateFocusables[i].Item1;
				bestScore = scores[i];
			}
		}
		handle2?.Invoke();
		handle1.Return();
		return next != null;
	}

	public static List<RectResult> GetIntersectingRects(List<Rect> rects, Vector2 origin, Vector2 direction, float angle, float radius)
	{
		List<RectResult> results = new List<RectResult>();
		Vector2 leftPoint = origin + RotateVector(direction, (0f - angle) / 2f) * radius;
		Vector2 rightPoint = origin + RotateVector(direction, angle / 2f) * radius;
		foreach (Rect rect in rects)
		{
			if (IsRectIntersectingTriangle(rect, origin, leftPoint, rightPoint))
			{
				float distance = GetClosestPointDistance(rect, origin);
				results.Add(new RectResult
				{
					rect = rect,
					distance = distance
				});
			}
		}
		return results;
	}

	private static bool IsRectIntersectingTriangle(Rect rect, Vector2 a, Vector2 b, Vector2 c)
	{
		if (rect.Contains(a) || rect.Contains(b) || rect.Contains(c))
		{
			return true;
		}
		Vector2[] array = new Vector2[4]
		{
			new Vector2(rect.xMin, rect.yMin),
			new Vector2(rect.xMax, rect.yMin),
			new Vector2(rect.xMax, rect.yMax),
			new Vector2(rect.xMin, rect.yMax)
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (IsPointInTriangle(array[i], a, b, c))
			{
				return true;
			}
		}
		if (LineIntersectsRect(a, b, rect) || LineIntersectsRect(b, c, rect) || LineIntersectsRect(c, a, rect))
		{
			return true;
		}
		return false;
	}

	private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
	{
		float num = Sign(p, a, b);
		float d2 = Sign(p, b, c);
		float d3 = Sign(p, c, a);
		bool hasNegative = num < 0f || d2 < 0f || d3 < 0f;
		bool hasPositive = num > 0f || d2 > 0f || d3 > 0f;
		return !(hasNegative && hasPositive);
	}

	private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
	}

	private static bool LineIntersectsRect(Vector2 p1, Vector2 p2, Rect r)
	{
		if (!LineIntersectsLine(p1, p2, new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin)) && !LineIntersectsLine(p1, p2, new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax)) && !LineIntersectsLine(p1, p2, new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax)))
		{
			return LineIntersectsLine(p1, p2, new Vector2(r.xMin, r.yMax), new Vector2(r.xMin, r.yMin));
		}
		return true;
	}

	private static bool LineIntersectsLine(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
	{
		Vector2 b3 = a2 - a1;
		Vector2 d = b2 - b1;
		float bDotDPerp = b3.x * d.y - b3.y * d.x;
		if (bDotDPerp == 0f)
		{
			return false;
		}
		Vector2 c = b1 - a1;
		float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
		if (t < 0f || t > 1f)
		{
			return false;
		}
		float u = (c.x * b3.y - c.y * b3.x) / bDotDPerp;
		if (u < 0f || u > 1f)
		{
			return false;
		}
		return true;
	}

	private static float GetClosestPointDistance(Rect rect, Vector2 origin)
	{
		return Vector2.Distance(b: new Vector2(Mathf.Clamp(origin.x, rect.xMin, rect.xMax), Mathf.Clamp(origin.y, rect.yMin, rect.yMax)), a: origin);
	}

	private static Vector2 RotateVector(Vector2 vector, float degrees)
	{
		float f = degrees * (MathF.PI / 180f);
		float sin = Mathf.Sin(f);
		float cos = Mathf.Cos(f);
		return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
	}

	public IEnumerator HighlightForTutorial(TutorialHighlightSettings s)
	{
		if (!(s.target == null))
		{
			_highlightStartUnscaledTime = Time.unscaledTime;
			_currentHighlight = s;
			_didAddContinueText = false;
			tutHighlightText.text = _currentHighlight.rawText + "<size=80%>\n<color=#ccc> ";
			_anchoredPosCv = Vector2.zero;
			_sizeDeltaCv = Vector2.zero;
			UpdateCurrentHighlight(Time.frameCount - _lastHighlightFrame > 2);
			DewEffect.Play(fxHighlightShow);
			yield return new WaitWhile(() => _currentHighlight == s);
		}
	}

	public void ClickHighlightForTutorial()
	{
		if (_currentHighlight != null && !(Time.unscaledTime - _highlightStartUnscaledTime < 0.75f))
		{
			UnhighlightForTutorial();
			DewEffect.Play(fxHighlightHide);
		}
	}

	public void UnhighlightForTutorial()
	{
		if (_currentHighlight != null)
		{
			_currentHighlight = null;
			tutHighlightObject.SetActive(value: false);
		}
	}

	private void UnhighlightForTutorial_Imp(Scene arg0, LoadSceneMode arg1)
	{
		UnhighlightForTutorial();
	}

	private void Start_Tutorial()
	{
		SceneManager.sceneLoaded += UnhighlightForTutorial_Imp;
		AddBackHandler(this, 99999, () => isTutorialHighlighting);
	}

	private void OnDestroy_Tutorial()
	{
		SceneManager.sceneLoaded -= UnhighlightForTutorial_Imp;
	}

	private void FrameUpdate_Tutorial()
	{
		if (_currentHighlight != null)
		{
			_lastHighlightFrame = Time.frameCount;
			UpdateCurrentHighlight(immediately: false);
		}
	}

	private void UpdateCurrentHighlight(bool immediately)
	{
		if (_currentHighlight.target == null)
		{
			UnhighlightForTutorial();
			return;
		}
		if (!_didAddContinueText && Time.unscaledTime - _highlightStartUnscaledTime > 0.75f)
		{
			_didAddContinueText = true;
			tutHighlightText.text = tutHighlightText.text.Substring(0, tutHighlightText.text.Length - 1) + DewLocalization.GetUIValue((DewInput.currentMode == InputMode.KeyboardAndMouse) ? "Generic_TutorialHighlight_Continue_PC" : "Generic_TutorialHighlight_Continue_Gamepad");
		}
		tutHighlightObject.SetActive(value: true);
		float divisor = 1f / tutHighlightBackdropLeft.lossyScale.x;
		Rect ssRect = _currentHighlight.target.GetScreenSpaceRect();
		ssRect.yMax += _currentHighlight.padding.x;
		ssRect.xMax += _currentHighlight.padding.y;
		ssRect.yMin -= _currentHighlight.padding.z;
		ssRect.xMin -= _currentHighlight.padding.w;
		Vector2 apTarget = new Vector2(ssRect.x * divisor, ssRect.y * divisor);
		Vector2 sdTarget = new Vector2(ssRect.width * divisor, ssRect.height * divisor);
		if (immediately)
		{
			tutHighlightBox.anchoredPosition = new Vector2(apTarget.x - 60f, apTarget.y - 60f);
			tutHighlightBox.sizeDelta = new Vector2(sdTarget.x + 120f, sdTarget.y + 120f);
		}
		tutHighlightBox.anchoredPosition = Vector2.SmoothDamp(tutHighlightBox.anchoredPosition, apTarget, ref _anchoredPosCv, 0.1f, float.PositiveInfinity, Time.unscaledDeltaTime);
		tutHighlightBox.sizeDelta = Vector2.SmoothDamp(tutHighlightBox.sizeDelta, sdTarget, ref _sizeDeltaCv, 0.1f, float.PositiveInfinity, Time.unscaledDeltaTime);
		ssRect = tutHighlightBox.GetScreenSpaceRect();
		Vector2 min = ssRect.min * divisor;
		Vector2 max = ssRect.max * divisor;
		Vector2 center = (min + max) * 0.5f;
		float sH = (float)Screen.height * divisor;
		float sW = (float)Screen.width * divisor;
		tutHighlightBackdropLeft.anchoredPosition = new Vector2(0f, 0f);
		tutHighlightBackdropLeft.sizeDelta = new Vector2(min.x, sH);
		tutHighlightBackdropRight.anchoredPosition = new Vector2(max.x, 0f);
		tutHighlightBackdropRight.sizeDelta = new Vector2(sW - min.x, sH);
		tutHighlightBackdropTop.anchoredPosition = new Vector2(min.x, max.y);
		tutHighlightBackdropTop.sizeDelta = new Vector2(max.x - min.x, sH - max.y);
		tutHighlightBackdropBottom.anchoredPosition = new Vector2(min.x, 0f);
		tutHighlightBackdropBottom.sizeDelta = new Vector2(max.x - min.x, min.y);
		RectTransform textRt = (RectTransform)tutHighlightText.transform;
		float margin = 35f;
		float desiredWidth = Mathf.Min(center.x, sW - center.x, 700f) * 2f - 100f;
		float desiredHeight = Mathf.Min(center.y, sH - center.y, 700f) * 2f - 100f;
		switch (_currentHighlight.textPlacement)
		{
		case TutorialHighlightTextPlacement.Left:
			tutHighlightText.verticalAlignment = VerticalAlignmentOptions.Middle;
			tutHighlightText.horizontalAlignment = HorizontalAlignmentOptions.Right;
			textRt.sizeDelta = new Vector2(desiredWidth, desiredHeight);
			textRt.anchoredPosition = new Vector2(min.x - desiredWidth - margin, center.y - desiredHeight * 0.5f);
			break;
		case TutorialHighlightTextPlacement.Right:
			tutHighlightText.verticalAlignment = VerticalAlignmentOptions.Middle;
			tutHighlightText.horizontalAlignment = HorizontalAlignmentOptions.Left;
			textRt.sizeDelta = new Vector2(desiredWidth, desiredHeight);
			textRt.anchoredPosition = new Vector2(max.x + margin, center.y - desiredHeight * 0.5f);
			break;
		case TutorialHighlightTextPlacement.Top:
			tutHighlightText.verticalAlignment = VerticalAlignmentOptions.Bottom;
			tutHighlightText.horizontalAlignment = HorizontalAlignmentOptions.Center;
			textRt.sizeDelta = new Vector2(desiredWidth, sH);
			textRt.anchoredPosition = new Vector2(center.x - desiredWidth * 0.5f, max.y + margin);
			break;
		case TutorialHighlightTextPlacement.Bottom:
			tutHighlightText.verticalAlignment = VerticalAlignmentOptions.Top;
			tutHighlightText.horizontalAlignment = HorizontalAlignmentOptions.Center;
			textRt.sizeDelta = new Vector2(desiredWidth, sH);
			textRt.anchoredPosition = new Vector2(center.x - desiredWidth * 0.5f, min.y - margin - sH);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void LogicUpdate_Tutorial()
	{
	}
}
