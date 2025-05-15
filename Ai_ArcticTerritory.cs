using System;
using UnityEngine;

public class Ai_ArcticTerritory : AbilityInstance
{
	[HideInInspector]
	public float radius;

	public float collisionCheckInterval;

	public GameObject fxInstance;

	private float _currentTime;

	private float _elapsedTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		_currentTime = 0f;
		_elapsedTime = 0f;
		fxInstance.transform.localScale = Vector3.one * radius;
		FxPlay(fxInstance, base.transform.position, null);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxInstance);
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!base.isServer || Time.time - _currentTime < collisionCheckInterval)
		{
			return;
		}
		_currentTime = Time.time;
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> readOnlySpan = DewPhysics.OverlapCircleAllEntities(out handle, base.transform.position, radius);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Entity entity = readOnlySpan[i];
			if (entity.owner.isHumanPlayer && !entity.Control.isDashing && !entity.IsNullInactiveDeadOrKnockedOut())
			{
				if (entity.Status.TryGetStatusEffect<Se_ArcticTerritory>(out var effect))
				{
					effect.ResetTimer();
				}
				else
				{
					CreateStatusEffect<Se_ArcticTerritory>(entity);
				}
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
