using System;
using HighlightPlus;
using UnityEngine;

public class ObjectHighlightManager : ManagerBase<ObjectHighlightManager>, ICinematicCameraHelperStateReceiver
{
	[Serializable]
	public struct OutlineStyle
	{
		public Color color;
	}

	public const float MouseOverStrength = 0.4f;

	public const float MouseDownStrength = 0.6f;

	[NonSerialized]
	public bool isObjectHighlightDisabled;

	public OutlineStyle interactable;

	public OutlineStyle own;

	public OutlineStyle neutral;

	public OutlineStyle enemy;

	public OutlineStyle ally;

	private HighlightProvider _highlightedObject;

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!InGameUIManager.instance.IsState("Playing") || isObjectHighlightDisabled || DewInput.currentMode != InputMode.KeyboardAndMouse)
		{
			if (_highlightedObject != null)
			{
				Unhighlight();
			}
			return;
		}
		HighlightProvider highlightable = ControlManager.GetHighlightableOnCursor();
		if (highlightable == null && _highlightedObject != null)
		{
			Unhighlight();
		}
		if (highlightable != null)
		{
			if (highlightable != _highlightedObject)
			{
				Unhighlight();
			}
			highlightable.cursorStatus = ((!Input.GetKey(KeyCode.Mouse0)) ? HighlightProvider.CursorStatus.Hover : HighlightProvider.CursorStatus.Active);
			_highlightedObject = highlightable;
		}
	}

	private void Unhighlight()
	{
		if (!(_highlightedObject == null))
		{
			_highlightedObject.cursorStatus = HighlightProvider.CursorStatus.None;
			_highlightedObject = null;
		}
	}

	public void OnCinematicCameraHelperChanged(bool on)
	{
		isObjectHighlightDisabled = on;
	}

	public static HighlightProfile GetProfile()
	{
		return Resources.Load<HighlightProfile>("Entity Highlight Plus Profile");
	}
}
