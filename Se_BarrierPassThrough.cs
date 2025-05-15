using System;
using UnityEngine;

public class Se_BarrierPassThrough : StatusEffect
{
	[NonSerialized]
	public Vector3 destination;

	public float duration = 0.5f;

	public DewEase ease;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			if (base.victim is Hero)
			{
				DoInvulnerable();
				DoUncollidable();
				DoInvisible();
			}
			base.victim.Control.StartDisplacement(new DispByDestination
			{
				destination = destination,
				canGoOverTerrain = true,
				duration = duration,
				ease = ease,
				isFriendly = true,
				rotateForward = true,
				isCanceledByCC = false,
				onFinish = base.DestroyIfActive,
				onCancel = base.DestroyIfActive
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
