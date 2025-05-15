using UnityEngine;

public class EntityShellModifier
{
	private Color _color = Color.white;

	private float _opacity;

	internal Entity _parent;

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyShellModifiers();
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
				_parent.Visual.DirtyShellModifiers();
			}
		}
	}

	public void Stop()
	{
		if (!(_parent == null) && !(_parent.Visual == null))
		{
			_parent.Visual.RemoveShellModifier(this);
			_parent = null;
		}
	}
}
