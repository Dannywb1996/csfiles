using UnityEngine;

public class FxEntityColor : FxInterpolatedEffectBase, IAttachableToEntity
{
	public bool doBaseColor;

	[ColorUsage(false, false)]
	public Color baseColor = Color.white;

	public bool doEmission;

	[ColorUsage(false, true)]
	public Color emission = Color.black;

	public bool doDissolve;

	public float dissolveAmount;

	public bool doDissolveColor;

	[ColorUsage(false, true)]
	public Color dissolveColor = Color.white;

	private EntityColorModifier _modifier;

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
				_modifier = _entity.Visual.GetNewColorModifier();
			}
			if (doBaseColor)
			{
				_modifier.baseColor = Color.Lerp(Color.white, baseColor, value);
			}
			if (doEmission)
			{
				_modifier.emission = Color.Lerp(Color.black, emission, value);
			}
			if (doDissolve)
			{
				_modifier.dissolveAmount = dissolveAmount * value;
			}
			_modifier.dissolveColor = (doDissolveColor ? new Color?(dissolveColor) : ((Color?)null));
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
