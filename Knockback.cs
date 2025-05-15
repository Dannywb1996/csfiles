using System;
using UnityEngine;

[Serializable]
public class Knockback
{
	public float distance = 1.25f;

	public float duration = 0.25f;

	public bool ignoreTenacity;

	public void ApplyWithOrigin(Vector3 origin, Entity to)
	{
		ApplyWithDirection((to.position - origin).normalized, to);
	}

	public void ApplyWithDirection(Quaternion rot, Entity to)
	{
		ApplyWithDirection(rot * Vector3.forward, to);
	}

	public void ApplyWithDirection(Vector3 dir, Entity to)
	{
		if (to.Status.hasCrowdControlImmunity)
		{
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnIgnoreCC(to);
			return;
		}
		float mult = (ignoreTenacity ? 1f : (1f - to.Status.tenacity / 100f));
		DispByDestination disp = new DispByDestination
		{
			canGoOverTerrain = false,
			destination = to.position + dir * (distance * mult),
			duration = duration * mult,
			ease = DewEase.EaseOutCirc,
			isCanceledByCC = false,
			isFriendly = false,
			rotateForward = false
		};
		to.Control.Rotate(-dir, immediately: true);
		to.Control.StartDisplacement(disp);
	}
}
