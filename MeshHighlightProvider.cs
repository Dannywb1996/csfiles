using System;
using HighlightPlus;
using UnityEngine;

public class MeshHighlightProvider : HighlightProvider
{
	private bool _isAnimatingClick;

	public HighlightEffect meshHighlight { get; private set; }

	protected virtual void Awake()
	{
		meshHighlight = GetComponent<HighlightEffect>();
		if (meshHighlight == null)
		{
			meshHighlight = base.gameObject.AddComponent<HighlightEffect>();
			meshHighlight.ProfileLoad(ObjectHighlightManager.GetProfile());
		}
		meshHighlight.innerGlow = 0f;
		meshHighlight.outline = 0f;
		meshHighlight.glow = 0f;
	}

	protected virtual void Start()
	{
		meshHighlight.highlighted = true;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (base.elapsedClickTime < 0.35f)
		{
			float clickedValue = Mathf.Round(Mathf.PingPong(base.elapsedClickTime * 16f + 1f, 1f)) * 0.5f + 0.5f;
			meshHighlight.outline = clickedValue * 0.75f;
			meshHighlight.outlineWidth = 2f;
			meshHighlight.UpdateMaterialProperties();
		}
		else if (_isAnimatingClick)
		{
			_isAnimatingClick = false;
			OnCursorStatusUpdated();
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
		switch (base.cursorStatus)
		{
		case CursorStatus.None:
			meshHighlight.outline = 0f;
			break;
		case CursorStatus.Hover:
			meshHighlight.outline = 0.4f;
			break;
		case CursorStatus.Active:
			meshHighlight.outline = 0.6f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		meshHighlight.outlineWidth = 1.25f;
		meshHighlight.UpdateMaterialProperties();
	}

	protected void OnDestroy()
	{
		if (meshHighlight != null)
		{
			UnityEngine.Object.Destroy(meshHighlight);
		}
	}
}
