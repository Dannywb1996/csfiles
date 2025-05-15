using System.Collections;
using UnityEngine;

public class Ai_C_BackStep : AbilityInstance
{
	public GameObject[] points;

	public int bombCount;

	public float backStepDis;

	public float stepDuration;

	public DewEase ease;

	public float delay;

	protected override IEnumerator OnCreateSequenced()
	{
		if (base.isServer)
		{
			DestroyOnDeath(base.info.caster);
			Vector3 end = base.info.caster.agentPosition - base.info.forward * backStepDis;
			end = Dew.GetValidAgentDestination_Closest(base.info.caster.agentPosition, end);
			base.info.caster.Control.StartDaze(stepDuration);
			CreateBasicEffect(base.info.caster, new UncollidableEffect(), stepDuration);
			base.info.caster.Visual.KnockUp(0.4f, isFriendly: true);
			base.info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = end,
				duration = stepDuration,
				ease = ease,
				rotateForward = false,
				isFriendly = true
			});
			Vector3 startPos = base.info.caster.agentPosition;
			for (int i = 0; i < bombCount; i++)
			{
				Vector3 positionOnGround = Dew.GetPositionOnGround(points[i].transform.position);
				positionOnGround = Dew.GetValidAgentDestination_LinearSweep(startPos, positionOnGround);
				CreateAbilityInstance<Ai_C_BackStep_Bomb>(positionOnGround, null, new CastInfo(base.info.caster, positionOnGround));
				yield return new SI.WaitForSeconds(0.03f);
			}
			yield return new SI.WaitForSeconds(delay);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
