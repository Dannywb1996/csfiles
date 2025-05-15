using UnityEngine;

public class EntityColorModifier
{
	private Color _baseColor = Color.white;

	private Color _emission = Color.black;

	private float _dissolveAmount;

	private float _opacity = 1f;

	private Color? _dissolveColor;

	internal Entity _parent;

	public Color baseColor
	{
		get
		{
			return _baseColor;
		}
		set
		{
			_baseColor = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyColorModifiers();
			}
		}
	}

	public Color emission
	{
		get
		{
			return _emission;
		}
		set
		{
			_emission = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyColorModifiers();
			}
		}
	}

	public float dissolveAmount
	{
		get
		{
			return _dissolveAmount;
		}
		set
		{
			_dissolveAmount = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyColorModifiers();
			}
		}
	}

	public float opacity
	{
		get
		{
			return _opacity;
		}
		set
		{
			_opacity = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyColorModifiers();
			}
		}
	}

	public Color? dissolveColor
	{
		get
		{
			return _dissolveColor;
		}
		set
		{
			_dissolveColor = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyColorModifiers();
			}
		}
	}

	public void Stop()
	{
		if (!(_parent == null) && !(_parent.Visual == null))
		{
			_parent.Visual.RemoveColorModifier(this);
			_parent = null;
		}
	}
}
