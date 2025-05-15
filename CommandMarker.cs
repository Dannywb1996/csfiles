using UnityEngine;

public class CommandMarker : EffectAutoDestroy
{
	public Transform followTransform;

	protected override void Update()
	{
		base.Update();
		if (base.transform != null && followTransform != null)
		{
			base.transform.position = followTransform.position;
		}
	}
}
