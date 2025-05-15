using System;
using UnityEngine;

public class DashAttackInstance : DamageInstance
{
	public float damageGracePeriod = 0.075f;

	public bool isDirectionMode;

	public Dash dash;

	private Quaternion _rotation;

	private bool _isDashing;

	private Vector3 _damageDirection;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (isDirectionMode)
		{
			_rotation = base.info.rotation;
		}
		else
		{
			_rotation = Quaternion.LookRotation(base.info.point - base.info.caster.position).Flattened();
		}
		if (!base.isServer)
		{
			return;
		}
		if (CheckShouldBeDestroyed())
		{
			Destroy();
			return;
		}
		_isDashing = true;
		if (isDirectionMode)
		{
			_damageDirection = base.info.forward;
			dash.ApplyByDirection(base.info.caster, base.info.forward, OnSetupDash);
		}
		else
		{
			_damageDirection = (base.info.point - base.info.caster.position).normalized;
			dash.ApplyByDestination(base.info.caster, base.info.point, OnSetupDash);
		}
		if (damageGracePeriod < 1E-05f)
		{
			DoCollisionChecks();
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		base.transform.SetPositionAndRotation(base.info.caster.position, _rotation);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && _isDashing && !(Time.time - base.creationTime < damageGracePeriod))
		{
			if (CheckShouldBeDestroyed())
			{
				Destroy();
			}
			else
			{
				DoCollisionChecks();
			}
		}
	}

	protected virtual void OnSetupDash(DispByDestination disp)
	{
		if (base.info.caster.Status.hasImmobility)
		{
			disp.destination = base.info.caster.agentPosition + (disp.destination - base.info.caster.agentPosition).normalized * 0.01f;
		}
		disp.onFinish = (Action)Delegate.Combine(disp.onFinish, (Action)delegate
		{
			_isDashing = false;
			if (destroyWhenDone && base.isActive)
			{
				Destroy();
			}
		});
		disp.onCancel = (Action)Delegate.Combine(disp.onCancel, (Action)delegate
		{
			_isDashing = false;
			if (destroyWhenDone && base.isActive)
			{
				Destroy();
			}
		});
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		dmg.SetDirection(_damageDirection);
	}

	private void MirrorProcessed()
	{
	}
}
