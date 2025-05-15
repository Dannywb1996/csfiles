using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_E_SearingCharge : AbilityInstance
{
	public GameObject firstHitEffect;

	public GameObject markingEffect;

	public GameObject secondHitEffect;

	public GameObject secondHitSound;

	public Knockback knockback;

	public GameObject resetEffect;

	public ScalingValue firstHitDmgAmount;

	public ScalingValue secondHitDmgAmount;

	public float chargeDuration;

	public float secondDmgDelay;

	public float checkKillGracePeriod = 0.5f;

	public float radius;

	public int maxSecondHitSound = 2;

	private List<Entity> _entsHit;

	private bool _enableCheckTimer;

	private bool _shouldDoCollisionCheck;

	private Vector3 _previousPos;

	private Vector3 _currentPos;

	private float _multiplier = 1f;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield break;
		}
		_entsHit = new List<Entity>();
		_previousPos = base.info.caster.position;
		Vector3 delta = base.info.forward * ((St_E_SearingCharge)base.firstTrigger).currentConfig.castMethod._length;
		base.info.caster.Control.StartDaze(chargeDuration);
		yield return new SI.WaitForSeconds(0.1f);
		base.info.caster.Status.UpdateStatusInfo();
		_multiplier = Mathf.Max(1f, base.info.caster.Status.movementSpeedMultiplier);
		base.info.caster.Control.StartDisplacement(new DispByDestination
		{
			destination = base.info.caster.position + delta * _multiplier,
			duration = chargeDuration,
			ease = DewEase.EaseOutQuad,
			isFriendly = true,
			rotateForward = true,
			canGoOverTerrain = false
		});
		_shouldDoCollisionCheck = true;
		yield return new SI.WaitForSeconds(chargeDuration);
		_shouldDoCollisionCheck = false;
		_enableCheckTimer = true;
		yield return new SI.WaitForSeconds(secondDmgDelay);
		for (int i = 0; i < _entsHit.Count; i++)
		{
			if (!(_entsHit[i] == null) && _entsHit[i].isServer)
			{
				if (i < maxSecondHitSound)
				{
					FxPlayNewNetworked(secondHitSound, _entsHit[i]);
				}
				FxPlayNewNetworked(secondHitEffect, _entsHit[i]);
				Damage(secondHitDmgAmount).SetElemental(ElementalType.Fire).SetDirection(base.rotation).ApplyAmplification(_multiplier - 1f)
					.Dispatch(_entsHit[i]);
			}
		}
		yield return new SI.WaitForSeconds(checkKillGracePeriod);
		Destroy();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!base.isServer || !_shouldDoCollisionCheck)
		{
			return;
		}
		_currentPos = base.info.caster.position;
		float maxDistance = Vector3.Distance(_previousPos, _currentPos);
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> readOnlySpan = DewPhysics.SphereCastAllEntities(out handle, _previousPos, radius, base.info.forward, maxDistance, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Entity item = readOnlySpan[i];
			if (!_entsHit.Contains(item))
			{
				FxPlayNewNetworked(firstHitEffect, readOnlySpan[i]);
				FxPlayNewNetworked(markingEffect, readOnlySpan[i]);
				knockback.ApplyWithOrigin(base.info.caster.position, readOnlySpan[i]);
				Damage(firstHitDmgAmount).SetElemental(ElementalType.Fire).SetDirection(base.rotation).ApplyAmplification(Mathf.Max(_multiplier - 1f, 0f))
					.Dispatch(readOnlySpan[i]);
				_entsHit.Add(item);
			}
		}
		_previousPos = base.info.caster.position;
		handle.Return();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer || !_enableCheckTimer)
		{
			return;
		}
		for (int i = 0; i < _entsHit.Count && !(_entsHit[i] == null); i++)
		{
			if (!_entsHit[i].isAlive)
			{
				if (base.firstTrigger != null)
				{
					base.firstTrigger.ResetCooldown();
					FxPlayNetworked(resetEffect, base.info.caster);
				}
				_enableCheckTimer = false;
				break;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
