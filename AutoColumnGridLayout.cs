using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Auto Column Grid Layout")]
[ExecuteAlways]
public class AutoColumnGridLayout : GridLayoutGroup
{
	[SerializeField]
	private bool _autoAdjustColumns = true;

	[SerializeField]
	private int _minColumns = 1;

	[SerializeField]
	private int _maxColumns = int.MaxValue;

	public bool AutoAdjustColumns
	{
		get
		{
			return _autoAdjustColumns;
		}
		set
		{
			_autoAdjustColumns = value;
			if (_autoAdjustColumns)
			{
				UpdateColumnCount();
			}
		}
	}

	public int MinColumns
	{
		get
		{
			return _minColumns;
		}
		set
		{
			_minColumns = Mathf.Max(1, value);
			if (_autoAdjustColumns)
			{
				UpdateColumnCount();
			}
		}
	}

	public int MaxColumns
	{
		get
		{
			return _maxColumns;
		}
		set
		{
			_maxColumns = Mathf.Max(_minColumns, value);
			if (_autoAdjustColumns)
			{
				UpdateColumnCount();
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateColumnCount();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		UpdateColumnCount();
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		UpdateColumnCount();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		UpdateColumnCount();
	}

	private void Update()
	{
	}

	public void UpdateColumnCount()
	{
		if (!_autoAdjustColumns || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		RectTransform parentRectTransform = base.transform.parent as RectTransform;
		if (!(parentRectTransform == null))
		{
			int maxPossibleColumns = Mathf.FloorToInt((parentRectTransform.rect.width - (float)base.padding.left - (float)base.padding.right + base.spacing.x) / (base.cellSize.x + base.spacing.x));
			maxPossibleColumns = Mathf.Max(1, maxPossibleColumns);
			int newColumnCount = Mathf.Clamp(maxPossibleColumns, _minColumns, _maxColumns);
			if (base.constraintCount != newColumnCount)
			{
				base.constraint = Constraint.FixedColumnCount;
				base.constraintCount = newColumnCount;
				LayoutRebuilder.MarkLayoutForRebuild(base.rectTransform);
			}
		}
	}
}
