using System;
using UnityEngine;

public class UI_GamepadNavigationHint : MonoBehaviour, IGamepadNavigationHint
{
	[Serializable]
	public struct NavigationSettings
	{
		public GamepadNavigation navigation;

		public GameObject customTarget;

		public bool limitToChildren;

		public bool canFallbackToGeneric;
	}

	public NavigationSettings navigation;

	public bool useDifferentNavigationsPerDirection;

	[Header("Up")]
	public NavigationSettings up;

	[Header("Left")]
	public NavigationSettings left;

	[Header("Down")]
	public NavigationSettings down;

	[Header("Right")]
	public NavigationSettings right;

	public bool TryGetUp(out IGamepadFocusable next)
	{
		return TryGet(Vector3.up, up, out next);
	}

	public bool TryGetLeft(out IGamepadFocusable next)
	{
		return TryGet(Vector3.left, left, out next);
	}

	public bool TryGetRight(out IGamepadFocusable next)
	{
		return TryGet(Vector3.right, right, out next);
	}

	public bool TryGetDown(out IGamepadFocusable next)
	{
		return TryGet(Vector3.down, down, out next);
	}

	private bool TryGet(Vector3 direction, NavigationSettings nav, out IGamepadFocusable next)
	{
		if (!useDifferentNavigationsPerDirection)
		{
			nav = navigation;
		}
		next = null;
		Transform t = base.transform;
		Func<IGamepadFocusable, bool> condition = (nav.limitToChildren ? ((Func<IGamepadFocusable, bool>)((IGamepadFocusable focusable) => focusable.GetTransform().IsSelfOrDescendantOf(t))) : null);
		GetFocusTargetSettings s = new GetFocusTargetSettings
		{
			direction = direction,
			condition = condition
		};
		ManagerBase<GlobalUIManager>.instance.PopulateCacheData(ref s, out var handle);
		switch (nav.navigation)
		{
		case GamepadNavigation.Generic:
			ManagerBase<GlobalUIManager>.instance.TryGetFocusTargetGeneric(direction, condition, out next);
			break;
		case GamepadNavigation.Grid:
			s.angleLimit = new Vector2(0f, 1f);
			s.normalizedPerpendicularDistLimit = new Vector2(0f, 0.005f);
			s.normalizedDistLimit = new Vector2(0f, 0.1f);
			if (!ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(s, out next))
			{
				s.angleLimit = new Vector2(0f, 45f);
				s.normalizedPerpendicularDistLimit = new Vector2(0f, 0.05f);
				s.normalizedDistLimit = new Vector2(0f, 0.2f);
				ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(s, out next);
			}
			break;
		case GamepadNavigation.GridWrapAround:
			s.angleLimit = new Vector2(0f, 0.5f);
			s.normalizedPerpendicularDistLimit = new Vector2(0f, 0.005f);
			s.normalizedDistLimit = new Vector2(0f, 0.1f);
			if (!ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(s, out next))
			{
				Vector3 focused = ManagerBase<GlobalUIManager>.instance.focused.GetTransform().position;
				s.direction = -direction;
				s.angleLimit = new Vector2(0f, 1f);
				s.normalizedPerpendicularDistLimit = new Vector2(0f, 0.01f);
				s.normalizedDistLimit = new Vector2(0f, float.PositiveInfinity);
				s.customScoreFunc = (IGamepadFocusable f) => Vector3.Distance(f.GetTransform().position, focused);
				ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(s, out next);
			}
			break;
		case GamepadNavigation.GridLong:
			s.angleLimit = new Vector2(0f, 0.5f);
			s.normalizedPerpendicularDistLimit = new Vector2(0f, 0.005f);
			s.normalizedDistLimit = new Vector2(0f, 0.3f);
			if (!ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(s, out next))
			{
				s.angleLimit = new Vector2(0f, 45f);
				s.normalizedPerpendicularDistLimit = new Vector2(0f, 0.05f);
				s.normalizedDistLimit = new Vector2(0f, 0.2f);
				ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(s, out next);
			}
			break;
		case GamepadNavigation.Wide:
			s.angleLimit = new Vector2(0f, 80f);
			s.normalizedPerpendicularDistLimit = new Vector2(0f, 1f);
			s.normalizedDistLimit = new Vector2(0.2f, 1f);
			ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(s, out next);
			break;
		case GamepadNavigation.HeroInfoBar:
		{
			s.angleLimit = new Vector2(0f, 80f);
			s.normalizedDistLimit = new Vector2(0f, 0.05f);
			Vector3 current = ManagerBase<GlobalUIManager>.instance.focused.GetTransform().position;
			s.customScoreFunc = delegate(IGamepadFocusable f)
			{
				Vector3 position = f.GetTransform().position;
				return 1f / (position.x + Mathf.Abs(position.y - current.y) * 50f);
			};
			ManagerBase<GlobalUIManager>.instance.TryGetFocusTarget(s, out next);
			break;
		}
		case GamepadNavigation.Custom:
		{
			if (nav.customTarget != null && nav.customTarget.TryGetComponent<IGamepadFocusable>(out var comp) && comp.CanBeFocused())
			{
				next = comp;
			}
			if (nav.customTarget != null && nav.customTarget.TryGetComponent<UI_GamepadFocusableGroup>(out var group) && group.IsValid())
			{
				next = group.GetEnterFocusable(direction);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("navigation", nav.navigation, null);
		}
		handle();
		if (next == null && nav.canFallbackToGeneric)
		{
			return false;
		}
		return true;
	}
}
