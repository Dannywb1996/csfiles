using UnityEngine;

public class Ai_GenericDodge : AbilityInstance
{
	public float speed = 12f;

	public DewEase ease;

	public float uncollidableRatio = 0.7f;

	public float minDistance = 2f;

	public float animSpeedMultiplier = 1f;

	public bool rotateForward;

	public DewAnimationClip anim;

	public bool doAttackReset;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			if (doAttackReset && base.info.caster != null)
			{
				base.info.caster.Ability.attackAbility.ResetCooldown();
			}
			Vector3 dest = Dew.GetValidAgentDestination_LinearSweep(base.info.caster.agentPosition, base.info.point);
			Vector3 delta = dest - base.info.caster.agentPosition;
			float distance = delta.magnitude;
			if (distance < minDistance)
			{
				delta = delta.normalized * minDistance;
				dest = base.info.caster.agentPosition + delta;
				dest = Dew.GetValidAgentDestination_LinearSweep(base.info.caster.agentPosition, dest);
				distance = minDistance;
			}
			float duration = distance / speed;
			CreateBasicEffect(base.info.caster, new UncollidableEffect(), duration * uncollidableRatio, "dodge_uncol");
			base.info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = dest,
				duration = duration,
				ease = ease,
				isFriendly = true,
				onCancel = base.Destroy,
				onFinish = base.Destroy,
				rotateForward = rotateForward,
				canGoOverTerrain = false,
				isCanceledByCC = false
			});
			if (anim != null)
			{
				base.info.caster.Animation.PlayAbilityAnimation(anim, animSpeedMultiplier * anim.entries[0].duration / duration);
			}
			base.transform.rotation = Quaternion.LookRotation(dest - base.info.caster.agentPosition).Flattened();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && !base.info.caster.IsNullOrInactive())
		{
			base.info.caster.Animation.StopAbilityAnimation(anim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
