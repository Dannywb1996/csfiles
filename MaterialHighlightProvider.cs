using System;
using UnityEngine;

public class MaterialHighlightProvider : HighlightProvider
{
	public Material targetMaterial;

	public string targetProperty;

	public float clickValueMax;

	public float cursorValueMultiplier;

	private bool _isAnimatingClick;

	private void Start()
	{
		OnCursorStatusUpdated();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!(targetMaterial == null))
		{
			if (base.elapsedClickTime < 0.35f)
			{
				float clickedValue = Mathf.Round(Mathf.PingPong(base.elapsedClickTime * 16f + 1f, 1f)) * 0.5f + 0.5f;
				targetMaterial.SetFloat(targetProperty, clickedValue * clickValueMax);
			}
			else if (_isAnimatingClick)
			{
				_isAnimatingClick = false;
				OnCursorStatusUpdated();
			}
		}
	}

	protected override void OnClickTimeUpdated()
	{
		base.OnClickTimeUpdated();
		_isAnimatingClick = true;
	}

	protected override void OnCursorStatusUpdated()
	{
		base.OnCursorStatusUpdated();
		if (!(targetMaterial == null))
		{
			switch (base.cursorStatus)
			{
			case CursorStatus.None:
				targetMaterial.SetFloat(targetProperty, 0f);
				break;
			case CursorStatus.Hover:
				targetMaterial.SetFloat(targetProperty, 0.4f * cursorValueMultiplier);
				break;
			case CursorStatus.Active:
				targetMaterial.SetFloat(targetProperty, 0.6f * cursorValueMultiplier);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
