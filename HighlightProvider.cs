using UnityEngine;

[LogicUpdatePriority(500)]
public class HighlightProvider : LogicBehaviour
{
	public enum CursorStatus
	{
		None,
		Hover,
		Active
	}

	private CursorStatus _cursorStatus;

	private float _clickTime = float.NegativeInfinity;

	public CursorStatus cursorStatus
	{
		get
		{
			return _cursorStatus;
		}
		set
		{
			if (_cursorStatus != value)
			{
				_cursorStatus = value;
				OnCursorStatusUpdated();
			}
		}
	}

	public float elapsedClickTime => Time.time - _clickTime;

	public void ShowClick()
	{
		_clickTime = Time.time;
		OnClickTimeUpdated();
	}

	protected virtual void OnCursorStatusUpdated()
	{
	}

	protected virtual void OnClickTimeUpdated()
	{
	}
}
