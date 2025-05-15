using System.Collections;
using UnityEngine;

public class Ai_C_Whirlwind : DamageInstance
{
	public DewAnimationClip animSpin;

	public DewAnimationClip animSpinEnd;

	public Transform swordHandleTransform;

	public float totalDuration = 4f;

	private EntityTransformModifier _mod;

	private float _currentAngle;

	private float _lastDamageTime = float.NegativeInfinity;

	private AbilityLockHandle _handle;

	protected override IEnumerator OnCreateSequenced()
	{
		_mod = base.info.caster.Visual.GetNewTransformModifier();
		if (base.isServer)
		{
			_handle = base.info.caster.Ability.GetNewAbilityLockHandle();
			_handle.LockAllAbilitiesCast();
			_handle.UnlockMovementSkillCast();
			base.info.caster.Control.Rotate(Quaternion.identity, immediately: true, totalDuration);
			base.info.caster.Control.StartChannel(new Channel
			{
				duration = totalDuration,
				onCancel = base.DestroyIfActive,
				onComplete = base.DestroyIfActive
			});
			base.info.caster.Animation.PlayAbilityAnimation(animSpin);
			yield return new SI.WaitForSeconds(0.75f);
			base.firstTrigger.ChangeConfigTimedOnce(1, totalDuration - 0.75f);
		}
	}

	private float GetTickInterval()
	{
		return 0.48f / Mathf.Max(1f, base.info.caster.Status.attackSpeedMultiplier);
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		base.transform.position = base.info.caster.position;
		_currentAngle += 1000f * Time.deltaTime * base.info.caster.Status.attackSpeedMultiplier;
		_mod.rotation = Quaternion.Euler(0f, _currentAngle, 0f);
		Vector3 forward = (swordHandleTransform.position - base.info.caster.position).Flattened();
		swordHandleTransform.rotation = Quaternion.Euler(0f, 30f, 0f) * Quaternion.LookRotation(forward);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer)
		{
			return;
		}
		if (Time.time - base.creationTime > totalDuration)
		{
			Destroy();
			return;
		}
		float tickInterval = GetTickInterval();
		if (!(Time.time - _lastDamageTime < tickInterval))
		{
			if (float.IsInfinity(_lastDamageTime))
			{
				_lastDamageTime = Time.time;
			}
			else
			{
				_lastDamageTime += tickInterval;
			}
			base.info.caster.Animation.PlayAbilityAnimation(animSpin);
			DoCollisionChecks();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_mod != null)
		{
			_mod.Stop();
			_mod = null;
		}
		if (base.isServer)
		{
			if (_handle != null)
			{
				_handle.Stop();
				_handle = null;
			}
			base.info.caster.Control.StopOverrideRotation();
			base.info.caster.Animation.StopAbilityAnimation(animSpin);
			if (!base.info.caster.IsNullInactiveDeadOrKnockedOut())
			{
				base.info.caster.Animation.PlayAbilityAnimation(animSpinEnd);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
