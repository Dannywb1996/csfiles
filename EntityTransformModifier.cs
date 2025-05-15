using UnityEngine;

public class EntityTransformModifier
{
	private Vector3 _localOffset = Vector3.zero;

	private Vector3 _worldOffset = Vector3.zero;

	private Vector3 _scaleMultiplier = Vector3.one;

	private Quaternion _rotation = Quaternion.identity;

	internal Entity _parent;

	public Vector3 localOffset
	{
		get
		{
			return _localOffset;
		}
		set
		{
			_localOffset = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyTransformModifiers();
			}
		}
	}

	public Vector3 worldOffset
	{
		get
		{
			return _worldOffset;
		}
		set
		{
			_worldOffset = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyTransformModifiers();
			}
		}
	}

	public Vector3 scaleMultiplier
	{
		get
		{
			return _scaleMultiplier;
		}
		set
		{
			_scaleMultiplier = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyTransformModifiers();
			}
		}
	}

	public Quaternion rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rotation = value;
			if (_parent != null)
			{
				_parent.Visual.DirtyTransformModifiers();
			}
		}
	}

	public void Stop()
	{
		if (!(_parent == null) && !(_parent.Visual == null))
		{
			_parent.Visual.RemoveTransformModifier(this);
			_parent = null;
		}
	}
}
