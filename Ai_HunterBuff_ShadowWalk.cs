using System.Collections;
using UnityEngine;

public class Ai_HunterBuff_ShadowWalk : AbilityInstance
{
	public float maxDistance;

	public float minDistance;

	public float randomMagnitude;

	public float startTime;

	public float disappearTime;

	public float appearTime;

	protected override IEnumerator OnCreateSequenced()
	{
		if (base.isServer)
		{
			base.info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = startTime + disappearTime + appearTime
			});
			Vector3 delta = base.info.target.agentPosition - base.info.caster.agentPosition + Random.onUnitSphere.Flattened() * randomMagnitude;
			if (delta.magnitude > maxDistance)
			{
				delta = delta.normalized * maxDistance;
			}
			else if (delta.magnitude < minDistance)
			{
				delta = delta.normalized * minDistance;
			}
			Vector3 dest = Dew.GetValidAgentDestination_LinearSweep(end: base.info.caster.agentPosition + delta, start: base.info.caster.agentPosition);
			yield return new SI.WaitForSeconds(startTime);
			CreateStatusEffect(base.info.caster, delegate(Se_HunterBuff_ShadowWalk_Disappear s)
			{
				s.duration = disappearTime;
			});
			base.info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = dest,
				canGoOverTerrain = true,
				duration = disappearTime,
				ease = DewEase.Linear,
				isCanceledByCC = false,
				isFriendly = true
			});
			yield return new SI.WaitForSeconds(disappearTime);
			yield return new SI.WaitForSeconds(appearTime);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
