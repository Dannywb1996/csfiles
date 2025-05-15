using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_GamepadFocusableGroup : MonoBehaviour
{
	public EnterGroupBehavior enterBehavior;

	private void Start()
	{
		ManagerBase<GlobalUIManager>.instance._allGroups.Add(this);
	}

	private void OnDestroy()
	{
		if (ManagerBase<GlobalUIManager>.instance != null)
		{
			ManagerBase<GlobalUIManager>.instance._allGroups.Remove(this);
		}
	}

	public virtual IGamepadFocusable GetEnterFocusable(Vector2 direction)
	{
		int xScoreMult = 0;
		int yScoreMult = 0;
		ListReturnHandle<IGamepadFocusable> handle;
		List<IGamepadFocusable> children = ((Component)this).GetComponentsInChildrenNonAlloc(out handle);
		IGamepadFocusable bestFocusable = null;
		float bestScore = float.NegativeInfinity;
		switch (enterBehavior)
		{
		case EnterGroupBehavior.UseDistanceFromPrevious:
			if (ManagerBase<GlobalUIManager>.instance.focused is Component currentFocused2 && currentFocused2 != null)
			{
				Vector2 current2 = ((RectTransform)currentFocused2.transform).GetScreenSpaceRect().center;
				foreach (IGamepadFocusable c2 in children)
				{
					Vector2 t = c2.GetTransform().GetScreenSpaceRect().center;
					float score2 = 1f / Vector2.Distance(current2, t);
					if (score2 > bestScore && c2.CanBeFocused())
					{
						bestScore = score2;
						bestFocusable = c2;
					}
				}
				handle.Return();
				return bestFocusable;
			}
			xScoreMult = -1;
			yScoreMult = 1;
			break;
		case EnterGroupBehavior.UseEnterDirection:
			if (ManagerBase<GlobalUIManager>.instance.focused is Component currentFocused && currentFocused != null)
			{
				Vector2 current = ((RectTransform)currentFocused.transform).GetScreenSpaceRect().center;
				foreach (IGamepadFocusable c in children)
				{
					Vector2 delta = c.GetTransform().GetScreenSpaceRect().center - current;
					float projectedDistance = Vector2.Dot(direction.normalized, delta);
					float score = 1f / projectedDistance;
					if (score > bestScore && c.CanBeFocused())
					{
						bestScore = score;
						bestFocusable = c;
					}
				}
				handle.Return();
				return bestFocusable;
			}
			xScoreMult = -1;
			yScoreMult = 1;
			break;
		case EnterGroupBehavior.PrioritizeLT:
			xScoreMult = -1;
			yScoreMult = 1;
			break;
		case EnterGroupBehavior.PrioritizeRT:
			xScoreMult = 1;
			yScoreMult = 1;
			break;
		case EnterGroupBehavior.PrioritizeLB:
			xScoreMult = -1;
			yScoreMult = -1;
			break;
		case EnterGroupBehavior.PrioritizeRB:
			xScoreMult = 1;
			yScoreMult = -1;
			break;
		case EnterGroupBehavior.PrioritizeTop:
			yScoreMult = 1;
			break;
		case EnterGroupBehavior.PrioritizeLeft:
			xScoreMult = -1;
			break;
		case EnterGroupBehavior.PrioritizeRight:
			xScoreMult = 1;
			break;
		case EnterGroupBehavior.PrioritizeBottom:
			yScoreMult = -1;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		foreach (IGamepadFocusable c3 in children)
		{
			Vector3 p = c3.GetTransform().position;
			float score3 = p.x * (float)xScoreMult + p.y * (float)yScoreMult;
			if (score3 > bestScore && c3.CanBeFocused())
			{
				bestScore = score3;
				bestFocusable = c3;
			}
		}
		handle.Return();
		return bestFocusable;
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}
}
