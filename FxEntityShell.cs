using UnityEngine;

public class FxEntityShell : FxInterpolatedEffectBase, IAttachableToEntity
{
	[ColorUsage(false, true)]
	public Color color = Color.white;

	private EntityShellModifier _modifier;

	private Entity _entity;

	public void OnAttachToEntity(Entity target)
	{
		if (_modifier != null)
		{
			_modifier.Stop();
			_modifier = null;
		}
		else
		{
			_entity = target;
		}
	}

	protected override void ValueSetter(float value)
	{
		if (value < 1E-05f && _modifier != null)
		{
			_modifier.Stop();
			_modifier = null;
		}
		else if (!(_entity == null))
		{
			if (_modifier == null)
			{
				_modifier = _entity.Visual.GetNewShellModifier();
			}
			_modifier.opacity = value;
			_modifier.color = color;
		}
	}

	private void OnDestroy()
	{
		if (_modifier != null)
		{
			_modifier.Stop();
			_modifier = null;
		}
	}
}
