using UnityEngine;

[DisallowMultipleComponent]
public class FxGameObject : FxInterpolatedEffectBase
{
	public bool disableWhenStopped = true;

	[SerializeField]
	[HideInInspector]
	private Vector3 _startScale = new Vector3(float.NaN, float.NaN, float.NaN);

	protected override void OnInit()
	{
		base.OnInit();
		if (float.IsNaN(_startScale.x))
		{
			_startScale = base.transform.localScale;
		}
	}

	public override void Play()
	{
		base.Play();
		base.currentValue = 0.001f;
		ValueSetter(0.001f);
	}

	protected override void ValueSetter(float value)
	{
		base.transform.localScale = value * _startScale;
		if (disableWhenStopped)
		{
			base.gameObject.SetActive(base.isEmitting || value > 0f);
		}
	}
}
